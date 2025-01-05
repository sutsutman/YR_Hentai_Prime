using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // 폰이 묶이고 일정 시간이 지나면 헤디프를 부여하는 컴포넌트 속성 정의 클래스
    public class CompProperties_AddHediffAfterSomeTick : CompProperties
    {
        // 생성자에서 해당 컴포넌트 클래스를 지정
        public CompProperties_AddHediffAfterSomeTick()
        {
            compClass = typeof(Comp_AddHediffAfterSomeTick);
        }

        // 부여할 헤디프 목록
        public List<HediffDef> hediffDefs = new List<HediffDef>();

        // 틱 카운트 설정
        public int ticks = 1000;

        // 처리 후 콘텐츠 배출 여부
        public bool ejectContents = true;

        // 메시지 표시 여부 및 메시지 설명
        public bool showMessage = true;
        public string messageDesc = "Message_Comp_AddHediffAfterSomeTick";
        public string compInspectStringExtra = "CompInspectStringExtra_Comp_AddHediffAfterSomeTick";
    }

    // 일정 시간이 지나면 헤디프를 부여하는 컴포넌트 클래스
    public class Comp_AddHediffAfterSomeTick : CompBaseOfAnimationBed
    {
        // 속성 가져오기
        public CompProperties_AddHediffAfterSomeTick Props => (CompProperties_AddHediffAfterSomeTick)props;

        // 내부 틱 카운트 변수
        public int ticks = 1000;

        // 컴포넌트가 스폰된 후 초기화 메서드
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ReseTicks();
        }

        // 틱 카운트를 초기화하는 메서드
        public void ReseTicks()
        {
            ticks = Props.ticks;
        }

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // 폰이 묶여 있는 경우 틱 감소 및 헤디프 부여 처리
            if (HeldPawn != null)
            {
                ticks--;
                if (ticks <= 0)
                {
                    AddHediff();
                }
            }
            else
            {
                ReseTicks();
            }
        }

        // 헤디프를 부여하는 메서드
        private void AddHediff()
        {
            if (HeldPawn == null)
            {
                return;
            }

            // 설정된 모든 헤디프를 폰에게 부여
            foreach (var hediffDef in Props.hediffDefs)
            {
                HeldPawn.health.AddHediff(hediffDef);
            }

            // 메시지를 표시하는 옵션이 활성화된 경우 메시지 출력
            if (Props.showMessage)
            {
                Messages.Message(Props.messageDesc.Translate(HeldPawn), MessageTypeDefOf.PositiveEvent, true);
            }

            // 설정에 따라 콘텐츠 배출
            if (Props.ejectContents)
            {
                Building_AnimationBed.EjectContents();
            }

            // 틱 카운트 초기화
            ReseTicks();
        }

        // 추가 디버그 명령 버튼을 반환하는 메서드
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: Finish Process",
                    icon = TexCommand.Install,
                    action = delegate ()
                    {
                        AddHediff();
                    }
                };
            }
            yield break;
        }

        // 추가 검사 문자열을 반환하는 메서드
        public override string CompInspectStringExtra()
        {
            return HeldPawn == null
                ? ""
                : (string)Props.compInspectStringExtra.Translate(ticks.ToStringTicksToPeriod(true, false, true, true).Colorize(ColoredText.DateTimeColor));
        }

        // 데이터 저장 및 로드 메서드
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticks, "ticks", 0, false);
        }
    }
}
