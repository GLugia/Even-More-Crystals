using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.WeaponModifiers
{
	public class AmmoReduce : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					Text = $"{Properties.RoundedPower}% chance to not consume ammo",
					Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 1f,
					maxMagnitude: 8f
				);
		}

		public override bool CanRoll(ModifierContext ctx)
		{
			// Only apply on items that consume ammo
			return base.CanRoll(ctx) && ctx.Item.useAmmo > 0;
		}

		public override bool ConsumeAmmo(Item item, Player player)
		{
			return Main.rand.NextFloat() > Properties.RoundedPower / 100f;
		}
	}
}