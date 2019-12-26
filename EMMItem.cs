using Loot.Core;
using Loot.Core.Graphics;
using Loot.Modifiers.EquipModifiers.Defensive;
using Loot.Modifiers.EquipModifiers.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using static Loot.ModifierPlayer;

namespace Loot
{
	//[ComVisible(true)]
	//[Flags]
	//public enum CustomReforgeMode : byte
	//{
	//	Vanilla = 1,
	//	ForceWeapon = 2,
	//	ForceAccessory = 4,
	//	ForceSimulate = 8,
	//	Custom = 16
	//}

	/// <summary>
	/// Defines an item that may be modified by modifiers from mods
	/// </summary>
	public sealed class EMMItem : GlobalItem
	{
		// Helpers
		public static EMMItem GetItemInfo(Item item) => item.GetGlobalItem<EMMItem>();
		public static IEnumerable<Modifier> GetActivePool(Item item) => GetItemInfo(item)?.ModifierPool?.ActiveModifiers ?? Enumerable.Empty<Modifier>();

		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;

		public ModifierPool ModifierPool; // the current pool of mods. null if none.
		public Modifier[] ActiveModifiers = new Modifier[MaximumPossibleModifiers]; // the current active pool of mods (only set when changing them)

		// For Eternic Crystal imprints
		public string ItemHash = "";

		// For Relic Crystal changes
		public static int MaximumPossibleModifiers => 5;
		public int CurrentPossibleModifiers = 2;

		public bool HasRolled; // has rolled a pool
		public bool IsReforging;

		// Non saved
		public bool JustTinkerModified; // is just tinker modified: e.g. armor hacked

		public const int SaveVersion = 2;

		public string RollNewHash(ModifierContext ctx)
		{
			var cinfo = GetItemInfo(ctx.Item);
			cinfo.ItemHash = "";
			cinfo.HasRolled = false;

			string abcs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,./;'[]<>?:\"{}|`~!@#$%^&*()-=_+";

			for (int i = 0; i < 12; i++)
			{
				int rand = Main.rand.Next(0, abcs.Length);
				cinfo.ItemHash += abcs[rand];
			}

			cinfo.HasRolled = true;
			return cinfo.ItemHash;
		}
		
		internal bool RevertPool(ModifierContext ctx)
		{
			EMMItem item = GetItemInfo(ctx.Item);
			EMMItem imprint = GetItemInfo(ctx.Imprint);

			List<Modifier> list = new List<Modifier>();

			if (imprint.ModifierPool != null)
			{
				for (int i = 0; i < imprint.ModifierPool.ActiveModifiers.Length; i++)
				{
					list.Add(imprint.ModifierPool.ActiveModifiers[i]);
				}

				if (item.ModifierPool == null)
				{
					item.ModifierPool = imprint.ModifierPool;
				}

				if (item.ModifierPool != null)
				{
					item.ModifierPool.ActiveModifiers = list.ToArray();
				}

				return list.Any();
			}
			else
			{
				item.ModifierPool = null;
			}

			return false;
		}

		internal ModifierPool AlterPool(ModifierContext ctx, bool roll = false, ItemRollProperties itemRollProperties = null, bool add = false, bool remove = false, int amount = -1, bool revert = false)
		{
			EMMItem item = GetItemInfo(ctx.Item);

			for (int i = 0; i < item.ActiveModifiers.Length; i++)
			{
				// Reset the previous pool so newly crafted/picked up items don't keep modifiers from other items
				if (item.ActiveModifiers[i] != null)
				{
					item.ActiveModifiers[i].locked = false;
					item.ActiveModifiers[i].Properties = null;
					item.ActiveModifiers[i] = null;
				}
			}

			// Remember current active modifiers for the locked variable
			if (item.ModifierPool != null && item.ModifierPool.ActiveModifiers != null)
			{
				for (int i = 0; i < item.ModifierPool.ActiveModifiers.Length; i++)
				{
					// Set the modifiers from the weapon
					Modifier old = (Modifier)item.ModifierPool.ActiveModifiers[i].Clone();
					item.ActiveModifiers[i] = (Modifier)old.Clone();
				}
			}

			if (item.ItemHash == null || item.ItemHash == "")
			{
				item.RollNewHash(ctx);
				Main.NewText("Hash for " + ctx.Item.Name + " was null or empty! Item owned by " + Main.LocalPlayer.name + "!", Color.Red);
			}

			// Now we have actually rolled
			HasRolled = true;

			if (remove)
			{
				ModifierPool = item.ModifierPool;

				// Attempt rolling modifiers
				if (!RemoveModifier(ctx, amount))
				{
					ModifierPool = null; // reset (didn't roll anything)
				}
				else
				{
					// The possible modifiers are unloaded to save memory
					ModifierPool.Modifiers = null;
					ModifierPool.UpdateRarity();
				}

				ctx.Item.GetGlobalItem<ShaderGlobalItem>().NeedsUpdate = true;
				ctx.Item.GetGlobalItem<GlowmaskGlobalItem>().NeedsUpdate = true;
				return ModifierPool;
			}
			else if (revert)
			{
				ModifierPool = item.ModifierPool;

				// Attempt rolling modifiers
				if (!RevertPool(ctx))
				{
					ModifierPool = null; // reset (didn't roll anything)
				}
				else
				{
					// The possible modifiers are unloaded to save memory
					ModifierPool.Modifiers = null;
					ModifierPool.UpdateRarity();
				}

				ctx.Item.GetGlobalItem<ShaderGlobalItem>().NeedsUpdate = true;
				ctx.Item.GetGlobalItem<GlowmaskGlobalItem>().NeedsUpdate = true;
				return ModifierPool;
			}
			else
			{
				// unload previous pool
				item.ModifierPool = null;

				// Default to normal
				if (itemRollProperties == null)
				{
					itemRollProperties = new ItemRollProperties();
				}

				// Rolling logic
				bool noForce = true;

				// Custom roll behavior provided
				if (itemRollProperties.OverrideRollModifierPool != null)
				{
					ModifierPool = itemRollProperties.OverrideRollModifierPool.Invoke();
					noForce = !ModifierPool?._CanRoll(ctx) ?? true;
				}

				// No behavior provided
				if (noForce)
				{
					// A pool is forced to roll
					if (itemRollProperties.ForceModifierPool != null)
					{
						ModifierPool = EMMLoader.GetModifierPool(itemRollProperties.ForceModifierPool.GetType());
						noForce = !ModifierPool?._CanRoll(ctx) ?? true;
					}

					// No pool forced to roll or it's not valid
					if (noForce)
					{
						// Try rolling a predefined (weighted) pool
						bool rollPredefinedPool = Main.rand.NextFloat() <= itemRollProperties.RollPredefinedPoolChance;
						noForce = !rollPredefinedPool;

						if (rollPredefinedPool)
						{
							// GetWeightedPool already checks _CanRoll
							ModifierPool = EMMLoader.GetWeightedPool(ctx);
							noForce = ModifierPool == null || !ModifierPool._CanRoll(ctx);
						}

						// Roll from all modifiers
						if (noForce)
						{
							ModifierPool = Loot.Instance.GetModifierPool<AllModifiersPool>();
							if (!ModifierPool._CanRoll(ctx))
							{
								ModifierPool = null;
								return null;
							}
						}
					}
				}

				if (!ModifierPool.CanRoll(ctx))
				{
					ModifierPool = null;
					return null;
				}

				if (add)
				{
					// Attempt rolling modifiers
					if (!AddModifier(ctx, itemRollProperties, amount))
					{
						ModifierPool = null; // reset (didn't roll anything)
					}
					else
					{
						// The possible modifiers are unloaded to save memory
						ModifierPool.Modifiers = null;
						ModifierPool.UpdateRarity();
					}
				}
				else if (roll)
				{
					// Attempt rolling modifiers
					if (!RollNewModifiers(ctx, itemRollProperties))
					{
						ModifierPool = null; // reset (didn't roll anything)
					}
					else
					{
						// The possible modifiers are unloaded to save memory
						ModifierPool.Modifiers = null;
						ModifierPool.UpdateRarity();
					}
				}
				else
				{
					return null;
				}

				ctx.Item.GetGlobalItem<ShaderGlobalItem>().NeedsUpdate = true;
				ctx.Item.GetGlobalItem<GlowmaskGlobalItem>().NeedsUpdate = true;
				return ModifierPool;
			}
		}

		private bool RemoveModifier(ModifierContext ctx, int amount = -1)
		{
			var item = GetItemInfo(ctx.Item);
			var actives = GetActivePool(ctx.Item);
			List<Modifier> unlocked = new List<Modifier>();
			List<Modifier> locked = new List<Modifier>();

			for (int i = 0; i < actives.Count(); i++)
			{
				if (!actives.ElementAt(i).locked)
				{
					unlocked.Add(actives.ElementAt(i));
				}
				else
				{
					locked.Add(actives.ElementAt(i));
				}
			}

			if (amount > unlocked.Count())
			{
				amount = unlocked.Count();
			}

			if (amount > 0)
			{
				for (int i = 0; i < amount; i++)
				{
					int id = Main.rand.Next(0, unlocked.Count());
					unlocked.RemoveAt(id);
				}
			}

			for (int i = 0; i < unlocked.Count(); i++)
			{
				locked.Add(unlocked.ElementAt(i));
			}

			item.ModifierPool.ActiveModifiers = locked.ToArray();

			return locked.Count() < actives.Count() ? true : locked.Any();
		}

		// Forces the next roll to succeed
		private bool _forceNextRoll;

		/// <summary>
		/// Roll active modifiers, can roll up to n maximum effects
		/// Returns if any modifiers were activated
		/// </summary>
		internal bool RollNewModifiers(ModifierContext ctx, ItemRollProperties itemRollProperties)
		{
			EMMItem item = GetItemInfo(ctx.Item);

			// Prepare a WeightedRandom list with modifiers
			// that are rollable in this context
			WeightedRandom<Modifier> wr = new WeightedRandom<Modifier>();
			List<Modifier> list = new List<Modifier>();

			foreach (var e in ModifierPool.RollableModifiers(ctx))
			{
				wr.Add(e, e.Properties.RollChance);
			}

			float lockedModsForReforgeNerf = 0f;
			// Add previous locked modifiers to where they previously existed
			if (item.ActiveModifiers != null)
			{
				for (int i = 0; i < item.ActiveModifiers.Length; i++)
				{
					if (item.ActiveModifiers[i] != null && item.ActiveModifiers[i].locked)
					{
						lockedModsForReforgeNerf += 1f;

						Modifier orig = item.ActiveModifiers[i];
						Modifier clone = (Modifier)orig.Clone();

						list.Add(clone);

						if (clone.Properties.UniqueModifier)
						{
							wr.elements.RemoveAll(x => x.Item1.Type == clone.Type);
							wr.needsRefresh = true;
						}
					}
				}
			}

			if (item.IsReforging && lockedModsForReforgeNerf != 0f)
			{
				itemRollProperties.RollNextChance /= (lockedModsForReforgeNerf / 2) + 1;
			}

			int maxLines = item.CurrentPossibleModifiers;

			if(itemRollProperties.MaxRollableLines < item.CurrentPossibleModifiers)
			{
				maxLines = itemRollProperties.MaxRollableLines;
			}

			// Check if the number of current modifiers is less than the number of available lines
			if (list.Count() < maxLines)
			{
				// Create a number of max roll lines based on if the list already has some values;
				int newLines = item.CurrentPossibleModifiers - list.Count();

				if (itemRollProperties.MaxRollableLines < item.CurrentPossibleModifiers)
				{
					newLines = itemRollProperties.MaxRollableLines - list.Count();
				}

				// Up to n times, try rolling a mod
				for (int i = 0; i < newLines; ++i)
				{
					// If there are no mods left, or we fail the roll, break.
					if (wr.elements.Count <= 0
						|| !_forceNextRoll
						&& i > 0
						&& list.Count() >= itemRollProperties.MinModifierRolls
						&& Main.rand.NextFloat() > itemRollProperties.RollNextChance)
					{
						break;
					}

					_forceNextRoll = false;

					// Get a next weighted random mod
					// Clone the mod (new instance) and roll it's properties, then roll it
					Modifier e = wr.Get();
					Modifier eClone = (Modifier)e.Clone();
					float luck = itemRollProperties.ExtraLuck;
					if (ctx.Player != null)
					{
						luck += ModifierPlayer.Player(ctx.Player).GetEffect<LuckEffect>().Luck;
					}
					eClone.Properties =
						eClone.GetModifierProperties(ctx.Item)
						.RollMagnitudeAndPower(
								magnitudePower: itemRollProperties.MagnitudePower,
								lukStat: luck);
					eClone.Roll(ctx, list);

					// If the mod deemed to be unable to be added,
					// Force that the next roll is successful
					// (no RNG on top of RNG)
					if (!eClone.PostRoll(ctx, list))
					{
						_forceNextRoll = true;
						continue;
					}

					// The mod can be added
					list.Add(eClone);

					// If it is a unique modifier, remove it from the list to be rolled
					if (eClone.Properties.UniqueModifier)
					{
						wr.elements.RemoveAll(x => x.Item1.Type == eClone.Type);
						wr.needsRefresh = true;
					}
				}
			}

			item.ModifierPool.ActiveModifiers = list.ToArray();
			return list.Any();
		}

		private bool AddModifier(ModifierContext ctx, ItemRollProperties itemRollProperties, int amount)
		{
			var item = GetItemInfo(ctx.Item);

			// Prepare a WeightedRandom list with modifiers
			// that are rollable in this context
			WeightedRandom<Modifier> wr = new WeightedRandom<Modifier>();
			List<Modifier> list = new List<Modifier>();

			// Add all modifiers available to the list
			foreach (var e in ModifierPool.RollableModifiers(ctx))
			{
				wr.Add(e, e.Properties.RollChance);
			}

			// Add previous modifiers in order and remove them from the wr list
			if (item.ActiveModifiers != null)
			{
				for (int i = 0; i < item.ActiveModifiers.Length; i++)
				{
					if (item.ActiveModifiers[i] != null)
					{
						Modifier orig = item.ActiveModifiers[i];
						Modifier clone = (Modifier)orig.Clone();

						list.Add(clone);

						if (clone.Properties.UniqueModifier)
						{
							wr.elements.RemoveAll(x => x.Item1.Type == clone.Type);
							wr.needsRefresh = true;
						}
					}
				}
			}

			int prevListAmount = list.Count();
			
			// Check if the number of current modifiers is less than the number of available lines
			if (list.Count() < item.CurrentPossibleModifiers)
			{
				// Up to n times, try rolling a mod
				for (int i = 0; i < amount; i++)
				{
					// If there are no mods left, or we fail the roll, break.
					if (wr.elements.Count <= 0)
					{
						break;
					}

					// Get a next weighted random mod
					// Clone the mod (new instance) and roll it's properties, then roll it
					Modifier e = wr.Get();
					Modifier eClone = (Modifier)e.Clone();
					float luck = itemRollProperties.ExtraLuck;

					if (ctx.Player != null)
					{
						luck += ModifierPlayer.Player(ctx.Player).GetEffect<LuckEffect>().Luck;
					}

					eClone.Properties =
						eClone.GetModifierProperties(ctx.Item)
						.RollMagnitudeAndPower(
								magnitudePower: itemRollProperties.MagnitudePower,
								lukStat: luck);
					eClone.Roll(ctx, list);

					// The mod can be added
					list.Add(eClone);

					// If it is a unique modifier, remove it from the list
					if (eClone.Properties.UniqueModifier)
					{
						wr.elements.RemoveAll(x => x.Item1.Type == eClone.Type);
						wr.needsRefresh = true;
					}
				}
			}

			item.ModifierPool.ActiveModifiers = list.ToArray();
			return list.Count() > prevListAmount;
		}

		public bool IsTheSameAs(EMMItem item)
		{
			return this.ItemHash == item.ItemHash;
		}

		public void Clone(EMMItem item)
		{
			this.ActiveModifiers = item.ActiveModifiers;
			this.CurrentPossibleModifiers = item.CurrentPossibleModifiers;
			this.HasRolled = item.HasRolled;
			this.ItemHash = item.ItemHash;

			if (item.ModifierPool != null)
			{
				this.ModifierPool = item.ModifierPool;

				if (item.ModifierPool.ActiveModifiers != null)
				{
					this.ModifierPool.ActiveModifiers = item.ModifierPool.ActiveModifiers;
				}
			}
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			EMMItem clone = (EMMItem)base.Clone(item, itemClone);
			clone.ModifierPool = (ModifierPool)ModifierPool?.Clone();
			// there is no need to apply here, we already cloned the item which stats are already modified by its pool
			return clone;
		}

		public override void Load(Item item, TagCompound tag)
		{
			// enforce illegitimate rolls to go away
			if (!ModifierPool.IsValidFor(item))
			{
				ModifierPool = null;
			}
			else if (tag.ContainsKey("Type"))
			{
				ModifierPool = ModifierPool._Load(item, tag);

				// enforce illegitimate rolls to go away
				if (ModifierPool.ActiveModifiers == null || ModifierPool.ActiveModifiers.Length <= 0)
				{
					ModifierPool = null;
				}
			}

			// SaveVersion >= 2
			if (tag.ContainsKey("SaveVersion"))
			{
				HasRolled = tag.GetBool("HasRolled");
				CurrentPossibleModifiers = tag.GetInt("MaxModifiers");
				ItemHash = tag.GetString("ImprintHash");
			}
			else // SaveVersion 1
			{
				HasRolled = tag.GetBool("HasRolled");
			}

			ModifierPool?.ApplyModifiers(item);
		}

		public override TagCompound Save(Item item)
		{
			TagCompound tag =
				ModifierPool != null
				? ModifierPool.Save(item, ModifierPool)
				: new TagCompound();

			// SaveVersion saved since SaveVersion 2, version 1 not present
			tag.Add("SaveVersion", SaveVersion);

			tag.Add("HasRolled", HasRolled);
			tag.Add("MaxModifiers", CurrentPossibleModifiers);
			tag.Add("ImprintHash", ItemHash);

			return tag;
		}

		public override bool NeedsSaving(Item item)
			=> ModifierPool != null || HasRolled;

		public override void NetReceive(Item item, BinaryReader reader)
		{
			if (reader.ReadBoolean())
			{
				ModifierPool = ModifierPool._NetReceive(item, reader);
			}

			HasRolled = reader.ReadBoolean();
			ItemHash = reader.ReadString();
			CurrentPossibleModifiers = reader.ReadInt32();

			ModifierPool?.ApplyModifiers(item);

			JustTinkerModified = reader.ReadBoolean();
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			bool hasPool = ModifierPool != null;
			writer.Write(hasPool);

			if (hasPool)
			{
				ModifierPool._NetSend(ModifierPool, item, writer);
			}

			writer.Write(HasRolled);
			writer.Write(ItemHash);
			writer.Write(CurrentPossibleModifiers);

			writer.Write(JustTinkerModified);
		}

		public override void OnCraft(Item item, Recipe recipe)
		{
			ModifierPool pool = GetItemInfo(item).ModifierPool;

			ModifierContext ctx = new ModifierContext
			{
				Method = ModifierContextMethod.OnCraft,
				Item = item,
				Player = Main.LocalPlayer,
				Recipe = recipe
			};

			if (!HasRolled && pool == null)
			{
				pool = AlterPool(ctx, roll: true);
				pool?.ApplyModifiers(item);
			}

			if (ItemHash == null || ItemHash == "")
			{
				RollNewHash(ctx);
			}

			base.OnCraft(item, recipe);
		}

		public override bool OnPickup(Item item, Player player)
		{
			ModifierPool pool = GetItemInfo(item).ModifierPool;

			ModifierContext ctx = new ModifierContext
			{
				Method = ModifierContextMethod.OnPickup,
				Item = item,
				Player = player
			};

			if (ItemHash == null || ItemHash == "")
			{
				RollNewHash(ctx);
			}

			if (!HasRolled && pool == null)
			{
				pool = AlterPool(ctx, roll: true);
				pool?.ApplyModifiers(item);
			}

			/// Here we call <see cref="ModifierPlayer.OnPickup"/> in case I ever add any OnPickup delegations
			/// such as <see cref="RecoveryRateEffect"/> scaling the heal from hearts
			return base.OnPickup(item, player) && Player(player).OnOnPickup(Player(player), item);
		}

		public override void PostReforge(Item item)
		{
			// This should never fail to succeed since locked modifiers are taken into
			// account within AlterPool and even further within RollNewModifiers
			IsReforging = true;
			ModifierContext ctx = new ModifierContext
			{
				Method = ModifierContextMethod.OnReforge,
				Item = item,
				Player = Main.LocalPlayer
			};

			if (ItemHash == null || ItemHash == "")
			{
				RollNewHash(ctx);
			}

			ModifierPool pool = AlterPool(ctx, roll: true);
			pool?.ApplyModifiers(item);
			IsReforging = false;
		}

		private string GetPrefixNormString(float cpStat, float rStat, ref double num, ref Color? color)
		{
			//float num19 = (float)Main.mouseTextColor / 255f;
			//patch file: num20
			float defColorVal = Main.mouseTextColor / 255f;
			int alphaColor = Main.mouseTextColor;

			if (cpStat == 0f && rStat != 0f)
			{
				num = 1;
				if (rStat > 0f)
				{
					color = new Color((byte)(120f * defColorVal), (byte)(190f * defColorVal), (byte)(120f * defColorVal), alphaColor);
					return "+" + rStat.ToString(CultureInfo.InvariantCulture); /* + Lang.tip[39].Value;*/
				}

				color = new Color((byte)(190f * defColorVal), (byte)(120f * defColorVal), (byte)(120f * defColorVal), alphaColor);
				return rStat.ToString(CultureInfo.InvariantCulture); /* + Lang.tip[39].Value;*/
			}

			double diffStat = rStat - cpStat;
			diffStat = diffStat / cpStat * 100.0;
			diffStat = Math.Round(diffStat);
			num = diffStat;

			// for some reason - is handled automatically, but + is not
			if (diffStat > 0.0)
			{
				color = new Color((byte)(120f * defColorVal), (byte)(190f * defColorVal), (byte)(120f * defColorVal), alphaColor);
				return "+" + diffStat.ToString(CultureInfo.InvariantCulture); /* + Lang.tip[39].Value;*/
			}

			color = new Color((byte)(190f * defColorVal), (byte)(120f * defColorVal), (byte)(120f * defColorVal), alphaColor);
			return diffStat.ToString(CultureInfo.InvariantCulture); /* + Lang.tip[39].Value;*/
																	//if (num12 < 0.0)
																	//{
																	//	array3[num4] = true;
																	//}
		}

		/// <summary>
		/// Will modify vanilla tooltips to add additional information for the affected item's modifiers
		/// </summary>
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			Player(Main.LocalPlayer).ModifyTooltip(Player(Main.LocalPlayer), item, tooltips);

			// the following part, recalculate the vanilla prefix tooltips
			// this is because our mods modify the stats, which was never intended by vanilla, causing the differences to be innacurate and bugged

			// RECALC START
			var vanillaTooltips = tooltips.Where(x => x.mod.Equals("Terraria")).ToArray();
			var baseItem = new Item();
			baseItem.netDefaults(item.netID);

			// the item with just the prefix applied
			var prefixItem = baseItem.Clone();
			prefixItem.Prefix(item.prefix);

			try
			{
				foreach (var vttl in vanillaTooltips)
				{
					//int number = Int32.Parse(new string(vttl.text.Where(char.IsNumber).ToArray()));
					double outNumber = 0d;
					string newTT = vttl.text;
					Color? newC = vttl.overrideColor;
					string TTend = new string(vttl.text.Reverse().ToArray().TakeWhile(x => !char.IsDigit(x)).Reverse().ToArray());

					if (vttl.Name.Equals("PrefixDamage"))
					{
						if (baseItem.damage > 0)
						{
							newTT = GetPrefixNormString(baseItem.damage, prefixItem.damage, ref outNumber, ref newC);
						}
						else
						{
							newTT = GetPrefixNormString(prefixItem.damage, baseItem.damage, ref outNumber, ref newC);
						}
					}
					else if (vttl.Name.Equals("PrefixSpeed"))
					{
						if (baseItem.useAnimation <= 0)
						{
							newTT = GetPrefixNormString(baseItem.useAnimation, prefixItem.useAnimation, ref outNumber, ref newC);
						}
						else
						{
							newTT = GetPrefixNormString(prefixItem.useAnimation, baseItem.useAnimation, ref outNumber, ref newC);
						}
					}
					else if (vttl.Name.Equals("PrefixCritChance"))
					{
						outNumber = prefixItem.crit - baseItem.crit;
						float defColorVal = Main.mouseTextColor / 255f;
						int alphaColor = Main.mouseTextColor;
						newTT = "";
						if (outNumber >= 0)
						{
							newTT += "+";
							newC = new Color((byte)(120f * defColorVal), (byte)(190f * defColorVal), (byte)(120f * defColorVal), alphaColor);
						}
						else
						{
							newC = new Color((byte)(190f * defColorVal), (byte)(120f * defColorVal), (byte)(120f * defColorVal), alphaColor);
						}

						newTT += outNumber.ToString(CultureInfo.InvariantCulture);
					}
					else if (vttl.Name.Equals("PrefixUseMana"))
					{
						if (baseItem.mana != 0)
						{
							float defColorVal = Main.mouseTextColor / 255f;
							int alphaColor = Main.mouseTextColor;
							newTT = GetPrefixNormString(baseItem.mana, prefixItem.mana, ref outNumber, ref newC);
							if (prefixItem.mana < baseItem.mana)
							{
								newC = new Color((byte)(120f * defColorVal), (byte)(190f * defColorVal), (byte)(120f * defColorVal), alphaColor);
							}
							else
							{
								newC = new Color((byte)(190f * defColorVal), (byte)(120f * defColorVal), (byte)(120f * defColorVal), alphaColor);
							}
						}
					}
					else if (vttl.Name.Equals("PrefixSize"))
					{
						if (baseItem.scale > 0)
						{
							newTT = GetPrefixNormString(baseItem.scale, prefixItem.scale, ref outNumber, ref newC);
						}
						else
						{
							newTT = GetPrefixNormString(prefixItem.scale, baseItem.scale, ref outNumber, ref newC);
						}
					}
					else if (vttl.Name.Equals("PrefixShootSpeed"))
					{
						if (baseItem.shootSpeed > 0)
						{
							newTT = GetPrefixNormString(baseItem.shootSpeed, prefixItem.shootSpeed, ref outNumber, ref newC);
						}
						else
						{
							newTT = GetPrefixNormString(prefixItem.shootSpeed, baseItem.shootSpeed, ref outNumber, ref newC);
						}
					}
					else if (vttl.Name.Equals("PrefixKnockback"))
					{
						if (baseItem.knockBack > 0)
						{
							newTT = GetPrefixNormString(baseItem.knockBack, prefixItem.knockBack, ref outNumber, ref newC);
						}
						else
						{
							newTT = GetPrefixNormString(prefixItem.knockBack, baseItem.knockBack, ref outNumber, ref newC);
						}
					}
					else
					{
						continue;
					}

					int ttlI = tooltips.FindIndex(x => x.mod.Equals(vttl.mod) && x.Name.Equals(vttl.Name));
					if (ttlI != -1)
					{
						if (outNumber == 0d)
						{
							tooltips.RemoveAt(ttlI);
						}
						else
						{
							tooltips[ttlI].text = $"{newTT}{TTend}";
							tooltips[ttlI].overrideColor = newC;
						}
					}
				}
			}
			catch (Exception e)
			{
				// Hopefully never happens
				Main.NewTextMultiline(e.ToString());
			}
			// RECALC END

			// Insert maximum possible modifiers
			if(item.IsArmor() || item.IsAccessory() || item.IsWeapon() || item.pick > 0 || item.axe > 0 || item.hammer > 0)
			{
				// Add current maximum modifiers tooltip
				tooltips.Add(new TooltipLine(mod, "Loot: Modifier:Max", $"Available modifier slots: {GetItemInfo(item).CurrentPossibleModifiers}") { overrideColor = Main.selColor });

				// Add imprint hash tooltip
				tooltips.Add(new TooltipLine(mod, "Loot: Modifier:Hash", $"Saved hash: {GetItemInfo(item).ItemHash}") { overrideColor = Color.Gold * Main.inventoryScale });
			}

			var pool = GetItemInfo(item).ModifierPool;
			if (pool != null && pool.ActiveModifiers.Length > 0)
			{
				// Modifies the tooltips, to insert generic mods data
				int i = tooltips.FindIndex(x => x.mod == "Terraria" && x.Name == "ItemName");
				if (i != -1)
				{
					var namelayer = tooltips[i];
					if (pool.Rarity.ItemPrefix != null)
					{
						namelayer.text = $"{pool.Rarity.ItemPrefix} {namelayer.text}";
					}

					if (pool.Rarity.ItemSuffix != null)
					{
						namelayer.text += $" {pool.Rarity.ItemSuffix}";
					}

					if (pool.Rarity.OverrideNameColor != null)
					{
						namelayer.overrideColor = pool.Rarity.OverrideNameColor;
					}

					tooltips[i] = namelayer;
				}


				// Insert modifier rarity
				ActivatedModifierItem activatedModifierItem = ActivatedModifierItem.Item(item);
				bool isVanityIgnored = activatedModifierItem.ShouldBeIgnored(item, Main.LocalPlayer);
				
				Color? inactiveColor = isVanityIgnored ? (Color?)Color.DarkSlateGray : null;

				i = tooltips.Count;

				// Insert maximum possible modifiers
				//tooltips.Insert(i, new TooltipLine(mod, "Loot: Modifier:Max", $"Available modifier slots: {CurrentPossibleModifiers}") { overrideColor = inactiveColor ?? Color.White * Main.inventoryScale });

				// Insert rarity name
				tooltips.Insert(i, new TooltipLine(mod, "Loot: Modifier:Rarity", $"[{pool.Rarity.Name}]{(isVanityIgnored ? " [IGNORED]" : "")}") { overrideColor = inactiveColor ?? pool.Rarity.Color * Main.inventoryScale });
				int j = 0;

				// Insert modifier lines
				foreach (var ttcol in pool.Description)
				{
					foreach (var tt in ttcol)
					{
						tooltips.Insert(++i, new TooltipLine(mod, $"Loot: Modifier:Line:{i}", (pool.ActiveModifiers[j].locked ? "[x] " : "[ ] ") + tt.Text) { overrideColor = inactiveColor ?? (tt.Color ?? Color.White) * Main.inventoryScale });
						j++;
					}
				}

				// Call modify tooltips
				foreach (var e in pool.ActiveModifiers)
				{
					e.ModifyTooltips(item, tooltips);
				}
			}
		}
	}
}
