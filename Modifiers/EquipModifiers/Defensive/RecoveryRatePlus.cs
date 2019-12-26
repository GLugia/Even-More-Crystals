using Loot.Core;
using Loot.Core.Attributes;
using Loot.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class RecoveryRateEffect : ModifierEffect
	{
		public float LifeEffect = 0f;
		public float ManaEffect = 0f;

		public override void ResetEffects(ModifierPlayer player)
		{
			LifeEffect = 0f;
			ManaEffect = 0f;
		}

		// This scales the healing recieved from potions and similar effects
		[AutoDelegation("OnHealLife")]
		[DelegationPrioritization(DelegationPrioritization.Early, 910)]
		private void fromLifeFlasks(ModifierPlayer player, Item item, bool quickHeal, ref int healValue)
		{
			if (healValue > 0)
			{
				healValue = (int)Math.Ceiling(healValue * (1.0 + LifeEffect));
			}
		}

		// This scales the mana recieved from potions and similar effects
		[AutoDelegation("OnHealMana")]
		[DelegationPrioritization(DelegationPrioritization.Early, 910)]
		private void fromManaFlasks(ModifierPlayer player, Item item, bool quickMana, ref int manaValue)
		{
			if (manaValue > 0)
			{
				manaValue = (int)Math.Ceiling(manaValue * (1.0 + ManaEffect));
			}
		}

		[AutoDelegation("OnPickup")]
		private bool fromPickup(ModifierPlayer player, Item item)
		{
			if(item.netID == ItemID.Heart)
			{
				int heal = (int)Math.Ceiling(20 * (1.0 + LifeEffect));

				Main.PlaySound(SoundID.Grab);
				player.player.statLife += heal;
				player.player.HealEffect(heal); // Call after increasing the life so the server syncs properly
				return false;
			}

			if(item.netID == ItemID.Star)
			{
				int mana = (int)Math.Ceiling(40 * (1.0 + ManaEffect));

				Main.PlaySound(SoundID.Grab);
				player.player.statMana += mana;
				player.player.ManaEffect(mana); // Call after increasing the mana so the server syncs properly
				return false;
			}

			return true;
		}

		[AutoDelegation("OnUpdateLifeRegen")]
		private void fromRegen(ModifierPlayer player)
		{
			/// We can't scale <see cref="Player.lifeRegen"/> since it's ever-changing but we can at least scale this
			int thismodRegen = player.GetEffect<LifeRegenEffect>().LifeRegen;

			// This just checks if the player has a regen potion active
			// although modded regen effects won't be taken into account
			// since that's a lot of work and i'm lazy
			if (player.player.HasBuff(BuffID.Regeneration))
				thismodRegen += 4;

			player.player.lifeRegen = (int)Math.Ceiling(thismodRegen * (1.0 + LifeEffect));


			// @TODO this doesn't seem to work either? maybe try to get what would be the base mana regen from the wiki's equation?
			int manaRegen = player.player.manaRegen;

			if (player.player.manaRegenBuff)
				manaRegen += player.player.manaRegenBonus;

			player.player.manaRegen = (int)Math.Ceiling(manaRegen * (1.0 + ManaEffect));
		}

		// Here we attempt to scale the healing recieved by healers
		// but i'm honestly not sure if it even works
		// tp.healRecieveBonus might not even be used as i assume
		// but i don't have the ThoriumMod's source so i can't check easily
		/*private void fromHealer(ModifierPlayer player, ModifierPlayer target, int heal)
		{
			// @TODO hooks for player recieving healing
			if(ModLoader.GetMod("ThoriumMod") == null)
			{
				throw new Exception("Thorium is not loaded");
			}

			ThoriumPlayer tp = ThoriumMod.Utilities.PlayerHelper.GetThoriumPlayer(player.player);

			if(tp.healReceiveBonus > 0)
			{
				tp.healReceiveBonus = (int)Math.Ceiling(tp.healReceiveBonus * (1.0 + RecoveryEffect));
			}
			else
			{
				tp.healReceiveBonus = (int)Math.Ceiling(1.0 * (1.0 + RecoveryEffect));
			}
		}*/
	}

	[UsesEffect(typeof(RecoveryRateEffect))]
	public class RecoveryRatePlus : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{(horm ? Properties.RoundedPower : Properties.RoundedPower * 2)}% {(horm ? "life" : "mana")} recovered from all sources",
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
					rarityLevel: 2f
				);
		}

		private bool horm;

		public override void Roll(ModifierContext ctx, IEnumerable<Modifier> rolledModifiers)
		{
			base.Roll(ctx, rolledModifiers);
			horm = Main.rand.NextBool();
		}

		public override void Save(Item item, TagCompound tag)
		{
			base.Save(item, tag);
			tag.Add("recovery", horm);
		}

		public override void Load(Item item, TagCompound tag)
		{
			base.Load(item, tag);
			horm = tag.GetBool("recovery");
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			base.NetSend(item, writer);
			writer.Write(horm);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			base.NetReceive(item, reader);
			horm = reader.ReadBoolean();
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (horm)
			{
				ModifierPlayer.Player(player).GetEffect<RecoveryRateEffect>().LifeEffect += Properties.RoundedPower / 100f;
			}
			else
			{
				ModifierPlayer.Player(player).GetEffect<RecoveryRateEffect>().ManaEffect += (Properties.RoundedPower * 2) / 100f;
			}
		}
	}
}
