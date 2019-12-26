using Loot.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot.Tiles
{
	public class ChaoticCrystalTile : CrystalTile
	{
		public override string TileName => "Chaotic Crystal";
		public override float GrowthFrequency => 0.005f;
		public override Color OverrideMinimapColor => Color.Gray;

		protected override int[] ValidTileIDs()
		{
			return new int[]
			{
				TileID.Stone,
				TileID.GrayBrick,
				TileID.Pearlstone,
				TileID.PearlstoneBrick,
				TileID.Ebonstone,
				TileID.EbonstoneBrick,
				TileID.Crimstone
			};
		}

		protected override int[] AltValidTileIDs()
		{
			return new int[]
			{
				TileID.PlanterBox,
				TileID.ClayPot
			};
		}

		public override void SetTileDefaults()
		{
			drop = ModContent.ItemType<ChaoticCrystal>();
		}

		public override (int, int) FinishRecipe()
		{
			return (ModContent.ItemType<ChaoticCrystal>(), 1);
		}

		public override CrystalItem ReturnItem()
		{
			return ModContent.GetModItem(ModContent.ItemType<ChaoticCrystal>()) as CrystalItem;
		}

		public override int ReturnAmount()
		{
			return 2;
		}
	}
}
