using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Tentacle_Apparel : CompProperties
    {
        public CompProperties_Tentacle_Apparel()
        {
            compClass = typeof(Comp_Tentacle_Apparel);
        }

        public int ticks = 10;
        public int hpRecovery = 1;
    }

    public class Comp_Tentacle_Apparel : ThingComp
    {
        private int ticks = 0;
        private bool destroty = false;
        private CompProperties_Tentacle_Apparel Props => (CompProperties_Tentacle_Apparel)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (destroty || !(ParentHolder is Pawn_ApparelTracker))
            {
                if (!parent.Destroyed)
                {
                    parent.Destroy();
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            if (!parent.Destroyed)
            {
                destroty = true;
            }
        }

        public override void CompTick()
        {
            ticks--;
            if (ticks <= 0)
            {
                ticks = Props.ticks;
                if (parent is Apparel ap)
                {
                    if (ap.Wearer != null)
                    {
                        ap.Wearer.apparel.Lock(ap);

                        if (parent.HitPoints < parent.MaxHitPoints)
                        {
                            if (parent.MaxHitPoints - parent.HitPoints < Props.hpRecovery)
                            {
                                parent.HitPoints += parent.MaxHitPoints - parent.HitPoints;
                            }
                            else
                            {
                                parent.HitPoints += Props.hpRecovery;
                            }
                        }
                    }
                }
            }
        }
    }
}
