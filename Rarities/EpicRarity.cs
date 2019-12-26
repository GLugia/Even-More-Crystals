using Loot.Core;
using Microsoft.Xna.Framework;

namespace Loot.Rarities
{
	public class EpicRarity : ModifierRarity
	{
		public override string Name => "Epic";
		public override Color Color => Color.LightSkyBlue;
		public override float RequiredRarityLevel => 3f;
	}
}
