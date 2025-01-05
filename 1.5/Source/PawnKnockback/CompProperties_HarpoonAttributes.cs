using System;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000005 RID: 5
	public class CompProperties_HarpoonAttributes : CompProperties
	{
		// Token: 0x0600001C RID: 28 RVA: 0x00002E29 File Offset: 0x00001029
		public CompProperties_HarpoonAttributes()
		{
			this.compClass = typeof(Comp_HarpoonAttributes);
		}

		// Token: 0x04000017 RID: 23
		public DamageKnockbackDef damageKnockbackDef;
	}
}
