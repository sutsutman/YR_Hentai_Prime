using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{

    public class HediffCompProperties_Tentalce : HediffCompProperties
    {
        public HediffCompProperties_Tentalce() => compClass = typeof(HediffComp_Tentalce);

        public int healPoint = 1;

        public int ticks = 100;
        public List<TraitDef> colonistTraitDefs = new List<TraitDef>();
        public List<TraitDef> nonColonistTraitDefs = new List<TraitDef>();
        public List<ThingDef> makeApparels = new List<ThingDef>();
    }


    public class HediffComp_Tentalce : HediffComp
    {
        private int ticksToHeal;
        private int ticks = 1;
        private float healPoint;
        public static Func<Hediff, bool> func;
        private bool noMoreColonist = false;
        private bool noMoreNonColonist = false;
        private bool pawnDied = false;

        private HediffCompProperties_Tentalce Props => (HediffCompProperties_Tentalce)props;
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            pawnDied = true;
        }
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            parent.pawn.workSettings.DisableAll();
            ResetTicksToHeal();
            MakeApparel(Props.makeApparels);
        }
        private void MakeApparel(List<ThingDef> makeApparels)
        {
            if (!makeApparels.NullOrEmpty())
            {
                foreach (ThingDef makeApparel in makeApparels.ToList())
                {
                    if (!makeApparel.IsApparel)
                    {
                        Log.ErrorOnce($"{makeApparel.defName} is not Apparel", 100);
                    }
                    else
                    {
                        foreach (Apparel pawnApparel in Pawn.apparel?.WornApparel?.ToList())
                        {
                            if (pawnApparel.def == makeApparel)
                            {
                                makeApparels.Remove(makeApparel);
                            }
                        }
                    }
                }

                foreach (ThingDef makeApparel in makeApparels)
                {
                    ThingWithComps app = null;
                    if (makeApparel.MadeFromStuff)
                    {
                        if (makeApparel.defaultStuff != null)
                        {
                            app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, makeApparel.defaultStuff);
                        }
                        else
                        {
                            foreach (StuffCategoryDef stuffCategory in makeApparel.stuffCategories)
                            {
                                if (stuffCategory == StuffCategoryDefOf.Fabric)
                                {
                                    app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Cloth);
                                    break;
                                }
                                else if (stuffCategory == StuffCategoryDefOf.Metallic)
                                {
                                    app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Steel);
                                    break;
                                }
                                else if (stuffCategory == StuffCategoryDefOf.Woody)
                                {
                                    app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.WoodLog);
                                    break;
                                }
                                else if (stuffCategory == StuffCategoryDefOf.Leathery)
                                {
                                    app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Leather_Plain);
                                    break;
                                }
                                else if (stuffCategory == StuffCategoryDefOf.Stony)
                                {
                                    app = (ThingWithComps)ThingMaker.MakeThing(makeApparel, ThingDefOf.Sandstone);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        app = (ThingWithComps)ThingMaker.MakeThing(makeApparel);
                    }
                    Pawn.apparel.Wear((Apparel)app);
                    Pawn.apparel.Lock((Apparel)app);
                }
            }
        }

        private void ResetTicksToHeal()
        {
            // Rand.Range(1, 3) *
            healPoint = Props.healPoint;
            ticksToHeal = Props.ticks;
        }
        public override void CompPostTick(ref float severityAdjustment)
        {
            if (pawnDied)
            {
                pawnDied = false;
                MakeApparel(Props.makeApparels);
            }
            ticksToHeal--;
            ticks--;
            if (ticksToHeal <= 0)
            {
                TryHealRandomPermanentWound();
                TryHealRandomHediffTendable();
                //List<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
                //for (int i = 0; i < hediffs.Count; i++)
                //{
                //    for (int k = 0; k < Props.healHediffDefs.Count; k++)
                //    {
                //        if (hediffs[i].def == Props.healHediffDefs[k])
                //        {
                //            MinchoDefOf.HealHediff(parent.pawn, hediffs[i].def);
                //        }
                //    }
                //}
                ResetTicksToHeal();
            }
            if (ticks <= 0)
            {
                ticks = 500;
                if (Pawn.IsColonist)
                {
                    if (noMoreNonColonist)
                    {
                        noMoreColonist = false;
                        noMoreNonColonist = false;
                    }

                    if (!noMoreColonist)
                    {
                        foreach (TraitDef traitDef in Props.nonColonistTraitDefs)
                        {
                            foreach (Trait trait in Pawn.story.traits.allTraits.ToArray())
                            {
                                if (traitDef == trait.def)
                                {
                                    Pawn.story.traits.allTraits.Remove(trait);
                                }
                            }
                        }

                        foreach (TraitDef traitDef in Props.colonistTraitDefs)
                        {
                            if (!Pawn.story.traits.HasTrait(traitDef))
                            {
                                Pawn.story.traits.GainTrait(new Trait(traitDef));
                            }
                        }
                        noMoreColonist = true;
                    }
                }
                else
                {
                    if (noMoreColonist)
                    {
                        noMoreNonColonist = false;
                        noMoreColonist = false;
                    }
                    if (!noMoreNonColonist)
                    {
                        foreach (TraitDef traitDef in Props.colonistTraitDefs)
                        {
                            foreach (Trait trait in Pawn.story.traits.allTraits.ToArray())
                            {
                                if (traitDef == trait.def)
                                {
                                    Pawn.story.traits.allTraits.Remove(trait);
                                }
                            }
                        }

                        foreach (TraitDef traitDef in Props.nonColonistTraitDefs)
                        {
                            if (!Pawn.story.traits.HasTrait(traitDef))
                            {
                                Trait trait = new Trait(traitDef);
                                Pawn.story.traits.GainTrait(trait);

                                List<WorkTypeDef> disabledWorkTypes = trait.GetDisabledWorkTypes();
                                foreach (WorkTypeDef disabledWorkType in disabledWorkTypes)
                                {
                                    Traverse.Create(Pawn).Field<List<WorkTypeDef>>("cachedDisabledWorkTypes").Value.Add(disabledWorkType);
                                }

                                foreach (WorkTypeDef disabledWorkType in trait.def.disabledWorkTypes)
                                {
                                    Traverse.Create(Pawn).Field<List<WorkTypeDef>>("cachedDisabledWorkTypes").Value.Add(disabledWorkType);
                                }
                            }
                        }
                        noMoreNonColonist = true;
                    }
                }
            }
        }

        private void TryHealRandomPermanentWound()
        {
            IEnumerable<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in hediffs)
            {
                if (hediff.def.isBad && hediff.IsPermanent())
                {
                    hediff.Severity -= healPoint;
                }
            }
        }
        private void TryHealRandomHediffTendable()
        {
            IEnumerable<Hediff> hediffs = Pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in hediffs)
            {
                if (hediff.def.defName != "Scaria" && hediff.def.isBad && (hediff.IsPermanent() || hediff.def.chronic || hediff.def.tendable || hediff.def.makesSickThought))
                {
                    hediff.Heal(healPoint);
                    if (hediff.Severity <= 0.003)
                    {
                        HealthUtility.Cure(hediff);
                        return;
                    }

                }

            }
        }
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref noMoreColonist, "noMoreColonist");
            Scribe_Values.Look(ref noMoreNonColonist, "noMoreNonColonist");
            Scribe_Values.Look(ref ticksToHeal, "ticksToHeal");
            Scribe_Values.Look(ref healPoint, "healPoint");
            Scribe_Values.Look(ref pawnDied, "pawnDied");
        }
    }
}
