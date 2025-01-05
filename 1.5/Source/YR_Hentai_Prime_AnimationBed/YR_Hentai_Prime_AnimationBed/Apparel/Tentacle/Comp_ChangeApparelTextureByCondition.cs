//using RimWorld;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class CompProperties_ChangeApparelTextureByCondition : CompProperties
//    {
//        public CompProperties_ChangeApparelTextureByCondition()
//        {
//            compClass = typeof(Comp_ChangeApparelTextureByCondition);
//        }
//        public List<ChangeApparelTextureByCondition> changeApparelTextureByConditions = new List<ChangeApparelTextureByCondition>();
//    }

//    public class ChangeApparelTextureByCondition
//    {
//        public Condition condition;
//        public string texturePath;
//    }

//    public class Comp_ChangeApparelTextureByCondition : ThingComp
//    {
//        public CompProperties_ChangeApparelTextureByCondition Props => (CompProperties_ChangeApparelTextureByCondition)props;
//        public Apparel Apparel => parent as Apparel;
//        public Pawn Wearer => Apparel.Wearer;

//        public ApparelGraphicRecord apparelGraphicRecord = new ApparelGraphicRecord();

//        public bool changeTexture = true;

//        public bool graphicMaked = false;

//        private PawnCondition compPawnCondition = null;
//        private PawnCondition originalPawnCondition = null;

//        public override void PostSpawnSetup(bool respawningAfterLoad)
//        {
//            base.PostSpawnSetup(respawningAfterLoad);
//            changeTexture = true;
//            graphicMaked = false;
//        }
//        public override void Notify_Equipped(Pawn pawn)
//        {
//            base.Notify_Equipped(pawn);
//            changeTexture = true;
//            graphicMaked = false;
//        }
//        public override void Notify_Unequipped(Pawn pawn)
//        {
//            base.Notify_Unequipped(pawn);
//            changeTexture = true;
//            graphicMaked = false;
//        }
//        public override void CompDrawWornExtras()
//        {
//            base.CompDrawWornExtras();

//            if (changeTexture)
//            {
//                if (TryGetGraphicApparel_Transparency(Apparel, Wearer.story.bodyType, out apparelGraphicRecord))
//                {
//                    Wearer.Drawer.renderer.graphics.ResolveAllGraphics();
//                    changeTexture = false;
//                    graphicMaked = true;
//                }
//                else
//                {
//                    changeTexture = false;
//                    graphicMaked = false;
//                }
//            }
//        }
//        public override void CompTick()
//        {
//            base.CompTick();
//            CheckPawnAndCompPawnCondition();
//        }
//        void CheckPawnAndCompPawnCondition()
//        {
//            List<Hediff> hediffs = Wearer.health.hediffSet.hediffs;
//            List<Trait> traits = Wearer.story.traits.allTraits;

//            if (compPawnCondition == null)
//            {
//                compPawnCondition = new PawnCondition
//                {
//                    race = Wearer.def,
//                    gender = Wearer.gender,
//                    hediffDefs = hediffs.Select(x => x.def).ToList(),
//                    bodyTypeDef = Wearer.story.bodyType,
//                    traitDefs = traits.Select(x => x.def).ToList()
//                };
//                changeTexture = true;
//                return;
//            }
//            else
//            {
//                originalPawnCondition = new PawnCondition
//                {
//                    race = Wearer.def,
//                    gender = Wearer.gender,
//                    bodyTypeDef = Wearer.story.bodyType
//                };
//            }

//            bool race = compPawnCondition.race == originalPawnCondition.race;
//            bool gender = compPawnCondition.gender == originalPawnCondition.gender;

//            bool hediffDef = hediffs.Select(x => x.def).SequenceEqual(compPawnCondition.hediffDefs);

//            bool bodyTypeDef = compPawnCondition.bodyTypeDef == originalPawnCondition.bodyTypeDef;

//            bool traitDef = traits.Select(x => x.def).SequenceEqual(compPawnCondition.traitDefs);

//            bool allEqual = race && gender && hediffDef && bodyTypeDef && traitDef;

//            if (!allEqual)
//            {
//                compPawnCondition = new PawnCondition
//                {
//                    race = Wearer.def,
//                    gender = Wearer.gender,
//                    hediffDefs = hediffs.Select(x => x.def).ToList(),
//                    bodyTypeDef = Wearer.story.bodyType,
//                    traitDefs = traits.Select(x => x.def).ToList()
//                };
//                changeTexture = true;
//            }
//        }

//        public bool TryGetGraphicApparel_Transparency(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec)
//        {
//            if (bodyType == null)
//            {
//                Log.Error("Getting apparel graphic with undefined body type.");
//                bodyType = BodyTypeDefOf.Male;
//            }

//            rec = new ApparelGraphicRecord(null, null);

//            string path;
//            bool result = false;
//            foreach (var changeApparelTextureByCondition in Props.changeApparelTextureByConditions)
//            {
//                if (Condition.Match(Wearer, changeApparelTextureByCondition.condition))
//                {
//                    path = changeApparelTextureByCondition.texturePath;

//                    //if (apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !PawnRenderer.RenderAsPack(apparel) && apparel.WornGraphicPath != BaseContent.PlaceholderImagePath && apparel.WornGraphicPath != BaseContent.PlaceholderGearImagePath)
//                    //{
//                    //    var bodyTypeDefName = bodyType.defName;

//                    //    path += "_" + bodyTypeDefName;
//                    //}

//                    Shader shader = ShaderDatabase.Cutout;
//                    if (apparel.def.apparel.useWornGraphicMask)
//                    {
//                        shader = ShaderDatabase.CutoutComplex;
//                    }

//                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
//                    rec = new ApparelGraphicRecord(graphic, apparel);

//                    result = true;
//                    if (Condition.NeedBreak(changeApparelTextureByCondition.condition))
//                    {
//                        break;
//                    }
//                }
//            }
//            return result;
//        }
//    }
//}
