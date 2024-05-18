﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompAnimationSetting : CompBaseOfAnimationBed
    {
        public CompProperties_AnimationSetting Props => (CompProperties_AnimationSetting)props;

        public List<BedAnimationSettingAndTick> bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

        public bool needMakeGraphics = true;

        public List<PortraitIngredient> portraitIngredients = new List<PortraitIngredient>();
    }


    public class PortraitIngredient
    {
        public Material iconMat;
        public Mesh portraitMesh;

        public Vector3 offset;
        public float angle;
        public Vector2 drawSize;
        public Vector3 cameraOffset;
        public float cameraZoom;

        public PortraitSetting portraitSetting;

        public bool openTestGizmo;

        public Vector3 testOffset;
        public float testAngle;
        public Vector2 testDrawSize;
        public Vector3 testCameraOffset;
        public float testCameraZoom;
    }

    public class BedAnimationSettingAndTick
    {
        public List<BedAnimationSetting> bedAnimationSettings = new List<BedAnimationSetting>();

        public int durationTick;

        public int currentTick;

        public bool autoDurationTicksSetting = false;

        public BedAnimationDef parentBedAnimationDef;

        public bool openTestGizmo = false;

    }

    public class BedAnimationSetting
    {
        public Condition condition;
        public GraphicData graphicData;
        public string maskPathForPortrait;
        public Graphic graphic;
        public int tick;
        public Vector3 offset;
        public Rot4? rotation;
        public SoundDef soundDef;
        public int angle;
        //모종의 이유로 안하고 싶으면 xml에서 별도 조정
        //public bool animationSynchro;
        //public PawnRenderNodeTagDef pawnRenderNodeTagDef;
        public bool setPawnColor;
        public BedAnimationDef parentBedAnimationDef;

        public Vector3 testOffset = new Vector3();
        public Vector2 testDrawSize;

        public BedAnimationSetting() => graphic = new Graphic();

        public BedAnimationSetting Copy()
        {
            var copied = (BedAnimationSetting)this.MemberwiseClone();
            copied.graphic = new Graphic();
            return copied;
        }
    }
}