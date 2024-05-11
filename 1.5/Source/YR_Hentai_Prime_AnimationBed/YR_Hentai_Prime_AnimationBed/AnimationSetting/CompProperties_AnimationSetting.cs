using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AnimationSetting : CompProperties
    {
        public CompProperties_AnimationSetting() => compClass = typeof(CompAnimationSetting);
        public PawnAnimationSetting pawnAnimationSetting = new PawnAnimationSetting();

        public List<BedAnimation> bedAnimationList = new List<BedAnimation>();
    }
    public class PawnAnimationSetting
    {
        public AnimationDef pawnAnimationDef;
        public List<ConditonPawnAnimation> conditonPawnAnimations = new List<ConditonPawnAnimation>();

        public Vector3 offset;
        public List<ConditonPawnOffset> conditonPawnOffsets = new List<ConditonPawnOffset>();
    }

    public class ConditonPawnAnimation
    {
        public Condition condition;
        public AnimationDef pawnAnimationDef;
    }
    public class ConditonPawnOffset
    {
        public Condition condition;
        public Vector3 offset;
    }

    public class BedAnimation
    {
        public BedAnimationDef bedAnimationDef;

        //public PawnRenderNodeTagDef animationPartKeySynchro;

        public List<ConditionBedAnimation> conditionBedAnimationDefs = new List<ConditionBedAnimation>();
    }

    public class ConditionBedAnimation
    {
        public Condition condition;

        public BedAnimationDef bedAnimationDef;
    }
}
