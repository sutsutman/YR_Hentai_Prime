using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class JobDriver_SelfTieToAnimationBed : JobDriver
    {
        private const TargetIndex DestHolderIndex = TargetIndex.A;

        private const int EnterDelayTicks = 300;

        private Thing DestHolder => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // 예약이 가능한지 확인하고, 예약합니다.
            return pawn.Reserve(DestHolder, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 대상이 유효하지 않으면 작업 실패 처리
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !((CompAnimationBed)DestHolder.TryGetComp<CompAnimationBed>()).Available);

            // 대상 가구로 이동
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);

            // 가구에 자신을 묶기 전 대기 (애니메이션 및 진행 표시용)
            yield return Toils_General.WaitWith(TargetIndex.A, EnterDelayTicks, useProgressBar: true);

            // 자신을 가구에 묶기
            yield return Toils_General.Do(delegate
            {
                var compAnimationBed = DestHolder.TryGetComp<CompAnimationBed>();

                if (compAnimationBed != null)
                {
                    // 폰을 ThingOwner에 추가
                    CompAnimationBedTarget compTarget = pawn.TryGetComp<CompAnimationBedTarget>();
                    if (compTarget != null)
                    {
                        if (compAnimationBed.Container.TryAdd(pawn.SplitOff(1)))
                        {
                            //pawn.DeSpawn(); // 맵에서 제거
                            compTarget.Notify_HeldOnPlatform(compAnimationBed.Container);
                        }
                        else
                        {
                            Log.Error("Failed to add pawn to AnimationBed container.");
                        }
                    }
                    else
                    {
                        Log.Error("Failed to add pawn to AnimationBed container, because this pawn don't have CompAnimationBedTarget.");
                    }
                }
                else
                {
                    Log.Error("Destination holder does not have CompAnimationBed.");
                }
            });

        }
    }


}
