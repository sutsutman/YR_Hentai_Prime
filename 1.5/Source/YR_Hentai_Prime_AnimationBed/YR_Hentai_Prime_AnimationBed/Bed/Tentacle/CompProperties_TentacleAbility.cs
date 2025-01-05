//using RimWorld;
//using System.Collections.Generic;
//using UnityEngine;
//using Verse;
//using Verse.Sound;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class CompProperties_TentacleAbility : CompProperties
//    {
//        public int consumption = 10;
//        public List<HediffDef> hediffDefs = new List<HediffDef>();
//        public ThingDef filthDef;
//        public int filthNum = 1;
//        public float time = 4f;
//        public float enemyTime = 4f;
//        public SoundDef sound;
//        public string label = "YR_TentacleAbility_Label";
//        public string description = "YR_TentacleAbility_Desc";
//        public string disabledReason = "YR_TentacleAbility_DisabledReason";
//        public string message = "YR_TentacleAbility_Message";
//        public string iconPath = "Yuran/Icon/Wiggle";
//        public EffecterDef centerEffecter;

//        public CompProperties_TentacleAbility()
//        {
//            compClass = typeof(CompTentacleAbility);
//        }
//    }

//    public class CompTentacleAbility : Comp_Building_BaseBondageBed
//    {
//        public Building_Tentacle_Altar BTA => (Building_Tentacle_Altar)parent;
//        public CompProperties_TentacleAbility Props => (CompProperties_TentacleAbility)props;

//        public override IEnumerable<Gizmo> CompGetGizmosExtra()
//        {
//            if (BTA.CorpseFulfilled && BTA.refuelableComp != null && BTA.refuelableComp.Fuel >= Props.consumption)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = Props.label.Translate(),
//                    defaultDesc = Props.description.Translate(Props.consumption, Props.time, Props.enemyTime),
//                    icon = ContentFinder<Texture2D>.Get(Props.iconPath),
//                    action = delegate ()
//                    {
//                        BTA.refuelableComp.ConsumeFuel(Props.consumption);
//                        if (!Props.message.NullOrEmpty())
//                        {
//                            Messages.Message(Props.message.Translate(BTA.Label), MessageTypeDefOf.PositiveEvent, true);
//                        }
//                        if (Props.sound != null)
//                        {
//                            Props.sound.PlayOneShot(new TargetInfo(BTA.Position, BTA.Map, false));
//                        }
//                        if (Props.centerEffecter != null)
//                        {
//                            Props.centerEffecter.Spawn(BTA.Position, BTA.Map, Vector3.zero, 1f);
//                        }
//                        foreach (Pawn pawn in BTA.Map.mapPawns.AllPawnsSpawned)
//                        {
//                            if (pawn != null && !pawn.Dead && pawn.Spawned)
//                            {
//                                Verb_EggVibrator.SwitchOn(pawn, Props.time, Props.enemyTime, Props.hediffDefs, null, Props.filthDef, Props.filthNum);
//                            }
//                        }
//                    },
//                    disabled = false,
//                    disabledReason = ""
//                };
//            }
//        }
//    }
//}
