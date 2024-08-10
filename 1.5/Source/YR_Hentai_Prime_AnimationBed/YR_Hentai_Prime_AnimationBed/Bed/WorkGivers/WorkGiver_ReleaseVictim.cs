using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_ReleaseVictim : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Undefined);

        //public override bool ShouldSkip(Pawn pawn, bool forced = false)
        //{
        //    return !ModsConfig.AnomalyActive;
        //}

        public override string PostProcessedGerund(Job job)
        {
            return "ReleasingEntity".Translate(def.gerund.Named("GERUND"), job.targetB.Label.Named("TARGETLABEL"));
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            if (GetVictim(t) == null)
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn victim = GetVictim(t);
            if (victim == null)
            {
                return null;
            }

            return JobMaker.MakeJob(YR_H_P_DefOf.YR_ReleaseVictim, t, victim).WithCount(1);
        }

        private Pawn GetVictim(Thing thing)
        {
            if (thing is Building_AnimationBed Building_AnimationBed)
            {
                Pawn heldPawn = Building_AnimationBed.HeldPawn;
                if (heldPawn == null)
                {
                    return null;
                }

                CompAnimationBedTarget compAnimationBedTarget = heldPawn.TryGetComp<CompAnimationBedTarget>();
                if (compAnimationBedTarget != null && compAnimationBedTarget.containmentMode != EntityContainmentMode.Release)
                {
                    return null;
                }

                return heldPawn;
            }

            return null;
        }
    }
}
