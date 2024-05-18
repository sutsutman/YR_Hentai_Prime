using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationUtility
    {
        public static void DrawBedAnimation(Building_AnimationBed building_AnimationBed)
        {
            Vector3 bedBasePos = building_AnimationBed.DrawPos;

            foreach (var bedAnimationSettingAndTick in building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks)
            {
                int currentTick = bedAnimationSettingAndTick.currentTick;
                var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                    .Where(b => b.tick <= currentTick)
                    .OrderByDescending(b => b.tick)
                    .FirstOrDefault()
                    ?? bedAnimationSettingAndTick.bedAnimationSettings
                    .OrderByDescending(x => x.tick)
                    .FirstOrDefault();

                if (closestSetting == null && (!bedAnimationSettingAndTick.autoDurationTicksSetting && currentTick > bedAnimationSettingAndTick.durationTick))
                    continue;

                Vector3 pos = CalculatePos(building_AnimationBed, bedBasePos, bedAnimationSettingAndTick, closestSetting);
                closestSetting.graphic?.Draw(pos, Rot4.North, building_AnimationBed.HeldPawn);
            }

            // 포트레잇
            foreach (var portraitIngredient in building_AnimationBed.AnimationSettingComp.portraitIngredients)
            {
                DrawPortrait(building_AnimationBed, portraitIngredient);
            }
        }
        private static void DrawPortrait(Building_AnimationBed building_AnimationBed, PortraitIngredient portraitIngredient)
        {
            Vector3 drawPos = building_AnimationBed.DrawPos + portraitIngredient.offset + portraitIngredient.testOffset;
            //안나오던건 Vector3 drawsize 변환 문제!!!!!!
            Vector3 drawSize = new Vector3(portraitIngredient.drawSize.x + portraitIngredient.testDrawSize.x, 1, portraitIngredient.drawSize.y + portraitIngredient.testDrawSize.y);
            Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(portraitIngredient.angle + portraitIngredient.testAngle, Vector3.up), drawSize);

            var portraitSetting = portraitIngredient.portraitSetting;
            var cameraOffset = portraitIngredient.cameraOffset + portraitIngredient.testCameraOffset;
            if(portraitSetting.animationSynchro)
            {
                PawnRenderNode renderNode = building_AnimationBed.HeldPawn.Drawer.renderer.renderTree.rootNode.children
    .FirstOrDefault(n => n?.Props?.tagDef == portraitSetting.pawnRenderNodeTagDef);

                if (renderNode != null)
                {
                    Vector3 offset = renderNode.Worker.OffsetFor(renderNode, building_AnimationBed.HeldPawnDrawParms, out var pivot);
                    offset -= pivot;
                    cameraOffset += offset*-1;
                }
            }

            TestLog.Error($"cameraOffset : {cameraOffset.x:F5}, {cameraOffset.y:F5}, {cameraOffset.z:F5}");

            var cameraZoom = portraitIngredient.cameraZoom + portraitIngredient.testCameraZoom;

            portraitIngredient.iconMat.mainTexture = PortraitsCache.Get(building_AnimationBed.HeldPawn, new Vector2(256, 256), portraitSetting.rotation, cameraOffset, cameraZoom, renderClothes: portraitSetting.renderClothes, renderHeadgear: portraitSetting.renderHeadgear, stylingStation: false, healthStateOverride: PawnHealthState.Mobile);

            GenDraw.DrawMeshNowOrLater(portraitIngredient.portraitMesh, matrix, portraitIngredient.iconMat, PawnRenderFlags.None.FlagSet(PawnRenderFlags.DrawNow));
        }

        private static Vector3 CalculatePos(Building_AnimationBed building_AnimationBed, Vector3 bedBasePos, BedAnimationSettingAndTick bedAnimationSettingAndTick, BedAnimationSetting closestSetting)
        {
            var pawn = building_AnimationBed.HeldPawn;
            // 기본 위치 계산
            Vector3 pos = new Vector3(bedBasePos.x, pawn.DrawPos.y, bedBasePos.z) + closestSetting.offset;

            // 렌더 노드의 오프셋 계산
            if (bedAnimationSettingAndTick.parentBedAnimationDef.animationSynchro && bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef != null)
            {
                PawnRenderNode renderNode = pawn.Drawer.renderer.renderTree.rootNode.children
                    .FirstOrDefault(n => n?.Props?.tagDef == bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef);

                if (renderNode != null)
                {
                    Vector3 offset = renderNode.Worker.OffsetFor(renderNode, building_AnimationBed.HeldPawnDrawParms, out var pivot);
                    offset -= pivot;
                    pos += offset;

                    if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
                    {
                        TestLog.Error($"pawnRenderNodeTagDef offset : {offset.x:F5}, {offset.y:F5}, {offset.z:F5}");
                        TestLog.Error($"pawnRenderNodeTagDef pivot : {pivot.x:F5}, {pivot.y:F5}, {pivot.z:F5}");
                    }
                }
            }

            // 테스트용 오프셋 추가
            pos += closestSetting.testOffset;
            return pos;
        }

        public static bool MakeBedAnimation(Building_AnimationBed building_AnimationBed)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;

            if (building_AnimationBed.AnimationSettingComp == null || !building_AnimationBed.AnimationSettingComp.needMakeGraphics)
                return false;

            if (pawn == null || pawn.Drawer.renderer.renderTree.rootNode == null)
            {
                TestLog.Error("can't make Graphic");
                return false;
            }

            TestLog.Error($"{pawn.LabelShort} : MakeBedAnimation Start");
            building_AnimationBed.AnimationSettingComp.needMakeGraphics = false; // 필요성 초기화
            building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

            var Props = building_AnimationBed.AnimationSettingComp.Props;
            var pawnAnimationDef = GetPawnAnimationDef(building_AnimationBed);
            pawn.Drawer.renderer.SetAnimation(pawnAnimationDef);

            TestLog.Error("+++Check bedAnimationList+++");
            foreach (var bedAnimation in Props.bedAnimationList)
            {
                BedAnimationDef bedAnimationDef = bedAnimation.bedAnimationDef;

                foreach (var conditionBedAnimationDef in bedAnimation.conditionBedAnimationDefs)
                {
                    if (Condition.Match(pawn, building_AnimationBed, conditionBedAnimationDef.condition))
                    {
                        bedAnimationDef = conditionBedAnimationDef.bedAnimationDef;
                        if (Condition.NeedBreak(conditionBedAnimationDef.condition))
                            break;
                    }
                }

                TestLog.Error($"=={bedAnimationDef.defName}==");
                TestLog.Error("++MakeBedAnimationSettingAndTick Start++");

                var result = MakeBedAnimationSettingAndTick(building_AnimationBed, bedAnimationDef, pawnAnimationDef);
                building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks.Add(result);

                TestLog.Error("++MakeBedAnimationSettingAndTick Finish++");
            }

            TestLog.Error("+++Finish Check bedAnimationList+++");

            // 포트레잇
            TestLog.Error("=============================");
            MakePortrait(building_AnimationBed);

            return true;
        }
        private static void MakePortrait(Building_AnimationBed building_AnimationBed)
        {
            building_AnimationBed.AnimationSettingComp.portraitIngredients = new List<PortraitIngredient>();
            foreach (var pawnPortraitSetting in building_AnimationBed.AnimationSettingComp.Props.pawnPortraitSettings)
            {
                var portraitSetting = pawnPortraitSetting.portraitSetting;

                foreach (var conditionPortraitSetting in pawnPortraitSetting.conditonPortraitSettings)
                {
                    if (Condition.Match(building_AnimationBed.HeldPawn, building_AnimationBed, conditionPortraitSetting.condition))
                    {
                        portraitSetting = conditionPortraitSetting.portraitSetting;
                        if (Condition.NeedBreak(conditionPortraitSetting.condition))
                            break;
                    }
                }

                if (!portraitSetting.draw)
                {
                    continue;
                }

                PortraitIngredient portraitIngredient = new PortraitIngredient
                {
                    drawSize = portraitSetting.drawSize,
                    angle = portraitSetting.angle,
                    offset = portraitSetting.offset,
                    portraitMesh = portraitSetting.portraitMeshGraphicData.Graphic.MeshAt(Rot4.South),
                    iconMat = CreateMaterial(portraitSetting, building_AnimationBed.HeldPawn),
                    cameraOffset = portraitSetting.cameraOffset,
                    cameraZoom = portraitSetting.cameraZoom,
                    portraitSetting = portraitSetting
                };

                building_AnimationBed.AnimationSettingComp.portraitIngredients.Add(portraitIngredient);
            }
        }
        private static Material CreateMaterial(PortraitSetting portraitSetting, Pawn pawn)
        {
            var tempMat = new Material(ShaderDatabase.CutoutComplex)
            {
                mainTexture = PortraitsCache.Get(pawn, new Vector2(256, 256), Rot4.South, default, 1, renderClothes: true, renderHeadgear: true, stylingStation: false, healthStateOverride: PawnHealthState.Mobile),
                color = Color.white
            };
            var maskTex = ContentFinder<Texture2D>.Get(portraitSetting.maskPath);

            MaterialRequest req = default;
            if (maskTex != null)
                req.maskTex = maskTex;

            req.mainTex = tempMat.mainTexture;
            req.color = tempMat.color;
            req.shader = tempMat.shader;
            return MaterialPool.MatFrom(req);
        }

        public static BedAnimationSettingAndTick MakeBedAnimationSettingAndTick(Building_AnimationBed building_AnimationBed, BedAnimationDef bedAnimationDef, AnimationDef pawnAnimationDef)
        {
            var bedAnimationSettings = new List<BedAnimationSetting>();

            var bedAnimationSettingAndTick = new BedAnimationSettingAndTick
            {
                autoDurationTicksSetting = bedAnimationDef.autoDurationTicksSetting,
                parentBedAnimationDef = bedAnimationDef,
                bedAnimationSettings = bedAnimationSettings
            };

            TestLog.Error("+Check bedAnimationSetting+");
            // Loop through animation settings
            foreach (var bedAnimationSetting in bedAnimationDef.bedAnimationSettings)
            {
                if (!Condition.Match(building_AnimationBed.HeldPawn, building_AnimationBed, bedAnimationSetting.condition))
                {
                    continue;
                }

                var settingCopy = new BedAnimationSetting();
                settingCopy = bedAnimationSetting.Copy();
                settingCopy.parentBedAnimationDef = bedAnimationDef;
                settingCopy.setPawnColor = bedAnimationDef.setPawnColor;
                settingCopy.graphic = null;

                // Set appropriate graphic(그래픽 제작)
                settingCopy.graphic = bedAnimationDef.isPawnTextureReplace
                    ? GetGraphic(building_AnimationBed.HeldPawn, bedAnimationDef.pawnRenderNodeTagDef, settingCopy)
                    : settingCopy.graphicData.Graphic;


                if (settingCopy.graphic == null)
                {
                    TestLog.Error("settingCopy Skip");
                    continue;
                }

                bedAnimationSettings.Add(settingCopy);

                TestLog.Error("settingCopy Finish");
                if (Condition.NeedBreak(bedAnimationSetting.condition))
                {
                    break;
                }
            }

            TestLog.Error("+Finish Check bedAnimationSetting+");

            if (bedAnimationSettings.Count != 0)
            {
                // Determine durationTick based on autoDurationTicksSetting
                bedAnimationSettingAndTick.durationTick = bedAnimationDef.autoDurationTicksSetting
                    ? pawnAnimationDef.durationTicks
                    : bedAnimationDef.durationTicks != 0 && bedAnimationDef.durationTicks >= bedAnimationSettings.Max(x => x.tick)
                        ? bedAnimationDef.durationTicks
                        : bedAnimationSettings.Max(x => x.tick);
            }


            return bedAnimationSettingAndTick;
        }

        private static Graphic GetGraphic(Pawn pawn, PawnRenderNodeTagDef tagDef, BedAnimationSetting bedAnimationSetting)
        {
            if (tagDef == PawnRenderNodeTagDefOf.Body)
            {
                return GraphicForBody(pawn, bedAnimationSetting);
            }
            else if (tagDef == PawnRenderNodeTagDefOf.Head)
            {
                return GraphicForHead(pawn, bedAnimationSetting);
            }

            return bedAnimationSetting.graphicData.Graphic;

        }

        private static AnimationDef GetPawnAnimationDef(Building_AnimationBed building_AnimationBed)
        {
            var pawnAnimationDef = building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.pawnAnimationDef;

            foreach (var condition in building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.conditonPawnAnimations)
            {
                if (Condition.Match(building_AnimationBed.HeldPawn, building_AnimationBed.AnimationSettingComp.Building_AnimationBed, condition.condition))
                {
                    pawnAnimationDef = condition.pawnAnimationDef;

                    if (Condition.NeedBreak(condition.condition))
                    {
                        break;
                    }
                }
            }

            return pawnAnimationDef;
        }

        public static Graphic GraphicForHead(Pawn pawn, BedAnimationSetting bedAnimationSetting)
        {
            // HasHead가 false이면 즉시 null 반환
            if (!pawn.health.hediffSet.HasHead)
            {
                return null;
            }

            // PawnRenderNode_Head를 안전하게 찾기
            var pawnRenderNode_Head = pawn.Drawer.renderer.renderTree.rootNode?.children
                .OfType<PawnRenderNode_Head>()
                .FirstOrDefault(x => x?.Props?.tagDef == bedAnimationSetting.parentBedAnimationDef.pawnRenderNodeTagDef);

            if (pawnRenderNode_Head == null)
            {
                return null;
            }

            // 데시케이트 모드 처리
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return HeadTypeDefOf.Skull.GetGraphic(pawn, Color.white);
            }

            // 셰이더 및 색상 설정
            var graphicData = bedAnimationSetting.graphicData;
            var skinShader = graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;
            var color = graphicData.color;

            if (bedAnimationSetting.setPawnColor)
            {
                color = pawnRenderNode_Head.ColorFor(pawn);
                skinShader = ShaderUtility.GetSkinShader(pawn);
            }

            // 그래픽 생성 및 반환
            return GraphicDatabase.Get<Graphic_Multi>(graphicData.texPath, skinShader, graphicData.drawSize * 1.25f, color);
        }

        public static Graphic GraphicForBody(Pawn pawn, BedAnimationSetting bedAnimationSetting)
        {
            // PawnRenderNode_Body를 찾습니다.
            PawnRenderNode_Body pawnRenderNode_Body = pawn.Drawer.renderer.renderTree.rootNode?.children
                .OfType<PawnRenderNode_Body>()
                .FirstOrDefault(child => child?.Props?.tagDef == bedAnimationSetting.parentBedAnimationDef.pawnRenderNodeTagDef);

            if (pawnRenderNode_Body == null)
            {
                return null;
            }

            // Shader 초기화
            var shader = bedAnimationSetting.graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;

            // 데시케이트 모드 처리
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
            }

            // 색상 및 셰이더 처리
            var color = bedAnimationSetting.graphicData.color;
            if (bedAnimationSetting.setPawnColor)
            {
                color = pawnRenderNode_Body.ColorFor(pawn);
                shader = pawnRenderNode_Body.ShaderFor(pawn);
            }

            // 그래픽 생성 및 반환
            return GraphicDatabase.Get<Graphic_Multi>(bedAnimationSetting.graphicData.texPath, shader, bedAnimationSetting.graphicData.drawSize * 1.25f, color);
        }

        public static void SetAnimation(Building_AnimationBed building_AnimationBed)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;
            var animationSettingComp = building_AnimationBed.AnimationSettingComp;

            if (!MakeBedAnimation(building_AnimationBed))
            {
                return;
            }
            // Bed 애니메이션 업데이트
            foreach (var bedAnimationSettingAndTick in animationSettingComp.bedAnimationSettingAndTicks)
            {
                if (building_AnimationBed.setAnimation)
                {
                    bedAnimationSettingAndTick.currentTick = 0;
                }
                else if (bedAnimationSettingAndTick.parentBedAnimationDef.autoDurationTicksSetting)
                {
                    bedAnimationSettingAndTick.currentTick = pawn.Drawer.renderer.renderTree.AnimationTick;
                }
                else
                {
                    bedAnimationSettingAndTick.currentTick =
                        bedAnimationSettingAndTick.currentTick < bedAnimationSettingAndTick.durationTick - 1
                        ? bedAnimationSettingAndTick.currentTick + 1 : 0;
                }

                if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentTick)
                {
                    Log.Error($"{bedAnimationSettingAndTick.parentBedAnimationDef} : " + bedAnimationSettingAndTick.currentTick + $"(current texture : {bedAnimationSettingAndTick.durationTick})");
                }
            }

            building_AnimationBed.setAnimation = false;
        }

    }
}