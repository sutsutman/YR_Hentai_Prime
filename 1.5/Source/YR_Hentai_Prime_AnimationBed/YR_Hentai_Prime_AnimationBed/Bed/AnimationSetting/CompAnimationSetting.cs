using RimWorld;
using System.Collections.Generic;
using System.Linq;
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
        public List<FillableBarIngredient> fillableBarIngredients = new List<FillableBarIngredient>();

        public override void PostDraw()
        {
            if (HeldPawn != null)
            {
                DrawBedAnimation();
            }
        }

        public void DrawBedAnimation()
        {

            foreach (var bedAnimationSettingAndTick in bedAnimationSettingAndTicks)
            {
                int currentTick = bedAnimationSettingAndTick.currentTick;

                var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                    .Where(b => b.tick <= currentTick)
                    .OrderByDescending(b => b.tick)
                    .FirstOrDefault();

                // 설정이 없을 경우 처리
                if (closestSetting == null && (bedAnimationSettingAndTick.autoDurationTicksSetting || currentTick <= bedAnimationSettingAndTick.durationTick))
                {
                    closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                        .OrderByDescending(x => x.tick)
                        .FirstOrDefault();

                    if (closestSetting == null)
                        continue; // 설정이 없으면 다음으로 넘어감
                }

                Vector3 pos = CalculatePos(bedAnimationSettingAndTick, closestSetting);

                closestSetting.graphic?.Draw(pos, Rot4.North, Building_AnimationBed.HeldPawn);
            }

            // 포트레잇
            DrawPortrait();
            //액체 바
            DrawFillableBar();
            Building_AnimationBed.makePortrait = false;
        }

        private void DrawFillableBar()
        {
            foreach (var fillableBarIngredient in fillableBarIngredients)
            {
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = Building_AnimationBed.DrawPos + fillableBarIngredient.offset + fillableBarIngredient.testOffset;
                r.size = fillableBarIngredient.drawSize + fillableBarIngredient.testDrawSize;
                r.fillPercent = 1;
                r.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(fillableBarIngredient.color + fillableBarIngredient.testColor, false);
                r.unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0, 0, 0, 0), false);
                r.margin = 0f;
                Rot4 rotation = Rot4.South;
                rotation.Rotate(RotationDirection.Clockwise);
                r.rotation = rotation;
                GenDraw.DrawFillableBar(r);
            }
        }

        //포트레잇 그려내기
        private void DrawPortrait()
        {
            foreach (var portraitIngredient in portraitIngredients)
            {
                if (portraitIngredient.label != null)
                {
                    Log.Error(portraitIngredient.label);
                }

                Pawn pawn = portraitIngredient.pawn;
                Vector3 drawPos = Building_AnimationBed.DrawPos + portraitIngredient.offset + portraitIngredient.testOffset;
                Vector3 drawSize = new Vector3(portraitIngredient.drawSize.x + portraitIngredient.testDrawSize.x, 1, portraitIngredient.drawSize.y + portraitIngredient.testDrawSize.y);

                if (Building_AnimationBed.makePortrait)
                {
                    var portraitSetting = portraitIngredient.portraitSetting;
                    var cameraOffset = portraitIngredient.cameraOffset;

                    if (portraitSetting.animationSynchro)
                    {
                        var renderNode = pawn.Drawer.renderer.renderTree.rootNode.children
                            .FirstOrDefault(n => n?.Props?.tagDef == portraitSetting.pawnRenderNodeTagDef);

                        if (renderNode != null)
                        {
                            var offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                            cameraOffset -= offset - pivot;
                        }
                    }

                    cameraOffset += portraitIngredient.testCameraOffset;
                    //TestLog.Error($"cameraOffset : {cameraOffset.x:F5}, {cameraOffset.y:F5}, {cameraOffset.z:F5}");

                    var cameraZoom = portraitIngredient.cameraZoom + portraitIngredient.testCameraZoom;
                    var rotation = Props.pawnAnimationSetting.rotation;

                    foreach (var conditionPawnRotation in Props.pawnAnimationSetting.conditionPawnRotations)
                    {
                        if (Condition.ExecuteActionIfConditionMatches(Building_AnimationBed, conditionPawnRotation.pawnCondition,
                            () => rotation = conditionPawnRotation.rotation))
                        {
                            break;
                        }
                    }

                    portraitIngredient.iconMat.mainTexture = PortraitsCache.Get(pawn, new Vector2(256, 256), portraitSetting.rotation, cameraOffset, cameraZoom, renderClothes: portraitSetting.renderClothes, renderHeadgear: portraitSetting.renderHeadgear, stylingStation: false, healthStateOverride: PawnHealthState.Mobile);
                }

                if (pawn.Drawer.renderer.HasAnimation && pawn.Drawer.renderer.CurAnimation != YR_H_P_DefOf.YR_Global_Animation_NoMove)
                {
                    var pos = (pawn == HeldPawn) ? Building_AnimationBed.DrawPos + Building_AnimationBed.PawnDrawOffset : pawn.Drawer.DrawPos;
                    var pawnRotation = (pawn == HeldPawn) ? Props.pawnAnimationSetting.rotation : pawn.Rotation;
                    pawn.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, pos, pawnRotation, neverAimWeapon: true);
                }

                Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(portraitIngredient.angle + portraitIngredient.testAngle, Vector3.up), drawSize);
                GenDraw.DrawMeshNowOrLater(portraitIngredient.portraitMesh, matrix, portraitIngredient.iconMat, PawnRenderFlags.None.FlagSet(PawnRenderFlags.DrawNow));
            }
        }

        private Vector3 CalculatePos(BedAnimationSettingAndTick bedAnimationSettingAndTick, BedAnimationSetting closestSetting)
        {
            // 기본 위치 계산
            Vector3 pos = Building_AnimationBed.DrawPos;

            // 렌더 노드의 오프셋 계산
            if (bedAnimationSettingAndTick.parentBedAnimationDef.animationSynchro && bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef != null)
            {
                PawnRenderNode renderNode = HeldPawn.Drawer.renderer.renderTree.rootNode.children
                    .FirstOrDefault(n => n?.Props?.tagDef == bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef);

                if (renderNode != null)
                {
                    Vector3 offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                    offset -= pivot;
                    pos += offset;

                    if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
                    {
                        TestLog.Error($"pawnRenderNodeTagDef offset : {offset.x:F5}, {offset.y:F5}, {offset.z:F5}");
                        TestLog.Error($"pawnRenderNodeTagDef pivot : {pivot.x:F5}, {pivot.y:F5}, {pivot.z:F5}");
                    }
                }
            }

            else if (bedAnimationSettingAndTick.parentBedAnimationDef.animationSynchrotoDummyForJoyAnimation && bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef != null && Building_AnimationBed.dummyForJoyIsActive && Building_AnimationBed.dummyForJoyPawn != null)
            {
                var dummyPawn = Building_AnimationBed.dummyForJoyPawn;
                PawnRenderNode renderNode = dummyPawn.Drawer.renderer.renderTree.rootNode.children
                    .FirstOrDefault(n => n?.Props?.tagDef == bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef);

                if (renderNode != null)
                {
                    Vector3 offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                    offset -= pivot;
                    pos += offset;

                    if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
                    {
                        Log.Error($"pawnRenderNodeTagDef offset : {offset.x:F5}, {offset.y:F5}, {offset.z:F5}");
                        Log.Error($"pawnRenderNodeTagDef pivot : {pivot.x:F5}, {pivot.y:F5}, {pivot.z:F5}");
                    }
                }
            }

            // 테스트용 오프셋 추가
            pos.y = Building_AnimationBed.DrawPos.y;
            pos += closestSetting.offset;
            pos += closestSetting.testOffset;
            if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
            {
                Log.Error($"pos : {pos:F10}");
            }
            return pos;
        }
    }

    public class FillableBarIngredient
    {
        public Pawn pawn;

        public Vector3 offset;
        public Vector2 drawSize;
        public Color color;

        public bool openTestGizmo;

        public Vector3 testOffset;
        public Vector2 testDrawSize;
        public Color testColor;
    }


    public class PortraitIngredient
    {
        public string label;
        public Pawn pawn;
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
        public PawnCondition pawnCondition;
        public GraphicData graphicData;
        public List<ConditionGraphicData> conditionGraphicDatas = new List<ConditionGraphicData>();
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

    public class ConditionGraphicData
    {
        public PawnCondition pawnCondition;

        public GraphicData graphicData;
    }
}
