using UnityEngine;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    [StaticConstructorOnStartup]
    public class YR_H_P_Icon
    {
        //Dev용
        public static Texture2D rimworldLogoIcon = ContentFinder<Texture2D>.Get("UI/Heroart/RimWorldLogo");
        //방향 조정
        public static Texture2D resetIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Reset");
        public static Texture2D leftIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Left");
        public static Texture2D rightIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Right");
        public static Texture2D upIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Up");
        public static Texture2D downIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Down");
        public static Texture2D frontIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Front");
        public static Texture2D backIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/Back");
        //크기 조정
        public static Texture2D drawSizeUpIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Big");
        public static Texture2D drawSizeDownIcon = ContentFinder<Texture2D>.Get("UI/Icons/GDAV3/draw_x_Small");
    }
}
