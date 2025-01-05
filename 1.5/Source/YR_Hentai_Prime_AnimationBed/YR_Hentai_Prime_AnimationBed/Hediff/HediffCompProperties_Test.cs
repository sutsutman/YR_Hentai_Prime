using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_Test : HediffCompProperties
    {
        public HediffCompProperties_Test() => compClass = typeof(HediffComp_Test);

        public BodyTypeDef bodyTypeDef = BodyTypeDefOf.Hulk;
    }


    public class HediffComp_Test : HediffComp
    {
        private HediffCompProperties_Test Props => (HediffCompProperties_Test)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            parent.pawn.story.bodyType = Props.bodyTypeDef;

            parent.pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}
