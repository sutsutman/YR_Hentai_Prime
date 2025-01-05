using System;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000007 RID: 7
	public class DamageKnockbackDef : DamageDef
	{
		// Token: 0x04000018 RID: 24
		public ThingDef bulletTipDef;

		// Token: 0x04000019 RID: 25
		public bool noKnockbackIfDeflected;

		// Token: 0x0400001A RID: 26
		public bool useExplosionCenter;

		// Token: 0x0400001B RID: 27
		public bool pullInstead;

		// Token: 0x0400001C RID: 28
		public bool pullToNearbySpotInstead;

		// Token: 0x0400001D RID: 29
		public float knockbackSpeedMultiplier = 0.3f;

		// Token: 0x0400001E RID: 30
		public float knockbackHeightMultiplier = 0.3f;

		// Token: 0x0400001F RID: 31
		public float knockbackRadiusMin = 3f;

		// Token: 0x04000020 RID: 32
		public float knockbackRadiusMax = 10f;

		// Token: 0x04000021 RID: 33
		public ThingDef flyerToApply;

		// Token: 0x04000022 RID: 34
		public ThingDef emptyFlyerToApply;

		// Token: 0x04000023 RID: 35
		public bool useHarpoonGraphic;

		// Token: 0x04000024 RID: 36
		public float harpoonSineSpeed = 0.25f;

		// Token: 0x04000025 RID: 37
		public int harpoonSegements = 20;

		// Token: 0x04000026 RID: 38
		public bool harpoonUseHarpoonUnitInstead;

		// Token: 0x04000027 RID: 39
		public float harpoonUnit = 1f;

		// Token: 0x04000028 RID: 40
		public float harpoonSineZAmp = 1f;

		// Token: 0x04000029 RID: 41
		public float landingDamageAmount = 1f;

		// Token: 0x0400002A RID: 42
		public DamageDef landingDamage;

		// Token: 0x0400002B RID: 43
		public GraphicData graphicDataHarpoonRope;
	}
}
