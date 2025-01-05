using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public class Harmony_PawnGraphicSet
    {
        [HarmonyPrefix]
        public static bool TryGetGraphicApparel_Prefix(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec, ref bool __result)
        {
            rec = new ApparelGraphicRecord(null, null);

            var comp = apparel.TryGetComp<Comp_ChangeApparelTextureByCondition>();

            if (comp != null)
            {
                rec = comp.apparelGraphicRecord;
                __result = true;
                return false;
            }
            __result = false;
            return true;
        }
    }
}
