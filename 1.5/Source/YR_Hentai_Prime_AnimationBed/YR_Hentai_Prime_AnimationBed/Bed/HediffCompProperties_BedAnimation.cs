using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_BedAnimation : HediffCompProperties
    {
        public HediffCompProperties_BedAnimation() => compClass = typeof(HediffComp_BedAnimation);

        public bool postAdd = false;
        public bool postRemoved = false;
    }

    public class HediffComp_BedAnimation : HediffComp
    {
        public HediffCompProperties_BedAnimation Props => (HediffCompProperties_BedAnimation)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Pawn.holdingOwner.Owner is Building_AnimationBed building_AnimationBed && Props.postAdd)
            {
                building_AnimationBed.AnimationSettingComp.needMakeGraphics = true;
                building_AnimationBed.setAnimation = true;
            }
        }
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn.holdingOwner.Owner is Building_AnimationBed building_AnimationBed && Props.postRemoved)
            {
                building_AnimationBed.AnimationSettingComp.needMakeGraphics = true;
                building_AnimationBed.setAnimation = true;
            }
        }
    }
}
