using System.Collections.Generic;
using System.IO;
using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Loot.Items
{
	public class EternicCrystalImprint : CrystalItem
	{
		internal override string CrystalName => "Imprint";
		protected override Color? OverrideNameColor => Main.DiscoColor;
		protected override string Usage => "return an item to its previous form";
		protected override bool Placeable => false;

		public override void SafeDefaults()
		{
			item.maxStack = 1;
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			Item nitem = new Item();
			nitem.SetDefaults(item.type);
			var oldInfo = EMMItem.GetItemInfo(item);
			var newInfo = EMMItem.GetItemInfo(nitem);
			newInfo.Clone(oldInfo);

			var cinfo = EMMItem.GetItemInfo(this.item);

			if (newInfo.ItemHash == cinfo.ItemHash)
			{
				ModifierContext ctx = new ModifierContext
				{
					Method = ModifierContextMethod.OnCubeReroll,
					Item = nitem,
					Imprint = this.item,
					Player = player,
					CustomData = new Dictionary<string, object>
					{
						{ "Source", "Imprint" }
					}
				};

				ModifierPool pool = newInfo.AlterPool(ctx, revert: true);

				return ApplyModifiers(player, pool, nitem);
			}

			Main.NewText("Imprint cannot be applied to this item");
			return false;
		}

		protected override ItemRollProperties RollLogic()
		{
			return null;
		}

		protected override void AddTooltip(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(mod, "Modifier: CannotEdit", "Item cannot be modified"));
			tooltips.Add(new TooltipLine(mod, "Modifier: Hash", $"Saved hash: {EMMItem.GetItemInfo(this.item).ItemHash}") { overrideColor = Color.Gold * Main.inventoryScale });
		}
	}
}
