using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public static class YR_Hentai_Prime_AnimationBed
    {
        static YR_Hentai_Prime_AnimationBed()
        {
            var validDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(thingDef => thingDef.race != null);

            foreach (var thingDef in validDefs)
            {
                // 중복 컴포넌트 추가 방지
                bool hasAnimationBedTarget = thingDef.comps.Any(comp => comp is CompProperties_AnimationBedTarget);

                if (!hasAnimationBedTarget)
                {
                    thingDef.comps.Add(new CompProperties_AnimationBedTarget());
                }
            }
        }
    }
}
