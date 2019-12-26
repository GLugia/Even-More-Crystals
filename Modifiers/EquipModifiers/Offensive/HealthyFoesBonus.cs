using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class HealthyFoesEffect : ModifierEffect
	{
		public float Multiplier;

		public override void OnInitialize(ModifierPlayer player)
		{
			Multiplier = 1f;
		}

		public override void ResetEffects(ModifierPlayer player)
		{
			Multiplier = 1f;
		}

		// @todo must be prioritized before crit

		[AutoDelegation("OnModifyHitNPC")]
		[DelegationPrioritization(DelegationPrioritization.Late, 998)]
		public void ModifyHitNPC(ModifierPlayer player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (target.life == target.lifeMax)
			{
				HealthyFoes(ref damage);
			}
		}

		[AutoDelegation("OnModifyHitNPCWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 998)]
		private void ModifyHitNPCWithProj(ModifierPlayer player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (target.life == target.lifeMax)
			{
				HealthyFoes(ref damage);
			}
		}

		[AutoDelegation("OnModifyHitPvp")]
		[DelegationPrioritization(DelegationPrioritization.Late, 998)]
		private void ModifyHitPvp(ModifierPlayer player, Item item, Player target, ref int damage, ref bool crit)
		{
			if (target.statLife == target.statLifeMax2)
			{
				HealthyFoes(ref damage);
			}
		}

		[AutoDelegation("OnModifyHitPvpWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 998)]
		private void ModifyHitPvpWithProj(ModifierPlayer player, Projectile proj, Player target, ref int damage, ref bool crit)
		{
			if (target.statLife == target.statLifeMax2)
			{
				HealthyFoes(ref damage);
			}
		}

		private void HealthyFoes(ref int damage)
		{
			damage = (int)(Math.Ceiling(damage * Multiplier));
		}
	}

	[UsesEffect(typeof(HealthyFoesEffect))]
	public class HealthyFoesBonus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% damage vs max life foes",
				Color = Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 2f,
					maxMagnitude: 10f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<HealthyFoesEffect>().Multiplier += Properties.RoundedPower / 100f;
		}
	}
}
