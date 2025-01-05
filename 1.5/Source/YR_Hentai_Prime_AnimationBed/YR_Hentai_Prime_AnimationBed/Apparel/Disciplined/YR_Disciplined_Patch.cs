// RimWorld 및 Verse 네임스페이스 사용
using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // 애플리케이션 시작 시 자동으로 실행되는 정적 클래스
    [StaticConstructorOnStartup]
    public static class YR_Disciplined_Patch
    {
        // 정적 생성자: 애플리케이션 초기화 시 한 번 실행
        static YR_Disciplined_Patch()
        {
            // 모든 ThoughtDef을 순회하며 조건에 따라 패치 적용
            foreach (ThoughtDef thoughtDef in DefDatabase<ThoughtDef>.AllDefsListForReading)
            {
                // nullifyingTraits가 null이 아닌 경우에만 처리
                if (thoughtDef.nullifyingTraits != null)
                {
                    // Nudist 특성이 포함된 경우 YR_Disciplined 특성을 추가
                    if (thoughtDef.nullifyingTraits.Contains(TraitDefOf.Nudist))
                    {
                        thoughtDef.nullifyingTraits.Add(YR_H_P_DefOf.YR_Disciplined);
                    }
                }
            }
        }
    }
}
