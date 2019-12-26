using Loot.Core;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Loot.Modifiers.WeaponModifiers
{
	public class VelocityDamage : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					Text = $"Added damage based on player's velocity (multiplier: {Math.Round(Properties.RoundedPower / 2, 1)}x)",
					Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					maxMagnitude: 3f,
					roundPrecision: 1
				);
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float multi, ref float flat)
		{
			base.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);
			float magnitude = Properties.RoundedPower * player.velocity.Length() / 4;
			add += (magnitude / 100);
		}
	}
}
