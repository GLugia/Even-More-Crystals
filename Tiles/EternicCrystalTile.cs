using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loot.Items;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot.Tiles
{
	public class EternicCrystalTile : CrystalTile
	{
		public override string TileName => "Eternic Crystal";
		public override float GrowthFrequency => 0.2f;
		public override Color OverrideMinimapColor => Color.Purple;

		public override (int, int) FinishRecipe()
		{
			return (0, 0);
		}

		public override int ReturnAmount()
		{
			return 2;
		}

		public override CrystalItem ReturnItem()
		{
			return ModContent.GetModItem(ModContent.ItemType<EternicCrystal>()) as CrystalItem;
		}

		protected override int[] AltValidTileIDs()
		{
			return new int[]
			{
				TileID.PlanterBox,
				TileID.ClayPot
			};
		}

		protected override int[] ValidTileIDs()
		{
			return new int[]
			{
				TileID.Cloud,
				TileID.RainCloud,
				TileID.SnowCloud
			};
		}
	}
}
