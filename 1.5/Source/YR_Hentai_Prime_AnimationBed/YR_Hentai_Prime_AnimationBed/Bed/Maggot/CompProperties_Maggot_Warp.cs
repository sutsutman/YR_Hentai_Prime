using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{
    public class CompProperties_Maggot_Warp : CompProperties
    {
        public CompProperties_Maggot_Warp() => compClass = typeof(Comp_Maggot_Warp);

        public List<ThingDef> warpPointThingDefs = new List<ThingDef>();
    }

    public class Comp_Maggot_Warp : ThingComp
    {
        public CompProperties_Maggot_Warp Props => (CompProperties_Maggot_Warp)props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "YR_Warp_Label".Translate(),
                defaultDesc = "YR_Warp_Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Ui/Commands/Vent"),
                action = WarpPawn
            };
        }

        public bool mainWarpPoint = false;
        public void WarpPawn()
        {
            if (parent is Building_AnimationBed bed)
            {
                var target = bed.HeldPawn;

                if (target != null)
                {
                    foreach (Thing thing in parent.Map.spawnedThings)
                    {
                        if (Props.warpPointThingDefs.Contains(thing.def))
                        {
                            if (!thing.def.IsEdifice())
                            {
                                bed.EjectContents();
                                YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(thing.Position, thing.Map, false));
                                target.Position = thing.Position;

                            }
                        }
                    }
                }
            }
        }
        public bool WarpPawnBool()
        {
            if (parent is Building_AnimationBed bed)
            {
                var target = bed.HeldPawn;

                if (target != null)
                {
                    foreach (Thing thing in parent.Map.spawnedThings)
                    {
                        if (Props.warpPointThingDefs.Contains(thing.def))
                        {
                            if (!thing.def.IsEdifice())
                            {
                                bed.EjectContents();
                                YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(thing.Position, thing.Map, false));
                                target.Position = thing.Position;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}