using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_SpawnDummyForJoy : CompProperties
    {
        public CompProperties_SpawnDummyForJoy() => compClass = typeof(CompSpawnDummyForJoy);

        public ThingDef thingDef;

        public AnimationDef animationDef;
    }

    public class CompSpawnDummyForJoy : CompBaseOfAnimationBed
    {
        public CompProperties_SpawnDummyForJoy Props => (CompProperties_SpawnDummyForJoy)props;

        public Thing spawnThing = null;
        public override void Notify_HeldOnPlatform(ThingOwner newOwner)
        {
            base.Notify_HeldOnPlatform(newOwner);

            spawnThing = GenSpawn.Spawn(Props.thingDef, parent.Position, parent.Map);

            var comp = spawnThing.TryGetComp<CompDummyForJoy>();
            if (comp != null)
            {
                comp.building_AnimationBed = Building_AnimationBed;
            }
        }

        public override void Notify_ReleasedFromPlatform()
        {
            base.Notify_ReleasedFromPlatform();

            spawnThing?.Destroy();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref spawnThing, "spawnThing");
        }
    }
}
