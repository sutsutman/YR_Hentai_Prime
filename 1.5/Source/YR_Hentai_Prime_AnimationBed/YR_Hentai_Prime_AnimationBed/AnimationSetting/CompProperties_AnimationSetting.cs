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

        public List<PawnPortraitSetting> pawnPortraitSettings = new List<PawnPortraitSetting>();
    }

    public class PawnPortraitSetting
    {
        public PortraitSetting portraitSetting = new PortraitSetting();

        public List<ConditonPortraitSetting> conditonPortraitSettings = new List<ConditonPortraitSetting>();
    }

    public class PortraitSetting
    {
        public GraphicData portraitMeshGraphicData;
        public Vector2 drawSize = Vector2.one;
        public Vector3 offset;
        public string maskPath;
        public Rot4 rotation = Rot4.South;
        public Vector3 cameraOffset;
        public float cameraZoom;
        public bool renderClothes;
        public bool renderHeadgear;
        public float angle;
        public bool draw = true;

        public PawnRenderNodeTagDef pawnRenderNodeTagDef;
        public bool animationSynchro;
    }

    public class ConditonPortraitSetting
    {
        public PortraitSetting portraitSetting;
        public Condition condition;
    }

    public class PawnAnimationSetting
    {
        //애니메이션
        public AnimationDef pawnAnimationDef;
        public List<ConditonPawnAnimation> conditonPawnAnimations = new List<ConditonPawnAnimation>();

        //위치
        public Vector3 offset;
        public List<ConditonPawnOffset> conditonPawnOffsets = new List<ConditonPawnOffset>();

        //방향
        public Rot4 rotation = Rot4.South;
        public List<ConditionPawnRotation> conditionPawnRotations = new List<ConditionPawnRotation>();
    }

    public class ConditionPawnRotation
    {
        public Condition condition;
        public Rot4 rotation = Rot4.South;
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
