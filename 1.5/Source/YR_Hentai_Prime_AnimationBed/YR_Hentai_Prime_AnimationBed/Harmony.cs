//using HarmonyLib;
//using RimWorld;
//using System;
//using UnityEngine;
//using Verse;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    [StaticConstructorOnStartup]
//    public static class Harmony_AddBaseHediff
//    {
//        static Harmony_AddBaseHediff()
//        {
//            Harmony harmonyInstance = new Harmony("Mincho_The_Mint_Choco_Slime");
//            harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderNode), "EnsureInitialized", null, null), null, new HarmonyMethod(patchType, "GenerateInitialHediffsPostFix", null), null, null);
//            //harmonyInstance.Patch(AccessTools.Method(typeof(PawnRenderNode_Body), "GraphicFor", null, null),null, new HarmonyMethod(patchType, "GraphicForPostFix", null), null, null);
//            Log.Message("Mincho_The_Mint_Choco_Slime_patched");
//        }

//        [HarmonyPostfix]
//        public static void GenerateInitialHediffsPostFix(PawnRenderNode __instance, PawnRenderFlags defaultRenderFlagsNow)
//        {
//            if (__instance is PawnRenderNode_Body)
//            {
//                Pawn pawn = __instance.tree.pawn;
//                if (pawn.health.hediffSet.HasHediff(HediffDefOf.AlcoholHigh))
//                {
//                    if (defaultRenderFlagsNow != PawnRenderFlags.Portrait)
//                    {
//                        //나중에 헤디프 쪽에서 아예 아래 방법으로 건드려보기.
//                        Traverse.Create(__instance).Field<Graphic>("graphic").Value = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, __instance.ShaderFor(pawn));
//                    }
//                }
//            }
//        }

//        [HarmonyPostfix]
//        public static void GraphicForPostFix(ref PawnRenderNode_Body __instance, ref Graphic __result)
//        {
//            Pawn pawn = __instance.tree.pawn;
//            if (__instance.AnimationWorker is AnimationWorker_KeyframesBedAnimation AKA)
//            {
//                string textureReplacePath = AKA.TexPathAtTick(pawn.Drawer.renderer.renderTree.AnimationTick);

//                if (textureReplacePath.NullOrEmpty())
//                {
//                    return;
//                }

//                Shader shader = __instance.ShaderFor(pawn);
//                if (shader == null)
//                {
//                    return;
//                }
//                Log.Error("틱 나와야 함");

//                __result = GraphicDatabase.Get<Graphic_Multi>(textureReplacePath, shader, Vector2.one, __instance.ColorFor(pawn));
//            }
//        }

//        private static readonly Type patchType = typeof(Harmony_AddBaseHediff);
//    }
//}
