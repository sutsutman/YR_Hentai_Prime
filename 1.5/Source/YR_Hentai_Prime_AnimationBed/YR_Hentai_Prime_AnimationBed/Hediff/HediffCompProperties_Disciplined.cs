using AlienRace;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_Disciplined : HediffCompProperties
    {
        public HediffCompProperties_Disciplined() => compClass = typeof(HediffComp_Disciplined);
        public int ticks = 600;
        public List<GainTrait> gainTraits;
        public float severity = 0.01f;
        public float addTraitSeverity = 0.99f;
        public bool removeHediffPostAddTrait;
        public List<ThingDef> apparelDefs = new List<ThingDef>();
        public float removeUnrecruitableSeverity = 100f;
        public bool removeHediffPostRemoveUnrecruitable;
    }
    public class GainTrait
    {
        public TraitDef def;

        public int degree;
    }
    public class HediffComp_Disciplined : HediffComp
    {
        private int ticks = 600;

        private HediffCompProperties_Disciplined Props => (HediffCompProperties_Disciplined)props;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref ticks, "ticks");

        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ticks--;
            //테스트용

            if (ticks <= 0)
            {
                ticks = Props.ticks;

                if (!Props.apparelDefs.NullOrEmpty())
                {
                    if (CheckMatchAny(Props.apparelDefs, Pawn.apparel.WornApparel))
                    {
                        parent.Severity += Props.severity;
                    }
                    else
                    {
                        parent.Severity -= Props.severity;
                    }
                }

                if (parent.Severity >= Props.addTraitSeverity)
                {
                    foreach (var gainTrait in Props.gainTraits)
                    {
                        bool hasTrait = false;
                        foreach (var pawnTrait in Pawn.story.traits.allTraits)
                        {
                            if (pawnTrait.def == gainTrait.def && pawnTrait.Degree == gainTrait.degree)
                            {
                                hasTrait = true;
                                break;
                            }
                            if (Pawn.def is ThingDef_AlienRace alienThingDef)
                            {
                                if (!RaceRestrictionSettings.CanGetTrait(gainTrait.def, Pawn.def, gainTrait.degree))
                                {
                                    hasTrait = true;
                                    break;
                                }

                                //foreach (var disallowedTrait in alienThingDef.alienRace.generalSettings.disallowedTraits)
                                //{
                                //    if (disallowedTrait.defName.def == gainTrait.def && disallowedTrait.defName.degree == gainTrait.degree)
                                //    {
                                //        hasTrait = true;
                                //        break;
                                //    }
                                //}
                            }
                        }
                        if (!hasTrait)
                        {
                            Pawn.story.traits.GainTrait(new Trait(gainTrait.def, gainTrait.degree));
                            Messages.Message("YR_GainTrait".Translate(Pawn.LabelShort, parent.def, gainTrait.def.degreeDatas[gainTrait.degree].LabelCap), MessageTypeDefOf.PositiveEvent, false);
                        }
                    }
                    if (Props.removeHediffPostAddTrait)
                    {
                        HealthUtility.Cure(parent);
                    }
                }
                if (parent.Severity >= Props.removeUnrecruitableSeverity)
                {
                    if (!Pawn.guest.Recruitable)
                    {
                        Pawn.guest.Recruitable = true;
                        Messages.Message("YR_RemoveUnrecruitable".Translate(Pawn.LabelShort, parent.def, "Unrecruitable".Translate()), MessageTypeDefOf.PositiveEvent, false);
                    }
                    if (Props.removeHediffPostRemoveUnrecruitable)
                    {
                        HealthUtility.Cure(parent);
                    }
                }
            }
        }

        public static bool CheckMatchAny<T>(List<ThingDef> listA, List<T> listB) where T : ThingWithComps
        {
            if (listA.NullOrEmpty() || listB.NullOrEmpty())
            {
                return false;
            }

            listA = listA.OrderBy(x => x.defName).ToList();
            listB = listB.OrderBy(x => x.def.defName).ToList();

            foreach (ThingDef def in listA)
            {
                foreach (T item in listB)
                {
                    if (def == item.def)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
