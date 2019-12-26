using Loot.Core;
using Loot.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Loot.Items
{
	public class AbysmicCrystal : CrystalItem
	{
		internal override string CrystalName => "Abysmic Crystal";
		protected override Color? OverrideNameColor => Color.RoyalBlue;
		protected override string Usage => "remove a random modifier";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<AbysmicCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			// @TODO remove a modifier from the pool

			var actives = EMMItem.GetActivePool(item);

			if(actives.Count() > 0)
			{
				Item nitem = new Item();
				nitem.SetDefaults(item.type);
				EMMItem oldItem = EMMItem.GetItemInfo(item);
				EMMItem theItem = EMMItem.GetItemInfo(nitem);
				theItem.Clone(oldItem);

				ModifierContext ctx = new ModifierContext
				{
					Method = ModifierContextMethod.OnCubeReroll,
					Item = nitem,
					Player = player,
					CustomData = new Dictionary<string, object>
					{
						{ "Source", "AbysmicCrystal" }
					}
				};

				ModifierPool pool = theItem.AlterPool(ctx, remove: true, amount: 1);

				return ApplyModifiers(player, pool, nitem);
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
