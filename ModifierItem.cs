using Loot.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Loot
{
	public sealed class ActivatedModifierItem : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;

		/// <summary>
		/// Keeps track of if the particular item was activated ('delegated')
		/// Specific usecase see CursedEffect and modifier
		/// </summary>
		public bool IsActivated { get; internal set; }

		/// <summary>
		/// Keeps track of if the item was activated whilst in a vanity slot (by another mod)
		/// In this case activated means giving its regular bonuses
		/// </summary>
		public bool IsCheated { get; internal set; }

		public bool ShouldBeIgnored(Item item, Player player)
		{
			return !IsCheated && IsInVanitySot(item, player)
					|| IsCheated && !IsInVanitySot(item, player) && !IsInInventory(item, player);
		}

		public bool IsInInventory(Item item, Player player)
		{
			return player.inventory.Any(x => x.IsTheSameAs(item));
		}

		/// <summary>
		/// Returns if the item is in a player's vanity slot
		/// </summary>
		public bool IsInVanitySot(Item item, Player player)
		{
			return player.armor.Skip(10).Any(x => x.IsTheSameAs(item));
		}

		private bool IsNotEquippedAtAll(Item item, Player player)
		{
			return !player.armor.Any(x => x.IsTheSameAs(item));
		}

		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			if (IsInVanitySot(item, player) || IsNotEquippedAtAll(item, player))
			{
				IsCheated = true;
			}
			else
			{
				IsCheated = false;
			}
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (IsInVanitySot(item, player) || IsNotEquippedAtAll(item, player))
			{
				IsCheated = true;
			}
			else
			{
				IsCheated = false;
			}
		}

		public static ActivatedModifierItem Item(Item item) => item.GetGlobalItem<ActivatedModifierItem>();
	}

	/// <summary>
	/// Calls GlobalItem hooks on modifiers
	/// </summary>
	public sealed class ModifierItem : GlobalItem
	{
		public override bool InstancePerEntity => false;
		public override bool CloneNewInstances => false;
		private ActivatedModifierItem MItem(Item item) => ActivatedModifierItem.Item(item);

		public override bool AltFunctionUse(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.AltFunctionUse(item, player);
			}

			bool b = base.AltFunctionUse(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.AltFunctionUse(item, player);
			}

			return b;
		}

		public override bool CanEquipAccessory(Item item, Player player, int slot)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.CanEquipAccessory(item, player, slot);
			}

			bool b = base.CanEquipAccessory(item, player, slot);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanEquipAccessory(item, player, slot);
			}

			return b;
		}

		public override bool? CanHitNPC(Item item, Player player, NPC target)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.CanHitNPC(item, player, target);
			}

			bool? b = base.CanHitNPC(item, player, target);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanHitNPC(item, player, target);
			}

			return b;
		}

		public override bool CanHitPvp(Item item, Player player, Player target)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.CanHitPvp(item, player, target);
			}

			bool b = base.CanHitPvp(item, player, target);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanHitPvp(item, player, target);
			}

			return b;
		}

		public override bool CanPickup(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.CanPickup(item, player);
			}

			bool b = base.CanPickup(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanPickup(item, player);
			}

			return b;
		}

		public override bool CanRightClick(Item item)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.CanRightClick(item);
			}

			bool b = base.CanRightClick(item);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanRightClick(item);
			}

			return b;
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.CanUseItem(item, player);
			}

			bool b = base.CanUseItem(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.CanUseItem(item, player);
			}

			return b;
		}

		public override int ChoosePrefix(Item item, UnifiedRandom rand)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.ChoosePrefix(item, rand);
			}

			int p = base.ChoosePrefix(item, rand);
			if (p != -1)
			{
				return p;
			}

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				p = m.ChoosePrefix(item, rand);
				// TODO which modifier takes precedence ?
				if (p != -1)
				{
					return p;
				}
			}

			return -1;
		}

		public override bool ConsumeAmmo(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.ConsumeAmmo(item, player);
			}

			bool b = base.ConsumeAmmo(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.ConsumeAmmo(item, player);
			}

			return b;
		}

		public override bool ConsumeItem(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.ConsumeItem(item, player);
			}

			bool b = base.ConsumeItem(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.ConsumeItem(item, player);
			}

			return b;
		}

		public override Color? GetAlpha(Item item, Color lightColor)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.GetAlpha(item, lightColor);
			}

			Color? a = base.GetAlpha(item, lightColor);
			if (a.HasValue)
			{
				return a;
			}

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				a = m.GetAlpha(item, lightColor);
				// TODO which modifier takes precedence ?
				if (a.HasValue)
				{
					return a;
				}
			}

			return null;
		}

		public override void GetWeaponCrit(Item item, Player player, ref int crit)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.GetWeaponCrit(item, player, ref crit);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.GetWeaponCrit(item, player, ref crit);
			}
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float multi, ref float flat)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);
			}
		}

		public override void GetWeaponKnockback(Item item, Player player, ref float knockback)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.GetWeaponKnockback(item, player, ref knockback);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.GetWeaponKnockback(item, player, ref knockback);
			}
		}

		public override void GrabRange(Item item, Player player, ref int grabRange)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.GrabRange(item, player, ref grabRange);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.GrabRange(item, player, ref grabRange);
			}
		}

		public override bool GrabStyle(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.GrabStyle(item, player);
			}

			bool b = base.GrabStyle(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.GrabStyle(item, player);
			}

			return b;
		}

		public override void HoldItem(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.HoldItem(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.HoldItem(item, player);
			}
		}

		public override bool HoldItemFrame(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.HoldItemFrame(item, player);
			}

			bool b = base.HoldItemFrame(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.HoldItemFrame(item, player);
			}

			return b;
		}

		public override void HoldStyle(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.HoldStyle(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.HoldStyle(item, player);
			}
		}

		public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.HorizontalWingSpeeds(item, player, ref speed, ref acceleration);
			}
		}

		public override bool ItemSpace(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.ItemSpace(item, player);
			}

			bool b = base.ItemSpace(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.ItemSpace(item, player);
			}

			return b;
		}

		public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.MeleeEffects(item, player, hitbox);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.MeleeEffects(item, player, hitbox);
			}
		}

		public override float MeleeSpeedMultiplier(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.MeleeSpeedMultiplier(item, player);
			}

			float mult = base.MeleeSpeedMultiplier(item, player);
			if (mult != 1f)
			{
				return mult;
			}

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				mult = m.MeleeSpeedMultiplier(item, player);
				// TODO which modifier takes precedence ?
				if (mult != 1f)
				{
					return mult;
				}
			}

			return 1f;
		}

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.ModifyHitNPC(item, player, target, ref damage, ref knockBack, ref crit);
			}
		}

		public override void ModifyHitPvp(Item item, Player player, Player target, ref int damage, ref bool crit)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.ModifyHitPvp(item, player, target, ref damage, ref crit);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.ModifyHitPvp(item, player, target, ref damage, ref crit);
			}
		}

		public override bool NewPreReforge(Item item)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.NewPreReforge(item);
			}

			bool b = base.NewPreReforge(item);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.NewPreReforge(item);
			}

			return b;
		}

		public override void OnCraft(Item item, Recipe recipe)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.OnCraft(item, recipe);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.OnCraft(item, recipe);
			}
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.OnHitNPC(item, player, target, damage, knockBack, crit);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.OnHitNPC(item, player, target, damage, knockBack, crit);
			}
		}

		public override void OnHitPvp(Item item, Player player, Player target, int damage, bool crit)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.OnHitPvp(item, player, target, damage, crit);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.OnHitPvp(item, player, target, damage, crit);
			}
		}
		public override bool OnPickup(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.OnPickup(item, player);
			}

			bool b = base.OnPickup(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.OnPickup(item, player);
			}

			return b;
		}

		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref int damage, ref float knockback)
		{
			if (MItem(weapon).ShouldBeIgnored(weapon, player))
			{
				return;
			}

			base.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);

			foreach (Modifier m in EMMItem.GetActivePool(weapon))
			{
				m.PickAmmo(weapon, ammo, player, ref type, ref speed, ref damage, ref knockback);
			}
		}

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}
		}

		public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
			}
		}

		public override void PostDrawTooltip(Item item, ReadOnlyCollection<DrawableTooltipLine> lines)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostDrawTooltip(item, lines);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostDrawTooltip(item, lines);
			}
		}

		public override void PostDrawTooltipLine(Item item, DrawableTooltipLine line)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostDrawTooltipLine(item, line);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostDrawTooltipLine(item, line);
			}
		}

		public override void PostReforge(Item item)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostReforge(item);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostReforge(item);
			}
		}

		public override void PostUpdate(Item item)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.PostUpdate(item);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.PostUpdate(item);
			}
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}

			bool b = base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
			}

			return b;
		}

		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
			}

			bool b = base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
			}

			return b;
		}

		public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.PreDrawTooltip(item, lines, ref x, ref y);
			}

			bool b = base.PreDrawTooltip(item, lines, ref x, ref y);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.PreDrawTooltip(item, lines, ref x, ref y);
			}

			return b;
		}

		public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.PreDrawTooltipLine(item, line, ref yOffset);
			}

			bool b = base.PreDrawTooltipLine(item, line, ref yOffset);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.PreDrawTooltipLine(item, line, ref yOffset);
			}

			return b;
		}

		public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
			}

			bool b = base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
			}

			return b;
		}

		public override void RightClick(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.RightClick(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.RightClick(item, player);
			}
		}

		public override void SetDefaults(Item item)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.SetDefaults(item);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.SetDefaults(item);
			}
		}

		public override bool Shoot(Item item, Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
			};

			bool b = base.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.Shoot(item, player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
			}

			return b;
		}

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			if (MItem(item).ShouldBeIgnored(item, Main.LocalPlayer))
			{
				return;
			}

			base.Update(item, ref gravity, ref maxFallSpeed);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.Update(item, ref gravity, ref maxFallSpeed);
			}
		}

		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.UpdateAccessory(item, player, hideVisual);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.UpdateAccessory(item, player, hideVisual);
			}
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.UpdateEquip(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.UpdateEquip(item, player);
			}
		}

		public override void UpdateInventory(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.UpdateInventory(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.UpdateInventory(item, player);
			}
		}

		public override bool UseItem(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.UseItem(item, player);
			}

			bool b = base.UseItem(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.UseItem(item, player);
			}

			return b;
		}

		public override bool UseItemFrame(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.UseItemFrame(item, player);
			}

			bool b = base.UseItemFrame(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				b &= m.UseItemFrame(item, player);
			}

			return b;
		}

		public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.UseItemHitbox(item, player, ref hitbox, ref noHitbox);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
			}
		}

		public override void UseStyle(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.UseStyle(item, player);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.UseStyle(item, player);
			}
		}

		public override float UseTimeMultiplier(Item item, Player player)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return base.UseTimeMultiplier(item, player);
			}

			float f = base.UseTimeMultiplier(item, player);
			if (f != 1f)
			{
				return f;
			}

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				f = m.UseTimeMultiplier(item, player);
				// TODO which modifier takes precedence ?
				if (f != 1f)
				{
					return f;
				}
			}

			return f;
		}

		public override void VerticalWingSpeeds(Item item, Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
		{
			if (MItem(item).ShouldBeIgnored(item, player))
			{
				return;
			}

			base.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);

			foreach (Modifier m in EMMItem.GetActivePool(item))
			{
				m.VerticalWingSpeeds(item, player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
			}
		}
	}
}
