using Loot.Items;
using Loot.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.World.Generation;

namespace Loot.World
{
	public static class CrystalBools
	{
		public const int Abysmic = 0;
		public const int Alchemic = 1;
		public const int Arctic = 2;
		public const int Chaotic = 3;
		public const int Eternic = 4;
		public const int Magmatic = 5;
		public const int Oceanic = 6;
		public const int Relic = 7;
	}

	public class LootWorld : ModWorld
	{
		public static bool[] SpawnedCrystals = new bool[8];

		public override void Initialize()
		{
			for (int i = 0; i < SpawnedCrystals.Length; i++)
			{
				SpawnedCrystals[i] = false;
			}
		}

		/// <summary>
		/// May add some crystal generation to the world from empowered crystals
		/// </summary>
		/// <param name="list"></param>
		public override void ModifyHardmodeTasks(List<GenPass> list)
		{
		}

		/// <summary>
		/// It may seem like a bit much here but we require spawning crystals in specific places after world generation
		/// This allows us to avoid any issues with there not being any room for the crystals to sit such as jungle plants >:(
		/// <see cref="GenUtil.SpawnPlant(int)"/>
		/// <see cref="GenUtil.GenPhraseGenerator()"/>
		/// </summary>
		/// <param name="list"></param>
		/// <param name="totalWeight"></param>
		public override void ModifyWorldGenTasks(List<GenPass> list, ref float totalWeight)
		{
			int islandindex = list.FindIndex((GenPass pass) => pass.Name == "Floating Islands");

			if (islandindex != -1)
			{
				list.Insert(islandindex + 1, new PassLegacy("Crystals wave 1", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<EternicCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Eternic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int underindex = list.FindIndex((GenPass pass) => pass.Name == "Underworld");

			if (underindex != -1)
			{
				list.Insert(underindex + 1, new PassLegacy("Crystals wave 2", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<MagmaticCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Magmatic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int gemindex = list.FindIndex((GenPass pass) => pass.Name == "Gems");

			if (gemindex != -1)
			{
				list.Insert(gemindex + 1, new PassLegacy("Crystals wave 3", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<ChaoticCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Chaotic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int pindex = list.FindIndex((GenPass pass) => pass.Name == "Pyramids");

			if (pindex != -1)
			{
				list.Insert(pindex + 1, new PassLegacy("Crystals wave 4", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<RelicCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Relic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int snowindex = list.FindIndex((GenPass pass) => pass.Name == "Ice");

			if (snowindex != -1)
			{
				list.Insert(snowindex + 1, new PassLegacy("Crystals wave 5", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<ArcticCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Arctic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int jindex = list.FindIndex((GenPass pass) => pass.Name == "Jungle Trees");

			if(jindex != -1)
			{
				list.Insert(jindex + 1, new PassLegacy("Crystals wave 6", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 3f;
					GenUtil.SpawnPlant(ModContent.TileType<AlchemicCrystalTile>());
					progress.Value += 1f / 3f;
					SpawnedCrystals[CrystalBools.Alchemic] = true;
					progress.Value += 1f / 3f;
					progress.CurrentPassWeight++;
				}));
			}

			int abyssindex = list.FindIndex((GenPass match) => match.Name == "Final Cleanup");

			if(abyssindex != -1)
			{
				list.Insert(abyssindex + 5, new PassLegacy("Crystals wave 7", delegate (GenerationProgress progress)
				{
					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 8f;
					GenUtil.SpawnPlant(ModContent.TileType<AbysmicCrystalTile>());
					progress.Value += 1f / 8f;
					SpawnedCrystals[CrystalBools.Abysmic] = true;
					progress.Value += 1f / 8f;
					GenUtil.SpawnPlant(ModContent.TileType<OceanicCrystalTile>());
					progress.Value += 1f / 8f;
					SpawnedCrystals[CrystalBools.Oceanic] = true;
					progress.Value += 1f / 8f;

					progress.Message = GenUtil.GenPhraseGenerator();
					progress.Value += 1f / 8f;
					GenUtil.FixPlants();
					progress.Value += 1f / 8f;
					WorldGen.WaterCheck();
					progress.Value += 1f / 8f;
					progress.CurrentPassWeight++;
				}));
			}
		}

		// Spawn plants randomly based on their growth rate
		public override void PostUpdate()
		{
			base.PostUpdate();

			GenUtil.UpdateWorld(ModContent.TileType<AbysmicCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<AlchemicCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<ArcticCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<ChaoticCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<EternicCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<MagmaticCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<OceanicCrystalTile>());
			GenUtil.UpdateWorld(ModContent.TileType<RelicCrystalTile>());
		}

		// Maybe finalize or do some cleanup here?
		public override void PostWorldGen()
		{
		}

		// Prep ores to be spawned
		public override void PreWorldGen()
		{
		}

		public override void Load(TagCompound tag)
		{
			var spawned = tag.GetList<string>("spawned");

			for (int i = 0; i < SpawnedCrystals.Length; i++)
			{
				SpawnedCrystals[i] = spawned.Contains($"crystal{i}");
			}
		}

		public override TagCompound Save()
		{
			var spawned = new List<string>();

			for (int i = 0; i < SpawnedCrystals.Length; i++)
			{
				if (SpawnedCrystals[i])
				{
					spawned.Add($"crystal{i}");
				}
			}

			return new TagCompound
			{
				["spawned"] = spawned
			};
		}

		public override void NetReceive(BinaryReader reader)
		{
			base.NetReceive(reader);

			BitsByte b0 = reader.ReadByte();

			for (int i = 0; i < SpawnedCrystals.Length; i++)
			{
				SpawnedCrystals[i] = b0[i];
			}

			GenUtil.NetRecieve(reader);
		}

		public override void NetSend(BinaryWriter writer)
		{
			base.NetSend(writer);

			BitsByte b0 = new BitsByte();

			for (int i = 0; i < SpawnedCrystals.Length; i++)
			{
				b0[i] = SpawnedCrystals[i];
			}

			writer.Write(b0);

			GenUtil.NetSend(writer);
		}
	}
}
