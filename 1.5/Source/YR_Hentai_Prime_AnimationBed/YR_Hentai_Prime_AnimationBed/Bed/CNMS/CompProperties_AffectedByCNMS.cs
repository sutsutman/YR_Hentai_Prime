using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    // 사물 생성 정보 클래스
    public class SpawnThingInfo
    {
        public ThingDef thingDef; // 생성할 사물의 정의
        public int spawnCount; // 생성 개수
    }

    // 폰 생성 정보 클래스
    public class SpawnPawnInfo
    {
        public PawnKindDef pawnKindDef; // 생성할 PawnKind
        public int spawnPawnCount; // 생성할 Pawn 개수
    }

    // 생성 항목 클래스 (사물, 폰, 사운드 포함)
    public class SpawnItem
    {
        public SpawnThingInfo spawnThingInfo; // 사물 생성 정보
        public SpawnPawnInfo spawnPawnInfo; // 폰 생성 정보
        public SoundDef soundDef; // 생성 시 재생할 사운드
    }

    // 조건부 생성 항목 클래스
    public class ConditionSpawnItem
    {
        public PawnCondition pawnCondition; // 생성 조건
        public SpawnItem spawnItem; // 조건 충족 시 생성할 항목
    }

    // CNMS 관련 컴포넌트 속성 정의 클래스
    public class CompProperties_AffectedByCNMS : CompProperties
    {
        public CompProperties_AffectedByCNMS() => compClass = typeof(CompAffectedByCNMS);

        public int ticksToSpawn = 2000; // 기본 생성 주기

        public List<SpawnItem> spawnItems = new List<SpawnItem>(); // 기초 생성 항목
        public List<ConditionSpawnItem> conditionSpawnItems = new List<ConditionSpawnItem>(); // 조건부 생성 항목 리스트
    }

    // CNMS와 상호작용하는 컴포넌트 클래스
    [StaticConstructorOnStartup]
    public class CompAffectedByCNMS : CompBaseOfAnimationBed
    {
        private const string DebugSpawnLabel = "DEBUG: Spawn ";
        private const string UiIconPath = "Ui/Widgets/CheckOff";
        private const string PawnOnlyIconPath = "AAA"; // Pawn만 생성 시 아이콘 경로

        public CompProperties_AffectedByCNMS Props => (CompProperties_AffectedByCNMS)props; // 속성에 접근하는 프로퍼티
        public CompAffectedByFacilities AffectedByFacilitiesComp => parent.TryGetComp<CompAffectedByFacilities>(); // CNMS 시설과의 상호작용 컴포넌트

        private int ticksToSpawn; // 생성 주기를 저장하는 변수

        // 컴포넌트 초기 설정 메서드
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksToSpawn = Props.ticksToSpawn; // 생성 주기 초기화
        }

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // 대상 Pawn이 있는 경우 생성 주기를 감소
            if (HeldPawn != null && Building_AnimationBed.PowerOn)
            {
                ticksToSpawn--;
            }
            else
            {
                // 대상 Pawn이 없는 경우 생성 주기를 초기화
                ticksToSpawn = Props.ticksToSpawn;
            }

            // 생성 주기가 0 이하가 되면 생성 항목 처리
            if (ticksToSpawn <= 0)
            {
                ticksToSpawn = Props.ticksToSpawn; // 주기 초기화
                SpawnItem();
            }
        }

        // 항목을 생성하는 메서드
        private void SpawnItem()
        {
            var spawnItem = SelectSpawnItem(); // 생성할 항목 선택
            if (spawnItem == null) return;

            if (spawnItem.spawnThingInfo != null && spawnItem.spawnThingInfo.thingDef != null)
            {
                SpawnThing(spawnItem.spawnThingInfo); // 사물 생성
            }

            if (spawnItem.spawnPawnInfo != null && spawnItem.spawnPawnInfo.pawnKindDef != null)
            {
                SpawnPawn(spawnItem.spawnPawnInfo); // 폰 생성
            }

            spawnItem.soundDef?.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false)); // 사운드 재생
        }

        // 생성 항목 선택 메서드
        private SpawnItem SelectSpawnItem()
        {
            foreach (var conditionItem in Props.conditionSpawnItems)
            {
                // 조건에 맞는 항목을 찾으면 반환
                if (Condition.ExecuteActionIfConditionMatches(Building_AnimationBed, conditionItem.pawnCondition, () => { }))
                {
                    return conditionItem.spawnItem;
                }
            }

            // 조건에 맞는 항목이 없으면 기본 생성 항목 반환
            return Props.spawnItems.FirstOrDefault();
        }

        // 사물을 생성하는 메서드
        private void SpawnThing(SpawnThingInfo info)
        {
            if (info == null || info.thingDef == null) return;

            var thing = ThingMaker.MakeThing(info.thingDef); // 사물 생성
            thing.stackCount = info.spawnCount; // 생성 개수 설정

            if (AffectedByFacilitiesComp?.LinkedFacilitiesListForReading.NullOrEmpty() ?? true)
            {
                // 연계된 시설이 없으면 현재 위치에 생성
                GenSpawn.Spawn(thing, parent.Position, parent.Map);
                return;
            }

            var closestCNMS = FindClosestCNMS(parent.Position, AffectedByFacilitiesComp.LinkedFacilitiesListForReading);
            if (closestCNMS?.TryGetComp<CompFacility_CNMS>() is CompFacility_CNMS compCNMS)
            {
                // 가장 가까운 CNMS에 추가
                compCNMS.innerContainer.TryAddOrTransfer(thing);
            }
            else
            {
                // CNMS가 제대로 설정되지 않은 경우 현재 위치에 생성
                GenSpawn.Spawn(thing, parent.Position, parent.Map);
            }
        }

        // 폰을 생성하는 메서드
        private void SpawnPawn(SpawnPawnInfo info)
        {
            if (info == null || info.pawnKindDef == null) return;

            for (int i = 0; i < info.spawnPawnCount; i++)
            {
                var request = new PawnGenerationRequest(info.pawnKindDef, allowDowned: true, fixedBiologicalAge: 0);
                var pawn = PawnGenerator.GeneratePawn(request); // 폰 생성
                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, 1), parent.Map, WipeMode.Vanish); // 폰 스폰
                pawn.SetFaction(HeldPawn.Faction); // 팩션 설정
            }
        }

        // 가장 가까운 CNMS를 찾는 메서드
        private Thing FindClosestCNMS(IntVec3 referencePosition, List<Thing> things)
        {
            return things?.OrderBy(thing => referencePosition.DistanceTo(thing.Position)).FirstOrDefault();
        }

        // 컴포넌트의 추가 설명 문자열 반환
        public override string CompInspectStringExtra()
        {
            var nextSpawnItem = SelectSpawnItem(); // 다음 생성 항목 선택
            if (nextSpawnItem == null) return "";

            var thingInfo = nextSpawnItem.spawnThingInfo;
            var pawnInfo = nextSpawnItem.spawnPawnInfo;

            var info = $"NextSpawnedResourceIn: {ticksToSpawn.ToStringTicksToPeriod()}"; // 다음 생성까지 남은 시간 표시

            if (thingInfo != null)
            {
                // 생성될 사물 정보 추가
                info += $"\nSpawn Thing: {thingInfo.thingDef?.label ?? "Unknown"} \nSpawn Count: {thingInfo.spawnCount}";
            }

            if (pawnInfo != null)
            {
                // 생성될 폰 정보 추가
                info += $"\nSpawn Pawn: {pawnInfo.pawnKindDef?.label ?? "Unknown"} \nSpawn Count: {pawnInfo.spawnPawnCount}";
            }

            return info;
        }

        // 디버그용 추가 Gizmo 생성
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                var spawnItem = SelectSpawnItem(); // 생성 항목 선택
                var iconPath = spawnItem?.spawnPawnInfo != null && spawnItem.spawnThingInfo == null
                    ? ContentFinder<Texture2D>.Get(spawnItem.spawnPawnInfo.pawnKindDef.lifeStages[0].bodyGraphicData.texPath + "_south") // 폰만 생성 시 지정된 아이콘 경로 사용
                    : spawnItem?.spawnThingInfo?.thingDef?.uiIcon ?? ContentFinder<Texture2D>.Get(UiIconPath);

                var label = spawnItem?.spawnPawnInfo?.pawnKindDef != null
                    ? $"{DebugSpawnLabel}{spawnItem.spawnPawnInfo.pawnKindDef.label}" // 폰Kind 라벨 표시
                    : $"{DebugSpawnLabel}{spawnItem?.spawnThingInfo?.thingDef?.label ?? "Nothing"}"; // 사물 라벨 표시

                yield return new Command_Action
                {
                    defaultLabel = label,
                    icon = iconPath, // 아이콘 설정
                    action = () => SpawnItem() // 클릭 시 항목 생성
                };
            }
        }

        // 저장/불러오기 시 데이터를 노출
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");
        }
    }
}
