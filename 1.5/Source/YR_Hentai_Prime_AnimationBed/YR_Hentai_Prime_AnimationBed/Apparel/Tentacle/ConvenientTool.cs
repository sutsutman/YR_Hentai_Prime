using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    internal class ConvenientTool
    {
        public static void ShotSexSound(Pawn pawn, EroVoiceDef femaleEroVoiceDef = null, EroVoiceDef maleEroVoiceDef = null, EroVoiceDef noneEroVoiceDef = null, int voiceProbability = 50)
        {
            if (pawn == null)
            {
                return;
            }

            SoundDef soundDef = null;

            // 성별에 따른 적절한 소리를 선택합니다.
            switch (pawn.gender)
            {
                case Gender.Female:
                    if (femaleEroVoiceDef != null) soundDef = RandSound(femaleEroVoiceDef);
                    break;
                case Gender.Male:
                    if (maleEroVoiceDef != null) soundDef = RandSound(maleEroVoiceDef);
                    break;
                case Gender.None:
                    if (noneEroVoiceDef != null) soundDef = RandSound(noneEroVoiceDef);
                    break;
            }

            // 설정 및 확률에 따라 소리를 재생합니다.
            if (
                //YR_RJW_Setting.YR_SexSound && 
                soundDef != null && new System.Random().Next(0, 101) <= voiceProbability)
            {
                soundDef.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
            }
        }
        static SoundDef RandSound(EroVoiceDef eroVoiceDef)
        {
            return eroVoiceDef.soundDefs.RandomElement();
        }
    }
}