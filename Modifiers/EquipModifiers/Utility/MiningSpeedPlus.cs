using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Utility
{
	public class MiningSpeedPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% mining speed",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 7.5f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			player.pickSpeed -= (player.pickSpeed * Properties.RoundedPower / 100);
		}
	}
}
