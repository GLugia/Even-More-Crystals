using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Loot.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot.Tiles
{
	public class RelicCrystalTile : CrystalTile
	{
		public override string TileName => "Relic Crystal";
		public override float GrowthFrequency => 0.0001f;
		public override Color OverrideMinimapColor => Color.Tan;

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
			return ModContent.GetModItem(ModContent.ItemType<RelicCrystal>()) as CrystalItem;
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
				TileID.Sand,
				TileID.HardenedSand,
				TileID.HallowSandstone,
				TileID.HallowHardenedSand,
				TileID.CorruptSandstone,
				TileID.CorruptHardenedSand,
				TileID.CrimsonSandstone,
				TileID.CrimsonHardenedSand,
				TileID.Sandstone,
				TileID.SandstoneBrick
			};
		}
	}
}
