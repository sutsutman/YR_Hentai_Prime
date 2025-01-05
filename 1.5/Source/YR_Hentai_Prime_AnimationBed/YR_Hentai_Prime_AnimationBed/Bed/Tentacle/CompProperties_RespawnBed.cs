using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_RespawnBed : CompProperties
    {
        public CompProperties_RespawnBed() => compClass = typeof(CompRespawnBed);
        public Vector2 drawsize = new Vector2(5, 5);
        public Vector3 adjustOffsetBeforeSetOccupant = new Vector3(0, 0, -1);
    }
    public class CompRespawnBed : ThingComp
    {
        public Building_Tentacle_Altar parentTree = null;
        public bool parentTreeDestroyed = false;
        public CompProperties_RespawnBed Props => (CompProperties_RespawnBed)props;

        public override void PostDeSpawn(Map map)
        {
            if (!parentTreeDestroyed && !parentTree.DestroyedOrNull())
            {
                CompSpawnBed compSpawnBed = parentTree.TryGetComp<CompSpawnBed>();
                compSpawnBed?.CheckBed();
            }
        }
    }
}