//using HarmonyLib;
//using System.Collections.Generic;
//using Verse;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    [StaticConstructorOnStartup]
//    public static class IFCHarmony
//    {
//        static IFCHarmony()
//        {
//            Harmony harmony = new Harmony("IFC.Patch");
//            harmony.PatchAll();
//        }
//    }
//}
//        public static IEnumerable<Gizmo> GetExtraEquipmentGizmosPassThrough(IEnumerable<Gizmo> values, Pawn_EquipmentTracker __instance)
//        {
//            foreach (Gizmo giz in values)
//            {
//                yield return giz;
//            }
//            bool flag = CanShowEquipmentGizmos();
//            if (flag)
//            {
//                List<ThingWithComps> list = __instance.AllEquipmentListForReading;
//                int num;
//                for (int i = 0; i < list.Count; i = num + 1)
//                {
//                    ThingWithComps eq = list[i];
//                    Comp_EggVibrator comp = ThingCompUtility.TryGetComp<Comp_EggVibrator>(eq);
//                    bool flag2 = comp != null;
//                    if (flag2)
//                    {
//                        foreach (Gizmo giz2 in comp.CompGetWornGizmosExtra())
//                        {
//                            yield return giz2;
//                        }
//                    }
//                    eq = null;
//                    comp = null;
//                    num = i;
//                }
//                list = null;
//            }
//            yield break;
//        }


//        public static bool CanShowEquipmentGizmos()
//        {
//            return !AtLeastTwoSelectedHaveDifferentWeapons();
//        }

//        private static bool AtLeastTwoSelectedHaveDifferentWeapons()
//        {
//            if (Find.Selector.NumSelected <= 1)
//            {
//                return false;
//            }
//            ThingDef thingDef = null;
//            bool flag = false;
//            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
//            for (int i = 0; i < selectedObjectsListForReading.Count; i++)
//            {
//                Pawn pawn = selectedObjectsListForReading[i] as Pawn;
//                if (pawn != null)
//                {
//                    ThingDef thingDef2;
//                    if (pawn.equipment == null || pawn.equipment.Primary == null)
//                    {
//                        thingDef2 = null;
//                    }
//                    else
//                    {
//                        thingDef2 = pawn.equipment.Primary.def;
//                    }
//                    if (!flag)
//                    {
//                        thingDef = thingDef2;
//                        flag = true;
//                    }
//                    else if (thingDef2 != thingDef)
//                    {
//                        return true;
//                    }
//                }
//            }
//            return false;
//        }
//    }
//}
