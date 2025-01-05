// RimWorld 및 Unity 관련 네임스페이스 사용
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // EggVibrator 컴포넌트 속성 정의 클래스
    public class CompProperties_EggVibrator : CompProperties
    {
        public CompProperties_EggVibrator() => compClass = typeof(Comp_EggVibrator);

        // 자동으로 추가될 Hediff 목록
        public List<HediffDef> hediffDefs = new List<HediffDef>();

        // 명령 실행 시 추가될 Hediff 목록
        public List<HediffDef> commandHediffDefs = new List<HediffDef>();

        // 효과가 발생하는 주기(틱 단위)
        public int ticks = 600;

        // 효과 발생 확률(0~100%)
        public int rand = 50;

        // 명령 사용 후 쿨다운 시간(틱 단위)
        public int cooldownTicks = 3000;

        // 단축키 설정
        public KeyBindingDef hotKey;

        // 생성할 오염물(필드) 정의
        public ThingDef filthDef;

        // 오염물 생성 수
        public int filthNum = 1;

        // Gizmo 명령 설명
        public string description;
    }

    // EggVibrator 컴포넌트의 동작 정의 클래스
    public class Comp_EggVibrator : ThingComp, IVerbOwner
    {
        // 내부 틱 카운터
        private int ticks = 0;

        // 명령 쿨다운 시간
        public int cooldownTicks = 0;

        // Verb 추적기
        private VerbTracker verbTracker;

        // 현재 장비를 착용한 Pawn 반환
        public Pawn Wearer => WearerOf(this);

        // 특정 컴포넌트의 착용자를 반환하는 정적 메서드
        public static Pawn WearerOf(Comp_EggVibrator comp)
        {
            return comp.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker ? pawn_ApparelTracker.pawn : null;
        }

        // Verb 속성 목록 반환
        public List<VerbProperties> VerbProperties => parent.def.Verbs;

        // Tool 속성 목록 반환
        public List<Tool> Tools => parent.def.tools;

        // Verb 소유자 유형 반환
        public ImplementOwnerTypeDef ImplementOwnerTypeDef => ImplementOwnerTypeDefOf.NativeVerb;

        // 상시 캐스터(착용자) 반환
        public Thing ConstantCaster => Wearer;

        // 고유 Verb 소유자 ID 반환
        public string UniqueVerbOwnerID()
        {
            return "YR_EggVibrator" + parent.ThingID;
        }

        // Verb가 특정 Pawn에 의해 여전히 사용 가능한지 확인
        public bool VerbsStillUsableBy(Pawn p)
        {
            return Wearer == p;
        }

        // VerbTracker 반환
        public VerbTracker VerbTracker
        {
            get
            {
                verbTracker ??= new VerbTracker(this);
                return verbTracker;
            }
        }

        // 생성 후 호출
        public override void PostPostMake()
        {
            base.PostPostMake();
        }

        // 장비 착용 시 호출
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
        }

        // 장비 해제 시 호출
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
        }

        // 속성 접근자
        public CompProperties_EggVibrator Props => (CompProperties_EggVibrator)props;

        // 매 틱마다 호출되는 메서드
        public override void CompTick()
        {
            base.CompTick();

            // 쿨다운 시간 감소
            if (cooldownTicks > 0)
            {
                cooldownTicks--;
            }

            // 효과 주기 카운터 감소
            ticks--;
            if (ticks <= 0)
            {
                ticks = Props.ticks;
                if (Wearer != null)
                {
                    // 랜덤 값 생성
                    System.Random rand = new System.Random();
                    int num = rand.Next(0, 101);

                    // 설정된 확률에 따라 Hediff 추가
                    if (num <= Props.rand)
                    {
                        foreach (HediffDef hediffDef in Props.hediffDefs)
                        {
                            Wearer.health.AddHediff(hediffDef);
                        }
                    }
                }
            }
        }

        // 착용 중인 장비에 대한 추가 Gizmo 반환
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            ThingWithComps gear = parent;
            foreach (Verb verb in VerbTracker.AllVerbs)
            {
                if (verb.verbProps.hasStandardCommand)
                {
                    if (verb is Verb_EggVibrator)
                    {
                        Comp_EggVibrator comp = ThingCompUtility.TryGetComp<Comp_EggVibrator>(parent);
                        yield return CreateVerbTargetCommand(gear, verb, comp);
                    }
                }
            }
        }

        // Verb 명령 생성
        private Command_EggVibrator CreateVerbTargetCommand(Thing gear, Verb verb, Comp_EggVibrator comp_EggVibrator)
        {
            Command_EggVibrator command_EggVibrator = new Command_EggVibrator(comp_EggVibrator)
            {
                defaultDesc = gear.def.description
            };

            // 설명이 설정되어 있으면 번역된 설명 적용
            if (!comp_EggVibrator.Props.description.NullOrEmpty())
            {
                command_EggVibrator.defaultDesc = comp_EggVibrator.Props.description.Translate(gear.def.label);
            }

            // 단축키 및 기본 레이블 설정
            command_EggVibrator.hotKey = comp_EggVibrator.Props.hotKey;
            command_EggVibrator.defaultLabel = verb.verbProps.label;
            command_EggVibrator.verb = verb;

            // 아이콘 설정
            if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
            {
                command_EggVibrator.icon = verb.verbProps.defaultProjectile.uiIcon;
                command_EggVibrator.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
                command_EggVibrator.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
                command_EggVibrator.overrideColor = new Color?(verb.verbProps.defaultProjectile.graphicData.color);
            }
            else
            {
                command_EggVibrator.icon = (verb.UIIcon != BaseContent.BadTex) ? verb.UIIcon : gear.def.uiIcon;
                command_EggVibrator.iconAngle = gear.def.uiIconAngle;
                command_EggVibrator.iconOffset = gear.def.uiIconOffset;
                command_EggVibrator.defaultIconColor = gear.DrawColor;
            }

            // 쿨다운 상태일 경우 명령 비활성화
            if (cooldownTicks > 0)
            {
                command_EggVibrator.Disable("YR_EggVibrator_Cooldown".Translate(cooldownTicks.TicksToSeconds()));
            }

            return command_EggVibrator;
        }

        // 데이터 저장/로드 처리
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownTicks, "cooldownTicks", 0, false);
        }
    }
}
