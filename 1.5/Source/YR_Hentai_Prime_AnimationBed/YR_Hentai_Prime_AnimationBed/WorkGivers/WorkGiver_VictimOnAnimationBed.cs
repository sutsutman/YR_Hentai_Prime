using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public abstract class WorkGiver_VictimOnAnimationBed : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.ThingHolder);

        //public override bool ShouldSkip(Pawn pawn, bool forced = false)
        //{
        //    return !ModsConfig.AnomalyActive;
        //}

        public override string PostProcessedGerund(Job job)
        {
            Pawn victim = GetVictim(job.targetA.Thing);


            var st = "DoWorkAtThing".Translate(def.gerund.Named("GERUND"), victim.LabelShort.Named("TARGETLABEL"));

            return st;
        }

        protected abstract Pawn GetVictim(Thing potentialPlatform);
    }
}
