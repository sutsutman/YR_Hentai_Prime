using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Tentacle_Apparel : CompProperties
    {
        public CompProperties_Tentacle_Apparel() => compClass = typeof(Comp_Tentacle_Apparel);

        public int ticks = 10;      // 효과 발생 주기(틱 단위)
        public int hpRecovery = 1;  // 장비의 HP 회복량
    }

    public class Comp_Tentacle_Apparel : ThingComp
    {
        private int ticks = 0;
        private bool destroy = false;

        private CompProperties_Tentacle_Apparel Props => (CompProperties_Tentacle_Apparel)props;

        // 장비가 스폰된 후 호출
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // 장비가 Pawn에게 장착되지 않았거나 제거된 경우 파괴 처리
            if (destroy || !(ParentHolder is Pawn_ApparelTracker))
            {
                if (!parent.Destroyed)
                {
                    parent.Destroy();
                }
            }
        }

        // 장비가 제거될 때 호출
        public override void Notify_Unequipped(Pawn pawn)
        {
            if (!parent.Destroyed)
            {
                destroy = true;
            }
        }

        // 매 틱마다 호출
        public override void CompTick()
        {
            ticks--;

            // 설정된 주기마다 효과 적용
            if (ticks <= 0)
            {
                ticks = Props.ticks;

                if (parent is Apparel ap && ap.Wearer != null)
                {
                    // 장비를 잠금 처리하여 착용 해제를 방지
                    ap.Wearer.apparel.Lock(ap);

                    // 장비의 내구도(HP) 회복 처리
                    if (parent.HitPoints < parent.MaxHitPoints)
                    {
                        int recoverAmount = Props.hpRecovery;
                        int hpDifference = parent.MaxHitPoints - parent.HitPoints;

                        // 회복량이 최대 내구도 차이를 초과하지 않도록 설정
                        parent.HitPoints += hpDifference < recoverAmount ? hpDifference : recoverAmount;
                    }
                }
            }
        }
    }
}
