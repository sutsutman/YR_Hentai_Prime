using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static YR_WearSetting.WearSettingMethod;

namespace YR_WearSetting
{
    [StaticConstructorOnStartup]
    public static class Harmony_EquipmentUtility
    {
        static Harmony_EquipmentUtility()
        {
            var harmony = new Harmony("Mincho_The_Mint_Choco_Slime");

            harmony.Patch(AccessTools.Method(typeof(EquipmentUtility), "CanEquip", new Type[]
                {
                typeof(Thing),
                typeof(Pawn),
                typeof(string).MakeByRefType(),
                typeof(bool)
                }, null), null, new HarmonyMethod(patchType, "CanEquipPostfix", null), null, null);
        }

        public static void CanEquipPostfix(ref bool __result, Thing thing, Pawn pawn, ref string cantReason)
        {
            if (__result)
            {
                ThingDef thingDef = thing.def;
                bool isApparel = thingDef.IsApparel;
                bool isWeapon = thingDef.IsWeapon;
                var pawn_ThingComp_WearSetting = pawn.GetComps<ThingComp_WearSetting>();

                //무기 잠금 상태에서 무기 못 바꾸게
                if (isWeapon)
                {
                    TestError("1");
                    foreach (var thingComp_WearSetting in pawn_ThingComp_WearSetting)
                    {
                        foreach (var wearSetting in thingComp_WearSetting.Props.wearSetting)
                        {
                            foreach (var wearList in wearSetting.wearList)
                            {
                                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
                                {
                                    if (equipment.def == wearList && wearSetting.lockWeapon)
                                    {
                                        __result = false;
                                        cantReason = string.Format(wearSetting.lockWeaponCantReason ?? "LockWeaponCantReason_Default".Translate(
                                            pawn.def.LabelCap,
                                            pawn.LabelShort,
                                            thingDef.LabelCap
                                            ));
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                //특정 레이어 장비 못줍게
                foreach (var thingComp_WearSetting in pawn_ThingComp_WearSetting)
                {
                    foreach (var wearSetting in thingComp_WearSetting.Props.wearSetting)
                    {
                        foreach (var wearList in wearSetting.wearList)
                        {
                            if ((isApparel || isWeapon) && thingDef == wearList)
                            {
                                TestError("2");
                                TestError(wearSetting.label);
                                WearSettingBoolMaker(wearSetting, pawn, out WearSettingBool wearSettingBool, out WearSettingBoolString wearSettingBoolString);
                                TestError("wearSettingBool : " + wearSettingBool.ToString());
                                if (wearSettingBool.canWearBool)
                                {
                                    TestError("canWearBool");
                                    __result = true;
                                    return;
                                }
                                if (wearSettingBool.cantWearBool)
                                {
                                    TestError("cantWearBool");
                                    CantBoolStringMaker(out __result, out cantReason, thing, pawn, wearSetting, wearSettingBool, wearSettingBoolString);
                                    return;
                                }
                            }
                        }
                    }
                }

                //옷만
                if (isApparel)
                {
                    TestError("3");
                    foreach (var layer in thingDef.apparel.layers)
                    {
                        if (WearSettingDictionary.ContainsKey(layer))
                        {
                            foreach (var thingComp_WearSetting in pawn_ThingComp_WearSetting)
                            {
                                foreach (var wearSetting in thingComp_WearSetting.Props.wearSetting)
                                {
                                    if (WearSettingDictionary[layer](wearSetting) && !wearSetting.wearList.Contains(thingDef))
                                    {
                                        if (OnlyWearListApparelLogic(ref __result, thing, pawn, ref cantReason, wearSetting))
                                        {
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //무기만
                else if (isWeapon)
                {
                    TestError("4");
                    foreach (var thingComp_WearSetting in pawn_ThingComp_WearSetting)
                    {
                        foreach (var wearSetting in thingComp_WearSetting.Props.wearSetting)
                        {
                            if (wearSetting.onlyWearListApparel.weapon && !wearSetting.wearList.Contains(thingDef))
                            {
                                if (OnlyWearListApparelLogic(ref __result, thing, pawn, ref cantReason, wearSetting))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void TestError(string error)
        {
            bool test = false;
            if (test)
            {
                Log.Error(error);
            }
        }

        private static bool OnlyWearListApparelLogic(ref bool __result, Thing thing, Pawn pawn, ref string cantReason, WearSetting wearSetting)
        {
            TestError(wearSetting.label);
            WearSettingBoolMaker(wearSetting,
                                 pawn,
                                 out WearSettingBool wearSettingBool,
                                 out WearSettingBoolString wearSettingBoolString);
            if (wearSettingBool.canWearBool)
            {
                TestError("OnlyWearListApparelLogic");
                CantBoolStringMaker(out __result, out cantReason, thing, pawn, wearSetting, wearSettingBool, wearSettingBoolString);
                return true;
            }
            return false;
        }

        public static string GetBoolString(string falseString, string trueString, bool condition)
        {
            return condition ? trueString ?? "(V)" : falseString ?? "";
        }

        private static void CantBoolStringMaker(out bool __result, out string cantReason, Thing thing, Pawn pawn, WearSetting wearSetting, WearSettingBool wearSettingBool, WearSettingBoolString wearSettingBoolString)
        {
            __result = false;
            BoolString boolString = wearSetting.boolString;

            string ageBoolString = GetBoolString(boolString.ageFalse.Translate(), boolString.ageTrue.Translate(), wearSettingBool.age);
            string genderBoolString = GetBoolString(boolString.genderFalse.Translate(), boolString.genderTrue.Translate(), wearSettingBool.gender);
            string bodyTypeBoolString = GetBoolString(boolString.bodyTypeFalse.Translate(), boolString.bodyTypeTrue.Translate(), wearSettingBool.bodyType);
            string orHediffBoolString = GetBoolString(boolString.orHediffFalse.Translate(), boolString.orHediffTrue.Translate(), wearSettingBool.orHediff);
            string andHediffBoolString = GetBoolString(boolString.andHediffFalse.Translate(), boolString.andHediffTrue.Translate(), wearSettingBool.andHediff);
            string forbiddenHediffBoolString = GetBoolString(boolString.forbiddenHediffFalse.Translate(), boolString.forbiddenHediffTrue.Translate(), !wearSettingBool.forbiddenHediff);
            string mustWearWithBoolString = GetBoolString(boolString.mustWearWithFalse.Translate(), boolString.mustWearWithTrue.Translate(), wearSettingBool.mustWearWith);

            string reasonKey = wearSetting.cantReason ?? "WearSettingForbidden";
            cantReason = CantReasonMaker(reasonKey, thing, pawn, wearSetting, wearSettingBoolString, ageBoolString, genderBoolString, bodyTypeBoolString, orHediffBoolString, andHediffBoolString, forbiddenHediffBoolString, mustWearWithBoolString);
        }

        private static string CantReasonMaker(string label, Thing thing, Pawn pawn, WearSetting wearSetting, WearSettingBoolString wearSettingBoolString, string ageBoolString, string genderBoolString, string bodyTypeBoolString, string orHediffBoolString, string andHediffBoolString, string forbiddenHediffBoolString, string mustWearWithBoolString)
        {
            return string.Format(label.Translate(pawn.def.LabelCap,//0
                                                 pawn.LabelShort,//1
                                                 thing.def.LabelCap,//2
                                                 ageBoolString,//3
                                                 wearSetting.condition.age.min,//4
                                                 wearSetting.condition.age.max,//5
                                                 genderBoolString,//6
                                                 wearSettingBoolString.genderString,//7
                                                 bodyTypeBoolString,//8
                                                 wearSettingBoolString.bodyTypeString,//9
                                                 orHediffBoolString,//10
                                                 wearSettingBoolString.orHediffString,//11
                                                 andHediffBoolString,//12
                                                 wearSettingBoolString.andHediffString,//13
                                                 forbiddenHediffBoolString,//14
                                                 wearSettingBoolString.forbiddenHediffString,//15
                                                 mustWearWithBoolString,//16
                                                 wearSettingBoolString.mustWearWithString));//17
        }

        private static readonly Type patchType = typeof(Harmony_EquipmentUtility);
    }
}
