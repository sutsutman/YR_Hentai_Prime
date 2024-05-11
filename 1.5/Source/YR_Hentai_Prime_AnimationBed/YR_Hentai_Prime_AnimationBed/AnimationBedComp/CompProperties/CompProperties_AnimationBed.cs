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

        public CompProperties_AnimationBed() => compClass = typeof(CompAnimationBed);
    }
}
