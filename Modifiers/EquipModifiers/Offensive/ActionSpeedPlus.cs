using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using ThoriumMod;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class ActionSpeedEffect : ModifierEffect
	{
		public float ActionSpeed = 0f;

		public override void ResetEffects(ModifierPlayer player)
		{
			ActionSpeed = 0f;
		}

		[AutoDelegation("OnPostUpdateEquips")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		private void ActionSpeedInc(ModifierPlayer player)
		{
			ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player.player);

			tp.healingSpeed = (float)Math.Ceiling(tp.healingSpeed * (1.0 + ActionSpeed));
			player.player.moveSpeed = (float)Math.Ceiling(player.player.moveSpeed * (1 + ActionSpeed));
			player.player.jumpSpeedBoost = (float)Math.Ceiling(player.player.jumpSpeedBoost * (1 + ActionSpeed));
			player.player.maxRunSpeed = (float)Math.Ceiling(player.player.maxRunSpeed * (1 + ActionSpeed));
		}
	}

	[UsesEffect(typeof(ActionSpeedEffect))]
	public class ActionSpeedPlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% bonus action speed",
				Color = Color.LimeGreen,
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 3f,
					rollChance: 0.333f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<ActionSpeedEffect>().ActionSpeed += Properties.RoundedPower / 100f;
		}

		public override void Apply(Item item)
		{
			base.Apply(item);

			item.useTime = (int)(item.useTime * (1 - Properties.RoundedPower / 100f));
			item.useAnimation = (int)(item.useAnimation * (1 - Properties.RoundedPower / 100f));

			// Don't go below the minimum
			if (item.useTime < 2)
			{
				item.useTime = 2;
			}

			if (item.useAnimation < 2)
			{
				item.useAnimation = 2;
			}

			item.shootSpeed *= ((Properties.RoundedPower / 100f) + 1f);
		}
	}
}
