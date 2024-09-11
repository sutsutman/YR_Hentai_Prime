using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JoyGiver_InteractBuildingInteractionCell_DummyForJoy : JoyGiver_InteractBuildingInteractionCell
    {
        protected override Job TryGivePlayJob(Pawn pawn, Thing t)
        {
            if (t.InteractionCell.Standable(t.Map) && !t.IsForbidden(pawn) && !t.InteractionCell.IsForbidden(pawn) && !pawn.Map.pawnDestinationReservationManager.IsReserved(t.InteractionCell) && pawn.CanReserveSittableOrSpot(t.InteractionCell, false))
            {
                CompDummyForJoy comp = t.TryGetComp<CompDummyForJoy>();

                if (comp != null && comp.building_AnimationBed.PowerOn)
                {
                    return JobMaker.MakeJob(this.def.jobDef, t, t.InteractionCell);
                }
            }
            return null;
        }
    }
}
