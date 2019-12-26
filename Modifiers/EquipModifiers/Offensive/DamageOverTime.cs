using System;
using System.Collections.Generic;
using System.IO;
using Loot.Core;
using Loot.Core.Attributes;
using Loot.Modifiers.EquipModifiers.Defensive;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Loot.Modifiers.EquipModifiers.Offensive
{
	public class DamageOverTimeEffect : ModifierEffect
	{
		public float Damage;
		public float procChance;
		public static float MAX_PROC_CHANCE = 1.0f;
		private float duration => 10f;
		private int timeNpc = 60;
		private int timePlayer = 60;

		private List<(NPC, int, bool, float)> npcdata = new List<(NPC, int, bool, float)>();
		private List<(Player, int, bool, float)> playerdata = new List<(Player, int, bool, float)>();

		public override void OnInitialize(ModifierPlayer player)
		{
			Damage = 0f;

			npcdata.TrimExcess();
			playerdata.TrimExcess();

			timeNpc = 60;
			timePlayer = 60;
		}

		public override void ResetEffects(ModifierPlayer player)
		{
			Damage = 0f;
			procChance = 0f;
		}

		private bool alive(NPC ntarget = null, Player ptarget = null, bool nododge = false)
		{
			bool yes = true;

			if (ntarget != null)
			{
				yes &= ntarget.life > 0 && ntarget.active && !ntarget.dontTakeDamage && !ntarget.dontTakeDamageFromHostiles;
			}
			else if (ptarget != null)
			{
				yes &= Main.player[ptarget.whoAmI].statLife > 0 && Main.player[ptarget.whoAmI].active && !Main.player[ptarget.whoAmI].dead;

				if (!nododge)
				{
					yes &= !Main.player[ptarget.whoAmI].shadowDodge && !ModifierPlayer.Player(Main.player[ptarget.whoAmI]).GetEffect<DodgeEffect>().Dodged;
				}
			}
			else
			{
				throw new ArgumentNullException("ntarget and ptarget");
			}

			return yes;
		}

		[AutoDelegation("OnModifyHitNPC")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		public void ModifyHitNPC(ModifierPlayer player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			float rand = Main.rand.NextFloat(0f, 1f);
			float chance = Math.Min(procChance, MAX_PROC_CHANCE);

			if (rand <= chance
				&& alive(ntarget: target))
			{
				npcdata.Add((target, damage, crit, Main.GlobalTime + duration));
			}
		}

		[AutoDelegation("OnModifyHitNPCWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		private void OnModifyHitNPCWithProj(ModifierPlayer player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			float rand = Main.rand.NextFloat(0f, 1f);
			float chance = Math.Min(procChance, MAX_PROC_CHANCE);

			if (rand <= chance
				&& alive(ntarget: target))
			{
				npcdata.Add((target, damage, crit, Main.GlobalTime + duration));
			}
		}

		[AutoDelegation("OnModifyHitPvp")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		private void ModifyHitPvp(ModifierPlayer player, Item item, Player target, ref int damage, ref bool crit)
		{
			float rand = Main.rand.NextFloat(0f, 1f);
			float chance = Math.Min(procChance, MAX_PROC_CHANCE);

			if (rand <= chance
				&& alive(ptarget: target))
			{
				playerdata.Add((target, damage, crit, Main.GlobalTime + duration));
			}
		}

		[AutoDelegation("OnModifyHitPvpWithProj")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		private void OnModifyHitPvpWithProj(ModifierPlayer player, Projectile proj, Player target, ref int damage, ref bool crit)
		{
			float rand = Main.rand.NextFloat(0f, 1f);
			float chance = Math.Min(procChance, MAX_PROC_CHANCE);

			if (rand <= chance
				&& alive(ptarget: target))
			{
				playerdata.Add((target, damage, crit, Main.GlobalTime + duration));
			}
		}

		[AutoDelegation("OnPreUpdate")]
		[DelegationPrioritization(DelegationPrioritization.Late, 990)]
		private void onupdate(ModifierPlayer player)
		{
			if (npcdata.Count > 0)
			{
				for (int i = 0; i < npcdata.Count; i++)
				{
					int damage = (int)Math.Ceiling((npcdata[i].Item2 * (1.0 + Damage)) / duration);
					
					if(timeNpc <= 0)
					{
						player.ApplyDamageToNPC(target: npcdata[i].Item1, damage: damage, knockback: 0, direction: 0, crit: npcdata[i].Item3, dot: true);
						player.player.addDPS(damage);
						timeNpc = 60;
					}
					else
					{
						timeNpc--;
					}

					if(npcdata[i].Item4 <= Main.GlobalTime
						|| !alive(ntarget: npcdata[i].Item1))
					{
						npcdata.RemoveAt(i);
						npcdata.TrimExcess();
					}
				}
			}

			if (playerdata.Count > 0)
			{
				for (int i = 0; i < playerdata.Count; i++)
				{
					int damage = (int)Math.Ceiling((playerdata[i].Item2 * (1.0 * Damage)) / duration);

					if (timePlayer <= 0)
					{
						player.ApplyDamageToPlayer(target: playerdata[i].Item1, damage: damage, direction: 0, crit: playerdata[i].Item3, pvp: true, dot: true);
						player.player.addDPS(damage);
						timePlayer = 60;
					}
					else
					{
						timePlayer--;
					}

					if(playerdata[i].Item4 <= Main.GlobalTime
						|| !alive(ptarget: playerdata[i].Item1, nododge: true))
					{
						playerdata.RemoveAt(i);
						playerdata.TrimExcess();
					}
				}
			}
		}
	}

	[UsesEffect(typeof(DamageOverTimeEffect))]
	public class DamageOverTime : EquipModifier
	{
		private double multiplier;

		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{multiplier}% chance to deal {Properties.RoundedPower}% of your damage over 10 seconds",
				Color = Color.LimeGreen
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 2f,
					maxMagnitude: 8f,
					rollChance: 0.5f,
					rarityLevel: 2f,
					uniqueRoll: true
				);
		}

		public override void Roll(ModifierContext ctx, IEnumerable<Modifier> rolledModifiers)
		{
			base.Roll(ctx, rolledModifiers);
			multiplier = (float)Math.Ceiling(Main.rand.NextFloat(0.75f, 1.25f) * Properties.RoundedPower);
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			base.NetSend(item, writer);
			writer.Write(multiplier);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			base.NetReceive(item, reader);
			multiplier = reader.ReadDouble();
		}

		public override void Save(Item item, TagCompound tag)
		{
			base.Save(item, tag);
			tag.Add("dotm", multiplier);
		}

		public override void Load(Item item, TagCompound tag)
		{
			base.Load(item, tag);
			multiplier = tag.GetDouble("dotm");
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<DamageOverTimeEffect>().Damage += Properties.RoundedPower / 100f;
			ModifierPlayer.Player(player).GetEffect<DamageOverTimeEffect>().procChance += (float)multiplier / 100f;
		}
	}
}
