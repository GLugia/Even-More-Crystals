using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Utility
{
	public class HealingPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower} cleric bonus healing",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 2f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if(ModLoader.GetMod("ThoriumMod") != null)
			{
				ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player);
				tp.healBonus += (int)Properties.RoundedPower;
			}
			else
			{
				Loot.Logger.Error("Thorium is not loaded");
			}
		}
	}
}
