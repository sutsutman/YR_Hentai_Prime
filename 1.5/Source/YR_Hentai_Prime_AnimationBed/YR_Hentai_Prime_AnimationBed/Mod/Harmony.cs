using HarmonyLib;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [HarmonyPatch(typeof(PawnRenderNodeWorker_Apparel_Body), "CanDrawNow")]
    internal class Patch_PawnRenderNodeWorker_Apparel_Body_CanDrawNow
    {
        public static bool Prefix(PawnDrawParms parms)
        {
            if (parms.pawn.holdingOwner.Owner is Building_AnimationBed building_AnimationBed)
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
            if (parms.pawn.jobs.curDriver is JobDriver_WatchDummyForJoy jobDriver_WatchDummyForJoy && jobDriver_WatchDummyForJoy.watchNow)
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
            if (parms.pawn.holdingOwner.Owner is Building_AnimationBed building_AnimationBed)
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

            if (parms.pawn.jobs.curDriver is JobDriver_WatchDummyForJoy jobDriver_WatchDummyForJoy && jobDriver_WatchDummyForJoy.watchNow)
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

}
