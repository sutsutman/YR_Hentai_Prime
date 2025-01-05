using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class SoundSettingDef : Def
    {
        public List<SoundDef> soundDefs = new List<SoundDef>();
        public float probability = 10f;
        public List<ConditionSoundSetting> conditionSoundSettings = new List<ConditionSoundSetting>();
    }
    public class ConditionSoundSetting
    {
        public List<SoundDef> soundDefs = new List<SoundDef>();
        public float probability = 10f;
        public PawnCondition pawnCondition;
    }
}
