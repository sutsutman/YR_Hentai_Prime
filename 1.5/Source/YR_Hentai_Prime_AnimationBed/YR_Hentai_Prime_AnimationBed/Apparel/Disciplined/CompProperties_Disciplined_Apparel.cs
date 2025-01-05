// RimWorld 및 Verse 네임스페이스 사용
using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // Disciplined_Apparel용 컴포넌트 속성 정의 클래스
    public class CompProperties_Disciplined_Apparel : CompProperties
    {
        // 생성자: 관련 컴포넌트 클래스 설정
        public CompProperties_Disciplined_Apparel() => compClass = typeof(Comp_Disciplined_Apparel);

        // 추가될 Hediff 정의
        public HediffDef hediffDef;

        // Hediff의 기본 심각도 값
        public float severity = 0.01f;

        // Hediff 추가를 방지하는 Trait 정의
        public TraitDef stopAddHediffTraitDef;
    }

    // Disciplined_Apparel용 컴포넌트 클래스
    public class Comp_Disciplined_Apparel : ThingComp
    {
        // Props 속성을 통해 CompProperties_Disciplined_Apparel에 접근
        private CompProperties_Disciplined_Apparel Props => (CompProperties_Disciplined_Apparel)props;

        // 장비 착용 시 호출되는 메서드
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            // 특정 Trait이 있으면 Hediff 추가를 중단
            if (Props.stopAddHediffTraitDef != null)
            {
                foreach (Trait trait in pawn.story?.traits?.allTraits)
                {
                    if (trait.def == Props.stopAddHediffTraitDef)
                    {
                        return; // Hediff 추가하지 않음
                    }
                }
            }

            // Hediff 생성 및 추가
            Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
            hediff.Severity = Props.severity;
            pawn.health.AddHediff(hediff);
        }
    }
}
