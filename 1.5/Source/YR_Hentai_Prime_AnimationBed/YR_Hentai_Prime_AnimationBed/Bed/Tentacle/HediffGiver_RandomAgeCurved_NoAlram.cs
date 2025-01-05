using RimWorld;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffGiver_RandomAgeCurved_NoAlram : HediffGiver
    {
        // Token: 0x0600002F RID: 47 RVA: 0x00002F38 File Offset: 0x00001138
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            float x = (float)pawn.ageTracker.AgeBiologicalYears / pawn.RaceProps.lifeExpectancy;
            if (Rand.MTBEventOccurs(this.ageFractionMtbDaysCurve.Evaluate(x), 60000f, 60f) && (this.minPlayerPopulation <= 0 || pawn.Faction != Faction.OfPlayer || PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Count<Pawn>() >= this.minPlayerPopulation))
            {
                TryApply(pawn, null);
            }
        }

        // Token: 0x04000010 RID: 16
        public SimpleCurve ageFractionMtbDaysCurve;

        // Token: 0x04000011 RID: 17
        public int minPlayerPopulation;
    }

}
