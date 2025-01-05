// Unity 및 Verse 네임스페이스 사용
using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    // Command_EggVibrator 클래스: 사용자 인터페이스에서 특정 동작을 제어하는 Gizmo 명령
    [StaticConstructorOnStartup]
    public class Command_EggVibrator : Command_VerbTarget
    {
        // Comp_EggVibrator 인스턴스를 생성자에서 설정
        public Command_EggVibrator(Comp_EggVibrator comp) => this.comp = comp;

        // 아이콘 색상 제어: overrideColor가 설정되어 있으면 해당 색상 사용
        public override Color IconDrawColor
        {
            get
            {
                Color? color = overrideColor;
                return color == null ? base.IconDrawColor : color.GetValueOrDefault();
            }
        }

        // 같은 유형의 Gizmo인지 그룹화 여부를 판단
        public override bool GroupsWith(Gizmo other)
        {
            return base.GroupsWith(other)
                && other is Command_EggVibrator command_EggVibrator
                && comp.parent.def == command_EggVibrator.comp.parent.def;
        }

        // 사용자 인터페이스에 Gizmo를 렌더링하는 메서드
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            // Gizmo의 위치와 크기 설정
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);

            // 기본 Gizmo 렌더링
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            // 쿨다운 상태를 시각적으로 표시
            if (comp.cooldownTicks > 0)
            {
                // 쿨다운 바의 진행 상태 계산
                float num = Mathf.InverseLerp(comp.Props.cooldownTicks, 0f, comp.cooldownTicks);
                Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, false);

                // 쿨다운 남은 시간 텍스트 표시
                if (comp.cooldownTicks > 0)
                {
                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(rect, num.ToStringPercent("F0"));
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }

            // 상호작용 상태 반환
            return result.State == GizmoState.Interacted && comp.cooldownTicks <= 0 ? result : new GizmoResult(result.State);
        }

        // Comp_EggVibrator 인스턴스
        private readonly Comp_EggVibrator comp;

        // 아이콘 색상 덮어쓰기 (선택적)
        public Color? overrideColor;

        // 쿨다운 바 텍스처 정의
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));
    }
}
