using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_Milking : HediffCompProperties
    {
        public HediffCompProperties_Milking() => compClass = typeof(HediffComp_Milking);

        public List<HediffDef> requestHediffDefs = new List<HediffDef>();

        public int ticks = 1200;
        public float severity = 0.01f;
    }


    public class HediffComp_Milking : HediffComp
    {
        private HediffCompProperties_Milking Props => (HediffCompProperties_Milking)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn.IsHashIntervalTick(Props.ticks))
            {
                foreach (var _ in from pawnHediff in parent.pawn.health.hediffSet.hediffs
                                  from hediffDef in Props.requestHediffDefs
                                  where pawnHediff.def == hediffDef
                                  select new { })
                {
                    return;
                }

                parent.Severity -= Props.severity;
            }
        }
    }
}
