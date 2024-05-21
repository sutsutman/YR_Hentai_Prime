using RimWorld;
using System.Collections.Generic;
using System.Linq;
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

        bool start = true;
        protected override void WatchTickAction()
        {
            pawn.rotationTracker.FaceCell(base.TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, Building_AnimationBed);

            if (stop)
            {
                return;
            }

            CompProperties_SpawnDummyForJoy props = CompSpawnDummyForJoy?.Props;

            if (props != null)
            {
                Pawn heldPawn = Building_AnimationBed?.HeldPawn;
                if (pawn.Drawer.renderer.CurAnimation != props.animationDef && heldPawn != null)
                {
                    //여기에 뭔가 원인이 있음
                    pawn.Drawer.renderer.SetAnimation(props.animationDef);
                    Building_AnimationBed.dummyForJoyIsActive = true;
                    Building_AnimationBed.dummyForJoyPawn = pawn;
                    Building_AnimationBed.setAnimation = true;
                    CompAnimationSetting.needMakeGraphics = true;

                    if (start)
                    {
                        AddHediff(heldPawn, Building_AnimationBed, props.makeHediff.heldPawnHediffSetting.startHediffSettings);
                        AddHediff(pawn, Building_AnimationBed, props.makeHediff.joyPawnHediffSetting.startHediffSettings);

                        Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.startSoundSettings);
                        Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.startSoundSettings);

                        start = false;
                    }

                    Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.randomSoundSettings);
                    Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.randomSoundSettings);
                }
            }
        }


        private static void AddHediff(Pawn pawn, Building_AnimationBed building_AnimationBed, List<HediffSetting> hediffSettings)
        {
            foreach (var hediffSetting in hediffSettings)
            {
                var hediffDef = hediffSetting.hediffDef;

                foreach (var conditionHediffDef in hediffSetting.conditionHediffDefs)
                {
                    void action() => hediffDef = conditionHediffDef.hediffDef;

                    if (Condition.ExecuteActionIfConditionMatches(pawn, building_AnimationBed, conditionHediffDef.condition, action))
                    {
                        break;
                    }

                }
                pawn.health.AddHediff(hediffDef);
            }
        }

        protected void FinishAction()
        {
            pawn.Drawer.renderer.SetAnimation(null);
            Building_AnimationBed.dummyForJoyIsActive = false;
            Building_AnimationBed.dummyForJoyPawn = null;
            Building_AnimationBed.setAnimation = true;
            CompAnimationSetting.needMakeGraphics = true;


            Pawn heldPawn = Building_AnimationBed.HeldPawn;

            CompProperties_SpawnDummyForJoy props = CompSpawnDummyForJoy.Props;
            AddHediff(heldPawn, Building_AnimationBed, props.makeHediff.heldPawnHediffSetting.finishHediffSettings);
            AddHediff(pawn, Building_AnimationBed, props.makeHediff.joyPawnHediffSetting.finishHediffSettings);

            RemoveStartHediff(heldPawn, props.makeHediff.heldPawnHediffSetting.startHediffSettings);
            RemoveStartHediff(pawn, props.makeHediff.joyPawnHediffSetting.startHediffSettings);

            Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.finishSoundSettings);
            Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.finishSoundSettings);

            //이걸로 멈춰야 에러 안남
            stop = true;
        }

        private static void RemoveStartHediff(Pawn pawn, List<HediffSetting> hediffSettings)
        {
            var hediffsToRemove = pawn.health.hediffSet.hediffs
                .Where(hediff => hediffSettings
                    .Any(startHediffSetting => startHediffSetting.removeWhenFinish &&
                        (hediff.def == startHediffSetting.hediffDef ||
                        startHediffSetting.conditionHediffDefs.Any(conditionHediffDef => hediff.def == conditionHediffDef.hediffDef))))
                .ToList();

            foreach (var hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();


            Scribe_Values.Look(ref start, "start");
        }
    }
}
