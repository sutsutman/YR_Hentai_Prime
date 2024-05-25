//using System.Collections.Generic;
//using UnityEngine;
//using Verse;
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
//        private static readonly Vector3 PictureOffset = new Vector3(0, 0, 0.5f);
//        private static readonly Vector3 FrameOffset = new Vector3(0, 0, 0);
//        Graphic pictureGraphic = ContentFinder<Graphic>.Get("Yuran/Things/Building/Furniture/EnjoyingArmpit/Body/Race/Yuran/2_south");
//        Graphic frameGraphic => parent.Graphic;
//        public override void PostDraw()
//        {
//            base.PostDraw();

//            // 현재 위치 가져오기
//            Vector3 drawPos = this.parent.DrawPos;
//            Vector3 picturePosition = drawPos + PictureOffset;
//            Vector3 framePosition = drawPos + FrameOffset;

//            // 텍스처 크기 계산 (예: 1x1 셀)
//            float textureSize = 256f;

//            // 그림과 액자의 영역 계산
//            Rect pictureRect = new Rect(picturePosition.x, picturePosition.z, textureSize, textureSize);
//            Rect frameRect = new Rect(framePosition.x, framePosition.z, textureSize, textureSize);

//            // 겹치는 부분 계산
//            Rect clippingRect = RectIntersection(pictureRect, frameRect);

//            // 겹치는 부분이 있으면 그림을 그립니다.
//            if (clippingRect.width > 0 && clippingRect.height > 0)
//            {
//                // 클리핑 영역 설정
//                GL.PushMatrix();
//                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
//                GL.Begin(GL.QUADS);

//                // 클리핑 적용하여 그림 그리기
//                Material mat = pictureGraphic.MatSingle;
//                mat.SetPass(0);
//                GL.Begin(GL.QUADS);
//                GL.TexCoord2(0, 0); GL.Vertex3(clippingRect.xMin, clippingRect.yMin, 0);
//                GL.TexCoord2(1, 0); GL.Vertex3(clippingRect.xMax, clippingRect.yMin, 0);
//                GL.TexCoord2(1, 1); GL.Vertex3(clippingRect.xMax, clippingRect.yMax, 0);
//                GL.TexCoord2(0, 1); GL.Vertex3(clippingRect.xMin, clippingRect.yMax, 0);
//                GL.End();

//                GL.PopMatrix();
//            }

//            // 액자 그리기
//            //frameGraphic.Draw(drawPos, Rot4.North, this.parent);
//        }

//        private Rect RectIntersection(Rect a, Rect b)
//        {
//            float xMin = Mathf.Max(a.xMin, b.xMin);
//            float xMax = Mathf.Min(a.xMax, b.xMax);
//            float yMin = Mathf.Max(a.yMin, b.yMin);
//            float yMax = Mathf.Min(a.yMax, b.yMax);

//            if (xMax >= xMin && yMax >= yMin)
//            {
//                return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
//            }
//            else
//            {
//                return Rect.zero;
//            }
//        }
//    }
//}
