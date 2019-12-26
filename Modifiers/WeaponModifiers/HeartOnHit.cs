using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Loot.Modifiers.WeaponModifiers
{
	public class HeartOnHitEffect : ModifierEffect
	{
		public float hchance = 0f;
		public float mchance = 0f;
		private bool hhasRolled = false;
		private bool mhasRolled = false;
		private float htime = 0f;
		private float mtime = 0f;

		public override void ResetEffects(ModifierPlayer player)
		{
			hchance = 0f;
			mchance = 0f;
		}

		[AutoDelegation("OnModifyHitNPC")]
		[DelegationPrioritization(DelegationPrioritization.Late, 100)]
		public void ModifyHitNPC(ModifierPlayer player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			AttemptToSpawn(ntarget: target);
		}

		[AutoDelegation("OnModifyHitNPCWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 100)]
		private void ModifyHitNPCWithProj(ModifierPlayer player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			AttemptToSpawn(ntarget: target);
		}

		[AutoDelegation("OnModifyHitPvp")]
		[DelegationPrioritization(DelegationPrioritization.Late, 100)]
		private void ModifyHitPvp(ModifierPlayer player, Item item, Player target, ref int damage, ref bool crit)
		{
			AttemptToSpawn(ptarget: target);
		}

		[AutoDelegation("OnModifyHitPvpWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 100)]
		private void ModifyHitPvpWithProj(ModifierPlayer player, Projectile proj, Player target, ref int damage, ref bool crit)
		{
			AttemptToSpawn(ptarget: target);
		}

		private void AttemptToSpawn(NPC ntarget = null, Player ptarget = null)
		{
			float hroll = Main.rand.NextFloat();
			float mroll = Main.rand.NextFloat();
			var rect = ntarget != null ? ntarget.getRect() : ptarget != null ? ptarget.getRect() : throw new ArgumentNullException("Unknown error occurred in HeartOnHit");

			if (hroll <= hchance && !hhasRolled)
			{
				hhasRolled = true;
				htime = Main.GlobalTime + 1f;
				int type = Item.NewItem(rect, ItemID.Heart);

				if (Main.netMode == 2)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, type);
				}
			}

			if (mroll <= mchance && !mhasRolled)
			{
				mhasRolled = true;
				mtime = Main.GlobalTime + 1f;
				int type = Item.NewItem(rect, ItemID.Star);

				if (Main.netMode == 2)
				{
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, type);
				}
			}
		}

		[AutoDelegation("OnPostUpdate")]
		[DelegationPrioritization(DelegationPrioritization.Late, 100)]
		private void ResetRolls(ModifierPlayer player)
		{
			if (hhasRolled && (htime <= Main.GlobalTime))
			{
				hhasRolled = false;
			}

			if (mhasRolled && (mtime <= Main.GlobalTime))
			{
				mhasRolled = false;
			}
		}
	}

	[UsesEffect(typeof(HeartOnHitEffect))]
	public class HeartOnHit : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower}% chance to create a {(_hom ? "heart" : "mana star")} on hit",
				Color = Color.Lime
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 3f,
					rollChance: 0.333f,
					rarityLevel: 2f
				);
		}

		private bool _hom;

		public override void Roll(ModifierContext ctx, IEnumerable<Modifier> rolledModifiers)
		{
			base.Roll(ctx, rolledModifiers);
			_hom = Main.rand.NextBool();
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			base.NetSend(item, writer);
			writer.Write(_hom);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			base.NetReceive(item, reader);
			_hom = reader.ReadBoolean();
		}

		public override void Save(Item item, TagCompound tag)
		{
			base.Save(item, tag);
			tag.Add("hom", _hom);
		}

		public override void Load(Item item, TagCompound tag)
		{
			base.Load(item, tag);
			_hom = tag.GetBool("hom");
		}

		public override void UpdateInventory(Item item, Player player)
		{
			base.UpdateInventory(item, player);

			if (ActivatedModifierItem.Item(item).IsActivated)
			{
				if (_hom)
				{
					ModifierPlayer
						.Player(player)
						.GetEffect<HeartOnHitEffect>()
						.hchance += Properties.RoundedPower / 100f;
				}
				else
				{
					ModifierPlayer
						.Player(player)
						.GetEffect<HeartOnHitEffect>()
						.mchance += Properties.RoundedPower / 100f;
				}
			}
		}
	}
}
