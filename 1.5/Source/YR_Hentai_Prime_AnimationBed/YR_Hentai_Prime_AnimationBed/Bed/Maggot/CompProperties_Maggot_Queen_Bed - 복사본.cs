using RimWorld;
using System.Linq;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_Maggot_Queen_Bed : CompProperties
    {
        public CompProperties_Maggot_Queen_Bed() => compClass = typeof(Comp_Maggot_Queen_Bed);
        public int tendTick = 100;
        public int destroySelfAndPawnTicks = -10000;
    }

    public class Comp_Maggot_Queen_Bed : ThingComp
    {
        public CompProperties_Maggot_Queen_Bed Props => (CompProperties_Maggot_Queen_Bed)props;

        public CompAnimationBed CompAnimationBed => parent.TryGetComp<CompAnimationBed>();

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            destroySelfAndPawnTicks = Props.destroySelfAndPawnTicks;
        }

        int tick = -10;
        int destroySelfAndPawnTicks = -10000;
        public override void CompTick()
        {
            base.CompTick();
            if (tick < -1)
            {
                tick = Props.tendTick;
            }
            if (CompAnimationBed.HeldPawn == null)
            {
                parent.Kill();
            }
            else
            {
                tick--;

                if (tick <= 0)
                {
                    tick = Props.tendTick;

                    foreach (var hediff in CompAnimationBed.HeldPawn.health.hediffSet.GetHediffsTendable().ToList())
                    {
                        hediff.Tended(0.01f, 0.05f);
                        break;
                    }
                }
            }

            if (destroySelfAndPawnTicks > 0)
            {
                destroySelfAndPawnTicks--;

                if (destroySelfAndPawnTicks <= 0)
                {
                    var comp = parent.TryGetComp<Comp_Maggot_Warp>();

                    bool destroyed = false;
                    if (comp == null)
                    {
                        destroyed = true;
                    }
                    else
                    {
                        if (!comp.WarpPawnBool())
                        {
                            destroyed = true;
                        }
                    }
                    if (destroyed)
                    {
                        YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));

                        Find.LetterStack.ReceiveLetter("YR_DestroySelfAndPawn_Label".Translate(CompAnimationBed.HeldPawn.LabelShort, CompAnimationBed.HeldPawn).CapitalizeFirst(), "YR_DestroySelfAndPawn_Desc".Translate(CompAnimationBed.HeldPawn.LabelShort, CompAnimationBed.HeldPawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));

                        CompAnimationBed.HeldPawn.DropAndForbidEverything();
                        CompAnimationBed.HeldPawn.apparel.DropAll(parent.Position, true);

                        CompAnimationBed.HeldPawn.Destroy();
                        parent.Destroy();
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref tick, "tick", -10000);
            Scribe_Values.Look(ref destroySelfAndPawnTicks, "destroySelfAndPawnTicks", -10000);

        }
    }
}