using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_ItemFormChange
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    public static class IFCHarmony
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        static IFCHarmony()
        {
            Harmony harmony = new Harmony("IFC.Patch");
            harmony.PatchAll();
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "GetGizmos", null, null), null, new HarmonyMethod(typeof(IFCHarmony), "GetExtraEquipmentGizmosPassThrough", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "EquipmentTrackerTick", null, null), null, new HarmonyMethod(typeof(IFCHarmony), "IFCPostTickEquipment", null), null, null);


            harmony.Patch(AccessTools.Method(typeof(Pawn_ApparelTracker), "GetGizmos", null, null), null, new HarmonyMethod(typeof(IFCHarmony), "GetExtraApparelGizmosPassThrough", null), null, null);
            harmony.Patch(AccessTools.Method(typeof(Pawn_ApparelTracker), "ApparelTrackerTick", null, null), null, new HarmonyMethod(typeof(IFCHarmony), "IFCPostTickApparel", null), null, null);
        }

        // Token: 0x06000002 RID: 2 RVA: 0x000020DC File Offset: 0x000002DC
        public static void IFCPostTickEquipment(Pawn_EquipmentTracker __instance)
        {
            List<ThingWithComps> allEquipmentListForReading = __instance.AllEquipmentListForReading;
            for (int i = 0; i < allEquipmentListForReading.Count; i++)
            {
                CompFormChange comp = allEquipmentListForReading[i].GetComp<CompFormChange>();
                if (comp != null)
                {
                    comp.cooldownTick();
                }
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002120 File Offset: 0x00000320
        public static IEnumerable<Gizmo> GetExtraEquipmentGizmosPassThrough(IEnumerable<Gizmo> values, Pawn_EquipmentTracker __instance)
        {
            foreach (Gizmo giz in values)
            {
                yield return giz;
            }
            bool flag = __instance.pawn.IsColonistPlayerControlled && PawnAttackGizmoUtility.CanShowEquipmentGizmos();
            if (flag)
            {
                List<ThingWithComps> list = __instance.AllEquipmentListForReading;
                int num;
                for (int i = 0; i < list.Count; i = num + 1)
                {
                    ThingWithComps eq = list[i];
                    CompFormChange twg = ThingCompUtility.TryGetComp<CompFormChange>(eq);
                    bool flag2 = twg != null;
                    if (flag2)
                    {
                        foreach (Gizmo giz2 in twg.heldGizmos(__instance.pawn))
                        {
                            yield return giz2;
                        }
                    }
                    eq = null;
                    twg = null;
                    num = i;
                }
                list = null;
            }
            yield break;
        }

        public static void IFCPostTickApparel(Pawn_ApparelTracker __instance)
        {
            List<Apparel> wornApparel = __instance.WornApparel;
            for (int i = 0; i < wornApparel.Count; i++)
            {
                CompFormChange comp = wornApparel[i].GetComp<CompFormChange>();
                if (comp != null)
                {
                    comp.cooldownTick();
                }
            }
        }

        public static IEnumerable<Gizmo> GetExtraApparelGizmosPassThrough(IEnumerable<Gizmo> values, Pawn_ApparelTracker __instance)
        {
            foreach (Gizmo giz in values)
            {
                yield return giz;
            }
            bool flag = __instance.pawn.IsColonistPlayerControlled && PawnAttackGizmoUtility.CanShowEquipmentGizmos();
            if (flag)
            {
                List<Apparel> list = __instance.WornApparel;
                int num;
                for (int i = 0; i < list.Count; i = num + 1)
                {
                    ThingWithComps eq = list[i];
                    CompFormChange twg = ThingCompUtility.TryGetComp<CompFormChange>(eq);
                    bool flag2 = twg != null;
                    if (flag2)
                    {
                        foreach (Gizmo giz2 in twg.heldGizmos(__instance.pawn))
                        {
                            yield return giz2;
                        }
                    }
                    eq = null;
                    twg = null;
                    num = i;
                }
                list = null;
            }
            yield break;
        }
    }
}
