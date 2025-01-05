using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Tentacle_Weapon : CompProperties
    {
        public CompProperties_Tentacle_Weapon()
        {
            compClass = typeof(Comp_Tentacle_Weapon);
        }
    }

    public class Comp_Tentacle_Weapon : ThingComp
    {
        private bool destroty = false;
        private CompProperties_Tentacle_Weapon Props => (CompProperties_Tentacle_Weapon)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (destroty || !(ParentHolder is Pawn_EquipmentTracker))
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
    }
}
