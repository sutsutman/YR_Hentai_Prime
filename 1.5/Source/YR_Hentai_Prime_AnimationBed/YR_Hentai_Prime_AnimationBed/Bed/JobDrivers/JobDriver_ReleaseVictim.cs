using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JobDriver_ReleaseVictim : JobDriver
    {
        private const TargetIndex PlatformIndex = TargetIndex.A;

        private const TargetIndex EntityIndex = TargetIndex.B;

        private const int TransferTicks = 300;

        private Thing Platform => TargetThingA;

        private Pawn InnerPawn => (Platform as Building_AnimationBed)?.HeldPawn;

        private bool EntityShouldBeReleased
        {
            get
            {
                CompAnimationBedTarget CompAnimationBedTarget = InnerPawn?.TryGetComp<CompAnimationBedTarget>();
                if (CompAnimationBedTarget != null)
                {
                    if (CompAnimationBedTarget.containmentMode != EntityContainmentMode.Release)
                    {
                        return job.ignoreDesignations;
                    }

                    return true;
                }

                return false;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A).FailOn(() => !EntityShouldBeReleased);
            Toil toil = Toils_General.WaitWhileExtractingContents(TargetIndex.A, TargetIndex.B, 300);
            toil.PlaySustainerOrSound(SoundDefOf.ReleaseFromPlatform);
            yield return toil;
            yield return Toils_General.Do(delegate
            {
                if (TargetThingB is Pawn thing)
                {
                    CompAnimationBedTarget CompAnimationBedTarget = thing.TryGetComp<CompAnimationBedTarget>();
                    //CompAnimationBedTarget.Escape(initiator: false);
                    if (CompAnimationBedTarget != null)
                    {
                        CompAnimationBedTarget.containmentMode = EntityContainmentMode.MaintainOnly;
                    }
                }

                pawn.MentalState?.Notify_ReleasedTarget();
            });
        }
    }
}
