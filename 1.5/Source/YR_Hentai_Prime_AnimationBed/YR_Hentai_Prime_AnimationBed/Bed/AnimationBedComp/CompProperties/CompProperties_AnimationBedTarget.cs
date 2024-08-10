using Verse;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_AnimationBedTarget : CompProperties
    {
        public PawnKindDef heldPawnKind;

        [MustTranslate]
        public string capturedLetterLabel;

        [MustTranslate]
        public string capturedLetterText;

        public float baseEscapeIntervalMtbDays = 60f;

        public bool lookForTargetOnEscape = true;

        public bool canBeExecuted = true;

        public bool getsColdContainmentBonus;

        public bool hasAnimation = true;

        public CompProperties_AnimationBedTarget() => compClass = typeof(CompAnimationBedTarget);
    }
}
