using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_SpawnDummyForJoy : CompProperties
    {
        public CompProperties_SpawnDummyForJoy() => compClass = typeof(CompSpawnDummyForJoy);

        public ThingDef thingDef;

        public AnimationDef animationDef;

        public int waitAfterJoyTick = 0;

        public MakeHediff makeHediff = new MakeHediff();
        public MakeSound makeSound = new MakeSound();
    }

    public class MakeSound
    {
        public PawnSound heldPawnSound = new PawnSound();
        public PawnSound joyPawnSound = new PawnSound();

    }

    public class PawnSound
    {
        public List<SoundSetting> startSoundSettings = new List<SoundSetting>();
        public List<SoundSetting> randomSoundSettings = new List<SoundSetting>();
        public List<SoundSetting> finishSoundSettings = new List<SoundSetting>();
        public List<SoundSetting> waitAfterJoySoundSettings = new List<SoundSetting>();
    }

    public class MakeHediff
    {
        public PawnHediffSetting heldPawnHediffSetting = new PawnHediffSetting();
        public PawnHediffSetting joyPawnHediffSetting = new PawnHediffSetting();
    }

    public class PawnHediffSetting
    {
        public List<HediffSetting> startHediffSettings = new List<HediffSetting>();
        public List<HediffSetting> finishHediffSettings = new List<HediffSetting>();
    }

    public class HediffSetting
    {
        public HediffDef hediffDef;

        public bool removeWhenFinish = false;

        public List<ConditionHediffDefs> conditionHediffDefs = new List<ConditionHediffDefs>();
    }

    public class ConditionHediffDefs
    {
        public HediffDef hediffDef;
        public Condition condition;
    }

    public class CompSpawnDummyForJoy : CompBaseOfAnimationBed
    {
        public CompProperties_SpawnDummyForJoy Props => (CompProperties_SpawnDummyForJoy)props;

        public Thing spawnThing = null;

        public CompDummyForJoy DummyForJoyComp => spawnThing?.TryGetComp<CompDummyForJoy>();
        public override void Notify_HeldOnPlatform(ThingOwner newOwner)
        {
            base.Notify_HeldOnPlatform(newOwner);

            spawnThing = GenSpawn.Spawn(Props.thingDef, parent.Position, parent.Map);

            if (DummyForJoyComp != null)
            {
                DummyForJoyComp.building_AnimationBed = Building_AnimationBed;
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
