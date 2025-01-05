using System;
using UnityEngine;
using Verse;

namespace YR_ItemFormChange
{
    // Token: 0x02000006 RID: 6
    [StaticConstructorOnStartup]
    public class Command_Transform_Action : Command
    {
        // Token: 0x06000012 RID: 18 RVA: 0x0000265B File Offset: 0x0000085B
        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            action();
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002674 File Offset: 0x00000874
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            GizmoResult gizmoResult = base.GizmoOnGUI(topLeft, maxWidth, parms);
            bool flag = compFormChange.cooldownNow > 0;
            if (flag)
            {
                float num = Mathf.InverseLerp(compFormChange.cooldownMax, 0f, compFormChange.cooldownNow);
                Widgets.FillableBar(rect, Mathf.Clamp01(num), cooldownBarTex, null, false);
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

        // Token: 0x0400000F RID: 15
        public Action action;

        // Token: 0x04000010 RID: 16
        public CompFormChange compFormChange;

        // Token: 0x04000011 RID: 17
        public TransformData transformData;

        // Token: 0x04000012 RID: 18
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));
    }
}
