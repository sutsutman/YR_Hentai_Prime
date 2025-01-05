using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using Random = System.Random;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_SoundAndMote : CompProperties
    {
        public CompProperties_SoundAndMote() => compClass = typeof(Comp_SoundAndMote);
        public SoundDef soundAmbient;

        public List<ThingDef> moteDefs;

        public int ticks = 100;
    }

    public class Comp_SoundAndMote : CompBaseOfAnimationBed
    {
        private Sustainer sustainerAmbient;
        public CompProperties_SoundAndMote Props => (CompProperties_SoundAndMote)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }
        public int ticks = 0;
        public override void CompTick()
        {
            base.CompTick();

            if (Building_AnimationBed.PowerOn && Building_AnimationBed.HeldPawn != null)
            {
                ticks--;
                if (ticks <= 0)
                {
                    Random rand = new Random();

                    ticks = Props.ticks + rand.Next(-30, 30);

                    if (!Props.moteDefs.NullOrEmpty())
                    {
                        MoteMaker.MakeAttachedOverlay(parent, Props.moteDefs.RandomElement(), new Vector3(), 1f, -1f);
                    }
                }

                if (Props.soundAmbient != null)
                {
                    if (sustainerAmbient == null || sustainerAmbient.Ended)
                    {
                        sustainerAmbient = Props.soundAmbient.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.None));
                    }
                    sustainerAmbient.Maintain();
                }

                return;
            }
            if (sustainerAmbient != null)
            {
                sustainerAmbient.End();
                sustainerAmbient = null;
            }
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            sustainerAmbient?.End();
        }
    }
}