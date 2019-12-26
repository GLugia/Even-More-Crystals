using Loot.Core.Recipes;
using Loot.Items;
using Loot.World;
using Microsoft.Win32.SafeHandles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Loot.Tiles
{
	public abstract class CrystalTile : ModTile
	{
		// The tile's name
		public virtual string TileName { get; }

		public virtual float GrowthFrequency => 0.05f;

		public virtual Color OverrideMinimapColor => Color.White;

		public virtual short CrystalRarity => -1;

		//public static T GetCrystalTile<T>(int x, int y) where T : CrystalTile => Framing.GetTileSafely(new Vector2(x, y)) as T;

		// Animate the tile with a glossy effect
		public sealed override void AnimateTile(ref int frame, ref int frameCounter)
		{
			base.AnimateTile(ref frame, ref frameCounter);
		}

		// Autoselect the pickaxe available in the inventory
		public sealed override bool AutoSelect(int i, int j, Item item)
		{
			return item.pick > 0;
		}
		
		// I don't want to allow overriding this
		public sealed override bool CanPlace(int i, int j)
		{
			TileObjectData data = TileObjectData.GetTileData(Type, TileObjectData.Style1x1.Style);

			if ((Main.tile[i, j + 1].active() && Main.tileSolid[Main.tile[i, j + 1].type] && Main.tile[i, j + 1].slope() == 0 && !Main.tile[i, j + 1].halfBrick() && (data.isValidTileAnchor(Main.tile[i, j + 1].type) || data.isValidAlternateAnchor(Main.tile[i, j + 1].type)))
			 || (Main.tile[i, j - 1].active() && Main.tileSolid[Main.tile[i, j - 1].type] && Main.tile[i, j - 1].slope() == 0 && !Main.tile[i, j - 1].halfBrick() && (data.isValidTileAnchor(Main.tile[i, j - 1].type) || data.isValidAlternateAnchor(Main.tile[i, j - 1].type)))
			 || (Main.tile[i + 1, j].active() && Main.tileSolid[Main.tile[i + 1, j].type] && Main.tile[i + 1, j].slope() == 0 && !Main.tile[i + 1, j].halfBrick() && (data.isValidTileAnchor(Main.tile[i + 1, j].type) || data.isValidAlternateAnchor(Main.tile[i + 1, j].type)))
			 || (Main.tile[i - 1, j].active() && Main.tileSolid[Main.tile[i - 1, j].type] && Main.tile[i - 1, j].slope() == 0 && !Main.tile[i - 1, j].halfBrick() && (data.isValidTileAnchor(Main.tile[i - 1, j].type) || data.isValidAlternateAnchor(Main.tile[i - 1, j].type))))
			{
				return true;
			}
			else if (Main.tile[i, j + 1].type == TileID.ClayPot)
			{
				return true;
			}

			return false;
		}
		
		// Draw dust particles around the crystal while it's placed in the world
		public sealed override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
		{
			base.DrawEffects(i, j, spriteBatch, ref drawColor, ref nextSpecialDrawIndex);
		}

		// Drop the corrosponding crystal item instead of a tile block
		public sealed override bool Drop(int i, int j)
		{
			int item = ReturnItem().item.type;
			int stage = Main.tile[i, j].frameX / 18;
			int amount = ReturnAmount();

			if (stage == 2)
			{
				Item.NewItem(new Vector2(i * 16, j * 16), item, amount);
			}
			else if (Main.rand.NextBool())
			{
				Item.NewItem(new Vector2(i * 16, j * 16), item, 1);
			}

			return false;
		}

		// Generate a glass breaking sound in the location of the tile
		public sealed override bool KillSound(int i, int j)
		{
			Vector2 pos = new Vector2(i * 16, j * 16);
			Main.PlaySound(SoundID.Shatter, pos);
			return false;
		}

		// Only give multiple items if the crystal is fully grown
		// Otherwise give one item
		public sealed override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = ReturnItem().item.type;
			int stage = Main.tile[i, j].frameX / 18;
			int amount = ReturnAmount();

			if (stage == 2)
			{
				Item.NewItem(new Vector2(i * 16, j * 16), item, amount);
			}
			else if(Main.rand.NextBool())
			{
				Item.NewItem(new Vector2(i * 16, j * 16), item, 1);
			}
			else
			{
				return;
			}
		}

		public abstract int ReturnAmount();

		public abstract CrystalItem ReturnItem();

		// Generate a glowy effect from the crystals
		public sealed override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = OverrideMinimapColor.R / 1000f;
			b = OverrideMinimapColor.B / 1000f;
			g = OverrideMinimapColor.G / 1000f;
		}

		public sealed override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = (fail ? 1 : 3);
		}

		public sealed override void PlaceInWorld(int i, int j, Item item)
		{
			TileObjectData data = TileObjectData.GetTileData(Type, TileObjectData.Style1x1.Style);

			if (Main.tile[i, j + 1].active() && Main.tileSolid[Main.tile[i, j + 1].type] && Main.tile[i, j + 1].slope() == 0 && !Main.tile[i, j + 1].halfBrick() && (data.isValidTileAnchor(Main.tile[i, j + 1].type) || data.isValidAlternateAnchor(Main.tile[i, j + 1].type)))
			{
				Main.tile[i, j].frameY = 0;
			}
			else if (Main.tile[i, j - 1].active() && Main.tileSolid[Main.tile[i, j - 1].type] && Main.tile[i, j - 1].slope() == 0 && !Main.tile[i, j - 1].halfBrick() && (data.isValidTileAnchor(Main.tile[i, j - 1].type) || data.isValidAlternateAnchor(Main.tile[i, j - 1].type)))
			{
				Main.tile[i, j].frameY = 18;
			}
			else if (Main.tile[i + 1, j].active() && Main.tileSolid[Main.tile[i + 1, j].type] && Main.tile[i + 1, j].slope() == 0 && !Main.tile[i + 1, j].halfBrick() && (data.isValidTileAnchor(Main.tile[i + 1, j].type) || data.isValidAlternateAnchor(Main.tile[i + 1, j].type)))
			{
				Main.tile[i, j].frameY = 36;
			}
			else if (Main.tile[i - 1, j].active() && Main.tileSolid[Main.tile[i - 1, j].type] && Main.tile[i - 1, j].slope() == 0 && !Main.tile[i - 1, j].halfBrick() && (data.isValidTileAnchor(Main.tile[i - 1, j].type) || data.isValidAlternateAnchor(Main.tile[i - 1, j].type)))
			{
				Main.tile[i, j].frameY = 54;
			}
		}

		// Grow the crystal to the next stages
		public sealed override void RandomUpdate(int i, int j)
		{
			if (Main.tile[i, j].type == Type
				&& Main.rand.NextFloat() <= GrowthFrequency
				&& Main.tile[i, j].frameX / 18 < 2)
			{
				Main.tile[i, j].frameX += 18;
			}
		}

		// Set the tile properties
		public sealed override void SetDefaults()
		{
			base.SetDefaults();

			Main.tileFrameImportant[Type] = true;
			
			// Create tile data from 1x1
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.UsesCustomCanPlace = true;
			// Allow anchoring to only allowed tiles
			TileObjectData.newTile.AnchorValidTiles = ValidTileIDs();
			TileObjectData.newTile.AnchorAlternateTiles = ValidTileIDs();

			// Anchoring begin
			
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorTop = AnchorData.Empty;
			TileObjectData.addAlternate(0);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(1);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(2);

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
			TileObjectData.addAlternate(3);

			// Anchoring end

			TileObjectData.addTile(Type);

			Main.tileNoAttach[Type] = true;
			Main.tileNoFail[Type] = true;
			
			Main.tileSpelunker[Type] = true;
			Main.tileValue[Type] = CrystalRarity != -1 ? CrystalRarity : (short)300;

			Main.tileShine[Type] = 250;
			Main.tileShine2[Type] = true;

			Main.tileLighted[Type] = true;

			ModTranslation name = CreateMapEntryName();
			name.SetDefault(TileName);
			AddMapEntry(OverrideMinimapColor, name);

			dustType = DustID.LunarOre;
			soundType = SoundID.Shatter;
			soundStyle = 1;

			mineResist = 4;
			minPick = 25;

			this.SetTileDefaults();

			// One of these tiles will result in 2 of the related item
			RecipeHandler.AddRecipe
				(
				ModContent.GetModItem(FinishRecipe().Item1), FinishRecipe().Item2,
				new object[]
				{
					this
				}, new int[]
				{
					1
				});
		}

		protected abstract int[] ValidTileIDs();
		protected abstract int[] AltValidTileIDs();

		// This may be needed instead of setting a property
		// Depends on when certain methods are called tbh
		// public abstract CrystalItem SetRelatedItem();

		public virtual void SetTileDefaults() { }

		public abstract (int, int) FinishRecipe();

		// Don't allow hammers to slope the tile
		public sealed override bool Slope(int i, int j)
		{
			return false;
		}

		public sealed override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
			int fv = Main.tile[i, j].frameY / 18;

			switch (fv)
			{
				case 0:
				case 1:
					{
						if (i % 2 == 1 && j % 2 == 1)
						{
							spriteEffects = SpriteEffects.FlipHorizontally;
						}

						break;
					}
				case 2:
				case 3:
					{
						if(i % 2 == 1 && j % 2 == 1)
						{
							spriteEffects = SpriteEffects.FlipVertically;
						}

						break;
					}
			}
		}

		public sealed override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			Tile ct = Main.tile[i, j];

			if(ct.type == Type)
			{
				Tile tileUp = Main.tile[i, j - 1];
				Tile tileDown = Main.tile[i, j + 1];
				Tile tileLeft = Main.tile[i - 1, j];
				Tile tileRight = Main.tile[i + 1, j];
				TileObjectData data = TileObjectData.GetTileData(Type, TileObjectData.Style1x1.Style);

				if (tileDown != null && tileDown.type > 0 && tileDown.slope() == 0 && !tileDown.halfBrick() && (data.isValidTileAnchor(tileDown.type) || data.isValidAlternateAnchor(tileDown.type)))
				{
					ct.frameY = 0;
				}
				else if (tileUp != null && tileUp.type > 0 && tileUp.slope() == 0 && !tileUp.halfBrick() && (data.isValidTileAnchor(tileUp.type) || data.isValidAlternateAnchor(tileUp.type)))
				{
					ct.frameY = 18;
				}
				else if (tileRight != null && tileRight.type > 0 && tileRight.slope() == 0 && !tileRight.halfBrick() && (data.isValidTileAnchor(tileRight.type) || data.isValidAlternateAnchor(tileRight.type)))
				{
					ct.frameY = 36;
				}
				else if (tileLeft != null && tileLeft.type > 0 && tileLeft.slope() == 0 && !tileLeft.halfBrick() && (data.isValidTileAnchor(tileLeft.type) || data.isValidAlternateAnchor(tileLeft.type)))
				{
					ct.frameY = 54;
				}
				else
				{
					WorldGen.KillTile(i, j, noBreak, false, false);

					if (Main.netMode == 2)
					{
						NetMessage.SendData(MessageID.TileChange, -1, -1, null, i, j);
					}
				}
			}

			return true;
		}
	}
}
