using Loot.Core;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.WeaponModifiers
{
	public class MissingHealthDamage : WeaponModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
			{
				new ModifierTooltipLine
				{
					Text = $"Up to +{Properties.RoundedPower}% damage based on missing health",
					Color = Color.Lime
				}
			};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 4f,
					maxMagnitude: 12f
				);
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref float add, ref float multi, ref float flat)
		{
			base.ModifyWeaponDamage(item, player, ref add, ref multi, ref flat);
			// Formula ported from old mod
			float mag = (Properties.RoundedPower * ((player.statLifeMax2 - player.statLife) / (float)player.statLifeMax2));
			add +=(mag / 100f);
		}
	}
}
