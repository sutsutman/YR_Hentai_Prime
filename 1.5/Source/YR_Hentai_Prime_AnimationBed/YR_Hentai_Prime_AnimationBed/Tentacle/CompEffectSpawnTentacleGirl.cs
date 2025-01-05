using RimWorld;
using Verse;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompEffectSpawnTentacleGirl : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            PawnGenerationRequest request = new PawnGenerationRequest(YR_H_P_DefOf.YR_Tentacle_Girl, usedBy.Faction, fixedGender: Gender.Female, fixedBiologicalAge: 0);

            Pawn pawn = PawnGenerator.GeneratePawn(request);

            GenSpawn.Spawn(pawn, usedBy.Position, usedBy.Map);
        }
    }
}
