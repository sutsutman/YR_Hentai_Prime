using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Maggot_Queen : CompProperties
    {
        public CompProperties_Maggot_Queen()
        {
            compClass = typeof(Comp_Maggot_Queen);
        }
        public ThingDef bedDef;
        public ThingDef doorDef;
    }

    public class Comp_Maggot_Queen : ThingComp
    {
        public CompProperties_Maggot_Queen Props => (CompProperties_Maggot_Queen)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        public int ticks = 100;
        public override void CompTick()
        {
            base.CompTick();
            ticks--;
            if (ticks <= 0)
            {
                ticks = 100;
                if (parent is Pawn pawn)
                {
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                }
            }
        }

        public void SpawnDoor(Pawn pawn, int loopNum = 3)

        {
            foreach (IntVec3 intVec in GenAdj.CellsAdjacent8Way(pawn))
            {
                SpawnDoorAt(intVec, pawn, Props.doorDef, loopNum);
            }

            bool SpawnDoorAt(IntVec3 intVec, Pawn pawn, ThingDef doorDef, int loopNum)
            {
                if (loopNum <= 0)
                {
                    return false;
                }

                if (intVec != pawn.Position)
                {
                    Building edifice = intVec.GetEdifice(pawn.Map);
                    // If the edifice is null, spawn doorDef
                    if (edifice == null)
                    {
                        var thing = ThingMaker.MakeThing(doorDef);
                        bool spawnSucceeded = GenPlace.TryPlaceThing(thing, intVec, pawn.Map, ThingPlaceMode.Direct, out Thing t, null, null, default);
                        return spawnSucceeded;
                    }
                    // If something is there but it's not impassable (a wall), spawn doorDef in adjacent cells
                    else if (edifice.def.passability != Traversability.Impassable && !(edifice is Building_Door))
                    {
                        foreach (IntVec3 intVec2 in GenAdj.CellsAdjacent8Way(edifice))
                        {
                            if (GenSight.LineOfSight(pawn.Position, intVec2, pawn.Map, false, null, 0, 0))
                            {
                                SpawnDoorAt(intVec2, pawn, doorDef, loopNum - 1);
                            }
                        }
                    }
                }

                return false;
            }
        }

        public void StartSpawnBed(Pawn carrier, Pawn pawn)
        {
            Building edifice = carrier.Position.GetEdifice(carrier.Map);
            if (edifice != null && edifice != parent)
            {
                return;
            }

            bool becomPrisoner = (pawn.IsPrisoner || pawn.Faction.HostileTo(Faction.OfPlayer));
            Building_AnimationBed bed = (Building_AnimationBed)GenSpawn.Spawn(Props.bedDef, carrier.Position, carrier.Map);
            bed.SetFaction(Faction.OfPlayer, null);
            //bed.Rotation = Rot4.South;

            //아군, 비 적대
            //if (!becomPrisoner)
            //{
            //    //침대 설치 불가능
            //    if (carrier.GetRoom().IsPrisonCell)
            //    {
            //        SpawnDoor(carrier);
            //    }
            //}
            //else if (becomPrisoner && (carrier.GetRoom() == null || (!carrier.GetRoom().IsPrisonCell && carrier.GetRoom().ContainedBeds.Count() > 0)))
            //{
            //    SpawnDoor(carrier);
            //}

            //아군, 비 적대고, 일반 침대가 설치 불가능할 경우
            if (becomPrisoner)
            {
                //bed.ForOwnerType = BedOwnerType.Prisoner;

                pawn.GetRoom().Notify_RoomShapeChanged();
                pawn.guest.CapturedBy(Faction.OfPlayer);
            }
            else if (pawn.IsSlave)
            {
                //bed.ForOwnerType = BedOwnerType.Slave;
            }
            //bed.SetCompAnimationBed.HeldPawn(pawn);
            //pawn.jobs.Notify_TuckedIntoBed(bed);
            //pawn.mindState.Notify_TuckedIntoBed();

            var compAnimationBed = bed.TryGetComp<CompAnimationBed>();

            if (compAnimationBed.Container.TryAdd(pawn.SplitOff(1)))
            {
                //pawn.DeSpawn(); // 맵에서 제거
                CompAnimationBedTarget compTarget = pawn.TryGetComp<CompAnimationBedTarget>();
                if (compTarget != null)
                {
                    compTarget.Notify_HeldOnPlatform(compAnimationBed.Container);
                }
            }

            if (bed != null)
            {
                var comp = bed.TryGetComp<Comp_Maggot_Queen_Bed>();
                if (comp != null)
                {
                    void action()
                    {
                        bool flag = parent.DeSpawnOrDeselect(DestroyMode.Vanish);
                        if (TryAcceptThing(comp, parent, true) && flag)
                        {
                            Find.Selector.Select(parent, false, false);
                        }
                    }
                    action();

                    //CompEffectBondageBed compEffectBondageBed = bed?.TryGetComp<CompEffectBondageBed>();
                    //if (compEffectBondageBed == null)
                    //{
                    //    return;
                    //}

                    //compEffectBondageBed.DoEffect(pawn);
                    MoteMaker.ThrowText(pawn.PositionHeld.ToVector3(), pawn.MapHeld, "YR_Bound".Translate(),
                        4f);

                    return;
                }
                //parent.Destroy();
            }


        }

        public virtual bool TryAcceptThing(Comp_Maggot_Queen_Bed comp, Thing thing, bool allowSpecialEffects = true)
        {
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, comp.innerContainer, thing.stackCount, true);
                flag = true;
            }
            else
            {
                flag = comp.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                //if (thing.Faction != null && thing.Faction.IsPlayer)
                //{
                //    this.contentsKnown = true;
                //}
                return true;
            }
            return false;
        }
    }
}