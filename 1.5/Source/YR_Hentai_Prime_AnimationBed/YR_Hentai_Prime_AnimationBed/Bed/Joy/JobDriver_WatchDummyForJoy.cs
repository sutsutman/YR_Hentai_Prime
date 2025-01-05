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
            if (TargetC.HasThing && TargetC.Thing is Building_Bed)
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

            // JoyUtility와 FinishAction 호출
            watch.AddFinishAction(delegate
            {
                JoyUtility.TryGainRecRoomThought(pawn);
                FinishAction();

                CompProperties_SpawnDummyForJoy props = CompSpawnDummyForJoy?.Props;
                Building_AnimationBed.PlaySoundSettings(Building_AnimationBed?.HeldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.waitAfterJoySoundSettingDefs);
                Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.waitAfterJoySoundSettingDefs);

                pawn.Drawer.renderer.SetAllGraphicsDirty();
            });

            watch.defaultCompleteMode = ToilCompleteMode.Delay;
            watch.defaultDuration = job.def.joyDuration;
            watch.handlingFacing = true;
            if (TargetA.Thing.def.building != null && TargetA.Thing.def.building.effectWatching != null)
            {
                watch.WithEffect(() => TargetA.Thing.def.building.effectWatching, EffectTargetGetter);
            }

            yield return watch;

            LocalTargetInfo EffectTargetGetter()
            {
                return TargetA.Thing.OccupiedRect().RandomCell + IntVec3.North.RotatedBy(TargetA.Thing.Rotation);
            }
        }


        public CompDummyForJoy CompDummyForJoy => TargetThingA?.TryGetComp<CompDummyForJoy>();

        public Building_AnimationBed Building_AnimationBed => CompDummyForJoy?.building_AnimationBed;
        public CompSpawnDummyForJoy CompSpawnDummyForJoy => Building_AnimationBed?.TryGetComp<CompSpawnDummyForJoy>();
        public CompAnimationSetting CompAnimationSetting => Building_AnimationBed?.TryGetComp<CompAnimationSetting>();

        bool stop = false;

        bool start = true;

        public bool watchNow = false;
        protected override void WatchTickAction()
        {
            pawn.rotationTracker.FaceCell(TargetA.Cell);
            pawn.GainComfortFromCellIfPossible();
            JoyUtility.JoyTickCheckEnd(pawn, JoyTickFullJoyAction.EndJob, 1f, Building_AnimationBed);

            if (stop)
            {
                return;
            }

            watchNow = true;

            CompProperties_SpawnDummyForJoy props = CompSpawnDummyForJoy?.Props;

            if (props != null)
            {
                Pawn heldPawn = Building_AnimationBed?.HeldPawn;

                var animationDef = props.animationDef;
                foreach (var conditionAnimationDef in props.conditionAnimationDefs)
                {
                    void action() => animationDef = conditionAnimationDef.animationDef;
                    if (Condition.ExecuteActionIfConditionMatches(Building_AnimationBed, conditionAnimationDef.pawnCondition, action))
                    {
                        break;
                    }
                }

                if (pawn.Drawer.renderer.CurAnimation != animationDef && heldPawn != null)
                {
                    //여기에 뭔가 원인이 있음
                    pawn.Drawer.renderer.SetAnimation(animationDef);
                    Building_AnimationBed.dummyForJoyIsActive = true;
                    Building_AnimationBed.dummyForJoyPawn = pawn;
                    //이전 폰 초기화
                    Building_AnimationBed.previousJoyPawn = pawn;
                    Building_AnimationBed.setAnimation = true;
                    CompAnimationSetting.needMakeGraphics = true;

                    if (start)
                    {
                        AddHediff(heldPawn, Building_AnimationBed, props.makeHediff.heldPawnHediffSetting.startHediffSettings);
                        AddHediff(pawn, Building_AnimationBed, props.makeHediff.joyPawnHediffSetting.startHediffSettings);

                        Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.startSoundSettingDefs);
                        Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.startSoundSettingDefs);

                        start = false;
                    }
                }

                if (heldPawn != null)
                {
                    Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.randomSoundSettingDefs);
                    Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.randomSoundSettingDefs);
                }


                foreach (var animationPart in pawn.Drawer.renderer.CurAnimation.animationParts)
                {
                    for (int i = 0; i < animationPart.Value.keyframes.Count; i++)
                    {
                        var currentKeyframe = animationPart.Value.keyframes[i];
                        var animationTick = pawn.Drawer.renderer.renderTree.AnimationTick;



                        if (tempKeyframe != currentKeyframe && currentKeyframe is BedAnimationKeyframe BAK)
                        {
                            // 마지막 키프레임인지 확인
                            if (i == animationPart.Value.keyframes.Count - 1)
                            {
                                // 마지막 키프레임 이후로는 해당 키프레임의 텍스처 유지
                                if (currentKeyframe.tick <= animationTick)
                                {
                                    if (!BAK.texPath.NullOrEmpty())
                                    {
                                        if (!graphicSeted)
                                        {
                                            pawn.Drawer.renderer.SetAllGraphicsDirty();
                                            graphicSeted = true;
                                        }
                                        else if (tempKeyframe.texPath != BAK.texPath)
                                        {
                                            pawn.Drawer.renderer.SetAllGraphicsDirty();
                                        }
                                        tempKeyframe = BAK;
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                var nextKeyframe = animationPart.Value.keyframes[i + 1];
                                // 현재 애니메이션 틱이 두 키프레임 사이에 있을 때 텍스처 변경
                                if (currentKeyframe.tick <= animationTick && animationTick < nextKeyframe.tick)
                                {
                                    if (!BAK.texPath.NullOrEmpty())
                                    {
                                        if (!graphicSeted)
                                        {
                                            pawn.Drawer.renderer.SetAllGraphicsDirty();
                                            graphicSeted = true;
                                        }
                                        else if (tempKeyframe.texPath != BAK.texPath)
                                        {
                                            pawn.Drawer.renderer.SetAllGraphicsDirty();
                                        }
                                        tempKeyframe = BAK;
                                    }
                                    break; // 텍스처가 설정되었으므로 루프 중단
                                }
                            }
                        }
                    }
                }
            }
        }
        bool graphicSeted = false;
        BedAnimationKeyframe tempKeyframe;

        private static void AddHediff(Pawn pawn, Building_AnimationBed building_AnimationBed, List<HediffSetting> hediffSettings)
        {
            foreach (var hediffSetting in hediffSettings)
            {
                var hediffDef = hediffSetting.hediffDef;

                foreach (var conditionHediffDef in hediffSetting.conditionHediffDefs)
                {
                    void action() => hediffDef = conditionHediffDef.hediffDef;

                    if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionHediffDef.pawnCondition, action))
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

            Building_AnimationBed.PlaySoundSettings(heldPawn, Building_AnimationBed, props.makeSound.heldPawnSound.finishSoundSettingDefs);
            Building_AnimationBed.PlaySoundSettings(pawn, Building_AnimationBed, props.makeSound.joyPawnSound.finishSoundSettingDefs);

            //이걸로 멈춰야 에러 안남
            stop = true;

            watchNow = false;
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
