using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompUseEffect_EggVibrator : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            Thing thing = parent;
            usedBy.apparel.Wear((Apparel)thing);
            if (PawnUtility.ShouldSendNotificationAbout(usedBy))
            {
                Messages.Message("CompUseEffect_EggVibrator_String".Translate(usedBy, thing.Label), usedBy, MessageTypeDefOf.PositiveEvent, true);
            }
        }
    }
}
