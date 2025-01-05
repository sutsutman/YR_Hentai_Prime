using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000002 RID: 2
	[HarmonyPatch(typeof(Thing))]
	[HarmonyPatch("TakeDamage")]
	public static class AbsorbDamagePatch
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static void Check(DamageInfo damageInfo)
		{
			bool flag = AbsorbDamagePatch.SA_2138784 != null;
			if (flag)
			{
				AbsorbDamagePatch.SA_2138784.retract(AbsorbDamagePatch.SA_2138785, false);
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000207D File Offset: 0x0000027D
		private static IEnumerable<CodeInstruction> Transpiler(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
		{
			CodeInstruction[] codeInst = instructions.ToArray<CodeInstruction>();
			int num;
			for (int i = 0; i < codeInst.Length; i = num + 1)
			{
				yield return codeInst[i];
				bool flag = i >= 4 && codeInst[i - 4].opcode == OpCodes.Ldarga_S && codeInst[i - 3].opcode == OpCodes.Ldloca_S && codeInst[i - 2].opcode == OpCodes.Callvirt && codeInst[i - 1].opcode == OpCodes.Ldloc_0 && codeInst[i].opcode == OpCodes.Brfalse_S;
				if (flag)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_1, null);
					yield return new CodeInstruction(OpCodes.Call, AbsorbDamagePatch.CheckInfo);
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x04000001 RID: 1
		public static Bullet_Harpoon SA_2138784;

		// Token: 0x04000002 RID: 2
		public static Map SA_2138785;

		// Token: 0x04000003 RID: 3
		private static readonly MethodInfo CheckInfo = AccessTools.Method(typeof(AbsorbDamagePatch), "Check", null, null);
	}
}
