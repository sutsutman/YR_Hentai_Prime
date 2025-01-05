using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Building_Tentacle_Altar : Building_Grave
    {
        // 제물로 바쳐진 시체 수를 기록하는 변수
        public int offeredCorpse = 0;

        // 데이터 저장 및 로드 메서드
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref offeredCorpse, "offeredCorpse", 0, false);
        }

        // 이 건물의 특별한 컴포넌트를 가져오는 속성
        private Comp_Building_Tentacle_Altar Comp_Building_Tentacle_Altar => this.TryGetComp<Comp_Building_Tentacle_Altar>();

        // 연료 관련 컴포넌트
        public CompRefuelable RefuelableComp => this.TryGetComp<CompRefuelable>();

        // 시체가 건물로 옮겨졌을 때 호출되는 메서드
        public override void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            base.Notify_HauledTo(hauler, thing, count);

            // 건물에 시체가 있는 경우 시체를 파괴하고 제물로 바친 횟수를 증가시킴
            if (Corpse != null)
            {
                Corpse.Destroy();
                offeredCorpse++;
            }

            // 특정 조건을 만족하면 건물을 새로운 것으로 변환
            Evolution();
        }


        // 특정 조건을 만족하면 건물을 새로운 것으로 변환
        public void Evolution()
        {
            if (Comp_Building_Tentacle_Altar != null)
            {
                CompProperties_Building_Tentacle_Altar Props = Comp_Building_Tentacle_Altar.Props;

                if (Props != null && Props.thingDef != null)
                {
                    if (offeredCorpse >= Props.needCorpseCount)
                    {
                        if (Map != null && Position != null)
                        {
                            IntVec3 position = Position;
                            Map map = Map;

                            if(position==null)
                            {
                                Log.Error("position is null");
                            }
                            if (map == null)
                            {
                                Log.Error("map is null");
                            }
                            Destroy();
                            var thing = GenSpawn.Spawn(Props.thingDef, position, map);
                            thing.SetFaction(Faction.OfPlayer);
                        }
                        else
                        {
                            Log.Error("Building_Tentacle_Altar: Map or Position is null.");
                        }
                    }
                }
                else
                {
                    Log.Error("Building_Tentacle_Altar: Props or thingDef is null.");
                }
            }
        }


        // 건물이 특정 아이템을 수락할 수 있는지 여부를 확인하는 메서드
        public override bool Accepts(Thing thing)
        {
            return base.Accepts(thing) && offeredCorpse < Comp_Building_Tentacle_Altar.Props.maxCorpseCount;
        }

        // 건물의 그래픽을 설정하는 속성
        public override Graphic Graphic
        {
            get
            {
                if (Comp_Building_Tentacle_Altar == null)
                {
                    return base.Graphic;
                }
                else
                {
                    if (CorpseFulfilled && Comp_Building_Tentacle_Altar.Props.graphicData != null)
                    {
                        return cachedGraphicFull ??= Comp_Building_Tentacle_Altar.Props.graphicData.Graphic;
                    }
                }
                return base.Graphic;
            }
        }

        // 필요한 시체 수를 만족했는지 확인하는 속성
        public bool CorpseFulfilled => offeredCorpse >= Comp_Building_Tentacle_Altar.Props.needCorpseCount;

        // 그래픽 캐시 변수
        private Graphic cachedGraphicFull;
    }
}
