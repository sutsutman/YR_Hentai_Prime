using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class BedAnimationUtility
    {
        // 포트레잇 정보 생성
        public static void MakePortrait(Building_AnimationBed building_AnimationBed)
        {
            // 기존 포트레잇 데이터 초기화
            var portraitIngredients = building_AnimationBed.AnimationSettingComp.portraitIngredients;
            portraitIngredients.Clear();

            // 각 PawnPortraitSetting 처리
            foreach (var pawnPortraitSetting in building_AnimationBed.AnimationSettingComp.Props.pawnPortraitSettings)
            {
                // 기본 포트레잇 설정
                var portraitSetting = pawnPortraitSetting.portraitSetting;

                // 조건에 따라 포트레잇 설정 변경
                foreach (var conditionPortraitSetting in pawnPortraitSetting.conditonPortraitSettings)
                {
                    // JoyPawn(가짜 Pawn)을 사용할지, HeldPawn(실제 Pawn)을 사용할지 결정
                    var tempPawn = conditionPortraitSetting.portraitSetting.drawJoyPawn
                        ? building_AnimationBed.dummyForJoyPawn
                        : building_AnimationBed.HeldPawn;

                    if (tempPawn == null) // Pawn이 없으면 생략
                    {
                        continue;
                    }

                    // 조건이 맞으면 포트레잇 설정 변경
                    void action() => portraitSetting = conditionPortraitSetting.portraitSetting;
                    if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionPortraitSetting.pawnCondition, action))
                    {
                        break;
                    }
                }

                // 유효하지 않거나 draw가 false인 경우 스킵
                if (portraitSetting == null || !portraitSetting.draw)
                {
                    continue;
                }

                // 실제 그릴 Pawn 결정
                var drawPawn = portraitSetting.drawJoyPawn ? building_AnimationBed.dummyForJoyPawn : building_AnimationBed.HeldPawn;
                if (drawPawn == null)
                {
                    continue;
                }

                // 포트레잇 크기 설정
                var drawSize = Vector3.zero;
                void setDrawSize() => drawSize = portraitSetting.drawSize;
                Condition.ExecuteActionIfConditionMatches(building_AnimationBed, portraitSetting.visibleCondition, setDrawSize);

                // 카메라 오프셋 설정
                var cameraOffset = Vector3.zero;
                foreach (var conditionCameraOffset in portraitSetting.conditionCameraOffsets)
                {
                    void cameraOffsetAction() => cameraOffset = conditionCameraOffset.cameraOffset;
                    if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionCameraOffset.pawnCondition, cameraOffsetAction))
                    {
                        break;
                    }
                }

                // 포트레잇 재료를 생성하여 추가
                portraitIngredients.Add(new PortraitIngredient
                {
                    label = portraitSetting.label, // 포트레잇 라벨
                    pawn = drawPawn, // 그릴 Pawn
                    drawSize = drawSize, // 그릴 크기
                    angle = portraitSetting.angle, // 각도
                    offset = portraitSetting.offset, // 위치 오프셋
                    portraitMesh = portraitSetting.portraitMeshGraphicData.Graphic.MeshAt(Rot4.South), // 메쉬 정보
                    iconMat = CreateMaterial(portraitSetting, drawPawn), // 아이콘용 Material 생성
                    cameraOffset = cameraOffset, // 카메라 오프셋
                    cameraZoom = portraitSetting.cameraZoom, // 카메라 줌
                    portraitSetting = portraitSetting // 현재 포트레잇 설정
                });
            }
        }

        // AnimationBed의 애니메이션 설정 및 업데이트
        public static void SetAnimation(Building_AnimationBed building_AnimationBed)
        {
            // AnimationBed에 놓인 Pawn 가져오기
            Pawn pawn = building_AnimationBed.HeldPawn;

            // AnimationSettingComp 가져오기
            var animationSettingComp = building_AnimationBed.AnimationSettingComp;

            // Bed 애니메이션 생성에 실패하면 메서드 종료
            if (!MakeBedAnimation(building_AnimationBed))
            {
                return;
            }

            // Bed 애니메이션 설정 업데이트
            foreach (var bedAnimationSettingAndTick in animationSettingComp.bedAnimationSettingAndTicks)
            {
                // Animation 설정 상태에 따라 currentTick 초기화 또는 업데이트
                if (building_AnimationBed.setAnimation)
                {
                    // Animation 재설정 상태라면 Tick을 0으로 초기화
                    bedAnimationSettingAndTick.currentTick = 0;
                }
                else if (bedAnimationSettingAndTick.parentBedAnimationDef.autoDurationTicksSetting)
                {
                    // 자동 지속 시간 설정일 경우 Pawn의 현재 AnimationTick으로 설정
                    bedAnimationSettingAndTick.currentTick = pawn.Drawer.renderer.renderTree.AnimationTick;
                }
                else
                {
                    // currentTick 업데이트: durationTick을 초과하지 않으면 증가, 초과 시 0으로 초기화
                    bedAnimationSettingAndTick.currentTick =
                        bedAnimationSettingAndTick.currentTick < bedAnimationSettingAndTick.durationTick - 1
                        ? bedAnimationSettingAndTick.currentTick + 1
                        : 0;
                }

                // 현재 Tick을 로그로 출력 (디버깅용)
                if (bedAnimationSettingAndTick.parentBedAnimationDef.logCurrentTick)
                {
                    Log.Error($"{bedAnimationSettingAndTick.parentBedAnimationDef} : " +
                              $"{bedAnimationSettingAndTick.currentTick} (current texture: {bedAnimationSettingAndTick.durationTick})");
                }
            }

            // Animation 설정 상태 초기화
            building_AnimationBed.setAnimation = false;
        }

        private static bool MakeBedAnimation(Building_AnimationBed building_AnimationBed)
        {
            // 애니메이션 침대에 연관된 Pawn을 가져옵니다.
            Pawn HeldPawn = building_AnimationBed.HeldPawn;

            // 필수 값들이 없을 경우 애니메이션 생성 불가
            if (building_AnimationBed.AnimationSettingComp == null || HeldPawn == null || HeldPawn.Drawer.renderer.renderTree.rootNode == null)
            {
                TestLog.Error("그래픽을 생성할 수 없습니다.");
                return false;
            }

            // 그래픽 생성을 이미 처리했는지 확인
            if (!building_AnimationBed.AnimationSettingComp.needMakeGraphics)
            {
                return true; // 이미 생성된 상태
            }

            TestLog.Error($"{HeldPawn.LabelShort} : 애니메이션 생성 시작");
            building_AnimationBed.AnimationSettingComp.needMakeGraphics = false; // 필요성 플래그 초기화
            building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks = new List<BedAnimationSettingAndTick>();

            // 설정 정보와 Pawn 애니메이션 정의를 가져옵니다.
            var Props = building_AnimationBed.AnimationSettingComp.Props;
            var pawnAnimationDef = GetPawnAnimationDef(building_AnimationBed);

            // Pawn의 애니메이션 설정
            HeldPawn.Drawer.renderer.SetAnimation(pawnAnimationDef);

            TestLog.Error("+++ 애니메이션 리스트 확인 +++");
            int index = 0;

            // 침대 애니메이션 리스트를 순회하며 설정 처리
            foreach (var bedAnimation in Props.bedAnimationList)
            {
                TestLog.Error($"침대 애니메이션 설정: {index}");
                index++;

                BedAnimationDef bedAnimationDef = bedAnimation.bedAnimationDef;
                Vector3 offset = bedAnimation.offset;
                Vector2 drawSize = bedAnimation.drawSize;

                // 조건에 맞는 애니메이션 설정 변경
                foreach (var conditionBedAnimationDef in bedAnimation.conditionBedAnimationDefs)
                {
                    if (conditionBedAnimationDef.bedAnimationDef == null)
                    {
                        TestLog.Error("conditionBedAnimationDef.bedAnimationDef가 null입니다.");
                        continue;
                    }

                    void action()
                    {
                        bedAnimationDef = conditionBedAnimationDef.bedAnimationDef;
                        offset = conditionBedAnimationDef.offset;
                        drawSize = conditionBedAnimationDef.drawSize;
                    }

                    // 조건이 맞으면 설정 업데이트
                    if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionBedAnimationDef.pawnCondition, action))
                    {
                        break;
                    }
                }

                TestLog.Error($"== {bedAnimationDef.defName} ==");
                TestLog.Error("++ BedAnimationSettingAndTick 생성 시작 ++");

                // BedAnimationSettingAndTick 생성
                var result = MakeBedAnimationSettingAndTick(building_AnimationBed, bedAnimationDef, offset, drawSize, pawnAnimationDef);
                building_AnimationBed.AnimationSettingComp.bedAnimationSettingAndTicks.Add(result);

                TestLog.Error("++ BedAnimationSettingAndTick 생성 완료 ++");
            }

            TestLog.Error("+++ 애니메이션 리스트 확인 완료 +++");

            // 초상화 및 FillableBar 생성
            TestLog.Error("### 초상화 및 FillableBar 생성 ###");
            MakePortrait(building_AnimationBed);
            MakeFillableBar(building_AnimationBed);
            TestLog.Error("### 초상화 및 FillableBar 생성 완료 ###");

            TestLog.Error("=============================");

            return true;
        }

        // FillableBar 정보를 생성 (반투명 바를 그릴 때 사용)
        private static void MakeFillableBar(Building_AnimationBed building_AnimationBed)
        {
            // FillableBar 재료 리스트 초기화
            building_AnimationBed.AnimationSettingComp.fillableBarIngredients = new List<FillableBarIngredient>();

            // 설정된 FillableBar 목록 순회
            foreach (var fillableBarSetting in building_AnimationBed.AnimationSettingComp.Props.fillableBarSettings)
            {
                // 조건에 따라 FillableBarIngredient 생성 및 추가
                void action()
                {
                    var fillableBarIngredient = new FillableBarIngredient
                    {
                        pawn = building_AnimationBed.HeldPawn, // 현재 침대에 놓인 Pawn
                        offset = fillableBarSetting.offset,   // FillableBar의 위치 오프셋
                        drawSize = fillableBarSetting.drawSize, // FillableBar의 크기
                        color = fillableBarSetting.color      // FillableBar의 색상
                    };

                    // FillableBar 재료를 AnimationSettingComp에 추가
                    building_AnimationBed.AnimationSettingComp.fillableBarIngredients.Add(fillableBarIngredient);
                }

                // 설정 조건에 맞으면 action 실행
                if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, fillableBarSetting.pawnCondition, action))
                {
                    break; // 조건에 맞는 설정이 처리되면 루프 종료
                }
            }
        }

        // 포트레잇에 필요한 Material 생성
        private static Material CreateMaterial(PortraitSetting portraitSetting, Pawn pawn)
        {
            // Pawn의 포트레잇 텍스처를 생성
            var mainTexture = PortraitsCache.Get(
                pawn,
                new Vector2(256, 256), // 텍스처 크기
                portraitSetting.rotation, // 회전 각도
                default, // 기본 값
                1, // 스케일
                renderClothes: true, // 의복 렌더링 활성화
                renderHeadgear: true, // 머리 장비 렌더링 활성화
                stylingStation: false, // 스타일링 스테이션 여부
                healthStateOverride: PawnHealthState.Mobile // Pawn의 상태 오버라이드
            );

            // 마스크 텍스처 로드
            var maskTex = ContentFinder<Texture2D>.Get(portraitSetting.maskPath);

            // MaterialRequest 생성
            var req = new MaterialRequest
            {
                mainTex = mainTexture, // 메인 텍스처
                color = Color.white, // 기본 색상
                shader = ShaderDatabase.CutoutComplex, // 셰이더
                maskTex = maskTex // 마스크 텍스처
            };

            // MaterialPool에서 Material 생성 및 반환
            return MaterialPool.MatFrom(req);
        }

        // BedAnimationSettingAndTick 생성
        private static BedAnimationSettingAndTick MakeBedAnimationSettingAndTick(
            Building_AnimationBed building_AnimationBed,
            BedAnimationDef bedAnimationDef,
            Vector3 offset,
            Vector2 drawSize,
            AnimationDef pawnAnimationDef)
        {
            // 애니메이션 설정 리스트
            var bedAnimationSettings = new List<BedAnimationSetting>();

            // BedAnimationSettingAndTick 객체 초기화
            var bedAnimationSettingAndTick = new BedAnimationSettingAndTick
            {
                autoDurationTicksSetting = bedAnimationDef.autoDurationTicksSetting, // 자동 지속 시간 설정
                parentBedAnimationDef = bedAnimationDef, // 부모 애니메이션 정의
                bedAnimationSettings = bedAnimationSettings // 애니메이션 설정 리스트
            };

            TestLog.Error("+Check bedAnimationSetting+");

            // 애니메이션 설정 순회
            foreach (var bedAnimationSetting in bedAnimationDef.bedAnimationSettings)
            {
                // 현재 애니메이션 침대에 놓인 Pawn
                Pawn HeldPawn = building_AnimationBed.HeldPawn;

                // 조건이 만족되면 애니메이션 설정을 복사하고 수정
                void action()
                {
                    var settingCopy = bedAnimationSetting.Copy(); // 설정 복사
                    settingCopy.parentBedAnimationDef = bedAnimationDef; // 부모 애니메이션 정의 설정
                    settingCopy.setPawnColor = bedAnimationDef.setPawnColor; // Pawn 색상 설정 여부
                    settingCopy.offset = bedAnimationSetting.offset + offset + bedAnimationDef.offset; // 위치 오프셋 계산

                    // 그래픽 데이터 설정
                    GraphicData graphicData = settingCopy.graphicData;

                    TestLog.Error("choiceGraphicData");
                    foreach (var conditionGraphicData in bedAnimationSetting.conditionGraphicDatas)
                    {
                        // 조건이 만족되면 그래픽 데이터 선택
                        void choiceGraphicData()
                        {
                            graphicData = conditionGraphicData.graphicData;
                        }

                        if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionGraphicData.pawnCondition, choiceGraphicData))
                        {
                            TestLog.Error("break");
                            break;
                        }
                    }

                    // 그래픽 생성 및 설정
                    TestLog.Error("make graphic");
                    if (graphicData != null)
                    {
                        settingCopy.graphic = bedAnimationDef.isPawnTextureReplace
                            ? GetGraphic(HeldPawn, bedAnimationDef.pawnRenderNodeTagDef, settingCopy, graphicData)
                            : graphicData.Graphic;

                        // 그래픽 크기 조정
                        if (settingCopy.graphic != null && !bedAnimationDef.isPawnTextureReplace)
                        {
                            settingCopy.graphic.drawSize = graphicData.drawSize + drawSize;
                        }

                        // 유효한 그래픽이 있는 경우 설정 추가
                        if (settingCopy.graphic != null)
                        {
                            bedAnimationSettings.Add(settingCopy);
                            TestLog.Error("settingCopy is finish");
                        }
                        else
                        {
                            TestLog.Error("settingCopy Skip");
                        }
                    }
                    else
                    {
                        TestLog.Error("graphicData is null");
                    }
                }

                // 조건이 만족되면 설정 처리 후 중단
                TestLog.Error("check Condition");
                if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, bedAnimationSetting.pawnCondition, action))
                {
                    TestLog.Error("break");
                    break;
                }
                else
                {
                    TestLog.Error("조건 불충족");
                }
            }

            TestLog.Error("+Finish Check bedAnimationSetting+");

            // 지속 시간 계산
            if (bedAnimationSettings.Count != 0)
            {
                bedAnimationSettingAndTick.durationTick = bedAnimationDef.autoDurationTicksSetting
                    ? pawnAnimationDef.durationTicks // 자동 지속 시간 설정
                    : bedAnimationDef.durationTicks != 0 && bedAnimationDef.durationTicks >= bedAnimationSettings.Max(x => x.tick)
                        ? bedAnimationDef.durationTicks // 정의된 지속 시간 사용
                        : bedAnimationSettings.Max(x => x.tick); // 설정 중 가장 큰 tick 값 사용
            }

            return bedAnimationSettingAndTick;
        }

        // Pawn의 그래픽 데이터를 가져옵니다.
        private static Graphic GetGraphic(Pawn pawn, PawnRenderNodeTagDef tagDef, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
        {
            // 반환할 그래픽 객체
            Graphic graphic;

            // tagDef에 따라 적절한 그래픽 생성 메서드 호출
            if (tagDef == PawnRenderNodeTagDefOf.Body)
            {
                // Pawn의 몸체 그래픽 가져오기
                graphic = GraphicForBody(pawn, bedAnimationSetting, graphicData);
            }
            else if (tagDef == PawnRenderNodeTagDefOf.Head)
            {
                // Pawn의 머리 그래픽 가져오기
                graphic = GraphicForHead(pawn, bedAnimationSetting, graphicData);
            }
            else
            {
                // 기본 그래픽 가져오기
                graphic = graphicData.Graphic;
            }

            // 생성된 그래픽 반환
            return graphic;
        }


        // AnimationBed에서 Pawn에 적합한 AnimationDef를 가져옵니다.
        private static AnimationDef GetPawnAnimationDef(Building_AnimationBed building_AnimationBed)
        {
            // 기본 AnimationDef 가져오기
            var pawnAnimationDef = building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.pawnAnimationDef;

            // 조건에 따라 AnimationDef를 변경
            foreach (var conditionPawnAnimation in building_AnimationBed.AnimationSettingComp.Props.pawnAnimationSetting.conditonPawnAnimations)
            {
                // 조건이 만족되면 pawnAnimationDef를 업데이트
                void action() => pawnAnimationDef = conditionPawnAnimation.pawnAnimationDef;

                // 조건 실행 및 조건 충족 시 루프 중단
                if (Condition.ExecuteActionIfConditionMatches(building_AnimationBed, conditionPawnAnimation.pawnCondition, action))
                {
                    break;
                }
            }

            // 최종 AnimationDef 반환
            return pawnAnimationDef;
        }


        // Pawn의 머리 그래픽 데이터를 생성하여 반환합니다.
        private static Graphic GraphicForHead(Pawn pawn, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
        {
            // Pawn이 머리를 가지고 있지 않다면 null 반환
            if (!pawn.health.hediffSet.HasHead)
            {
                return null;
            }

            // PawnRenderNode_Head를 안전하게 검색
            var pawnRenderNode_Head = pawn.Drawer.renderer.renderTree.rootNode?.children
                .OfType<PawnRenderNode_Head>() // Head 노드만 필터링
                .FirstOrDefault(node => node?.Props?.tagDef == bedAnimationSetting.parentBedAnimationDef.pawnRenderNodeTagDef);

            // Head 노드가 존재하지 않으면 null 반환
            if (pawnRenderNode_Head == null)
            {
                return null;
            }

            // Pawn이 건조된 상태(Dessicated)라면 Skull 그래픽 반환
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return HeadTypeDefOf.Skull.GetGraphic(pawn, Color.white);
            }

            // 기본 셰이더와 색상 설정
            var skinShader = graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;
            var color = graphicData.color;

            // 설정에 따라 Pawn의 색상 및 셰이더를 적용
            if (bedAnimationSetting.setPawnColor)
            {
                color = pawnRenderNode_Head.ColorFor(pawn); // Pawn의 색상 가져오기
                skinShader = ShaderUtility.GetSkinShader(pawn); // Pawn의 스킨 셰이더 가져오기
            }

            // 그래픽을 생성하여 반환
            return GraphicDatabase.Get<Graphic_Multi>(
                graphicData.texPath,      // 그래픽 텍스처 경로
                skinShader,               // 적용할 셰이더
                graphicData.drawSize * 1.25f, // 크기 조정
                color                     // 적용할 색상
            );
        }

        // Pawn의 몸체 그래픽 데이터를 생성하여 반환합니다.
        private static Graphic GraphicForBody(Pawn pawn, BedAnimationSetting bedAnimationSetting, GraphicData graphicData)
        {
            // PawnRenderNode_Body를 검색하여 적합한 노드를 찾습니다.
            var pawnRenderNode_Body = pawn.Drawer.renderer.renderTree.rootNode?.children
                .OfType<PawnRenderNode_Body>() // Body 노드만 필터링
                .FirstOrDefault(child => child?.Props?.tagDef == bedAnimationSetting.parentBedAnimationDef.pawnRenderNodeTagDef);

            // Body 노드가 없으면 null 반환
            if (pawnRenderNode_Body == null)
            {
                return null;
            }

            // 기본 셰이더 초기화
            var shader = graphicData.shaderType?.Shader ?? ShaderDatabase.Cutout;

            // Pawn이 건조된 상태(Dessicated)라면 건조된 몸체 그래픽을 반환
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Dessicated)
            {
                return GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, shader);
            }

            // 기본 색상 설정
            var color = graphicData.color;

            // 설정에 따라 Pawn의 색상 및 셰이더를 적용
            if (bedAnimationSetting.setPawnColor)
            {
                color = pawnRenderNode_Body.ColorFor(pawn); // Pawn의 색상 가져오기
                shader = pawnRenderNode_Body.ShaderFor(pawn); // Pawn의 셰이더 가져오기
            }

            // 그래픽 생성 및 반환
            return GraphicDatabase.Get<Graphic_Multi>(
                graphicData.texPath,      // 그래픽 텍스처 경로
                shader,                   // 적용할 셰이더
                graphicData.drawSize * 1.25f, // 크기 조정
                color                     // 적용할 색상
            );
        }

    }
}