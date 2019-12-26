using Loot.Core;
using Loot.Core.Attributes;
using Microsoft.Xna.Framework;
using Terraria;

namespace Loot.Modifiers.EquipModifiers.Defensive
{
	public class LifeRegenEffect : ModifierEffect
	{
		public int LifeRegen;

		public override void ResetEffects(ModifierPlayer player)
		{
			LifeRegen = 0;
		}

		[AutoDelegation("OnUpdateLifeRegen")]
		private void Regen(ModifierPlayer player)
		{
			player.player.lifeRegen += LifeRegen;
		}
	}

	[UsesEffect(typeof(LifeRegenEffect))]
	public class LifeRegen : EquipModifier
	{
		public override ModifierTooltipLine[] TooltipLines => new[]
		{
			new ModifierTooltipLine
			{
				Text = $"+{Properties.RoundedPower} life regen/second",
				Color =  Color.LimeGreen
			},
		};

		public override ModifierProperties GetModifierProperties(Item item)
		{
			return base.GetModifierProperties(item).Set
				(
					minMagnitude: 2f,
					maxMagnitude: 10f,
					rollChance: 0.5f
				);
		}

		public override void UpdateEquip(Item item, Player player)
		{
			ModifierPlayer.Player(player).GetEffect<LifeRegenEffect>().LifeRegen += (int)Properties.RoundedPower;
		}
	}
}
