using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PawnKnockback
{
	// Token: 0x0200000A RID: 10
	public class PawnEmptyFlyer : Thing
	{
		// Token: 0x06000025 RID: 37 RVA: 0x000036A8 File Offset: 0x000018A8
		public PawnEmptyFlyer()
		{
			AnimationCurve animationCurve = new AnimationCurve();
			animationCurve.AddKey(0f, 0f);
			animationCurve.AddKey(0.1f, 0.15f);
			animationCurve.AddKey(1f, 1f);
			PawnKnockback.FlightSpeed = new Func<float, float>(animationCurve.Evaluate);
		}

		// Token: 0x06000026 RID: 38 RVA: 0x0000372C File Offset: 0x0000192C
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			bool flag = !respawningAfterLoad;
			if (flag)
			{
				float num = Mathf.Max(this.flightDistance, 1f) / this.def.pawnFlyer.flightSpeed;
				num = Mathf.Max(num, this.def.pawnFlyer.flightDurationMin);
				this.ticksFlightTime = num.SecondsToTicks();
				this.ticksFlying = 0;
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x0000379C File Offset: 0x0000199C
		private void CheckDestination()
		{
			bool flag = !JumpUtility.ValidJumpTarget(base.Map, base.Position);
			if (flag)
			{
				int num = GenRadial.NumCellsInRadius(3.9f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
					bool flag2 = JumpUtility.ValidJumpTarget(base.Map, intVec);
					if (flag2)
					{
						base.Position = intVec;
						break;
					}
				}
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003818 File Offset: 0x00001A18
		public static PawnEmptyFlyer MakeFlyer(ThingDef flyingDef, Vector3 startCell, IntVec3 destCell)
		{
			PawnEmptyFlyer pawnEmptyFlyer = (PawnEmptyFlyer)ThingMaker.MakeThing(flyingDef, null);
			pawnEmptyFlyer.startVec = startCell;
			pawnEmptyFlyer.endVec = destCell.ToVector3Shifted();
			pawnEmptyFlyer.flightDistance = Vector3.Distance(startCell, destCell.ToVector3());
			return pawnEmptyFlyer;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003860 File Offset: 0x00001A60
		protected virtual bool ValidateFlyer()
		{
			return true;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003874 File Offset: 0x00001A74
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<Vector3>(ref this.endVec, "endVec", default(Vector3), false);
			Scribe_Values.Look<Vector3>(ref this.startVec, "startVec", default(Vector3), false);
			Scribe_Values.Look<float>(ref this.flightDistance, "flightDistance", 0f, false);
			Scribe_Values.Look<int>(ref this.ticksFlightTime, "ticksFlightTime", 0, false);
			Scribe_Values.Look<int>(ref this.ticksFlying, "ticksFlying", 0, false);
			Scribe_Values.Look<float>(ref this.speedMultiplication, "PK_SpeedMult", 0f, false);
			Scribe_Values.Look<float>(ref this.heightMultiplication, "PK_HeightMult", 0f, false);
			Scribe_Defs.Look<DamageKnockbackDef>(ref this.damageDefRef, "PK_DamageRef");
			Scribe_Defs.Look<ThingDef>(ref this.instigatorEquipmentDef, "PK_EquipmentRef");
			Scribe_Defs.Look<ThingDef>(ref this.bulletDef, "PK_BulletRef");
			Scribe_References.Look<Thing>(ref this.instigator, "PK_Instigator", false);
			Scribe_Values.Look<Vector3>(ref this.centerIfNoinstigator, "PK_centerIfNoinstigator", default(Vector3), false);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x0000398A File Offset: 0x00001B8A
		public void adjustFlightTimeMax()
		{
			this.ticksFlightTime = (int)((float)this.ticksFlightTime / this.speedMultiplication);
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002C RID: 44 RVA: 0x000039A4 File Offset: 0x00001BA4
		public override Vector3 DrawPos
		{
			get
			{
				this.RecomputePosition();
				return this.effectivePos;
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000039C4 File Offset: 0x00001BC4
		private void RecomputePosition()
		{
			bool flag = this.positionLastComputedTick != this.ticksFlying;
			if (flag)
			{
				this.positionLastComputedTick = this.ticksFlying;
				float arg = (float)this.ticksFlying / (float)this.ticksFlightTime;
				float num = PawnKnockback.FlightSpeed(arg);
				this.effectiveHeight = PawnKnockback.FlightCurveHeight(num) * this.heightMultiplication;
				this.groundPos = Vector3.Lerp(this.startVec, this.endVec, num);
				Vector3 a = new Vector3(0f, 0f, 2f);
				Vector3 b = Altitudes.AltIncVect * this.effectiveHeight;
				Vector3 b2 = a * this.effectiveHeight;
				this.effectivePos = this.groundPos + b + b2;
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00003A94 File Offset: 0x00001C94
		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			this.RecomputePosition();
			Vector3 vector = (this.instigator == null) ? this.centerIfNoinstigator : this.instigator.DrawPos;
			DamageKnockbackDef damageKnockbackDef = this.damageDefRef;
			bool flag = damageKnockbackDef != null && damageKnockbackDef.pullInstead && damageKnockbackDef.useHarpoonGraphic;
			if (flag)
			{
				float num = (float)this.ticksFlying / (float)this.ticksFlightTime;
				float num2 = 0f;
				float num3 = 0f;
				bool flag2 = num < 0.1f;
				if (flag2)
				{
					num2 = (0.1f - num) / 0.1f;
				}
				bool flag3 = num > 0.8f;
				if (flag3)
				{
					num3 = (num - 0.8f) / 0.19999999f;
				}
				Vector3 b = vector;
				Vector3 vector2 = this.effectivePos;
				vector2 = vector2 * (1f - num3) + vector2 * num3;
				float y = vector2.y;
				vector2.y = 0f;
				b.y = 0f;
				float num4 = Vector3.Distance(vector2, b);
				float num5 = damageKnockbackDef.harpoonUseHarpoonUnitInstead ? (num4 / damageKnockbackDef.harpoonUnit) : ((float)damageKnockbackDef.harpoonSegements);
				float num6 = Mathf.Cos((float)(Find.TickManager.TicksAbs - this.thingIDNumber) * damageKnockbackDef.harpoonSineSpeed) * Mathf.Sqrt(num4) / 5f;
				Quaternion quaternion = Quaternion.AngleAxis(-Mathf.Atan2((vector2 - b).z, (vector2 - b).x) * 180f / 3.1415927f, Vector3.up);
				float num7 = 1f - (num5 - Mathf.Floor(num5));
				bool flag4 = num7 == 1f;
				if (flag4)
				{
					num7 = 0f;
				}
				Vector3 b2 = Bullet_Harpoon.vec3MLC(-num7 / num5, num4, 5, num6 * damageKnockbackDef.harpoonSineZAmp * num2);
				for (float num8 = 1f - num7; num8 <= num5; num8 += 1f)
				{
					Vector3 vector3 = Bullet_Harpoon.vec3MLC(num8 / num5, num4, 5, num6 * damageKnockbackDef.harpoonSineZAmp * num2);
					float length = Vector3.Distance(vector3, b2);
					Vector3 point = (vector3 + b2) / 2f;
					point.y = 0f;
					Quaternion rhs = Quaternion.AngleAxis(-Mathf.Atan2((vector3 - b2).z, (vector3 - b2).x) * 180f / 3.1415927f, Vector3.up);
					Graphics.DrawMesh(Bullet_Harpoon.meshOfSize(length), quaternion * point + b + new Vector3(0f, y, 0f), quaternion * rhs, damageKnockbackDef.graphicDataHarpoonRope.Graphic.MatSingle, 0);
					b2 = vector3;
				}
			}
			Graphics.DrawMesh(MeshPool.GridPlane(this.bulletDef.graphicData.drawSize), this.effectivePos, Quaternion.LookRotation((this.effectivePos - vector).Yto0()), this.bulletDef.DrawMatSingle, 0);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003DA8 File Offset: 0x00001FA8
		public override void Tick()
		{
			bool flag = this.flightEffecter == null && this.flightEffecterDef_Anty != null;
			if (flag)
			{
				this.flightEffecter = this.flightEffecterDef_Anty.Spawn();
				this.flightEffecter.Trigger(this, TargetInfo.Invalid, -1);
			}
			else
			{
				Effecter effecter = this.flightEffecter;
				bool flag2 = effecter != null;
				if (flag2)
				{
					effecter.EffectTick(this, TargetInfo.Invalid);
				}
			}
			bool flag3 = this.ticksFlying >= this.ticksFlightTime;
			if (flag3)
			{
				this.Destroy(DestroyMode.Vanish);
			}
			else
			{
				bool flag4 = this.ticksFlying % 5 == 0;
				if (flag4)
				{
					this.CheckDestination();
				}
			}
			this.ticksFlying++;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00003E6C File Offset: 0x0000206C
		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			Effecter effecter = this.flightEffecter;
			bool flag = effecter != null;
			if (flag)
			{
				effecter.Cleanup();
			}
			base.Destroy(mode);
		}

		// Token: 0x0400002D RID: 45
		public EffecterDef flightEffecterDef_Anty;

		// Token: 0x0400002E RID: 46
		public SoundDef soundLanding_Anty;

		// Token: 0x0400002F RID: 47
		private static readonly Func<float, float> FlightSpeed;

		// Token: 0x04000030 RID: 48
		private static readonly Func<float, float> FlightCurveHeight = new Func<float, float>(GenMath.InverseParabola);

		// Token: 0x04000031 RID: 49
		public Thing instigator;

		// Token: 0x04000032 RID: 50
		public Vector3 centerIfNoinstigator;

		// Token: 0x04000033 RID: 51
		public ThingDef bulletDef;

		// Token: 0x04000034 RID: 52
		public ThingDef instigatorEquipmentDef;

		// Token: 0x04000035 RID: 53
		public DamageKnockbackDef damageDefRef;

		// Token: 0x04000036 RID: 54
		public float speedMultiplication = 1f;

		// Token: 0x04000037 RID: 55
		public float heightMultiplication = 1f;

		// Token: 0x04000038 RID: 56
		public Vector3 vectorPosition;

		// Token: 0x04000039 RID: 57
		private Material cachedShadowMaterial;

		// Token: 0x0400003A RID: 58
		private Effecter flightEffecter;

		// Token: 0x0400003B RID: 59
		private int positionLastComputedTick = -1;

		// Token: 0x0400003C RID: 60
		private Vector3 groundPos;

		// Token: 0x0400003D RID: 61
		private Vector3 effectivePos;

		// Token: 0x0400003E RID: 62
		private float effectiveHeight;

		// Token: 0x0400003F RID: 63
		protected Vector3 endVec;

		// Token: 0x04000040 RID: 64
		protected Vector3 startVec;

		// Token: 0x04000041 RID: 65
		private float flightDistance;

		// Token: 0x04000042 RID: 66
		protected int ticksFlightTime = 120;

		// Token: 0x04000043 RID: 67
		protected int ticksFlying;
	}
}
