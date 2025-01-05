using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_PheromoneGenerator : CompProperties
    {
        public CompProperties_PheromoneGenerator() => compClass = typeof(Comp_PheromoneGenerator);
        public string label = "YR_Maggot_PheromoneGenerator_Active";
        public string desc = "YR_Maggot_PheromoneGenerator_Desc";
        public int ticks = 2500;
        public List<SpawnPawnSetting> spawnPawnSettings = new List<SpawnPawnSetting>();
        public int pawnSpawnRadius = 2;
        //public string message = "YR_Maggot_PheromoneGenerator_Message";
        //public MessageTypeDef messageTypeDef = MessageTypeDefOf.NeutralEvent;
        public SoundDef spawnSoundDef;
        public int fuel = 10;
    }

    public class SpawnPawnSetting
    {
        public PawnKindDef pawnKindDef;
        public IntRange spawnAmountRange = new IntRange(1, 2);
        public FactionDef defaultFactionType;
        public bool nullFaction = false;
        public bool playerFaction = false;
        public IntRange ageRange = new IntRange(0, 2);
        public List<Gender> genders = new List<Gender>();
        public SoundDef spawnSoundDef;
    }

    public class Comp_PheromoneGenerator : ThingComp
    {
        public CompProperties_PheromoneGenerator Props => (CompProperties_PheromoneGenerator)props;

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref active, "active");
            Scribe_Values.Look(ref ticks, "ticks");
        }

        private Building Building => (Building)parent;
        private CompRefuelable refuelableComp;
        bool active = false;
        int ticks = 2500;
        public override void PostPostMake()
        {
            refuelableComp = Building.GetComp<CompRefuelable>();
        }
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            refuelableComp = Building.GetComp<CompRefuelable>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (active)
            {
                if (Find.TickManager.TicksGame % 10 == 0)
                {
                    FleckMaker.ThrowAirPuffUp(parent.TrueCenter(), parent.Map);
                }
                ticks--;
                if (ticks <= 0)
                {
                    active = false;

                    foreach (var spawnPawnSetting in Props.spawnPawnSettings)
                    {
                        if (spawnPawnSetting.pawnKindDef != null)
                        {
                            PawnGenerationRequest request = new PawnGenerationRequest(spawnPawnSetting.pawnKindDef, allowDowned: true, fixedBiologicalAge: spawnPawnSetting.ageRange.RandomInRange);

                            //성별
                            if (!spawnPawnSetting.genders.NullOrEmpty())
                            {
                                System.Random random = new System.Random();
                                int num = random.Next(0, spawnPawnSetting.genders.Count);
                                request.FixedGender = spawnPawnSetting.genders[num];
                            }

                            //팩션
                            Faction fixedFaction = new Faction();
                            if (spawnPawnSetting.playerFaction)
                            {
                                fixedFaction = Faction.OfPlayer;
                            }
                            else if (spawnPawnSetting.nullFaction)
                            {
                                fixedFaction = null;
                            }
                            else if (spawnPawnSetting.defaultFactionType != null)
                            {
                                fixedFaction = FactionUtility.DefaultFactionFrom(spawnPawnSetting.defaultFactionType);
                            }
                            else
                            {
                                fixedFaction = FactionUtility.DefaultFactionFrom(spawnPawnSetting.pawnKindDef.defaultFactionType);
                            }

                            for (int i = 0; i < spawnPawnSetting.spawnAmountRange.RandomInRange; i++)
                            {
                                Pawn pawn = PawnGenerator.GeneratePawn(request);
                                //pawn.ageTracker.AgeBiologicalTicks = spawnPawnSetting.ageRange.RandomInRange * 3600000;

                                GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(parent.Position, parent.Map, Props.pawnSpawnRadius, null), parent.Map, WipeMode.Vanish);

                                pawn.SetFaction(fixedFaction);


                                SoundDef soundDef = null;

                                if (spawnPawnSetting.spawnSoundDef != null)
                                {
                                    soundDef = spawnPawnSetting.spawnSoundDef;
                                }
                                else if (Props.spawnSoundDef != null)
                                {
                                    soundDef = Props.spawnSoundDef;
                                }

                                soundDef?.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                            }

                            //if (!Props.message.NullOrEmpty())
                            //{
                            //    Messages.Message(Props.message.Translate(), Props.messageTypeDef);
                            //}
                        }
                    }
                }
            }
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            yield return new Command_Action
            {
                defaultLabel = Props.label.Translate(),
                defaultDesc = Props.desc.Translate(),
                Disabled = active || refuelableComp.Fuel < Props.fuel,
                icon = ContentFinder<Texture2D>.Get("Ui/Commands/CallAid"),
                action = delegate ()
                {
                    if (refuelableComp != null)
                    {
                        if (refuelableComp.Fuel >= Props.fuel)
                        {
                            refuelableComp.ConsumeFuel(Props.fuel);
                            active = true;
                            ticks = Props.ticks;
                        }
                    }
                    else
                    {
                        active = true;
                        ticks = Props.ticks;
                    }
                }
            };

            yield break;
        }
    }


}