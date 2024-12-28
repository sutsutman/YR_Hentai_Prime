using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_AnimationBed : CompProperties
    {
        public float containmentFactor = 1f;
        //[NoTranslate]
        //public string untetheredGraphicTexPath;

        //[NoTranslate]
        //public string tilingChainTexPath;

        //[NoTranslate]
        //public string baseChainFastenerTexPath;

        //[NoTranslate]
        //public string targetChainFastenerTexPath;

        //public SoundDef entityLungeSoundHi;

        //public SoundDef entityLungeSoundLow;

        public List<HediffDef> addedHediffDefs = new List<HediffDef>();

        public List<HediffDef> addedAfterEjectHediffDefs = new List<HediffDef>();

        public PawnCondition pawnCondition;
        public CompProperties_AnimationBed() => compClass = typeof(CompAnimationBed);
    }

        public class CompAnimationBed : ThingComp
        {
            public float ContainmentStrength => parent.GetStatValue(StatDefOf.ContainmentStrength);

            public CompProperties_AnimationBed Props => (CompProperties_AnimationBed)props;

            protected Building_AnimationBed AnimationBed => (Building_AnimationBed)parent;

            public bool Available
            {
                get
                {
                    return !AnimationBed.Occupied;
                }
            }

            public Pawn HeldPawn
            {
                get
                {
                    return AnimationBed.HeldPawn;
                }
            }

            public ThingOwner Container
            {
                get
                {
                    return AnimationBed.innerContainer;
                }
            }

            public void EjectContents()
            {
                AnimationBed.EjectContents();
            }

            //public override string CompInspectStringExtra()
            //{
            //    string text = base.CompInspectStringExtra();
            //    if (!text.NullOrEmpty())
            //    {
            //        text += "\n";
            //    }

            //    float statValue = parent.GetStatValue(StatDefOf.ContainmentStrength);
            //    text += $"{StatDefOf.ContainmentStrength.LabelCap}: {statValue:F0}";
            //    if (!parent.Spawned)
            //    {
            //        return text;
            //    }

            //    if (parent.IsOutside())
            //    {
            //        text += string.Format(" ({0})", "Outdoors".Translate());
            //    }
            //    else if (StatWorker_ContainmentStrength.AnyDoorForcedOpen(parent.GetRoom()))
            //    {
            //        text += string.Format(" ({0})", "Stat_ContainmentStrength_DoorForcedOpen".Translate());
            //    }

            //    return text;
            //}
        }
    }


