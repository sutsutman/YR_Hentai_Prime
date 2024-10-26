using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AffectedByStockEnergy : CompProperties
    {
        public CompProperties_AffectedByStockEnergy() => compClass = typeof(CompAffectedByStockEnergy);
        public int ticksToMakeEnergy = 2000;
        public int makeEnergy = 1;
        public bool dummyForJoyIsActive = true;
    }


    [StaticConstructorOnStartup]
    public class CompAffectedByStockEnergy : CompBaseOfAnimationBed
    {
        public CompProperties_AffectedByStockEnergy Props => (CompProperties_AffectedByStockEnergy)props;


        public CompAffectedByFacilities AffectedByFacilitiesComp => parent.TryGetComp<CompAffectedByFacilities>();


        public int ticksToMakeEnergy = 100;
        public bool energyMaked = true;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksToMakeEnergy = Props.ticksToMakeEnergy;
        }
        public override void CompTick()
        {
            base.CompTick();

            if (HeldPawn != null)
            {
                if (Building_AnimationBed.PowerOn)
                {
                    if (Props.dummyForJoyIsActive && Building_AnimationBed.dummyForJoyIsActive)
                    {
                        if (ticksToMakeEnergy > 0)
                        {
                            ticksToMakeEnergy--;
                        }
                    }
                    else if (!Props.dummyForJoyIsActive)
                    {
                        if (ticksToMakeEnergy > 0)
                        {
                            ticksToMakeEnergy--;
                        }
                    }
                }
            }

            if (ticksToMakeEnergy <= 0)
            {
                energyMaked = true;
            }
            //시간 초기화는 에너지 받는 쪽에서 컨트롤
        }

        public override string CompInspectStringExtra()
        {
            return "YR_EnegyMake".Translate() + $" : {Props.makeEnergy}" + "\n" + $"YR_TicksToMakeEnergy".Translate() + " : " + (int)ticksToMakeEnergy.TicksToSeconds();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToMakeEnergy, "ticksToMakeEnergy");
            Scribe_Values.Look(ref energyMaked, "makeEnergy");

        }
    }
}
