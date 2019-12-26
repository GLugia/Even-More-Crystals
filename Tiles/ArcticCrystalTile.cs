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
	public class ArcticCrystalTile : CrystalTile
	{
		public override string TileName => "Arctic Crystal";
		public override float GrowthFrequency => 0.005f;
		public override Color OverrideMinimapColor => Color.LightSkyBlue;

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
			return ModContent.GetModItem(ModContent.ItemType<ArcticCrystal>()) as CrystalItem;
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
				TileID.IceBrick,
				TileID.SnowBlock,
				TileID.SnowBrick,
				TileID.BreakableIce,

				TileID.IceBlock,
				TileID.CorruptIce,
				TileID.FleshIce,
				TileID.HallowedIce
			};
		}
	}
}
