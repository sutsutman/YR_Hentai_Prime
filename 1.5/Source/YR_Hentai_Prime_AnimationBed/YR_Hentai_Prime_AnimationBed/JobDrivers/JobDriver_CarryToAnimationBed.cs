using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JobDriver_CarryToAnimationBed : JobDriver
    {
        private const TargetIndex DestHolderIndex = TargetIndex.A;

        private const TargetIndex TakeeIndex = TargetIndex.B;

        private const int EnterDelayTicks = 300;

        private Thing Takee => job.GetTarget(TargetIndex.B).Thing;

        private CompAnimationBed DestHolder => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompAnimationBed>();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(DestHolder.parent, job, 1, -1, null, errorOnFailed);
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !DestHolder.Available);
            this.FailOn(() => Takee is Pawn pawn2 && !(pawn2.GetComp<CompActivity>()?.IsDormant ?? true));
            this.FailOn(() => Takee.TryGetComp<CompAnimationBedTarget>().CompAnimationBed != DestHolder);
            if (pawn.carryTracker.CarriedThing != Takee)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            }

            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            foreach (Toil item in ChainTakeeToPlatformToils(pawn, Takee, DestHolder, TargetIndex.A))
            {
                yield return item;
            }

            yield return Toils_General.Do(delegate
            {
                if (Takee is Pawn pawn && pawn.RaceProps.Humanlike)
                {
                    TaleRecorder.RecordTale(TaleDefOf.Captured, base.pawn, pawn);
                }
            });
        }

        public static IEnumerable<Toil> ChainTakeeToPlatformToils(Pawn taker, Thing takee, CompAnimationBed platform, TargetIndex platformIndex)
        {
            yield return Toils_Goto.GotoThing(platformIndex, PathEndMode.ClosestTouch);
            Toil toil = Toils_General.WaitWith(platformIndex, 300, useProgressBar: true);
            toil.PlaySustainerOrSound(SoundDefOf.ChainToPlatform);
            yield return toil;
            yield return Toils_General.Do(delegate
            {
                Thing thing = takee;
                platform.Container.TryAddOrTransfer(thing, 1);
                thing.Rotation = Rot4.South;
                CompAnimationBedTarget compAnimationBedTarget = thing.TryGetComp<CompAnimationBedTarget>();
                if (compAnimationBedTarget != null)
                {
                    compAnimationBedTarget.Notify_HeldOnPlatform(platform.Container);

                    if (thing is Pawn pawn)
                    {
                        foreach (var hediffDef in platform.Props.addedHediffDefs)
                        {
                            pawn.health.AddHediff(hediffDef);
                        }
                    }
                    if (compAnimationBedTarget.Props.capturedLetterLabel != null)
                    {
                        Find.LetterStack.ReceiveLetter(compAnimationBedTarget.Props.capturedLetterLabel, compAnimationBedTarget.Props.capturedLetterText.Formatted(taker.Named("PAWN")), LetterDefOf.NeutralEvent, platform.parent);
                    }
                }
            });
        }
    }
}