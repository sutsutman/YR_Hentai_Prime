using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace YR_ItemFormChange
{
    // Token: 0x02000005 RID: 5
    public class CompFormChange : ThingComp
    {
        // Token: 0x06000009 RID: 9 RVA: 0x0000226C File Offset: 0x0000046C
        public ThingComp thingComp(Type t)
        {
            for (int i = 0; i < parent.AllComps.Count; i++)
            {
                bool flag = parent.AllComps[i].GetType() == t;
                if (flag)
                {
                    return parent.AllComps[i];
                }
            }
            return null;
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000022D8 File Offset: 0x000004D8
        public void tryTransformInto(Pawn pawn, TransformData tsd)
        {
            if(pawn ==null)
            {
                return;
            }
            ThingWithComps thingWithComps = null;
            //저격총 - 곤봉 처럼 왔다갔다 하면 에러 발생 할 수 있음. 가능하면 고정재료로만
            if (tsd.thingDef.MadeFromStuff)
            {
                thingWithComps = (ThingWithComps)ThingMaker.MakeThing(tsd.thingDef, parent.Stuff);
            }
            else
            {
                thingWithComps = (ThingWithComps)ThingMaker.MakeThing(tsd.thingDef);
            }
            thingWithComps.HitPoints = parent.HitPoints;
            List<ThingComp> list = new List<ThingComp>();
            list.AddRange(thingWithComps.AllComps);
            foreach (ThingComp thingComp in list)
            {
                bool flag = Props.SharedCompsResolved.Contains(thingComp.GetType());
                if (flag)
                {
                    ThingComp item = this.thingComp(thingComp.GetType());
                    thingWithComps.AllComps.Remove(thingComp);
                    thingWithComps.AllComps.Add(item);
                    parent.AllComps.Remove(item);
                    parent.AllComps.Add(thingComp);
                }
            }
            foreach (var comp in thingWithComps.AllComps)
            {
                comp.parent = thingWithComps;
            }
            CompFormChange compFormChange = ThingCompUtility.TryGetComp<CompFormChange>(thingWithComps);
            if (compFormChange != null)
            {
                compFormChange.cooldownNow = tsd.transformCooldown;
                compFormChange.cooldownMax = tsd.transformCooldown;
            }
            IThingHolder parentHolder = ParentHolder;
            Map map = parent.Map;
            Vector3 drawPos = parent.DrawPos;
            bool locked = false;
            if (parent is Apparel apparel)
            {
                locked = pawn.apparel.IsLocked(apparel);
            }
            parent.Destroy(0);
            bool flag2 = pawn == null;
            if (flag2)
            {
                bool flag3 = map != null;
                if (flag3)
                {
                    GenSpawn.Spawn(thingWithComps, IntVec3Utility.ToIntVec3(drawPos), map, 0);
                    bool flag4 = tsd.moteOnTransform != null;
                    if (flag4)
                    {
                        MoteMaker.MakeStaticMote(drawPos, map, tsd.moteOnTransform, 1f);
                    }
                }
                else
                {
                    parentHolder.GetDirectlyHeldThings().TryAdd(thingWithComps, true);
                }
            }
            else
            {
                if (thingWithComps.def.IsApparel)
                {
                    pawn.apparel.Wear((Apparel)thingWithComps,true, locked);
                }
                if (thingWithComps.def.IsWeapon)
                {
                    pawn.equipment.AddEquipment(thingWithComps);
                }
                drawPos = pawn.DrawPos;
                map = pawn.Map;
                bool flag5 = tsd.moteOnTransform != null;
                if (flag5)
                {
                    MoteMaker.MakeStaticMote(drawPos, map, tsd.moteOnTransform, 1f);
                }
            }
            bool flag6 = map != null;
            if (flag6)
            {
                bool flag7 = tsd.soundOnTransform != null;
                if (flag7)
                {
                    SoundStarter.PlayOneShot(tsd.soundOnTransform, SoundInfo.InMap(new TargetInfo(IntVec3Utility.ToIntVec3(drawPos), map, false), 0));
                }
            }
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x0600000B RID: 11 RVA: 0x00002510 File Offset: 0x00000710
        public CompPropertiesFormChange Props
        {
            get
            {
                return (CompPropertiesFormChange)props;
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x0000252D File Offset: 0x0000072D
        public override void CompTick()
        {
            cooldownTick();
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002538 File Offset: 0x00000738
        public Pawn getEquipper()
        {
            IThingHolder parentHolder = ParentHolder;
            bool flag = parentHolder != null && parentHolder is Pawn_EquipmentTracker;
            Pawn result = null;
            if (flag)
            {
                result = ((Pawn_EquipmentTracker)parentHolder).pawn;
            }
            return result;
        }

        public Pawn getApparel()
        {
            IThingHolder parentHolder = ParentHolder;
            bool flag = parentHolder != null && parentHolder is Pawn_ApparelTracker;
            Pawn result = null;
            if (flag)
            {
                result = ((Pawn_ApparelTracker)parentHolder).pawn;
            }
            return result;
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002574 File Offset: 0x00000774
        public void cooldownTick()
        {
            cooldownNow = Mathf.Max(cooldownNow - 1, 0);
            revertTickCounter++;
            bool flag = Props.revertData != null && Props.revertData.revertAfterTicks <= revertTickCounter;
            if (flag)
            {
                Pawn pawn;

                pawn = getEquipper();
                if (pawn != null)
                {
                    tryTransformInto(pawn, Props.revertData);
                }
                pawn = getApparel();
                if (pawn != null)
                {
                    tryTransformInto(pawn, Props.revertData);
                }
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x000025ED File Offset: 0x000007ED
        public IEnumerable<Gizmo> heldGizmos(Pawn pawn)
        {
            foreach (TransformData tsd2 in Props.transformData)
            {
                bool appActive = true;
                if (tsd2.needApparel != null)
                {
                    appActive = false;
                    foreach (var pawnApparel in pawn.apparel.WornApparel)
                    {
                        if (pawnApparel.def == tsd2.needApparel)
                        {
                            appActive = true;
                        }
                    }
                }
                TransformData tsdP = tsd2;
                var icon = tsd2.thingDef.uiIcon;
                if (!tsdP.commandIcon.NullOrEmpty())
                {
                    icon = ContentFinder<Texture2D>.Get(tsdP.commandIcon);
                }

                yield return new Command_Transform_Action
                {
                    defaultLabel = tsd2.label,
                    defaultDesc = tsd2.description,
                    compFormChange = this,
                    transformData = tsdP,
                    icon = icon,
                    Disabled = (cooldownNow > 0) || !appActive,
                    disabledReason = tsdP.disabledReason?.Translate(cooldownNow.TicksToSeconds())??"" + "\n" + (appActive ? (TaggedString)"" : "appActive".Translate(tsd2.needApparel.label)),
                    action = delegate
                    {
                        tryTransformInto(pawn, tsdP);
                    }
                };
            }
            if (Props.revertData != null)
            {
                TransformData tsd = Props.revertData;
                var icon = tsd.thingDef.uiIcon;
                if (!tsd.commandIcon.NullOrEmpty())
                {
                    icon = ContentFinder<Texture2D>.Get(tsd.commandIcon);
                }
                yield return new Command_AutoReversion_Action
                {
                    defaultLabel = tsd.label,
                    defaultDesc = tsd.description,
                    compFormChange = this,
                    transformData = tsd,
                    icon = icon,
                    Disabled = true,
                    disabledReason = tsd.disabledReason.Translate((Props.revertData.revertAfterTicks - revertTickCounter).TicksToSeconds(), tsd.thingDef.label)
                };
            }
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002604 File Offset: 0x00000804
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref cooldownNow, "cooldownNow", 0, false);
            Scribe_Values.Look<int>(ref cooldownMax, "cooldownMax", 0, false);
            Scribe_Values.Look<int>(ref revertTickCounter, "revertTickCounter", 0, false);
        }

        // Token: 0x0400000C RID: 12
        public int revertTickCounter;

        // Token: 0x0400000D RID: 13
        public int cooldownNow;

        // Token: 0x0400000E RID: 14
        public int cooldownMax;
    }
}
