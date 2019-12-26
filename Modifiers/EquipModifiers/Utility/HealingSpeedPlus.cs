using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Utility
{
	public class HealingSpeedPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% cleric healing speed",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 3f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (ModLoader.GetMod("ThoriumMod") != null)
			{
				ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player);
				tp.healingSpeed += Properties.RoundedPower / 100f;
			}
			else
			{
				Loot.Logger.Error("Thorium is not loaded");
			}
		}
	}
}
