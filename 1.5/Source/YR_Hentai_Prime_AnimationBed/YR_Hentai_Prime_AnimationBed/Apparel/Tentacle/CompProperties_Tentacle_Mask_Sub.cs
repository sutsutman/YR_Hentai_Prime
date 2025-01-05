namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Tentacle_Mask_Sub : CompProperties_Tentacle_Mask
    {
        public CompProperties_Tentacle_Mask_Sub() => compClass = typeof(Comp_Tentacle_Mask_Sub);
    }

    public class Comp_Tentacle_Mask_Sub : Comp_Tentacle_Mask
    {
        private CompProperties_Tentacle_Mask_Sub Props => (CompProperties_Tentacle_Mask_Sub)props;
    }
}
