using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static RimWorld.MechClusterSketch;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationUtility
    {
        public static void DrawBedAnimation(Building_AnimationBed building_AnimationBed, CompAnimationSetting animationSettingComp, Pawn pawn)
        {
            Vector3 bedBasePos = building_AnimationBed.DrawPos;

            foreach (var bedAnimationSettingAndTick in animationSettingComp.bedAnimationSettingAndTicks)
            {
                Vector3 pos = new Vector3(0, 0, 0);

                int currentTick = bedAnimationSettingAndTick.currentTick;

                var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                    .Where(b => b.tick <= currentTick)
                    .OrderByDescending(b => b.tick)
                    .FirstOrDefault();

                // 설정이 없을 경우 처리
                if (closestSetting == null && (bedAnimationSettingAndTick.autoDurationTicksSetting || currentTick <= bedAnimationSettingAndTick.durationTick))
                {
                    closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                        .OrderByDescending(x => x.tick)
                        .FirstOrDefault();

                    if (closestSetting == null)
                        continue; // 설정이 없으면 다음으로 넘어감
                }

                pos = CalcuratePos(building_AnimationBed, pawn, bedBasePos, bedAnimationSettingAndTick, closestSetting);

                closestSetting.graphic?.Draw(pos, Rot4.North, pawn);
            }
        }

        private static Vector3 CalcuratePos(Building_AnimationBed building_AnimationBed, Pawn pawn, Vector3 bedBasePos, BedAnimationSettingAndTick bedAnimationSettingAndTick, BedAnimationSetting closestSetting)
        {
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

        public static void DrawBedAnimation_Old(Building_AnimationBed building_AnimationBed, CompAnimationSetting animationSettingComp, Pawn pawn)
        {
            Vector3 bedBasePos = building_AnimationBed.DrawPos;

            foreach (var bedAnimationSettingAndTick in animationSettingComp.bedAnimationSettingAndTicks)
            {
                int currentTick = bedAnimationSettingAndTick.currentTick;

                // `currentTick`보다 큰 첫 번째 설정 찾기
                var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                    .Where(b => b.tick <= currentTick)
                    .OrderByDescending(b => b.tick)
                    .FirstOrDefault();

                if (closestSetting == null)
                {
                    if (bedAnimationSettingAndTick.autoDurationTicksSetting || currentTick <= bedAnimationSettingAndTick.durationTick)
                    {
                        closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                            .OrderByDescending(x => x.tick) // 가장 큰 tick을 찾음
                            .FirstOrDefault();
                    }


                    if (closestSetting == null)
                    {
                        continue; // 설정이 없으면 다음으로 넘어감
                    }
                }
                if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentTexture)
                {
                    Log.Error($"{bedAnimationSettingAndTick.parentBedAnimationDef} : {closestSetting.graphicData.texPath}");
                }
                // 기본 위치에 설정된 오프셋을 추가
                Vector3 pos = new Vector3(bedBasePos.x, pawn.DrawPos.y, bedBasePos.z) + closestSetting.offset;
                var posY = pos.y;
                BedAnimationDef parentBedAnimationDef = bedAnimationSettingAndTick.parentBedAnimationDef;
                if (parentBedAnimationDef.animationSynchro && parentBedAnimationDef.pawnRenderNodeTagDef != null)
                {
                    // 일치하는 렌더 노드를 찾아 추가적인 오프셋을 적용

                    PawnRenderNode renderNode = pawn.Drawer.renderer.renderTree.rootNode.children
                        .FirstOrDefault(n => n?.Props?.tagDef == parentBedAnimationDef.pawnRenderNodeTagDef);

                    if (renderNode != null)
                    {
                        Vector3 offset;

                        offset = renderNode.Worker.OffsetFor(renderNode, building_AnimationBed.HeldPawnDrawParms, out var pivot);

                        if (parentBedAnimationDef.logCurrentOffset)
                        {
                            Log.Error($"pawnRenderNodeTagDef offset : {offset.x:F5}, {offset.y:F5}, {offset.z:F5}");
                            Log.Error($"pawnRenderNodeTagDef pivot : {pivot.x:F5}, {pivot.y:F5}, {pivot.z:F5}");

                        }

                        //renderNode.GetTransform(building_AnimationBed.HeldPawnDrawParms, out var offset, out _, out _, out _);
                        offset -= pivot;
                        pos.x += offset.x;
                        pos.z += offset.z;
                    }
                }
                pos.y = posY;

                //테스트용
                pos += closestSetting.testOffset;

                if (parentBedAnimationDef.logCurrentOffset)
                {
                    Log.Error($"parentBedAnimationDef defName : {pos.x:F5}, {pos.y:F5}, {pos.z:F5}");
                }

                // 그래픽을 위치에 그리기

                closestSetting.graphic?.Draw(pos, Rot4.North, pawn);
            }
        }


        public static bool MakeBedAnimation(Building_AnimationBed building_AnimationBed, CompAnimationSetting animationSettingComp)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;

            if (animationSettingComp.needMakeGraphics == false)
            {
                return true; // 애니메이션 설정이 필요 없거나 없으면 바로 반환
            }

            if (pawn == null || animationSettingComp == null || pawn.Drawer.renderer.renderTree.rootNode == null)
            {
                TestLog.Error($"can't make Graphic");
                return false; // 그래픽을 만들 수 없는 경우
            }

            TestLog.Error($"{pawn.LabelShort} : MakeBedAnimation Start");
            animationSettingComp.needMakeGraphics = false; // 필요성 초기화
            animationSettingComp.bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

            var Props = animationSettingComp.Props;

            // Determine pawnAnimationDef based on conditions
            var pawnAnimationDef = GetPawnAnimationDef(animationSettingComp, pawn);
            pawn.Drawer.renderer.SetAnimation(pawnAnimationDef);

            TestLog.Error($"+++Check bedAnimationList+++");
            foreach (var bedAnimation in Props.bedAnimationList)
            {
                BedAnimationDef bedAnimationDef = bedAnimation.bedAnimationDef;

                // 컨디션에 따라 bedAnimationDef 선택
                foreach (var conditionBedAnimationDef in bedAnimation.conditionBedAnimationDefs)
                {
                    if (Condition.Match(pawn, building_AnimationBed, conditionBedAnimationDef.condition))
                    {
                        bedAnimationDef = conditionBedAnimationDef.bedAnimationDef;

                        if (Condition.NeedBreak(conditionBedAnimationDef.condition))
                        {
                            break;
                        }
                    }
                }
                TestLog.Error($"=={bedAnimationDef.defName}==");

                //Log.Error(bedAnimationDef.defName);

                TestLog.Error($"++MakeBedAnimationSettingAndTick Start++");

                var result = MakeBedAnimationSettingAndTick(bedAnimationDef, animationSettingComp, pawn, pawnAnimationDef);

                TestLog.Error("++MakeBedAnimationSettingAndTick Finish++");

                animationSettingComp.bedAnimationSettingAndTicks.Add(result);
            }

            TestLog.Error($"+++Finish Check bedAnimationList+++");
            TestLog.Error($"=============================");

            return true; // 애니메이션 설정이 성공적으로 완료된 경우
        }


        public static BedAnimationSettingAndTick MakeBedAnimationSettingAndTick(BedAnimationDef bedAnimationDef, CompAnimationSetting animationSettingComp, Pawn pawn, AnimationDef pawnAnimationDef)
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
                if (!Condition.Match(pawn, animationSettingComp.Building_AnimationBed, bedAnimationSetting.condition))
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
                        ? GetGraphic(pawn, bedAnimationDef.pawnRenderNodeTagDef, settingCopy)
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

        private static AnimationDef GetPawnAnimationDef(CompAnimationSetting animationSettingComp, Pawn pawn)
        {
            var pawnAnimationDef = animationSettingComp.Props.pawnAnimationSetting.pawnAnimationDef;

            foreach (var condition in animationSettingComp.Props.pawnAnimationSetting.conditonPawnAnimations)
            {
                if (Condition.Match(pawn, animationSettingComp.Building_AnimationBed, condition.condition))
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

        public static void MaterialAndMeshForPortrait(Pawn pawn, BedAnimationSetting bedAnimationSetting)
        {
            // 생성된 Material의 초기화
            Material iconMat = new Material(ShaderDatabase.CutoutComplex);

            // 필요한 변수들 초기화
            Vector2 iconSize = new Vector2(256, 256);
            Rot4 iconRot = Rot4.South;
            Vector3 cameraOffset = Vector3.zero;
            float cameraZoom = 1f;
            bool renderClothes = true;
            bool renderHeadgear = true;
            int angle = 0;
            // RenderTexture 생성
            RenderTexture renderTexture = PortraitsCache.Get(pawn, iconSize, iconRot, default, cameraZoom,
                renderClothes: renderClothes,
                renderHeadgear: renderHeadgear,
                stylingStation: false,
                healthStateOverride: PawnHealthState.Mobile);

            // iconMat에 mainTexture 및 color 설정
            iconMat.mainTexture = renderTexture;
            iconMat.color = Color.white;

            // mainTexture가 null이 아닌 경우에만 Material 및 portraitMesh 설정
            if (iconMat.mainTexture != null)
            {
                // Material 생성
                MaterialRequest req = new MaterialRequest
                {
                    //maskTex = ContentFinder<Texture2D>.Get(bedAnimationSetting.maskPathForPortrait),
                    mainTex = iconMat.mainTexture,
                    color = iconMat.color,
                    shader = iconMat.shader
                };
                //materialAndMesh = new MaterialAndMesh
                //{
                //    material = MaterialPool.MatFrom(req),
                //    mesh = bedAnimationSetting.graphicData.Graphic.MeshAt(Rot4.South),
                //    quaternion = Quaternion.AngleAxis(angle, Vector3.up)
                //};
            }
        }
        public static void SetAnimation(Building_AnimationBed building_AnimationBed)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;
            var animationSettingComp = building_AnimationBed.AnimationSettingComp;

            if (!MakeBedAnimation(building_AnimationBed, animationSettingComp))
            {
                return;
            }


            //Log.Error("========");
            //Log.Error("animation Tick : " + pawn.Drawer.renderer.renderTree.AnimationTick.ToString());
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