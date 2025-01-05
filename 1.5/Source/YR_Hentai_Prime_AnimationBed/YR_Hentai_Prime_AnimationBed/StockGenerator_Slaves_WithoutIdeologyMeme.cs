using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class StockGenerator_Slaves_WithoutIdeologyMeme : StockGenerator_Slaves
    {
        public int minAge = 18;
        public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
        {
            if (respectPopulationIntent && Rand.Value > StorytellerUtilityPopulation.PopulationIntent)
            {
                yield break;
            }
            int count = countRange.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                if (!(from fac in Find.FactionManager.AllFactionsVisible
                      where fac != Faction.OfPlayer && fac.def.humanlikeFaction && !fac.temporary
                      select fac).TryRandomElement(out Faction faction2))
                {
                    yield break;
                }
                DevelopmentalStage developmentalStages = DevelopmentalStage.Adult;
                PawnGenerationRequest request = new PawnGenerationRequest(slaveKindDef ?? PawnKindDefOf.Slave, faction2, PawnGenerationContext.NonPlayer, forTile, false, false, false, true, false, 1f, !trader.orbital, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, developmentalStages, null, null, null, false);

                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn.ageTracker.AgeBiologicalYears < minAge)
                {
                    long ageAdd = (long)((minAge / 3600000f) - pawn.ageTracker.AgeBiologicalTicks);
                    pawn.ageTracker.AgeBiologicalTicks += ageAdd;
                }
                yield return pawn;
            }
        }

        private readonly bool respectPopulationIntent;
    }
}
