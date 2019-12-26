using System;
using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class GlobalCritPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"{Properties.RoundedPower}% crit chance",
				Color = Color.LimeGreen
			}
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
			ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player);

			player.magicCrit += (int)Properties.RoundedPower;
			player.meleeCrit += (int)Properties.RoundedPower;
			// @TODO Minion crit??
			player.rangedCrit += (int)Properties.RoundedPower;
			player.thrownCrit += (int)Properties.RoundedPower;

			tp.symphonicCrit += (int)Properties.RoundedPower;
			tp.radiantCrit += (int)Properties.RoundedPower;
			tp.radiantHealCrit += (int)Properties.RoundedPower;
		}
	}
}
