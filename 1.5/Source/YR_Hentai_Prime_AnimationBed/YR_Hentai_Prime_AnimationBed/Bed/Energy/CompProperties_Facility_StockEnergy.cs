using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // Facility 속성 정의
    public class CompProperties_Facility_StockEnergy : CompProperties_Facility
    {
        // 생성자: 해당 컴포넌트를 CompFacility_StockEnergy로 연결
        public CompProperties_Facility_StockEnergy() => compClass = typeof(CompFacility_StockEnergy);

        // 저장 가능한 최대 에너지 양
        public int stockableEnergy = 1000;
    }

    // Facility 컴포넌트
    public class CompFacility_StockEnergy : CompFacility
    {
        // 속성 접근자
        public new CompProperties_Facility_StockEnergy Props => (CompProperties_Facility_StockEnergy)props;

        // 저장된 에너지 양
        private int stockedEnergy = 0;

        // 에너지 확인 및 작업 간격 (틱 단위)
        private int ticks = 100;

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // 작업 간격을 줄이고 실행 조건 확인
            ticks--;
            if (ticks <= 0)
            {
                ticks = 100; // 작업 간격 초기화

                // 저장 가능한 최대 에너지 이하일 경우 처리
                if (stockedEnergy < Props.stockableEnergy)
                {
                    // 연결된 빌딩 순회
                    foreach (var linkedBuilding in LinkedBuildings)
                    {
                        // 연결된 컴포넌트에서 에너지 확인
                        var comp = linkedBuilding.TryGetComp<CompAffectedByStockEnergy>();
                        if (comp != null && comp.energyMaked)
                        {
                            // 에너지 전송
                            comp.energyMaked = false;
                            comp.ticksToMakeEnergy = comp.Props.ticksToMakeEnergy;
                            stockedEnergy += comp.Props.makeEnergy;

                            // 최대 저장량 초과 방지
                            if (stockedEnergy >= Props.stockableEnergy)
                            {
                                stockedEnergy = Props.stockableEnergy;
                                break; // 반복 종료
                            }
                        }
                    }
                }
            }
        }

        // 검사 창에 표시할 추가 정보
        public override string CompInspectStringExtra()
        {
            // 저장된 에너지와 최대 저장량 표시
            return "YR_StockedEnergy".Translate() + $" : {stockedEnergy} / {Props.stockableEnergy}";
        }

        // 저장 및 로드 지원
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref stockedEnergy, "stockedEnergy"); // 저장된 에너지 값 저장/로드
        }
    }
}
