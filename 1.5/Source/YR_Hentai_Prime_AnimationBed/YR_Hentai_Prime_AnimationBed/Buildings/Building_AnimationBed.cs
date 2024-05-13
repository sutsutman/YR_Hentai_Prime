using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class Building_AnimationBed : Building, IThingHolderWithDrawnPawn, IThingHolder, IRoofCollapseAlert, ISearchableContents
    {
        //public struct Chain
        //{
        //    public Vector3 from;

        //    public Vector3 to;

        //    public Graphic graphic;

        //    public Graphic baseFastenerGraphic;

        //    public Graphic targetFastenerGraphic;

        //    public float rotation;
        //}


        public ThingOwner innerContainer;

        private int lastDamaged;

        private Graphic chainsUntetheredGraphic;

        //private List<Chain> chains;

        private CompAffectedByFacilities facilitiesComp;

        private CompAttachPoints attachPointsComp;

        private CompAnimationSetting animationSettingComp;

        private CompToggleHediff toggleHediffComp;

        private AttachPointTracker targetPoints;

        //private List<Chain> defaultPointMapping;

        [Unsaved(false)]
        private int debugEscapeTick = -1;

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
                PawnAnimationSetting pawnAnimationSetting = AnimationSettingComp.Props.pawnAnimationSetting;
                var offset = pawnAnimationSetting.offset;

                foreach (var conditonPawnOffset in pawnAnimationSetting.conditonPawnOffsets)
                {
                    if (Condition.Match(HeldPawn, this, conditonPawnOffset.condition))
                    {
                        offset = conditonPawnOffset.offset;
                        if (Condition.NeedBreak(conditonPawnOffset.condition))
                        {
                            break;
                        }
                    }
                }

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

        public ThingOwner SearchableContents => innerContainer;

        private AttachPointTracker TargetPawnAttachPoints
        {
            get
            {
                if (targetPoints != null && targetPoints.ThingId != HeldPawn.ThingID)
                {
                    targetPoints = null;
                }

                bool num = targetPoints == null;
                targetPoints = targetPoints ?? HeldPawn.TryGetComp<CompAttachPoints>()?.points;
                if (num)
                {
                    foreach (HediffComp_AttachPoints hediffComp in HeldPawn.health.hediffSet.GetHediffComps<HediffComp_AttachPoints>())
                    {
                        if (hediffComp.Points != null)
                        {
                            if (targetPoints == null)
                            {
                                targetPoints = hediffComp.Points;
                            }
                            else
                            {
                                targetPoints.Add(hediffComp.Points);
                            }
                        }
                    }
                }

                return targetPoints;
            }
        }

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

        //private Graphic ChainsUntetheredGraphic
        //{
        //    get
        //    {
        //        if (chainsUntetheredGraphic == null)
        //        {
        //            chainsUntetheredGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.untetheredGraphicTexPath, ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
        //        }

        //        return chainsUntetheredGraphic;
        //    }
        //}

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

        //public List<Chain> DefaultPointMapping
        //{
        //    get
        //    {
        //        if (defaultPointMapping == null)
        //        {
        //            defaultPointMapping = new List<Chain>();
        //            Vector3 worldPos = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint0);
        //            Vector3 worldPos2 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint1);
        //            Vector3 worldPos3 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint2);
        //            Vector3 worldPos4 = AttachPointsComp.points.GetWorldPos(AttachPointType.PlatformRestraint3);
        //            Vector2 vector = new Vector2(Vector3.Distance(worldPos, worldPos3), 1f);
        //            defaultPointMapping.Add(new Chain
        //            {
        //                from = worldPos,
        //                to = worldPos3,
        //                graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.Cutout.Shader, vector, Color.white) as Graphic_Tiling).WithTiling(vector),
        //                baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
        //                targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
        //                rotation = (worldPos3.WithY(0f) - worldPos.WithY(0f)).normalized.ToAngleFlat()
        //            });
        //            vector = new Vector2(Vector3.Distance(worldPos2, worldPos4), 1f);
        //            defaultPointMapping.Add(new Chain
        //            {
        //                from = worldPos2,
        //                to = worldPos4,
        //                graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.Cutout.Shader, vector, Color.white) as Graphic_Tiling).WithTiling(vector),
        //                baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
        //                targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.Cutout.Shader, Vector2.one, Color.white),
        //                rotation = (worldPos4.WithY(0f) - worldPos2.WithY(0f)).normalized.ToAngleFlat()
        //            });
        //        }

        //        return defaultPointMapping;
        //    }
        //}

        public Building_AnimationBed() => innerContainer = new ThingOwner<Thing>(this);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            //if (!ModLister.CheckAnomaly("Holding platform"))
            //{
            //    Destroy();
            //    return;
            //}

            base.SpawnSetup(map, respawningAfterLoad);
            //Find.StudyManager.UpdateStudiableCache(this, base.Map);
            //Find.Anomaly.hasBuiltHoldingPlatform = true;
        }

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

        private bool TryGetFirstColonistDirection(out Vector2 direction)
        {
            foreach (Thing item in GenRadial.RadialDistinctThingsAround(base.Position, base.Map, 4f, useCenter: false))
            {
                if (item is Pawn pawn && pawn.IsColonist)
                {
                    direction = pawn.Position.ToVector2() - base.Position.ToVector2();
                    return true;
                }
            }

            direction = Vector2.zero;
            return false;
        }

        public bool setAnimation = true;
        public bool openControllSetting;

        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();

            {
                //if (Occupied && chains == null && AttachPointsComp != null)
                //{
                //    chains = ((TargetPawnAttachPoints != null) ? BuildTargetPointMapping() : DefaultPointMapping);
                //}

                //if (!Occupied && chains != null)
                //{
                //    chains = null;
                //}

                //if (Occupied && HasAttachedElectroharvester && Rand.MTBEventOccurs(2f, 60000f, 1f))
                //{
                //    HeldPawn.TakeDamage(new DamageInfo(DamageDefOf.ElectricalBurn, Damage.RandomInRange));
                //}

                //if (Occupied && Rand.MTBEventOccurs(100f, 1f, 1f))
                //{
                //    UpdateAnimation();
                //}

                //if (debugEscapeTick > 0 && Find.TickManager.TicksGame == debugEscapeTick && HeldPawn != null)
                //{
                //    HeldPawn.TryGetComp<CompAnimationBedTarget>()?.Escape(initiator: false);
                //}
            }

            if (HeldPawn != null)
            {
                BedAnimationUtility.SetAnimation(this);
            }
            if (heldPawnStartTick == -1 && HeldPawn != null)
            {
                heldPawnStartTick = Find.TickManager.TicksGame;
            }
            else if (HeldPawn == null)
            {
                heldPawnStartTick = -1;
            }
        }


        //private void UpdateAnimation()
        //{
        //    if (HeldPawn.TryGetComp<CompAnimationBedTarget>(out var comp) && (!comp.Props.hasAnimation || HeldPawn.health.Downed))
        //    {
        //        HeldPawn.Drawer.renderer.SetAnimation(null);
        //        return;
        //    }

        //    SoundDef soundDef = PlatformProps.entityLungeSoundLow;
        //    AnimationDef animationDef = AnimationDefOf.HoldingPlatformWiggleLight;
        //    if (TryGetFirstColonistDirection(out var direction))
        //    {
        //        if (TargetPawnAttachPoints != null && GenTicks.IsTickInterval(3))
        //        {
        //            Vector2 vector = direction.normalized.Cardinalize();
        //            if (vector == Vector2.up)
        //            {
        //                animationDef = AnimationDefOf.HoldingPlatformLungeUp;
        //            }

        //            if (vector == Vector2.right)
        //            {
        //                animationDef = AnimationDefOf.HoldingPlatformLungeRight;
        //            }

        //            if (vector == Vector2.left)
        //            {
        //                animationDef = AnimationDefOf.HoldingPlatformLungeLeft;
        //            }

        //            if (vector == Vector2.down)
        //            {
        //                animationDef = AnimationDefOf.HoldingPlatformLungeDown;
        //            }

        //            soundDef = PlatformProps.entityLungeSoundHi;
        //        }
        //        else
        //        {
        //            animationDef = AnimationDefOf.HoldingPlatformWiggleIntense;
        //        }
        //    }

        //    if (HeldPawn.Drawer.renderer.CurAnimation != animationDef)
        //    {
        //        soundDef?.PlayOneShot(this);
        //        HeldPawn.Drawer.renderer.SetAnimation(animationDef);
        //    }
        //}

        //public List<Chain> BuildTargetPointMapping()
        //{
        //    if (chains == null)
        //    {
        //        chains = new List<Chain>();
        //    }
        //    else
        //    {
        //        chains.Clear();
        //    }

        //    HeldPawn.Drawer.renderer.renderTree.GetRootTPRS(HeldPawnDrawParms, out var offset, out var _, out var rotation, out var _);
        //    Vector3 vector = DrawPos + PawnDrawOffset;
        //    Dictionary<AttachPointType, Vector3> dictionary = new Dictionary<AttachPointType, Vector3>();
        //    int num = 5;
        //    int num2 = 8;
        //    foreach (AttachPointType item in TargetPawnAttachPoints.PointTypes(num, num2))
        //    {
        //        Vector3 value = vector + rotation * (offset + TargetPawnAttachPoints.GetRotatedOffset(item, base.Rotation));
        //        dictionary.Add(item, value);
        //    }

        //    for (int i = num; i <= num2; i++)
        //    {
        //        Vector3 vector2 = GetPlatformPoints()[(AttachPointType)i];
        //        Vector3 vector3 = dictionary[(AttachPointType)i];
        //        Vector3 vector4 = Vector3.Lerp(vector2, vector3, AnimationAlpha);
        //        float x = Vector3.Distance(vector4, vector2);
        //        Vector2 vector5 = new Vector2(x, 1f);
        //        chains.Add(new Chain
        //        {
        //            from = vector2,
        //            to = vector4,
        //            graphic = (GraphicDatabase.Get<Graphic_Tiling>(PlatformProps.tilingChainTexPath, ShaderTypeDefOf.CutoutTiling.Shader, vector5, Color.white) as Graphic_Tiling).WithTiling(vector5),
        //            baseFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.baseChainFastenerTexPath, ShaderTypeDefOf.CutoutTiling.Shader, Vector2.one, Color.white),
        //            targetFastenerGraphic = GraphicDatabase.Get<Graphic_Single>(PlatformProps.targetChainFastenerTexPath, ShaderTypeDefOf.CutoutTiling.Shader, Vector2.one, Color.white),
        //            rotation = (vector3.WithY(0f) - vector2.WithY(0f)).normalized.ToAngleFlat()
        //        });
        //    }

        //    return chains;
        //}

        private Dictionary<AttachPointType, Vector3> GetPlatformPoints()
        {
            if (platformPoints == null)
            {
                platformPoints = new Dictionary<AttachPointType, Vector3>();
                int min = 5;
                int max = 8;
                foreach (AttachPointType item in AttachPointsComp.points.PointTypes(min, max))
                {
                    platformPoints.Add(item, AttachPointsComp.points.GetWorldPos(item));
                }
            }

            return platformPoints;
        }

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);
            Pawn heldPawn = HeldPawn;
            if (heldPawn != null)
            {
                Rot4 rotation = AnimationSettingComp.Props.pawnAnimationSetting.rotation;
                if (heldPawn.IsMutant && heldPawn.RaceProps.Animal)
                {
                    rotation = Rot4.East;
                }

                foreach (var conditionPawnRotation in AnimationSettingComp.Props.pawnAnimationSetting.conditionPawnRotations)
                {
                    if (Condition.Match(HeldPawn, this, conditionPawnRotation.condition))
                    {
                        rotation = conditionPawnRotation.rotation;

                        if (Condition.NeedBreak(conditionPawnRotation.condition))
                        {
                            break;
                        }
                    }
                }

                heldPawn.Drawer.renderer.DynamicDrawPhaseAt(phase, DrawPos + PawnDrawOffset, rotation, neverAimWeapon: true);

                BedAnimationUtility.DrawBedAnimation(this, AnimationSettingComp, HeldPawn);
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
            //if (HeldPawn != null)
            //{
            //    DrawChains();
            //}
            //else
            //{
            //    ChainsUntetheredGraphic.Draw(drawLoc + Vector3.up * 0.05f, base.Rotation, this);
            //}
        }

        //private void DrawChains()
        //{
        //    if (chains == null)
        //    {
        //        return;
        //    }

        //    chains = ((TargetPawnAttachPoints != null) ? BuildTargetPointMapping() : DefaultPointMapping);
        //    Vector3 vector = Vector3.up * (9f / 65f);
        //    foreach (Chain chain in chains)
        //    {
        //        Vector3 v = (chain.from + chain.to) / 2f;
        //        chain.graphic.Draw(v.WithY(DrawPos.y) + vector, base.Rotation, this, chain.rotation + 180f);
        //        chain.targetFastenerGraphic.Draw(chain.to + 2f * vector, base.Rotation, this, chain.rotation + 90f);
        //        chain.baseFastenerGraphic.Draw(chain.from + 2f * vector, base.Rotation, this, chain.rotation + 90f);
        //    }
        //}

        public void EjectContents()
        {
            //defaultPointMapping = null;
            //chains = null;
            setAnimation = true;
            animationSettingComp.needMakeGraphics = true;

            HeldPawn?.Drawer.renderer.SetAnimation(null);
            HeldPawn?.GetComp<CompAnimationBedTarget>()?.Notify_ReleasedFromPlatform();
            ToggleHediffComp?.RemoveAllHediffs(HeldPawn);

            RemoveAnimationBedHediff();

            innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
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

        //public override string GetInspectString()
        //{
        //    string text = base.GetInspectString();
        //    if (!text.NullOrEmpty())
        //    {
        //        text += "\n";
        //    }

        //    Pawn heldPawn = HeldPawn;
        //    if (heldPawn != null)
        //    {
        //        TaggedString ts = "HoldingThing".Translate() + ": " + heldPawn.NameShortColored.CapitalizeFirst();
        //        bool flag = this.SafelyContains(heldPawn);
        //        if (!flag)
        //        {
        //            ts += " (" + "HoldingPlatformRequiresStrength".Translate(StatDefOf.MinimumContainmentStrength.Worker.ValueToString(heldPawn.GetStatValue(StatDefOf.MinimumContainmentStrength), finalized: false)) + ")";
        //        }

        //        text += ts.Colorize(flag ? Color.white : ColorLibrary.RedReadable);
        //    }
        //    else
        //    {
        //        text += "HoldingThing".Translate() + ": " + "Nothing".Translate().CapitalizeFirst();
        //    }

        //    //if (heldPawn != null && heldPawn.def.IsStudiable)
        //    //{
        //    //    string inspectStringExtraFor = CompStudiable.GetInspectStringExtraFor(heldPawn);
        //    //    if (!inspectStringExtraFor.NullOrEmpty())
        //    //    {
        //    //        text = text + "\n" + inspectStringExtraFor;
        //    //    }
        //    //}

        //    //if (heldPawn != null && heldPawn.TryGetComp<CompProducesBioferrite>(out var comp))
        //    //{
        //    //    string text2 = comp.CompInspectStringExtra();
        //    //    if (!text2.NullOrEmpty())
        //    //    {
        //    //        text = text + "\n" + text2;
        //    //    }
        //    //}

        //    return text;
        //}

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

            if (!DebugSettings.ShowDevGizmos || HeldPawn == null)
            {
                yield break;
            }

            yield return new Command_Action
            {
                defaultLabel = "DEV: Timed escape",
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    for (int i = 1; i < 21; i++)
                    {
                        int delay = i * 60;
                        list.Add(new FloatMenuOption(delay.TicksToSeconds() + "s", delegate
                        {
                            debugEscapeTick = Find.TickManager.TicksGame + delay;
                        }));
                    }

                    Find.WindowStack.Add(new FloatMenu(list));
                }
            };

            //테스트용 기즈모

            foreach (var testGizmo in TESTGIZMO())
            {
                yield return testGizmo;
            }

        }

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


                    foreach (var bedAnimationSettingAndTick in AnimationSettingComp.bedAnimationSettingAndTicks)
                    {
                        List<BedAnimationSetting> bedAnimationSettings = bedAnimationSettingAndTick.bedAnimationSettings;

                        Texture2D openGizmoIcon = ContentFinder<Texture2D>.Get("UI/YR_Dummy");
                        if (bedAnimationSettings[0]?.graphicData?.texPath != null)
                        {

                            openGizmoIcon = ContentFinder<Texture2D>.Get(bedAnimationSettings[0].graphicData.texPath);
                        }

                        yield return new Command_Action
                        {
                            defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "OpenGizmo",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Reset",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Right",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Left",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Front",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Back",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Up",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "Down",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "draw x Big",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "draw x Small",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "draw y Big",
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
                                defaultLabel = $"{bedAnimationSettingAndTick.parentBedAnimationDef.defName} : " + "draw y Small",
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

                    if (AnimationSettingComp.portraitMeshs != null)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = $"Open Portrait Gizmo",
                            icon = ContentFinder<Texture2D>.Get("UI/YR_Dummy"),
                            action = delegate
                            {
                                if (AnimationSettingComp.portraitMeshs.openTestGizmo)
                                {
                                    AnimationSettingComp.portraitMeshs.openTestGizmo = false;
                                }
                                else
                                {
                                    AnimationSettingComp.portraitMeshs.openTestGizmo = true;
                                }
                            }
                        };

                        if (AnimationSettingComp.portraitMeshs.openTestGizmo)
                        {
                            //reset
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Reset",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testAngle = 0;
                                    animationSettingComp.portraitMeshs.testDrawSize = new Vector2();
                                    animationSettingComp.portraitMeshs.testOffset = new Vector3();
                                }
                            };

                            // X
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Right",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.x += movef;

                                    Messages.Message($"Portrait testOffset.x : {animationSettingComp.portraitMeshs.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Left",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.x -= movef;

                                    Messages.Message($"Portrait testOffset.x : {animationSettingComp.portraitMeshs.testOffset.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // Y
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Front",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.y += movef;

                                    Messages.Message($"testOffset.y : {animationSettingComp.portraitMeshs.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Back",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.y -= movef;

                                    Messages.Message($"testOffset.y : {animationSettingComp.portraitMeshs.testOffset.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };


                            // Z
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Up",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.z += movef;

                                    Messages.Message($"testOffset.z : {animationSettingComp.portraitMeshs.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : Down",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testOffset.z -= movef;

                                    Messages.Message($"testOffset.z : {animationSettingComp.portraitMeshs.testOffset.z}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // DrawSize x
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw x Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testDrawSize.x += movef;

                                    Messages.Message($"Portrait testDrawSize.x : {animationSettingComp.portraitMeshs.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw x Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testDrawSize.x -= movef;

                                    Messages.Message($"Portrait testDrawSize.x : {animationSettingComp.portraitMeshs.testDrawSize.x}", MessageTypeDefOf.SilentInput, false);
                                }
                            };

                            // DrawSize y
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw y Big",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Big"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testDrawSize.y += movef;

                                    Messages.Message($"Portrait testDrawSize.y : {animationSettingComp.portraitMeshs.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                            yield return new Command_Action
                            {
                                defaultLabel = "Portrait : draw y Small",
                                icon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_y_Small"),
                                action = delegate
                                {
                                    animationSettingComp.portraitMeshs.testDrawSize.y -= movef;

                                    Messages.Message($"Portrait testDrawSize.y : {animationSettingComp.portraitMeshs.testDrawSize.y}", MessageTypeDefOf.SilentInput, false);
                                }
                            };
                        }
                    }

                }


            }

            void ToggleOpenControlSetting()
            {
                if (openControllSetting)
                {
                    openControllSetting = false;
                    foreach (var bedAnimationSettingAndTick in AnimationSettingComp.bedAnimationSettingAndTicks)
                    {
                        bedAnimationSettingAndTick.openTestGizmo = false;
                    }

                    if (AnimationSettingComp.portraitMeshs != null)
                    {
                        AnimationSettingComp.portraitMeshs.openTestGizmo = false;
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
                movef = (float)Math.Round(values[currentIndex], 3);
            }
        }


        private readonly List<float> values = new List<float> { 0.001f, 0.01f, 0.1f, 1f };
        private int currentIndex = 0;
        float movef = 0.001f;
        public bool dummyForJoyIsActive;

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
        }
    }
}
