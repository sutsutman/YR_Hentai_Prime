using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_SpawnBed : CompProperties
    {
        public ThingDef bedDef;

        public CompProperties_SpawnBed()
        {
            compClass = typeof(CompSpawnBed);
        }
    }

    public class CompSpawnBed : ThingComp
    {
        private Building_AnimationBed bed = null;

        public CompProperties_SpawnBed Props => (CompProperties_SpawnBed)props;

        public override void PostExposeData()
        {
            Scribe_References.Look(ref bed, "bed");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CheckBed();
        }

        public void CheckBed()
        {
            if (Props.bedDef != null && (bed == null || bed.Destroyed))
            {
                bed = FindBed();
                if (bed == null)
                {
                    bed = GenSpawn.Spawn(Props.bedDef, parent.Position, parent.Map) as Building_AnimationBed;
                    bed.SetFaction(Faction.OfPlayer, null);
                    bed.Rotation = Rot4.South;
                    CompRespawnBed compRespawnBed = bed.TryGetComp<CompRespawnBed>();
                    if (compRespawnBed != null)
                    {
                        compRespawnBed.parentTree = (Building_Tentacle_Altar)parent;
                    }
                }
                else
                {
                    CompRespawnBed compRespawnBed = bed.TryGetComp<CompRespawnBed>();
                    if (compRespawnBed != null)
                    {
                        compRespawnBed.parentTree = (Building_Tentacle_Altar)parent;
                    }
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (bed != null && !bed.Destroyed)
            {
                bed.Destroy();
            }
        }

        private Building_AnimationBed FindBed()
        {
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
