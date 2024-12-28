using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;


namespace YR_Hentai_Prime_AnimationBed
{
    public class Alert_PawnDownButMaggotCantTouch : Alert
    {
        public Alert_PawnDownButMaggotCantTouch()
        {
            defaultLabel = "YR_Alert_PawnDownButMaggotCantTouch_Label".Translate();
            defaultPriority = AlertPriority.Medium;
        }

        private List<Pawn> DownPawns
        {
            get
            {
                DownPawnsResult.Clear();

                bool queenExists = PawnsFinder.AllMaps_Spawned.Any(pawn => pawn.def == YR_H_P_DefOf.YR_Maggot_Queen);

                if (queenExists)
                {
                    var downedPawns = PawnsFinder.AllMaps_Spawned.Where(pawn => pawn.Downed
                                       && (pawn.Position.GetEdifice(pawn.Map) != null || GenAdj.CellsAdjacent8Way(pawn).Any(intVec => intVec.GetEdifice(pawn.Map) is Building_Bed))
                                       && pawn.GetPosture() != PawnPosture.LayingInBed
                                       && (pawn.IsColonist || pawn.IsPrisoner || pawn.IsSlaveOfColony));
                    DownPawnsResult.AddRange(downedPawns);
                    //DownPawnsResult.AddRange(downedPawns.Where(pawn =>
                    //    {
                    //        bool becomPrisoner = (pawn.IsPrisoner || pawn.Faction.HostileTo(Faction.OfPlayer));
                    //        return ((becomPrisoner && (pawn.GetRoom() == null || !pawn.GetRoom().IsPrisonCell))
                    //                || (!becomPrisoner && pawn.GetRoom().IsPrisonCell));
                    //    }));
                }

                return DownPawnsResult;
            }
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Pawn pawn in DownPawns)
            {
                stringBuilder.AppendLine("  - " + pawn.NameShortColored.Resolve());
            }
            return "YR_Alert_PawnDownButMaggotCantTouch_Desc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(DownPawns);
        }
        private List<Pawn> DownPawnsResult = new List<Pawn>();
    }
}