//using System.IO;
using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;
//using Terraria.ModLoader.IO;

namespace Loot.Modifiers.WeaponModifiers
{
	public class SpeedPlus : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					Text = $"+{Properties.RoundedPower}% use speed",
					Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					basePower: 1/5f,
					maxMagnitude: 15f
				);
		}

		public override void Apply(Item item)
		{
			base.Apply(item);

			item.useTime = (int)(item.useTime * (1 - Properties.RoundedPower / 100f));
			item.useAnimation = (int)(item.useAnimation * (1 - Properties.RoundedPower / 100f));

			// Don't go below the minimum
			if (item.useTime < 2)
			{
				item.useTime = 2;
			}

			if (item.useAnimation < 2)
			{
				item.useAnimation = 2;
			}
		}
	}
}
