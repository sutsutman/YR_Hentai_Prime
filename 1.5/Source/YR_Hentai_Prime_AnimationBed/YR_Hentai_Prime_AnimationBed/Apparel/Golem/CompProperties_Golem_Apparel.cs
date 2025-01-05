//using System.Collections.Generic;
//using UnityEngine;
//using Verse;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class CompProperties_Golem_Apparel : CompProperties
//    {
//        public CompProperties_Golem_Apparel()
//        {
//            compClass = typeof(Comp_Golem_Apparel);
//        }
//        public List<DecorationSetting> decorationSettings = new List<DecorationSetting>();
//        public PawnOffsetSetting pawnOffsetSetting;
//        public List<ConditionHediffSetting> conditionHediffSettings = new List<ConditionHediffSetting>();

//        public GraphicData downGraphicData;
//    }
//    public class PawnOffsetSetting
//    {
//        public List<PawnOffsetCondition> pawnOffsetConditions = new List<PawnOffsetCondition>();
//        public PawnOffsets commonPawnOffsets;
//    }

//    public class PawnOffsetCondition
//    {
//        public Condition condition;
//        public PawnOffsets pawnOffsets;
//    }

//    public class PawnOffsets
//    {
//        public Vector2 south;
//        public Vector2 east;
//        public Vector2 north;
//        public Vector2 west;

//        private Vector2 AdjustedVector2(Vector2 original, float x, float y)
//        {
//            return new Vector2(original.x + x, original.y + y);
//        }

//        public PawnOffsets AdjustOffset(float x, float y, Pawn pawn)
//        {
//            var offsets = new PawnOffsets()
//            {
//                south = south,
//                east = east,
//                north = north,
//                west = west
//            };

//            if (pawn.Rotation == Rot4.South)
//            {
//                offsets.south = AdjustedVector2(south, x, y);
//            }
//            else if (pawn.Rotation == Rot4.East)
//            {
//                offsets.east = AdjustedVector2(east, x, y);
//            }
//            else if (pawn.Rotation == Rot4.North)
//            {
//                offsets.north = AdjustedVector2(north, x, y);
//            }
//            else if (pawn.Rotation == Rot4.West)
//            {
//                offsets.west = AdjustedVector2(west, x, y);
//            }

//            return offsets;
//        }
//    }

//    public class RotationOffsetAndLayer
//    {
//        public OffsetAndLayer south = new OffsetAndLayer { layer = 0 };
//        public OffsetAndLayer east = new OffsetAndLayer { layer = 0 };
//        public OffsetAndLayer north = new OffsetAndLayer { layer = 0 };
//        public OffsetAndLayer west = new OffsetAndLayer { layer = 0 };

//        private OffsetAndLayer AdjustedOffsetAndLayer(OffsetAndLayer original, float x, float y, float z)
//        {
//            return new OffsetAndLayer { offset = new Vector2(original.offset.x + x, original.offset.y + z), layer = original.layer + y };
//        }

//        public RotationOffsetAndLayer AdjustOffset(float x, float y, float z, Pawn pawn)
//        {
//            var offsets = new RotationOffsetAndLayer()
//            {
//                south = south,
//                east = east,
//                north = north,
//                west = west
//            };

//            if (pawn.Rotation == Rot4.South)
//            {
//                offsets.south = AdjustedOffsetAndLayer(south, x, y, z);
//            }
//            else if (pawn.Rotation == Rot4.East)
//            {
//                offsets.east = AdjustedOffsetAndLayer(east, x, y, z);
//            }
//            else if (pawn.Rotation == Rot4.North)
//            {
//                offsets.north = AdjustedOffsetAndLayer(north, x, y, z);
//            }
//            else if (pawn.Rotation == Rot4.West)
//            {
//                offsets.west = AdjustedOffsetAndLayer(west, x, y, z);
//            }

//            return offsets;
//        }
//    }


//    public class OffsetAndLayer
//    {
//        public Vector2 offset = new Vector2();
//        //0으로 해두면 복사 부분이 제대로 안될 수 있어서 이렇게 해둠
//        public float layer = -10;
//    }

//    public class DecorationSetting
//    {
//        public MakeTestGizmo makeTestGizmo = new MakeTestGizmo();
//        public Condition condition;
//        public List<OffsetAndDrawSizeCondition> offsetAndDrawSizeConditions = new List<OffsetAndDrawSizeCondition>();
//        public Vector2 commonDrawSize;
//        public RotationOffsetAndLayer commonOffsets;
//        public string path;
//    }

//    public class OffsetAndDrawSizeCondition
//    {
//        public Condition condition;
//        public Vector2 drawSize;
//        public RotationOffsetAndLayer offsets;
//    }

//    public class GraphicAndOffsetAndDrawSize
//    {
//        public Graphic Graphic { get; set; }
//        public Vector2 DrawSize { get; set; }

//        public float layerOffset;
//        public RotationOffsetAndLayer Offsets { get; set; }

//        public string path;

//        public MakeTestGizmo makeTestGizmo;

//        public TESTGAOAD test = new TESTGAOAD();
//    }

//    public class TESTGAOAD
//    {
//        public bool openControl;
//        public bool drawSizeChange = false;
//        public Vector2 DrawSize { get; set; }
//        public RotationOffsetAndLayer Offsets = new RotationOffsetAndLayer();

//    }
//    public class MakeTestGizmo
//    {
//        public string label = "";
//        public string iconPath = "UI/Commands/AttackMelee";
//        public float uiIconScale = 1.5f;
//    }
//}