using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AffectedByStockEnergy : CompProperties
    {
        // 기본 생성자: 해당 컴포넌트를 CompAffectedByStockEnergy로 연결
        public CompProperties_AffectedByStockEnergy() => compClass = typeof(CompAffectedByStockEnergy);

        // 에너지 생성에 필요한 틱 수
        public int ticksToMakeEnergy = 2000;

        // 생성할 에너지 양
        public int makeEnergy = 1;

        // Joy Pawn이 활성화된 경우만 작동 여부
        public bool dummyForJoyIsActive = true;
    }

    [StaticConstructorOnStartup]
    public class CompAffectedByStockEnergy : CompBaseOfAnimationBed
    {
        // 속성 정보에 접근하기 위한 Props
        public CompProperties_AffectedByStockEnergy Props => (CompProperties_AffectedByStockEnergy)props;

        // Affect by Facilities 컴포넌트 참조
        public CompAffectedByFacilities AffectedByFacilitiesComp => parent.TryGetComp<CompAffectedByFacilities>();

        // 에너지 생성까지 남은 틱 수
        public int ticksToMakeEnergy = 100;

        // 에너지가 생성되었는지 여부
        public bool energyMaked = true;

        // 생성 시 호출: 속성값 초기화
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksToMakeEnergy = Props.ticksToMakeEnergy; // 속성에서 틱 값 가져오기
        }

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // Pawn이 침대에 놓여 있고, 침대가 작동 중인지 확인
            if (HeldPawn != null && Building_AnimationBed.PowerOn)
            {
                // 조건에 따라 에너지 생성 틱 감소
                if (Props.dummyForJoyIsActive)
                {
                    // Joy Pawn이 활성화된 상태에서 작동
                    if (Building_AnimationBed.dummyForJoyIsActive && ticksToMakeEnergy > 0)
                    {
                        ticksToMakeEnergy--;
                    }
                }
                else
                {
                    // Joy Pawn이 활성화 상태와 상관없이 작동
                    if (ticksToMakeEnergy > 0)
                    {
                        ticksToMakeEnergy--;
                    }
                }
            }

            // 에너지 생성 완료 여부 갱신
            if (ticksToMakeEnergy <= 0)
            {
                energyMaked = true;
            }
        }

        // 검사 창에 표시할 추가 정보
        public override string CompInspectStringExtra()
        {
            // 에너지 생성 정보와 남은 시간 표시
            return "YR_EnergyMake".Translate() + $" : {Props.makeEnergy}" + "\n" +
                   $"YR_TicksToMakeEnergy".Translate() + " : " + ticksToMakeEnergy.TicksToSeconds().ToString("F2") + "s";
        }

        // 저장 및 로드 지원
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToMakeEnergy, "ticksToMakeEnergy"); // 남은 틱 수 저장
            Scribe_Values.Look(ref energyMaked, "energyMaked"); // 에너지 생성 상태 저장
        }
    }
}
