using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class IngestionOutcomeDoer_OffsetNeed_Race : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
        {
            if (pawn.needs == null)
            {
                return;
            }
            Need need = pawn.needs.TryGetNeed(this.need);
            if (need == null)
            {
                return;
            }

            float num = offset;
            if (raceOnly.race.Contains(pawn.def))
            {
                num = raceOnly.offset;
            }

            if (perIngested)
            {
                num *= ingested.stackCount;
            }
            need.CurLevel += num;
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            string str = (offset >= 0f) ? "+" : string.Empty;
            yield return new StatDrawEntry(StatCategoryDefOf.Drug, need.LabelCap, str + offset.ToStringPercent(), need.description, need.listPriority, null, null, false);
            yield break;
        }

        public NeedDef need;

        public float offset;

        public ChemicalDef toleranceChemical;

        public bool perIngested;

        public RaceOnly raceOnly = new RaceOnly();
    }

    public class RaceOnly
    {
        public List<ThingDef> race = new List<ThingDef>();
        public float offset;
    }
}