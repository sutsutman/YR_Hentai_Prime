using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class CompPowerPlant_Building_BaseBondageBed : CompPowerPlant
    {
        protected override float DesiredPowerOutput => RoofedPowerOutputFactor;
        private float RoofedPowerOutputFactor
        {
            get
            {
                if (parent is Building_AnimationBed building_AnimationBed)
                {
                    if (building_AnimationBed.HeldPawn != null)
                    {
                        return -base.Props.PowerConsumption;
                    }
                }

                return 0;
            }
        }
    }
}
