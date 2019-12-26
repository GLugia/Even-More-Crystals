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
using Terraria.ModLoader;

namespace Loot.Items
{
	public class EternicCrystal : CrystalItem
	{
		internal override string CrystalName => "Eternic Crystal";
		protected override Color? OverrideNameColor => Color.Purple;
		protected override string Usage => "create an imprint containing an exact copy of it's modifiers";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<EternicCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			if (!Main.mouseItem.IsAir || item.type == ModContent.ItemType<EternicCrystalImprint>())
			{
				return false;
			}

			Item imprint = new Item();
			imprint.SetDefaults(ModContent.ItemType<EternicCrystalImprint>());

			var info = EMMItem.GetItemInfo(item);
			var cinfo = EMMItem.GetItemInfo(imprint);

			cinfo.ModifierPool = info.ModifierPool;
			cinfo.HasRolled = true;
			cinfo.ItemHash = info.ItemHash;
			player.QuickSpawnClonedItem(imprint);

			return true;
		}

		protected override ItemRollProperties RollLogic()
		{
			return null;
		}
	}
}
