using log4net;
using Loot.Core.ContentMod;
using Loot.Core.Recipes;
using Loot.Items;
using Loot.Sounds;
using Loot.Tiles;
using Loot.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

/*
 * original version by hiccup
 * reworked and maintained by jofairden
 * modified and updated by Lulu
 * for tmodloader
 *
 * (c) Lulu 2019
 */

[assembly: InternalsVisibleTo("LootTests")] // Allow doing unit tests
namespace Loot
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	public sealed class Loot : Mod
	{
		internal new static ILog Logger => ((Mod)Instance)?.Logger;
		internal static Loot Instance;
		public static bool CheatSheetLoaded;
		public static bool WingSlotLoaded;
		public static bool WingSlotVersionInvalid;

#if DEBUG
		public override string Name => "Loot";
#endif

		internal CrystalItem crystalItem;

		internal static ContentManager ContentManager;
		public static bool Loaded;

		public Loot()
		{
			Properties = new ModProperties
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}

		public override void Load()
		{
			Instance = this;

			// Ensure cheat sheet loaded and required version
			var cheatSheetMod = ModLoader.GetMod("CheatSheet");
			CheatSheetLoaded = cheatSheetMod != null && cheatSheetMod.Version >= new Version(0, 4, 3, 1);
			var wingSlotMod = ModLoader.GetMod("WingSlot");
			WingSlotLoaded = wingSlotMod != null;
			WingSlotVersionInvalid = WingSlotLoaded && wingSlotMod.Version < new Version(1, 6, 1);

			//(string Name, string test) variable = ("Compiled with", "C#7");

			EMMLoader.Initialize();
			RecipeHandler.Initialize();
			EMMLoader.Load();

			EMMLoader.RegisterMod(this);
			EMMLoader.SetupContent(this);

			if (!Main.dedServ)
			{
				SetupContentMgr();
				EMMLoader.RegisterAssets(this, "GraphicsAssets");
			}
		}

		private void SetupContentMgr()
		{
			ContentManager = new ContentManager();
			ContentManager.Initialize(this);
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			base.PostDrawInterface(spriteBatch);

			if(crystalItem != null)
			{
				if (crystalItem.Activated)
				{
					bool canApply = (Main.HoverItem.IsArmor() || Main.HoverItem.IsAccessory() || Main.HoverItem.IsWeapon());
					float picscale = 1f;
					float textscale = 1.5f;
					string arrow = !Main.HoverItem.IsAir ? (canApply ? ">" : "x") : " ";
					float textpixels = ChatManager.GetStringSize(Main.fontMouseText, arrow, new Vector2(textscale)).X;

					Main.player[Main.myPlayer].showItemIcon = false;
					Main.player[Main.myPlayer].showItemIcon2 = 0;
					Main.mouseText = true;
					float num = Main.inventoryScale;
					Main.inventoryScale = Main.cursorScale;

					Texture2D pic = ModContent.GetTexture(crystalItem.Texture);
					int mouseX = (int)(Main.mouseX - pic.Width * Main.inventoryScale * picscale + textpixels / 2);
					int mouseY = (int)(Main.mouseY + pic.Height * 1.8f * Main.inventoryScale * (picscale * 0.8f));
					Vector2 loc = new Vector2(mouseX, mouseY);

					int mouseX2 = (int)(Main.mouseX * (canApply ? 1f : 1.01f) + textpixels / 2);
					int mouseY2 = (int)(Main.mouseY + pic.Height * 1.5f * Main.inventoryScale * (picscale * 0.8f));
					Vector2 loc2 = new Vector2(mouseX2, mouseY2);
					Color color = canApply ? Color.Green : Color.Red;

					spriteBatch.Draw(pic, loc, pic.Bounds, Color.White, 0f, new Vector2(), picscale, SpriteEffects.None, 0f);
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, arrow, loc2, color, 0f, new Vector2(), new Vector2(1.2f));

					Main.inventoryScale = num;
				}
			}
		}

		public override void PreSaveAndQuit()
		{
			base.PreSaveAndQuit();

			crystalItem = null;
		}

		// AddRecipes() here functions as a PostLoad() hook where all mods have loaded
		public override void AddRecipes()
		{
			if (!Main.dedServ)
			{
				//RecipeHandler.IterateRecipes();
				ContentManager.Load();
			}

			Loaded = true;
		}

		public override void Unload()
		{
			Instance = null;
			GenUtil.UnloadUtil();
			EMMLoader.Unload();

			ContentManager?.Unload();
			ContentManager = null;

			Loaded = false;
		}

		// @TODO this isn't called even tho it says it is. have tried logging, main.newtext, and networktext. nothing happens.
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			//throw new Exception("called");
		}
	}
}
