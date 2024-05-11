using RimWorld;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_TendVictim : WorkGiver_VictimOnAnimationBed
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            Pawn victim = GetVictim(t);
            if (victim != null)
            {
                return HealthAIUtility.ShouldBeTendedNowByPlayer(victim);
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn victim = GetVictim(t);
            if (victim == null)
            {
                return null;
            }

            Thing thing = HealthAIUtility.FindBestMedicine(pawn, victim);
            JobDef tendEntity = YR_H_P_DefOf.YR_TendVictim;
            LocalTargetInfo targetA = t;

            return JobMaker.MakeJob(tendEntity, targetA, (thing != null) ? ((LocalTargetInfo)thing) : LocalTargetInfo.Invalid, pawn);
        }

        protected override Pawn GetVictim(Thing potentialPlatform)
        {
            return GetTendableVictimFromPotentialPlatform(potentialPlatform);
        }

        public static Pawn GetTendableVictimFromPotentialPlatform(Thing potentialPlatform)
        {
            if (potentialPlatform is Building_AnimationBed Building_AnimationBed)
            {
                Pawn heldPawn = Building_AnimationBed.HeldPawn;
                if (heldPawn == null)
                {
                    return null;
                }

                CompAnimationBedTarget CompAnimationBedTarget = heldPawn.TryGetComp<CompAnimationBedTarget>();
                if (CompAnimationBedTarget != null && (CompAnimationBedTarget.containmentMode == EntityContainmentMode.Release || CompAnimationBedTarget.containmentMode == EntityContainmentMode.Execute))
                {
                    return null;
                }

                return heldPawn;
            }

            return null;
        }
    }
}
