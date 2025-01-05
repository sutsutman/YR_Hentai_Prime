using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnKnockback
{
	// Token: 0x02000008 RID: 8
	public class DamageWorker_Knockback : DamageWorker_AddInjury
	{
		// Token: 0x06000020 RID: 32 RVA: 0x00002EE0 File Offset: 0x000010E0
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing thing)
		{
			Pawn pawn = thing as Pawn;
			bool flag = pawn == null || pawn.Dead;
			DamageWorker.DamageResult damageResult = base.Apply(dinfo, thing);
			DamageKnockbackDef damageKnockbackDef = this.def as DamageKnockbackDef;
			bool flag2 = !flag && (damageKnockbackDef.useExplosionCenter || !damageKnockbackDef.noKnockbackIfDeflected || damageResult.totalDamageDealt == 0f);
			if (flag2)
			{
				this.KnockbackFlyer(dinfo, pawn);
			}
			else
			{
				bool flag3 = dinfo.Instigator != null && dinfo.Instigator.Spawned && dinfo.Instigator.Map == thing.Map;
				if (flag3)
				{
					Bullet_Harpoon.retract(damageKnockbackDef, damageKnockbackDef.bulletTipDef, damageKnockbackDef.useExplosionCenter ? null : dinfo.Instigator, thing.Map, thing.Position.ToVector3Shifted(), damageKnockbackDef.useExplosionCenter ? Harmony_PawnKnockback.SA_Exp.DrawPos : dinfo.Instigator.Position.ToVector3Shifted(), damageKnockbackDef.useExplosionCenter);
				}
			}
			return damageResult;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002FF4 File Offset: 0x000011F4
		private void KnockbackFlyer(DamageInfo dinfo, Pawn pawn)
		{
			bool spawned = pawn.Spawned;
			if (spawned)
			{
				Thing instigator = dinfo.Instigator;
				DamageKnockbackDef damageKnockbackDef = this.def as DamageKnockbackDef;
				bool flag = damageKnockbackDef.useExplosionCenter || (instigator != null && instigator.Spawned && instigator.Map == pawn.Map);
				if (flag)
				{
					Vector3 vector = damageKnockbackDef.useExplosionCenter ? Harmony_PawnKnockback.SA_Exp.DrawPos : instigator.DrawPos;
					float num = Rand.Value * (damageKnockbackDef.knockbackRadiusMax - damageKnockbackDef.knockbackRadiusMin) + damageKnockbackDef.knockbackRadiusMin;
					Vector3 vector2 = damageKnockbackDef.pullInstead ? (vector - pawn.DrawPos) : (pawn.DrawPos - vector);
					Map map = pawn.Map;
					Vector3 vector3 = pawn.DrawPos;
					bool flag2 = damageKnockbackDef.pullInstead && damageKnockbackDef.pullToNearbySpotInstead;
					if (flag2)
					{
						IntVec3 intVec = damageKnockbackDef.useExplosionCenter ? Harmony_PawnKnockback.SA_Exp.Position : instigator.Position;
						IntVec3 intVec2 = intVec;
						intVec2.x++;
						bool flag3 = intVec2.InBounds(map) && !intVec2.Impassable(map) && Vector3.Distance(intVec2.ToVector3Shifted(), vector3) < Vector3.Distance(intVec.ToVector3Shifted(), vector3);
						if (flag3)
						{
							intVec = intVec2;
						}
						intVec2 = intVec;
						intVec2.x--;
						bool flag4 = intVec2.InBounds(map) && !intVec2.Impassable(map) && Vector3.Distance(intVec2.ToVector3Shifted(), vector3) < Vector3.Distance(intVec.ToVector3Shifted(), vector3);
						if (flag4)
						{
							intVec = intVec2;
						}
						intVec2 = intVec;
						intVec2.z++;
						bool flag5 = intVec2.InBounds(map) && !intVec2.Impassable(map) && Vector3.Distance(intVec2.ToVector3Shifted(), vector3) < Vector3.Distance(intVec.ToVector3Shifted(), vector3);
						if (flag5)
						{
							intVec = intVec2;
						}
						intVec2 = intVec;
						intVec2.z--;
						bool flag6 = intVec2.InBounds(map) && !intVec2.Impassable(map) && Vector3.Distance(intVec2.ToVector3Shifted(), vector3) < Vector3.Distance(intVec.ToVector3Shifted(), vector3);
						if (flag6)
						{
							intVec = intVec2;
						}
						vector3 = intVec.ToVector3Shifted();
					}
					else
					{
						vector2.y = 0f;
						vector2 = vector2.normalized;
						bool flag7 = vector2.x == 0f && vector2.z == 0f;
						if (flag7)
						{
							return;
						}
						Vector3 vector4 = vector2;
						Vector3 drawPos = pawn.DrawPos;
						int num2 = 0;
						while (Vector3.Distance(drawPos, vector3) < num - (float)num2)
						{
							float num3 = Mathf.Abs(vector4.x);
							float num4 = Mathf.Abs(vector4.y);
							float num5 = Mathf.Abs(vector4.z);
							while (num3 >= 1f || num4 >= 1f || num5 >= 1f)
							{
								bool flag8 = false;
								bool flag9 = num3 >= 1f && num3 >= num4 && num3 >= num5;
								if (flag9)
								{
									float x = vector3.x;
									vector3.x += (float)((int)Mathf.Sign(vector2.x));
									vector4.x -= Mathf.Sign(vector2.x);
									bool flag10 = vector3.ToIntVec3().InBounds(map) && !vector3.ToIntVec3().Impassable(map);
									if (flag10)
									{
										flag8 = true;
									}
									else
									{
										vector3.x = x;
										num2++;
									}
								}
								else
								{
									bool flag11 = num4 >= 1f && num4 >= num3 && num4 >= num5;
									if (flag11)
									{
										float y = vector3.y;
										vector3.y += (float)((int)Mathf.Sign(vector2.y));
										vector4.y -= Mathf.Sign(vector2.y);
										bool flag12 = vector3.ToIntVec3().InBounds(map) && !vector3.ToIntVec3().Impassable(map);
										if (flag12)
										{
											flag8 = true;
										}
										else
										{
											vector3.y = y;
											num2++;
										}
									}
									else
									{
										bool flag13 = num5 >= 1f && num5 >= num4 && num5 >= num3;
										if (flag13)
										{
											float z = vector3.z;
											vector3.z += (float)((int)Mathf.Sign(vector2.z));
											vector4.z -= Mathf.Sign(vector2.z);
											bool flag14 = vector3.ToIntVec3().InBounds(map) && !vector3.ToIntVec3().Impassable(map);
											if (flag14)
											{
												flag8 = true;
											}
											else
											{
												vector3.z = z;
												num2++;
											}
										}
									}
								}
								num3 = Mathf.Abs(vector4.x);
								num4 = Mathf.Abs(vector4.y);
								num5 = Mathf.Abs(vector4.z);
								bool flag15 = !flag8;
								if (flag15)
								{
									break;
								}
							}
							vector4 += vector2;
						}
					}
					PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(damageKnockbackDef.flyerToApply, pawn, vector3.ToIntVec3(), null, null, false, null, null, default(LocalTargetInfo));
					bool flag16 = pawnFlyer != null;
					if (flag16)
					{
						PawnKnockback pawnKnockback = pawnFlyer as PawnKnockback;
						bool flag17 = pawnKnockback != null;
						if (flag17)
						{
							pawnKnockback.speedMultiplication = damageKnockbackDef.knockbackSpeedMultiplier;
							pawnKnockback.heightMultiplication = damageKnockbackDef.knockbackHeightMultiplier;
							pawnKnockback.damageDefRef = damageKnockbackDef;
							pawnKnockback.instigator = (damageKnockbackDef.useExplosionCenter ? null : instigator);
							pawnKnockback.centerIfNoinstigator = (damageKnockbackDef.useExplosionCenter ? Harmony_PawnKnockback.SA_Exp.Position.ToVector3() : Vector3.zero);
							pawnKnockback.instigatorEquipmentDef = dinfo.Weapon;
							pawnKnockback.adjustFlightTimeMax();
						}
						GenSpawn.Spawn(pawnFlyer, vector3.ToIntVec3(), map, WipeMode.Vanish);
					}
				}
			}
		}
	}
}
