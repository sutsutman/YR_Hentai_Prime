using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_WearSetting
{
    [StaticConstructorOnStartup]
    //대상 종족에 콤프 추가
    public class AddComp
    {
        static AddComp()
        {
            var allThingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
            var allWearSettingDefs = DefDatabase<WearSettingDef>.AllDefsListForReading;

            foreach (var wearSettingDef in allWearSettingDefs)
            {
                if (wearSettingDef.targetRaces == null)
                {
                    for (int i = 0; i < allThingDefs.Count; i++)
                    {
                        var thingDef = allThingDefs[i];
                        if (thingDef.race?.Humanlike == true)
                        {
                            var compProperties_WearSetting = new CompProperties_WearSetting
                            {
                                targetRace = thingDef,
                                wearSetting = wearSettingDef.wearSetting
                            };
                            thingDef.comps.Add(compProperties_WearSetting);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < wearSettingDef.targetRaces.Count; i++)
                    {
                        var targetRace = wearSettingDef.targetRaces[i];
                        var compProperties_WearSetting = new CompProperties_WearSetting
                        {
                            targetRace = targetRace,
                            wearSetting = wearSettingDef.wearSetting
                        };
                        targetRace.comps.Add(compProperties_WearSetting);
                    }
                }
            }
        }
    }
    public class PawnCondition
    {
        public BodyTypeDef bodyTypeDef = null;
        public List<HediffDef> hediffDefs = new List<HediffDef>();
        public Gender? gender = null;
        public ThingDef race = null;
        public List<TraitDef> traitDefs;
    }

    public class CompProperties_WearSetting : CompProperties
    {
        public CompProperties_WearSetting()
        {
            this.compClass = typeof(ThingComp_WearSetting);
        }
        public ThingDef targetRace = new ThingDef();
        public List<WearSetting> wearSetting = new List<WearSetting>();
    }
    public class ThingComp_WearSetting : ThingComp
    {
        public CompProperties_WearSetting Props
        {
            get
            {
                return (CompProperties_WearSetting)this.props;
            }
        }
        public List<string> apparelDefNames = new List<string>();

        bool checkLater = false;
        public override void Notify_KilledPawn(Pawn pawn)
        {
            base.Notify_KilledPawn(pawn);
            checkLater = true;
        }
        private PawnCondition compPawnCondition = null;
        private PawnCondition originalPawnCondition = null;

        public override void CompTickRare()
        {
            if (pawn != null && pawn.Spawned)
            {
                base.CompTickRare();

                CheckNeedMakeNew(out List<string> tempApparelDefNames, out bool makeNew);

                if (!makeNew)
                {
                    return;
                }

                // 변경사항이 있다면, 기존 의복 목록 업데이트
                apparelDefNames = tempApparelDefNames;

                string addHediffString = "";
                string partsToAffectString = "";

                foreach (WearSetting wearSetting in Props.wearSetting)
                {
                    WearSettingMethod.WearSettingBoolMaker(wearSetting,
                                                           pawn,
                                                           out WearSettingBool wearSettingBool,
                                                           out WearSettingBoolString wearSettingBoolString);

                    bool sendLockApparelMessage = true;
                    foreach (ThingDef wearThingDef in wearSetting.wearList)
                    {
                        // 의복
                        if (wearThingDef.IsApparel)
                        {
                            ProcessApparel(wearThingDef, wearSetting, wearSettingBool, wearSettingBoolString, addHediffString, partsToAffectString, sendLockApparelMessage);
                        }

                        // 무기
                        else if (wearThingDef.IsWeapon)
                        {
                            ProcessWeapon(wearSetting, wearSettingBool, wearSettingBoolString, addHediffString, partsToAffectString);
                        }
                    }
                }

                UpdateApparelDefNames();

            }
        }

        private void CheckNeedMakeNew(out List<string> tempApparelDefNames, out bool makeNew)
        {
            if (pawn.Downed)
            {
                checkLater = true;
            }

            tempApparelDefNames = pawn.apparel.WornApparel.Select(a => a.def.defName).ToList();

            makeNew = true;
            if (apparelDefNames.SequenceEqual(tempApparelDefNames))
            {
                makeNew = false;
            }

            if (checkLater && !pawn.Downed)
            {
                makeNew = true;
                checkLater = false;
                return;
            }

            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            List<Trait> traits = pawn.story.traits.allTraits;

            if (compPawnCondition == null)
            {
                compPawnCondition = new PawnCondition
                {
                    race = pawn.def,
                    gender = pawn.gender,
                    hediffDefs = hediffs.Select(x => x.def).ToList(),
                    bodyTypeDef = pawn.story.bodyType,
                    traitDefs = traits.Select(x => x.def).ToList()
                };
                makeNew = true;
                return;
            }
            else
            {
                originalPawnCondition = new PawnCondition
                {
                    race = pawn.def,
                    gender = pawn.gender,
                    bodyTypeDef = pawn.story.bodyType
                };
            }

            bool race = compPawnCondition.race == originalPawnCondition.race;
            bool gender = compPawnCondition.gender == originalPawnCondition.gender;

            bool hediffDef = hediffs.Select(x => x.def).SequenceEqual(compPawnCondition.hediffDefs);

            bool bodyTypeDef = compPawnCondition.bodyTypeDef == originalPawnCondition.bodyTypeDef;

            bool traitDef = traits.Select(x => x.def).SequenceEqual(compPawnCondition.traitDefs);

            bool allEqual = race && gender && hediffDef && bodyTypeDef && traitDef;

            if (!allEqual)
            {
                compPawnCondition = new PawnCondition
                {
                    race = pawn.def,
                    gender = pawn.gender,
                    hediffDefs = hediffs.Select(x => x.def).ToList(),
                    bodyTypeDef = pawn.story.bodyType,
                    traitDefs = traits.Select(x => x.def).ToList()
                };
                makeNew = true;
            }
        }

        private bool MustDrop(WearSetting wearSetting, Apparel wornApparel)
        {
            foreach (var layer in wornApparel.def.apparel.layers)
            {
                if (WearSettingMethod.WearSettingDictionary.ContainsKey(layer))
                {
                    if (WearSettingMethod.WearSettingDictionary[layer](wearSetting))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void ProcessApparel(ThingDef thingDef, WearSetting wearSetting, WearSettingBool wearSettingBool, WearSettingBoolString wearSettingBoolString, string addHediffString, string partsToAffectString, bool sendLockApparelMessage)
        {
            foreach (var wornApparel in pawn.apparel.WornApparel.ToList())
            {
                if (wornApparel.def == thingDef)
                {
                    if (wearSettingBool.canWearBool)
                    {
                        AddHediff(ref addHediffString, ref partsToAffectString, wearSetting, wornApparel, wearSettingBoolString);
                        LockApparel(pawn.apparel, wearSetting, wornApparel, wearSettingBoolString, wearSetting.lockApparelMessage, ref sendLockApparelMessage);
                    }
                    else if (wearSettingBool.cantWearBool)
                    {
                        if (wearSetting.forceDrop || wearSettingBool.forceDropHediffBool)
                        {
                            ApparelDrop(wearSetting, wearSettingBoolString, wornApparel);
                        }
                    }

                    MakeWeaponAndApparel(wearSetting, wornApparel);
                }
                else if (!wearSetting.wearList.Contains(wornApparel.def) && wearSettingBool.canWearBool && wearSetting.forceDropOnlyMatchCondition)
                {
                    if (MustDrop(wearSetting, wornApparel))
                    {
                        ApparelDrop(wearSetting, wearSettingBoolString, wornApparel);
                    }
                }
            }
        }
        private void ProcessWeapon(WearSetting wearSetting, WearSettingBool wearSettingBool, WearSettingBoolString wearSettingBoolString, string addHediffString, string partsToAffectString)
        {
            foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading)
            {
                if (wearSettingBool.canWearBool && wearSetting.forceDrop && wearSetting.onlyWearListApparel.weapon == true)
                {
                    if (wearSettingBool.cantWearBool)
                    {
                        if (wearSetting.forceDrop || wearSettingBool.forceDropHediffBool || ((pawn.IsPrisoner || pawn.IsPrisonerInPrisonCell() || pawn.IsPrisonerOfColony) && wearSetting.prisonerRemoveWeapon))
                        {
                            DropItem(wearSetting, wearSettingBoolString, equipment);
                        }
                    }
                    else
                    {
                        AddHediff(ref addHediffString, ref partsToAffectString, wearSetting, equipment, wearSettingBoolString);
                    }
                    MakeWeaponAndApparel(wearSetting, equipment);
                }
            }
        }

        private void DropItem(WearSetting wearSetting, WearSettingBoolString wearSettingBoolString, ThingWithComps wornItem)
        {
            pawn.equipment.TryDropEquipment(wornItem, out ThingWithComps thingWithCompsTemp, pawn.Position, true);

            if (wearSetting.destroyAfterDrop)
            {
                thingWithCompsTemp.Destroy();
            }

            if (wearSetting.forceDropMessage != null)
            {
                WearSettingMessages(wearSetting.forceDropMessage, wearSetting, wornItem, wearSettingBoolString);
            }
        }

        private void UpdateApparelDefNames()
        {
            apparelDefNames = new List<string>();
            foreach (var apparel in pawn.apparel.WornApparel)
            {
                apparelDefNames.Add(apparel.def.defName);
            }
        }

        private void ApparelDrop(WearSetting wearSetting, WearSettingBoolString wearSettingBoolString, Apparel wornApparel)
        {
            pawn.apparel.TryDrop(wornApparel, out Apparel apparelTemp, pawn.Position, true);

            if (wearSetting.destroyAfterDrop)
            {
                apparelTemp.Destroy();
            }

            if (wearSetting.forceDropMessage != null)
            {
                WearSettingMessages(wearSetting.forceDropMessage, wearSetting, wornApparel, wearSettingBoolString);
            }
        }

        private void MakeWeaponAndApparel(WearSetting wearSetting, ThingWithComps pawnWorn)
        {
            //무기 장착
            MakeWeapon(wearSetting, pawnWorn);
            //의류 장착
            MakeApparel(wearSetting, pawnWorn);
        }

        private void MakeWeapon(WearSetting wearSetting, ThingWithComps pawnWorn)
        {
            if ((pawn.IsPrisoner || pawn.IsPrisonerInPrisonCell() || pawn.IsPrisonerOfColony) && wearSetting.prisonerRemoveWeapon)
            {
                return;
            }

            if (wearSetting.makeWeapon != null)
            {
                ThingDef makeWeapon = wearSetting.makeWeapon;
                if (!makeWeapon.IsWeapon)
                {
                    Log.ErrorOnce($"{makeWeapon.defName} is not Weapon", 100);
                }
                else
                {
                    bool noMore = false;
                    foreach (var equipment in pawn.equipment.AllEquipmentListForReading.ToList())
                    {
                        if (equipment?.def == makeWeapon || wearSetting.makeWeaponDontDrop.Any(dontDrop => equipment?.def == dontDrop))
                        {
                            return;
                        }
                    }
                    if (!noMore)
                    {
                        ThingWithComps weapon = null;
                        if (makeWeapon.MadeFromStuff)
                        {
                            if ((bool)pawnWorn?.def.MadeFromStuff)
                            {
                                weapon = MakeWeaponFromPawnWornStuff(makeWeapon, pawnWorn);
                            }
                            else if (makeWeapon.defaultStuff != null)
                            {
                                weapon = (ThingWithComps)ThingMaker.MakeThing(makeWeapon, makeWeapon.defaultStuff);
                            }
                            else
                            {
                                weapon = MakeWeaponFromStuffCategories(makeWeapon);
                            }
                        }
                        else
                        {
                            weapon = (ThingWithComps)ThingMaker.MakeThing(makeWeapon);
                        }

                        if (weapon != null)
                        {
                            SetQualityAndHitPoints(weapon, pawnWorn, wearSetting.makeThingCopyHitPoint);
                            foreach (var equipment in pawn.equipment.AllEquipmentListForReading.ToList())
                            {
                                if ((bool)(equipment?.def.IsWeapon))
                                {
                                    pawn.equipment.TryDropEquipment(equipment, out ThingWithComps thingWithCompsTemp, pawn.Position, true);
                                    foreach (var destroyWeaponPostDrop in wearSetting.destroyWeaponPostDrop)
                                    {
                                        if (thingWithCompsTemp.def == destroyWeaponPostDrop)
                                        {
                                            thingWithCompsTemp.Destroy();
                                        }
                                    }
                                }
                            }
                            pawn.equipment.AddEquipment(weapon);
                        }
                    }
                }
            }
        }

        static ThingWithComps MakeWeaponFromPawnWornStuff(ThingDef makeWeapon, ThingWithComps pawnWorn)
        {
            foreach (var stuffCategory1 in pawnWorn.Stuff.stuffCategories)
            {
                if (makeWeapon.stuffCategories.Contains(stuffCategory1))
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, pawnWorn.Stuff);
                }
            }
            return null;
        }

        static ThingWithComps MakeWeaponFromStuffCategories(ThingDef makeWeapon)
        {
            foreach (var stuffCategory in makeWeapon.stuffCategories)
            {
                if (stuffCategory == StuffCategoryDefOf.Fabric)
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, ThingDefOf.Cloth);
                }
                else if (stuffCategory == StuffCategoryDefOf.Metallic)
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, ThingDefOf.Steel);
                }
                else if (stuffCategory == StuffCategoryDefOf.Woody)
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, ThingDefOf.WoodLog);
                }
                else if (stuffCategory == StuffCategoryDefOf.Leathery)
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, ThingDefOf.Leather_Plain);
                }
                else if (stuffCategory == StuffCategoryDefOf.Stony)
                {
                    return (ThingWithComps)ThingMaker.MakeThing(makeWeapon, ThingDefOf.Sandstone);
                }
            }
            return null;
        }

        static void SetQualityAndHitPoints(ThingWithComps weapon, ThingWithComps pawnWorn, bool makeThingCopyHitPoint)
        {
            if (pawnWorn.TryGetQuality(out QualityCategory qualityCategory))
            {
                weapon?.TryGetComp<CompQuality>()?.SetQuality(qualityCategory, ArtGenerationContext.Colony);
            }
            if (makeThingCopyHitPoint)
            {
                var hitPointsMultiple = Math.Round((double)pawnWorn.HitPoints / (double)pawnWorn.MaxHitPoints, 3);
                var hitpoint = (int)(weapon.MaxHitPoints * hitPointsMultiple);
                weapon.HitPoints = hitpoint;
            }
        }

        void MakeApparel(WearSetting wearSetting, ThingWithComps pawnWorn)
        {
            List<ThingDef> tempApparelList = wearSetting.makeApparel.ToList();

            if (!wearSetting.makeApparel.NullOrEmpty())
            {
                foreach (var makeApparel in tempApparelList.ToList())
                {
                    if (!makeApparel.IsApparel)
                    {
                        Log.ErrorOnce($"{makeApparel.defName} is not Apparel", 100);
                    }
                    else
                    {
                        if (pawn.apparel.WornApparel.Any(pawnApparel => pawnApparel.def == makeApparel))
                        {
                            tempApparelList.Remove(makeApparel);
                        }

                        foreach (var dontDrop in wearSetting.makeApparelDontDrop)
                        {
                            if (pawn.apparel.WornApparel.Any(pawnApparel => pawnApparel.def == dontDrop))
                            {
                                CheckApparelLayers(ref tempApparelList, makeApparel, dontDrop);
                            }
                        }
                    }
                }

                foreach (var makeApparel in tempApparelList)
                {
                    ThingWithComps app = null;
                    if (makeApparel.MadeFromStuff)
                    {
                        if ((bool)pawnWorn?.def.MadeFromStuff)
                        {
                            app = MakeApparelFromPawnWornStuff(makeApparel, pawnWorn);
                        }
                        else if (makeApparel.defaultStuff != null)
                        {
                            app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, makeApparel.defaultStuff);
                        }
                        else
                        {
                            app = MakeApparelFromStuffCategories(makeApparel);
                        }
                    }
                    else
                    {
                        app = (ThingWithComps)ThingMaker.MakeThing(makeApparel);
                    }

                    if (app != null)
                    {
                        SetQualityAndHitPoints(app, pawnWorn, wearSetting.makeThingCopyHitPoint);
                        pawn.apparel.Wear((Apparel)app);
                    }
                }
            }

            static ThingWithComps MakeApparelFromPawnWornStuff(ThingDef makeApparel, ThingWithComps pawnWorn)
            {
                foreach (var stuffCategory1 in pawnWorn.Stuff.stuffCategories)
                {
                    if (makeApparel.stuffCategories.Contains(stuffCategory1))
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, pawnWorn.Stuff);
                    }
                }
                return null;
            }

            static ThingWithComps MakeApparelFromStuffCategories(ThingDef makeApparel)
            {
                foreach (var stuffCategory in makeApparel.stuffCategories)
                {
                    if (stuffCategory == StuffCategoryDefOf.Fabric)
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Cloth);
                    }
                    else if (stuffCategory == StuffCategoryDefOf.Metallic)
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Steel);
                    }
                    else if (stuffCategory == StuffCategoryDefOf.Woody)
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.WoodLog);
                    }
                    else if (stuffCategory == StuffCategoryDefOf.Leathery)
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Leather_Plain);
                    }
                    else if (stuffCategory == StuffCategoryDefOf.Stony)
                    {
                        return (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Sandstone);
                    }

                }
                return null;
            }

            static void SetQualityAndHitPoints(ThingWithComps app, ThingWithComps pawnWorn, bool makeThingCopyHitPoint)
            {
                if (pawnWorn.TryGetQuality(out QualityCategory qualityCategory))
                {
                    app.TryGetComp<CompQuality>()?.SetQuality(qualityCategory, ArtGenerationContext.Colony);
                }

                if (makeThingCopyHitPoint)
                {
                    double hitPointsMultiple = (double)pawnWorn.HitPoints / (double)pawnWorn.MaxHitPoints;
                    int hitpoint = (int)(app.MaxHitPoints * hitPointsMultiple);
                    app.HitPoints = hitpoint;
                }
            }

            static void CheckApparelLayers(ref List<ThingDef> tempApparelList, ThingDef makeApparel, ThingDef dontDrop)
            {
                foreach (var makeApparelLayer in makeApparel.apparel.layers)
                {
                    foreach (var dontDropApparelLayer in dontDrop.apparel.layers)
                    {
                        if (makeApparelLayer == dontDropApparelLayer)
                        {
                            tempApparelList.Remove(makeApparel);
                            return;
                        }
                    }
                }
            }
        }

        void LockApparel(Pawn_ApparelTracker apparel, WearSetting wearSetting, Apparel wornApparel, WearSettingBoolString wearSettingBoolString, string label, ref bool sendLockApparelMessage)
        {
            if (wearSetting.lockApparel && !apparel.LockedApparel.Contains(wornApparel))
            {
                apparel.Lock(wornApparel);

                if (wearSetting.lockApparelMessage != null && (wearSetting.lockApparelMessageApparels.NullOrEmpty() || wearSetting.lockApparelMessageApparels.Contains(wornApparel.def)))
                {
                    WearSettingMessages(label, wearSetting, wornApparel, wearSettingBoolString);
                    sendLockApparelMessage = false;
                }
            }
        }

        void AddHediff(ref string addHediffString, ref string partsToAffectString, WearSetting wearSetting, Thing thing, WearSettingBoolString wearSettingBoolString)
        {
            foreach (AddHediff addHediff in wearSetting.addHediff)
            {
                addHediffString = "";
                partsToAffectString = "";

                foreach (HediffDef hediffDef in addHediff.hediff)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediffDef, false))
                    {
                        if (HediffGiverUtility.TryApply(pawn, hediffDef, addHediff.partsToAffect))
                        {
                            addHediffString += hediffDef.LabelCap;
                            if (addHediff.hediff.Last() != hediffDef)
                            {
                                addHediffString += ", ";
                            }
                        }
                    }
                }

                foreach (BodyPartDef part in addHediff.partsToAffect)
                {
                    partsToAffectString += part.LabelCap;
                    if (addHediff.partsToAffect.Last() != part)
                    {
                        partsToAffectString += ", ";
                    }
                }

                if (addHediff.addHediffMessage != null && !addHediffString.Equals(""))
                {
                    Messages.Message(addHediff.addHediffMessage.Translate(
                        pawn.def.LabelCap,
                        pawn.LabelShort,
                        thing.def.LabelCap,
                        //여기 위치 바뀜 주의
                        addHediffString,
                        partsToAffectString,
                        wearSetting.condition.age.min,
                        wearSetting.condition.age.max,
                        wearSettingBoolString.genderString,
                        wearSettingBoolString.bodyTypeString,
                        wearSettingBoolString.orHediffString,
                        wearSettingBoolString.andHediffString,
                        wearSettingBoolString.forbiddenHediffString,
                        wearSettingBoolString.mustWearWithString
                    ).CapitalizeFirst(), pawn, MessageTypeDefOf.NeutralEvent, true);
                }
            }
        }

        void WearSettingMessages(string label, WearSetting wearSetting, Thing thing, WearSettingBoolString wearSettingBoolString)
        {
            Messages.Message(label.Translate(
                                pawn.def.LabelCap,
                                pawn.LabelShort,
                                thing.def.LabelCap,
                                wearSetting.condition.age.min,
                                wearSetting.condition.age.max,
                                wearSettingBoolString.genderString,
                                wearSettingBoolString.bodyTypeString,
                                wearSettingBoolString.orHediffString,
                                wearSettingBoolString.andHediffString,
                                wearSettingBoolString.forbiddenHediffString,
                                wearSettingBoolString.mustWearWithString
                                ).CapitalizeFirst(), pawn, MessageTypeDefOf.NeutralEvent, true);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.pawn = (this.parent as Pawn);
        }
        public Pawn pawn { get; private set; }
    }
}
