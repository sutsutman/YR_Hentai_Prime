using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_TransferVictim : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.ThingHolder);

        //public override bool ShouldSkip(Pawn pawn, bool forced = false)
        //{
        //    return !ModsConfig.
        //    lyActive;
        //}

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            Pawn victim = GetVictim(t);
            if (victim == null)
            {
                return false;
            }

            CompAnimationBedTarget CompAnimationBedTarget = victim.TryGetComp<CompAnimationBedTarget>();
            if (CompAnimationBedTarget == null)
            {
                return false;
            }

            if (CompAnimationBedTarget.targetHolder == null)
            {
                return false;
            }

            if (!pawn.CanReserve(CompAnimationBedTarget.targetHolder, 1, -1, null, forced) || !pawn.CanReserve(victim, 1, -1, null, forced))
            {
                return false;
            }

            if (CompAnimationBedTarget.AnimationBed.IsForbidden(pawn) || CompAnimationBedTarget.targetHolder.IsForbidden(pawn))
            {
                return false;
            }

            return CompAnimationBedTarget.AnimationBed != CompAnimationBedTarget.targetHolder;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn victim = GetVictim(t);
            if (victim == null)
            {
                return null;
            }

            CompAnimationBedTarget CompAnimationBedTarget = victim.TryGetComp<CompAnimationBedTarget>();
            if (CompAnimationBedTarget == null)
            {
                return null;
            }

            return JobMaker.MakeJob(YR_H_P_DefOf.YR_TransferBetweenAnimationBeds, t, CompAnimationBedTarget.targetHolder, victim).WithCount(1);
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

                CompAnimationBedTarget CompAnimationBedTarget = heldPawn.TryGetComp<CompAnimationBedTarget>();
                if (CompAnimationBedTarget != null && CompAnimationBedTarget.AnimationBed == CompAnimationBedTarget.targetHolder)
                {
                    return null;
                }

                return heldPawn;
            }

            return null;
        }
    }
}
