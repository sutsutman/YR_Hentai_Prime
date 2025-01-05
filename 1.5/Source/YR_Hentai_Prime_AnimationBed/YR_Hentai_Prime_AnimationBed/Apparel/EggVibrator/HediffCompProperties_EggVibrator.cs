using HarmonyLib;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_EggVibrator : HediffCompProperties
    {
        public HediffCompProperties_EggVibrator()
        {
            compClass = typeof(HediffComp_EggVibrator);
        }
        public int ticks = 10;
        public int maxNum = 10;
        public SoundDef sound;

        public ThingDef filthDef;
        public int filthNum = 1;

        public SoundSettingDef femaleEroVoice;
        public SoundSettingDef noneEroVoice;
        public SoundSettingDef maleEroVoice;
        public int voiceProbability = 50;
    }

    public class HediffComp_EggVibrator : HediffComp
    {
        private int ticks = 0;
        private int maxNum = 0;

        private HediffCompProperties_EggVibrator Props => (HediffCompProperties_EggVibrator)props;
        public override void CompPostMake()
        {
            base.CompPostMake();
            maxNum = Props.maxNum;

            Props.sound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            ConvenientTool.ShotSexSound(Pawn, Props.femaleEroVoice, Props.maleEroVoice, Props.noneEroVoice, Props.voiceProbability);
            if (Props.filthDef != null)
            {
                ThingDef filthDef = Props.filthDef;

                int filthNum = Props.filthNum;

                Verb_EggVibrator.CreateFilth(Pawn, filthDef, filthNum);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticks--;

            if (ticks <= 0)
            {
                if (maxNum >= 0)
                {
                    maxNum--;
                    ticks = Props.ticks;

                    Traverse.Create(Pawn.Drawer).Field<JitterHandler>("jitterer").Value.AddOffset(0.07f, Rand.Range(0, 360));
                }
            }
        }
    }
}
