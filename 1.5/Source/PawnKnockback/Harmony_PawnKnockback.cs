using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000009 RID: 9
	[StaticConstructorOnStartup]
	public static class Harmony_PawnKnockback
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00003634 File Offset: 0x00001834
		static Harmony_PawnKnockback()
		{
			Harmony harmony = new Harmony("Amnabi.Knockback");
			harmony.PatchAll();
			harmony.Patch(AccessTools.Method(typeof(DamageWorker), "ExplosionDamageThing", null, null), new HarmonyMethod(typeof(Harmony_PawnKnockback), "HP_EDT", null), null, null, null);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x0000368C File Offset: 0x0000188C
		public static bool HP_EDT(Explosion explosion, Thing t, List<Thing> damagedThings, List<Thing> ignoredThings, IntVec3 cell)
		{
			Harmony_PawnKnockback.SA_Exp = explosion;
			return true;
		}

		// Token: 0x0400002C RID: 44
		public static Explosion SA_Exp;
	}
}
