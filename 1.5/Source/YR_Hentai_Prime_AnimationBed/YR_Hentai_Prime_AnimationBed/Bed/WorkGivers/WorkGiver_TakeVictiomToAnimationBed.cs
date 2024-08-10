using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_TakeVictiomToAnimationBed : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompAnimationBedTarget compAnimationBedTarget = t.TryGetComp<CompAnimationBedTarget>();
            if (compAnimationBedTarget?.targetHolder == null || compAnimationBedTarget.targetHolder.Destroyed || compAnimationBedTarget.targetHolder.MapHeld != t.MapHeld || compAnimationBedTarget.CompAnimationBed.HeldPawn != null)
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(compAnimationBedTarget.targetHolder, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
            {
                return false;
            }

            if (t is Pawn pawn2 && pawn2.Faction != Faction.OfPlayer && !pawn2.ThreatDisabled(pawn))
            {
                return false;
            }

            if (pawn == t)
            {
                return false;

            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompAnimationBedTarget compAnimationBedTarget = t.TryGetComp<CompAnimationBedTarget>();
            if (compAnimationBedTarget == null)
            {
                return null;
            }

            Job job = JobMaker.MakeJob(YR_H_P_DefOf.YR_CarryToAnimationBed, compAnimationBedTarget.targetHolder, t);
            job.count = 1;
            return job;
        }
    }
}
