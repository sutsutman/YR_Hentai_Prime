using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public static class TestLog
    {
        public static void Error(params string[] messages)
        {
            return;
            if (Prefs.DevMode)
            {
                string combinedMessage = string.Join(" ", messages);
                Log.Error(combinedMessage);
            }
        }
    }
}
