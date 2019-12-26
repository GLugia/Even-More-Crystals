using Loot.Core;
using Loot.Modifiers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Loot
{
	/// <summary>
	/// Holds player-entity data and handles it
	/// </summary>
	public class ModifierPlayer : ModPlayer
	{
		private IList<ModifierEffect> _modifierEffects;

		public bool HasEffect(Type type) => _modifierEffects.Any(x => x.GetType() == type);
		public bool HasEffect<T>() where T : ModifierEffect => _modifierEffects.Any(x => x.GetType() == typeof(T));

		public ModifierEffect GetEffect(Type type)
		{
			return _modifierEffects.FirstOrDefault(x => x.GetType() == type);
		}

		public T GetEffect<T>() where T : ModifierEffect
		{
			return (T)_modifierEffects.FirstOrDefault(x => x.GetType() == typeof(T));
		}

		// Attempt rolling modifiers on first load
		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath)
		{
			EMMWorld.WorldGenModifiersPass.GenerateModifiers(null, ModifierContextMethod.SetupStartInventory, items.Where(x => !x.IsAir && x.IsModifierRollableItem()), player);
		}

		public override void OnEnterWorld(Player player)
		{
			EMMWorld.WorldGenModifiersPass.GenerateModifiers(null, ModifierContextMethod.FirstLoad, player.inventory.Where(x => !x.IsAir && x.IsModifierRollableItem()).Concat(player.armor.Where(x => !x.IsAir && !x.IsModifierRollableItem())), player);
		}

		public void ApplyDamageToNPC(NPC target, int damage, float knockback, int direction, bool crit, bool dot)
		{
			ModifierNpc.StrikeNPC(target, damage, knockback, direction, crit, dot: dot);

			if (Main.netMode != 0)
			{
				// @TODO look for a way to send combattext data seperately from strikenpc
				NetMessage.SendData(MessageID.StrikeNPC, -1, -1, null, target.whoAmI, damage, knockback, direction, crit.ToInt());
			}

			int num = Item.NPCtoBanner(target.BannerID());

			if (num >= 0)
			{
				player.lastCreatureHit = num;
			}
		}

		public void ApplyDamageToPlayer(Player target, int damage, int direction, bool crit, bool pvp, bool dot)
		{
			ModifierNpc.StrikePlayer(target, PlayerDeathReason.ByPlayer(player.whoAmI), damage, direction, pvp, false, crit, -1, dot);
		}

		public static ModifierPlayer Player(Player player) => player.GetModPlayer<ModifierPlayer>();

		public delegate void VoidEventRaiser(ModifierPlayer player);

		public event VoidEventRaiser OnInitialize;
		public override void Initialize()
		{
			// Initialize the effects list for this player
			_modifierEffects = new List<ModifierEffect>();
			// Need to initialize with a fresh set of new effect instances
			foreach (var effect in EMMLoader.Effects.Select(x => x.Value))
			{
				var clone = (ModifierEffect)effect.Clone();
				clone.OnInitialize(this);
				_modifierEffects.Add(clone);
			}

			OnInitialize?.Invoke(this);
		}

		public event VoidEventRaiser OnPreUpdate;
		public override void PreUpdate()
		{
			OnPreUpdate?.Invoke(this);
		}

		public event VoidEventRaiser OnResetEffects;
		public override void ResetEffects()
		{
			if (player.GetModPlayer<ModifierCachePlayer>().Ready)
			{
				OnResetEffects?.Invoke(this);
			}
		}

		public event VoidEventRaiser OnUpdateLifeRegen;
		public override void UpdateLifeRegen()
		{
			OnUpdateLifeRegen?.Invoke(this);
		}

		public event VoidEventRaiser OnUpdateBadLifeRegen;
		public override void UpdateBadLifeRegen()
		{
			OnUpdateBadLifeRegen?.Invoke(this);
		}

		public event VoidEventRaiser OnPostUpdateEquips;
		public override void PostUpdateEquips()
		{
			OnPostUpdateEquips?.Invoke(this);
		}

		public event VoidEventRaiser OnPostUpdate;
		public override void PostUpdate()
		{
			OnPostUpdate?.Invoke(this);
		}

		public delegate bool OnPickupEvent(ModifierPlayer player, Item item);
		public event OnPickupEvent OnPickup;
		public bool OnOnPickup(ModifierPlayer player, Item item)
		{
			if(OnPickup != null)
			{
				return OnPickup.Invoke(this, item);
			}

			return true;
		}

		public delegate void TooltipModifierEvent(ModifierPlayer player, Item item, List<TooltipLine> tooltip);
		public event TooltipModifierEvent TooltipModifier;
		public void ModifyTooltip(ModifierPlayer player, Item item, List<TooltipLine> tooltip)
		{
			TooltipModifier?.Invoke(this, item, tooltip);
		}

		public delegate void OnPreUpdateBuffsEvent(ModifierPlayer player);
		public event OnPreUpdateBuffsEvent OnPreUpdateBuffs;
		public override void PreUpdateBuffs()
		{
			OnPreUpdateBuffs?.Invoke(this);
		}

		public delegate void OnPostUpdateBuffsEvent(ModifierPlayer player);
		public event OnPostUpdateBuffsEvent OnPostUpdateBuffs;
		public override void PostUpdateBuffs()
		{
			OnPostUpdateBuffs?.Invoke(this);
		}

		public delegate void OnHealLifeEventRaiser(ModifierPlayer player, Item item, bool quickHeal, ref int healValue);
		public event OnHealLifeEventRaiser OnHealLife;
		public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
		{
			OnHealLife?.Invoke(this, item, quickHeal, ref healValue);
		}

		public delegate void OnHealManaEventRaiser(ModifierPlayer player, Item item, bool quickMana, ref int manaValue);
		public event OnHealManaEventRaiser OnHealMana;
		public override void GetHealMana(Item item, bool quickMana, ref int manaValue)
		{
			OnHealMana?.Invoke(this, item, quickMana, ref manaValue);
		}

		public delegate bool PreHurtEventRaiser(ModifierPlayer player, bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
		public event PreHurtEventRaiser OnPreHurt;
		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			bool b = base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
			if (OnPreHurt != null)
			{
				b &= OnPreHurt.Invoke(this, pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource);
			}

			return b;
		}

		public delegate void PostHurtEventRaiser(ModifierPlayer player, bool pvp, bool quiet, double damage, int hitDirection, bool crit);
		public event PostHurtEventRaiser OnPostHurt;
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			OnPostHurt?.Invoke(this, pvp, quiet, damage, hitDirection, crit);
		}

		public delegate void ModifyHitNPCEventRaiser(ModifierPlayer player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit);
		public event ModifyHitNPCEventRaiser OnModifyHitNPC;
		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			OnModifyHitNPC?.Invoke(this, item, target, ref damage, ref knockback, ref crit);
		}

		public delegate void ModifyHitNPCWithProjEvent(ModifierPlayer player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection);
		public event ModifyHitNPCWithProjEvent OnModifyHitNPCWithProj;
		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			OnModifyHitNPCWithProj?.Invoke(this, proj, target, ref damage, ref knockback, ref crit, ref hitDirection);
		}

		public delegate void ModifyHitPvpEventRaiser(ModifierPlayer player, Item item, Player target, ref int damage, ref bool crit);
		public event ModifyHitPvpEventRaiser OnModifyHitPvp;
		public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
		{
			OnModifyHitPvp?.Invoke(this, item, target, ref damage, ref crit);
		}

		public delegate void ModifyHitPvpWithProjEvent(ModifierPlayer player, Projectile proj, Player target, ref int damage, ref bool crit);
		public event ModifyHitPvpWithProjEvent OnModifyHitPvpWithProj;
		public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
		{
			OnModifyHitPvpWithProj?.Invoke(this, proj, target, ref damage, ref crit);
		}

		public delegate void OnHitNPCEventRaiser(ModifierPlayer player, Item item, NPC target, int damage, float knockback, bool crit);
		public event OnHitNPCEventRaiser OnOnHitNPC;
		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
		{
			OnOnHitNPC?.Invoke(this, item, target, damage, knockback, crit);
		}

		public delegate void OnHitPvpEventRaiser(ModifierPlayer player, Item item, Player target, int damage, bool crit);
		public event OnHitPvpEventRaiser OnOnHitPvp;
		public override void OnHitPvp(Item item, Player target, int damage, bool crit)
		{
			OnOnHitPvp?.Invoke(this, item, target, damage, crit);
		}

		public delegate bool PreKillEventRaiser(ModifierPlayer player, double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource);
		public event PreKillEventRaiser OnPreKill;
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
		{
			bool b = base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
			if (OnPreKill != null)
			{
				b &= OnPreKill.Invoke(this, damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
			}

			return b;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			base.SyncPlayer(toWho, fromWho, newPlayer);
			// @TODO sync player info
		}
	}
}
