using System.Collections.Generic;
using Verse;
using Keyframe = Verse.Keyframe;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationKeyframe : Keyframe
    {
        public List<SoundSetting> soundSettings = new List<SoundSetting>();
    }

    public class SoundSetting
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

    public class AnimationWorker_KeyframesBedAnimation : AnimationWorker_Keyframes
    {
        public AnimationWorker_KeyframesBedAnimation(AnimationDef def, Pawn pawn, AnimationPart part, PawnRenderNode node) : base(def, pawn, part, node)
        {
        }
    }
}
