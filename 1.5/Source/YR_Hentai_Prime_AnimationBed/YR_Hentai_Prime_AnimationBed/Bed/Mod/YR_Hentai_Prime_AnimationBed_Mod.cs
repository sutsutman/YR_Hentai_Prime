using HarmonyLib;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed

{
    [StaticConstructorOnStartup]
    public class YR_Hentai_Prime_AnimationBed_Mod : Mod
    {
        public YR_Hentai_Prime_AnimationBed_Mod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<YR_Hentai_Prime_AnimationBed_Setting>();

            Harmony harmony = new Harmony("YR_Hentai_Prime_AnimationBed_Mod.Patch");
            harmony.PatchAll();
        }

        public override string SettingsCategory()
        {
            return "YR_Hentai_Prime_AnimationBed_Mod".Translate();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }

        // Token: 0x04000001 RID: 1
        public static YR_Hentai_Prime_AnimationBed_Setting Settings;
    }
}