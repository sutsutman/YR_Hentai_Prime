using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_ChangeBodyType : HediffCompProperties
    {
        public HediffCompProperties_ChangeBodyType()
        {
            compClass = typeof(HediffCompChangeBodyType);
        }

        public string bodyTypePostfix = "";

        public List<string> preferencePostfixs = new List<string>();
        public List<string> unPreferencePostfixs = new List<string>();

        public float severity = 0.5f;
        public int ticks = 600;
        public bool checkOnce = false;
        public bool firstZeroTick = false;

        public CompSetting compSetting = new CompSetting();

        public List<ThingDef> races = new List<ThingDef>();
    }
    public class CompSetting
    {
        public CompSet compPostTick = new CompSet();
        public CompSet compPostPostAdd = new CompSet();
        public CompSet compPostPostRemoved = new CompSet();
    }

    public class CompSet
    {
        public bool activeComp = false;
        public List<HediffDef> addHediffDefs;
    }

    public class HediffCompChangeBodyType : HediffComp
    {
        private string replacedBodyTypePostfix = "";
        private int ticks = 600;
        private bool checkOnce = false;

        private HediffCompProperties_ChangeBodyType Props => (HediffCompProperties_ChangeBodyType)props;

        public bool CheckHuman()
        {
            return parent.pawn.def.race.Humanlike && parent.pawn.story?.bodyType != null;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref replacedBodyTypePostfix, "replacedBodyTypePostfix");
            Scribe_Values.Look(ref checkOnce, "checkOnce");
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Props.compSetting.compPostTick.activeComp && CheckHuman())
            {
                if (checkOnce)
                {
                    return;
                }
                if (Props.firstZeroTick)
                {
                    ticks = 0;
                }
                ticks--;
                if (ticks <= 0)
                {
                    if (parent.Severity >= Props.severity && !parent.pawn.story.bodyType.defName.Contains(Props.bodyTypePostfix) && Props.bodyTypePostfix != "" && Props.races.Contains(parent.pawn.def))
                    {
                        //Log.Error($"{parent.def.defName} : 1");
                        string bodyType = parent.pawn.story.bodyType.defName;

                        bodyType = bodyType.Replace(Props.bodyTypePostfix, "");

                        foreach (string unPreferencePostfixs in Props.unPreferencePostfixs)
                        {
                            if (bodyType.Contains(unPreferencePostfixs))
                            {
                                bodyType = bodyType.Replace(unPreferencePostfixs, "");
                                replacedBodyTypePostfix = unPreferencePostfixs;
                            }
                        }

                        foreach (string preferencePostfix in Props.preferencePostfixs)
                        {
                            if (bodyType.Contains(preferencePostfix))
                            {
                                return;
                            }
                        }

                        parent.pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetNamed(bodyType + Props.bodyTypePostfix);

                        foreach (HediffDef hediffDef in Props.compSetting.compPostTick.addHediffDefs)
                        {
                            parent.pawn.health.AddHediff(hediffDef);
                        }
                        parent.pawn.Drawer.renderer.SetAllGraphicsDirty();

                        //Log.Error($"CompPostTick");
                    }

                    ticks = Props.ticks;
                    checkOnce = Props.checkOnce;
                }
            }

        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.compSetting.compPostPostAdd.activeComp && CheckHuman())
            {
                Pawn pawn = parent.pawn;
                foreach (HediffDef hediffDef in Props.compSetting.compPostPostAdd.addHediffDefs)
                {
                    parent.pawn.health.AddHediff(hediffDef);
                }

                if (Props.bodyTypePostfix != "" && Props.races.Contains(parent.pawn.def))
                {
                    //Log.Error($"{parent.def.defName} : 2");
                    //foreach(var aa in Props.races)
                    //{
                    //    Log.Error(aa.defName);
                    //}
                    string bodyType = pawn.story.bodyType.defName;

                    bodyType = bodyType.Replace(Props.bodyTypePostfix, "");

                    foreach (string unPreferencePostfixs in Props.unPreferencePostfixs)
                    {
                        if (bodyType.Contains(unPreferencePostfixs))
                        {
                            bodyType = bodyType.Replace(unPreferencePostfixs, "");
                            replacedBodyTypePostfix = unPreferencePostfixs;
                        }
                    }

                    foreach (string preferencePostfix in Props.preferencePostfixs)
                    {
                        if (bodyType.Contains(preferencePostfix))
                        {
                            return;
                        }
                    }

                    pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetNamed(bodyType + Props.bodyTypePostfix);

                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                    //Log.Error($"CompPostPostAdd");
                }
            }
        }
        public override void CompPostPostRemoved()
        {
            if (Props.compSetting.compPostPostRemoved.activeComp && CheckHuman())
            {
                //Log.Error($"{parent.def.defName} : 3");
                Pawn pawn = parent.pawn;
                if (Props.bodyTypePostfix != "" && Props.races.Contains(parent.pawn.def))
                {
                    string bodyType = pawn.story.bodyType.defName;

                    if (bodyType.Contains(Props.bodyTypePostfix))
                    {
                        bodyType = bodyType.Replace(Props.bodyTypePostfix, "");
                    }
                    else
                    {
                        return;
                    }
                    pawn.story.bodyType = DefDatabase<BodyTypeDef>.GetNamed(bodyType.Replace(Props.bodyTypePostfix, "") + replacedBodyTypePostfix);
                    replacedBodyTypePostfix = "";

                    foreach (HediffDef hediffDef in Props.compSetting.compPostPostRemoved.addHediffDefs)
                    {
                        parent.pawn.health.AddHediff(hediffDef);
                    }
                    pawn.Drawer.renderer.SetAllGraphicsDirty();
                    //Log.Error($"CompPostPostRemoved");
                }
            }
        }
    }
}
