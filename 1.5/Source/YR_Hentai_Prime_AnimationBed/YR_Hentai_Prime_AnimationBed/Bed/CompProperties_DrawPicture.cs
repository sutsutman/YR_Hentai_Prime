//using System.Collections.Generic;
//using UnityEngine;
//using Verse;
//using UnityEngine.Rendering;

//namespace YR_Hentai_Prime_AnimationBed
//{
//    public class CompProperties_DrawPicture : CompProperties
//    {
//        public CompProperties_DrawPicture()
//        {
//            this.compClass = typeof(Comp_DrawPicture);
//        }
//    }

//    public class Comp_DrawPicture : ThingComp
//    {
//        public Graphic overlayGraphic;
//        private Material stencilMaterial;

//        public override void CompTick()
//        {
//            base.CompTick();

//            // overlayGraphic을 설정합니다.
//            overlayGraphic = GraphicDatabase.Get<Graphic_Single>("Yuran/Things/Building/Furniture/EnjoyingArmpit/Body/Race/Yuran/2_south", ShaderDatabase.Transparent, new Vector2(10, 10), Color.white);

//            if (stencilMaterial == null)
//            {
//                // 셰이더를 로드합니다.
//                Shader stencilShader = Shader.Find("Custom/StencilShader");
//                if (stencilShader == null)
//                {
//                    Log.Error("StencilShader not found!");
//                    return;
//                }
//                stencilMaterial = new Material(stencilShader);
//                stencilMaterial.mainTexture = overlayGraphic.MatSingle.mainTexture;
//            }
//        }

//        public override void PostDraw()
//        {
//            base.PostDraw();

//            // 오버레이 그래픽을 그립니다.
//            this.DrawOverlayGraphic();
//        }

//        private void DrawOverlayGraphic()
//        {
//            if (overlayGraphic != null && stencilMaterial != null)
//            {
//                Vector3 drawPos = parent.DrawPos;
//                drawPos.y += 0.046875f; // 그래픽이 다른 그래픽과 겹치지 않도록 약간 높입니다.

//                // 클리핑 영역 정의
//                Matrix4x4 matrix = Matrix4x4.TRS(drawPos, Quaternion.identity, new Vector3(10, 1, 10));
//                Graphics.DrawMesh(MeshPool.plane10, matrix, stencilMaterial, 0);
//            }
//        }
//    }
//}
