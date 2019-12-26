using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class HealthPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower} max life",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 5f,
					maxMagnitude: 15f,
					rollChance: 0.333f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			player.statLifeMax2 += (int)Properties.RoundedPower;
		}
	}
}
