using System.Collections.Generic;
using Verse;
using Keyframe = Verse.Keyframe;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationKeyframe : Keyframe
    {
        public List<SoundSettingDef> soundSettingDefs = new List<SoundSettingDef>();
        public string texPath;
    }


    public class AnimationWorker_KeyframesBedAnimation : AnimationWorker_Keyframes
    {
        public AnimationWorker_KeyframesBedAnimation(AnimationDef def, Pawn pawn, AnimationPart part, PawnRenderNode node) : base(def, pawn, part, node)
        {
        }
    }
}
