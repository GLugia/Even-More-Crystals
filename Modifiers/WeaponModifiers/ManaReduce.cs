using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace Loot.Modifiers.WeaponModifiers
{
	public class ManaReduce : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"-{Properties.RoundedPower}% mana cost",
				Color = Color.Lime
			}
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 2f,
					maxMagnitude: 8f
				);
		}

		public override bool CanRoll(ModifierContext ctx)
			=> base.CanRoll(ctx) && ctx.Item.mana > 0;

		public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
		{
			base.ModifyManaCost(item, player, ref reduce, ref mult);

			if(item.mana < 1)
			{
				item.mana = 1;
			}
		}

		public override void Apply(Item item)
		{
			base.Apply(item);

			item.mana = (int)Math.Floor(item.mana * (1 - Properties.RoundedPower / 100f));

			if (item.mana < 1)
			{
				item.mana = 1;
			}
		}
	}
}
