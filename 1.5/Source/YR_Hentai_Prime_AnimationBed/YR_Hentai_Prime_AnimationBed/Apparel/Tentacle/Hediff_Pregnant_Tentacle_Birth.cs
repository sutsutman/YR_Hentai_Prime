using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Hediff_Pregnant_Tentacle_Birth : HediffWithComps
    {
        private int ticks = 0;
        private int subTicks = 0;
        private bool birth = false;
        public override void PostAdd(DamageInfo? dinfo)
        {
            HediffComp_Pregnant_Tentacle_Birth hediffComp_Pregnant_Tentacle_Birth = new HediffComp_Pregnant_Tentacle_Birth();

            foreach (HediffComp comp in comps)
            {
                if (comp is HediffComp_Pregnant_Tentacle_Birth)
                {
                    hediffComp_Pregnant_Tentacle_Birth = (HediffComp_Pregnant_Tentacle_Birth)comp;
                    break;
                }
            }

            ticks = hediffComp_Pregnant_Tentacle_Birth.Props.ticks;
            subTicks = ticks + (ticks / 2);
            if (pawn.Faction == Faction.OfPlayer)
            {
                Messages.Message("YR_Birth_Tentacle_Start".Translate(), MessageTypeDefOf.PositiveEvent, true);
            }
        }

        public override void Tick()
        {
            ticks--;
            subTicks--;

            if (0 >= ticks && !birth)
            {
                DoBirthSpawn(pawn);
                birth = true;
            }
            if (0 >= subTicks)
            {
                pawn.health.RemoveHediff(this);
            }
        }

        public void DoBirthSpawn(Pawn mother)
        {

            HediffComp_Pregnant_Tentacle_Birth hediffComp_Pregnant_Tentacle_Birth = new HediffComp_Pregnant_Tentacle_Birth();

            foreach (HediffComp comp in comps)
            {
                if (comp is HediffComp_Pregnant_Tentacle_Birth)
                {
                    hediffComp_Pregnant_Tentacle_Birth = (HediffComp_Pregnant_Tentacle_Birth)comp;
                    break;
                }
            }
            if (hediffComp_Pregnant_Tentacle_Birth == null)
            {
                return;
            }

            ThingDef thingToSpawn = hediffComp_Pregnant_Tentacle_Birth.Props.thingDef;
            int spawnCount = hediffComp_Pregnant_Tentacle_Birth.Props.spawnCount;

            if (CompSpawner.TryFindSpawnCell(mother, thingToSpawn, spawnCount, out IntVec3 center))
            {
                Thing thing = ThingMaker.MakeThing(thingToSpawn, null);
                thing.stackCount = spawnCount;
                if (thing == null)
                {
                    Log.Error("Could not spawn anything for " + mother);
                }

                GenPlace.TryPlaceThing(thing, center, mother.Map, ThingPlaceMode.Direct, out Thing t, null, null, default);

                if (mother.Faction == Faction.OfPlayer)
                {
                    Messages.Message("YR_Birth_Tentacle".Translate(thingToSpawn.LabelCap), thing, MessageTypeDefOf.PositiveEvent, true);
                }
            }

            if (mother.Spawned)
            {
                FilthMaker.TryMakeFilth(mother.Position, mother.Map, ThingDefOf.Filth_AmnioticFluid, mother.LabelIndefinite(), 5, FilthSourceFlags.None);
                if (mother.caller != null)
                {
                    mother.caller.DoCall();
                }

                ConvenientTool.ShotSexSound(mother, hediffComp_Pregnant_Tentacle_Birth.Props.femaleEroVoice, null, null, 100);

            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticks, "ticks", 0, false);
            Scribe_Values.Look(ref subTicks, "subTicks", 0, false);
        }
    }
}
