using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    internal class BedAnimationUtility
    {

        internal static void DrawBedAnimation(Building_AnimationBed building_AnimationBed, CompAnimationSetting animationSettingComp, Pawn pawn)
        {
            Vector3 bedBasePos = building_AnimationBed.DrawPos;

            foreach (var bedAnimationSettingAndTick in animationSettingComp.bedAnimationSettingAndTicks)
            {
                int currentTick = bedAnimationSettingAndTick.currentTick;

                // `currentTick`보다 큰 첫 번째 설정 찾기
                //var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings.FirstOrDefault(b => b.tick > currentTick);
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
                            Log.Error($"pawnRenderNodeTagDef offset : {offset.x.ToString("F5")}, {offset.y.ToString("F5")}, {offset.z.ToString("F5")}");
                            Log.Error($"pawnRenderNodeTagDef pivot : {pivot.x.ToString("F5")}, {pivot.y.ToString("F5")}, {pivot.z.ToString("F5")}");

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
                //closestSetting.graphic.data.drawSize.x += closestSetting.testDrawSize.x;
                //closestSetting.graphic.data.drawSize.y += closestSetting.testDrawSize.y;

                if (parentBedAnimationDef.logCurrentOffset)
                {
                    Log.Error($"parentBedAnimationDef defName : {pos.x.ToString("F5")}, {pos.y.ToString("F5")}, {pos.z.ToString("F5")}");
                }

                // 그래픽을 위치에 그리기
                closestSetting.graphic.Draw(pos, Rot4.North, pawn);
            }
        }


        internal static bool MakeBedAnimation(Building_AnimationBed building_AnimationBed, CompAnimationSetting animationSettingComp)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;

            if (animationSettingComp.needMakeGraphics == false)
            {
                return true; // 애니메이션 설정이 필요 없거나 없으면 바로 반환
            }

            if (pawn == null || animationSettingComp == null || pawn.Drawer.renderer.renderTree.rootNode == null)
            {
                return false; // 그래픽을 만들 수 없는 경우
            }

            animationSettingComp.needMakeGraphics = false; // 필요성 초기화
            animationSettingComp.bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

            var Props = animationSettingComp.Props;


            //Log.Error(pawn.Label);
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

                //Log.Error(bedAnimationDef.defName);

                var result = MakeBedAnimationSettingAndTick(bedAnimationDef, animationSettingComp, bedAnimation, pawn);

                animationSettingComp.bedAnimationSettingAndTicks.Add(result);
            }
            //Log.Error("==========");

            return true; // 애니메이션 설정이 성공적으로 완료된 경우
        }


        public static BedAnimationSettingAndTick MakeBedAnimationSettingAndTick(BedAnimationDef bedAnimationDef, CompAnimationSetting animationSettingComp, BedAnimation bedAnimation, Pawn pawn)
        {
            var bedAnimationSettings = new List<BedAnimationSetting>();

            var bedAnimationSettingAndTick = new BedAnimationSettingAndTick
            {
                autoDurationTicksSetting = bedAnimationDef.autoDurationTicksSetting,
                parentBedAnimationDef = bedAnimationDef,
                bedAnimationSettings = bedAnimationSettings
            };

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

                // Set appropriate graphic
                settingCopy.graphic = bedAnimationDef.isPawnTextureReplace
                    ? GetGraphic(pawn, bedAnimationDef.pawnRenderNodeTagDef, settingCopy)
                    : settingCopy.graphicData.Graphic;

                bedAnimationSettings.Add(settingCopy);

                if (Condition.NeedBreak(bedAnimationSetting.condition))
                {
                    break;
                }
            }

            // Determine pawnAnimationDef based on conditions
            var pawnAnimationDef = GetPawnAnimationDef(animationSettingComp, pawn);

            // Determine durationTick based on autoDurationTicksSetting
            bedAnimationSettingAndTick.durationTick = bedAnimationDef.autoDurationTicksSetting
                ? pawnAnimationDef.durationTicks
                : bedAnimationDef.durationTicks != 0
                    ? bedAnimationDef.durationTicks
                    : bedAnimationSettings.Max(x => x.tick);

            return bedAnimationSettingAndTick;
        }

        private static Graphic GetGraphic(Pawn pawn, PawnRenderNodeTagDef tagDef, BedAnimationSetting setting)
        {
            if (tagDef == PawnRenderNodeTagDefOf.Body)
            {
                return GraphicForBody(pawn, setting);
            }
            else if (tagDef == PawnRenderNodeTagDefOf.Head)
            {
                return GraphicForHead(pawn, setting);
            }

            return setting.graphicData.Graphic;

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

        public static void SetAnimation(Building_AnimationBed building_AnimationBed)
        {
            Pawn pawn = building_AnimationBed.HeldPawn;
            var animationSettingComp = building_AnimationBed.AnimationSettingComp;

            if (!MakeBedAnimation(building_AnimationBed, animationSettingComp))
            {
                return;
            }


            // 조건에 따라 pawnAnimationDef를 설정
            if (building_AnimationBed.setAnimation)
            {
                PawnAnimationSetting pawnAnimationSetting = animationSettingComp.Props.pawnAnimationSetting;
                var pawnAnimationDef = pawnAnimationSetting.pawnAnimationDef;
                foreach (var conditionPawnAnimation in pawnAnimationSetting.conditonPawnAnimations)
                {
                    if (Condition.Match(pawn, building_AnimationBed, conditionPawnAnimation.condition))
                    {
                        pawnAnimationDef = conditionPawnAnimation.pawnAnimationDef;

                        if (Condition.NeedBreak(conditionPawnAnimation.condition))
                        {
                            break;
                        }
                    }
                }

                // 애니메이션 설정
                pawn.Drawer.renderer.SetAnimation(pawnAnimationDef);

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