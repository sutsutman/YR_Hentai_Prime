using Verse;

namespace YR_Hentai_Prime_AnimationBed
{

    public class HediffCompProperties_Pregnant_Tentacle_Birth : HediffCompProperties
    {
        public HediffCompProperties_Pregnant_Tentacle_Birth() => compClass = typeof(HediffComp_Pregnant_Tentacle_Birth);

        public ThingDef thingDef;
        public int spawnCount = 1;
        public SoundSettingDef femaleEroVoice;
        public int ticks = 6000;
    }


    public class HediffComp_Pregnant_Tentacle_Birth : HediffComp
    {
        public HediffCompProperties_Pregnant_Tentacle_Birth Props => (HediffCompProperties_Pregnant_Tentacle_Birth)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
        }
    }
}
