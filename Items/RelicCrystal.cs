using Loot.Core;
using Loot.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Loot.Items
{
	public class RelicCrystal : CrystalItem
	{
		internal override string CrystalName => "Relic Crystal";
		protected override Color? OverrideNameColor => Color.Tan;
		protected override string Usage => "increase it's maximum possible modifiers";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<RelicCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			EMMItem oldItem = EMMItem.GetItemInfo(item);

			if (oldItem.CurrentPossibleModifiers >= EMMItem.MaximumPossibleModifiers)
			{
				Main.NewText(item.Name + " cannot be modified further");
				return false;
			}

			Item nitem = new Item();
			nitem.SetDefaults(item.type);
			EMMItem theItem = EMMItem.GetItemInfo(nitem);
			theItem.Clone(oldItem);

			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (player.inventory[i].active && player.inventory[i].stack > 0 && player.inventory[i].type > 0)
				{
					var a = EMMItem.GetItemInfo(player.inventory[i]);

					if (a != null && theItem != null)
					{
						if (theItem.IsTheSameAs(a) && player.inventory[i].IsTheSameAs(hoverItem))
						{
							theItem.CurrentPossibleModifiers++;
							player.inventory[i] = nitem;
							hoverItem = player.inventory[i];

							if (Main.netMode == 2)
							{
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, nitem.type);
							}

							return true;
						}
					}
				}
			}

			for (int i = 0; i < player.armor.Length; i++)
			{
				if (player.armor[i].active && player.armor[i].stack > 0 && player.armor[i].type > 0)
				{
					var a = EMMItem.GetItemInfo(player.armor[i]);

					if (a != null && theItem != null)
					{
						if (theItem.IsTheSameAs(a) && player.armor[i].IsTheSameAs(hoverItem))
						{
							theItem.CurrentPossibleModifiers++;
							player.armor[i] = nitem;
							hoverItem = player.armor[i];

							if (Main.netMode == 2)
							{
								NetMessage.SendData(MessageID.SyncItem, -1, -1, null, nitem.type);
							}

							return true;
						}
					}
				}
			}

			NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Failed to find item " + item.Name + " in " + Main.LocalPlayer.name + "'s inventory"), Color.Red);
			return false;
		}

		protected override ItemRollProperties RollLogic()
		{
			return null;
		}
	}
}
