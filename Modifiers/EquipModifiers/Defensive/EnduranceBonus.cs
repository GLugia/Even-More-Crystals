using Loot.Core;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class EnduranceBonus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"{Properties.RoundedPower}% bonus damage reduction",
				Color = Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 5f,
					rollChance: 0.333f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			player.endurance = (player.endurance + ((float)Math.Pow(Properties.RoundedPower, 1 - player.endurance) / 100));
		}
	}
}
