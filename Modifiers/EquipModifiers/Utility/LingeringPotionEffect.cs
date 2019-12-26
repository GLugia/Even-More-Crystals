using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot.Modifiers.EquipModifiers.Utility
{
	public class LingeringEffect : ModifierEffect
	{
		public float PotionHealEffect = 0f;
		public static float MAX_POTION_EFFECT = 1f;
		private float healsettime = 0f;
		private bool hasHealed = false;
		private bool canHeal = false;
		private int heal = 0;

		public override void ResetEffects(ModifierPlayer player)
		{
			PotionHealEffect = 0f;
			heal = 0;
		}

		[AutoDelegation("OnHealLife")]
		[DelegationPrioritization(DelegationPrioritization.Early, 100)]
		private void changeLifeFlasks(ModifierPlayer player, Item item, bool quickHeal, ref int healValue)
		{
			if (healValue > 0)
			{
				heal = healValue;
				// ((heal * (1 - %regen)) * (1 + ((1 - %regen) * 1.5))) / duration
				healValue = (int)Math.Ceiling((healValue * (1.0 - Math.Min(PotionHealEffect, MAX_POTION_EFFECT))) * (1 - (Math.Min(PotionHealEffect, MAX_POTION_EFFECT) * 1.5)));
			}
		}

		[AutoDelegation("OnPreUpdateBuffs")]
		[DelegationPrioritization(DelegationPrioritization.Early, 100)]
		private void checkBuffs(ModifierPlayer player)
		{
			// Activates the life regen effect below if the quickheal has been pressed while the player has
			//   an item that activates on quickheal (ie any healing potion in this case), the settime is older
			//   than the globaltime, the player doesn't have potion sickness, AND the player is actually able to heal from a potion
			if (!player.player.releaseQuickHeal
				&& healsettime < Main.GlobalTime
				&& player.player.QuickHeal_GetItemToUse() != null
				&& !hasHealed
				&& canHeal)
			{
				healsettime = Main.GlobalTime + 10f;
				//player.player.AddBuff(BuffID.PotionSickness, 60);
			}
		}

		[AutoDelegation("OnUpdateLifeRegen")]
		[DelegationPrioritization(DelegationPrioritization.Early, 100)]
		private void healPlayer(ModifierPlayer player)
		{
			if (healsettime >= Main.GlobalTime && player.player.HasBuff(BuffID.PotionSickness))
			{
				if (!hasHealed)
				{
					hasHealed = true;
				}
				// ((heal * (1 - %regen)) * (1 + (%regen * 1.5)))
				// ((heal * %regen) * (1 + (%regen * 1.5))) / duration
				player.player.lifeRegen += (int)Math.Ceiling(((heal * Math.Min(PotionHealEffect, MAX_POTION_EFFECT)) * (1.0 - (Math.Min(PotionHealEffect, MAX_POTION_EFFECT) * 1.5))) / 10.0);
			}
			else if (healsettime != 0)
			{
				healsettime = 0;
			}

			if(player.player.statLife < player.player.statLifeMax2 && !canHeal)
			{
				canHeal = true;
			}
			else
			{
				canHeal = false;
			}

			if (!player.player.HasBuff(BuffID.PotionSickness) && hasHealed)
			{
				hasHealed = false;
			}
		}

		[AutoDelegation("TooltipModifier")]
		private void tooltip(ModifierPlayer player, Item item, List<TooltipLine> tooltip)
		{
			// Check if the item is able to heal the player via quickheal hotkey
			if (item.healLife > 0)
			{
				// The value the potion is supposed to heal
				int healValue = (int)Math.Ceiling((heal * (1.0 - Math.Min(PotionHealEffect, MAX_POTION_EFFECT))) * (1.0 - (Math.Min(PotionHealEffect, MAX_POTION_EFFECT) * 1.5)));
				// The value the player is supposed to regenerate
				int lifeRegenValue = (int)Math.Ceiling(((heal * Math.Min(PotionHealEffect, MAX_POTION_EFFECT)) * (1.0 - (Math.Min(PotionHealEffect, MAX_POTION_EFFECT) * 1.5))) / 10.0);
				// The duration of the regenerate effect
				int durValue = 10;

				TooltipLine heallife = new TooltipLine(Loot.Instance, item.Name, "Restores " + healValue + " life");
				TooltipLine liferegen = new TooltipLine(Loot.Instance, item.Name, "Increases Life Regen by " + lifeRegenValue + " for " + durValue + " seconds");

				TooltipLine[] array = tooltip.ToArray();

				for (int i = 0; i < array.Length; i++)
				{
					// Clear the entire tooltip
					tooltip.Remove(array[i]);

					if (array[i].text.ToLower().Contains("life"))
					{
						// if the value is higher than 0 add the respective tooltip
						if (healValue > 0)
							tooltip.Add(heallife);
						if (lifeRegenValue > 0)
							tooltip.Add(liferegen);
					}
					else
					{
						// Add the old tooltip lines excluding anything with "life"
						tooltip.Add(array[i]);
					}
				}
			}
		}
	}

	[UsesEffect(typeof(LingeringEffect))]
	public class LingeringPotionEffect : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new ModifierTooltipLine[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{(int)(Properties.RoundedPower * 1.5)}% healing from potions, but {Properties.RoundedPower}% of it is over time" +
					   $"{(Main.LocalPlayer.GetModPlayer<ModifierPlayer>().GetEffect<LingeringEffect>().PotionHealEffect >= LingeringEffect.MAX_POTION_EFFECT ? $" (cap reached: {LingeringEffect.MAX_POTION_EFFECT * 100f}%)" : "")}",
				Color = Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 8f,
					rollChance: 0.333f,
					rarityLevel: 2f,
					uniqueRoll: true
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<LingeringEffect>().PotionHealEffect += (Properties.RoundedPower / 100f);
		}
	}
}
