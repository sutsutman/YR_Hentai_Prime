using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_ChangeApparelTextureByCondition : CompProperties
    {
        public CompProperties_ChangeApparelTextureByCondition() => compClass = typeof(Comp_ChangeApparelTextureByCondition);

        // 조건에 따라 변경할 텍스처 목록
        public List<ChangeApparelTextureByCondition> changeApparelTextureByConditions = new List<ChangeApparelTextureByCondition>();
    }

    public class ChangeApparelTextureByCondition
    {
        // 텍스처 변경 조건과 경로
        public Condition condition;
        public string texturePath;
    }

    public class Comp_ChangeApparelTextureByCondition : ThingComp
    {
        public CompProperties_ChangeApparelTextureByCondition Props => (CompProperties_ChangeApparelTextureByCondition)props;
        public Apparel Apparel => parent as Apparel;
        public Pawn Wearer => Apparel.Wearer;

        // 착용 아이템 그래픽 기록
        public ApparelGraphicRecord apparelGraphicRecord = new ApparelGraphicRecord();

        public bool changeTexture = true;
        public bool graphicMaked = false;

        // Pawn 상태 저장
        private PawnConditionForCheck compPawnConditionForCheck = null;
        private PawnConditionForCheck originalPawnConditionForCheck = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            changeTexture = true;
            graphicMaked = false;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            changeTexture = true;
            graphicMaked = false;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            changeTexture = true;
            graphicMaked = false;
        }

        public override void CompDrawWornExtras()
        {
            base.CompDrawWornExtras();

            // 텍스처 변경 필요 시 처리
            if (changeTexture && Wearer != null)
            {
                if (TryGetGraphicApparel_Transparency(Apparel, Wearer.story.bodyType, out apparelGraphicRecord))
                {
                    Wearer.Drawer.renderer.SetAllGraphicsDirty();
                    changeTexture = false;
                    graphicMaked = true;
                }
                else
                {
                    changeTexture = false;
                    graphicMaked = false;
                }
            }
        }

        public class PawnConditionForCheck
        {
            public BodyTypeDef bodyTypeDef = null;
            public List<HediffDef> hediffDefs = new List<HediffDef>();
            public Gender? gender = null;
            public ThingDef race = null;
            public List<TraitDef> traitDefs;
        }

        int ticks = 10;
        public override void CompTick()
        {
            base.CompTick();
            if (Wearer != null)
            {
                ticks--;
                if (ticks <= 0)
                {
                    CheckPawnAndCompPawnCondition();
                    ticks = 10;
                }
            }
        }

        void CheckPawnAndCompPawnCondition()
        {
            // Wearer가 null인지 확인하여 예외 방지
            if (Wearer == null)
            {
                return;
            }

            // Hediff와 Trait 목록 가져오기
            List<Hediff> hediffs = Wearer.health?.hediffSet?.hediffs ?? new List<Hediff>();
            List<Trait> traits = Wearer.story?.traits?.allTraits ?? new List<Trait>();

            // 처음 상태 기록
            if (compPawnConditionForCheck == null)
            {
                compPawnConditionForCheck = new PawnConditionForCheck
                {
                    race = Wearer.def,
                    gender = Wearer.gender,
                    hediffDefs = hediffs.Select(x => x.def).ToList(),
                    bodyTypeDef = Wearer.story?.bodyType,
                    traitDefs = traits.Select(x => x.def).ToList()
                };
                changeTexture = true;
                return;
            }
            else
            {
                originalPawnConditionForCheck = new PawnConditionForCheck
                {
                    race = Wearer.def,
                    gender = Wearer.gender,
                    bodyTypeDef = Wearer.story?.bodyType
                };
            }

            // 상태 비교
            bool allEqual = compPawnConditionForCheck.race == originalPawnConditionForCheck.race &&
                            compPawnConditionForCheck.gender == originalPawnConditionForCheck.gender &&
                            hediffs.Select(x => x.def).SequenceEqual(compPawnConditionForCheck.hediffDefs) &&
                            compPawnConditionForCheck.bodyTypeDef == originalPawnConditionForCheck.bodyTypeDef &&
                            traits.Select(x => x.def).SequenceEqual(compPawnConditionForCheck.traitDefs);

            // 변경된 상태가 있으면 텍스처 변경 플래그 설정
            if (!allEqual)
            {
                compPawnConditionForCheck = new PawnConditionForCheck
                {
                    race = Wearer.def,
                    gender = Wearer.gender,
                    hediffDefs = hediffs.Select(x => x.def).ToList(),
                    bodyTypeDef = Wearer.story?.bodyType,
                    traitDefs = traits.Select(x => x.def).ToList()
                };
                changeTexture = true;
                Wearer.Drawer.renderer.SetAllGraphicsDirty();
            }
        }


        public bool TryGetGraphicApparel_Transparency(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec)
        {
            if (bodyType == null)
            {
                Log.Error("Getting apparel graphic with undefined body type.");
            }

            rec = new ApparelGraphicRecord(null, null);

            foreach (var changeApparelTextureByCondition in Props.changeApparelTextureByConditions)
            {
                if (Condition.Match(Wearer, null, changeApparelTextureByCondition.condition, out bool needBreak))
                {
                    string path = changeApparelTextureByCondition.texturePath;

                    // 그래픽 생성 및 설정
                    Shader shader = apparel.def.apparel.useWornGraphicMask ? ShaderDatabase.CutoutComplex : ShaderDatabase.Cutout;
                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);

                    return true;
                }
            }
            return false;
        }
    }
}
