using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using Verse;

namespace YR_WearSetting
{
    [StaticConstructorOnStartup]
    public static class Harmony_JobGiver_OptimizeApparel
    {
        static Harmony_JobGiver_OptimizeApparel()
        {
            var harmony = new Harmony("Mincho_The_Mint_Choco_Slime");

            harmony.Patch(AccessTools.Method(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain", null, null), null, new HarmonyMethod(patchType, "ApparelScoreGainPostFix", null), null, null);
        }
        public static void ApparelScoreGainPostFix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (__result < 0f)
            {
                return;
            }

            foreach (var wearSettingDef in DefDatabase<WearSettingDef>.AllDefs)
            {
                // Checking if targetRaces is null or contains pawn.def
                if (wearSettingDef.targetRaces == null || wearSettingDef.targetRaces.Contains(pawn.def))
                {
                    ProcessWearSettings(pawn, ap, wearSettingDef, ref __result);
                }
            }
        }

        private static void ProcessWearSettings(Pawn pawn, Apparel ap, WearSettingDef wearSettingDef, ref float result)
        {
            foreach (var (wearSetting, wearListThingDef) in from wearSetting in wearSettingDef.wearSetting
                                                    from wearListThingDef in wearSetting.wearList
                                                    where ap.def == wearListThingDef
                                                            select (wearSetting, wearListThingDef))
            {
                WearSettingMethod.WearSettingBoolMaker(wearSetting,
                                                       pawn,
                                                       out WearSettingBool wearSettingBool,
                                                       out WearSettingBoolString wearSettingBoolString);

                if (wearSettingBool.cantWearBool)
                {
                    result = -50f;
                }
                else if (wearSettingBool.canWearBool)
                {
                    foreach (var layer in wearListThingDef.apparel?.layers)
                    {
                        if (WearSettingMethod.WearSettingDictionary.ContainsKey(layer))
                        {
                            if(WearSettingMethod.WearSettingDictionary[layer](wearSetting))
                            {
                                result = -50f;
                            }
                        }
                    }
                }
            }
        }


        private static readonly Type patchType = typeof(Harmony_JobGiver_OptimizeApparel);
    }
}
