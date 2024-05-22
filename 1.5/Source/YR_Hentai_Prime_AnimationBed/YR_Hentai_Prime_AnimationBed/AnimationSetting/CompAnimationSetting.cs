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

            Building_AnimationBed.makePortrait = false;
        }
        private void DrawPortrait()
        {
            foreach (var portraitIngredient in portraitIngredients)
            {
                Vector3 drawPos = Building_AnimationBed.DrawPos + portraitIngredient.offset + portraitIngredient.testOffset;
                //안나오던건 Vector3 drawsize 변환 문제!!!!!!
                Vector3 drawSize = new Vector3(portraitIngredient.drawSize.x + portraitIngredient.testDrawSize.x, 1, portraitIngredient.drawSize.y + portraitIngredient.testDrawSize.y);
                Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.AngleAxis(portraitIngredient.angle + portraitIngredient.testAngle, Vector3.up), drawSize);

                //1틱당 한번 계산 해서 부담 줄이기
                if (Building_AnimationBed.makePortrait)
                {
                    var portraitSetting = portraitIngredient.portraitSetting;
                    var cameraOffset = portraitIngredient.cameraOffset;
                    if (portraitSetting.animationSynchro)
                    {
                        PawnRenderNode renderNode = Building_AnimationBed.HeldPawn.Drawer.renderer.renderTree.rootNode.children
                        .FirstOrDefault(n => n?.Props?.tagDef == portraitSetting.pawnRenderNodeTagDef);

                        if (renderNode != null)
                        {
                            Vector3 offset = renderNode.Worker.OffsetFor(renderNode, Building_AnimationBed.HeldPawnDrawParms, out var pivot);
                            offset -= pivot;

                            cameraOffset -= offset;
                        }
                    }
                    cameraOffset += portraitIngredient.testCameraOffset;

                    TestLog.Error($"cameraOffset : {cameraOffset.x:F5}, {cameraOffset.y:F5}, {cameraOffset.z:F5}");

                    var cameraZoom = portraitIngredient.cameraZoom + portraitIngredient.testCameraZoom;

                    Rot4 rotation = Props.pawnAnimationSetting.rotation;

                    foreach (var conditionPawnRotation in Props.pawnAnimationSetting.conditionPawnRotations)
                    {
                        void action() => rotation = conditionPawnRotation.rotation;

                        if (Condition.ExecuteActionIfConditionMatches(HeldPawn, Building_AnimationBed, conditionPawnRotation.condition, action))
                        {
                            break;
                        }
                    }

                    portraitIngredient.iconMat.mainTexture = PortraitsCache.Get(Building_AnimationBed.HeldPawn, new Vector2(256, 256), portraitSetting.rotation, cameraOffset, cameraZoom, renderClothes: portraitSetting.renderClothes, renderHeadgear: portraitSetting.renderHeadgear, stylingStation: false, healthStateOverride: PawnHealthState.Mobile);
                }

                //포트레잇 카메라 오프셋이 바뀌면 폰이 순간 깜빡거리는 문제가 있어서, 그러한 포트레잇이 있을때 용으로 넣어둠.
                if (HeldPawn.Drawer.renderer.HasAnimation && HeldPawn.Drawer.renderer.CurAnimation != YR_H_P_DefOf.YR_Dummy_Animation)
                {
                    var pos = Building_AnimationBed.DrawPos + Building_AnimationBed.PawnDrawOffset;
                    Rot4 pawnRotation = Props.pawnAnimationSetting.rotation;
                    HeldPawn.Drawer.renderer.DynamicDrawPhaseAt(DrawPhase.Draw, pos, pawnRotation, neverAimWeapon: true);
                }

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
        public Condition condition;

        public GraphicData graphicData;
    }
}