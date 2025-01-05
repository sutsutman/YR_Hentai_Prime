using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_EggVibrator : CompProperties
    {
        public CompProperties_EggVibrator()
        {
            compClass = typeof(Comp_EggVibrator);
        }
        public List<HediffDef> hediffDefs = new List<HediffDef>();
        public List<HediffDef> commandHediffDefs = new List<HediffDef>();

        public int ticks = 600;

        public int rand = 50;

        public int cooldownTicks = 3000;

        public KeyBindingDef hotKey;

        public ThingDef filthDef;

        public int filthNum = 1;

        public string description;
    }

    public class Comp_EggVibrator : ThingComp, IVerbOwner
    {
        private int ticks = 0;
        public int cooldownTicks = 0;
        private VerbTracker verbTracker;

        public Pawn Wearer => WearerOf(this);
        public static Pawn WearerOf(Comp_EggVibrator comp)
        {
            return comp.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker ? pawn_ApparelTracker.pawn : null;
        }

        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        public List<Tool> Tools => parent.def.tools;
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;
        public Thing ConstantCaster => Wearer;
        public string UniqueVerbOwnerID()
        {
            return "YR_EggVibrator" + parent.ThingID;
        }

        public bool VerbsStillUsableBy(Pawn p)
        {
            return Wearer == p;
        }

        public VerbTracker VerbTracker
        {
            get
            {
                verbTracker ??= new VerbTracker(this);
                return verbTracker;
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
        }
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
        }
        public CompProperties_EggVibrator Props => (CompProperties_EggVibrator)props;
        public override void CompTick()
        {
            base.CompTick();
            if (cooldownTicks > 0)
            {
                cooldownTicks--;
            }
            ticks--;
            if (ticks <= 0)
            {
                ticks = Props.ticks;
                if (Wearer != null)
                {
                    System.Random rand = new System.Random();
                    int num = rand.Next(0, 101);
                    if (num <= Props.rand)
                    {
                        foreach (HediffDef hediffDef in Props.hediffDefs)
                        {
                            Wearer.health.AddHediff(hediffDef);
                        }
                    }
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {

            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }
            ThingWithComps gear = parent;
            foreach (Verb verb in VerbTracker.AllVerbs)
            {
                if (verb.verbProps.hasStandardCommand)
                {
                    if (verb is Verb_EggVibrator)
                    {
                        Comp_EggVibrator comp = ThingCompUtility.TryGetComp<Comp_EggVibrator>(parent);
                        yield return CreateVerbTargetCommand(gear, verb, comp);
                    }
                }
            }
            yield break;
        }

        private Command_EggVibrator CreateVerbTargetCommand(Thing gear, Verb verb, Comp_EggVibrator comp_EggVibrator)
        {
            Command_EggVibrator command_EggVibrator = new Command_EggVibrator(comp_EggVibrator)
            {
                defaultDesc = gear.def.description
            };
            if (!comp_EggVibrator.Props.description.NullOrEmpty())
            {
                command_EggVibrator.defaultDesc = comp_EggVibrator.Props.description.Translate(gear.def.label);
            }
            command_EggVibrator.hotKey = comp_EggVibrator.Props.hotKey;
            command_EggVibrator.defaultLabel = verb.verbProps.label;
            command_EggVibrator.verb = verb;
            if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
            {
                command_EggVibrator.icon = verb.verbProps.defaultProjectile.uiIcon;
                command_EggVibrator.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
                command_EggVibrator.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
                command_EggVibrator.overrideColor = new Color?(verb.verbProps.defaultProjectile.graphicData.color);
            }
            else
            {
                command_EggVibrator.icon = (verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon;
                command_EggVibrator.iconAngle = gear.def.uiIconAngle;
                command_EggVibrator.iconOffset = gear.def.uiIconOffset;
                command_EggVibrator.defaultIconColor = gear.DrawColor;
            }

            if (cooldownTicks > 0)
            {
                command_EggVibrator.Disable("YR_EggVibrator_Cooldown".Translate(cooldownTicks.TicksToSeconds()));
            }
            return command_EggVibrator;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0, false);
        }
    }
}
