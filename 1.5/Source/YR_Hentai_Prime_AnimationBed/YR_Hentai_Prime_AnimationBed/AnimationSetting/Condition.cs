using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Condition
    {
        public List<BodyTypeDef> bodyTypeDefs = null;
        public List<HediffDef> hediffDefs = null;
        public List<HediffAndSeverity> hediffAndSeverities = null;
        public List<Gender> genders = null;
        public List<ThingDef> races = null;
        public List<TraitDef> traitDefs = null;
        public List<Intelligence> intelligences = null;

        public List<BodyTypeDef> neverBodyTypeDefs = null;
        public List<HediffDef> neverHediffDefs = null;
        public List<HediffAndSeverity> neverHediffAndSeverities = null;
        public List<Gender> neverGenders = null;
        public List<ThingDef> neverRaces = null;
        public List<TraitDef> neverTraitDefs = null;
        public List<Intelligence> neverIntelligences = null;

        public bool allMatch = false;
        public bool reverseCondition = false;
        bool Break = true;


        public static bool NeedBreak(Condition condition)
        {
            return condition != null && condition.Break;
        }

        public static bool Match(Pawn pawn, Condition condition)
        {
            if (condition == null || pawn == null)
            {
                return true;
            }

            bool allMatch = condition.allMatch;

            bool CheckNever<T>(T pawnCondition, List<T> conditionList)
                => !conditionList.NullOrEmpty() && conditionList.Contains(pawnCondition);

            bool Check<T>(T pawnCondition, List<T> conditionList)
                => allMatch
                    ? conditionList == null || conditionList.Contains(pawnCondition)
                    : conditionList != null && conditionList.Contains(pawnCondition);

            bool CheckListCondition<T>(List<T> conditionList, Func<Pawn, List<T>, bool> checkFunc, bool isNever = false)
                => isNever
                    ? !conditionList.NullOrEmpty() && checkFunc(pawn, conditionList)
                    : conditionList.NullOrEmpty() ? allMatch : checkFunc(pawn, conditionList);

            BodyTypeDef bodyType = pawn.story?.bodyType;

            IEnumerable<bool> neverConditions = new[]
            {
                CheckNever(bodyType, condition.neverBodyTypeDefs),
                CheckNever(pawn.gender, condition.neverGenders),
                CheckNever(pawn.def, condition.neverRaces),
                CheckNever(pawn.def.race.intelligence, condition.neverIntelligences),
                CheckListCondition(condition.neverHediffDefs, HediffCheck, true),
                CheckListCondition(condition.neverHediffAndSeverities, HediffAndSeverityCheck, true),
                CheckListCondition(condition.neverTraitDefs, TraitCheck, true)
            };

            if (neverConditions.Any(x => x))
            {
                return false;
            }

            IEnumerable<bool> matchConditions = new[]
            {
                Check(bodyType, condition.bodyTypeDefs),
                Check(pawn.gender, condition.genders),
                Check(pawn.def, condition.races),
                Check(pawn.def.race.intelligence, condition.intelligences),
                CheckListCondition(condition.hediffDefs, HediffCheck),
                CheckListCondition(condition.hediffAndSeverities, HediffAndSeverityCheck),
                CheckListCondition(condition.traitDefs, TraitCheck)
            };

            bool match = allMatch
                ? matchConditions.All(x => x)
                : matchConditions.Any(x => x);

            return condition.reverseCondition ? !match : match;

            // 로컬 함수
            static bool HediffCheck(Pawn pawn, List<HediffDef> hediffDefs)
                => pawn.health.hediffSet.hediffs.Any(pawnHediff => hediffDefs.Contains(pawnHediff.def));

            static bool HediffAndSeverityCheck(Pawn pawn, List<HediffAndSeverity> hediffAndSeverities)
                => pawn.health.hediffSet.hediffs.Any(pawnHediff =>
                        hediffAndSeverities.Any(hediffAndSeveritie =>
                            pawnHediff.def == hediffAndSeveritie.hediffDef &&
                            hediffAndSeveritie.severityRange.min <= pawnHediff.Severity &&
                            pawnHediff.Severity <= hediffAndSeveritie.severityRange.max));

            static bool TraitCheck(Pawn pawn, List<TraitDef> traitDefs)
                => pawn.story.traits.allTraits.Any(pawnTrait => traitDefs.Contains(pawnTrait.def));
        }

    }
    public class HediffAndSeverity
    {
        public HediffDef hediffDef;
        public FloatRange severityRange;
    }

}
