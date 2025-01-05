using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class ThingComp_FemaleGender : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn = (parent as Pawn);
            Pawn.gender = Gender.Female;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            Pawn = (parent as Pawn);
            Pawn.gender = Gender.Female;
        }

        public override void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
        {
            base.PostPostGeneratedForTrader(trader, forTile, forFaction);
            Pawn = (parent as Pawn);
            Pawn.gender = Gender.Female;
        }
        public Pawn Pawn { get; private set; }
    }

    public class CompProperties_FemaleGender : CompProperties
    {
        public CompProperties_FemaleGender() => this.compClass = typeof(ThingComp_FemaleGender);
    }
}
