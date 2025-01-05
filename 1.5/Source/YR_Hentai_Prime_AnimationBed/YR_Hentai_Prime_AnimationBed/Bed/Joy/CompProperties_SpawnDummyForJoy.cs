using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_SpawnDummyForJoy : CompProperties
    {
        public CompProperties_SpawnDummyForJoy() => compClass = typeof(CompSpawnDummyForJoy);

        public ThingDef thingDef;

        public AnimationDef animationDef;
        public List<ConditionAnimationDef> conditionAnimationDefs = new List<ConditionAnimationDef>();

        public int waitAfterJoyTick = 0;

        public MakeHediff makeHediff = new MakeHediff();
        public MakeSound makeSound = new MakeSound();
    }
    public class ConditionAnimationDef
    {
        public AnimationDef animationDef;
        public PawnCondition pawnCondition;
    }

    public class MakeSound
    {
        public PawnSound heldPawnSound = new PawnSound();
        public PawnSound joyPawnSound = new PawnSound();

    }

    public class PawnSound
    {
        public List<SoundSettingDef> startSoundSettingDefs = new List<SoundSettingDef>();
        public List<SoundSettingDef> randomSoundSettingDefs = new List<SoundSettingDef>();
        public List<SoundSettingDef> finishSoundSettingDefs = new List<SoundSettingDef>();
        public List<SoundSettingDef> waitAfterJoySoundSettingDefs = new List<SoundSettingDef>();
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
        public PawnCondition pawnCondition;
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
