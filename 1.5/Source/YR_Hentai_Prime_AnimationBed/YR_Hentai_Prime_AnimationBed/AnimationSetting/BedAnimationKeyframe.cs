//using System.Collections.Generic;
//using Verse;
//using Keyframe = Verse.Keyframe;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class BedAnimationKeyframe : Keyframe
//    {
//        public List<TextureReplaceSetting> textureReplaceSettings = new List<TextureReplaceSetting>();
//    }

//    public class TextureReplaceSetting
//    {
//        public Condition condition;
//        public string textureReplacePath = "";
//    }

//    public class AnimationWorker_KeyframesBedAnimation : AnimationWorker_Keyframes
//    {
//        public AnimationWorker_KeyframesBedAnimation(AnimationDef def, Pawn pawn, AnimationPart part, PawnRenderNode node) : base(def, pawn, part, node)
//        {
//        }

//        public string TexPathAtTick(int tick)
//        {
//            if (tick <= part.keyframes[0].tick)
//            {
//                return ChoiceTextureReplacePath(part.keyframes[0]);
//            }

//            if (tick >= part.keyframes[part.keyframes.Count - 1].tick)
//            {
//                return ChoiceTextureReplacePath(part.keyframes[part.keyframes.Count - 1]);
//            }

//            return null;
//        }

//        private string ChoiceTextureReplacePath(Keyframe keyframe)
//        {
//            var BAK = keyframe as BedAnimationKeyframe;
//            string path = null;
//            foreach (var textureReplaceSetting in BAK?.textureReplaceSettings)
//            {
//                if (Condition.Match(pawn,textureReplaceSetting.condition))
//                {
//                    path = textureReplaceSetting.textureReplacePath;

//                    if (Condition.NeedBreak(textureReplaceSetting.condition))
//                    {
//                        break;
//                    }
//                }
//            }
//            return path;
//        }
//    }
//}
