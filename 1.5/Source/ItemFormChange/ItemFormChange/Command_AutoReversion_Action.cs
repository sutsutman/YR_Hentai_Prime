using UnityEngine;
using Verse;

namespace YR_ItemFormChange
{
    // Token: 0x02000007 RID: 7
    [StaticConstructorOnStartup]
    public class Command_AutoReversion_Action : Command
    {
        // Token: 0x06000016 RID: 22 RVA: 0x00002746 File Offset: 0x00000946
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x00002754 File Offset: 0x00000954
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult gizmoResult = base.GizmoOnGUI(topLeft, maxWidth, parms);
            float num = 1f - Mathf.InverseLerp(compFormChange.Props.revertData.revertAfterTicks, 0f, compFormChange.revertTickCounter);
            Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, false);
            bool flag = compFormChange.cooldownNow > 0;
            if (flag)
            {
                Text.Font = 0;
                Text.Anchor = (TextAnchor)1;
                Widgets.Label(rect, GenText.ToStringPercent(num, "F0"));
                Text.Anchor = 0;
            }
            bool flag2 = gizmoResult.State == (GizmoState)2;
            GizmoResult result;
            if (flag2)
            {
                result = gizmoResult;
            }
            else
            {
                result = new GizmoResult(gizmoResult.State);
            }
            return result;
        }

        // Token: 0x04000013 RID: 19
        public CompFormChange compFormChange;

        // Token: 0x04000014 RID: 20
        public TransformData transformData;

        // Token: 0x04000015 RID: 21
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));
    }
}
