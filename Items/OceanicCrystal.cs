using Loot.Core;
using Loot.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Loot.Items
{
	public class OceanicCrystal : CrystalItem
	{
		internal override string CrystalName => "Oceanic Crystal";
		protected override Color? OverrideNameColor => Color.DeepSkyBlue;
		protected override string Usage => "remove all of it's modifiers";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<OceanicCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
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
					Item = item,
					Player = player,
					CustomData = new Dictionary<string, object>
					{
						{ "Source", "OceanicCrystal" }
					}
				};

				ModifierPool pool = theItem.AlterPool(ctx, remove: true, amount: actives.Count());

				if (pool != null && pool.ActiveModifiers.Where(x => x.locked).Count() == actives.Count())
				{
					return false;
				}

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
