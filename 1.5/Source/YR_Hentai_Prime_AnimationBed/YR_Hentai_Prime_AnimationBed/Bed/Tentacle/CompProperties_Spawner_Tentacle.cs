using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Spawner_Tentacle : CompProperties_Spawner
    {
        public CompProperties_Spawner_Tentacle() => this.compClass = typeof(CompSpawner_Tentacle);
        public bool spawnSelfPosition = false;
        public List<ThingDef> neverStackThingDefs = new List<ThingDef>();
        public bool anotherRoomChange = false;
    }

    public class CompSpawner_Tentacle : CompSpawner
    {
        private int ticksUntilSpawn;

        public new CompProperties_Spawner_Tentacle PropsSpawner => (CompProperties_Spawner_Tentacle)props;

        private bool PowerOn => parent.GetComp<CompPowerTrader>()?.PowerOn ?? false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                ResetCountdown();
            }
        }

        public override void CompTick()
        {
            TickInterval(1);
        }

        public override void CompTickRare()
        {
            TickInterval(250);
        }

        private void TickInterval(int interval)
        {
            if (!parent.Spawned)
            {
                return;
            }

            CompCanBeDormant comp = parent.GetComp<CompCanBeDormant>();
            if (comp != null)
            {
                if (!comp.Awake)
                {
                    return;
                }
            }
            else if (parent.Position.Fogged(parent.Map))
            {
                return;
            }

            if (!PropsSpawner.requiresPower || PowerOn)
            {
                ticksUntilSpawn -= interval;
                CheckShouldSpawn();
            }

            //if(PropsSpawner.anotherRoomChange)
            //{
            //    parent.GetRoom().
            //}
        }

        private void CheckShouldSpawn()
        {
            if (ticksUntilSpawn <= 0)
            {
                ResetCountdown();
                TryDoSpawn();
            }
        }

        public new bool TryDoSpawn()
        {
            if (!parent.Spawned)
            {
                return false;
            }

            if (PropsSpawner.spawnMaxAdjacent >= 0)
            {
                int num = 0;
                for (int i = 0; i < 9; i++)
                {
                    IntVec3 c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
                    if (!c.InBounds(parent.Map))
                    {
                        continue;
                    }

                    List<Thing> thingList = c.GetThingList(parent.Map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j].def == PropsSpawner.thingToSpawn)
                        {
                            num += thingList[j].stackCount;
                            if (num >= PropsSpawner.spawnMaxAdjacent)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (TryFindSpawnCell(parent, PropsSpawner, PropsSpawner.thingToSpawn, PropsSpawner.spawnCount, out var result))
            {
                Thing thing = ThingMaker.MakeThing(PropsSpawner.thingToSpawn);
                thing.stackCount = PropsSpawner.spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + parent);
                }

                if (PropsSpawner.inheritFaction && thing.Faction != parent.Faction)
                {
                    thing.SetFaction(parent.Faction);
                }

                GenPlace.TryPlaceThing(thing, result, parent.Map, ThingPlaceMode.Direct, out var lastResultingThing, rot: Rot4.Random);
                if (PropsSpawner.spawnForbidden)
                {
                    lastResultingThing.SetForbidden(value: true);
                }

                if (PropsSpawner.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
                {
                    Messages.Message("MessageCompSpawnerSpawnedItem".Translate(PropsSpawner.thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent);
                }

                return true;
            }

            return false;
        }

        public static bool TryFindSpawnCell(Thing parent, CompProperties_Spawner_Tentacle Props, ThingDef thingToSpawn, int spawnCount, out IntVec3 result)
        {
            if (Props.spawnSelfPosition && !parent.Position.GetThingList(parent.Map).Any(x => x.def == thingToSpawn))
            {
                result = parent.Position;
                return true;
            }
            foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
            {
                if (!item.Walkable(parent.Map))
                {
                    continue;
                }

                Building edifice = item.GetEdifice(parent.Map);
                if (edifice != null && thingToSpawn.IsEdifice())
                {
                    continue;
                }

                Building_Door building_Door = edifice as Building_Door;
                if ((building_Door != null && !building_Door.FreePassage) || (parent.def.passability != Traversability.Impassable && !GenSight.LineOfSight(parent.Position, item, parent.Map)))
                {
                    continue;
                }

                bool flag = false;
                List<Thing> thingList = item.GetThingList(parent.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    if (thing.def.category == ThingCategory.Item && (thing.def != thingToSpawn || thing.stackCount > thingToSpawn.stackLimit - spawnCount))
                    {
                        flag = true;
                        break;
                    }
                    bool flag2 = false;
                    //이 코드는 뭐때문에 넣은거였지?
                    foreach (var neverStackThingDef in Props.neverStackThingDefs)
                    {
                        if (neverStackThingDef == thingToSpawn)
                        {
                            flag = true;
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        break;
                    }
                }

                if (!flag)
                {
                    result = item;
                    return true;
                }
            }

            result = IntVec3.Invalid;
            return false;
        }

        private void ResetCountdown()
        {
            ticksUntilSpawn = PropsSpawner.spawnIntervalRange.RandomInRange;
        }

        public override void PostExposeData()
        {
            string text = (PropsSpawner.saveKeysPrefix.NullOrEmpty() ? null : (PropsSpawner.saveKeysPrefix + "_"));
            Scribe_Values.Look(ref ticksUntilSpawn, text + "ticksUntilSpawn", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.defaultLabel = "DEV: Spawn " + PropsSpawner.thingToSpawn.label;
                command_Action.icon = TexCommand.DesirePower;
                command_Action.action = delegate
                {
                    ResetCountdown();
                    TryDoSpawn();
                };
                yield return command_Action;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (PropsSpawner.writeTimeLeftToSpawn && (!PropsSpawner.requiresPower || PowerOn))
            {
                return "NextSpawnedItemIn".Translate(GenLabel.ThingLabel(PropsSpawner.thingToSpawn, null, PropsSpawner.spawnCount)).Resolve() + ": " + ticksUntilSpawn.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor);
            }

            return null;
        }
    }
}