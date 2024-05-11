using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Facility_CNMS : CompProperties_Facility
    {
        public CompProperties_Facility_CNMS() => compClass = typeof(CompFacility_CNMS);

        public int ejectTicks = 1000;
    }

    public class CompFacility_CNMS : CompFacility, IThingHolder
    {
        public new CompProperties_Facility_CNMS Props => (CompProperties_Facility_CNMS)props;

        public ThingOwner innerContainer;
        private readonly List<Thing> tmpThings = new List<Thing>();
        public CompFacility_CNMS()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        int ejectTicks = 1000;
        public override void CompTick()
        {
            base.CompTick();

            ejectTicks--;
            if (ejectTicks <= 0)
            {
                foreach (var thing in innerContainer)
                {
                    Log.Error(thing.Label + " : " + thing.stackCount);
                }
                ejectTicks = Props.ejectTicks;


                // innerContainer 속 thing의 스택이 해당 thing의 def의 stack의 10배면 주변에 배출
                EjectOrTransfer();
            }
        }

        private void EjectOrTransfer()
        {
            if (isMainCNMS)
            {
                CheckAndEjectOverflowingStacks();
            }
            else
            {
                if (parent.Map != null)
                {
                    var allThings = parent.Map.listerThings.AllThings;

                    foreach (var thing in allThings)
                    {
                        if (thing == parent)
                        {
                            continue;
                        }
                        var compCNMS = thing.TryGetComp<CompFacility_CNMS>();

                        if (compCNMS != null)
                        {
                            if (compCNMS.isMainCNMS)
                            {
                                innerContainer.TryTransferAllToContainer(compCNMS.innerContainer);
                                return;
                            }
                        }
                    }
                }
            }
            CheckAndEjectOverflowingStacks();
        }

        private void CheckAndEjectOverflowingStacks()
        {
            var thingDefCounts = new Dictionary<ThingDef, int>();

            // innerContainer에 있는 모든 Thing을 검사하고 각 ThingDef의 개수를 셉니다.
            foreach (var thing in innerContainer)
            {
                var thingDef = thing.def;

                if (!thingDefCounts.ContainsKey(thingDef))
                {
                    thingDefCounts[thingDef] = 0;
                }

                thingDefCounts[thingDef] += 1;
            }

            // 배출할 Thing들을 추출
            var thingsToEject = new List<Thing>();
            foreach (var kvp in thingDefCounts)
            {
                if (kvp.Value >= 10)
                {
                    // 해당 ThingDef의 모든 Thing을 배출 목록에 추가
                    thingsToEject.AddRange(innerContainer.Where(t => t.def == kvp.Key));
                }
            }

            // 배출할 Thing들을 주변에 떨어뜨림
            foreach (var thing in thingsToEject)
            {
                if (!innerContainer.TryDrop(thing, parent.Position, parent.Map, ThingPlaceMode.Near, out var droppedThing))
                {
                    Log.Warning($"Failed to drop {thing.LabelCap} from innerContainer.");
                }
            }

        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "YR_ToggleMainCNMS_Label".Translate(),
                defaultDesc = "YR_ToggleMainCNMS_Desc".Translate(),
                icon = isMainCNMS ? ContentFinder<Texture2D>.Get("UI/Commands/ToggleHediff") : ContentFinder<Texture2D>.Get("UI/Commands/TransferVictim"),
                action = ToggleMainCNMS
            };

            yield return new Command_Action
            {
                defaultLabel = "YR_EjectInnerContainer_Label".Translate(),
                defaultDesc = "YR_EjectInnerContainer_Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/ToggleHediff"),
                action = EjectInnerContainer
            };
        }

        private void EjectInnerContainer()
        {
            innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
        }

        bool isMainCNMS = false;
        public void ToggleMainCNMS()
        {
            if (isMainCNMS)
            {
                isMainCNMS = false;
            }
            else
            {
                isMainCNMS = true;

                // 맵 전체에서 CompFacility_CNMS를 가진 자신을 제외한 모든 Thing을 찾고, 해당 CompFacility_CNMS의 isMainCNMS를 false로 처리
                Map map = parent.Map;

                if (map != null)
                {
                    var allThings = map.listerThings.AllThings;

                    foreach (var thing in allThings)
                    {
                        if (thing == parent)
                        {
                            continue;
                        }
                        var compCNMS = thing.TryGetComp<CompFacility_CNMS>();

                        if (compCNMS != null)
                        {
                            compCNMS.isMainCNMS = false; // CompFacility_CNMS의 isMainCNMS를 false로 설정
                        }
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isMainCNMS, "isMainCNMS");
            Scribe_Values.Look(ref ejectTicks, "EjectTicks");

            bool flag = !parent.SpawnedOrAnyParentSpawned;
            if (flag && Scribe.mode == LoadSaveMode.Saving)
            {
                tmpThings.Clear();
                tmpThings.AddRange(innerContainer);
                tmpThings.Clear();
            }
            Scribe_Deep.Look(ref innerContainer, "innerContainer", new object[]
            {
                this
            });
            //Scribe_Values.Look(ref ticksToSpawn, "ticksToSpawn");

        }
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }
    }
}
