using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class HediffCompProperties_MovingHead : HediffCompProperties
    {
        public HediffCompProperties_MovingHead() => compClass = typeof(HediffComp_MovingHead);

        public AnimationDef animationDef;
    }

    public class HediffComp_MovingHead : HediffComp
    {
        public HediffCompProperties_MovingHead Props => (HediffCompProperties_MovingHead)props;
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            Pawn.Drawer.renderer.SetAnimation(Props.animationDef);

            foreach (var a in Pawn.Drawer.renderer.renderTree.rootNode.children)
            {
                LogPawnNode(a);
                Log.Error("===children===");
                if (a.children != null)
                {

                    foreach (var b in a.children)
                    {
                        LogPawnNode(b);
                    }
                }
                Log.Error("@================@");
            }

        }

        private static void LogPawnNode(PawnRenderNode node)
        {
            Log.Error(node.ToString() + " ; " + node.Props.tagDef ?? "tagDef" + " ; " + node.Props.workerClass?.ToString() ?? "workerClass" + " ; " + node.Props.nodeClass?.ToString() ?? "nodeClass");
        }

        int tick = 200;

        public PawnDrawParms PawnDrawParms
        {
            get
            {
                return new PawnDrawParms
                {
                    facing = Pawn.Rotation,
                    rotDrawMode = RotDrawMode.Fresh,
                    posture = Pawn.GetPosture(),
                    flags = (PawnRenderFlags.Headgear | PawnRenderFlags.Clothes),
                    tint = Color.white
                };
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            ddddddd(out var outNode);

            if (tick <= 0)
            {
                Pawn.Drawer.renderer.SetAnimation(null);


                Traverse.Create(outNode).Field<Graphic>("graphic").Value = null;

                Pawn.Drawer.renderer.renderTree.EnsureInitialized(PawnRenderFlags.DrawNow);

                Pawn.health.RemoveHediff(parent);
            }
        }

        private void ddddddd(out PawnRenderNode outNode)
        {
            outNode = null;
            if (Pawn.Spawned)
            {
                foreach (var a in Pawn.Drawer.renderer.renderTree?.rootNode?.children)
                {
                    NewMethod(a, out var result);
                    outNode = result;
                }
            }
        }

        private void NewMethod(PawnRenderNode a, out PawnRenderNode outNode)
        {
            outNode = null;
            if (a.Props.tagDef == PawnRenderNodeTagDefOf.Head)
            {
                var pos = Pawn.DrawPos;
                pos.x += 1;
                DrawGraphic(a, pos);

                if (a.children != null)
                {
                    foreach (var b in a.children)
                    {
                        pos.y += 0.1f;
                        DrawGraphic(b, pos);
                    }
                }
            }
            if (a.Props.tagDef == PawnRenderNodeTagDefOf.Body)
            {
                var pos = Pawn.DrawPos;
                pos.x -= 1;
                DrawGraphic(a, pos);

                Traverse.Create(a).Field<Graphic>("graphic").Value = GraphicDatabase.Get<Graphic_Multi>(Pawn.story.bodyType.bodyDessicatedGraphicPath, a.ShaderFor(Pawn));
                outNode = a;
                if (a.children != null)
                {
                    foreach (var b in a.children)
                    {
                        pos.y += 0.1f;
                        DrawGraphic(b, pos);
                    }
                }
            }
        }

        private void DrawGraphic(PawnRenderNode a, Vector3 pos)
        {
            var graphic = a.GraphicFor(Pawn);
            if (graphic != null)
            {
                if (Pawn.def is AlienRace.ThingDef_AlienRace alienDef)
                {
                    graphic.drawSize = alienDef.alienRace.generalSettings.alienPartGenerator.customDrawSize * 1.5f;
                }
                else
                {
                    graphic.drawSize = Pawn.DrawSize;
                }
                graphic.Draw(pos, Rot4.South, Pawn);
            }
        }
    }
}
