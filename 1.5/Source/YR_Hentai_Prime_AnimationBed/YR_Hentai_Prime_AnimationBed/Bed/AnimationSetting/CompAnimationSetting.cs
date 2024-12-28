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

        private FillableBarRenderer fillableBarRenderer = new FillableBarRenderer();

        public override void PostDraw()
        {
            if (HeldPawn != null)
            {
                DrawBedAnimation();
            }
        }

        // 애니메이션 설정 및 그래픽 데이터를 기반으로 침대 애니메이션을 그립니다.
        private void DrawBedAnimation()
        {
            foreach (var bedAnimationSettingAndTick in bedAnimationSettingAndTicks)
            {
                int currentTick = bedAnimationSettingAndTick.currentTick;

                // 현재 Tick에 가장 가까운 애니메이션 설정 찾기
                var closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                    .Where(b => b.tick <= currentTick)
                    .OrderByDescending(b => b.tick)
                    .FirstOrDefault();

                // 설정이 없을 경우 기본 설정 사용
                if (closestSetting == null &&
                    (bedAnimationSettingAndTick.autoDurationTicksSetting || currentTick <= bedAnimationSettingAndTick.durationTick))
                {
                    closestSetting = bedAnimationSettingAndTick.bedAnimationSettings
                        .OrderByDescending(x => x.tick)
                        .FirstOrDefault();

                    if (closestSetting == null)
                        continue;
                }

                // 설정된 애니메이션의 위치 계산
                Vector3 pos = CalculatePos(bedAnimationSettingAndTick, closestSetting);

                // 그래픽을 계산된 위치에 렌더링
                closestSetting.graphic?.Draw(pos, Rot4.North, Building_AnimationBed.HeldPawn);
            }

            // 포트레잇 및 FillableBar 그리기
            DrawPortrait();
            fillableBarRenderer.Render(fillableBarIngredients, Building_AnimationBed.DrawPos);

            // 포트레잇 생성 플래그 초기화
            Building_AnimationBed.makePortrait = false;
        }

        // 포트레잇을 그리는 메서드
        private void DrawPortrait()
        {
            foreach (var portraitIngredient in portraitIngredients)
            {
                if (portraitIngredient.label != null)
                {
                    Log.Error(portraitIngredient.label);
                }

                // 위치 및 크기 계산
                Vector3 drawPos = CalculateDrawPosition(Building_AnimationBed.DrawPos, portraitIngredient.offset, portraitIngredient.testOffset);
                Vector3 drawSize = new Vector3(
                    portraitIngredient.drawSize.x + portraitIngredient.testDrawSize.x,
                    1,
                    portraitIngredient.drawSize.y + portraitIngredient.testDrawSize.y
                );

                Pawn pawn = portraitIngredient.pawn;

                if (Building_AnimationBed.makePortrait)
                {
                    var portraitSetting = portraitIngredient.portraitSetting;
                    var cameraOffset = GetCameraOffset(pawn, portraitSetting, portraitIngredient.cameraOffset + portraitIngredient.testCameraOffset);
                    var cameraZoom = portraitIngredient.cameraZoom + portraitIngredient.testCameraZoom;

                    portraitIngredient.iconMat.mainTexture = PortraitsCache.Get(
                        pawn,
                        new Vector2(256, 256),
                        portraitSetting.rotation,
                        cameraOffset,
                        cameraZoom,
                        renderClothes: portraitSetting.renderClothes,
                        renderHeadgear: portraitSetting.renderHeadgear,
                        stylingStation: false,
                        healthStateOverride: PawnHealthState.Mobile
                    );
                }

                // 애니메이션 렌더링 조건 확인 및 그리기
                if (pawn.Drawer.renderer.HasAnimation && pawn.Drawer.renderer.CurAnimation != YR_H_P_DefOf.YR_Global_Animation_NoMove)
                {
                    var pos = (pawn == HeldPawn) ? Building_AnimationBed.DrawPos + Building_AnimationBed.PawnDrawOffset : pawn.Drawer.DrawPos;
                    var pawnRotation = (pawn == HeldPawn) ? Props.pawnAnimationSetting.rotation : pawn.Rotation;

                    // Pawn의 애니메이션 그리기 (이 코드 빼면 애들 깜빡거리니 주의!)
                    pawn.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, pos, pawnRotation, neverAimWeapon: true);
                }

                Matrix4x4 matrix = Matrix4x4.TRS(
                    drawPos,
                    Quaternion.AngleAxis(portraitIngredient.angle + portraitIngredient.testAngle, Vector3.up),
                    drawSize
                );

                GenDraw.DrawMeshNowOrLater(
                    portraitIngredient.portraitMesh,
                    matrix,
                    portraitIngredient.iconMat,
                    PawnRenderFlags.None.FlagSet(PawnRenderFlags.DrawNow)
                );
            }
        }

        // BedAnimationSettingAndTick 및 가장 가까운 설정을 기반으로 애니메이션 위치를 계산
        private Vector3 CalculatePos(BedAnimationSettingAndTick bedAnimationSettingAndTick, BedAnimationSetting closestSetting)
        {
            Vector3 pos = Building_AnimationBed.DrawPos;

            if (bedAnimationSettingAndTick.parentBedAnimationDef.animationSynchro &&
                bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef != null)
            {
                var renderNode = HeldPawn.Drawer.renderer.renderTree.rootNode.children
                    .FirstOrDefault(n => n?.Props?.tagDef == bedAnimationSettingAndTick.parentBedAnimationDef.pawnRenderNodeTagDef);

                if (renderNode != null)
                {
                    Vector3 offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                    pos += offset - pivot;

                    if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
                    {
                        TestLog.Error($"RenderNode Offset: {offset}, Pivot: {pivot}");
                    }
                }
            }

            pos.y = Building_AnimationBed.DrawPos.y;
            pos += closestSetting.offset + closestSetting.testOffset;

            if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentOffset)
            {
                TestLog.Error($"Final Position: {pos}");
            }

            return pos;
        }

        // 공통 위치 계산 메서드
        private Vector3 CalculateDrawPosition(Vector3 basePos, Vector3 offset, Vector3 testOffset)
        {
            return basePos + offset + testOffset;
        }

        // 카메라 오프셋 계산
        private Vector3 GetCameraOffset(Pawn pawn, PortraitSetting setting, Vector3 baseOffset)
        {
            if (!setting.animationSynchro) return baseOffset;

            var renderNode = pawn.Drawer.renderer.renderTree.rootNode.children
                .FirstOrDefault(n => n?.Props?.tagDef == setting.pawnRenderNodeTagDef);

            if (renderNode != null)
            {
                var offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                return baseOffset - (offset - pivot);
            }

            return baseOffset;
        }
    }

    // FillableBarRenderer 클래스
    public class FillableBarRenderer
    {
        public void Render(List<FillableBarIngredient> ingredients, Vector3 basePos)
        {
            foreach (var ingredient in ingredients)
            {
                GenDraw.FillableBarRequest request = default;
                request.center = basePos + ingredient.offset + ingredient.testOffset;
                request.size = ingredient.drawSize + ingredient.testDrawSize;
                request.fillPercent = 1f;
                request.filledMat = SolidColorMaterials.SimpleSolidColorMaterial(ingredient.color + ingredient.testColor, false);
                request.unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0, 0, 0, 0), false);
                request.margin = 0f;

                // Rot4.South를 복사한 새로운 Rot4 객체 생성
                Rot4 rotation = Rot4.South;
                rotation.Rotate(RotationDirection.Clockwise); // 원본 객체를 회전

                // 회전된 Rot4 객체를 설정
                request.rotation = rotation;

                GenDraw.DrawFillableBar(request);
            }
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
