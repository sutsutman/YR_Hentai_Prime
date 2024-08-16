using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AffectedByCNMS : CompProperties
    {
        public CompProperties_AffectedByCNMS() => compClass = typeof(CompAffectedByCNMS);
        public int ticksToSpawn = 2000;
        public ThingDef thingToSpawn;
        public int spawnCount;
        public List<ConditionSpawnThing> conditionSpawnThings = new List<ConditionSpawnThing>();
    }

    public class ConditionSpawnThing
    {
        public PawnCondition pawnCondition;
        public ThingDef thingToSpawn;
        public int spawnCount;
    }

    [StaticConstructorOnStartup]
    public class CompAffectedByCNMS : CompBaseOfAnimationBed
    {
        public CompProperties_AffectedByCNMS Props => (CompProperties_AffectedByCNMS)props;


        public CompAffectedByFacilities AffectedByFacilitiesComp => parent.TryGetComp<CompAffectedByFacilities>();


        public int ticksToSpawn = 100;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksToSpawn = Props.ticksToSpawn;
        }
        public override void CompTick()
        {
            base.CompTick();

            if (HeldPawn != null)
            {
                if (Building_AnimationBed.PowerOn)
                {
                    ticksToSpawn--;
                }
            }
            else
            {
                ticksToSpawn = Props.ticksToSpawn;
            }

            if (ticksToSpawn <= 0)
            {
                Thing spawnThing = MakeSpawnThing();

                if (AffectedByFacilitiesComp == null)
                {
                    GenSpawn.Spawn(spawnThing, parent.Position, parent.Map);
                    return;
                }

                if (AffectedByFacilitiesComp.LinkedFacilitiesListForReading.NullOrEmpty())
                {
                    GenSpawn.Spawn(spawnThing, parent.Position, parent.Map);
                    return;
                }
                else
                {
                    // 가장 가까운 CNMS 찾기
                    Thing closestCNMS = FindClosestCNMS(parent.Position, AffectedByFacilitiesComp.LinkedFacilitiesListForReading);

                    var compCNMS = closestCNMS.TryGetComp<CompFacility_CNMS>();
                    if (compCNMS != null)
                    {
                        compCNMS.innerContainer.TryAddOrTransfer(spawnThing);
                    }
                    else
                    {
                        Log.Error($"{closestCNMS.def} is set to CompProperties_AffectedByFacilities_CNMS, but it is not CNMS");

                        GenSpawn.Spawn(spawnThing, parent.Position, parent.Map);
                        return;
                    }
                }
            }
        }
        private Thing FindClosestCNMS(IntVec3 referencePosition, List<Thing> things)
        {
            if (things == null || things.Count == 0)
            {
                return null;
            }

            Thing closestThing = null;
            float closestDistance = float.MaxValue;

            foreach (var thing in things)
            {
                float distance = referencePosition.DistanceTo(thing.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestThing = thing;
                }
            }

            return closestThing;
        }

        private Thing MakeSpawnThing()
        {
            ticksToSpawn = Props.ticksToSpawn;

            ThingDef thingToSpawn = Props.thingToSpawn;
            int spawnCount = Props.spawnCount;

            foreach (var conditionSpawnThing in Props.conditionSpawnThings)
            {
                void action()
                {
                    thingToSpawn = conditionSpawnThing.thingToSpawn;
                    spawnCount = conditionSpawnThing.spawnCount;
                }

                if (Condition.ExecuteActionIfConditionMatches(Building_AnimationBed, conditionSpawnThing.pawnCondition, action))
                {
                    break;
                }

            }

            Thing thing = ThingMaker.MakeThing(thingToSpawn, null);
            thing.stackCount = spawnCount;

            return thing;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");

        }
    }
}
