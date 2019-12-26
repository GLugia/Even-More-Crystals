using Loot.Core;
using Loot.Sounds;
using Loot.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Loot.Items
{
	public abstract class CrystalItem : ModItem
	{
		/// <summary>
		/// The hover name of the crystal.
		/// </summary>
		internal abstract string CrystalName { get; }

		/// <summary>
		/// The crystal's usage. 
		/// <see cref="SetStaticDefaults"/> sets a proper tooltip default for this item using this variable.
		/// </summary>
		protected virtual string Usage { get; }

		/// <summary>
		/// The name color of the crystal.
		/// </summary>
		protected virtual Color? OverrideNameColor => null;

		/// <summary>
		/// Automatically pulls the crystal's texture.
		/// @TODO add optional override to this
		/// </summary>
		public sealed override string Texture => GetType().Namespace.Replace('.', '/') + "/assets/" + GetType().Name;

		/// <summary>
		/// If the item is placeable. Returns true by default.
		/// </summary>
		protected virtual bool Placeable => true;

		/// <summary>
		/// The pick resistance the crystal has
		/// </summary>
		public virtual int Resistance => 25;

		/// <summary>
		/// Whether the crystal has been activated via CTRL+Right Click or not.
		/// </summary>
		public bool Activated = false;

		/// <summary>
		/// The hovered item. Always set so long as the crystal is activated. Otherwise null.
		/// </summary>
		protected Item hoverItem = null;

		/// <summary>
		/// Sets the name and tooltip of the crystal. <see cref="SafeStaticDefaults"/> to set your own or override any necessary.
		/// </summary>
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			DisplayName.SetDefault(CrystalName);
			Tooltip.SetDefault
				(
						"Hold Left Control and right click to activate"
					+ "\nThen right click an item to " + Usage
					+ "\nHold Left Control for repeated use"
				);
			SafeStaticDefaults();
		}

		/// <summary>
		/// Sets the defaults of the crystal. <see cref="Placeable"/> defines the majority of the properties set.
		/// <seealso cref="SafeDefaults"/> to set your own or override any necessary.
		/// </summary>
		public sealed override void SetDefaults()
		{
			item.maxStack = 999;
			item.value = Item.sellPrice(0, 0, 0, 0);

			if (Placeable)
			{
				item.createTile = ModContent.TileType<CrystalTile>();
				item.consumable = true;
				item.useStyle = 1;
				item.useTime = 10;
				item.useAnimation = 15;
				item.useTurn = true;
				item.autoReuse = true;
				item.placeStyle = 0;
			}

			item.width = 16;
			item.height = 16;

			item.rare = 3;

			SafeDefaults();
		}

		public virtual void SafeStaticDefaults() { }
		public virtual void SafeDefaults() { }

		/// <summary>
		/// Loads the basic tags necessary for the crystal to function. <see cref="LoadInfo(TagCompound, string)"/> to load your own tags.
		/// </summary>
		/// <param name="tag"></param>
		public sealed override void Load(TagCompound tag)
		{
			string b = "LootCrystal"; // For future additions to variables requiring saving

			if (tag.GetInt(b + "Items") == 2)
			{
				//heldItem = ItemLoader.GetItem(tag.GetInt(b + "Held")).item;
				Activated = tag.GetBool(b + "Activation");
				LoadInfo(tag, b);
			}
		}

		/// <summary>
		/// Loads tags saved by extensions of this class.
		/// Always load objects using theBase + object name as a string.
		/// </summary>
		/// <param name="tag"> The TagCompound used to load </param>
		/// <param name="theBase"> The base string is always "LootCrystal" </param>
		public virtual void LoadInfo(TagCompound tag, string theBase) { }

		public override void NetRecieve(BinaryReader reader)
		{
			Activated = reader.ReadBoolean();
		}

		/// <summary>
		/// Saves the basic information about the crystal. <see cref="SaveInfo(TagCompound, string)"/> to save your own tags.
		/// </summary>
		/// <returns></returns>
		public sealed override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			string b = "LootCrystal";

			tag.Add(b + "Items", 2);
			tag.Add(b + "Activation", Activated);

			SaveInfo(tag, b);

			return tag;
		}

		/// <summary>
		/// Saves tags necessary from extensions of this class.
		/// Always save objects using theBase + object name as a string.
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="theBase"></param>
		public virtual void SaveInfo(TagCompound tag, string theBase) { }

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(Activated);
		}

		/// <summary>
		/// This should never be changed. Allows right clicking to activate the crystal only when the mouse item is empty.
		/// <para>This solves a huge bug where some crystals were disappearing when right clicked while a different crystal was in the mouse item.</para>
		/// </summary>
		/// <returns></returns>
		public sealed override bool CanRightClick() => Main.mouseItem.IsAir;

		/// <summary>
		/// The main activation code happens here.
		/// </summary>
		/// <param name="player"> The player in this case </param>
		public sealed override void RightClick(Player player)
		{
			if (Main.playerInventory
				&& Main.mouseRight
				&& Main.keyState.IsKeyDown(Keys.LeftControl))
			{
				if (Main.mouseRightRelease)
				{
					if (!Activated)
					{
						if (Loot.Instance.crystalItem != null) // Remove the old crystal identity
						{
							Loot.Instance.crystalItem.Activated = false;
							Loot.Instance.crystalItem = null;
						}

						SoundHelper.PlayCustomSound(SoundHelper.SoundType.Notif); // Activation sound
						Loot.Instance.crystalItem = this; // Create a new crystal identity
						PreActivation(player, item); // Preactivation hook called
						OnActivation(player, item); // OnActivation hook called
						PostActivation(player, item); // PostActivation hook called
						item.stack++; // Increment the item stack so right clicking doesn't decrement
						Main.mouseRightRelease = false; // Force release the right mouse button to avoid bugs
						return; // Immediately stop here
					}

					if (Activated)
					{
						SoundHelper.PlayCustomSound(SoundHelper.SoundType.LoseSeal); // Deactivation sound
						PreActivation(player, item); // PreActivation hook called
						OnActivation(player, item); // OnActivation hook called
						PostActivation(player, item); // PostActivation hook called
						item.stack++; // Increment the item stack so right clicking doesn't decrement
						Loot.Instance.crystalItem = null; // Remove the crystal identity
						Main.mouseRightRelease = false; // Force release the right mouse button to avoid bugs
						return; // Immediately stop here
					}
				}
			}
			else if (Main.playerInventory) // if the player is still in inventory
			{
				if (Main.mouseItem.IsAir) // if the mouse item doesn't exist
				{
					Item item = new Item(); // create a new item
					item.SetDefaults(this.item.type); // based on this item
					var info = EMMItem.GetItemInfo(this.item); // grab existing global info
					var ninfo = EMMItem.GetItemInfo(item); // create new global info
					ninfo.Clone(info); // clone the info from existing to new
					Main.mouseItem = item; // set the mouse item to the new item
					return; // immediately stop here
				}
			}
			else
			{
				Loot.Instance.crystalItem = null;
				Main.NewText("An error occurred in CrystalItem", Color.Red);
			}
		}

		/// <summary>
		/// The crystal's magic happens here for the most part.
		/// </summary>
		/// <param name="player"> The player in this case </param>
		public sealed override void UpdateInventory(Player player)
		{
			base.UpdateInventory(player);

			if (Loot.Instance.crystalItem != null && Activated)
			{
				// Update the hovered item every tick
				hoverItem = Main.HoverItem;

				// Deactivate if the player right clicks a null item
				if (hoverItem == null
					|| hoverItem.IsAir
					&& Main.mouseRight)
				{
					if (Main.mouseRightRelease)
					{
						Activated = false;
						Loot.Instance.crystalItem = null;
						SoundHelper.PlayCustomSound(SoundHelper.SoundType.Decline);
						Main.mouseRightRelease = false;
						return;
					}
				}

				// Deactivate if the player has a mouse item
				if (!Main.mouseItem.IsAir)
				{
					Activated = false;
					Loot.Instance.crystalItem = null;
					SoundHelper.PlayCustomSound(SoundHelper.SoundType.Decline);
					return;
				}

				// Deactivate if the player closes the inventory
				if (!Main.playerInventory)
				{
					Activated = false;
					Loot.Instance.crystalItem = null;
					return;
				}

				// Lots of things to check here but all are required to avoid bugs and exploits
				if (hoverItem != null
					&& Loot.Instance.crystalItem != null
					&& !hoverItem.IsAir
					&& hoverItem.type != item.type
					&& hoverItem.type != Loot.Instance.crystalItem.item.type
					&& Main.mouseRight)
				{
					if (Main.mouseRightRelease
					&& (hoverItem.IsWeapon() || hoverItem.IsArmor() || hoverItem.IsAccessory() || hoverItem.axe > 0 || hoverItem.pick > 0))
					{
						PreModifyItem(player, hoverItem); // Call pre modify item hook

						if (ModifyItem(player, hoverItem)) // Check if modify item hook returns true
						{
							SoundHelper.PlayCustomSound(SoundHelper.SoundType.Redeem); // play confirm sound
							item.stack--; // take from stack
							PostModifyItem(player, hoverItem); // post modify hook

							// only deactivate if noy holding control or if the item stack is 0
							if (!Main.keyState.IsKeyDown(Keys.LeftControl) || item.stack == 0)
							{
								Activated = false;
								Loot.Instance.crystalItem = null;
							}

							Main.mouseRightRelease = false; // force release mouse right button to avoid bugs
							return;
						}
						else // play deactivate sound and nullify the instance
						{
							SoundHelper.PlayCustomSound(SoundHelper.SoundType.Decline);
							Activated = false;
							Loot.Instance.crystalItem = null;
							Main.mouseRightRelease = false;
							return;
						}
					}
				}
			}
			else
			{
				Activated = false;
				return;
			}
		}

		/// <summary>
		/// Set up any necessary variables for modifications to take place
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hover item being modified </param>
		protected virtual void PreModifyItem(Player player, Item item) { }

		/// <summary>
		/// Main modifications to items happens here
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hovered item being modified </param>
		/// <returns></returns>
		protected virtual bool ModifyItem(Player player, Item item) { return false; }

		/// <summary>
		/// General server syncing typically happens here
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hovered item being modified </param>
		protected virtual void PostModifyItem(Player player, Item item)
		{
			if (Main.netMode == 2)
			{
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.type);
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, this.item.type);
			}
		}

		/// <summary>
		/// Called before activating the crystal
		/// Typically used for attempting to ignore the typical "pick up the item with right click" code vanilla has
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hovered crystal being activated </param>
		protected virtual void PreActivation(Player player, Item item)
		{
			if (Main.mouseItem != null
				&& Loot.Instance.crystalItem != null
				&& Main.mouseItem.type == Loot.Instance.crystalItem.item.type)
			{
				Main.mouseItem.TurnToAir();
			}
		}

		/// <summary>
		/// Activates the crystal. Do as you please with this if necessary
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hovered crystal being activated </param>
		protected virtual void OnActivation(Player player, Item item)
		{
			if (Loot.Instance.crystalItem != null)
			{
				Activated = !Activated;
			}
		}

		/// <summary>
		/// Do any necessary post-activation things here
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="item"> The hovered crystal being activated </param>
		protected virtual void PostActivation(Player player, Item item)
		{
		}

		/// <summary>
		/// The roll properties of the crystal. Returns null if not overwritten.
		/// </summary>
		/// <returns></returns>
		protected virtual ItemRollProperties RollLogic() { return null; }

		/// <summary>
		/// Applies the modifier pool to the item and replaces the item in the inventory or armor slots.
		/// Recommended to create your own item and EMMItem clones and pass the new item through instead.
		/// This replaces the item even if the modifier pool returns null. This way more interesting modifications can happen if necessary.
		/// <see cref="ModifyItem(Player, Item)"/> will generally return this unless impossible.
		/// </summary>
		/// <param name="player"> The player in this case </param>
		/// <param name="pool"> The pool returned from <see cref="EMMItem.AlterPool(ModifierContext, bool, ItemRollProperties, bool, bool, int, bool)"/> </param>
		/// <param name="item"> The new cloned item </param>
		/// <returns></returns>
		protected bool ApplyModifiers(Player player, ModifierPool pool, Item item)
		{
			if(item != null)
			{
				if (pool != null)
				{
					pool?.ApplyModifiers(item);
				}

				var b = EMMItem.GetItemInfo(hoverItem);

				for(int i = 0; i < player.inventory.Length; i++)
				{
					if (player.inventory[i].active && player.inventory[i].stack > 0 && player.inventory[i].type > 0)
					{
						var a = EMMItem.GetItemInfo(player.inventory[i]);

						if (a != null && b != null)
						{
							if (b.IsTheSameAs(a) && player.inventory[i].IsTheSameAs(hoverItem))
							{
								player.inventory[i] = item;
								hoverItem = player.inventory[i];

								if (Main.netMode == 2)
								{
									NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.type);
								}

								return true;
							}
						}
					}
				}

				for (int i = 0; i < player.armor.Length; i++)
				{
					if (player.armor[i].active && player.armor[i].stack > 0 && player.armor[i].type > 0)
					{
						var a = EMMItem.GetItemInfo(player.armor[i]);

						if (a != null && b != null)
						{
							if (b.IsTheSameAs(a) && player.armor[i].IsTheSameAs(hoverItem))
							{
								player.armor[i] = item;
								hoverItem = player.armor[i];

								if (Main.netMode == 2)
								{
									NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.type);
								}

								return true;
							}
						}
					}
				}

				NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("Failed to find item " + item.Name + " in " + Main.LocalPlayer.name + "'s inventory"), Color.Red);
				return false;
			}

			Main.NewText(item.Name + " cannot be modified");
			return false;
		}

		/// <summary>
		/// Adds the name color change here. <see cref="AddTooltip(List{TooltipLine})"/> to modify tooltips further.
		/// </summary>
		/// <param name="tooltips"></param>
		public sealed override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);

			var tts = tooltips.Where(x => x.mod.Equals("Terraria"));

			if (OverrideNameColor != null)
			{
				var itemName = tts.FirstOrDefault(x => x.Name.Equals("ItemName"));

				if (itemName != null)
				{
					itemName.overrideColor = OverrideNameColor.Value;
				}
			}

			AddTooltip(tooltips);
		}

		/// <summary>
		/// Edit the tooltip of the crystal.
		/// </summary>
		/// <param name="tooltips"> The list of tooltips </param>
		protected virtual void AddTooltip(List<TooltipLine> tooltips) { }

		/// <summary>
		/// @TODO add recipe system
		/// </summary>
		public sealed override void AddRecipes()
		{
		}
	}
}
