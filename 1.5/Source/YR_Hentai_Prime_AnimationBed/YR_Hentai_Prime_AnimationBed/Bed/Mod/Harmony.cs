using AlienRace;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body), "CanDrawNow")]
    internal class Patch_PawnRenderNodeWorker_Apparel_Body_CanDrawNow
    {
        public static bool Prefix(PawnDrawParms parms)
        {
            if (parms.pawn.holdingOwner?.Owner is Building_AnimationBed building_AnimationBed)
            {
                if (building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.renderClothes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (parms.pawn.jobs?.curDriver is JobDriver_WatchDummyForJoy jobDriver_WatchDummyForJoy && jobDriver_WatchDummyForJoy.watchNow)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Head), "CanDrawNow")]
    internal class Patch_PawnRenderNodeWorker_Apparel_Head_CanDrawNow
    {
        public static bool Prefix(PawnDrawParms parms)
        {
            if (parms.pawn.holdingOwner?.Owner is Building_AnimationBed building_AnimationBed)
            {
                if (building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.renderHeadgear)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (parms.pawn.jobs?.curDriver is JobDriver_WatchDummyForJoy jobDriver_WatchDummyForJoy && jobDriver_WatchDummyForJoy.watchNow)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderTree), "get_AnimationTick")]
    internal class Patch_AnimationTick
    {
        public static void Postfix(PawnRenderTree __instance, ref int __result)
        {
            if (__instance.pawn.holdingOwner.Owner is Building_AnimationBed building_AnimationBed && !building_AnimationBed.PowerOn)
            {
                __result = building_AnimationBed.tempAnimationTick;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_Body), "GraphicFor")]
    internal class Patch_PawnRenderNode_Body_GraphicFor
    {
        public static void Postfix(PawnRenderNode_Body __instance, Pawn pawn, ref Graphic __result)
        {
            if (pawn.jobs?.curDriver is JobDriver_WatchDummyForJoy jobDriver_WatchDummyForJoy && jobDriver_WatchDummyForJoy.watchNow)
            {

                AlienRenderTreePatches.PawnRenderResolveData pawnRenderData = AlienRenderTreePatches.RegenerateResolveData(pawn);
                int sharedIndex = pawnRenderData.sharedIndex;
                GraphicPaths graphicPaths = pawnRenderData.alienProps.alienRace.graphicPaths;
                AlienPartGenerator.AlienComp alienComp = pawnRenderData.alienComp;
                AlienPartGenerator apg = pawnRenderData.alienProps.alienRace.generalSettings.alienPartGenerator;
                string bodyPath = graphicPaths.body.GetPath(pawn, ref sharedIndex, (alienComp.bodyVariant < 0) ? null : new int?(alienComp.bodyVariant), null);
                alienComp.bodyVariant = sharedIndex;
                string bodyMask = graphicPaths.bodyMasks.GetPath(pawn, ref sharedIndex, (alienComp.bodyMaskVariant < 0) ? null : new int?(alienComp.bodyMaskVariant), null);
                alienComp.bodyMaskVariant = sharedIndex;
                pawnRenderData.sharedIndex = sharedIndex;
                ShaderTypeDef skinShader2 = graphicPaths.skinShader;
                Shader skinShader = ((skinShader2 != null) ? skinShader2.Shader : null) ?? ShaderUtility.GetSkinShader(pawn);
                if (skinShader == ShaderDatabase.CutoutSkin && pawn.story.SkinColorOverriden)
                {
                    skinShader = ShaderDatabase.CutoutSkinColorOverride;
                }
                Shader shader = __instance.ShaderFor(pawn);
                if (pawn.def is AlienRace.ThingDef_AlienRace thingDef_AlienRace)
                {
                    if (skinShader == ShaderDatabase.CutoutSkin && pawn.story.SkinColorOverriden)
                    {
                        shader = ShaderDatabase.CutoutSkinColorOverride;
                    }
                }

                foreach (var animationPart in pawn.Drawer.renderer.CurAnimation.animationParts)
                {
                    for (int i = 0; i < animationPart.Value.keyframes.Count; i++)
                    {
                        var currentKeyframe = animationPart.Value.keyframes[i];
                        var animationTick = pawn.Drawer.renderer.renderTree.AnimationTick;

                        if (currentKeyframe is BedAnimationKeyframe BAK)
                        {
                            // 마지막 키프레임인지 확인
                            if (i == animationPart.Value.keyframes.Count - 1)
                            {
                                // 마지막 키프레임 이후로는 해당 키프레임의 텍스처 유지
                                if (currentKeyframe.tick <= animationTick)
                                {
                                    if (!BAK.texPath.NullOrEmpty())
                                    {
                                        __result = ((!BAK.texPath.NullOrEmpty()) ? CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), BAK.texPath, AlienRenderTreePatches.CheckMaskShader(BAK.texPath, skinShader, !bodyMask.NullOrEmpty()), Vector2.one, __instance.ColorFor(pawn), apg.SkinColor(pawn, false), null, 0, graphicPaths.SkinColoringParameter, bodyMask)) : null);
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                var nextKeyframe = animationPart.Value.keyframes[i + 1];
                                // 현재 애니메이션 틱이 두 키프레임 사이에 있을 때 텍스처 변경
                                if (currentKeyframe.tick <= animationTick && animationTick < nextKeyframe.tick)
                                {
                                    if (!BAK.texPath.NullOrEmpty())
                                    {
                                        __result = ((!BAK.texPath.NullOrEmpty()) ? CachedData.getInnerGraphic(new GraphicRequest(typeof(Graphic_Multi), BAK.texPath, AlienRenderTreePatches.CheckMaskShader(BAK.texPath, skinShader, !bodyMask.NullOrEmpty()), Vector2.one, __instance.ColorFor(pawn), apg.SkinColor(pawn, false), null, 0, graphicPaths.SkinColoringParameter, bodyMask)) : null);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
