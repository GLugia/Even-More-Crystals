using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class DodgeEffect : ModifierEffect
	{
		public float DodgeChance; // Dodge chance
		public static float MAX_DODGE_CHANCE = 0.5f;
		public bool Dodged;

		public override void ResetEffects(ModifierPlayer player)
		{
			DodgeChance = 0f;
			Dodged = false;
		}

		[AutoDelegation("OnPreHurt")]
		[DelegationPrioritization(DelegationPrioritization.Early, 100)]
		private bool TryDodge(ModifierPlayer player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			float rand = Main.rand.NextFloat();
			float chance = Math.Min(DodgeChance, MAX_DODGE_CHANCE);

			if (rand <= chance)
			{
				player.player.NinjaDodge();
				Dodged = true;
				return false;
			}

			return true;
		}
	}

	[UsesEffect(typeof(DodgeEffect))]
	public class DodgeChance : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% dodge chance"
					 + $"{(Main.LocalPlayer.GetModPlayer<ModifierPlayer>().GetEffect<DodgeEffect>().DodgeChance >= DodgeEffect.MAX_DODGE_CHANCE ? $" (cap reached: {DodgeEffect.MAX_DODGE_CHANCE * 100}%)" : "")}"
					 ,
				Color = Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 8f,
					rollChance: 0.333f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<DodgeEffect>().DodgeChance += Properties.RoundedPower / 100f;
		}
	}
}
