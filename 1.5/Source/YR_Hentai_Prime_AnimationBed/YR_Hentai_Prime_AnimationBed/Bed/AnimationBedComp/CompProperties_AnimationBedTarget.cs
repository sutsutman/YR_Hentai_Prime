using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AnimationBedTarget : CompProperties
    {
        public PawnKindDef heldPawnKind;

        [MustTranslate] public string capturedLetterLabel;
        [MustTranslate] public string capturedLetterText;

        public float baseEscapeIntervalMtbDays = 60f;
        public bool lookForTargetOnEscape = true;
        public bool canBeExecuted = true;
        public bool getsColdContainmentBonus = false;
        public bool hasAnimation = true;

        public CompProperties_AnimationBedTarget() => compClass = typeof(CompAnimationBedTarget);
    }

    [StaticConstructorOnStartup]
    public class CompAnimationBedTarget : ThingComp
    {
        // 속성 접근자
        public CompProperties_AnimationBedTarget Props => (CompProperties_AnimationBedTarget)props;
        public CompStudiable CompStudiable => compStudiable ??= parent.GetComp<CompStudiable>();
        public CompAnimationBed CompAnimationBed => targetHolder?.TryGetComp<CompAnimationBed>();
        public Building_AnimationBed AnimationBed => parent.ParentHolder as Building_AnimationBed;

        // 필드 및 캐시된 텍스처
        private static readonly CachedTexture CaptureIcon = new CachedTexture("UI/Commands/CaptureVictim");
        private static readonly CachedTexture SelfTieIcon = new CachedTexture("UI/Commands/SelfTie");
        private static readonly CachedTexture TransferIcon = new CachedTexture("UI/Commands/TransferVictim");
        private static readonly Texture2D CancelTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        public Thing targetHolder;
        public bool isEscaping;
        public EntityContainmentMode containmentMode;

        [Unsaved(false)] private CompStudiable compStudiable;

        // 캡처 가능한지 여부
        public bool CanBeCaptured =>
            parent is Pawn pawn && pawn.RaceProps.Humanlike;

        // 현재 플랫폼에 있는지 확인
        public bool CurrentlyHeldOnPlatform => AnimationBed != null && parent.SpawnedOrAnyParentSpawned;

        // 플랫폼에서 학습 가능한지 여부
        public bool StudiedAtHoldingPlatform
        {
            get
            {

                if (parent is Pawn pawn)
                {
                    if (!pawn.RaceProps.Humanlike)
                    {
                        return false;
                    }

                    return true;
                }

                return true;
            }
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (targetHolder != null && parent.Map != targetHolder.Map)
            {
                targetHolder = null;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent is Pawn pawn)
            {
                if (targetHolder != null && (targetHolder.Destroyed || CompAnimationBed?.HeldPawn != null))
                {
                    targetHolder = null;
                }

                if (isEscaping && pawn.mindState.enemyTarget == null)
                {
                    isEscaping = false;
                }
            }
        }

        public void Notify_HeldOnPlatform(ThingOwner newOwner)
        {
            targetHolder = null;
            if (newOwner != null && Props.heldPawnKind != null)
            {
                var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Props.heldPawnKind, Faction.OfEntities));
                newOwner.TryAdd(pawn);
                pawn.TryGetComp<CompAnimationBedTarget>()?.Notify_HeldOnPlatform(newOwner);
                if (Props.heldPawnKind == PawnKindDefOf.Revenant)
                {
                    HandleRevenantSpecifics(pawn);
                }

                Find.HiddenItemsManager.SetDiscovered(pawn.def);
                parent.Destroy();
            }

            NotifyHoldingPlatformUpdate(newOwner);
        }

        private void HandleRevenantSpecifics(Pawn pawn)
        {
            var compBiosignatureOwner = parent.TryGetComp<CompBiosignatureOwner>();
            if (compBiosignatureOwner != null)
            {
                pawn.TryGetComp<CompRevenant>().biosignature = compBiosignatureOwner.biosignature;
            }

            if (pawn.TryGetComp<CompStudiable>(out var comp))
            {
                comp.lastStudiedTick = Find.TickManager.TicksGame;
            }
        }

        private void NotifyHoldingPlatformUpdate(ThingOwner newOwner)
        {
            if (AnimationBed == null) return;

            foreach (var comp in AnimationBed.AllComps)
            {
                if (comp is CompBaseOfAnimationBed baseComp)
                {
                    baseComp.Notify_HeldOnPlatform(newOwner);
                }
            }
        }

        public void Notify_ReleasedFromPlatform()
        {
            foreach (var comp in AnimationBed.AllComps)
            {
                if (comp is CompBaseOfAnimationBed baseComp)
                {
                    baseComp.Notify_ReleasedFromPlatform();
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Gizmo 생성
            if (StudiedAtHoldingPlatform && !CurrentlyHeldOnPlatform && CanBeCaptured)
            {
                yield return CreateCaptureCommand();
                yield return CreateSelfTieCommand();
            }

            if (CurrentlyHeldOnPlatform)
            {
                yield return CreateTransferCommand();
                yield return CreateReleaseCommand();
            }

            if (DebugSettings.ShowDevGizmos && CurrentlyHeldOnPlatform)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Kill",
                    action = () => parent.Kill(),
                    Order = 10005,
                };
            }
        }

        private Command_Action CreateCaptureCommand()
        {
            return new Command_Action
            {
                defaultLabel = "YR_CaptureEntity".Translate() + "...",
                defaultDesc = "YR_CaptureEntityDesc".Translate(parent).Resolve(),
                icon = CaptureIcon.Texture,
                action = () => YR_StudyUtility.TargetHoldingPlatformForVictim(null, parent),
                activateSound = SoundDefOf.Click,
                Disabled = !YR_StudyUtility.HoldingPlatformAvailableOnCurrentMap(),
                disabledReason = "NoHoldingPlatformsAvailable".Translate(),
                Order = 10001,
            };
        }
        private Command_Action CreateSelfTieCommand()
        {
            return new Command_Action
            {
                defaultLabel = "YR_SelfTieToPlatform".Translate() + "...",
                defaultDesc = "YR_SelfTieToPlatformDesc".Translate(parent).Resolve(),
                icon = SelfTieIcon.Texture,
                action = () => YR_StudyUtility.TargetHoldingPlatformForVictim((Pawn)parent, parent),
                activateSound = SoundDefOf.Click,
                Disabled = !YR_StudyUtility.HoldingPlatformAvailableOnCurrentMap(),
                disabledReason = "NoHoldingPlatformsAvailable".Translate(),
                Order = 10002,
            };
        }


        private Command_Action CreateTransferCommand()
        {
            return new Command_Action
            {
                defaultLabel = "YR_TransferEntity".Translate(parent) + "...",
                defaultDesc = "YR_TransferEntityDesc".Translate(parent).Resolve(),
                icon = TransferIcon.Texture,
                action = () => YR_StudyUtility.TargetHoldingPlatformForVictim(null, parent, true, AnimationBed),
                activateSound = SoundDefOf.Click,
                Order = 10003,
            };
        }

        private Command_Action CreateReleaseCommand()
        {
            return new Command_Action
            {
                defaultLabel = "YR_ReleaseVictim".Translate(parent) + "...",
                defaultDesc = "YR_ReleaseVictimDesc".Translate(parent).Resolve(),
                icon = TransferIcon.Texture,
                action = () => AnimationBed.EjectContents(),
                activateSound = SoundDefOf.Click,
                Order = 10004,
            };
        }

        public override string CompInspectStringExtra()
        {
            string text = base.CompInspectStringExtra();
            if (CanBeCaptured && ContainmentUtility.ShowContainmentStats(parent))
            {
                float minContainment = parent.GetStatValue(StatDefOf.MinimumContainmentStrength);
                if (Props.heldPawnKind != null)
                {
                    minContainment = Props.heldPawnKind.race.GetStatValueAbstract(StatDefOf.MinimumContainmentStrength);
                }

                text = $"{text}\nCapturable: {StatDefOf.MinimumContainmentStrength.LabelCap}: {minContainment:F1}";
            }

            return text;
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref targetHolder, "YR_TargetHolder");
            Scribe_Values.Look(ref isEscaping, "YR_IsEscaping", false);
            Scribe_Values.Look(ref containmentMode, "YR_ContainmentMode", EntityContainmentMode.MaintainOnly);
        }
    }
}
