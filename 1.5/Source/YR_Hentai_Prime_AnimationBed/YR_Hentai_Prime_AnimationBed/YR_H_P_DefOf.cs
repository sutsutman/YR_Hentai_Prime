using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [DefOf]
    public static class YR_H_P_DefOf
    {
        public static JobDef YR_ExecuteVictim;
        public static JobDef YR_ReleaseVictim;
        public static JobDef YR_CarryToAnimationBed;
        public static JobDef YR_TendVictim;
        public static JobDef YR_TransferBetweenAnimationBeds;
        //public static JobDef YR_FeedVictim;
        public static BedAnimationDef YR_Dummy_BedAnimation;
        public static AnimationDef YR_Global_Animation_NoMove;
        public static ThingDef YR_Maggot_Queen;
        public static JobDef YR_RapeDownedPawn;
        public static ThingDef YR_Maggot_Larva_CorpseMeat;

        public static SoundDef HiveSpawnSound => DefDatabase<SoundDef>.GetNamed("Hive_Spawn");

        public static JobDef YR_SelfTieToAnimationBed;
        internal static TraitDef YR_Disciplined;
        internal static Hediff YR_Pregnant_Tentacle_Birth;

        static YR_H_P_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(YR_H_P_DefOf));
    }
}