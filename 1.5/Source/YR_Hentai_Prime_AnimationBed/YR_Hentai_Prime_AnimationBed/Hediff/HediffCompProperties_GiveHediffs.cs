using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // 일정 틱이 경과하면 특정 헤디프를 부여하는 컴포넌트의 속성 정의 클래스
    public class HediffCompProperties_GiveHediffs : HediffCompProperties
    {
        // 생성자에서 해당 컴포넌트 클래스를 지정
        public HediffCompProperties_GiveHediffs() => compClass = typeof(HediffComp_GiveHediffs);

        // 틱 카운트 (헤디프가 부여되기 전까지의 시간)
        public int ticks = 100;

        // 헤디프의 심각도 증가값 설정
        public float severity = 0.005f;

        // 부여할 헤디프 목록
        public List<HediffDef> hediffDefs = new List<HediffDef>();

        // 헤디프 부여 후 심각도를 초기화할지 여부 설정
        public bool reset = false;
    }

    // 일정 틱마다 헤디프를 부여하는 컴포넌트 클래스
    public class HediffComp_GiveHediffs : HediffComp
    {
        // 내부 틱 카운트 변수
        private int ticks = 0;

        // 속성 가져오기
        private HediffCompProperties_GiveHediffs Props => (HediffCompProperties_GiveHediffs)props;

        // 헤디프가 추가된 후 초기 설정 메서드
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            ticks = Props.ticks;
        }

        // 매 틱마다 호출되는 메서드
        public override void CompPostTick(ref float severityAdjustment)
        {
            ticks--;

            // 틱이 0이 되면 설정된 틱 값으로 초기화하고 헤디프 부여 로직 실행
            if (ticks <= 0)
            {
                ticks = Props.ticks;

                // 이미 헤디프가 존재하는지 확인하고, 존재하면 초기 심각도로 설정 후 종료
                foreach (HediffDef hediffDef in Props.hediffDefs)
                {
                    if (Pawn.health.hediffSet.HasHediff(hediffDef))
                    {
                        parent.Severity = parent.def.initialSeverity;
                        return;
                    }
                }

                // 헤디프의 심각도 증가
                parent.Severity += Props.severity;

                // 심각도가 최대치에 도달하면 새로운 헤디프를 부여
                if (parent.Severity >= parent.def.maxSeverity)
                {
                    foreach (HediffDef hediffDef in Props.hediffDefs)
                    {
                        Pawn.health.AddHediff(hediffDef);
                    }

                    // 설정에 따라 심각도를 초기화하거나 헤디프를 제거
                    if (Props.reset)
                    {
                        parent.Severity = parent.def.initialSeverity;
                    }
                    else
                    {
                        HealthUtility.Cure(parent);
                    }
                    return;
                }
            }
        }

        // 데이터 저장 및 로드 메서드
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks");
        }
    }
}
