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
        public List<FillableBarSetting> fillableBarSettings = new List<FillableBarSetting>();

        public GraphicData bedHeldPawnGraphicData;
    }

    public class FillableBarSetting
    {
        public PawnCondition pawnCondition;
        public Vector2 drawSize = Vector2.one;
        public Vector3 offset;
        public Color color = Color.white;
    }

    public class PawnPortraitSetting
    {
        public PortraitSetting portraitSetting = new PortraitSetting();

        public List<ConditonPortraitSetting> conditonPortraitSettings = new List<ConditonPortraitSetting>();
    }

    public class PortraitSetting
    {
        public string label;
        public bool drawJoyPawn = false;
        public GraphicData portraitMeshGraphicData;
        public Vector2 drawSize = Vector2.one;
        public Vector3 offset;
        public string maskPath;
        public Rot4 rotation = Rot4.South;
        public List<ConditionCameraOffset> conditionCameraOffsets;
        public float cameraZoom;
        public bool renderClothes;
        public bool renderHeadgear;
        public float angle;
        public bool draw = true;

        public PawnRenderNodeTagDef pawnRenderNodeTagDef;
        public bool animationSynchro;

        public PawnCondition visibleCondition;
    }

    public class ConditionCameraOffset
    {
        public PawnCondition pawnCondition;
        public Vector3 cameraOffset;
    }

    public class ConditonPortraitSetting
    {
        public PortraitSetting portraitSetting;
        public PawnCondition pawnCondition;
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

        public bool renderClothes = false;
        public bool renderHeadgear = false;
    }

    public class ConditionPawnRotation
    {
        public PawnCondition pawnCondition;
        public Rot4 rotation = Rot4.South;
    }

    public class ConditonPawnAnimation
    {
        public PawnCondition pawnCondition;
        public AnimationDef pawnAnimationDef;
    }
    public class ConditonPawnOffset
    {
        public PawnCondition pawnCondition;
        public Vector3 offset;
    }

    public class BedAnimation
    {
        public BedAnimationDef bedAnimationDef;
        public Vector3 offset;

        //public PawnRenderNodeTagDef animationPartKeySynchro;

        public List<ConditionBedAnimation> conditionBedAnimationDefs = new List<ConditionBedAnimation>();
        public Vector2 drawSize;
    }

    public class ConditionBedAnimation
    {
        public PawnCondition pawnCondition;

        public BedAnimationDef bedAnimationDef;

        public Vector3 offset;
        public Vector2 drawSize;
    }
}
