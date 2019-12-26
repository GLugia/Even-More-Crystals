using Loot.Core;
using Loot.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Loot.Items
{
	public class AlchemicCrystal : CrystalItem
	{
		internal override string CrystalName => "Alchemic Crystal";
		protected override Color? OverrideNameColor => Color.Gold;
		protected override string Usage => "add a random modifier";

		public override void SafeDefaults()
		{
			item.createTile = ModContent.TileType<AlchemicCrystalTile>();
		}

		protected override bool ModifyItem(Player player, Item item)
		{
			var actives = EMMItem.GetActivePool(item);
			EMMItem oldItem = EMMItem.GetItemInfo(item);

			if (actives.Count() < oldItem.CurrentPossibleModifiers)
			{
				Item nitem = new Item();
				nitem.SetDefaults(item.type);
				EMMItem theItem = EMMItem.GetItemInfo(nitem);
				theItem.Clone(oldItem);

				ModifierContext ctx = new ModifierContext
				{
					Method = ModifierContextMethod.OnCubeReroll,
					Item = nitem,
					Player = player,
					CustomData = new Dictionary<string, object>
					{
						{ "Source", "AlchemicCrystal" }
					}
				};

				ModifierPool pool = theItem.AlterPool(ctx, itemRollProperties: RollLogic(), add: true, amount: 1);

				return ApplyModifiers(player, pool, nitem);
			}

			Main.NewText(item.Name + " cannot be modified further");
			return false;
		}

		protected override ItemRollProperties RollLogic()
		{
			float kslime = NPC.downedSlimeKing ? 0.1f : 0f;
			float eoc = NPC.downedBoss1 ? 0.1f : 0f;
			float eowboc = NPC.downedBoss2 ? 0.1f : 0f;
			float skeleman = NPC.downedBoss3 ? 0.1f : 0f;
			float beeeeeee = NPC.downedQueenBee ? 0.1f : 0f;
			float hardmode = Main.hardMode ? 0.25f : 0f;
			float amechs = NPC.downedMechBossAny ? 0.25f : 0f;
			float plantera = NPC.downedPlantBoss ? 0.25f : 0f;
			float moonlord = NPC.downedMoonlord ? 0.25f : 0f;
			float dog = CalamityMod.World.CalamityWorld.downedDoG ? 0.5f : 0f;
			float yharon = CalamityMod.World.CalamityWorld.downedYharon ? 0.5f : 0f;
			float supcal = CalamityMod.World.CalamityWorld.downedSCal ? 1f : 0f;

			float multiplier = kslime + eoc + eowboc + skeleman + beeeeeee + hardmode + amechs + plantera + moonlord + dog + yharon + supcal;

			float newLuck = Main.rand.NextFloat(0f, 1f) * (1f + multiplier);
			float newPower = 1f + (Main.rand.NextFloat(0f, 0.5f) * (1 + multiplier));

			ItemRollProperties prop = new ItemRollProperties()
			{
				ExtraLuck = newLuck,
				MagnitudePower = newPower
			};

			return prop;
		}
	}
}
