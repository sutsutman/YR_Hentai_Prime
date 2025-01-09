using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // 일정 틱이 경과하면 특정 헤디프를 부여하는 컴포넌트의 속성 정의 클래스
    public class HediffCompProperties_MaggotToxic : HediffCompProperties_SeverityPerDay
    {
        // 생성자에서 해당 컴포넌트 클래스를 지정
        public HediffCompProperties_MaggotToxic() => compClass = typeof(HediffComp_MaggotToxic);
    }

    // 일정 틱마다 헤디프를 부여하는 컴포넌트 클래스
    public class HediffComp_MaggotToxic : HediffComp_SeverityPerDay
    {
        // 매 틱마다 호출되는 메서드
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.holdingOwner?.Owner is Building_AnimationBed)
            {
                parent.Severity = 1f;
            }
        }
    }
}
