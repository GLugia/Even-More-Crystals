using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Utility
{
	public class MoveSpeedPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% movement speed",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					basePower: 1 / 5f,
					minMagnitude: 1f,
					maxMagnitude: 45f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			player.moveSpeed += Properties.RoundedPower / 100;
			player.maxRunSpeed *= 1 + Properties.RoundedPower / 100;
		}
	}
}
