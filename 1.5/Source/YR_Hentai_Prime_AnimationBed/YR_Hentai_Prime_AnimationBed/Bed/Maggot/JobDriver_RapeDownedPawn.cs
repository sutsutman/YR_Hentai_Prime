﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JobDriver_RapeDownedPawn : JobDriver
    {
        protected Pawn Takee
        {
            get
            {
                return (Pawn)job.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnAggroMentalStateAndHostile(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => !Takee.Downed).FailOnSomeonePhysicallyInteracting(TargetIndex.A);

            //대기
            yield return Toils_General.WaitWith(TargetIndex.A, 100, useProgressBar: true);

            yield return Toils_Haul.StartCarryThing(TargetIndex.A);

            yield return Toils_General.Do(delegate
            {
                var comp = pawn.TryGetComp<Comp_Maggot_Queen>();
                Building_AnimationBed bed = (Building_AnimationBed)GenSpawn.Spawn(comp.Props.bedDef, pawn.Position, pawn.Map);
                bed.SetFaction(Faction.OfPlayer);
                var compAnimationBed = bed.TryGetComp<CompAnimationBed>();

                JobDriver_CarryToAnimationBed.ChainTakeeToPlatform(pawn, Takee, compAnimationBed);
            });

            //yield return Toils_General.Do(delegate
            //{
            //    var comp = pawn.TryGetComp<Comp_Maggot_Queen>();
            //    Building_AnimationBed bed = (Building_AnimationBed)GenSpawn.Spawn(comp.Props.bedDef, Takee.Position, Takee.Map);
            //    bed.SetFaction(Faction.OfPlayer);
            //    var compAnimationBed = bed.TryGetComp<CompAnimationBed>();

            //    if (compAnimationBed != null)
            //    {
            //        Takee.DeSpawnOrDeselect(DestroyMode.Vanish);
            //        bool flag = false;
            //        if (Takee.holdingOwner != null)
            //        {
            //            Takee.holdingOwner.TryTransferToContainer(Takee, bed.innerContainer, Takee.stackCount, true);
            //            flag=true;
            //        }
            //        else
            //        {
            //            flag = bed.innerContainer.TryAdd(Takee, true);
            //        }
            //        // 폰을 ThingOwner에 추가
            //        if (flag)
            //        {
            //            // 컴포넌트와 연결
            //            CompAnimationBedTarget compTarget = Takee.TryGetComp<CompAnimationBedTarget>();
            //            if (compTarget != null)
            //            {
            //                compTarget.Notify_HeldOnPlatform(compAnimationBed.Container);
            //            }
            //            else
            //            {
            //                Log.Error("Pawn does not have CompAnimationBedTarget.");
            //            }
            //        }
            //        else
            //        {
            //            Log.Error("Failed to add pawn to AnimationBed container.");
            //        }
            //    }
            //    else
            //    {
            //        Log.Error("Destination holder does not have CompAnimationBed.");
            //    }
            //});

        }
        private const TargetIndex TakeeIndex = TargetIndex.A;
    }

}
