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
	public class MagmaticCrystal : CrystalItem
	{
		internal override string CrystalName => "Magmatic Crystal";
		protected override Color? OverrideNameColor => Color.OrangeRed;
		protected override string Usage => "unfreeze a random modifier";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<MagmaticCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			var actives = EMMItem.GetActivePool(item);

			if (actives != null
				&& actives.Count() > 0
				&& actives.Where(x => x.locked).Count() > 0)
			{
				int id;

				do
				{
					id = Main.rand.Next(0, actives.Count());
				}
				while (!actives.ElementAt(id).locked);

				actives.ElementAt(id).locked = false;

				EMMItem oldItem = EMMItem.GetItemInfo(item);
				Item nitem = new Item();
				nitem.SetDefaults(item.type);
				EMMItem theItem = EMMItem.GetItemInfo(nitem);
				theItem.Clone(oldItem);

				theItem.ModifierPool?.ApplyModifiers(nitem);

				for (int i = 0; i < player.inventory.Length; i++)
				{
					if (player.inventory[i].active && player.inventory[i].stack > 0 && player.inventory[i].type > 0)
					{
						var invitem = EMMItem.GetItemInfo(player.inventory[i]);

						if (invitem != null && oldItem != null)
						{
							if (oldItem.IsTheSameAs(invitem) && player.inventory[i].IsTheSameAs(hoverItem))
							{
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
						var invitem = EMMItem.GetItemInfo(player.armor[i]);

						if (invitem != null && theItem != null)
						{
							if (theItem.IsTheSameAs(invitem) && player.armor[i].IsTheSameAs(hoverItem))
							{
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

			Main.NewText(item.Name + " cannot be modified further");
			return false;
		}

		protected override ItemRollProperties RollLogic()
		{
			return null;
		}
	}
}
