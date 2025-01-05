using RimWorld;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Blink : CompProperties
    {
        public CompProperties_Blink() => compClass = typeof(CompBlink);
        public int glowTicks = 1000;
        public int blinkTicks = 100;
        public bool neverTimeBlink = true;
    }

    public class CompBlink : ThingComp
    {
        public CompProperties_Blink Props => (CompProperties_Blink)props;
        public CompPowerTrader CompPowerTrader => this.parent.TryGetComp<CompPowerTrader>();

        int ticks = 1000;
        bool turnOff = false;
        private bool colorNeedChange = true;
        bool first = true;

        public override void CompTick()
        {
            base.CompTick();

            if (CompPowerTrader.PowerOn)
            {
                if (first)
                {
                    ticks = Props.glowTicks;
                    first = false;
                }
                colorNeedChange = true;
                if (!Props.neverTimeBlink)
                {
                    ticks--;
                    if (ticks <= 0)
                    {
                        if (turnOff)
                        {
                            this.parent.DrawColor = Color.black;
                            turnOff = false;
                            //YR_RJW_Setting.SetTicksByCurTimeSpeed(ref ticks, Props.blinkTicks);
                        }
                        else
                        {
                            this.parent.DrawColor = Color.white;
                            turnOff = true;
                            //YR_RJW_Setting.SetTicksByCurTimeSpeed(ref ticks, Props.glowTicks);
                        }
                    }
                }
                else
                {
                    this.parent.DrawColor = Color.white;
                }
            }
            else
            {
                if (colorNeedChange)
                {
                    this.parent.DrawColor = Color.black;
                    colorNeedChange = false;
                }
            }
        }
    }
}