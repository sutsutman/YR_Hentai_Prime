using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JobDriver_WatchDummyForJoy : JobDriver_WatchBuilding
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(TargetIndex.A);
            Toil watch;
            if (base.TargetC.HasThing && base.TargetC.Thing is Building_Bed)
            {
                this.KeepLyingDown(TargetIndex.C);
                yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.C);
                yield return Toils_Bed.GotoBed(TargetIndex.C);
                watch = Toils_LayDown.LayDown(TargetIndex.C, hasBed: true, lookForOtherJobs: false);
                watch.AddFailCondition(() => !watch.actor.Awake());
            }
            else
            {
                yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
                watch = ToilMaker.MakeToil("MakeNewToils");
            }

            watch.AddPreTickAction(delegate
            {
                WatchTickAction();
            });
            watch.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
                FinishAction();
            });
            watch.defaultCompleteMode = ToilCompleteMode.Delay;
            watch.defaultDuration = job.def.joyDuration;
            watch.handlingFacing = true;
            if (base.TargetA.Thing.def.building != null && base.TargetA.Thing.def.building.effectWatching != null)
            {
                watch.WithEffect(() => base.TargetA.Thing.def.building.effectWatching, EffectTargetGetter);
            }

            yield return watch;
            LocalTargetInfo EffectTargetGetter()
            {
                return base.TargetA.Thing.OccupiedRect().RandomCell + IntVec3.North.RotatedBy(base.TargetA.Thing.Rotation);
            }
        }


        public CompDummyForJoy CompDummyForJoy => TargetThingA?.TryGetComp<CompDummyForJoy>();

        public Building_AnimationBed Building_AnimationBed => CompDummyForJoy?.building_AnimationBed;
        public CompSpawnDummyForJoy CompSpawnDummyForJoy => Building_AnimationBed?.TryGetComp<CompSpawnDummyForJoy>();
        public CompAnimationSetting CompAnimationSetting => Building_AnimationBed?.TryGetComp<CompAnimationSetting>();

        bool stop = false;
        protected override void WatchTickAction()
        {
            pawn.rotationTracker.FaceCell(base.TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, Building_AnimationBed);

            if (!stop && pawn.Drawer.renderer.CurAnimation != CompSpawnDummyForJoy?.Props.animationDef)
            {
                pawn.Drawer.renderer.SetAnimation(CompSpawnDummyForJoy?.Props.animationDef);
                Building_AnimationBed.dummyForJoyIsActive = true;
                Building_AnimationBed.setAnimation = true;
                CompAnimationSetting.needMakeGraphics = true;
            }
        }
        protected void FinishAction()
        {
            pawn.Drawer.renderer.SetAnimation(null);
            Building_AnimationBed.dummyForJoyIsActive = false;
            Building_AnimationBed.setAnimation = true;
            CompAnimationSetting.needMakeGraphics = true;

            //이걸로 멈춰야 에러 안남
            stop = true;
        }
    }
}
