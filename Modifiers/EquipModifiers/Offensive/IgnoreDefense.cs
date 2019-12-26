using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class IgnoreDefenseEffect : ModifierEffect
	{
		public float proc = 0f;
		public static float MAX_PROC = 0.05f;

		public override void ResetEffects(ModifierPlayer player)
		{
			proc = 0f;
		}

		[AutoDelegation("OnModifyHitNPC")]
		[DelegationPrioritization(DelegationPrioritization.Late, 997)]
		public void ModifyHitNPC(ModifierPlayer player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			float rand = Main.rand.NextFloat();

			if (rand <= Math.Min(proc, MAX_PROC))
			{
				damage += target.defense / 2;
			}
		}

		[AutoDelegation("OnModifyHitNPCWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 997)]
		private void ModifyHitNPCWithProj(ModifierPlayer player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			float rand = Main.rand.NextFloat();

			if (rand <= Math.Min(proc, MAX_PROC))
			{
				damage += target.defense / 2;
			}
		}

		[AutoDelegation("OnModifyHitPvp")]
		[DelegationPrioritization(DelegationPrioritization.Late, 997)]
		private void ModifyHitPvp(ModifierPlayer player, Item item, Player target, ref int damage, ref bool crit)
		{
			float rand = Main.rand.NextFloat();

			if (rand <= Math.Min(proc, MAX_PROC))
			{
				damage += target.statDefense / 2;
			}
		}

		[AutoDelegation("OnModifyHitPvpWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 997)]
		private void ModifyHitPvpWithProj(ModifierPlayer player, Projectile proj, Player target, ref int damage, ref bool crit)
		{
			float rand = Main.rand.NextFloat();

			if (rand <= Math.Min(proc, MAX_PROC))
			{
				damage += target.statDefense / 2;
			}
		}
	}

	[UsesEffect(typeof(IgnoreDefenseEffect))]
	public class IgnoreDefense : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Math.Round(Properties.RoundedPower / 10, 2)}% chance to ignore enemy defense" +
					   $"{(Main.LocalPlayer.GetModPlayer<ModifierPlayer>().GetEffect<IgnoreDefenseEffect>().proc >= IgnoreDefenseEffect.MAX_PROC ? $" (cap reached: {IgnoreDefenseEffect.MAX_PROC * 100f}%)" : "")}",
				Color = Color.LimeGreen
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 5f,
					maxMagnitude: 15f,
					rollChance: 0.5f,
					uniqueRoll: true
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<IgnoreDefenseEffect>().proc += Properties.RoundedPower / 1000f;
		}
	}
}
