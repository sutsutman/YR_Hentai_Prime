using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_ForcePlay : CompProperties
    {
        public CompProperties_ForcePlay() => compClass = typeof(CompForcePlay);

        public JobDef jobDef;
    }


    public class CompForcePlay : CompBaseOfAnimationBed
    {
        public CompProperties_ForcePlay Props => (CompProperties_ForcePlay)props;
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            // 특정 건물을 우클릭했을 때의 동작 구현
            void action()
            {
                // 강제로 실행할 jobdef 설정
                JobDef jobDef = Props.jobDef;

                // Pawn에게 해당 job을 강제로 할당
                selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(Props.jobDef, parent, parent.InteractionCell));
            }

            // FloatMenuOption 생성 및 추가
            yield return new FloatMenuOption("YR_ForcePlay".Translate(selPawn), action);
        }
    }
}
