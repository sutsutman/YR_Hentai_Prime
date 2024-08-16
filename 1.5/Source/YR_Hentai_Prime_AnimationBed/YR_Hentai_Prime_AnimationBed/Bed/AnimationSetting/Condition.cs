using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class PawnCondition
    {
        public Condition heldPawnCondition;
        public Condition joyPawnCondition;

        public bool allMatch = true;
    }

    public class Condition
    {
        //피학자
        public List<BodyTypeDef> bodyTypeDefs = null;
        public List<HediffDef> hediffDefs = null;
        public List<HediffAndSeverity> hediffAndSeverities = null;
        public List<Gender> genders = null;
        public List<ThingDef> races = null;
        public List<TraitDef> traitDefs = null;
        public List<Intelligence> intelligences = null;
        public List<SkillLevelRage> skillLevelRages = null;

        public List<BodyTypeDef> neverBodyTypeDefs = null;
        public List<HediffDef> neverHediffDefs = null;
        public List<HediffAndSeverity> neverHediffAndSeverities = null;
        public List<Gender> neverGenders = null;
        public List<ThingDef> neverRaces = null;
        public List<TraitDef> neverTraitDefs = null;
        public List<Intelligence> neverIntelligences = null;

        public bool dummyForJoyIsActive = false;
        public bool dummyForJoyIsDeactive = false;
        public bool referPreviousJoyPawn = false;

        public float probability = -1;

        public bool allMatch = false;
        public bool reverseCondition = false;
        public bool Break = false;

        public static bool Match(Pawn pawn, Building_AnimationBed building_AnimationBed, Condition condition, out bool needBreak)
        {
            // needBreak 처리 좀 더 검증 필요;
            if (condition == null)
            {
                needBreak = false;
                return true;
            }

            if (pawn == null)
            {
                needBreak = false;
                return false;
            }

            bool allMatch = condition.allMatch;

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
                needBreak = false;
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
                CheckListCondition(condition.traitDefs, TraitCheck),
                CheckDummyForJoyIsActive(building_AnimationBed, condition),
                CheckDummyForJoyIsDeactive(building_AnimationBed, condition),
                CheckProbability(condition.probability),
                CheckSkill(pawn,condition.skillLevelRages)
            };

            bool match = allMatch
                    ? matchConditions.All(x => x)
                    : matchConditions.Any(x => x);

            bool result = condition.reverseCondition ? !match : match;


            needBreak = false;
            if (result)
            {
                needBreak = condition.Break;
            }

            return result;

            // 로컬 함수
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

            bool CheckDummyForJoyIsActive(Building_AnimationBed building_AnimationBed, Condition condition)
                => allMatch
                ? !condition.dummyForJoyIsActive || (condition.dummyForJoyIsActive && building_AnimationBed.dummyForJoyIsActive)
                : condition.dummyForJoyIsActive && building_AnimationBed.dummyForJoyIsActive;

            bool CheckDummyForJoyIsDeactive(Building_AnimationBed building_AnimationBed, Condition condition)
    => allMatch
    ? !condition.dummyForJoyIsDeactive || (condition.dummyForJoyIsDeactive && !building_AnimationBed.dummyForJoyIsActive)
    : condition.dummyForJoyIsDeactive && !building_AnimationBed.dummyForJoyIsActive;

            bool CheckProbability(float probability)
            {
                if (probability > 0)
                {
                    if (probability >= Rand.Range(0, 1f))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return allMatch;
                }
            }

            bool CheckSkill(Pawn pawn, List<SkillLevelRage> skillLevelRages)
            {
                if (skillLevelRages == null)
                {
                    return allMatch;
                }

                if (allMatch)
                {
                    foreach (var skillLevelRage in skillLevelRages)
                    {
                        var level = pawn.skills.GetSkill(skillLevelRage.skillDef).Level;
                        if (pawn.skills.GetSkill(skillLevelRage.skillDef).TotallyDisabled)
                        {
                            level = 0;
                        }

                        if (level < skillLevelRage.levelRange.min || level > skillLevelRage.levelRange.max)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    foreach (var skillLevelRage in skillLevelRages)
                    {
                        var level = pawn.skills.GetSkill(skillLevelRage.skillDef).Level;
                        if (skillLevelRage.levelRange.min <= level && level <= skillLevelRage.levelRange.max)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            bool HediffCheck(Pawn pawn, List<HediffDef> hediffDefs)
                => pawn.health.hediffSet.hediffs.Any(pawnHediff => hediffDefs.Contains(pawnHediff.def));

            bool HediffAndSeverityCheck(Pawn pawn, List<HediffAndSeverity> hediffAndSeverities)
               => pawn.health.hediffSet.hediffs.Any(pawnHediff =>
                       hediffAndSeverities.Any(hediffAndSeveritie =>
                           pawnHediff.def == hediffAndSeveritie.hediffDef &&
                           hediffAndSeveritie.severityRange.min <= pawnHediff.Severity &&
                           pawnHediff.Severity <= hediffAndSeveritie.severityRange.max));

            bool TraitCheck(Pawn pawn, List<TraitDef> traitDefs)
               => pawn.story.traits.allTraits.Any(pawnTrait => traitDefs.Contains(pawnTrait.def));
        }

        public static bool ExecuteActionIfConditionMatches(Building_AnimationBed building_AnimationBed, PawnCondition pawnCondition, Action action, Pawn anotherPawn = null)
        {

            if (pawnCondition == null)
            {
                action();
                return false;
            }

            var heldPawn = building_AnimationBed.HeldPawn;
            if (anotherPawn != null)
            {
                heldPawn = anotherPawn;
            }

            bool heldPawnConditionMatch = Match(heldPawn, building_AnimationBed, pawnCondition.heldPawnCondition, out bool heldPawnNeedBreak);

            var joyPawn = building_AnimationBed.dummyForJoyPawn;
            if (pawnCondition.joyPawnCondition != null && pawnCondition.joyPawnCondition.referPreviousJoyPawn)
            {
                joyPawn = building_AnimationBed.previousJoyPawn;
            }

            bool joyPawnConditionMatch = Match(joyPawn, building_AnimationBed, pawnCondition.joyPawnCondition, out bool joyPawnNeedBreak);


            if (pawnCondition.allMatch)
            {
                if (heldPawnConditionMatch && joyPawnConditionMatch)
                {
                    action();
                }
            }
            else
            {
                if (heldPawnConditionMatch || joyPawnConditionMatch)
                {
                    action();
                }

            }


            return heldPawnNeedBreak || joyPawnNeedBreak;
        }

    }

    public class SkillLevelRage
    {
        public SkillDef skillDef;
        public IntRange levelRange;
    }

    public class HediffAndSeverity
    {
        public HediffDef hediffDef;
        public FloatRange severityRange;
    }

}
