using Loot.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Utilities;
using Terraria.World.Generation;

namespace Loot.World
{
	public static class GenUtil
	{
		public static void SpawnPlant(int type)
		{
			if(Main.netMode == 1)
			{
				return;
			}

			UnifiedRandom rand = WorldGen.genRand;

			int minx = 1;
			int maxx = Main.maxTilesX - minx;
			int miny = (int)Main.worldSurface;
			int maxy = Main.maxTilesY - 1;

			int count = 0;

			if(ModContent.GetModTile(type).GetType().IsSubclassOf(typeof(CrystalTile)))
			{
				CrystalTile c = ModContent.GetModTile(type) as CrystalTile;

				Loot.Logger.Debug("Placing " + c.TileName);

				for (int x = minx; x < maxx; x++)
				{
					for (int y = miny; y < maxy; y++)
					{
						if (CanPlace(x, y, c) && !Main.tile[x, y].active() && rand.NextFloat() <= c.GrowthFrequency)
						{
							count++;
							WorldGen.PlaceTile(x, y, c.Type, true, false, -1);
							//WorldGen.Place1x1(x, y, c.Type, TileObjectData.Style1x1.Style);
							Main.tile[x, y].frameX = (short)(18 * rand.Next(0, 2));
							WorldUtils.TileFrame(x, y, true);
							WorldGen.TileFrame(x, y, true);
						}
					}
				}
			}
			else
			{
				throw new Exception("You shouldn't be calling this method.");
			}

			Loot.Logger.Debug(count);
		}

		private static bool CanPlace(int x, int y, CrystalTile item)
		{
			TileObjectData data = TileObjectData.GetTileData(item.Type, TileObjectData.Style1x1.Style);

			bool left = (data.isValidTileAnchor(Main.tile[x - 1, y].type) || data.isValidAlternateAnchor(Main.tile[x - 1, y].type)) && Main.tile[x - 1, y].active() && Main.tile[x - 1, y].slope() == 0 && !Main.tile[x - 1, y].halfBrick() && Main.tileSolid[Main.tile[x - 1, y].type];
			bool right = (data.isValidTileAnchor(Main.tile[x + 1, y].type) || data.isValidAlternateAnchor(Main.tile[x + 1, y].type)) && Main.tile[x + 1, y].active() && Main.tile[x + 1, y].slope() == 0 && !Main.tile[x + 1, y].halfBrick() && Main.tileSolid[Main.tile[x + 1, y].type];
			bool up = (data.isValidTileAnchor(Main.tile[x, y - 1].type) || data.isValidAlternateAnchor(Main.tile[x, y - 1].type)) && Main.tile[x, y - 1].active() && Main.tile[x, y - 1].slope() == 0 && !Main.tile[x, y - 1].halfBrick() && Main.tileSolid[Main.tile[x, y - 1].type];
			bool down = (data.isValidTileAnchor(Main.tile[x, y + 1].type) || data.isValidAlternateAnchor(Main.tile[x, y + 1].type)) && Main.tile[x, y + 1].active() && Main.tile[x, y + 1].slope() == 0 && !Main.tile[x, y + 1].halfBrick() && Main.tileSolid[Main.tile[x, y + 1].type];

			return left || right || up || down;
		}

		public static void FixPlants()
		{
			if(Main.netMode == 1)
			{
				return;
			}

			int minx = 1;
			int maxx = Main.maxTilesX - minx;
			int miny = (int)Main.worldSurface;
			int maxy = Main.maxTilesY - minx;

			for (int x = minx; x < maxx; x++)
			{
				for (int y = miny; y < maxy; y++)
				{
					if ((Main.tile[x, y].type == ModContent.TileType<AbysmicCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<AlchemicCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<ArcticCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<ChaoticCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<EternicCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<MagmaticCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<OceanicCrystalTile>()
					 || Main.tile[x, y].type == ModContent.TileType<RelicCrystalTile>()))
					{
						if ((!Main.tile[x, y - 1].active() || Main.tile[x, y - 1].slope() != 0 || Main.tile[x, y - 1].halfBrick() || !Main.tileSolid[Main.tile[x, y - 1].type])
						 && (!Main.tile[x, y + 1].active() || Main.tile[x, y + 1].slope() != 0 || Main.tile[x, y + 1].halfBrick() || !Main.tileSolid[Main.tile[x, y + 1].type])
						 && (!Main.tile[x - 1, y].active() || Main.tile[x - 1, y].slope() != 0 || Main.tile[x - 1, y].halfBrick() || !Main.tileSolid[Main.tile[x - 1, y].type])
						 && (!Main.tile[x + 1, y].active() || Main.tile[x + 1, y].slope() != 0 || Main.tile[x + 1, y].halfBrick() || !Main.tileSolid[Main.tile[x + 1, y].type]))
						{
							WorldGen.KillTile(x, y, noItem: true);
						}

						WorldGen.TileFrame(x, y, true);
						WorldUtils.TileFrame(x, y, true);
					}
				}
			}
		}

		public static void UpdateWorld(int type)
		{
			float updateRate = 3E-05f * Main.worldRate;
			float areaToScan = (Main.maxTilesX * Main.maxTilesY) * updateRate;

			for (int i = 0; i < areaToScan; i++)
			{
				if (Main.rand.Next(100) == 0)
				{
					int rx = Main.rand.Next(1, Main.maxTilesX - 1);
					int ry = Main.rand.Next(1, Main.maxTilesY - 1);
					CrystalTile c = ModContent.GetModTile(type) as CrystalTile;

					if (CanPlace(rx, ry, c) && !Main.tile[rx, ry].active() && Main.rand.NextFloat() <= c.GrowthFrequency)
					{
						WorldGen.PlaceTile(rx, ry, type, true, false, -1, 0);
						WorldGen.TileFrame(rx, ry, true);
						WorldUtils.TileFrame(rx, ry, true);

						if (Main.netMode == 2)
						{
							NetMessage.SendTileSquare(-1, rx, ry, 1);
						}
					}
				}
			}
		}

		private static string[] GenPhrase = new string[]
		{
			"Slamming some items", "Corrupting your gear", "Summoning the RNG gods", "Turning the world crystalline", "Crystalizing the moon", "RMTing gear", "Selling your soul to Tencent",
			"Incrementing relevance", "Empowering gems", "Swapping some gear", "Traversing enlightenment", "Enhancing the world", "Vendoring some trash", "Tenderizing bosses"
		};

		private static List<string> lastString;

		public static string GenPhraseGenerator()
		{
			if(lastString == null)
			{
				lastString = new List<string>();
			}

			if (lastString.Count() > 13)
			{
				lastString.Clear();
				lastString.TrimExcess();
			}

			string randString;

			do
			{
				randString = GenPhrase[Main.rand.Next(0, GenPhrase.Length)];
			}
			while (lastString.Contains(randString));

			lastString.Add(randString);

			return randString + "...";
		}

		internal static void UnloadUtil()
		{
			lastString = null;
			GenPhrase = null;
		}

		internal static void NetSend(BinaryWriter writer)
		{
		}

		internal static void NetRecieve(BinaryReader reader)
		{
		}
	}
}
