using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationUtility
    {
        public static bool MakeBedAnimation(Building_AnimationBed building_AnimationBed)
        {

            MakeFillableBar(building_AnimationBed);

            Pawn HeldPawn = building_AnimationBed.HeldPawn;

            if (building_AnimationBed.AnimationSettingComp == null || HeldPawn == null || HeldPawn.Drawer.renderer.renderTree.rootNode == null)
            {
                TestLog.Error("can't make Graphic");
                return false;
            }

            if (!building_AnimationBed.AnimationSettingComp.needMakeGraphics)
            {
                return true;
            }

            TestLog.Error($"{HeldPawn.LabelShort} : MakeBedAnimation Start");
            building_AnimationBed.AnimationSettingComp.needMakeGraphics = false; // 필요성 초기화
            building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

            var Props = building_AnimationBed.AnimationSettingComp.Props;
            var pawnAnimationDef = GetPawnAnimationDef(building_AnimationBed);
            HeldPawn.Drawer.renderer.SetAnimation(pawnAnimationDef);

            TestLog.Error("+++Check bedAnimationList+++");
            int i = 0;
            foreach (var bedAnimation in Props.bedAnimationList)
            {
                TestLog.Error($"bedAnimation 설정 : {i}");
                i++;
                BedAnimationDef bedAnimationDef = bedAnimation.bedAnimationDef;
                Vector3 offset = bedAnimation.offset;
                Vector2 drawSize = bedAnimation.drawSize;

                int ci = 0;
                foreach (var conditionBedAnimationDef in bedAnimation.conditionBedAnimationDefs)
                {
                    TestLog.Error($"conditionBedAnimationDef 설정 : {ci}");
                    ci++;
                    if (conditionBedAnimationDef.bedAnimationDef == null)
                    {
                        TestLog.Error($"conditionBedAnimationDef.bedAnimationDef == null");
                        continue;
                    }
                    void action()
                    {
                        bedAnimationDef = conditionBedAnimationDef.bedAnimationDef;
                        offset = conditionBedAnimationDef.offset;
                        drawSize = conditionBedAnimationDef.drawSize;
                    }

                    if (Condition.ExecuteActionIfConditionMatches(HeldPawn, building_AnimationBed, conditionBedAnimationDef.condition, action))
                    {
                        break;
                    }
                }

                TestLog.Error($"=={bedAnimationDef.defName}==");
                TestLog.Error("++MakeBedAnimationSettingAndTick Start++");

                var result = MakeBedAnimationSettingAndTick(building_AnimationBed, bedAnimationDef, offset, drawSize, pawnAnimationDef);
                building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks.Add(result);

                TestLog.Error("++MakeBedAnimationSettingAndTick Finish++");
            }

            TestLog.Error("+++Finish Check bedAnimationList+++");
            TestLog.Error("=============================");

            return true;
        }
        //바 정보 만들기(반투명, 물 그릴때 씀)
        public static void MakeFillableBar(Building_AnimationBed building_AnimationBed)
        {
            building_AnimationBed.AnimationSettingComp.fillableBarIngredients = new List<FillableBarIngredient>();
            foreach (var fillableBarSetting in building_AnimationBed.AnimationSettingComp.Props.fillableBarSettings)
            {
                if (Condition.Match(building_AnimationBed.HeldPawn, building_AnimationBed, fillableBarSetting.condition, out bool needBreak))
                {
                    FillableBarIngredient fillableBarIngredient = new FillableBarIngredient
                    {
                        pawn = building_AnimationBed.HeldPawn,
                        offset = fillableBarSetting.offset,
                        drawSize = fillableBarSetting.drawSize,
                        color = fillableBarSetting.color,
                    };

                    building_AnimationBed.AnimationSettingComp.fillableBarIngredients.Add(fillableBarIngredient);

                    if (needBreak)
                    {
                        break;
                    }
                }
            }
        }

        // 포트레잇 정보 만들기
        public static void MakePortrait(Building_AnimationBed building_AnimationBed)
        {
            //여기다가 다른 모든 포트레잇 리셋 스위치를 넣기?
            var portraitIngredients = building_AnimationBed.AnimationSettingComp.portraitIngredients;
            portraitIngredients.Clear();  // 이전 데이터를 제거하여 초기화합니다.

            foreach (var pawnPortraitSetting in building_AnimationBed.AnimationSettingComp.Props.pawnPortraitSettings)
            {
                var portraitSetting = pawnPortraitSetting.portraitSetting;

                foreach (var conditionPortraitSetting in pawnPortraitSetting.conditonPortraitSettings)
                {
                    var tempPawn = conditionPortraitSetting.portraitSetting.drawJoyPawn
                        ? building_AnimationBed.dummyForJoyPawn
                        : building_AnimationBed.HeldPawn;

                    if (tempPawn == null)
                    {
                        continue;
                    }

                    void action() => portraitSetting = conditionPortraitSetting.portraitSetting;

                    if (Condition.ExecuteActionIfConditionMatches(tempPawn, building_AnimationBed, conditionPortraitSetting.condition, action))
                    {
                        break;
                    }
                }

                if (portraitSetting == null || !portraitSetting.draw)
                {
                    continue;
                }

                var drawPawn = portraitSetting.drawJoyPawn ? building_AnimationBed.dummyForJoyPawn : building_AnimationBed.HeldPawn;

                if (drawPawn == null)
                {
                    continue;
                }

                var drawSize = Vector3.zero;
                if (Condition.Match(drawPawn, building_AnimationBed, portraitSetting.visibleCondition, out _))
                {
                    drawSize = portraitSetting.drawSize;
                }

                portraitIngredients.Add(new PortraitIngredient
                {
                    label = portraitSetting.label,
                    pawn = drawPawn,
                    drawSize = drawSize,
                    angle = portraitSetting.angle,
                    offset = portraitSetting.offset,
                    portraitMesh = portraitSetting.portraitMeshGraphicData.Graphic.MeshAt(Rot4.South),
                    iconMat = CreateMaterial(portraitSetting, drawPawn),
                    cameraOffset = portraitSetting.cameraOffset,
                    cameraZoom = portraitSetting.cameraZoom,
                    portraitSetting = portraitSetting
                });
            }
        }

        private static Material CreateMaterial(PortraitSetting portraitSetting, Pawn pawn)
        {
            var mainTexture = PortraitsCache.Get(pawn, new Vector2(256, 256), portraitSetting.rotation, default, 1, renderClothes: true, renderHeadgear: true, stylingStation: false, healthStateOverride: PawnHealthState.Mobile);
            var maskTex = ContentFinder<Texture2D>.Get(portraitSetting.maskPath);

            var req = new MaterialRequest
            {
                mainTex = mainTexture,
                color = Color.white,
                shader = ShaderDatabase.CutoutComplex,
                maskTex = maskTex
            };

            return MaterialPool.MatFrom(req);
        }


        public static BedAnimationSettingAndTick MakeBedAnimationSettingAndTick(Building_AnimationBed building_AnimationBed, BedAnimationDef bedAnimationDef, Vector3 offset, Vector2 drawSize, AnimationDef pawnAnimationDef)
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

                Pawn HeldPawn = building_AnimationBed.HeldPawn;
                void action()
                {
                    var settingCopy = new BedAnimationSetting();
                    settingCopy = bedAnimationSetting.Copy();
                    settingCopy.parentBedAnimationDef = bedAnimationDef;
                    settingCopy.setPawnColor = bedAnimationDef.setPawnColor;
                    settingCopy.offset = bedAnimationSetting.offset + offset + bedAnimationDef.offset;

                    GraphicData graphicData = settingCopy.graphicData;

                    foreach (var conditionGraphicData in bedAnimationSetting.conditionGraphicDatas)
                    {
                        void choiceGraphicData()
                        {
                            graphicData = conditionGraphicData.graphicData;
                        };
                        if (Condition.ExecuteActionIfConditionMatches(HeldPawn, building_AnimationBed, conditionGraphicData.condition, choiceGraphicData))
                        {
                            break;
                        }
                    }


                    if (graphicData != null)
                    {
                        // Set appropriate graphic(그래픽 제작)
                        settingCopy.graphic = bedAnimationDef.isPawnTextureReplace
                            ? GetGraphic(HeldPawn, bedAnimationDef.pawnRenderNodeTagDef, settingCopy, graphicData)
                            : graphicData.Graphic;

                        if (settingCopy.graphic != null && !bedAnimationDef.isPawnTextureReplace)
                        {
                            settingCopy.graphic.drawSize = graphicData.drawSize;
                            settingCopy.graphic.drawSize += drawSize;
                        }

                        if (settingCopy.graphic == null)
                        {
                            TestLog.Error("settingCopy Skip");
                        }
                        else
                        {
                            bedAnimationSettings.Add(settingCopy);
                        }
                    }
                }

                if (Condition.ExecuteActionIfConditionMatches(HeldPawn, building_AnimationBed, bedAnimationSetting.condition, action))
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

        private static Graphic GetGraphic(Pawn pawn, PawnRenderNodeTagDef tagDef, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
        {
            Graphic graphic;
            if (tagDef == PawnRenderNodeTagDefOf.Body)
            {
                graphic = GraphicForBody(pawn, bedAnimationSetting, graphicData);
            }
            else if (tagDef == PawnRenderNodeTagDefOf.Head)
            {
                graphic = GraphicForHead(pawn, bedAnimationSetting, graphicData);
            }
            else
            {
                graphic = graphicData.Graphic;
            }

            return graphic;

        }

        private static AnimationDef GetPawnAnimationDef(Building_AnimationBed building_AnimationBed)
        {
            var pawnAnimationDef = building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.pawnAnimationDef;

            foreach (var conditonPawnAnimation in building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.conditonPawnAnimations)
            {
                void action() => pawnAnimationDef = conditonPawnAnimation.pawnAnimationDef;

                if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed.HeldPawn, building_AnimationBed, conditonPawnAnimation.condition, action))
                {
                    break;
                }
            }

            return pawnAnimationDef;
        }

        public static Graphic GraphicForHead(Pawn pawn, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
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

        public static Graphic GraphicForBody(Pawn pawn, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
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
            var shader = graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;

            // 데시케이트 모드 처리
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
            }

            // 색상 및 셰이더 처리
            var color = graphicData.color;
            if (bedAnimationSetting.setPawnColor)
            {
                color = pawnRenderNode_Body.ColorFor(pawn);
                shader = pawnRenderNode_Body.ShaderFor(pawn);
            }

            // 그래픽 생성 및 반환
            return GraphicDatabase.Get<Graphic_Multi>(graphicData.texPath, shader, graphicData.drawSize * 1.25f, color);
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

                //Log.Error(bedAnimationSettingAndTick.currentTick.ToString());
                if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentTick)
                {
                    Log.Error($"{bedAnimationSettingAndTick.parentBedAnimationDef} : " + bedAnimationSettingAndTick.currentTick + $"(current texture : {bedAnimationSettingAndTick.durationTick})");
                }
            }

            building_AnimationBed.setAnimation = false;
        }

    }
}