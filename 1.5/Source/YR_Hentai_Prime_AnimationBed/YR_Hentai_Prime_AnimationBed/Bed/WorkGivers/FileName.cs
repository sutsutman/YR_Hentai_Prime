using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class WorkGiver_FeedVictim : WorkGiver_VictimOnAnimationBed
    {
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }

            return GetVictim(t) != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn victim = GetVictim(t);
            if (victim == null)
            {
                return null;
            }


            if (victim.needs.food == null)
            {
                return null;
            }

            if (victim.needs.food.CurLevelPercentage >= victim.needs.food.PercentageThreshHungry + 0.02f)
            {
                return null;
            }

            if (victim.foodRestriction != null)
            {
                FoodPolicy currentRespectedRestriction = victim.foodRestriction.GetCurrentRespectedRestriction(pawn);
                if (currentRespectedRestriction != null && currentRespectedRestriction.filter.AllowedDefCount == 0)
                {
                    JobFailReason.Is("NoFoodMatchingRestrictions".Translate());
                    return null;
                }
            }

            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, victim, victim.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, canRefillDispenser: false, canUseInventory: true, canUsePackAnimalInventory: false, allowForbidden: false, allowCorpse: false))
            {
                JobFailReason.Is("NoFood".Translate());
                return null;
            }

            float nutrition = FoodUtility.GetNutrition(victim, foodSource, foodDef);
            Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, foodSource, victim);
            job.count = FoodUtility.WillIngestStackCountOf(victim, foodDef, nutrition);
            return job;
        }

        public override string JobInfo(Pawn pawn, Job job)
        {
            if (FoodUtility.MoodFromIngesting((Pawn)(Thing)job.targetB, job.targetA.Thing, FoodUtility.GetFinalIngestibleDef(job.targetA.Thing)) < 0f)
            {
                return string.Format("({0})", "WarningFoodDisliked".Translate());
            }

            return string.Empty;
        }
        protected override Pawn GetVictim(Thing potentialPlatform)
        {
            return GetTendableVictimFromPotentialPlatform(potentialPlatform);
        }

        public static Pawn GetTendableVictimFromPotentialPlatform(Thing potentialPlatform)
        {
            if (potentialPlatform is Building_AnimationBed Building_AnimationBed)
            {
                Pawn heldPawn = Building_AnimationBed.HeldPawn;
                if (heldPawn == null)
                {
                    return null;
                }

                CompAnimationBedTarget CompAnimationBedTarget = heldPawn.TryGetComp<CompAnimationBedTarget>();
                if (CompAnimationBedTarget != null && (CompAnimationBedTarget.containmentMode == EntityContainmentMode.Release || CompAnimationBedTarget.containmentMode == EntityContainmentMode.Execute))
                {
                    return null;
                }

                return heldPawn;
            }

            return null;
        }
    }
}
