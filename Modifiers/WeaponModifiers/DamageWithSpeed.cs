using Loot.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace Loot.Modifiers.WeaponModifiers
{
	public class DamageWithSpeed : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					 Text = $"{(_fast ? "-" : "+")}{(int)(Properties.RoundedPower * 2.5f)}% damage, but {Math.Round(_fval, 1)}% {(_fast ? "faster" : "slower")} use speed",
					 Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 4f,
					maxMagnitude: 12f,
					rollChance: 0.5f,
					uniqueRoll: true
				);
		}

		private bool _fast;

		// Roll a random bool to decide if it's fast or slow
		public override void Roll(ModifierContext ctx, IEnumerable<Modifier> rolledModifiers)
		{
			base.Roll(ctx, rolledModifiers);
			_fast = Main.rand.NextBool();
		}

		// Begin send/recieve to/from server

		public override void NetReceive(Item item, BinaryReader reader)
		{
			base.NetReceive(item, reader);
			try
			{
				_fast = reader.ReadBoolean();
				_fval = reader.ReadDouble();
			}
			catch (Exception e)
			{
				Loot.Logger.Error(e.StackTrace.ToString());
				Loot.Logger.Error(e.ToString());
				Main.NewText("An error has occurred with Loot related to the 'DamageWithSpeed' modifier.", 255, 0, 0, true);
			}
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			base.NetSend(item, writer);
			try
			{
				writer.Write(_fast);
				writer.Write(_fval);
			}
			catch(Exception e)
			{
				Loot.Logger.Error(e.StackTrace.ToString());
				Loot.Logger.Error(e.ToString());
				Main.NewText("An error has occurred with Loot related to the 'DamageWithSpeed' modifier.", 255, 0, 0, true);
			}
		}

		// End send/recieve to/from server

		// Begin save/load to/from client

		public override void Load(Item item, TagCompound tag)
		{
			base.Load(item, tag);
			_fast = tag.GetBool("fastorslow");
			_fval = tag.GetDouble("fval");
		}

		public override void Save(Item item, TagCompound tag)
		{
			base.Save(item, tag);
			tag.Add("fastorslow", _fast);
			tag.Add("fval", _fval);
		}

		// End save/load to/from client

		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float multi, ref float flat)
		{
			base.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);

			if (_fast)
			{
				add -= (Properties.RoundedPower / 100f) * 2.5f;
			}
			else
			{
				add += (Properties.RoundedPower / 100f) * 2.5f;
			}
		}

		private double _fval;

		public override void Apply(Item item)
		{
			base.Apply(item);

			double a = Properties.RoundedPower * 0.5f;
			double b = item.useTime;
			double c = item.useAnimation;
			double x;
			double y;


			// Thank you my lord and savior, Advanced Algebra for these glorious equations
			if (_fast)
			{
				// 2b - ((a^2 + ab + (11b^2)) / 10b)
				// higher increase if the use time was low
				// higher increase if the damage is high
				y = (b * 2) - (Math.Pow(a, 2) + (a * b) + (11 * Math.Pow(b, 2))) / (10 * b);
				x = (c * 2) - (Math.Pow(a, 2) + (a * c) + (11 * Math.Pow(c, 2))) / (10 * c);
			}
			else
			{
				// here we get a diminishing value based on how low the use time and use animation already was
				//   as well as how high the damage increase/decrease is
				//
				// higher decrease in use time if the use time was already high
				// lower decrease in use time if the damage is low
				// it's very complex and i'm very proud of it
				y = (Math.Pow(a, 2) + (a * b) + (11 * Math.Pow(b, 2))) / (10 * b);
				x = (Math.Pow(a, 2) + (a * c) + (11 * Math.Pow(c, 2))) / (10 * c);
			}

			// just static values holding the ceiling of x/y since i've noticed some bugs in c# handling math
			item.useTime = (int)Math.Ceiling(y);
			item.useAnimation = (int)Math.Ceiling(x);

			// Don't go below the minimum
			if (item.useTime < 2)
			{
				item.useTime = 2;
			}

			if (item.useAnimation < 2)
			{
				item.useAnimation = 2;
			}

			// For displaying the increase/decrease in speed

			double d = item.useTime / b;
			double e = d - 1;
			double f = item.useAnimation / c;

			if (_fast)
			{
				_fval = (e * -1) * 100f;
			}
			else
			{
				_fval = e * 100f;
			}
		}
	}
}
