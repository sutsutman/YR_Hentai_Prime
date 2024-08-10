using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_DummyForJoy : CompProperties
    {
        public CompProperties_DummyForJoy() => compClass = typeof(CompDummyForJoy);
    }

    public class CompDummyForJoy : ThingComp
    {
        public CompProperties_DummyForJoy Props => (CompProperties_DummyForJoy)props;

        public Building_AnimationBed building_AnimationBed = null;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref building_AnimationBed, "building_AnimationBed");
        }
    }
}
