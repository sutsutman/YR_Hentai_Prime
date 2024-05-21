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
            if (parms.pawn.jobs.curDriver is JobDriver_WatchDummyForJoy)
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

            if (parms.pawn.jobs.curDriver is JobDriver_WatchDummyForJoy)
            {
                return false;
            }
            return true;
        }
    }
}
