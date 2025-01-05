using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffGiver_Random : HediffGiver
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (Rand.MTBEventOccurs(mtbDays, 60000f, 60f) && base.TryApply(pawn, null))
            {
                AddAnty(pawn, cause);
            }
        }

        protected void AddAnty(Pawn pawn, Hediff cause)
        {
        }

        public float mtbDays;
    }
}