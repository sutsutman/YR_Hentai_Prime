using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public static class TestLog
    {
        public static void Error(params string[] messages)
        {
            if (YR_Hentai_Prime_AnimationBed_Setting.YR_TestLog)
            {
                string combinedMessage = string.Join(" ", messages);
                Log.Error(combinedMessage);
            }
        }
    }
}
