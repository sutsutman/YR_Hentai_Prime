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
        public static AnimationDef YR_Dummy_Animation;

        static YR_H_P_DefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(YR_H_P_DefOf));
    }
}