using Loot.Modifiers;
using Loot.Modifiers.EquipModifiers.Offensive;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Loot
{
	// Technically this entire class isn't needed since we have projectile handling elsewhere and can just rewrite the code in that place but i'm lazy
	public class ModifierProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;

		public override GlobalProjectile Clone()
		{
			var clone = (ModifierProjectile)base.MemberwiseClone();
			clone.SNAPSHOT_DebuffChances = new List<DebuffTrigger>(SNAPSHOT_DebuffChances);
			return clone;
		}

		public ModifierProjectile Info(Projectile projectile, Mod mod = null)
			=> projectile.GetGlobalProjectile<ModifierProjectile>();

		public bool NeedsClear;
		public bool FirstTick;
		public float SNAPSHOT_HealthyFoesMulti = 1f;
		public float SNAPSHOT_CritMulti = 1f;
		public IList<DebuffTrigger> SNAPSHOT_DebuffChances = new List<DebuffTrigger>();

		private void AttemptDebuff(Projectile projectile, Player target)
		{
			foreach (var x in Info(projectile).SNAPSHOT_DebuffChances)
			{
				if (Main.rand.NextFloat() < x.InflictionChance)
				{
					target.AddBuff(x.BuffType, x.BuffTime);
				}
			}
		}

		private void AttemptDebuff(Projectile projectile, NPC target)
		{
			foreach (var x in Info(projectile).SNAPSHOT_DebuffChances)
			{
				if (Main.rand.NextFloat() < x.InflictionChance)
				{
					target.AddBuff(x.BuffType, x.BuffTime);
				}
			}
		}

		private void HealthyBonus(Projectile projectile, ref int damage, NPC target)
		{
			if (target.life == target.lifeMax)
			{
				damage = (int)Math.Ceiling(damage * Info(projectile).SNAPSHOT_HealthyFoesMulti);
			}
		}

		private void HealthyBonus(Projectile projectile, ref int damage, Player target)
		{
			if (target.statLife == target.statLifeMax2)
			{
				damage = (int)Math.Ceiling(damage * Info(projectile).SNAPSHOT_HealthyFoesMulti);
			}
		}

		private void CritBonus(Projectile projectile, ref int damage, bool crit)
		{
			if (crit)
			{
				damage = (int)Math.Ceiling(damage * Info(projectile).SNAPSHOT_CritMulti);
			}
		}

		public override bool PreAI(Projectile projectile)
		{
			var mproj = Info(projectile);
			// On first tick, copy over player stats
			if (!mproj.FirstTick)
			{
				mproj.FirstTick = true;
				
				// Snapshot current player values
				if (projectile.owner != 255 && projectile.friendly && projectile.owner == Main.myPlayer)
				{
					var mplr = Main.LocalPlayer.GetModPlayer<ModifierPlayer>();
					mproj.SNAPSHOT_DebuffChances = new List<DebuffTrigger>(mplr.GetEffect<WeaponDebuffEffect>().DebuffChances);
					mproj.SNAPSHOT_HealthyFoesMulti = mplr.GetEffect<HealthyFoesEffect>().Multiplier;
					if (!projectile.minion) // minions do not crit
					{
						mproj.SNAPSHOT_CritMulti = mplr.GetEffect<CritDamagePlusEffect>().Multiplier;
					}
				}
				else if (projectile.owner == 255)
				{
					// TODO snapshot npc values. possible?
				}
			}
			if (NeedsClear)
			{
				mproj.SNAPSHOT_HealthyFoesMulti = 1f;
				mproj.SNAPSHOT_DebuffChances.Clear();
			}
			return base.PreAI(projectile);
		}

		// TODO I hate the copy pasta

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			HealthyBonus(projectile, ref damage, target);
			CritBonus(projectile, ref damage, crit);
			base.ModifyHitNPC(projectile, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		// shouldn't run
		public override void ModifyHitPlayer(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			HealthyBonus(projectile, ref damage, target);
			CritBonus(projectile, ref damage, crit);
			base.ModifyHitPlayer(projectile, target, ref damage, ref crit);
		}

		public override void ModifyHitPvp(Projectile projectile, Player target, ref int damage, ref bool crit)
		{
			HealthyBonus(projectile, ref damage, target);
			CritBonus(projectile, ref damage, crit);
			base.ModifyHitPvp(projectile, target, ref damage, ref crit);
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
		{
			AttemptDebuff(projectile, target);
			base.OnHitNPC(projectile, target, damage, knockback, crit);
		}

		public override void OnHitPlayer(Projectile projectile, Player target, int damage, bool crit)
		{
			AttemptDebuff(projectile, target);
			base.OnHitPlayer(projectile, target, damage, crit);
		}

		public override void OnHitPvp(Projectile projectile, Player target, int damage, bool crit)
		{
			AttemptDebuff(projectile, target);
			base.OnHitPvp(projectile, target, damage, crit);
		}
	}
}
