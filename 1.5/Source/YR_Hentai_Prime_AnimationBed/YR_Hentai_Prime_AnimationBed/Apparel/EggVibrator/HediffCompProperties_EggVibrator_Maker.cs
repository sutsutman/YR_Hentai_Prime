using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_EggVibrator_Maker : HediffCompProperties
    {
        public HediffCompProperties_EggVibrator_Maker()
        {
            compClass = typeof(HediffComp_EggVibrator_Maker);
        }
        public HediffDef hediffDef;

        public int ticks = 600;

        public int rand = 50;

    }

    public class HediffComp_EggVibrator_Maker : HediffComp
    {
        private int ticks = 0;
        public HediffCompProperties_EggVibrator_Maker Props => (HediffCompProperties_EggVibrator_Maker)props;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticks--;
            if (ticks <= 0)
            {
                ticks = Props.ticks;
                if (parent.pawn != null)
                {
                    System.Random rand = new System.Random();
                    int num = rand.Next(0, 101);
                    if (num <= Props.rand)
                    {
                        parent.pawn.health.AddHediff(Props.hediffDef);
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            Scribe_Values.Look(ref ticks, "ticks", 0, false);
        }
    }
}
