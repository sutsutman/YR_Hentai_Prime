using RimWorld;
using System.Collections.Generic;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // AnimationBed의 속성을 정의하는 컴포넌트 프로퍼티 클래스
    public class CompProperties_AnimationBed : CompProperties
    {
        // 격리 강도에 영향을 주는 계수
        public float containmentFactor = 1f;

        // 침대에서 추가되는 Hediff 목록
        public List<HediffDef> addedHediffDefs = new List<HediffDef>();

        // 침대에서 추출된 후 추가되는 Hediff 목록
        public List<HediffDef> addedAfterEjectHediffDefs = new List<HediffDef>();

        // Pawn에 대한 조건
        public PawnCondition pawnCondition;

        // 생성자: 해당 컴포넌트를 CompAnimationBed와 연결
        public CompProperties_AnimationBed() => compClass = typeof(CompAnimationBed);
    }

    // AnimationBed 컴포넌트 클래스
    public class CompAnimationBed : ThingComp
    {
        // 격리 강도: 건물의 ContainmentStrength 스탯 값 반환
        public float ContainmentStrength => parent.GetStatValue(StatDefOf.ContainmentStrength);

        // 컴포넌트의 속성 접근자
        public CompProperties_AnimationBed Props => (CompProperties_AnimationBed)props;

        // 연결된 Building_AnimationBed 접근자
        protected Building_AnimationBed AnimationBed => (Building_AnimationBed)parent;

        // 침대가 비어있는지 여부 반환
        public bool Available => !AnimationBed.Occupied;

        // 침대에 놓인 Pawn 반환
        public Pawn HeldPawn => AnimationBed.HeldPawn;

        // 침대의 내부 컨테이너 반환
        public ThingOwner Container => AnimationBed.innerContainer;

        // 내부 컨테이너의 내용을 배출
        public void EjectContents() => AnimationBed.EjectContents();
    }
}
