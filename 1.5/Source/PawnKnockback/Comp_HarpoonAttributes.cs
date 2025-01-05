using System;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000006 RID: 6
	public class Comp_HarpoonAttributes : ThingComp
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600001D RID: 29 RVA: 0x00002E44 File Offset: 0x00001044
		public CompProperties_HarpoonAttributes Props
		{
			get
			{
				return this.props as CompProperties_HarpoonAttributes;
			}
		}
	}
}
