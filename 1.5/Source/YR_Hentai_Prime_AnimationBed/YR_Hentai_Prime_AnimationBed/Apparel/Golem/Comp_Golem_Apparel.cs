//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class Comp_Golem_Apparel : ThingComp
//    {
//        public CompProperties_Golem_Apparel Props => (CompProperties_Golem_Apparel)props;
//        public Apparel Apparel => parent as Apparel;
//        public Pawn Wearer => Apparel.Wearer;

//        public List<GraphicAndOffsetAndDrawSize> GAOADs = new List<GraphicAndOffsetAndDrawSize>();
//        public PawnOffsets pawnOffsets = new PawnOffsets();
//        public PawnOffsets testPawnOffsets = new PawnOffsets();
//        public bool first = true;
//        public bool hediffMake = true;
//        public bool openControllSetting;
//        public Vector3 pawnVector3;

//        public Graphic DownGraphic
//        {
//            get
//            {
//                if (Props.downGraphicData == null)
//                {
//                    return null;
//                }
//                else
//                {
//                    return GraphicDatabase.Get<Graphic_Multi>(Props.downGraphicData.texPath, ShaderDatabase.CutoutComplex, Props.downGraphicData.drawSize, parent.DrawColor);
//                }
//            }
//        }

//        public override void PostExposeData()
//        {
//            base.PostExposeData();
//            Scribe_Values.Look(ref hediffMake, "hediffMake");
//        }

//        public bool ShowNormal()
//        {
//            if (DownGraphic == null)
//            {
//                return true;
//            }
//            if (Wearer.CarriedBy != null)
//            {
//                return false;
//            }
//            if (Wearer.InBed())
//            {
//                return false;
//            }
//            if (Wearer.Downed)
//            {
//                return false;
//            }
//            if (Wearer.ParentHolder == null)
//            {
//                return true;
//            }
//            return true;
//        }
//        //CompDrawWornExtras()로 하면 드로잉 - > 콤프 -> 그 다음 반영이라 타이밍이 늦음

//        public void PawnOffsetMaker()
//        {
//            if (hediffMake)
//            {
//                hediffMake = false;
//                Condition.ConditionAddHediff(Wearer, Props.conditionHediffSettings);
//                Wearer.Drawer.renderer.graphics.ResolveAllGraphics();
//            }
//            if (first)
//            {
//                first = false;
//                MakePawnOffsetsAndGAOADs();
//            }

//            ApplyGolemPawnOffset(Wearer, Wearer.DrawPos, out pawnVector3);


//            void ApplyGolemPawnOffset(Pawn pawn, Vector3 drawLoc, out Vector3 result)
//            {

//                Vector2 vector2 = new Vector2();
//                Vector2 testVector2 = new Vector2();
//                if (pawn.Rotation == Rot4.South)
//                {
//                    vector2 = pawnOffsets.south;
//                    testVector2 = testPawnOffsets.south;
//                }
//                else if (pawn.Rotation == Rot4.East)
//                {
//                    vector2 = pawnOffsets.east;
//                    testVector2 = testPawnOffsets.east;
//                }
//                else if (pawn.Rotation == Rot4.North)
//                {
//                    vector2 = pawnOffsets.north;
//                    testVector2 = testPawnOffsets.north;
//                }
//                else if (pawn.Rotation == Rot4.West)
//                {
//                    //if (pawnOffsets.west == new Vector2())
//                    //{
//                    //    vector2 = new Vector2(-pawnOffsets.east.x, pawnOffsets.east.y);
//                    //    testVector2 = testPawnOffsets.west;
//                    //}
//                    //else
//                    //{
//                    vector2 = pawnOffsets.west;
//                    testVector2 = testPawnOffsets.west;
//                    //}
//                }

//                drawLoc.x += vector2.x + testVector2.x;
//                drawLoc.z += vector2.y + testVector2.y;
//                result = drawLoc;
//            }
//        }

//        public void ShowDownGraphic()
//        {
//            DownGraphic.Draw(pawnVector3, Wearer.Rotation, Wearer, Wearer.Drawer.renderer.BodyAngle());
//        }


//        public void DrawGolem()
//        {
//            foreach (var GAOAD in GAOADs)
//            {
//                // Wearer의 회전 각도에 따른 회전 행렬을 계산합니다.
//                Quaternion rotation = Quaternion.AngleAxis(Wearer.Drawer.renderer.BodyAngle(), Vector3.up);

//                Vector2 offset = Vector2.zero;
//                float layer = 0;
//                if (Wearer.Rotation == Rot4.South)
//                {
//                    offset = GAOAD.Offsets.south.offset + GAOAD.test.Offsets.south.offset;
//                    layer = GAOAD.Offsets.south.layer + GAOAD.test.Offsets.south.layer;
//                }
//                else if (Wearer.Rotation == Rot4.North)
//                {
//                    offset = GAOAD.Offsets.north.offset + GAOAD.test.Offsets.north.offset;
//                    layer = GAOAD.Offsets.north.layer + GAOAD.test.Offsets.north.layer;
//                }
//                else if (Wearer.Rotation == Rot4.East)
//                {
//                    offset = GAOAD.Offsets.east.offset + GAOAD.test.Offsets.east.offset;
//                    layer = GAOAD.Offsets.east.layer + GAOAD.test.Offsets.east.layer;
//                }
//                else if (Wearer.Rotation == Rot4.West)
//                {
//                    if (GAOAD.Offsets.west.offset == new Vector2())
//                    {
//                        offset = new Vector2(-(GAOAD.Offsets.east.offset.x + GAOAD.test.Offsets.east.offset.x), GAOAD.Offsets.east.offset.y + GAOAD.test.Offsets.east.offset.y);
//                        layer = GAOAD.Offsets.east.layer + GAOAD.test.Offsets.east.layer;
//                    }
//                    else
//                    {
//                        offset = GAOAD.Offsets.west.offset + GAOAD.test.Offsets.west.offset;
//                        layer = GAOAD.Offsets.west.layer + GAOAD.test.Offsets.west.layer;
//                    }
//                }
//                Vector3 finalPosition = CalculateFinalPosition(rotation, offset, layer);

//                if (GAOAD.test.drawSizeChange)
//                {
//                    GAOAD.Graphic = GraphicDatabase.Get<Graphic_Multi>(GAOAD.path, ShaderDatabase.CutoutComplex, GAOAD.DrawSize + GAOAD.test.DrawSize, parent.DrawColor);
//                    GAOAD.test.drawSizeChange = false;
//                }

//                // 최종 위치에서 GAOAD.Graphic을 그립니다.
//                GAOAD.Graphic.Draw(finalPosition, Wearer.Rotation, Wearer, Wearer.Drawer.renderer.BodyAngle());
//            }
//        }

//        private Vector3 CalculateFinalPosition(Quaternion rotation, Vector2 offset, float layer)
//        {
//            // GAOAD의 상대 위치를 3D 벡터로 변환합니다.
//            Vector3 offset3D = new Vector3(offset.x, 0, offset.y);

//            // 회전 행렬을 사용하여 GAOAD의 상대 위치를 회전시킵니다.
//            Vector3 rotatedOffset = rotation * offset3D;

//            // 회전된 상대 위치를 Wearer의 실제 위치에 더하여 최종 위치를 얻습니다.
//            Vector3 finalPosition = Wearer.Drawer.DrawPos + rotatedOffset;

//            // 레이어 오프셋을 Y축 값으로 설정합니다.
//            finalPosition.y += layer;
//            return finalPosition;
//        }

//        public PawnRenderFlags GetDefaultRenderFlags(Pawn pawn)
//        {
//            PawnRenderFlags pawnRenderFlags = PawnRenderFlags.None;
//            if (pawn.IsPsychologicallyInvisible())
//            {
//                pawnRenderFlags |= PawnRenderFlags.Invisible;
//            }
//            if (!pawn.health.hediffSet.HasHead)
//            {
//                pawnRenderFlags |= PawnRenderFlags.HeadStump;
//            }
//            return pawnRenderFlags;
//        }
//        public Material GetMaterial(Pawn pawn, PawnRenderer renderer, Graphic graphic, PawnRenderFlags flags)
//        {
//            var material = graphic.MatAt(pawn.Rotation, null);

//            return flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(renderer, material, flags, pawn);
//        }
//        public static Vector3 AdjustDrawPosForRotation(Pawn pawn, GraphicAndOffsetAndDrawSize GAOAD)
//        {
//            Vector3 loc = pawn.DrawPos;

//            OffsetAndLayer OAL = default;
//            OffsetAndLayer testOAL = default;

//            if (pawn.Rotation == Rot4.South)
//            {
//                OAL = GAOAD.Offsets.south;
//                testOAL = GAOAD.test.Offsets.south;
//            }
//            else if (pawn.Rotation == Rot4.East)
//            {
//                OAL = GAOAD.Offsets.east;
//                testOAL = GAOAD.test.Offsets.east;
//            }
//            else if (pawn.Rotation == Rot4.North)
//            {
//                OAL = GAOAD.Offsets.north;
//                testOAL = GAOAD.test.Offsets.north;
//            }
//            else if (pawn.Rotation == Rot4.West)
//            {
//                OAL = GAOAD.Offsets.west;
//                testOAL = GAOAD.test.Offsets.west;
//            }

//            var offset = new Vector3(OAL.offset.x, OAL.layer, OAL.offset.y);
//            var testOffset = new Vector3(testOAL.offset.x, testOAL.layer, testOAL.offset.y);
//            loc += offset + testOffset;
//            return loc;
//        }
//        //public static Material OverrideMaterialIfNeeded(PawnRenderer pawnRenderer, Material original, Pawn pawn, bool portrait = false)
//        //{
//        //    Material baseMat = (!portrait && pawn.IsPsychologicallyInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
//        //    return pawnRenderer.graphics.flasher.GetDamagedMat(baseMat);
//        //}
//        private Material OverrideMaterialIfNeeded(PawnRenderer pawnRenderer, Material original, PawnRenderFlags flags, Pawn pawn)
//        {
//            if (flags.FlagSet(PawnRenderFlags.Cache) || flags.FlagSet(PawnRenderFlags.Portrait))
//            {
//                return original;
//            }
//            if (pawn.IsPsychologicallyInvisible())
//            {
//                return InvisibilityMatPool.GetInvisibleMat(original);
//            }
//            return pawnRenderer.flasher.GetDamagedMat(original);
//        }
//        public void MakePawnOffsetsAndGAOADs()
//        {
//            MakePawnOffsets();
//            MakeGAOADs();

//            void MakePawnOffsets()
//            {
//                PawnOffsetSetting pawnOffsetSetting = Props.pawnOffsetSetting;
//                pawnOffsets = pawnOffsetSetting?.commonPawnOffsets;

//                foreach (var pawnOffsetCondition in pawnOffsetSetting.pawnOffsetConditions)
//                {
//                    if (Condition.Match(Wearer,null, pawnOffsetCondition.condition,out bool needBreak))
//                    {
//                        pawnOffsets = pawnOffsetCondition.pawnOffsets;
//                        if (needBreak)
//                        {
//                            break;
//                        }
//                    }
//                }
//            }

//            void MakeGAOADs()
//            {
//                GAOADs = new List<GraphicAndOffsetAndDrawSize> { };
//                foreach (var decorationSetting in Props.decorationSettings)
//                {
//                    if (Condition.Match(Wearer,null, decorationSetting.condition))
//                    {
//                        var drawSize = decorationSetting.commonDrawSize;
//                        var offsets = CloneOffsets(decorationSetting.commonOffsets);

//                        foreach (var offsetAndDrawSizeCondition in decorationSetting.offsetAndDrawSizeConditions)
//                        {
//                            if (Condition.Match(Wearer, null,offsetAndDrawSizeCondition.condition, out bool needBreak))
//                            {
//                                if (offsetAndDrawSizeCondition.drawSize != new Vector2())
//                                {
//                                    drawSize = offsetAndDrawSizeCondition.drawSize;
//                                }
//                                CopyLayersIfDefault(offsets, offsetAndDrawSizeCondition.offsets);
//                                offsets = offsetAndDrawSizeCondition.offsets;
//                                if (needBreak)
//                                {
//                                    break;
//                                }
//                            }
//                        }

//                        GraphicAndOffsetAndDrawSize GAOAD = new GraphicAndOffsetAndDrawSize
//                        {
//                            DrawSize = drawSize,
//                            Offsets = offsets,
//                            test = new TESTGAOAD(),
//                            Graphic = GraphicDatabase.Get<Graphic_Multi>(decorationSetting.path, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor),
//                            path = decorationSetting.path,
//                            makeTestGizmo = decorationSetting.makeTestGizmo
//                        };

//                        GAOADs.Add(GAOAD);
//                    }
//                }
//            }

//            RotationOffsetAndLayer CloneOffsets(RotationOffsetAndLayer original)
//            {
//                return new RotationOffsetAndLayer
//                {
//                    south = new OffsetAndLayer { offset = original.south.offset, layer = original.south.layer },
//                    east = new OffsetAndLayer { offset = original.east.offset, layer = original.east.layer },
//                    north = new OffsetAndLayer { offset = original.north.offset, layer = original.north.layer },
//                    west = new OffsetAndLayer { offset = original.west.offset, layer = original.west.layer }
//                };
//            }

//            void CopyLayersIfDefault(RotationOffsetAndLayer destination, RotationOffsetAndLayer source)
//            {
//                if (source.south.layer == -10)
//                {
//                    source.south.layer = destination.south.layer;
//                }
//                if (source.north.layer == -10)
//                {
//                    source.north.layer = destination.north.layer;
//                }
//                if (source.east.layer == -10)
//                {
//                    source.east.layer = destination.east.layer;
//                }
//                if (source.west.layer == -10)
//                {
//                    source.west.layer = destination.west.layer;
//                }
//            }

//        }

//        public static Matrix4x4 CreateDrawMatrix(GraphicAndOffsetAndDrawSize GAOAD, Vector3 loc)
//        {
//            var drawSize = GAOAD.DrawSize + GAOAD.test.DrawSize;
//            var matrix = Matrix4x4.TRS(loc, Quaternion.AngleAxis(0f, Vector3.up), new Vector3(drawSize.x, 1, drawSize.y));

//            return matrix;
//        }
//        public override void Notify_Unequipped(Pawn pawn)
//        {
//            base.Notify_Unequipped(pawn);
//            first = true;
//            hediffMake = true;
//            GAOADs = new List<GraphicAndOffsetAndDrawSize>();
//        }

//        public override void Notify_Equipped(Pawn pawn)
//        {
//            base.Notify_Equipped(pawn);
//            Wearer.apparel.Lock(Apparel);
//            foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
//            {
//                if (hediff.def == HediffDefOf.Anesthetic)
//                {
//                    pawn.health.RemoveHediff(hediff);
//                }
//            }
//        }
//        private Command_Action GenerateAction(string label, Texture icon, Action action)
//        {
//            return new Command_Action
//            {
//                defaultLabel = label,
//                icon = icon,
//                action = action
//            };
//        }

//        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
//        {
//            const float devF = 0.025f;
//            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
//            {
//                yield return gizmo;
//            }
//            if (Prefs.DevMode)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Open controll setting",
//                    icon = YR_H_P_Icon.rimworldLogoIcon,
//                    action = ToggleOpenControlSetting
//                };

//                if (openControllSetting)
//                {
//                    //폰 위치 수정
//                    Texture icon = ContentFinder<Texture2D>.Get("Ui/Widgets/CheckOff");
//                    yield return GenerateAction("pawn Up", icon, () => { testPawnOffsets = testPawnOffsets.AdjustOffset(0, devF, Wearer); MessagePawnOffset(testPawnOffsets); });
//                    yield return GenerateAction("pawn Down", icon, () => { testPawnOffsets = testPawnOffsets.AdjustOffset(0, -devF, Wearer); MessagePawnOffset(testPawnOffsets); });
//                    yield return GenerateAction("pawn Left", icon, () => { testPawnOffsets = testPawnOffsets.AdjustOffset(-devF, 0, Wearer); MessagePawnOffset(testPawnOffsets); });
//                    yield return GenerateAction("pawn Right", icon, () => { testPawnOffsets = testPawnOffsets.AdjustOffset(devF, 0, Wearer); MessagePawnOffset(testPawnOffsets); });
//                    yield return GenerateAction("pawn : Reset", YR_H_P_Icon.resetIcon, () => { testPawnOffsets = new PawnOffsets(); Messages.Message($"Reset", MessageTypeDefOf.SilentInput, false); });
//                    //옷 위치 수정
//                    foreach (var GAOAD in GAOADs)
//                    {
//                        icon = ContentFinder<Texture2D>.Get(GAOAD.path);
//                        var gizmoAction = ToggleOpenGizmo(GAOAD);

//                        MakeTestGizmo makeTestGizmo = GAOAD.makeTestGizmo;
//                        string label = makeTestGizmo.label;
//                        if (label.NullOrEmpty())
//                        {
//                            Log.Error("This path's makeTestGizmo's label is null or empty : " + GAOAD.path);
//                            Log.Error("This will cause some gizmo - error");
//                        }
//                        yield return new Command_Action
//                        {
//                            defaultLabel = "*" + label + "*",
//                            icon = ContentFinder<Texture2D>.Get(GAOAD.path),
//                            iconDrawScale = makeTestGizmo.uiIconScale,
//                            action = gizmoAction
//                        };

//                        if (GAOAD.test.openControl)
//                        {

//                            //위치 수정
//                            yield return GenerateAction(label + " :  Left", YR_H_P_Icon.leftIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(-devF, 0, 0, Wearer); MessageApparelOffset(GAOAD); });
//                            yield return GenerateAction(label + " :  Right", YR_H_P_Icon.rightIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(devF, 0, 0, Wearer); MessageApparelOffset(GAOAD); });
//                            yield return GenerateAction(label + " :  Up", YR_H_P_Icon.upIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(0, 0, devF, Wearer); MessageApparelOffset(GAOAD); });
//                            yield return GenerateAction(label + " :  Down", YR_H_P_Icon.downIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(0, 0, -devF, Wearer); MessageApparelOffset(GAOAD); });
//                            yield return GenerateAction(label + " :  Front", YR_H_P_Icon.frontIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(0, devF, 0, Wearer); MessageApparelOffset(GAOAD); });
//                            yield return GenerateAction(label + " :  Back", YR_H_P_Icon.backIcon, () => { GAOAD.test.Offsets = GAOAD.test.Offsets.AdjustOffset(0, -devF, 0, Wearer); MessageApparelOffset(GAOAD); });

//                            //크기 수정
//                            yield return GenerateAction(label + " : Draw Size Up", YR_H_P_Icon.drawSizeUpIcon, () => { GAOAD.test.DrawSize += new Vector2(devF, devF); MessageApparelDrawSize(GAOAD); GAOAD.test.drawSizeChange = true; });

//                            yield return GenerateAction(label + " : Draw Size Down", YR_H_P_Icon.drawSizeDownIcon, () => { GAOAD.test.DrawSize += new Vector2(-devF, -devF); MessageApparelDrawSize(GAOAD); GAOAD.test.drawSizeChange = true; });

//                            //리셋
//                            yield return GenerateAction(label + " : Reset", YR_H_P_Icon.resetIcon, () => { GAOAD.test.Offsets = new RotationOffsetAndLayer(); GAOAD.test.DrawSize = new Vector2(); Messages.Message($"Reset", MessageTypeDefOf.SilentInput, false); GAOAD.test.drawSizeChange = true; });
//                        }
//                    }
//                }
//            }

//            yield break;

//            //로컬 함수
//            void ToggleOpenControlSetting()
//            {
//                if (openControllSetting)
//                {
//                    openControllSetting = false;
//                    foreach (var GAOAD in GAOADs)
//                    {
//                        GAOAD.test.openControl = false;
//                    }
//                }
//                else
//                {
//                    openControllSetting = true;
//                }
//            }

//            void MessageApparelOffset(GraphicAndOffsetAndDrawSize GAOAD)
//            {
//                RotationOffsetAndLayer offsets = GAOAD.Offsets;
//                RotationOffsetAndLayer testOffsets = GAOAD.test.Offsets;
//                if (Wearer.Rotation == Rot4.West)
//                {
//                    MessageOAL(offsets.west, testOffsets.west, "west");
//                }
//                if (Wearer.Rotation == Rot4.North)
//                {
//                    MessageOAL(offsets.north, testOffsets.north, "north");
//                }
//                if (Wearer.Rotation == Rot4.East)
//                {
//                    MessageOAL(offsets.east, testOffsets.east, "east");
//                }
//                if (Wearer.Rotation == Rot4.South)
//                {
//                    MessageOAL(offsets.south, testOffsets.south, "south");
//                }


//                static void MessageOAL(OffsetAndLayer OAL, OffsetAndLayer testOAL, string rot4Name)
//                {
//                    ConvenientTool.Round(OAL.offset, testOAL.offset, out double x, out double y);
//                    var layer = Math.Round(OAL.layer + testOAL.layer, 5);
//                    Messages.Message($"{rot4Name} : {x}, {y}, (Layer : {layer})", MessageTypeDefOf.SilentInput, false);
//                }
//            }

//            void MessagePawnOffset(PawnOffsets testPawnOffsets)
//            {
//                if (Wearer.Rotation == Rot4.West)
//                {
//                    MessageOffset(pawnOffsets.west, testPawnOffsets.west, "west");
//                }
//                if (Wearer.Rotation == Rot4.North)
//                {
//                    MessageOffset(pawnOffsets.north, testPawnOffsets.north, "north");
//                }
//                if (Wearer.Rotation == Rot4.East)
//                {
//                    MessageOffset(pawnOffsets.east, testPawnOffsets.east, "east");
//                }
//                if (Wearer.Rotation == Rot4.South)
//                {
//                    MessageOffset(pawnOffsets.south, testPawnOffsets.south, "south");
//                }


//                static void MessageOffset(Vector2 offset, Vector2 testOffset, string rot4Name)
//                {
//                    ConvenientTool.Round(offset, testOffset, out double x, out double y);

//                    Messages.Message($"{rot4Name} : {x}, {y}", MessageTypeDefOf.SilentInput, false);
//                }
//            }

//            void MessageApparelDrawSize(GraphicAndOffsetAndDrawSize GAOAD)
//            {
//                ConvenientTool.Round(GAOAD.DrawSize, GAOAD.test.DrawSize, out double x, out double y);
//                Messages.Message($"drawSize : {x}, {y}", MessageTypeDefOf.SilentInput, false);
//            }

//            Action ToggleOpenGizmo(GraphicAndOffsetAndDrawSize GAOAD)
//            {
//                return () =>
//                {
//                    GAOAD.test.openControl = !GAOAD.test.openControl;
//                };
//            }
//        }
//    }
//}
