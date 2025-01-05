using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class CompPowerPlant_Building_AnimationBed : CompPowerPlant
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
                        return -Props.PowerConsumption;
                    }
                }

                return 0;
            }
        }
    }
}
