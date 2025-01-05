using HarmonyLib;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    // Hediff 컴포넌트 속성 정의
    public class HediffCompProperties_EggVibrator : HediffCompProperties
    {
        public HediffCompProperties_EggVibrator() => compClass = typeof(HediffComp_EggVibrator);

        public int ticks = 10;  // 효과 발생 주기(틱 단위)
        public int maxNum = 10; // 최대 효과 횟수
        public SoundDef sound;  // 재생할 사운드 정의

        public ThingDef filthDef; // 생성할 오염물 정의
        public int filthNum = 1;  // 생성할 오염물 수

        public SoundSettingDef femaleEroVoice; // 여성 음성 설정
        public SoundSettingDef noneEroVoice;   // 중성 음성 설정
        public SoundSettingDef maleEroVoice;   // 남성 음성 설정
        public int voiceProbability = 50;      // 음성 발생 확률(%)
    }

    // Hediff 컴포넌트 동작 정의
    public class HediffComp_EggVibrator : HediffComp
    {
        private int ticks = 0;
        private int maxNum = 0;

        // 속성 접근자
        private HediffCompProperties_EggVibrator Props => (HediffCompProperties_EggVibrator)props;

        // 컴포넌트 생성 시 호출
        public override void CompPostMake()
        {
            base.CompPostMake();
            maxNum = Props.maxNum;

            // 사운드 재생
            Props.sound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));

            // 음성 재생 로직 호출
            ConvenientTool.ShotSexSound(Pawn, Props.femaleEroVoice, Props.maleEroVoice, Props.noneEroVoice, Props.voiceProbability);

            // 오염물 생성
            if (Props.filthDef != null)
            {
                Verb_EggVibrator.CreateFilth(Pawn, Props.filthDef, Props.filthNum);
            }
        }

        // 매 틱마다 호출
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticks--;

            // 효과 발생 조건 확인
            if (ticks <= 0)
            {
                if (maxNum >= 0)
                {
                    maxNum--;
                    ticks = Props.ticks;

                    // Pawn의 외형에 흔들림 효과 적용
                    Traverse.Create(Pawn.Drawer).Field<JitterHandler>("jitterer").Value.AddOffset(0.07f, Rand.Range(0, 360));
                }
            }
        }
    }
}
