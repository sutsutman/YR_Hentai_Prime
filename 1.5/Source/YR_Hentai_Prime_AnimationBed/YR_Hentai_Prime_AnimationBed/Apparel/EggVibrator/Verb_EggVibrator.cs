using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // EggVibrator에 대한 Verb 정의 클래스
    public class Verb_EggVibrator : Verb
    {
        // 컴포넌트 소스를 가져옴
        public Comp_EggVibrator EggVibratorCompSource => DirectOwner as Comp_EggVibrator;

        // 장비 소스를 가져옴
        public new ThingWithComps EquipmentSource => EquipmentCompSource != null ? EquipmentCompSource.parent : (EggVibratorCompSource?.parent);

        // 효과 발동 시도
        protected override bool TryCastShot()
        {
            // 컴포넌트 및 속성 가져오기
            Comp_EggVibrator comp = EggVibratorCompSource;
            comp.cooldownTicks = comp.Props.cooldownTicks;

            CompProperties_EggVibrator props = comp.Props;

            // 효과 적용
            SwitchOn(comp.Wearer, 4f, 4f, props.hediffDefs, props.commandHediffDefs, props.filthDef, props.filthNum);

            return true;
        }

        // Pawn에게 효과 적용
        public static void SwitchOn(Pawn pawn, float time, float enemyTime, List<HediffDef> hediffs = null, List<HediffDef> subHediffs = null, ThingDef filthDef = null, int filthNum = 1)
        {
            bool hostile = false;

            // 적대 상태 여부 판단
            if ((pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) || pawn.InAggroMentalState)
            {
                hostile = true;
            }

            // 스턴 효과 적용
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

            // Hediff 적용
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

            // 추가 Hediff 적용
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

            // 시각적 흔들림 효과 적용
            Traverse.Create(pawn.Drawer).Field<JitterHandler>("jitterer").Value.AddOffset(0.07f, Rand.Range(0, 360));

            // 오염물 생성
            CreateFilth(pawn, filthDef, filthNum);
        }

        // Pawn의 위치에 오염물 생성
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
                        IntVec3 pos = pawnPosition + GenRadial.RadialPattern[i];
                        bool canPlaceFilthToPos = GenGrid.InBounds(pos, pawnMap) &&
                                                  GridsUtility.GetRoomOrAdjacent(pawnPosition, pawnMap, (RegionType)6) ==
                                                  GridsUtility.GetRoomOrAdjacent(pos, pawnMap, (RegionType)6);

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
