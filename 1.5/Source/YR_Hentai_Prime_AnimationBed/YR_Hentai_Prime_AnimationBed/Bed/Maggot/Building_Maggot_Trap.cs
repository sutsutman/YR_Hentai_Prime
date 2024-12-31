using RimWorld;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    public class Building_Maggot_Trap : Building_Trap
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(Position, Map, false));
            }
        }

        bool traped = false;
        Pawn pawn = null;
        protected override void SpringSub(Pawn p)
        {
            YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(Position, Map, false));
            if (p == null)
            {
                return;
            }
            traped = true;
            pawn = p;
        }
        public override void Tick()
        {
            base.Tick();
            if (traped)
            {
                //함정은 발동시 소멸이라 컨테이너에 안들어감
                //발동 타이밍 때문에 편지 날라옴

                var maggot_QueenComp = this.TryGetComp<Comp_Maggot_Queen>();
                Building_AnimationBed bed = (Building_AnimationBed)GenSpawn.Spawn(maggot_QueenComp.Props.bedDef, pawn.Position, pawn.Map);
                bed.SetFaction(Faction.OfPlayer);
                var compAnimationBed = bed.TryGetComp<CompAnimationBed>();

                JobDriver_CarryToAnimationBed.ChainTakeeToPlatform(pawn, pawn, compAnimationBed);
            }
        }
        protected override float SpringChance(Pawn p)
        {
            var result = base.SpringChance(p);

            if (!p.RaceProps.Humanlike)
            {
                result = 0;
            }
            return result;
        }


        // Token: 0x040042D7 RID: 17111
        private static readonly FloatRange DamageRandomFactorRange = new FloatRange(0.8f, 1.2f);

        // Token: 0x040042D8 RID: 17112
        private static readonly float DamageCount = 5f;
    }
}
