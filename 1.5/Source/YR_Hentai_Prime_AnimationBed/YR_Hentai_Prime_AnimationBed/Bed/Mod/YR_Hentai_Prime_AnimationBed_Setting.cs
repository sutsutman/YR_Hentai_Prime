using UnityEngine;
using Verse;


namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class YR_Hentai_Prime_AnimationBed_Setting : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref YR_TestLog, "YR_TestLog", false);
        }

        public void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard listing_Standard = new Listing_Standard
            {
                ColumnWidth = (canvas.width - 34f) / 2f
            };
            listing_Standard.Begin(canvas);

            listing_Standard.CheckboxLabeled("YR_TestLog".Translate(), ref YR_TestLog);
        }

        public static bool YR_TestLog = false;
    }
}