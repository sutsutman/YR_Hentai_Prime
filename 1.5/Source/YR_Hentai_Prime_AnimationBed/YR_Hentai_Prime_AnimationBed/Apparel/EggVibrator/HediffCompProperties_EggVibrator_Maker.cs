using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // Hediff 컴포넌트 속성 정의
    public class HediffCompProperties_EggVibrator_Maker : HediffCompProperties
    {
        public HediffCompProperties_EggVibrator_Maker()
        {
            compClass = typeof(HediffComp_EggVibrator_Maker);
        }

        public HediffDef hediffDef; // 추가할 Hediff 정의
        public int ticks = 600;     // 효과 발생 주기(틱 단위)
        public int rand = 50;       // 효과 발생 확률(%)
    }

    // Hediff 컴포넌트 동작 정의
    public class HediffComp_EggVibrator_Maker : HediffComp
    {
        private int ticks = 0;

        // 속성 접근자
        public HediffCompProperties_EggVibrator_Maker Props => (HediffCompProperties_EggVibrator_Maker)props;

        // 매 틱마다 호출되는 메서드
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticks--;

            // 설정된 주기마다 효과 발생
            if (ticks <= 0)
            {
                ticks = Props.ticks;

                // Pawn이 존재할 경우 효과 발생
                if (parent.pawn != null)
                {
                    // 랜덤 값 생성
                    System.Random rand = new System.Random();
                    int num = rand.Next(0, 101);

                    // 설정된 확률에 따라 Hediff 추가
                    if (num <= Props.rand)
                    {
                        parent.pawn.health.AddHediff(Props.hediffDef);
                    }
                }
            }
        }

        // 데이터 저장/로드 처리
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0, false);
        }
    }
}
