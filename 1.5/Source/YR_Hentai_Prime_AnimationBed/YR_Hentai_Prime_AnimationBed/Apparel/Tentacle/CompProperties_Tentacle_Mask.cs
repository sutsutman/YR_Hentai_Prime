using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Tentacle_Mask : CompProperties
    {
        public CompProperties_Tentacle_Mask() => compClass = typeof(Comp_Tentacle_Mask);

        public HediffDef hediffDef;
        public HediffDef unequippedHediffDef;
        public ThingDef moteDef;
        public ThingDef longMoteDef;
        public int moteRepeat = 1;
        public int ticks = 20;
        public Vector3 offSet = new Vector3();
        public Vector3 eastOffSet = new Vector3();
        public bool isEye = false;
        public bool moveOnly = false;
        public List<Tentacle_Mask_RaceSetting> raceSettings = new List<Tentacle_Mask_RaceSetting>();
    }
    public class Tentacle_Mask_RaceSetting
    {
        public List<ThingDef> races = new List<ThingDef>();
        public Vector3 offSet = new Vector3();
        public Vector3 eastOffSet = new Vector3();
    }

    public class Comp_Tentacle_Mask : ThingComp
    {
        private int ticks = 0;
        private int checkEyeTicks = 0;
        private Vector3 tempDrawPos;
        private bool hasRightEye = false;
        private bool hasLeftEye = false;
        private Pawn pawn = null;
        private bool reset = true;
        private Vector3 compOffset = new Vector3();
        private Vector3 compEastOffset = new Vector3();
        private CompProperties_Tentacle_Mask Props => (CompProperties_Tentacle_Mask)props;

        public override void CompTick()
        {
            base.CompTick();
            ticks--;
            checkEyeTicks--;

            if (ticks <= 0)
            {
                ticks = Props.ticks;

                if (parent is Apparel ap)
                {
                    if (ap.Wearer != null)
                    {
                        pawn = ap.Wearer;

                        if (reset)
                        {
                            compOffset = Props.offSet;
                            compEastOffset = Props.eastOffSet;

                            foreach (Tentacle_Mask_RaceSetting raceSetting in Props.raceSettings)
                            {
                                foreach (ThingDef race in raceSetting.races)
                                {
                                    if (race == pawn.def)
                                    {
                                        compOffset = raceSetting.offSet;
                                        compEastOffset = raceSetting.eastOffSet;
                                    }
                                }
                            }

                            reset = false;
                        }


                        if (Props.moveOnly)
                        {
                            if (pawn.DrawPos != tempDrawPos)
                            {
                                MakeMote(pawn);
                            }
                        }
                        else
                        {
                            MakeMote(pawn);
                        }
                        tempDrawPos = pawn.DrawPos;
                    }
                }
            }

            if (checkEyeTicks <= 0 && pawn != null)
            {
                checkEyeTicks = 100;

                hasRightEye = false;
                hasLeftEye = false;

                if (pawn.health.hediffSet.HasHead)
                {
                    foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts())
                    {
                        if (part.untranslatedCustomLabel != null)
                        {
                            string untranslatedCustomLabel = part.untranslatedCustomLabel;

                            if (untranslatedCustomLabel.ToUpper().Contains("RIGHT") && untranslatedCustomLabel.ToUpper().Contains("EYE"))
                            {
                                hasRightEye = true;
                            }

                            else if (untranslatedCustomLabel.ToUpper().Contains("LEFT") && untranslatedCustomLabel.ToUpper().Contains("EYE"))
                            {
                                hasLeftEye = true;
                            }
                        }
                    }
                }
                //Log.Error("--------");
                if (!hasLeftEye && !hasLeftEye)
                {
                    if (!PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn))
                    {
                        //Log.Error("! IsBiologicallyOrArtificiallyBlind");
                        hasRightEye = true;
                        hasLeftEye = true;
                    }
                }
                //Log.Error("hasRightEye : " + hasRightEye);
                //Log.Error("hasLeftEye : " + hasLeftEye);
            }
        }

        private void MakeMote(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null)
            {
                return;
            }
            ThingDef moteDef = Props.moteDef;
            if (Props.longMoteDef != null)
            {

                TickManager tickManager = Find.TickManager;
                if (tickManager.CurTimeSpeed == TimeSpeed.Superfast || tickManager.CurTimeSpeed == TimeSpeed.Ultrafast)
                {
                    moteDef = Props.longMoteDef;
                }
            }

            for (int i = 0; i < Props.moteRepeat; i++)
            {
                if (Props.isEye && (pawn.Rotation == Rot4.South || pawn.Rotation == Rot4.North))
                {
                    Vector3 offSet = compOffset;
                    //좌측 눈
                    if (hasLeftEye)
                    {
                        if (pawn.Rotation == Rot4.South)
                        {
                            offSet = compOffset;
                        }
                        else if (pawn.Rotation == Rot4.North)
                        {
                            offSet.x *= -1;
                        }
                        MoteMaker.MakeAttachedOverlay(pawn, moteDef, offSet, 1f, -1f);
                    }

                    offSet = compOffset;
                    //우측 눈
                    if (hasRightEye)
                    {
                        if (pawn.Rotation == Rot4.South)
                        {
                            offSet.x *= -1;
                        }
                        else if (pawn.Rotation == Rot4.North)
                        {
                            offSet = compOffset;
                        }
                        MoteMaker.MakeAttachedOverlay(pawn, moteDef, offSet, 1f, -1f);
                    }

                }
                else
                {
                    Vector3 offSet = compOffset;
                    if (Props.isEye)
                    {
                        if ((pawn.Rotation == Rot4.East || pawn.Rotation == Rot4.West) && (hasRightEye || hasLeftEye))
                        {
                            if (pawn.Rotation == Rot4.East)
                            {
                                offSet = compEastOffset;
                            }
                            else if (pawn.Rotation == Rot4.West)
                            {
                                offSet = compEastOffset;
                                offSet.x *= -1;
                            }
                            MoteMaker.MakeAttachedOverlay(pawn, moteDef, offSet, 1f, -1f);
                        }
                    }
                    else
                    {
                        MoteMaker.MakeAttachedOverlay(pawn, moteDef, offSet, 1f, -1f);
                    }
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            if (Props.hediffDef != null)
            {
                base.Notify_Equipped(pawn);
                Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, pawn);
                pawn.health.AddHediff(hediff);
            }

            for (int i = 0; i < Props.moteRepeat; i++)
            {
                MoteMaker.MakeAttachedOverlay(pawn, Props.moteDef, Vector3.zero, 1f, -1f);
            }
        }
        public override void Notify_Unequipped(Pawn pawn)
        {
            if (Props.hediffDef != null)
            {
                base.Notify_Unequipped(pawn);
                foreach (Hediff removeHediff in pawn.health.hediffSet.hediffs.ToList())
                {
                    if (removeHediff.def == Props.hediffDef)
                    {
                        pawn.health.RemoveHediff(removeHediff);
                    }
                }


            }
            if (Props.unequippedHediffDef != null)
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.unequippedHediffDef, pawn);
                pawn.health.AddHediff(hediff);
            }
        }
    }
}
