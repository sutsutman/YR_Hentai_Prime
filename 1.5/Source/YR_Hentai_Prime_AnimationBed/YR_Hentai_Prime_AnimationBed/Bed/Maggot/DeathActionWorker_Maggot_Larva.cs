using Verse;
using Verse.AI.Group;

namespace YR_Hentai_Prime_AnimationBed
{
    public class DeathActionWorker_Maggot_Larva : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            var position = corpse.Position;
            var map = corpse.Map;
            corpse.DeSpawn();

            var thing = ThingMaker.MakeThing(YR_H_P_DefOf.YR_Maggot_Larva_CorpseMeat);
            thing.stackCount = 30;
            GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near, null, null, default);
        }
    }
}
