using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace YR_Hentai_Prime_AnimationBed
{

    public class CompProperties_Maggot_Queen_Bed : CompProperties
    {
        public CompProperties_Maggot_Queen_Bed()
        {
            compClass = typeof(Comp_Maggot_Queen_Bed);
        }
        public int tendTick = 100;
        public int destroySelfAndPawnTicks = -10000;
    }

    public class Comp_Maggot_Queen_Bed : ThingComp, IThingHolder
    {
        public CompProperties_Maggot_Queen_Bed Props => (CompProperties_Maggot_Queen_Bed)props;

        public CompAnimationBed CompAnimationBed => parent.TryGetComp<CompAnimationBed>();

        public ThingOwner innerContainer;
        private readonly List<Thing> tmpThings = new List<Thing>();
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            destroySelfAndPawnTicks = Props.destroySelfAndPawnTicks;
        }
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            List<Pawn> innerContainerPawns = new List<Pawn>();

            foreach (var innerContainerThing in innerContainer.ToList())
            {
                if (innerContainerThing is Pawn innerContainerPawn)
                {
                    innerContainerPawns.Add(innerContainerPawn);
                }
                if (innerContainerThing is Building)
                {
                    var temp = parent.Position;
                    temp.x += 3;
                    var building = GenSpawn.Spawn(innerContainerThing, temp, parent.Map);
                    innerContainer.Remove(innerContainerThing);
                    Log.Error("spawn : " + building.Label);
                }
            }

            innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near, null, null, true);

            foreach (var innerContainerPawn in innerContainerPawns)
            {
                var hediff = HediffMaker.MakeHediff(HediffDefOf.Anesthetic, innerContainerPawn);
                hediff.Severity = 1;
                innerContainerPawn.health.AddHediff(hediff);
            }
        }
        int tick = -10;
        int destroySelfAndPawnTicks = -10000;
        public override void CompTick()
        {
            base.CompTick();
            if (tick < -1)
            {
                tick = Props.tendTick;
            }
            if (CompAnimationBed.HeldPawn == null)
            {
                parent.Destroy();
            }
            else
            {
                tick--;

                if (tick <= 0)
                {
                    tick = Props.tendTick;

                    foreach (var hediff in CompAnimationBed.HeldPawn.health.hediffSet.GetHediffsTendable().ToList())
                    {
                        hediff.Tended(0.01f, 0.05f);
                        break;
                    }
                }
            }

            if (destroySelfAndPawnTicks > 0)
            {
                destroySelfAndPawnTicks--;

                if (destroySelfAndPawnTicks <= 0)
                {
                    var comp = parent.TryGetComp<Comp_Maggot_Warp>();

                    bool destroyed = false;
                    if (comp == null)
                    {
                        destroyed = true;
                    }
                    else
                    {
                        if (!comp.WarpPawnBool())
                        {
                            destroyed = true;
                        }
                    }
                    if (destroyed)
                    {
                        YR_H_P_DefOf.HiveSpawnSound.PlayOneShot(new TargetInfo(parent.Position, parent.Map, false));

                        Find.LetterStack.ReceiveLetter("YR_DestroySelfAndPawn_Label".Translate(CompAnimationBed.HeldPawn.LabelShort, CompAnimationBed.HeldPawn).CapitalizeFirst(), "YR_DestroySelfAndPawn_Desc".Translate(CompAnimationBed.HeldPawn.LabelShort, CompAnimationBed.HeldPawn).CapitalizeFirst(), LetterDefOf.NeutralEvent, new TargetInfo(parent.Position, parent.Map));

                        CompAnimationBed.HeldPawn.DropAndForbidEverything();
                        CompAnimationBed.HeldPawn.apparel.DropAll(parent.Position, true);

                        CompAnimationBed.HeldPawn.Destroy();
                        parent.Destroy();
                    }
                }
            }
        }


        //public override void PostRemoveCompAnimationBed.HeldPawn(Pawn pawn)
        //{
        //    base.PostRemoveCompAnimationBed.HeldPawn(pawn);

        //    if (pawn.GetPosture() != PawnPosture.LayingInBed)
        //    {
        //        var hediff = HediffMaker.MakeHediff(HediffDefOf.Anesthetic, pawn);
        //        hediff.Severity = 0.75f;
        //        pawn.health.AddHediff(hediff);
        //    }
        //}

        public Comp_Maggot_Queen_Bed()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            bool flag = !parent.SpawnedOrAnyParentSpawned;
            if (flag && Scribe.mode == LoadSaveMode.Saving)
            {
                tmpThings.Clear();
                tmpThings.AddRange(innerContainer);
                tmpThings.Clear();
            }
            Scribe_Deep.Look(ref innerContainer, "innerContainer", new object[]
            {
                this
            });
            Scribe_Values.Look(ref destroySelfAndPawnTicks, "destroySelfAndPawnTicks", -10000);

        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }
        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }
        public override string CompInspectStringExtra()
        {
            return "Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
        }
    }
}