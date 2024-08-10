using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace YR_WearSetting
{
    public static class WearSettingMethod
    {
        public static void WearSettingBoolMaker(WearSetting wearSetting, Pawn pawn, out WearSettingBool wearSettingBool, out WearSettingBoolString wearSettingBoolString)
        {
            Condition condition = wearSetting.condition;

            List<HediffDef> hediffDefs = pawn.health.hediffSet.hediffs.Select(pawnHediff => pawnHediff.def).ToList();

            bool age = condition.age.min <= pawn.ageTracker.AgeBiologicalYears && pawn.ageTracker.AgeBiologicalYears <= condition.age.max;

            int match = condition.andHediffDef.NullOrEmpty() ? 0 : condition.andHediffDef.Count(item => hediffDefs.Contains(item));
            bool andHediff = match >= condition.andHediffDef.Count;

            bool forbiddenHediff = !condition.forbiddenHediffDef.NullOrEmpty() && condition.forbiddenHediffDef.Any(item => hediffDefs.Contains(item));

            bool mustWearWith = condition.mustWearWith.NullOrEmpty() || pawn.apparel.WornApparel.Any(item => condition.mustWearWith.Contains(item.def));

            bool forceDropHediffBool = ForceDropHediffBool(wearSetting, pawn);

            wearSettingBoolString = new WearSettingBoolString
            {
                genderString = ConditionCheck(pawn.gender, condition.gender, out bool gender),
                bodyTypeString = ConditionCheck(pawn.story.bodyType, condition.bodyTypeDef, out bool bodyType),
                orHediffString = ConditionCheck(hediffDefs, condition.orHediffDef, out bool orHediff),
                andHediffString = StringMaker(condition.andHediffDef),
                forbiddenHediffString = StringMaker(condition.forbiddenHediffDef),
                mustWearWithString = StringMaker(condition.mustWearWith)
            };

            wearSettingBool = new WearSettingBool
            {
                age = age,
                bodyType = bodyType,
                gender = gender,
                orHediff = orHediff,
                andHediff = andHediff,
                mustWearWith = mustWearWith,
                forbiddenHediff = forbiddenHediff,
                forceDropHediffBool = forceDropHediffBool,
                cantWearBool = forceDropHediffBool || !age || !bodyType || !gender || !orHediff || !andHediff || !mustWearWith || forbiddenHediff,
                canWearBool = age && bodyType && gender && orHediff && andHediff && mustWearWith && !forbiddenHediff
            };

            // 디버그
            TestError(wearSettingBool, age, andHediff, forbiddenHediff, mustWearWith, forceDropHediffBool, gender, bodyType, orHediff);
        }

        private static void TestError(WearSettingBool wearSettingBool, bool age, bool andHediff, bool forbiddenHediff, bool mustWearWith, bool forceDropHediffBool, bool gender, bool bodyType, bool orHediff)
        {
            bool test = false;
            if (test)
            {
                Log.Error("cantWearBool : " + wearSettingBool.cantWearBool.ToString());
                Log.Error("canWearBool : " + wearSettingBool.canWearBool.ToString());
                Log.Error("forceDropHediffBool : " + forceDropHediffBool.ToString());
                Log.Error("age : " + age.ToString());
                Log.Error("bodyType : " + bodyType.ToString());
                Log.Error("gender : " + gender.ToString());
                Log.Error("orHediff : " + orHediff.ToString());
                Log.Error("andHediff : " + andHediff.ToString());
                Log.Error("forbiddenHediff : " + forbiddenHediff.ToString());
                Log.Error("mustWearWith : " + mustWearWith.ToString());
            }
        }

        private static string ConditionCheck<T>(T pawnCondition, List<T> conditionList, out bool Bool)
        {
            Bool = conditionList.NullOrEmpty() || conditionList.Contains(pawnCondition);
            return StringMaker(conditionList);
        }

        private static string ConditionCheck<T>(List<T> pawnConditionList, List<T> conditionList, out bool Bool)
        {
            Bool = conditionList.NullOrEmpty() || conditionList.Any(item => pawnConditionList.Contains(item));
            return StringMaker(conditionList);
        }

        private static string StringMaker<T>(List<T> condition)
        {
            StringBuilder conditionString = new StringBuilder();
            for (int i = 0; i < condition.Count; i++)
            {
                string itemString;
                if (condition[i] is Def conditionDef)
                {
                    if (conditionDef.LabelCap.NullOrEmpty())
                    {
                        itemString = condition[i].ToString();
                    }
                    else
                    {
                        itemString = conditionDef.LabelCap;
                    }
                }
                else
                {
                    itemString = condition[i].ToString();
                }

                conditionString.Append(itemString);

                if (i < condition.Count - 1)
                {
                    conditionString.AppendLine();
                    conditionString.AppendLine();
                }
            }

            return conditionString.ToString();
        }

        private static bool ForceDropHediffBool(WearSetting wearSetting, Pawn pawn)
        {
            return wearSetting.forceDropHediff.Any(forceDropHediff => pawn.health.hediffSet.HasHediff(forceDropHediff, false));
        }

        public static Dictionary<ApparelLayerDef, Func<WearSetting, bool>> WearSettingDictionary = new Dictionary<ApparelLayerDef, Func<WearSetting, bool>>
                {
                    { ApparelLayerDefOf.OnSkin, (wearSetting) => wearSetting.onlyWearListApparel.onSkin },
                    { ApparelLayerDefOf.Shell, (wearSetting) => wearSetting.onlyWearListApparel.shell },
                    { ApparelLayerDefOf.Middle, (wearSetting) => wearSetting.onlyWearListApparel.middle },
                    { ApparelLayerDefOf.Belt, (wearSetting) => wearSetting.onlyWearListApparel.belt },
                    { ApparelLayerDefOf.Overhead, (wearSetting) => wearSetting.onlyWearListApparel.overhead },
                    { ApparelLayerDefOf.EyeCover, (wearSetting) => wearSetting.onlyWearListApparel.eyeCover },
                };
    }
}
