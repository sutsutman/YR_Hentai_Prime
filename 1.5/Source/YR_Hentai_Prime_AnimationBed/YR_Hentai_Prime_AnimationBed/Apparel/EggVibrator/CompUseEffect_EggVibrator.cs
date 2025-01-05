// RimWorld 및 Verse 네임스페이스 사용
using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // EggVibrator 아이템을 사용할 때의 효과를 정의하는 클래스
    public class CompUseEffect_EggVibrator : CompUseEffect
    {
        // 아이템 사용 시 호출되는 메서드
        public override void DoEffect(Pawn usedBy)
        {
            // 기본 효과 호출
            base.DoEffect(usedBy);

            // 현재 아이템(Thing)을 가져옴
            Thing thing = parent;

            // Pawn이 해당 아이템을 장착함
            usedBy.apparel.Wear((Apparel)thing);

            // Pawn에 대한 알림이 필요한 경우 메시지 출력
            if (PawnUtility.ShouldSendNotificationAbout(usedBy))
            {
                // 메시지를 사용자에게 출력 (긍정적인 이벤트로 표시)
                Messages.Message(
                    "CompUseEffect_EggVibrator_String".Translate(usedBy, thing.Label),
                    usedBy,
                    MessageTypeDefOf.PositiveEvent,
                    true
                );
            }
        }
    }
}
