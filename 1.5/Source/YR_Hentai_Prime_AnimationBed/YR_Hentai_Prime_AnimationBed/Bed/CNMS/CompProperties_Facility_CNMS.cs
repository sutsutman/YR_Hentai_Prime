using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // CNMS의 속성을 정의하는 클래스
    public class CompProperties_Facility_CNMS : CompProperties_Facility
    {
        public CompProperties_Facility_CNMS() => compClass = typeof(CompFacility_CNMS);

        // 사물을 배출할 기본 틱 수
        public int ejectTicks = 1000;
    }

    // CNMS 컴포넌트 클래스
    public class CompFacility_CNMS : CompFacility, IThingHolder
    {
        // 속성 접근자
        public new CompProperties_Facility_CNMS Props => (CompProperties_Facility_CNMS)props;

        // 내부 컨테이너 (Thing을 보관)
        public ThingOwner innerContainer;

        // 임시 리스트 (저장/로드 시 사용)
        private readonly List<Thing> tmpThings = new List<Thing>();

        // 배출 틱 관리
        private int ejectTicks;

        // 메인 CNMS 여부
        private bool isMainCNMS;

        // 생성자: innerContainer 초기화
        public CompFacility_CNMS()
        {
            innerContainer = new ThingOwner<Thing>(this);
            ejectTicks = Props?.ejectTicks ?? 1000;
        }

        // 틱 단위 동작
        public override void CompTick()
        {
            base.CompTick();
            ejectTicks--;

            if (ejectTicks <= 0)
            {
                ejectTicks = Props.ejectTicks;
                EjectOrTransfer(); // 조건에 따라 사물을 배출하거나 이동
            }
        }

        // 내부 컨테이너의 사물을 배출하거나 다른 CNMS로 이동
        private void EjectOrTransfer()
        {
            if (isMainCNMS)
            {
                CheckAndEjectOverflowingStacks();
            }
            else
            {
                // 메인 CNMS를 찾고 모든 사물을 이동
                var mainCNMS = parent.Map?.listerThings.AllThings
                    .Select(t => t.TryGetComp<CompFacility_CNMS>())
                    .FirstOrDefault(comp => comp?.isMainCNMS == true);

                if (mainCNMS != null)
                {
                    innerContainer.TryTransferAllToContainer(mainCNMS.innerContainer);
                    return;
                }
            }

            // 메인 CNMS가 없거나 이동 실패 시 배출
            CheckAndEjectOverflowingStacks();
        }

        // 스택 제한을 초과한 사물을 배출
        private void CheckAndEjectOverflowingStacks()
        {
            var thingsToEject = innerContainer
                .GroupBy(t => t.def)
                .Where(g => g.Count() >= 10)
                .SelectMany(g => g)
                .ToList();

            foreach (var thing in thingsToEject)
            {
                if (!innerContainer.TryDrop(thing, parent.Position, parent.Map, ThingPlaceMode.Near, out _))
                {
                    Log.Warning($"Failed to eject {thing.LabelCap} from innerContainer.");
                }
            }
        }

        // 추가적인 Gizmo 제공
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // 메인 CNMS 토글
            yield return new Command_Action
            {
                defaultLabel = "YR_ToggleMainCNMS_Label".Translate(),
                defaultDesc = "YR_ToggleMainCNMS_Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get(isMainCNMS ? "UI/Commands/ToggleHediff" : "UI/Commands/TransferVictim"),
                action = ToggleMainCNMS
            };

            // 내부 컨테이너 배출
            yield return new Command_Action
            {
                defaultLabel = "YR_EjectInnerContainer_Label".Translate(),
                defaultDesc = "YR_EjectInnerContainer_Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ToggleHediff"),
                action = EjectInnerContainer
            };
        }

        // 내부 컨테이너의 모든 사물을 배출
        private void EjectInnerContainer()
        {
            innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
        }

        // 메인 CNMS 상태 토글
        private void ToggleMainCNMS()
        {
            isMainCNMS = !isMainCNMS;

            if (isMainCNMS && parent.Map != null)
            {
                foreach (var thing in parent.Map.listerThings.AllThings)
                {
                    if (thing == parent) continue;

                    thing.TryGetComp<CompFacility_CNMS>()?.SetMainCNMS(false);
                }
            }
        }

        // 메인 CNMS 상태 설정
        public void SetMainCNMS(bool state)
        {
            isMainCNMS = state;
        }

        // 데이터 저장/로드
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isMainCNMS, "isMainCNMS");
            Scribe_Values.Look(ref ejectTicks, "ejectTicks");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }
        public override string CompInspectStringExtra()
        {
            return "Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
        }

        // IThingHolder 구현: 자식 컨테이너 반환
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        // IThingHolder 구현: 직접적으로 보유한 Thing 반환
        public ThingOwner GetDirectlyHeldThings() => innerContainer;
    }
}
