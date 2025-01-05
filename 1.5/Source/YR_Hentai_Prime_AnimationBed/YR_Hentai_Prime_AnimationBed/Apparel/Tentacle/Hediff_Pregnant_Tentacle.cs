using RimWorld;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Hediff_Pregnant_Tentacle : Hediff_Pregnant
    {
        private int ticks = 0;
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            ticks = 2500;
        }

        private bool IsSeverelyWounded
        {
            get
            {
                float num = 0f;
                List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    if (hediffs[i] is Hediff_Injury && !hediffs[i].IsPermanent())
                    {
                        num += hediffs[i].Severity;
                    }
                }
                List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
                for (int j = 0; j < missingPartsCommonAncestors.Count; j++)
                {
                    if (missingPartsCommonAncestors[j].IsFreshNonSolidExtremity)
                    {
                        num += missingPartsCommonAncestors[j].Part.def.GetMaxHealth(pawn);
                    }
                }
                return num > 38f * pawn.RaceProps.baseHealthScale;
            }
        }

        public new PregnancyAttitude? Attitude
        {
            get
            {
                if (comps == null || !pawn.RaceProps.Humanlike)
                {
                    return null;
                }
                for (int i = 0; i < comps.Count; i++)
                {
                    if (comps[i] is HediffComp_PregnantHuman hediffComp_PregnantHuman)
                    {
                        return hediffComp_PregnantHuman.Attitude;
                    }
                }
                return null;
            }
        }

        public override void Tick()
        {
            ageTicks++;
            if (CurStageIndex != lastStage)
            {
                NotifyPlayerOfTrimesterPassing();
                lastStage = CurStageIndex;
            }

            ticks--;

            if (ticks <= 0)
            {
                Severity += 0.00277778f;
                ticks = 2500;
            }

            if (Severity >= 1f)
            {
                pawn.health.AddHediff(YR_H_P_DefOf.YR_Pregnant_Tentacle_Birth);

                pawn.health.RemoveHediff(this);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastStage, "lastStage", 0, false);
            Scribe_Values.Look(ref ticks, "ticks", 0, false);
        }

        private void NotifyPlayerOfTrimesterPassing()
        {
            if (pawn.RaceProps.Humanlike && PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                Messages.Message(((lastStage == 0) ? "MessageColonistReaching2ndTrimesterPregnancy" : "MessageColonistReaching3rdTrimesterPregnancy").Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.PositiveEvent, true);
                if (lastStage == 1 && !Find.History.everThirdTrimesterPregnancy)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelThirdTrimester".Translate(pawn), "LetterTextThirdTrimester".Translate(pawn), LetterDefOf.PositiveEvent, pawn, null, null, null, null);
                    Find.History.everThirdTrimesterPregnancy = true;
                }
            }
        }

        public override void PostDebugAdd()
        {
            if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike)
            {
                SetParents(pawn, null, PregnancyUtility.GetInheritedGeneSet(null, pawn));
            }
        }

        public override string DebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.DebugString());
            stringBuilder.AppendLine("Gestation progress: " + GestationProgress.ToStringPercent());
            stringBuilder.AppendLine("Time left: " + ((int)((1f - GestationProgress) * pawn.RaceProps.gestationPeriodDays * 60000f)).ToStringTicksToPeriod(true, false, true, true, false));
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (DebugSettings.ShowDevGizmos)
            {
                if (CurStageIndex < 2)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Next trimester",
                        action = delegate ()
                        {
                            HediffStage hediffStage = def.stages[CurStageIndex + 1];
                            severityInt = hediffStage.minSeverity;
                        }
                    };
                }
                if (ModsConfig.BiotechActive && pawn.RaceProps.Humanlike && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PregnancyLabor, false) == null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Start Labor",
                        action = delegate ()
                        {
                            StartLabor();
                            pawn.health.RemoveHediff(this);
                        }
                    };
                }
            }
            yield break;
        }
        public new float GestationProgress
        {
            get => Severity;
            private set => Severity = value;
        }


        // Token: 0x0400119B RID: 4507
        private int lastStage;

        // Token: 0x0400119C RID: 4508
        private const int MiscarryCheckInterval = 1000;

        // Token: 0x0400119D RID: 4509
        private const float MTBMiscarryStarvingDays = 2f;

        // Token: 0x0400119E RID: 4510
        private const float MTBMiscarryWoundedDays = 2f;

        // Token: 0x0400119F RID: 4511
        private const float MalnutritionMinSeverityForMiscarry = 0.1f;
    }
}
