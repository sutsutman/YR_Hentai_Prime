using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_WearSetting
{
    //메소드
    public class WearSettingDef : Def
    {
        public List<ThingDef> targetRaces;
        //public ThingDef targetRace = new ThingDef();
        public List<WearSetting> wearSetting = new List<WearSetting>();
    }

    public class WearSetting
    {
        public string label = "";
        public List<ThingDef> wearList = new List<ThingDef>();
        public Condition condition = new Condition();
        public string cantReason = null;
        public List<HediffDef> forceDropHediff = new List<HediffDef>();
        public bool forceDrop = true;
        public bool forceDropOnlyMatchCondition = false;
        public string forceDropMessage = null;
        public bool lockApparel = false;
        public string lockApparelMessage = null;
        public List<AddHediff> addHediff = new List<AddHediff>();
        public BoolString boolString = new BoolString();
        public bool lockWeapon = false;
        public string lockWeaponConsumeThingTipRegion;
        public string lockWeaponTipRegion;
        public string lockWeaponCantReason;
        public bool makeThingCopyHitPoint = true;
        public ThingDef makeWeapon = null;
        public List<ThingDef> makeApparel = new List<ThingDef>();
        public List<ThingDef> makeApparelDontDrop = new List<ThingDef>();
        public List<ThingDef> makeWeaponDontDrop = new List<ThingDef>();
        public bool destroyAfterDrop = false;
        public List<ThingDef> destroyWeaponPostDrop = new List<ThingDef>();
        public OnlyWearListApparel onlyWearListApparel = new OnlyWearListApparel();
        public bool prisonerRemoveWeapon = true;
        public List<ThingDef> lockApparelMessageApparels = new List<ThingDef>();
    }

    public class OnlyWearListApparel
    {
        public bool onSkin = false;
        public bool shell = false;
        public bool middle = false;
        public bool belt = false;
        public bool overhead = false;
        public bool eyeCover = false;
        public bool weapon = false;
    }

    public class BoolString
    {
        public string ageFalse;
        public string ageTrue;
        public string genderTrue;
        public string genderFalse;
        public string bodyTypeTrue;
        public string bodyTypeFalse;
        public string orHediffTrue;
        public string orHediffFalse;
        public string andHediffTrue;
        public string andHediffFalse;
        public string forbiddenHediffTrue;
        public string forbiddenHediffFalse;
        public string mustWearWithFalse;
        public string mustWearWithTrue;
    }

    public class Condition
    {
        public Age age = new Age();
        public List<Gender> gender = new List<Gender>();
        public List<BodyTypeDef> bodyTypeDef = new List<BodyTypeDef>();
        public List<HediffDef> orHediffDef = new List<HediffDef>();
        public List<HediffDef> andHediffDef = new List<HediffDef>();
        public List<HediffDef> forbiddenHediffDef = new List<HediffDef>();
        public List<ThingDef> mustWearWith = new List<ThingDef>();
    }

    public class AddHediff
    {
        public List<HediffDef> hediff = new List<HediffDef>();
        public List<BodyPartDef> partsToAffect = new List<BodyPartDef>();
        public string addHediffMessage = null;
    }

    public class Age
    {
        public int min = 0;
        public int max = 9999;
    }


}
