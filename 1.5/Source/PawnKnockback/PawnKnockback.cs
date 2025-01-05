using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PawnKnockback
{
	// Token: 0x02000003 RID: 3
	public class PawnKnockback : PawnFlyer
	{
		// Token: 0x06000004 RID: 4 RVA: 0x000020B1 File Offset: 0x000002B1
		public void adjustFlightTimeMax()
		{
			this.ticksFlightTime = (int)((float)this.ticksFlightTime / this.speedMultiplication);
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020CC File Offset: 0x000002CC
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.speedMultiplication, "PK_SpeedMult", 0f, false);
			Scribe_Values.Look<float>(ref this.heightMultiplication, "PK_HeightMult", 0f, false);
			Scribe_Defs.Look<DamageKnockbackDef>(ref this.damageDefRef, "PK_DamageRef");
			Scribe_Defs.Look<ThingDef>(ref this.instigatorEquipmentDef, "PK_EquipmentRef");
			Scribe_References.Look<Thing>(ref this.instigator, "PK_Instigator", false);
			Scribe_Values.Look<Vector3>(ref this.centerIfNoinstigator, "PK_centerIfNoinstigator", default(Vector3), false);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000006 RID: 6 RVA: 0x00002160 File Offset: 0x00000360
		private Material ShadowMaterial
		{
			get
			{
				bool flag = this.cachedShadowMaterial == null && !this.shadowTexPath.NullOrEmpty();
				if (flag)
				{
					this.cachedShadowMaterial = MaterialPool.MatFrom(this.shadowTexPath, ShaderDatabase.Transparent);
				}
				return this.cachedShadowMaterial;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000021B4 File Offset: 0x000003B4
		static PawnKnockback()
		{
			AnimationCurve animationCurve = new AnimationCurve();
			animationCurve.AddKey(0f, 0f);
			animationCurve.AddKey(0.1f, 0.15f);
			animationCurve.AddKey(1f, 1f);
			PawnKnockback.FlightSpeed = new Func<float, float>(animationCurve.Evaluate);
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002220 File Offset: 0x00000420
		public override Vector3 DrawPos
		{
			get
			{
				this.RecomputePosition();
				return this.effectivePos;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002240 File Offset: 0x00000440
		protected virtual bool ValidateFlyer()
		{
			return true;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002254 File Offset: 0x00000454
		private void RecomputePosition()
		{
			bool flag = this.positionLastComputedTick != this.ticksFlying && base.FlyingPawn != null;
			if (flag)
			{
				this.positionLastComputedTick = this.ticksFlying;
				float arg = (float)this.ticksFlying / (float)this.ticksFlightTime;
				float num = PawnKnockback.FlightSpeed(arg);
				this.effectiveHeight = PawnKnockback.FlightCurveHeight(num) * this.heightMultiplication;
				this.groundPos = Vector3.Lerp(this.startVec, base.DestinationPos, num);
				Vector3 a = new Vector3(0f, 0f, 2f);
				Vector3 b = Altitudes.AltIncVect * this.effectiveHeight;
				Vector3 b2 = a * this.effectiveHeight;
				this.effectivePos = this.groundPos + b + b2;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002330 File Offset: 0x00000530
		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			bool flag = base.FlyingPawn != null;
			if (flag)
			{
				this.RecomputePosition();
				this.DrawShadow(this.groundPos, this.effectiveHeight);
				Thing thing = this.instigator;
				Vector3 b = (thing == null) ? this.centerIfNoinstigator : thing.DrawPos;
				DamageKnockbackDef damageKnockbackDef = this.damageDefRef;
				bool flag2 = damageKnockbackDef != null && damageKnockbackDef.pullInstead && damageKnockbackDef.useHarpoonGraphic;
				if (flag2)
				{
					float num = (float)this.ticksFlying / (float)this.ticksFlightTime;
					float num2 = 0f;
					float num3 = 0f;
					bool flag3 = num < 0.1f;
					if (flag3)
					{
						num2 = (0.1f - num) / 0.1f;
					}
					bool flag4 = num > 0.8f;
					if (flag4)
					{
						num3 = (num - 0.8f) / 0.19999999f;
					}
					Vector3 vector = this.effectivePos;
					vector = vector * (1f - num3) + vector * num3;
					float y = vector.y;
					vector.y = 0f;
					b.y = 0f;
					float num4 = Vector3.Distance(vector, b);
					float num5 = damageKnockbackDef.harpoonUseHarpoonUnitInstead ? (num4 / damageKnockbackDef.harpoonUnit) : ((float)damageKnockbackDef.harpoonSegements);
					float num6 = Mathf.Cos((float)(Find.TickManager.TicksAbs - this.thingIDNumber) * damageKnockbackDef.harpoonSineSpeed) * Mathf.Sqrt(num4) / 5f;
					Quaternion quaternion = Quaternion.AngleAxis(-Mathf.Atan2((vector - b).z, (vector - b).x) * 180f / 3.1415927f, Vector3.up);
					float num7 = 1f - (num5 - Mathf.Floor(num5));
					bool flag5 = num7 == 1f;
					if (flag5)
					{
						num7 = 0f;
					}
					Vector3 b2 = Bullet_Harpoon.vec3MLC(-num7 / num5, num4, 5, num6 * damageKnockbackDef.harpoonSineZAmp * num2);
					for (float num8 = 1f - num7; num8 <= num5; num8 += 1f)
					{
						Vector3 vector2 = Bullet_Harpoon.vec3MLC(num8 / num5, num4, 5, num6 * damageKnockbackDef.harpoonSineZAmp * num2);
						float length = Vector3.Distance(vector2, b2);
						Vector3 point = (vector2 + b2) / 2f;
						point.y = 0f;
						Quaternion rhs = Quaternion.AngleAxis(-Mathf.Atan2((vector2 - b2).z, (vector2 - b2).x) * 180f / 3.1415927f, Vector3.up);
						Graphics.DrawMesh(Bullet_Harpoon.meshOfSize(length), quaternion * point + b + new Vector3(0f, y, 0f), quaternion * rhs, damageKnockbackDef.graphicDataHarpoonRope.Graphic.MatSingle, 0);
						b2 = vector2;
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002624 File Offset: 0x00000824
		private void DrawShadow(Vector3 drawLoc, float height)
		{
			Material shadowMaterial = this.ShadowMaterial;
			bool flag = !(shadowMaterial == null);
			if (flag)
			{
				float num = Mathf.Lerp(1f, 0.6f, height);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(drawLoc, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002690 File Offset: 0x00000890
		protected override void RespawnPawn()
		{
			this.LandingEffects();
			Pawn flyingPawn = base.FlyingPawn;
			base.RespawnPawn();
			bool flag = flyingPawn != null && this.damageDefRef != null && this.damageDefRef.landingDamage != null;
			if (flag)
			{
				DamageDef landingDamage = this.damageDefRef.landingDamage;
				flyingPawn.TakeDamage(new DamageInfo(landingDamage, this.damageDefRef.landingDamageAmount, 0f, 0f, this.instigator, null, this.instigatorEquipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, flyingPawn, true, true, QualityCategory.Normal, true));
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x0000271C File Offset: 0x0000091C
		public override void Tick()
		{
			bool flag = base.FlyingPawn == null;
			if (flag)
			{
				Thing thing;
				base.GetDirectlyHeldThings().TryDrop((base.GetDirectlyHeldThings() as ThingOwner<Thing>).InnerListForReading[0], base.Position, base.MapHeld, ThingPlaceMode.Direct, out thing, null, null, false);
				this.Destroy(DestroyMode.Vanish);
			}
			else
			{
				bool flag2 = this.flightEffecter == null && this.flightEffecterDef_Anty != null;
				if (flag2)
				{
					this.flightEffecter = this.flightEffecterDef_Anty.Spawn();
					this.flightEffecter.Trigger(this, TargetInfo.Invalid, -1);
				}
				else
				{
					Effecter effecter = this.flightEffecter;
					bool flag3 = effecter != null;
					if (flag3)
					{
						effecter.EffectTick(this, TargetInfo.Invalid);
					}
				}
				base.Tick();
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000027EC File Offset: 0x000009EC
		private void LandingEffects()
		{
			bool flag = this.soundLanding_Anty != null;
			if (flag)
			{
				this.soundLanding_Anty.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
			}
			FleckMaker.ThrowDustPuff(base.DestinationPos + Gen.RandomHorizontalVector(0.5f), base.Map, 2f);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002854 File Offset: 0x00000A54
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

		// Token: 0x04000004 RID: 4
		public EffecterDef flightEffecterDef_Anty;

		// Token: 0x04000005 RID: 5
		public SoundDef soundLanding_Anty;

		// Token: 0x04000006 RID: 6
		public static Func<float, float> FlightSpeed;

		// Token: 0x04000007 RID: 7
		public static Func<float, float> FlightCurveHeight = new Func<float, float>(GenMath.InverseParabola);

		// Token: 0x04000008 RID: 8
		public Thing instigator;

		// Token: 0x04000009 RID: 9
		public Vector3 centerIfNoinstigator;

		// Token: 0x0400000A RID: 10
		public ThingDef instigatorEquipmentDef;

		// Token: 0x0400000B RID: 11
		public DamageKnockbackDef damageDefRef;

		// Token: 0x0400000C RID: 12
		public float speedMultiplication = 1f;

		// Token: 0x0400000D RID: 13
		public float heightMultiplication = 1f;

		// Token: 0x0400000E RID: 14
		public Vector3 vectorPosition;

		// Token: 0x0400000F RID: 15
		private Material cachedShadowMaterial;

		// Token: 0x04000010 RID: 16
		private Effecter flightEffecter;

		// Token: 0x04000011 RID: 17
		private int positionLastComputedTick = -1;

		// Token: 0x04000012 RID: 18
		private Vector3 groundPos;

		// Token: 0x04000013 RID: 19
		private Vector3 effectivePos;

		// Token: 0x04000014 RID: 20
		private float effectiveHeight;

		// Token: 0x04000015 RID: 21
		[NoTranslate]
		private string shadowTexPath = "Things/Skyfaller/SkyfallerShadowCircle";
	}
}
