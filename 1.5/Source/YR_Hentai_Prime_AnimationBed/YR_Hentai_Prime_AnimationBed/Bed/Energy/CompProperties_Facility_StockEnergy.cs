using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Facility_StockEnergy : CompProperties_Facility
    {
        public CompProperties_Facility_StockEnergy() => compClass = typeof(CompFacility_StockEnergy);

        public int stockableEnergy = 1000;
    }

    public class CompFacility_StockEnergy : CompFacility
    {
        public new CompProperties_Facility_StockEnergy Props => (CompProperties_Facility_StockEnergy)props;


        int stockedEnergy = 0;
        int ticks = 100;
        public override void CompTick()
        {
            base.CompTick();

            ticks--;
            if (ticks <= 0)
            {
                ticks = 100;
                if (stockedEnergy < Props.stockableEnergy)
                {
                    foreach (var linkedBuilding in LinkedBuildings)
                    {
                        var comp = linkedBuilding.TryGetComp<CompAffectedByStockEnergy>();
                        if (comp != null && comp.energyMaked)
                        {
                            comp.energyMaked = false;
                            comp.ticksToMakeEnergy = comp.Props.ticksToMakeEnergy;
                            stockedEnergy += comp.Props.makeEnergy;

                            if (stockedEnergy >= Props.stockableEnergy)
                            {
                                stockedEnergy = Props.stockableEnergy;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            return "YR_StockedEnergy".Translate() + $" : {stockedEnergy} / {Props.stockableEnergy}";
        }



        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref stockedEnergy, "stockedEnergy");
        }
    }
}
