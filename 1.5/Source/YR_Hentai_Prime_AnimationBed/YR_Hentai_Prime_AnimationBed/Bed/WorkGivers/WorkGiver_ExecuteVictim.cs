using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_ExecuteVictim : WorkGiver_VictimOnAnimationBed
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            return GetVictim(t) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(YR_H_P_DefOf.YR_ExecuteVictim, t);
        }

        protected override Pawn GetVictim(Thing potentialPlatform)
        {
            if (potentialPlatform is Building_AnimationBed Building_AnimationBed)
            {
                Pawn heldPawn = Building_AnimationBed.HeldPawn;
                CompAnimationBedTarget CompAnimationBedTarget = heldPawn?.TryGetComp<CompAnimationBedTarget>();
                if (CompAnimationBedTarget == null || CompAnimationBedTarget.containmentMode != EntityContainmentMode.Execute)
                {
                    return null;
                }

                return heldPawn;
            }

            return null;
        }
    }
}
