﻿using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace YR_Hentai_Prime_AnimationBed
{
    public class YR_StudyUtility
    {
        private static readonly HashSet<Pawn> tmpReservers = new HashSet<Pawn>();

        public static void TargetHoldingPlatformForVictim(Pawn carrier, Thing victim, bool transferBetweenPlatforms = false, Thing sourcePlatform = null)
        {
            //1
            Find.Targeter.BeginTargeting(TargetingParameters.ForBuilding(), delegate (LocalTargetInfo t)
            {
                if (carrier != null && !CanReserveForTransfer(t))
                {
                    Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
                }
                else
                {
                    foreach (Thing item in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_AnimationBed>())
                    {
                        CompAnimationBedTarget compAnimationBedTarget;
                        if (item is Building_AnimationBed building_AnimationBed && victim != building_AnimationBed.HeldPawn && (compAnimationBedTarget = building_AnimationBed.HeldPawn?.TryGetComp<CompAnimationBedTarget>()) != null && compAnimationBedTarget.targetHolder == t.Thing)
                        {
                            Messages.Message("MessageHolderReserved".Translate(t.Thing.Label), MessageTypeDefOf.RejectInput);
                            return;
                        }
                    }

                    CompAnimationBedTarget compHoldingPlatformTarget2 = victim.TryGetComp<CompAnimationBedTarget>();
                    if (compHoldingPlatformTarget2 != null)
                    {
                        compHoldingPlatformTarget2.targetHolder = t.Thing;
                    }

                    if (carrier != null && carrier == victim)
                    {
                        // Self-action logic
                        Job job = JobMaker.MakeJob(YR_H_P_DefOf.YR_SelfTieToAnimationBed, t, victim);
                        job.count = 1;
                        carrier.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }
                    else if (carrier != null)
                    {
                        // 기존 로직
                        Job job = (transferBetweenPlatforms
                            ? JobMaker.MakeJob(YR_H_P_DefOf.YR_TransferBetweenAnimationBeds, sourcePlatform, t, victim)
                            : JobMaker.MakeJob(YR_H_P_DefOf.YR_CarryToAnimationBed, t, victim));
                        job.count = 1;
                        carrier.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }

                }
            },
            //2
            delegate (LocalTargetInfo t)
            {
                if (ValidateTarget(t))
                {
                    GenDraw.DrawTargetHighlight(t);
                }
            },
            //3
            ValidateTarget, null, null, BaseContent.ClearTex, playSoundOnAction: true, delegate (LocalTargetInfo t)
            {
                CompAnimationBed compAnimationBed = t.Thing?.TryGetComp<CompAnimationBed>();
                if (compAnimationBed == null)
                {
                    TaggedString label = "ChooseEntityHolder".Translate().CapitalizeFirst() + "...";
                    Widgets.MouseAttachedLabel(label);
                }
                else
                {
                    Pawn pawn = null;
                    if (carrier != null)
                    {
                        pawn = t.Thing.Map.reservationManager.FirstRespectedReserver(t.Thing, carrier);
                    }
                    else if (t.Thing is Building_AnimationBed p && AlreadyReserved(p, out Pawn reserver))
                    {
                        pawn = reserver;
                    }

                    TaggedString label;
                    if (pawn != null)
                    {
                        label = string.Format("{0}: {1}", "EntityHolderReservedBy".Translate(), pawn.LabelShortCap);
                    }
                    else
                    {
                        if (!Condition.Match((Pawn)victim, (Building_AnimationBed)t.Thing, t.Thing.TryGetComp<CompAnimationBed>()?.Props.pawnCondition?.heldPawnCondition, out _))
                        {
                            label = "YR_TieUpPawn_Condition".Translate();
                        }
                        else
                        {
                            label = "YR_TieUpPawn".Translate();
                        }
                        //label = "FloatMenuContainmentStrength".Translate() + ": " + StatDefOf.ContainmentStrength.Worker.ValueToString(compAnimationBed.ContainmentStrength, finalized: false);
                        //label += "\n" + ("FloatMenuContainmentRequires".Translate(victim).CapitalizeFirst() + ": " + StatDefOf.MinimumContainmentStrength.Worker.ValueToString(victim.GetStatValue(StatDefOf.MinimumContainmentStrength), finalized: false)).Colorize(t.Thing.SafelyContains(victim) ? Color.white : Color.red);
                    }

                    Widgets.MouseAttachedLabel(label);
                }
            },
            //4
            delegate
            {
                foreach (Building item2 in victim.MapHeld.listerBuildings.AllBuildingsColonistOfClass<Building_AnimationBed>())
                {
                    if (ValidateTarget(item2) && (carrier == null || CanReserveForTransfer(item2)))
                    {
                        GenDraw.DrawArrowPointingAt(item2.DrawPos);
                    }
                }
            });
            //5
            bool CanReserveForTransfer(LocalTargetInfo t)
            {
                if (transferBetweenPlatforms)
                {
                    if (t.HasThing)
                    {
                        return carrier.CanReserve(t.Thing);
                    }

                    return false;
                }

                return true;
            }
            //6
            bool ValidateTarget(LocalTargetInfo t)
            {
                if (t.HasThing && t.Thing.TryGetComp(out CompAnimationBed comp) && comp.HeldPawn == null)
                {
                    if (!Condition.Match((Pawn)victim, (Building_AnimationBed)t.Thing, comp.Props.pawnCondition?.heldPawnCondition, out _))
                    {
                        return false;
                    }

                    if (carrier != null)
                    {
                        return carrier.CanReserveAndReach(t.Thing, PathEndMode.Touch, Danger.Some);
                    }

                    return true;
                }

                return false;
            }
        }

        public static bool AlreadyReserved(Thing p, out Pawn reserver)
        {
            tmpReservers.Clear();
            p.Map.reservationManager.ReserversOf(p, tmpReservers);
            reserver = tmpReservers.FirstOrDefault();
            if (reserver != null)
            {
                return true;
            }

            foreach (Thing item in p.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
            {
                if (item.TryGetComp<CompAnimationBedTarget>().targetHolder == p)
                {
                    reserver = item as Pawn;
                    return true;
                }
            }

            return false;
        }
        public static bool HoldingPlatformAvailableOnCurrentMap()
        {
            Map currentMap = Find.CurrentMap;
            if (currentMap == null)
            {
                return false;
            }

            foreach (Building item in currentMap.listerBuildings.AllBuildingsColonistOfClass<Building_AnimationBed>())
            {
                if (item.TryGetComp<CompAnimationBed>(out var comp) && comp.Available && !AlreadyReserved(item, out var _))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
