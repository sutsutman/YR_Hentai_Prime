using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompBaseOfAnimationBed : ThingComp
    {
        public Building_AnimationBed Building_AnimationBed => (Building_AnimationBed)parent;

        public Pawn HeldPawn => Building_AnimationBed.HeldPawn;

        public virtual void Notify_HeldOnPlatform(ThingOwner newOwner)
        {
        }

        public virtual void Notify_ReleasedFromPlatform()
        {
        }
    }
}
