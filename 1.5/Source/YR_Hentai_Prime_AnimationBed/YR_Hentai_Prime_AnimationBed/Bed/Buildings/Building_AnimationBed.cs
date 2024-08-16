using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class Building_AnimationBed : Building, IThingHolderWithDrawnPawn, IThingHolder, IRoofCollapseAlert, ISearchableContents
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        public ThingOwner innerContainer;

        private int lastDamaged;

        private CompAffectedByFacilities facilitiesComp;

        private CompAttachPoints attachPointsComp;

        private CompAnimationSetting animationSettingComp;

        private CompToggleHediff toggleHediffComp;

        private CompSpawnDummyForJoy spawnDummyForJoyComp;

        private int heldPawnStartTick = -1;

        private const float ChainsUntetheredYOffset = 0.05f;

        private const float ChainsTetheredYOffset = 9f / 65f;

        private const float LurchMTBTicks = 100f;

        private const float DamageMTBDays = 2f;

        private static readonly FloatRange Damage = new FloatRange(1f, 3f);

        private Dictionary<AttachPointType, Vector3> platformPoints;

        public float HeldPawnDrawPos_Y => DrawPos.y + 1f / 26f;

        public float HeldPawnBodyAngle => base.Rotation.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.LayingOnGroundFaceUp;

        public Rot4 HeldPawnRotation => base.Rotation;

        public Vector3 PawnDrawOffset
        {
            get
            {
                if (AnimationSettingComp == null)
                {
                    return new Vector3();
                }

                PawnAnimationSetting pawnAnimationSetting = AnimationSettingComp.Props.pawnAnimationSetting;
                var offset = pawnAnimationSetting.offset;

                foreach (var conditonPawnOffset in pawnAnimationSetting.conditonPawnOffsets)
                {
                    void action() => offset = conditonPawnOffset.offset;

                    if (Condition.ExecuteActionIfConditionMatches(this, conditonPawnOffset.pawnCondition, action))
                    {
                        break;
                    }
                }

                offset += testPawnOffset;

                return offset;
            }
        }

        public Pawn HeldPawn => innerContainer.FirstOrDefault((Thing x) => x is Pawn) as Pawn;

        public bool Occupied => HeldPawn != null;

        public float AnimationAlpha => Mathf.Clamp01((Find.TickManager.TicksGame - heldPawnStartTick) / 20f);

        private CompAffectedByFacilities FacilitiesComp => facilitiesComp ??= GetComp<CompAffectedByFacilities>();

        private CompAttachPoints AttachPointsComp => attachPointsComp ??= GetComp<CompAttachPoints>();

        public CompAnimationSetting AnimationSettingComp => animationSettingComp ??= GetComp<CompAnimationSetting>();

        public CompToggleHediff ToggleHediffComp => toggleHediffComp ??= GetComp<CompToggleHediff>();
        public CompSpawnDummyForJoy SpawnDummyForJoyComp => spawnDummyForJoyComp ??= GetComp<CompSpawnDummyForJoy>();

        public ThingOwner SearchableContents => innerContainer;

        public bool HasAttachedElectroharvester
        {
            get
            {
                foreach (Thing item in FacilitiesComp.LinkedFacilitiesListForReading)
                {
                    CompPowerPlantElectroharvester compPowerPlantElectroharvester = item.TryGetComp<CompPowerPlantElectroharvester>();
                    if (compPowerPlantElectroharvester != null && compPowerPlantElectroharvester.PowerOn)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private CompProperties_AnimationBed PlatformProps => GetComp<CompAnimationBed>().Props;

        public PawnDrawParms HeldPawnDrawParms
        {
            get
            {
                PawnDrawParms result = default;
                result.facing = HeldPawn.Rotation;
                result.rotDrawMode = RotDrawMode.Fresh;
                result.posture = HeldPawn.GetPosture();
                result.flags = PawnRenderFlags.Headgear | PawnRenderFlags.Clothes;
                result.tint = Color.white;
                result.pawn = HeldPawn;
                return result;
            }
        }

        public Building_AnimationBed() => innerContainer = new ThingOwner<Thing>(this);

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            EjectContents();
            platformPoints = null;
            base.DeSpawn(mode);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }


        public bool setAnimation = true;
        public bool openControllSetting;
        public int tempTick = num;
        public static int num = 1000;

        public bool makePortrait = true;

        public int tempAnimationTick = 0;
        private CompPowerTrader powerComp;
        public bool PowerOn => ((powerComp != null && powerComp.PowerOn) || powerComp == null) && !stopAnimation;

        public bool stopAnimation = false;

        public int tickForPortrait = 0;
        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();
            tickForPortrait--;

            if (HeldPawn != null)
            {
                makePortrait = true;

                if (PowerOn)
                {
                    BedAnimationUtility.SetAnimation(this);

                    if (tickForPortrait <= 0)
                    {
                        tickForPortrait = 1000;
                        BedAnimationUtility.MakePortrait(this);
                    }
                    tempAnimationTick = HeldPawn.Drawer.renderer.renderTree.AnimationTick;

                    if (HeldPawn.Drawer.renderer.HasAnimation)
                    {
                        ProcessAnimationSounds(HeldPawn.Drawer.renderer.CurAnimation.animationParts);
                    }
                    if (dummyForJoyPawn != null && dummyForJoyPawn.Drawer.renderer.HasAnimation)
                    {
                        ProcessAnimationSounds(dummyForJoyPawn.Drawer.renderer.CurAnimation.animationParts);
                    }
                }
            }

            UpdateHeldPawnStartTick();
        }

        private void ProcessAnimationSounds(Dictionary<PawnRenderNodeTagDef, AnimationPart> animationParts)
        {
            foreach (var animationPart in animationParts)
            {
                foreach (var keyframe in animationPart.Value.keyframes)
                {
                    if (keyframe is BedAnimationKeyframe BAK && HeldPawn.Drawer.renderer.renderTree.AnimationTick == BAK.tick)
                    {
                        PlaySoundSettings(HeldPawn, this, BAK.soundSettings);
                    }
                }
            }
        }

        public static void PlaySoundSettings(Pawn pawn, Building_AnimationBed building_AnimationBed, List<SoundSetting> soundSettings)
        {
            foreach (var soundSetting in soundSettings)
            {
                GetSoundDefAndProbability(pawn, building_AnimationBed, soundSetting, out var soundDef, out var probability);
                var rand = Rand.Range(0, 1f);
                //Log.Error(rand.ToString("F10"));
                if (probability >= rand)
                {
                    soundDef.PlayOneShot(building_AnimationBed);
                }
            }
        }

        private static void GetSoundDefAndProbability(Pawn pawn, Building_AnimationBed building_AnimationBed, SoundSetting soundSetting, out SoundDef soundDef, out float probability)
        {
            var tempSoundDef = soundSetting.soundDefs.RandomElement();
            var tempProbability = soundSetting.probability;

            foreach (var conditionSoundSetting in soundSetting.conditionSoundSettings)
            {
                void action()
                {
                    tempSoundDef = conditionSoundSetting.soundDefs.RandomElement();
                    tempProbability = conditionSoundSetting.probability;
                }

                if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionSoundSetting.pawnCondition, action))
                {
                    break;
                }

            }
            soundDef = tempSoundDef;
            probability = tempProbability;
        }

        private void UpdateHeldPawnStartTick()
        {
            if (heldPawnStartTick == -1 && HeldPawn != null)
            {
                heldPawnStartTick = Find.TickManager.TicksGame;
            }
            else if (HeldPawn == null)
            {
                heldPawnStartTick = -1;
            }
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            Pawn heldPawn = HeldPawn;
            if (heldPawn != null && AnimationSettingComp != null)
            {
                Rot4 rotation = AnimationSettingComp.Props.pawnAnimationSetting.rotation;

                foreach (var conditionPawnRotation in AnimationSettingComp.Props.pawnAnimationSetting.conditionPawnRotations)
                {
                    void action() => rotation = conditionPawnRotation.rotation;

                    if (Condition.ExecuteActionIfConditionMatches(this, conditionPawnRotation.pawnCondition, action))
                    {
                        break;
                    }
                }

                var pos = DrawPos + PawnDrawOffset;
                //Log.Error(heldPawn.LabelShort + " : " + pos.ToString("F10"));
                heldPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, pos, rotation, neverAimWeapon: true);
            }
            else
            {
                heldPawn?.Drawer.renderer.DynamicDrawPhaseAt(phase, DrawPos + PawnDrawOffset, Rot4.South, neverAimWeapon: true);
            }
        }

        public void RenderPawnHeedAt(Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            if (GlobalTextureAtlasManager.TryGetPawnFrameSet(HeldPawn, out var frameSet, out var _))
            {
                using (new ProfilerBlock("Draw Cached Mesh"))
                {
                    Rot4 facing = Rot4.South;
                    Vector3 bodyPos = DrawPos;
                    bodyPos.y += 0.5f;
                    float bodyAngle = 0;
                    PawnDrawMode drawMode = PawnDrawMode.HeadOnly;
                    Material mat = OverrideMaterialIfNeeded(MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout)), PawnRenderFlags.None);
                    GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, facing, drawMode), bodyPos, Quaternion.AngleAxis(bodyAngle, Vector3.up), mat, drawNow: false);
                    Vector3 drawPos = bodyPos.WithYOffset(PawnRenderUtility.AltitudeForLayer((facing == Rot4.North) ? (-10f) : 80f));
                    //PawnRenderUtility.DrawEquipmentAndApparelExtras(HeldPawn, drawPos, facing, results.parms.flags);
                }
            }
            else
            {
                Log.ErrorOnce($"Attempted to use a cached frame set for pawn {HeldPawn.Name} but failed to get one.", Gen.HashCombine(HeldPawn.GetHashCode(), 5938111));
            }
        }
        private Material OverrideMaterialIfNeeded(Material original, PawnRenderFlags flags)
        {
            if (flags.FlagSet(PawnRenderFlags.Cache) || flags.FlagSet(PawnRenderFlags.Portrait))
            {
                return original;
            }
            if (HeldPawn.IsPsychologicallyInvisible())
            {
                return InvisibilityMatPool.GetInvisibleMat(original);
            }
            return HeldPawn.Drawer.renderer.flasher.GetDamagedMat(original);
        }
        private Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode)
        {
            int index = frameSet.GetIndex(rotation, drawMode);
            if (frameSet.isDirty[index])
            {
                Find.PawnCacheCamera.rect = frameSet.uvRects[index];
                Find.PawnCacheRenderer.RenderPawn(HeldPawn, frameSet.atlas, Vector3.zero, 1f, 0f, rotation, true, true, true, false, default, null, null, false);
                Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
                frameSet.isDirty[index] = false;
            }
            return frameSet.meshes[index];
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
        }

        public void EjectContents()
        {
            setAnimation = true;
            if (animationSettingComp != null)
            {
                animationSettingComp.needMakeGraphics = true;
            }

            if (HeldPawn != null)
            {
                HeldPawn?.Drawer.renderer.SetAnimation(null);
                HeldPawn?.GetComp<CompAnimationBedTarget>()?.Notify_ReleasedFromPlatform();
                ToggleHediffComp?.RemoveAllHediffs(HeldPawn);
                RemoveAnimationBedHediff();
            }

            innerContainer?.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
        }

        private void RemoveAnimationBedHediff()
        {
            List<HediffDef> addedHediffDefs = PlatformProps.addedHediffDefs;
            if (HeldPawn == null || addedHediffDefs.NullOrEmpty())
            {
                return;
            }

            var hediffsToRemove = new List<Hediff>();

            // Find all matching Hediffs
            foreach (var hediff in HeldPawn.health.hediffSet.hediffs)
            {
                if (addedHediffDefs.Contains(hediff.def))
                {
                    hediffsToRemove.Add(hediff);
                }
            }

            // Remove all matching Hediffs
            foreach (var hediff in hediffsToRemove)
            {
                HeldPawn.health.RemoveHediff(hediff);
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }

            if (!Occupied)
            {
                yield break;
            }

            foreach (FloatMenuOption floatMenuOption2 in HeldPawn.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption2;
            }
        }

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            foreach (InspectTabBase inspectTab in base.GetInspectTabs())
            {
                yield return inspectTab;
            }

            if (HeldPawn != null && HeldPawn.def.inspectorTabs.Contains(typeof(ITab_StudyNotes)))
            {
                yield return HeldPawn.GetInspectTabs().FirstOrDefault((InspectTabBase tab) => tab is ITab_StudyNotes);
            }
        }

        //기즈모
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo2 in base.GetGizmos())
            {
                yield return gizmo2;
            }

            if (HeldPawn != null && HeldPawn.TryGetComp<CompActivity>(out var comp))
            {
                foreach (Gizmo item in comp.CompGetGizmosExtra())
                {
                    yield return item;
                }
            }

            if (HeldPawn != null && HeldPawn.TryGetComp<CompStudiable>(out var comp2))
            {
                foreach (Gizmo item2 in comp2.CompGetGizmosExtra())
                {
                    yield return item2;
                }
            }

            if (HeldPawn != null && HeldPawn.TryGetComp<CompAnimationBedTarget>(out var comp3))
            {
                foreach (Gizmo item3 in comp3.CompGetGizmosExtra())
                {
                    yield return item3;
                }
            }

            foreach (Thing item4 in innerContainer)
            {
                Gizmo gizmo;
                if ((gizmo = Building.SelectContainedItemGizmo(this, item4)) != null)
                {
                    yield return gizmo;
                }
            }

            if (HeldPawn == null)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "YR_StopAnimation".Translate(),
                icon = stopAnimation ? ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Normal") : ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Pause"),
                action = delegate
                {
                    if (stopAnimation)
                    {
                        stopAnimation = false;
                    }
                    else
                    {
                        stopAnimation = true;
                    }
                }
            };

            //테스트용 기즈모

            foreach (var testGizmo in TESTGIZMO())
            {
                yield return testGizmo;
            }

        }

        //테스트용 기즈모
        private IEnumerable<Gizmo> TESTGIZMO()
        {

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Open controll setting",
                    icon = ContentFinder<Texture2D>.Get("UI/Heroart/RimWorldLogo"),
                    action = ToggleOpenControlSetting
                };

                if (openControllSetting)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Micro Control : " + movef.ToString(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Micro"),
                        action = ToggleMovef
                    };

                    var defLabelCount = new Dictionary<BedAnimationDef, int>();

                    //폰 위치 쪽
                    if (HeldPawn != null)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = $"Pawn Offset Setting",
                            icon = ContentFinder<Texture2D>.Get("UI/Commands/ForColonists"),
                            action = delegate
                            {
                                if (openPawnOffsetSettingGizmo)
                                {
                                    openPawnOffsetSettingGizmo = false;
                                }
                                else
                                {
                                    openPawnOffsetSettingGizmo = true;
                                }
                            }
                        };

                        if (openPawnOffsetSettingGizmo)
                        {
                            //Reset
                            yield return new Command_Action
                            {
                                defaultLabel = $"Pawn Offset Reset",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset"),
                                action = delegate
                                {
                                    testPawnOffset = new Vector3();
                                }
                            };
                            // X
                            yield return new Command_Action
                            {
                                defaultLabel = $"Pawn Offset : Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    float n = 0;
                                    testPawnOffset.x += movef;
                                    n = testPawnOffset.x;
                                    Messages.Message($"testOffset.x : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"Pawn Offset : Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    float n = 0;
                                    testPawnOffset.x -= movef;
                                    n = testPawnOffset.x;
                                    Messages.Message($"testOffset.x : {n}", MessageTypeDefOf.SilentInput, false);

                                }
                            };

                            // Z
                            yield return new Command_Action
                            {
                                defaultLabel = $"Pawn Offset : Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    float n = 0;
                                    testPawnOffset.z += movef;
                                    n = testPawnOffset.z;
                                    Messages.Message($"testOffset.y : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"Pawn Offset : Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    float n = 0;
                                    testPawnOffset.z -= movef;
                                    n = testPawnOffset.z;
                                    Messages.Message($"testOffset.y : {n}", MessageTypeDefOf.SilentInput, false);

                                }
                            };
                        }
                    }
                    //애니메이션 쪽
                    foreach (var bedAnimationSettingAndTick in AnimationSettingComp.bedAnimationSettingAndTicks)
                    {
                        var parentDef = bedAnimationSettingAndTick.parentBedAnimationDef;

                        if (parentDef == YR_H_P_DefOf.YR_Dummy_BedAnimation)
                        {
                            continue;
                        }

                        List<BedAnimationSetting> bedAnimationSettings = bedAnimationSettingAndTick.bedAnimationSettings;

                        Texture2D openGizmoIcon = ContentFinder<Texture2D>.Get("UI/YR_Dummy");
                        if (bedAnimationSettings[0]?.graphicData?.texPath != null)
                        {
                            openGizmoIcon = ContentFinder<Texture2D>.Get(bedAnimationSettings[0].graphicData.texPath);
                        }

                        if (!defLabelCount.ContainsKey(parentDef))
                        {
                            defLabelCount[parentDef] = 0;
                        }
                        else
                        {
                            defLabelCount[parentDef]++;
                        }

                        string labelSuffix = defLabelCount[parentDef] > 0 ? $" [{defLabelCount[parentDef]}]" : string.Empty;
                        string label = (bedAnimationSettingAndTick.parentBedAnimationDef.label ?? bedAnimationSettingAndTick.parentBedAnimationDef.defName) + labelSuffix;


                        yield return new Command_Action
                        {
                            defaultLabel = $"{label} : " + "OpenGizmo",
                            icon = openGizmoIcon,
                            action = delegate
                            {
                                if (bedAnimationSettingAndTick.openTestGizmo)
                                {
                                    bedAnimationSettingAndTick.openTestGizmo = false;
                                }
                                else
                                {
                                    bedAnimationSettingAndTick.openTestGizmo = true;
                                }
                            }
                        };



                        if (bedAnimationSettingAndTick.openTestGizmo)
                        {
                            //Reset
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Reset",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset"),
                                action = delegate
                                {
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset = Vector3.zero;

                                        if (bedAnimationSetting.testDrawSize.x >= 0)
                                        {
                                            bedAnimationSetting.graphic.drawSize.x -= bedAnimationSetting.testDrawSize.x;
                                        }
                                        else
                                        {
                                            bedAnimationSetting.graphic.drawSize.x += bedAnimationSetting.testDrawSize.x;
                                        }

                                        if (bedAnimationSetting.testDrawSize.y >= 0)
                                        {
                                            bedAnimationSetting.graphic.drawSize.y -= bedAnimationSetting.testDrawSize.y;
                                        }
                                        else
                                        {
                                            bedAnimationSetting.graphic.drawSize.y += bedAnimationSetting.testDrawSize.y;
                                        }

                                        bedAnimationSetting.testDrawSize = Vector2.zero;

                                    }
                                }
                            };

                            // X
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.x += movef;
                                        n = bedAnimationSetting.testOffset.x;
                                    }
                                    Messages.Message($"testOffset.x : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.x -= movef;
                                        n = bedAnimationSetting.testOffset.x;
                                    }
                                    Messages.Message($"testOffset.x : {n}", MessageTypeDefOf.SilentInput, false);

                                }
                            };

                            // Y
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Front",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.y += movef;
                                        n = bedAnimationSetting.testOffset.y;
                                    }
                                    Messages.Message($"testOffset.y : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Back",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.y -= movef;
                                        n = bedAnimationSetting.testOffset.y;
                                    }
                                    Messages.Message($"testOffset.y : {n}", MessageTypeDefOf.SilentInput, false);

                                }
                            };

                            // Z
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.z += movef;
                                        n = bedAnimationSetting.testOffset.z;
                                    }
                                    Messages.Message($"testOffset.z : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testOffset.z -= movef;
                                        n = bedAnimationSetting.testOffset.z;
                                    }
                                    Messages.Message($"testOffset.z : {n}", MessageTypeDefOf.SilentInput, false);

                                }
                            };

                            // DrawSize x
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "draw x Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testDrawSize.x += movef;
                                        //bedAnimationSetting.graphic.data.drawSize.x += movef;
                                        bedAnimationSetting.graphic.drawSize.x += movef;
                                        n = bedAnimationSetting.testDrawSize.x;
                                    }
                                    Messages.Message($"testDrawSize.x : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "draw x Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testDrawSize.x -= movef;
                                        //bedAnimationSetting.graphic.data.drawSize.x -= movef;
                                        bedAnimationSetting.graphic.drawSize.x -= movef;
                                        n = bedAnimationSetting.testDrawSize.x;
                                    }
                                    Messages.Message($"testDrawSize.x : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // DrawSize y
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "draw y Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Big"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testDrawSize.y += movef;
                                        //bedAnimationSetting.graphic.data.drawSize.y += movef;
                                        bedAnimationSetting.graphic.drawSize.y += movef;
                                        n = bedAnimationSetting.testDrawSize.y;
                                    }
                                    Messages.Message($"testDrawSize.y : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = $"{label} : " + "draw y Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Small"),
                                action = delegate
                                {
                                    float n = 0;
                                    foreach (var bedAnimationSetting in bedAnimationSettings)
                                    {
                                        bedAnimationSetting.testDrawSize.y -= movef;
                                        //bedAnimationSetting.graphic.data.drawSize.y -= movef;
                                        bedAnimationSetting.graphic.drawSize.y -= movef;
                                        n = bedAnimationSetting.testDrawSize.y;
                                    }
                                    Messages.Message($"testDrawSize.y : {n}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                        }
                    }

                    //포트레이트 쪽
                    int i = 0;
                    foreach (var portraitIngredient in AnimationSettingComp.portraitIngredients)
                    {

                        yield return new Command_Action
                        {
                            defaultLabel = $"Open Portrait Gizmo : {i}",
                            icon = ContentFinder<Texture2D>.Get("UI/YR_Dummy"),
                            action = delegate
                            {
                                if (portraitIngredient.openTestGizmo)
                                {
                                    portraitIngredient.openTestGizmo = false;
                                }
                                else
                                {
                                    portraitIngredient.openTestGizmo = true;
                                }
                            }
                        };

                        if (portraitIngredient.openTestGizmo)
                        {
                            //reset
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Reset",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset"),
                                action = delegate
                                {
                                    portraitIngredient.testAngle = 0;
                                    portraitIngredient.testDrawSize = new Vector2();
                                    portraitIngredient.testOffset = new Vector3();
                                    portraitIngredient.testCameraOffset = new Vector3();
                                    portraitIngredient.testCameraZoom = 0;
                                }
                            };

                            // X
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.x += movef;

                                    Messages.Message($"Portrait testOffset.x : {portraitIngredient.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.x -= movef;

                                    Messages.Message($"Portrait testOffset.x : {portraitIngredient.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // Y
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Front",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.y += movef;

                                    Messages.Message($"testOffset.y : {portraitIngredient.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Back",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.y -= movef;

                                    Messages.Message($"testOffset.y : {portraitIngredient.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // Z
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.z += movef;

                                    Messages.Message($"testOffset.z : {portraitIngredient.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    portraitIngredient.testOffset.z -= movef;

                                    Messages.Message($"testOffset.z : {portraitIngredient.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };


                            // DrawSize x
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw x Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big"),
                                action = delegate
                                {
                                    portraitIngredient.testDrawSize.x += movef;

                                    Messages.Message($"Portrait testDrawSize.x : {portraitIngredient.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw x Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small"),
                                action = delegate
                                {
                                    portraitIngredient.testDrawSize.x -= movef;

                                    Messages.Message($"Portrait testDrawSize.x : {portraitIngredient.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // DrawSize y
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw y Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Big"),
                                action = delegate
                                {
                                    portraitIngredient.testDrawSize.y += movef;

                                    Messages.Message($"Portrait testDrawSize.y : {portraitIngredient.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw y Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Small"),
                                action = delegate
                                {
                                    portraitIngredient.testDrawSize.y -= movef;

                                    Messages.Message($"Portrait testDrawSize.y : {portraitIngredient.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // CameraOffset x
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.x += movef;

                                    Messages.Message($"Portrait testCameraOffset.x : {portraitIngredient.testCameraOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.x -= movef;

                                    Messages.Message($"Portrait testCameraOffset.x : {portraitIngredient.testCameraOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // CameraOffset y
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Front",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.y += movef;

                                    Messages.Message($"Portrait testCameraOffset.y : {portraitIngredient.testCameraOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Back",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.y -= movef;

                                    Messages.Message($"Portrait testCameraOffset.y : {portraitIngredient.testCameraOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // CameraOffset z
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.z += movef;

                                    Messages.Message($"Portrait testCameraOffset.z : {portraitIngredient.testCameraOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "CameraOffset : Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraOffset.z -= movef;

                                    Messages.Message($"Portrait testCameraOffset.z : {portraitIngredient.testCameraOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };


                            // CameraZoom
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : CameraZoom +",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraZoom += movef;

                                    Messages.Message($"Portrait testCameraZoom : {portraitIngredient.testCameraZoom}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : CameraZoom -",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small"),
                                action = delegate
                                {
                                    portraitIngredient.testCameraZoom -= movef;

                                    Messages.Message($"Portrait testCameraZoom : {portraitIngredient.testCameraZoom}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                        }
                        i++;
                    }

                    //채우는 바 쪽
                    int y = 0;
                    foreach (var fillableBarIngredient in AnimationSettingComp.fillableBarIngredients)
                    {

                        yield return new Command_Action
                        {
                            defaultLabel = $"Open FillableBar Gizmo : {y}",
                            icon = ContentFinder<Texture2D>.Get("UI/YR_Dummy"),
                            action = delegate
                            {
                                if (fillableBarIngredient.openTestGizmo)
                                {
                                    fillableBarIngredient.openTestGizmo = false;
                                }
                                else
                                {
                                    fillableBarIngredient.openTestGizmo = true;
                                }
                            }
                        };

                        if (fillableBarIngredient.openTestGizmo)
                        {
                            //reset
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Reset",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset"),
                                action = delegate
                                {
                                    fillableBarIngredient.testDrawSize = new Vector2();
                                    fillableBarIngredient.testOffset = new Vector3();
                                }
                            };

                            // X
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.x += movef;

                                    Messages.Message($"FillableBar testOffset.x : {fillableBarIngredient.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.x -= movef;

                                    Messages.Message($"FillableBar testOffset.x : {fillableBarIngredient.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // Y
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Front",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.y += movef;

                                    Messages.Message($"testOffset.y : {fillableBarIngredient.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Back",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.y -= movef;

                                    Messages.Message($"testOffset.y : {fillableBarIngredient.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // Z
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.z += movef;

                                    Messages.Message($"testOffset.z : {fillableBarIngredient.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    fillableBarIngredient.testOffset.z -= movef;

                                    Messages.Message($"testOffset.z : {fillableBarIngredient.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };


                            // DrawSize x
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : draw x Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big"),
                                action = delegate
                                {
                                    fillableBarIngredient.testDrawSize.x += movef;

                                    Messages.Message($"FillableBar testDrawSize.x : {fillableBarIngredient.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : draw x Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small"),
                                action = delegate
                                {
                                    fillableBarIngredient.testDrawSize.x -= movef;

                                    Messages.Message($"FillableBar testDrawSize.x : {fillableBarIngredient.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // DrawSize y
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : draw y Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Big"),
                                action = delegate
                                {
                                    fillableBarIngredient.testDrawSize.y += movef;

                                    Messages.Message($"FillableBar testDrawSize.y : {fillableBarIngredient.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "FillableBar : draw y Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Small"),
                                action = delegate
                                {
                                    fillableBarIngredient.testDrawSize.y -= movef;

                                    Messages.Message($"FillableBar testDrawSize.y : {fillableBarIngredient.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                        }
                        y++;
                    }
                }
            }

            void ToggleOpenControlSetting()
            {
                if (openControllSetting)
                {
                    openControllSetting = false;

                    openPawnOffsetSettingGizmo = false;

                    foreach (var bedAnimationSettingAndTick in AnimationSettingComp.bedAnimationSettingAndTicks)
                    {
                        bedAnimationSettingAndTick.openTestGizmo = false;
                    }

                    foreach (var portraitIngredient in AnimationSettingComp.portraitIngredients)
                    {
                        portraitIngredient.openTestGizmo = false;
                    }

                    foreach (var fillableBarIngredient in AnimationSettingComp.fillableBarIngredients)
                    {
                        fillableBarIngredient.openTestGizmo = false;
                    }
                }
                else
                {
                    openControllSetting = true;
                }
            }

            void ToggleMovef()
            {
                // 다음 인덱스로 이동, 리스트를 순환
                currentIndex = (currentIndex + 1) % values.Count;

                // 새 값을 얻고 특정 소숫점 이하에서 올림
                movef = (float)Math.Round(values[currentIndex], 5);
            }
        }

        private readonly List<float> values = new List<float> { 0.0001f, 0.001f, 0.01f, 0.1f, 1f };
        private int currentIndex = 0;
        float movef = 0.001f;
        public bool dummyForJoyIsActive;
        public Pawn dummyForJoyPawn;
        public Pawn previousJoyPawn;
        private Vector3 testPawnOffset;
        private bool openPawnOffsetSettingGizmo;

        public void Notify_PawnDied(Pawn pawn, DamageInfo? dinfo)
        {
            if (pawn == HeldPawn)
            {
                innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
                if (!dinfo.HasValue || !dinfo.Value.Def.execution)
                {
                    Messages.Message("EntityDiedOnHoldingPlatform".Translate(pawn), pawn, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
        public RoofCollapseResponse Notify_OnBeforeRoofCollapse()
        {
            if (!Occupied)
            {
                return RoofCollapseResponse.None;
            }

            if (HeldPawn is IRoofCollapseAlert roofCollapseAlert)
            {
                roofCollapseAlert.Notify_OnBeforeRoofCollapse();
            }

            foreach (IRoofCollapseAlert comp in HeldPawn.GetComps<IRoofCollapseAlert>())
            {
                comp.Notify_OnBeforeRoofCollapse();
            }

            return RoofCollapseResponse.None;
        }

        public override void Notify_DefsHotReloaded()
        {
            base.Notify_DefsHotReloaded();
            //chains = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastDamaged, "lastDamaged", 0);
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref heldPawnStartTick, "heldPawnStartTick", 0);
            Scribe_Values.Look(ref stopAnimation, "stopAnimation");
        }
    }
}
