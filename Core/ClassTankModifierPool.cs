using Loot.Modifiers;
using Loot.Modifiers.EquipModifiers.Defensive;
using Loot.Modifiers.EquipModifiers.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loot.Core
{
	public sealed class ClassTankModifierPool : ModifierPool
	{
		public override float RollChance => 0f;

		public ClassTankModifierPool()
		{
			var list = new List<Modifier>();

			foreach (KeyValuePair<uint, Modifier> m in EMMLoader.Modifiers)
			{
				String name = EMMLoader.GetModifier(m.Key).Name;

				// @TODO find an automatic way to ignore Offensive modifiers
				if (m.Key < 13 || m.Key > 27				// Ignore Offensive Modifiers
					&& name != "ManaPlus"					// Ignore ManaPlus
					&& name != "HealingPlus"				// Ignore HealingPlus
					&& name != "HealingSpeedPlus"			// Ignore HealingSpeedPlus
					)
				{
					list.Add(EMMLoader.GetModifier(m.Key));
				}
			}

			Modifiers = list.ToArray();
		}

		public override bool CanRoll(ModifierContext ctx)
		{
			return IsValidFor(ctx.Item);
		}
	}
}
