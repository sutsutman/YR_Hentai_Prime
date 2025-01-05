using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_SpawnBed : CompProperties
    {
        public ThingDef bedDef;

        public CompProperties_SpawnBed() => compClass = typeof(CompSpawnBed);
    }

    public class CompSpawnBed : ThingComp
    {
        private Building_AnimationBed bed = null;

        public CompProperties_SpawnBed Props => (CompProperties_SpawnBed)props;

        public bool spawn = true;
        public override void PostExposeData()
        {
            Scribe_References.Look(ref bed, "bed");
            Scribe_Values.Look(ref spawn, "spawn");
        }

        public override void CompTick()
        {
            base.CompTick();

            if(spawn)
            {
                spawn = false;
                CheckBed();
            }
        }

        public void CheckBed()
        {
            if (Props.bedDef == null)
            {
                Log.Error("CompSpawnBed: bedDef is null in CompProperties_SpawnBed.");
                return;
            }

            if (parent?.Map == null)
            {
                Log.Error("CompSpawnBed: parent or parent.Map is null.");
                return;
            }

            if (bed == null || bed.Destroyed)
            {
                bed = FindBed();
                if (bed == null)
                {
                    var pos = parent.Position;
                    pos.z--;
                    bed = GenSpawn.Spawn(Props.bedDef, pos, parent.Map) as Building_AnimationBed;
                    if (bed != null)
                    {
                        bed.SetFaction(Faction.OfPlayer, null);

                        CompRespawnBed compRespawnBed = bed.TryGetComp<CompRespawnBed>();
                        if (compRespawnBed != null)
                        {
                            compRespawnBed.parentTree = parent as Building_Tentacle_Altar;
                        }
                    }
                    else
                    {
                        Log.Error("CompSpawnBed: Failed to spawn bed.");
                    }
                }
                else
                {
                    CompRespawnBed compRespawnBed = bed.TryGetComp<CompRespawnBed>();
                    if (compRespawnBed != null)
                    {
                        compRespawnBed.parentTree = parent as Building_Tentacle_Altar;
                    }
                }
            }
        }


        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            if (bed != null && !bed.Destroyed)
            {
                bed.Destroy();
                bed = null;
            }
        }


        private Building_AnimationBed FindBed()
        {
            if (parent == null || parent.Map == null)
            {
                Log.Error("CompSpawnBed: parent or parent.Map is null in FindBed.");
                return null;
            }

            List<Thing> things = parent.Map.thingGrid.ThingsListAt(parent.Position);
            foreach (Thing thing in things)
            {
                if (thing is Building_AnimationBed bondageBed && bondageBed.def == Props.bedDef)
                {
                    return bondageBed;
                }
            }
            return null;
        }

    }
}
