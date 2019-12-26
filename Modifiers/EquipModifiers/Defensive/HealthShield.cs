using Loot.Core;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class HealthShield : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower} regenerating health shield",
				Color = Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 5f,
					maxMagnitude: 15f,
					rollChance: 0.5f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player);
			tp.shieldHealth += (int)Properties.RoundedPower;
		}
	}
}
