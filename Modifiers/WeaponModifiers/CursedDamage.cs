using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace Loot.Modifiers.WeaponModifiers
{
	public class CursedEffect : ModifierEffect
	{
		public int CurseCount;

		public override void ResetEffects(ModifierPlayer player)
		{
			CurseCount = 0;
		}

		[AutoDelegation("OnUpdateBadLifeRegen")]
		[DelegationPrioritization(DelegationPrioritization.Early, 101)]
		private void Curse(ModifierPlayer player)
		{
			if (CurseCount > 0 && !player.player.buffImmune[BuffID.Cursed])
			{
				if(player.player.lifeRegen < 2)
				{
					player.player.lifeRegen = 2;
				}

				int degen = (int)Math.Ceiling((player.player.statLifeMax2 / 10.0) / Math.Pow(player.player.lifeRegen, 1.25));

				player.player.lifeRegen -= degen;
				player.player.lifeRegenTime = 0;
			}
		}
	}

	[UsesEffect(typeof(CursedEffect))]
	public class CursedDamage : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					Text = $"+{Properties.RoundedPower}% damage, but you are cursed while holding this item",
					Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 4f,
					maxMagnitude: 12f,
					uniqueRoll: true
				);
		}

		public override bool CanRoll(ModifierContext ctx)
		{
			return base.CanRoll(ctx) && ctx.Method != ModifierContextMethod.SetupStartInventory;
		}

		public override void UpdateInventory(Item item, Player player)
		{
			if (player.HeldItem == item)
			{
				ModifierPlayer.Player(player).GetEffect<CursedEffect>().CurseCount++;
			}
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float multi, ref float flat)
		{
			base.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);
			add += Properties.RoundedPower / 100f;
		}
	}
}
