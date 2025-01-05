using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000004 RID: 4
	public class Bullet_Harpoon : Bullet
	{
		// Token: 0x06000012 RID: 18 RVA: 0x000028B4 File Offset: 0x00000AB4
		public static void retract(DamageKnockbackDef toc, ThingDef bulletDef, Thing initiator, Map map, Vector3 startVec, Vector3 endVec, bool useExplosionCenter)
		{
			PawnEmptyFlyer pawnEmptyFlyer = PawnEmptyFlyer.MakeFlyer(toc.emptyFlyerToApply, startVec, endVec.ToIntVec3());
			bool flag = pawnEmptyFlyer != null;
			if (flag)
			{
				PawnEmptyFlyer pawnEmptyFlyer2 = null;
				bool flag2 = pawnEmptyFlyer != null;
				bool flag3;
				if (flag2)
				{
					pawnEmptyFlyer2 = pawnEmptyFlyer;
					flag3 = true;
				}
				else
				{
					flag3 = false;
				}
				bool flag4 = flag3;
				if (flag4)
				{
					pawnEmptyFlyer2.speedMultiplication = toc.knockbackSpeedMultiplier;
					pawnEmptyFlyer2.heightMultiplication = toc.knockbackHeightMultiplier;
					pawnEmptyFlyer2.damageDefRef = toc;
					pawnEmptyFlyer2.centerIfNoinstigator = endVec;
					pawnEmptyFlyer2.instigator = initiator;
					pawnEmptyFlyer2.instigatorEquipmentDef = null;
					pawnEmptyFlyer2.adjustFlightTimeMax();
					pawnEmptyFlyer2.bulletDef = bulletDef;
				}
				GenSpawn.Spawn(pawnEmptyFlyer, startVec.ToIntVec3(), map, WipeMode.Vanish);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002958 File Offset: 0x00000B58
		public void retract(Map map, bool useExplosionCenter)
		{
			bool flag = this.TryGetComp<Comp_HarpoonAttributes>() != null;
			if (flag)
			{
				DamageKnockbackDef damageKnockbackDef = this.TryGetComp<Comp_HarpoonAttributes>().Props.damageKnockbackDef;
				PawnEmptyFlyer pawnEmptyFlyer = PawnEmptyFlyer.MakeFlyer(damageKnockbackDef.emptyFlyerToApply, base.Position.ToVector3Shifted(), base.Launcher.Position);
				bool flag2 = pawnEmptyFlyer != null;
				if (flag2)
				{
					PawnEmptyFlyer pawnEmptyFlyer2 = null;
					bool flag3 = pawnEmptyFlyer != null;
					bool flag4;
					if (flag3)
					{
						pawnEmptyFlyer2 = pawnEmptyFlyer;
						flag4 = true;
					}
					else
					{
						flag4 = false;
					}
					bool flag5 = flag4;
					if (flag5)
					{
						pawnEmptyFlyer2.speedMultiplication = damageKnockbackDef.knockbackSpeedMultiplier;
						pawnEmptyFlyer2.heightMultiplication = damageKnockbackDef.knockbackHeightMultiplier;
						pawnEmptyFlyer2.damageDefRef = damageKnockbackDef;
						pawnEmptyFlyer2.instigator = base.Launcher;
						pawnEmptyFlyer2.instigatorEquipmentDef = null;
						pawnEmptyFlyer2.adjustFlightTimeMax();
						pawnEmptyFlyer2.bulletDef = this.def;
					}
					GenSpawn.Spawn(pawnEmptyFlyer, this.destination.ToIntVec3(), map, WipeMode.Vanish);
				}
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002A44 File Offset: 0x00000C44
		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			bool flag = hitThing == null || !(hitThing is Pawn);
			if (flag)
			{
				this.retract(base.Map, false);
			}
			try
			{
				AbsorbDamagePatch.SA_2138784 = this;
				AbsorbDamagePatch.SA_2138785 = base.Map;
				base.Impact(hitThing, false);
			}
			catch (Exception ex)
			{
				Log.Error(ex.StackTrace);
			}
			finally
			{
				AbsorbDamagePatch.SA_2138784 = null;
				AbsorbDamagePatch.SA_2138785 = null;
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002AD8 File Offset: 0x00000CD8
		public static float round(float f)
		{
			return Mathf.Round(f * 100f) / 100f;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002AFC File Offset: 0x00000CFC
		public static Mesh meshOfSize(float length)
		{
			length = Bullet_Harpoon.round(length);
			Mesh mesh;
			bool flag = !Bullet_Harpoon.planes.TryGetValue(length, out mesh);
			if (flag)
			{
				mesh = MeshMakerPlanes.NewPlaneMesh(new Vector2(length, 1f), false, false, false);
				Bullet_Harpoon.planes.Add(length, mesh);
			}
			return mesh;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002B50 File Offset: 0x00000D50
		public static Vector3 vec3MLC(float x, float xAmp, int level, float yAmp)
		{
			Vector3 result = default(Vector3);
			result.x = x * xAmp;
			float num = 2f;
			for (int i = 0; i < level; i++)
			{
				result.z += yAmp * Mathf.Sin((x - 0.5f) * 2f * 3.1415927f * num) / num;
				num *= 2f;
			}
			return result;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002BC0 File Offset: 0x00000DC0
		public virtual void Draw()
		{
			bool flag = this.launcher != null;
			if (flag)
			{
				DamageKnockbackDef damageKnockbackDef = this.def.projectile.damageDef as DamageKnockbackDef;
				bool flag2 = damageKnockbackDef != null && damageKnockbackDef.useHarpoonGraphic;
				if (flag2)
				{
					Vector3 drawPos = this.launcher.DrawPos;
					Vector3 drawPos2 = this.DrawPos;
					float y = drawPos2.y;
					drawPos2.y = 0f;
					drawPos.y = 0f;
					float num = Vector3.Distance(drawPos2, drawPos);
					float num2 = damageKnockbackDef.harpoonUseHarpoonUnitInstead ? (num / damageKnockbackDef.harpoonUnit) : ((float)damageKnockbackDef.harpoonSegements);
					float num3 = Mathf.Cos((float)(Find.TickManager.TicksAbs - this.thingIDNumber) * damageKnockbackDef.harpoonSineSpeed) * Mathf.Sqrt(num) / 5f;
					Quaternion quaternion = Quaternion.AngleAxis(-Mathf.Atan2((drawPos2 - drawPos).z, (drawPos2 - drawPos).x) * 180f / 3.1415927f, Vector3.up);
					float num4 = 1f - (num2 - Mathf.Floor(num2));
					bool flag3 = num4 == 1f;
					if (flag3)
					{
						num4 = 0f;
					}
					Vector3 b = Bullet_Harpoon.vec3MLC(-num4 / num2, num, 5, num3 * damageKnockbackDef.harpoonSineZAmp);
					for (float num5 = 1f - num4; num5 <= num2; num5 += 1f)
					{
						Vector3 vector = Bullet_Harpoon.vec3MLC(num5 / num2, num, 5, num3 * damageKnockbackDef.harpoonSineZAmp);
						float length = Vector3.Distance(vector, b);
						Vector3 point = (vector + b) / 2f;
						point.y = 0f;
						Quaternion rhs = Quaternion.AngleAxis(-Mathf.Atan2((vector - b).z, (vector - b).x) * 180f / 3.1415927f, Vector3.up);
						Graphics.DrawMesh(Bullet_Harpoon.meshOfSize(length), quaternion * point + drawPos + new Vector3(0f, y, 0f), quaternion * rhs, damageKnockbackDef.graphicDataHarpoonRope.Graphic.MatSingle, 0);
						b = vector;
					}
				}
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002E0A File Offset: 0x0000100A
		public override void ExposeData()
		{
			base.ExposeData();
		}

		// Token: 0x04000016 RID: 22
		private static Dictionary<float, Mesh> planes = new Dictionary<float, Mesh>();
	}
}
