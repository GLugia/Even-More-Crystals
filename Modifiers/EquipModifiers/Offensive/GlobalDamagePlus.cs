using System;
using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class GlobalDamagePlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% damage",
				Color = Color.LimeGreen
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 2f,
					maxMagnitude: 10f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player);

			player.magicDamage += Properties.RoundedPower / 100f;
			player.meleeDamage += Properties.RoundedPower / 100f;
			player.minionDamage += Properties.RoundedPower / 100f;
			player.rangedDamage += Properties.RoundedPower / 100f;
			player.thrownDamage += Properties.RoundedPower / 100f;

			tp.symphonicDamage += (Properties.RoundedPower * 2) / 100f;
			tp.radiantBoost += (Properties.RoundedPower * 2) / 100f;
		}
	}
}
