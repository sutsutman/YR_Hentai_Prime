using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Verb_EggVibrator : Verb
    {
        public Comp_EggVibrator EggVibratorCompSource => DirectOwner as Comp_EggVibrator;
        public new ThingWithComps EquipmentSource => EquipmentCompSource != null ? EquipmentCompSource.parent : (EggVibratorCompSource?.parent);

        protected override bool TryCastShot()
        {
            Comp_EggVibrator comp = EggVibratorCompSource;
            comp.cooldownTicks = comp.Props.cooldownTicks;
            CompProperties_EggVibrator props = comp.Props;
            SwitchOn(comp.Wearer, 4f, 4f, props.hediffDefs, props.commandHediffDefs, props.filthDef, props.filthNum);
            return true;
        }

        public static void SwitchOn(Pawn pawn, float time, float enemyTime, List<HediffDef> hediffs = null, List<HediffDef> subHediffs = null, ThingDef filthDef = null, int filthNum = 1)
        {
            bool hostile = false;
            if ((pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) || pawn.InAggroMentalState)
            {
                hostile = true;
            }

            DamageInfo dinfo = new DamageInfo
            {
                Def = DamageDefOf.Stun
            };
            dinfo.SetAmount(time.SecondsToTicks() / 30f);
            if (hostile)
            {
                dinfo.SetAmount(enemyTime.SecondsToTicks() / 30f);
            }
            pawn.TakeDamage(dinfo);
            if (!hediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in hediffs)
                {
                    Hediff hediff = pawn.health.AddHediff(hediffDef);
                    if (hostile)
                    {
                        hediff.Severity = enemyTime / time;
                    }
                }
            }
            if (!subHediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in subHediffs)
                {
                    Hediff hediff = pawn.health.AddHediff(hediffDef);
                    if (hostile)
                    {
                        hediff.Severity = enemyTime / time;
                    }
                }
            }
            Traverse.Create(pawn.Drawer).Field<JitterHandler>("jitterer").Value.AddOffset(0.07f, Rand.Range(0, 360));

            CreateFilth(pawn, filthDef, filthNum);
        }

        public static void CreateFilth(Pawn pawn, ThingDef filthDef, int filthNum)
        {
            if (filthDef != null)
            {
                for (int i = 0; i < filthNum; i++)
                {
                    IntVec3 pawnPosition = pawn.Position;
                    Map pawnMap = pawn.Map;

                    if (pawnMap != null && pawnPosition != null)
                    {
                        IntVec3 Pos = pawnPosition + GenRadial.RadialPattern[i];
                        bool canPlaceFilthToPos = GenGrid.InBounds(Pos, pawnMap) && GridsUtility.GetRoomOrAdjacent(pawnPosition, pawnMap, (RegionType)6) == GridsUtility.GetRoomOrAdjacent(Pos, pawnMap, (RegionType)6);
                        if (canPlaceFilthToPos)
                        {
                            FilthMaker.TryMakeFilth(pawnPosition, pawnMap, filthDef, GenText.LabelIndefinite(pawn), Rand.RangeInclusive(0, 4));
                        }
                    }
                }
            }
        }
    }
}
