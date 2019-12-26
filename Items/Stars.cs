using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot.Items
{
	/// <summary>
	/// This is just here to nerf stars for the sake of balancing this mod
	/// </summary>
	public class Stars : GlobalItem
	{
		public override bool OnPickup(Item item, Player player)
		{
			if (item.netID == ItemID.Star)
			{
				Main.PlaySound(SoundID.Grab);
				player.statMana += 40;
				player.ManaEffect(40);

				if (Main.netMode == 2)
				{
					player.netMana = true;
					player.netManaTime = 60;
					NetMessage.SendData(MessageID.PlayerMana, -1, -1, null, Main.myPlayer);
					NetMessage.SendData(MessageID.ManaEffect, -1, -1, null, Main.myPlayer, 40);
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.type);
				}

				return false;
			}

			return base.OnPickup(item, player);
		}
	}
}
