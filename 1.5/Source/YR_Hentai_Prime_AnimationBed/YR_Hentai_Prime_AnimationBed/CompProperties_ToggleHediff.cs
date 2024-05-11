using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_ToggleHediff : CompProperties
    {
        public CompProperties_ToggleHediff() => compClass = typeof(CompToggleHediff);

        public List<HediffDef> hediffDefs = new List<HediffDef>();
    }

    public class CompToggleHediff : ThingComp
    {
        public CompProperties_ToggleHediff Props => (CompProperties_ToggleHediff)props;

        protected Building_AnimationBed AnimationBed => (Building_AnimationBed)parent;

        public Pawn Pawn => AnimationBed?.HeldPawn;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Pawn == null)
            {
                yield break;
            }

            var hediffs = Props.hediffDefs;
            if (hediffs != null && hediffs.Count != 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "YR_ToggleHediff_Label".Translate(),
                    defaultDesc = "YR_ToggleHediff_Desc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/ToggleHediff"),
                    action = ToggleHediff
                };
            }
        }

        private void ToggleHediff()
        {
            var hediffs = Props.hediffDefs;
            if (hediffs == null || hediffs.Count == 0)
            {
                return;
            }

            // 현재 pawn이 가진 헤디프 중에 리스트에 있는 것을 찾음
            var currentHediff = Pawn.health.hediffSet.hediffs.Find(h => hediffs.Contains(h.def));

            if (currentHediff != null)
            {
                var currentIndex = hediffs.IndexOf(currentHediff.def);
                // 현재 헤디프 제거
                Pawn.health.RemoveHediff(currentHediff);

                // 리스트의 다음 헤디프를 적용, 마지막이면 적용 안 함
                if (currentIndex + 1 < hediffs.Count)
                {
                    var nextHediffDef = hediffs[currentIndex + 1];
                    Pawn.health.AddHediff(nextHediffDef);
                }
            }
            else
            {
                // 헤디프가 없다면 리스트의 첫 번째 헤디프를 부여
                Pawn.health.AddHediff(hediffs[0]);
            }

            AnimationBed.setAnimation = true;
            AnimationBed.AnimationSettingComp.needMakeGraphics = true;
        }

        public void RemoveAllHediffs(Pawn pawn)
        {
            if (pawn == null || Props.hediffDefs == null || Props.hediffDefs.Count == 0)
            {
                return;
            }

            var hediffsToRemove = pawn.health.hediffSet.hediffs
                .Where(h => Props.hediffDefs.Contains(h.def))
                .ToList(); // ToList()를 통해 루프 중에 리스트 변경을 방지

            foreach (var hediff in hediffsToRemove)
            {
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}
