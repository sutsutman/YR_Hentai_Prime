using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // 이 클래스는 CNMS(생성 및 관리 시스템)과 상호작용하는 컴포넌트의 속성을 정의합니다.
    public class CompProperties_AffectedByCNMS : CompProperties
    {
        public CompProperties_AffectedByCNMS() => compClass = typeof(CompAffectedByCNMS);

        // 기본 생성 주기 (틱 단위)
        public int ticksToSpawn = 2000;

        // 생성될 사물의 정의
        public ThingDef thingToSpawn;

        // 생성할 사물의 개수
        public int spawnCount;

        // 조건부 생성 사물 리스트
        public List<ConditionSpawnThing> conditionSpawnThings = new List<ConditionSpawnThing>();

        // Joy 기능과 연계 여부
        public bool dummyForJoyIsActive = false;
    }

    // 조건부 생성 항목 클래스
    public class ConditionSpawnThing
    {
        public PawnCondition pawnCondition; // 생성 조건
        public ThingDef thingToSpawn; // 조건 충족 시 생성될 사물
        public int spawnCount; // 생성 개수
    }

    // CNMS와 상호작용하는 컴포넌트 클래스
    [StaticConstructorOnStartup]
    public class CompAffectedByCNMS : CompBaseOfAnimationBed
    {
        // 디버그 관련 상수 및 기본 아이콘 경로
        private const string DebugSpawnLabel = "DEBUG: Spawn ";
        private const string UiIconPath = "Ui/Widgets/CheckOff";

        // 현재 컴포넌트의 속성 접근자
        public CompProperties_AffectedByCNMS Props => (CompProperties_AffectedByCNMS)props;

        // CNMS 관련 시설 컴포넌트 접근자
        public CompAffectedByFacilities AffectedByFacilitiesComp => parent.TryGetComp<CompAffectedByFacilities>();

        // 생성 주기를 저장하는 변수
        private int ticksToSpawn;

        // 컴포넌트 초기 설정
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksToSpawn = Props.ticksToSpawn; // 기본 생성 주기 설정
        }

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // 대상 Pawn이 있을 경우, 생성 주기를 감소
            if (HeldPawn != null && Building_AnimationBed.PowerOn)
            {
                if (Props.dummyForJoyIsActive && Building_AnimationBed.dummyForJoyIsActive)
                    ticksToSpawn--;
                else if (!Props.dummyForJoyIsActive)
                    ticksToSpawn--;
            }
            else
            {
                // 대상 Pawn이 없으면 주기를 초기화
                ticksToSpawn = Props.ticksToSpawn;
            }

            // 생성 주기가 0 이하가 되면 사물 생성
            if (ticksToSpawn <= 0)
            {
                ticksToSpawn = Props.ticksToSpawn;
                SpawnThing();
            }
        }

        // 사물을 생성하는 메서드
        private void SpawnThing()
        {
            var spawnThing = MakeSpawnThing();

            // CNMS 관련 컴포넌트가 없거나 연계된 시설이 없으면 기본 위치에 생성
            if (AffectedByFacilitiesComp?.LinkedFacilitiesListForReading.NullOrEmpty() ?? true)
            {
                GenSpawn.Spawn(spawnThing, parent.Position, parent.Map);
                return;
            }

            // 가장 가까운 CNMS를 찾아서 해당 위치에 생성
            var closestCNMS = FindClosestCNMS(parent.Position, AffectedByFacilitiesComp.LinkedFacilitiesListForReading);
            if (closestCNMS?.TryGetComp<CompFacility_CNMS>() is CompFacility_CNMS compCNMS)
            {
                compCNMS.innerContainer.TryAddOrTransfer(spawnThing);
            }
            else
            {
                // CNMS가 제대로 설정되지 않은 경우 기본 위치에 생성
                Log.Error($"{closestCNMS?.def} is set to CompProperties_AffectedByFacilities_CNMS, but it is not CNMS");
                GenSpawn.Spawn(spawnThing, parent.Position, parent.Map);
            }
        }

        // 지정된 위치에서 가장 가까운 CNMS를 찾는 메서드
        private Thing FindClosestCNMS(IntVec3 referencePosition, List<Thing> things)
        {
            return things?.OrderBy(thing => referencePosition.DistanceTo(thing.Position)).FirstOrDefault();
        }

        // 조건에 따라 생성할 사물을 결정하는 메서드
        private Thing MakeSpawnThing()
        {
            var thingToSpawn = Props.thingToSpawn;
            var spawnCount = Props.spawnCount;

            foreach (var condition in Props.conditionSpawnThings)
            {
                // 조건이 충족되면 해당 사물을 설정
                if (Condition.ExecuteActionIfConditionMatches(Building_AnimationBed, condition.pawnCondition, () =>
                {
                    thingToSpawn = condition.thingToSpawn;
                    spawnCount = condition.spawnCount;
                }))
                {
                    break;
                }
            }

            // 사물을 생성하고 개수를 설정
            var thing = ThingMaker.MakeThing(thingToSpawn);
            thing.stackCount = spawnCount;

            return thing;
        }

        // 컴포넌트의 추가 설명 문자열을 반환
        public override string CompInspectStringExtra()
        {
            var pawn = Building_AnimationBed.HeldPawn;
            if (pawn == null) return "";

            var spawnThing = MakeSpawnThing();
            return spawnThing == null
                ? ""
                : $"NextSpawnedResourceIn: {ticksToSpawn.ToStringTicksToPeriod()} \n" +
                  $"Spawn Thing: {spawnThing.def.label} \n" +
                  $"Spawn Count: {spawnThing.stackCount}";
        }

        // 디버그용 추가 Gizmo를 생성
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                var spawnThing = MakeSpawnThing();
                yield return new Command_Action
                {
                    defaultLabel = $"{DebugSpawnLabel}{spawnThing?.def.label ?? "Nothing"}",
                    icon = spawnThing?.def.uiIcon ?? ContentFinder<Texture2D>.Get(UiIconPath),
                    action = SpawnThing
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
