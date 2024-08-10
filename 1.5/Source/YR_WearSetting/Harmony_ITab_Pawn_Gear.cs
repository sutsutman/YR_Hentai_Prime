using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_WearSetting
{
    [StaticConstructorOnStartup]
    public static class Harmony_ITab_Pawn_Gear
    {
        static Harmony_ITab_Pawn_Gear()
        {
            var harmony = new Harmony("Mincho_The_Mint_Choco_Slime");
            var original = AccessTools.Method(typeof(ITab_Pawn_Gear), "DrawThingRow");
            var prefix = AccessTools.Method(typeof(Harmony_ITab_Pawn_Gear), nameof(Harmony_ITab_Pawn_Gear.DrawThingRow_Prefix));
            harmony.Patch(original, new HarmonyMethod(prefix));
        }

        [HarmonyPrefix]
        public static bool DrawThingRow_Prefix(ITab_Pawn_Gear __instance, ref float y, float width, Thing thing, bool inventory)
        {
            Pawn SelPawnForGear = (Pawn)AccessTools.Property(typeof(ITab_Pawn_Gear), "SelPawnForGear").GetGetMethod(true).Invoke(__instance, null);
            bool CanControl = (bool)AccessTools.Property(typeof(ITab_Pawn_Gear), "CanControl").GetGetMethod(true).Invoke(__instance, null);
            bool CanControlColonist = (bool)AccessTools.Property(typeof(ITab_Pawn_Gear), "CanControlColonist").GetGetMethod(true).Invoke(__instance, null);

            //Log.Error(SelPawnForGear?.def?.defName ?? "null");
            //Log.Error(SelPawnForGear?.Name?.ToString() ?? "null");
            //Log.Error(thing?.def?.defName ?? "null");
            foreach (var wearSetting in
                    from thingComp_WearSetting in SelPawnForGear.GetComps<ThingComp_WearSetting>()
                    from wearSetting in thingComp_WearSetting.Props.wearSetting
                    from wearList in wearSetting.wearList
                    where thing.def.IsWeapon && thing.def == wearList
                    select wearSetting)
            {
                if (wearSetting.lockWeapon)
                {
                    WearSettingMethod.WearSettingBoolMaker(wearSetting,
                                                            SelPawnForGear,
                                                            out WearSettingBool wearSettingBool,
                                                            out WearSettingBoolString wearSettingBoolString);

                    if (wearSettingBool.cantWearBool)
                    {
                        return true;
                    }
                    else if (wearSettingBool.canWearBool)
                    {
                        Rect rect = new Rect(0f, y, width, 28f);
                        Widgets.InfoCardButton(rect.width - 24f, y, thing);
                        rect.width -= 24f;
                        bool flag = false;
                        if (CanControl && (inventory || CanControlColonist || (SelPawnForGear.Spawned && !SelPawnForGear.Map.IsPlayerHome)))
                        {
                            Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                            bool flag2 = false;
                            if (SelPawnForGear.IsQuestLodger())
                            {
                                flag2 = (inventory || !EquipmentUtility.QuestLodgerCanUnequip(thing, SelPawnForGear));
                            }
                            Apparel apparel;
                            bool flag3 = (apparel = (thing as Apparel)) != null && SelPawnForGear.apparel != null && SelPawnForGear.apparel.IsLocked(apparel);
                            flag = (flag2 || flag3);
                            if (Mouse.IsOver(rect2))
                            {
                                if (wearSetting.lockWeaponTipRegion != null)
                                {
                                    WearSettingToolTip(wearSetting.lockWeaponTipRegion,
                                                       thing,
                                                       SelPawnForGear,
                                                       wearSetting,
                                                       wearSettingBoolString,
                                                       rect2);
                                }
                                else
                                {
                                    WearSettingToolTip("LockWeaponTipRegion_Default",
                                                       thing,
                                                       SelPawnForGear,
                                                       wearSetting,
                                                       wearSettingBoolString,
                                                       rect2);
                                }
                            }
                            Color color = Color.grey;
                            Color mouseoverColor = color;
                            Widgets.ButtonImage(rect2, TexButton.Drop, color, mouseoverColor, !flag);
                            rect.width -= 24f;
                        }
                        if (CanControlColonist)
                        {
                            if (((thing.def.IsNutritionGivingIngestible || thing.def.IsNonMedicalDrug) && thing.IngestibleNow) || wearSetting.lockWeaponConsumeThingTipRegion != null)
                            {
                                Rect rect3 = new Rect(rect.width - 24f, y, 24f, 24f);

                                if (wearSetting.lockWeaponConsumeThingTipRegion != null)
                                {
                                    WearSettingToolTip(wearSetting.lockWeaponConsumeThingTipRegion,
                                                       thing,
                                                       SelPawnForGear,
                                                       wearSetting,
                                                       wearSettingBoolString,
                                                       rect3);
                                }
                                else
                                {
                                    WearSettingToolTip("LockWeaponConsumeThingTipRegion_Default",
                                                       thing,
                                                       SelPawnForGear,
                                                       wearSetting,
                                                       wearSettingBoolString,
                                                       rect3);
                                }

                                Color color = Color.grey;
                                Color mouseoverColor = color;
                                Widgets.ButtonImage(rect3, TexButton.Ingest, color, mouseoverColor, !flag);
                            }
                            rect.width -= 24f;
                        }
                        Rect rect4 = rect;
                        rect4.xMin = rect4.xMax - 60f;
                        CaravanThingsTabUtility.DrawMass(thing, rect4);
                        rect.width -= 60f;
                        if (Mouse.IsOver(rect))
                        {
                            GUI.color = ITab_Pawn_Gear.HighlightColor;
                            GUI.DrawTexture(rect, TexUI.HighlightTex);
                        }
                        if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
                        {
                            Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing, 1f, null);
                        }
                        Text.Anchor = TextAnchor.MiddleLeft;
                        GUI.color = ITab_Pawn_Gear.ThingLabelColor;
                        Rect rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
                        string text = thing.LabelCap;
                        Apparel apparel2 = thing as Apparel;
                        if (apparel2 != null && SelPawnForGear.outfits != null && SelPawnForGear.outfits.forcedHandler.IsForced(apparel2))
                        {
                            text += ", " + "ApparelForcedLower".Translate();
                        }
                        if (flag)
                        {
                            text += " (" + "ApparelLockedLower".Translate() + ")";
                        }
                        Text.WordWrap = false;
                        Widgets.Label(rect5, text.Truncate(rect5.width, null));
                        Text.WordWrap = true;
                        if (Mouse.IsOver(rect))
                        {
                            string text2 = thing.DescriptionDetailed;
                            if (thing.def.useHitPoints)
                            {
                                text2 = string.Concat(new object[]
                                {
                        text2,
                        "\n",
                        thing.HitPoints,
                        " / ",
                        thing.MaxHitPoints
                                });
                            }
                            TooltipHandler.TipRegion(rect, text2);
                        }
                        y += 28f;
                        return false;
                    }
                }
            }
            return true;

        }

        private static void WearSettingToolTip(string label, Thing thing, Pawn pawn, WearSetting wearSetting, WearSettingBoolString wearSettingBoolString, Rect rect)
        {
            TooltipHandler.TipRegion(rect, label.Translate(
                pawn.def.LabelCap,
                pawn.LabelShort,
                thing.def.LabelCap,
                wearSetting.condition.age.min,
                wearSetting.condition.age.max,
                wearSettingBoolString.genderString,
                wearSettingBoolString.bodyTypeString,
                wearSettingBoolString.orHediffString,
                wearSettingBoolString.andHediffString,
                wearSettingBoolString.forbiddenHediffString
                ).CapitalizeFirst());
        }
    }
}