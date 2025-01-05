using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    internal class CompProperties_SecondLayer : CompProperties
    {
        public GraphicData graphicData = null;
        public bool drawOnlySouth = false;

        public AltitudeLayer altitudeLayer = AltitudeLayer.FloorEmplacement;
        public float adjustLayerFloat = 0;

        public float Altitude
        {
            get
            {
                return altitudeLayer.AltitudeFor() + adjustLayerFloat;
            }
        }

        public CompProperties_SecondLayer() => compClass = typeof(CompSecondLayer);
    }
    internal class CompSecondLayer : ThingComp
    {
        bool drawOnlySouth = false;
        private Graphic graphicInt;
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            drawOnlySouth = Props.drawOnlySouth;
        }

        public CompProperties_SecondLayer Props
        {
            get
            {
                return (CompProperties_SecondLayer)props;
            }
        }

        public virtual Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if (Props.graphicData == null)
                    {
                        Log.ErrorOnce($"[GloomyFurniture] {parent.def} has no SecondLayer graphicData but we are trying to access it.", 764532);
                        return BaseContent.BadGraphic;
                    }
                    graphicInt = Props.graphicData.Graphic;
                }
                return graphicInt;
            }
        }

        public override void PostDraw()
        {
            if (drawOnlySouth && parent.Rotation == Rot4.South)
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }
            else
            {
                Graphic.Draw(GenThing.TrueCenter(parent.Position, parent.Rotation, parent.def.size, Props.Altitude), parent.Rotation, parent, 0f);
            }
        }
    }
}
