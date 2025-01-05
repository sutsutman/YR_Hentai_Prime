using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // Tentacle Altar의 속성 정의 클래스
    public class CompProperties_Building_Tentacle_Altar : CompProperties
    {
        // 생성자에서 해당 컴포넌트 클래스를 지정
        public CompProperties_Building_Tentacle_Altar()
        {
            compClass = typeof(Comp_Building_Tentacle_Altar);
        }

        // 필요한 시체 수 및 최대 시체 수 설정
        public int needCorpseCount = 2;
        public int maxCorpseCount = 4;

        // 생성할 아이템과 그래픽 데이터
        public ThingDef thingDef = null;
        public GraphicData graphicData = null;

        // 번역 키 문자열 정의
        public string needCorpseString = "YR_NeedCorpse_Tentacle_Tree";
        public string moreNeedCorpseString = "YR_MoreNeedCorpse_Tentacle_Tree";
        public string maxCorpseString = "YR_MaxCorpse";

        // 명령 버튼 관련 문자열 정의
        public string addCorpseCommandLabel = "YR_AddCorpseCommandLabel";
        public string addCorpseCommandDesc = "YR_AddCorpseCommandDesc";
        public string maxCorpseCommandLabel = "YR_MaxCorpseCommandLabel";
        public string maxCorpseCommandDesc = "YR_MaxCorpseCommandDesc";
    }

    // Tentacle Altar의 컴포넌트 클래스
    public class Comp_Building_Tentacle_Altar : ThingComp
    {
        // 부모 건물 객체 가져오기
        private Building_Tentacle_Altar BTA => (Building_Tentacle_Altar)parent;

        // 속성 가져오기
        public CompProperties_Building_Tentacle_Altar Props => (CompProperties_Building_Tentacle_Altar)props;

        // 추가 검사 문자열을 반환하는 메서드
        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int needCorpseCount = Props.needCorpseCount - BTA.offeredCorpse;
            int moreNeedCorpseCount = Props.maxCorpseCount - BTA.offeredCorpse;

            if (BTA.offeredCorpse >= Props.maxCorpseCount)
            {
                stringBuilder.Append(Props.maxCorpseString.Translate(BTA.offeredCorpse, needCorpseCount, moreNeedCorpseCount, Props.maxCorpseCount));
            }
            else if (needCorpseCount >= 1)
            {
                stringBuilder.Append(Props.needCorpseString.Translate(BTA.offeredCorpse, needCorpseCount, moreNeedCorpseCount, Props.maxCorpseCount));
            }
            else if (needCorpseCount <= 0)
            {
                stringBuilder.Append(Props.moreNeedCorpseString.Translate(BTA.offeredCorpse, needCorpseCount, moreNeedCorpseCount, Props.maxCorpseCount));
            }

            return stringBuilder.ToString();
        }

        // 추가 명령 버튼을 반환하는 메서드
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Prefs.DevMode)
            {
                // 시체 추가 버튼
                yield return new Command_Action
                {
                    defaultLabel = Props.addCorpseCommandLabel.Translate(),
                    defaultDesc = Props.addCorpseCommandDesc.Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Item/Resource/Milk"),
                    action = delegate ()
                    {
                        BTA.offeredCorpse++;
                        Pawn pawn = BTA.Map.mapPawns.FreeColonists.NullOrEmpty() ? BTA.Map.mapPawns.FreeColonists[1] : BTA.Map.mapPawns.AllPawns[1];
                        BTA.Evolution();
                    }
                };

                // 최대 시체 수 도달 버튼
                yield return new Command_Action
                {
                    defaultLabel = Props.maxCorpseCommandLabel.Translate(),
                    defaultDesc = Props.maxCorpseCommandDesc.Translate(),
                    icon = ContentFinder<Texture2D>.Get("Things/Item/Resource/Milk"),
                    action = delegate ()
                    {
                        BTA.offeredCorpse = Props.maxCorpseCount;
                        Pawn pawn = BTA.Map.mapPawns.FreeColonists.NullOrEmpty() ? BTA.Map.mapPawns.FreeColonists[1] : BTA.Map.mapPawns.AllPawns[1];
                        BTA.Evolution();
                    }
                };
            }
        }
    }
}
