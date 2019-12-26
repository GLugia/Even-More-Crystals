using Loot.Core;
using Microsoft.Xna.Framework;

namespace Loot.Rarities
{
	public class TranscendentRarity : ModifierRarity
	{
		public override string Name => "Transcendent";
		public override Color Color => Color.DeepPink;
		public override float RequiredRarityLevel => 6f;

		// @TODO add suffixes and suffix support
		// public override string Suffix => "of the Gods";
	}
}
