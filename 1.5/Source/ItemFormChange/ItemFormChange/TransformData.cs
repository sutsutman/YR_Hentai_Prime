using Verse;

namespace YR_ItemFormChange
{
    // Token: 0x02000003 RID: 3
    public class TransformData
    {
        // Token: 0x04000001 RID: 1
        public int transformCooldown;

        // Token: 0x04000002 RID: 2
        public ThingDef thingDef;

        // Token: 0x04000003 RID: 3
        public string label;

        // Token: 0x04000004 RID: 4
        public string description;

        // Token: 0x04000005 RID: 5
        public int revertAfterTicks;

        // Token: 0x04000006 RID: 6
        public SoundDef soundOnTransform;

        // Token: 0x04000007 RID: 7
        public ThingDef moteOnTransform;

        public ThingDef needApparel = null;

        public string commandIcon;

        public string disabledReason = "YR_Cooldown";
    }
}
