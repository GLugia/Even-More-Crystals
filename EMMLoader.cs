using Loot.Core;
using Loot.Core.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Loot.Core.ContentMod;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.Utilities;
using EffectMap = System.Collections.Generic.KeyValuePair<string, Loot.Core.ModifierEffect>;
using ModifierMap = System.Collections.Generic.KeyValuePair<string, Loot.Core.Modifier>;
using PoolMap = System.Collections.Generic.KeyValuePair<string, Loot.Core.ModifierPool>;
using RarityMap = System.Collections.Generic.KeyValuePair<string, Loot.Core.ModifierRarity>;
//using GlobalModifierMap = System.Collections.Generic.KeyValuePair<string, Loot.Core.GlobalModifier>;

namespace Loot
{
	/// <summary>
	/// The loader, handles all loading
	/// </summary>
	public static class EMMLoader
	{
		private static uint rarityNextID;
		private static uint modifierNextID;
		private static uint poolNextID;
		private static uint effectNextID;
		//		private static uint globalModifierNextID;

		internal static IDictionary<string, List<RarityMap>> RaritiesMap;
		internal static IDictionary<string, List<ModifierMap>> ModifiersMap;
		internal static IDictionary<string, List<PoolMap>> PoolsMap;
		internal static IDictionary<string, List<EffectMap>> EffectsMap;
		//		internal static IDictionary<string, List<GlobalModifierMap>> GlobalModifiersMap;

		internal static IDictionary<uint, ModifierRarity> Rarities;
		internal static IDictionary<uint, Modifier> Modifiers;
		internal static IDictionary<uint, ModifierPool> Pools;
		internal static IDictionary<uint, ModifierEffect> Effects;
		//		internal static IDictionary<uint, GlobalModifier> GlobalModifiers;

		internal static IDictionary<string, Assembly> Mods;

		internal static void Initialize()
		{
			rarityNextID = 0;
			modifierNextID = 0;
			poolNextID = 0;
			effectNextID = 0;
			//			globalModifierNextID = 0;

			RaritiesMap = new Dictionary<string, List<RarityMap>>();
			ModifiersMap = new Dictionary<string, List<ModifierMap>>();
			PoolsMap = new Dictionary<string, List<PoolMap>>();
			EffectsMap = new Dictionary<string, List<EffectMap>>();

			//			GlobalModifiersMap = new Dictionary<string, List<GlobalModifierMap>>();

			Rarities = new Dictionary<uint, ModifierRarity>();
			Modifiers = new Dictionary<uint, Modifier>();
			Pools = new Dictionary<uint, ModifierPool>();
			Effects = new Dictionary<uint, ModifierEffect>();
			//			GlobalModifiers = new Dictionary<uint, GlobalModifier>();

			// todo why did I make this concurrent
			Mods = new ConcurrentDictionary<string, Assembly>();
		}

		internal static void Load()
		{

		}

		internal static void Unload()
		{
			rarityNextID = 0;
			modifierNextID = 0;
			poolNextID = 0;
			effectNextID = 0;
			//			globalModifierNextID = 0;

			RaritiesMap = null;
			ModifiersMap = null;
			PoolsMap = null;
			EffectsMap = null;
			//			GlobalModifiersMap = null;

			Rarities = null;
			Modifiers = null;
			Pools = null;
			Mods = null;
			Effects = null;
			//			GlobalModifiers = null;
		}

		/// <summary>
		/// Registers specified mod, enabling autoloading for that mod
		/// </summary>
		public static void RegisterMod(Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("RegisterMod can only be called from Mod.Load or Mod.Autoload");
			}

			if (Mods.ContainsKey(mod.Name))
			{
				throw new Exception($"Mod {mod.Name} is already registered");
			}

			Assembly code;
#if DEBUG
			code = Assembly.GetAssembly(mod.GetType());
#else
			code = mod.Code;
#endif

			Mods.Add(new KeyValuePair<string, Assembly>(mod.Name, code));
			RaritiesMap.Add(new KeyValuePair<string, List<RarityMap>>(mod.Name, new List<RarityMap>()));
			ModifiersMap.Add(new KeyValuePair<string, List<ModifierMap>>(mod.Name, new List<ModifierMap>()));
			PoolsMap.Add(new KeyValuePair<string, List<PoolMap>>(mod.Name, new List<PoolMap>()));
			EffectsMap.Add(new KeyValuePair<string, List<EffectMap>>(mod.Name, new List<EffectMap>()));
			//			GlobalModifiersMap.Add(new KeyValuePair<string, List<GlobalModifierMap>>(mod.Name, new List<GlobalModifierMap>()));
		}

		public static void RegisterAssets(Mod mod, string folder, bool clearOwnTextures = true)
		{
			if (mod == null)
			{
				throw new NullReferenceException("Mod is null in RegisterAssets");
			}

			if (Loot.ContentManager == null)
			{
				throw new NullReferenceException("Loot.ContentManager is null in RegisterAssets");
			}

			var graphicsContent = Loot.ContentManager.GetContent<ModGraphicsContent>();
			if (graphicsContent == null)
			{
				throw new NullReferenceException("ModGraphicsContent is null in RegisterAssets");
			}

			if (folder.StartsWith($"{mod.Name}/"))
			{
				folder = folder.Replace($"{mod.Name}/", "");
			}
			string keyPass = $"{mod.Name}/{folder}";
			graphicsContent.AddKeyPass(mod.Name, keyPass);

			FieldInfo texturesField = typeof(Mod).GetField("textures", BindingFlags.Instance | BindingFlags.NonPublic);
			Dictionary<string, Texture2D> dictionary = ((Dictionary<string, Texture2D>)texturesField?.GetValue(mod));
			if (dictionary == null)
			{
				throw new NullReferenceException($"textures dictionary for mod {mod.Name} was null");
			}

			var textures = dictionary.Where(x => x.Key.StartsWith(folder)).ToList();
			var glowmasks = textures.Where(x => x.Key.EndsWith("_Glowmask") || x.Key.EndsWith("_Glow")).ToList();
			var shaders = textures.Where(x => x.Key.EndsWith("_Shader") || x.Key.EndsWith("_Shad")).ToList();
			
			foreach (var kvp in glowmasks)
			{
				string assetKey = graphicsContent.GetAssetKey(kvp.Value.Name, mod);
				if (assetKey == null) continue;
				if (graphicsContent.AnyGlowmaskAssetExists(assetKey, mod))
				{
					throw new Exception($"{mod.Name} attempted to add a glowmask asset already present: {assetKey}");
				}
				graphicsContent.AddGlowmaskTexture(assetKey, kvp.Value);
				if (clearOwnTextures)
				{
					dictionary.Remove(kvp.Key);
				}
			}

			foreach (var kvp in shaders)
			{
				string assetKey = graphicsContent.GetAssetKey(kvp.Value.Name, mod);
				if (assetKey == null) continue;
				if (graphicsContent.AnyShaderAssetExists(assetKey, mod))
				{
					throw new Exception($"{mod.Name} attempted to add a shader asset already present: {assetKey}");
				}
				graphicsContent.AddShaderTexture(assetKey, kvp.Value);
				if (clearOwnTextures)
				{
					dictionary.Remove(kvp.Key);
				}
			}

			// sanity check
			graphicsContent.Prepare(mod);
			graphicsContent.ClearPreparation();
		}

		// todo refactor to ReserveID
		internal static uint ReserveRarityID()
		{
			uint reserved = rarityNextID;
			rarityNextID++;
			return reserved;
		}

		internal static uint ReserveModifierID()
		{
			uint reserved = modifierNextID;
			modifierNextID++;
			return reserved;
		}

		internal static uint ReservePoolID()
		{
			uint reserved = poolNextID;
			poolNextID++;
			return reserved;
		}

		internal static uint ReserveEffectID()
		{
			uint reserved = effectNextID;
			effectNextID++;
			return reserved;
		}

		//		internal static uint ReserveGlobalModifierID()
		//		{
		//			uint reserved = globalModifierNextID;
		//			globalModifierNextID++;
		//			return reserved;
		//		}

		/// <summary>
		/// Returns a random weighted pool from all available pools that can apply
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		internal static ModifierPool GetWeightedPool(ModifierContext ctx)
		{
			var wr = new WeightedRandom<ModifierPool>();
			foreach (var m in Pools.Where(x => x.Value._CanRoll(ctx)))
			{
				wr.Add(m.Value, m.Value.RollChance);
			}

			var mod = wr.Get();
			return (ModifierPool)mod?.Clone();
		}

		// 
		/// <summary>
		/// Returns the rarity of a pool
		/// </summary>
		/// <param name="modifierPool"></param>
		/// <returns></returns>
		internal static ModifierRarity GetPoolRarity(ModifierPool modifierPool)
		{
			return (ModifierRarity)Rarities
					.Select(r => r.Value)
					.OrderByDescending(r => r.RequiredRarityLevel)
					.FirstOrDefault(r => r.MatchesRequirements(modifierPool))
					?.Clone();
		}

		/// <summary>
		/// Returns the ModifierRarity specified by type, null if not present
		/// </summary>
		public static ModifierRarity GetModifierRarity(uint type)
		{
			return type < rarityNextID ? (ModifierRarity)Rarities[type].Clone() : null;
		}

		/// <summary>
		/// Returns the ModifierEffect specified by type, null if not present
		/// </summary>
		public static Modifier GetModifier(uint type)
		{
			return type < modifierNextID ? (Modifier)Modifiers[type].Clone() : null;
		}

		/// <summary>
		/// Returns the Modifier specified by type, null if not present
		/// </summary>
		public static ModifierPool GetModifierPool(uint type)
		{
			return type < poolNextID ? (ModifierPool)Pools[type].Clone() : null;
		}

		/// <summary>
		/// Returns the ModifierEffect specified by type, null if not present
		/// </summary>
		public static ModifierEffect GetModifierEffect(uint type)
		{
			return type < effectNextID ? (ModifierEffect)Effects[type].Clone() : null;
		}

		/// <summary>
		/// Sets up content for the specified mod
		/// </summary>
		public static void SetupContent(Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("SetupContent for EMMLoader can only be called from Mod.Load or Mod.Autoload");
			}

			if (!Mods.ContainsKey(mod.Name))
			{
				throw new Exception($"Mod {mod.Name} is not yet registered in EMMLoader");
			}

			var ordered = Mods.FirstOrDefault(x => x.Key.Equals(mod.Name))
				.Value
				.GetTypes()
				.OrderBy(x => x.FullName, StringComparer.InvariantCulture)
				.Where(t => t.IsClass && !t.IsAbstract)
				.ToList(); /* || type.GetConstructor(new Type[0]) == null*/

			var rarities = ordered.Where(x => x.IsSubclassOf(typeof(ModifierRarity)));
			var modifiers = ordered.Where(x => x.IsSubclassOf(typeof(Modifier)));
			var pools = ordered.Where(x => x.IsSubclassOf(typeof(ModifierPool)));
			var effects = ordered.Where(x => x.IsSubclassOf(typeof(ModifierEffect)));
			//			var globalModifiers = ordered.Where(x => x.IsSubclassOf(typeof(GlobalModifier)));

			// important: load things in order. (modifiers relies on all.. etc.)
			foreach (Type type in rarities)
			{
				AutoloadModifierRarity(type, mod);
			}

			foreach (Type type in modifiers)
			{
				AutoloadModifier(type, mod);
			}

			foreach (Type type in pools)
			{
				AutoloadModifierPool(type, mod);
			}

			foreach (Type effect in effects)
			{
				AutoloadModifierEffect(effect, mod);
			}

			//			foreach (Type type in globalModifiers)
			//			{
			//				AutoloadGlobalModifier(type, mod);
			//			}
		}

		/// <summary>
		/// Autoloads a ModifierPool
		/// </summary>
		private static void AutoloadModifierPool(Type type, Mod mod)
		{
			ModifierPool modifierPool = (ModifierPool)Activator.CreateInstance(type);
			AddModifierPool(modifierPool, mod);
		}

		/// <summary>
		/// Adds a ModifierPool
		/// </summary>
		public static void AddModifierPool(ModifierPool pool, Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("AddModifierPool can only be called from Mod.Load or Mod.Autoload");
			}
			if (!Mods.ContainsKey(mod.Name))
			{
				throw new Exception($"Mod {mod.Name} is not registered, please register before adding");
			}

			List<PoolMap> lmm;
			if (!PoolsMap.TryGetValue(mod.Name, out lmm))
			{
				throw new Exception($"PoolMaps for {mod.Name} not found");
			}

			if (lmm.Exists(x => x.Value.Name.Equals(pool.Name)))
			{
				throw new Exception($"You have already added a ModifierPool with the name {pool.Name}");
			}

			pool.Mod = mod;
			pool.Type = ReservePoolID();
			Pools[pool.Type] = pool;
			PoolsMap[mod.Name].Add(new PoolMap(pool.Name, pool));
		}

		/// <summary>
		/// Autoloads a rarity
		/// </summary>
		private static void AutoloadModifierRarity(Type type, Mod mod)
		{
			ModifierRarity rarity = (ModifierRarity)Activator.CreateInstance(type);
			AddModifierRarity(rarity, mod);
		}

		/// <summary>
		/// Adds a rarity
		/// </summary>
		public static void AddModifierRarity(ModifierRarity rarity, Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("AddRarity can only be called from Mod.Load or Mod.Autoload");
			}
			if (!Mods.ContainsKey(mod.Name))
			{
				throw new Exception($"Mod {mod.Name} is not registered, please register before adding");
			}

			List<RarityMap> lrm;
			if (!RaritiesMap.TryGetValue(mod.Name, out lrm))
			{
				throw new Exception($"RarityMaps for {mod.Name} not found");
			}

			if (lrm.Exists(x => x.Value.Name.Equals(rarity.Name)))
			{
				throw new Exception($"You have already added a rarity with the name {rarity.Name}");
			}

			rarity.Mod = mod;
			rarity.Type = ReserveRarityID();
			Rarities[rarity.Type] = rarity;
			RaritiesMap[mod.Name].Add(new RarityMap(rarity.Name, rarity));
		}

		/// <summary>
		/// Autoloads a Modifier
		/// </summary>
		private static void AutoloadModifier(Type type, Mod mod)
		{
			Modifier modifier = (Modifier)Activator.CreateInstance(type);
			AddModifier(modifier, mod);
		}

		/// <summary>
		/// Adds a Modifier
		/// </summary>
		public static void AddModifier(Modifier modifier, Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("AddModifier can only be called from Mod.Load or Mod.Autoload");
			}

			List<ModifierMap> lem;
			if (!ModifiersMap.TryGetValue(mod.Name, out lem))
			{
				throw new Exception($"ModifierMaps for {mod.Name} not found");
			}

			if (lem.Exists(x => x.Value.Name.Equals(modifier.Name)))
			{
				throw new Exception($"You have already added a modifier with the name {modifier.Name}");
			}

			modifier.Mod = mod;
			modifier.Type = ReserveModifierID();
			Modifiers[modifier.Type] = modifier;
			ModifiersMap[mod.Name].Add(new ModifierMap(modifier.Name, modifier));
		}

		/// <summary>
		/// Autoloads a ModifierEffect 
		/// </summary>
		private static void AutoloadModifierEffect(Type type, Mod mod)
		{
			ModifierEffect effect = (ModifierEffect)Activator.CreateInstance(type);
			AddModifierEffect(effect, mod);
		}

		/// <summary>
		/// Adds a ModifierEffect
		/// </summary>
		public static void AddModifierEffect(ModifierEffect effect, Mod mod)
		{
			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
			if (b != null && !b.Value)
			{
				throw new Exception("AddModifierEffect can only be called from Mod.Load or Mod.Autoload");
			}

			List<EffectMap> lem;
			if (!EffectsMap.TryGetValue(mod.Name, out lem))
			{
				throw new Exception($"EffectsMap for {mod.Name} not found");
			}

			if (lem.Exists(x => x.Value.Name.Equals(effect.Name)))
			{
				throw new Exception($"You have already added a modifier with the name {effect.Name}");
			}

			//verbose GetCustomAttributes call
			//because we call the constructor, it will throw our
			//validation exceptions on load instead on entering world
			var attributes = effect
				.GetType()
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				.Select(x => x.GetCustomAttributes(false).OfType<DelegationPrioritizationAttribute>());

			foreach (var attribute in attributes.SelectMany(x => x))
			{
				Activator.CreateInstance(attribute.GetType(), attribute.DelegationPrioritization, attribute.DelegationLevel);
			}

			effect.Mod = mod;
			effect.Type = ReserveEffectID();
			Effects[effect.Type] = effect;
			EffectsMap[mod.Name].Add(new EffectMap(effect.Name, effect));
		}

		/// <summary>
		/// Autoloads a GlobalModifier
		/// </summary>
		/// <param name="type"></param>
		/// <param name="mod"></param>
		//		private static void AutoloadGlobalModifier(Type type, Mod mod)
		//		{
		//			GlobalModifier globalModifier = (GlobalModifier)Activator.CreateInstance(type);
		//			AddGlobalModifier(globalModifier, mod);
		//		}

		/// <summary>
		/// Adds a GlobalModifier
		/// </summary>
		/// <param name="modifier"></param>
		/// <param name="mod"></param>
		//		public static void AddGlobalModifier(GlobalModifier globalModifier, Mod mod)
		//		{
		//			bool? b = mod.GetType().GetField("loading", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod) as bool?;
		//			if (b != null && !b.Value)
		//			{
		//				throw new Exception("AddGlobalModifier can only be called from Mod.Load or Mod.Autoload");
		//			}
		//
		//			List<GlobalModifierMap> lem;
		//			if (!GlobalModifiersMap.TryGetValue(mod.Name, out lem))
		//			{
		//				throw new Exception($"GlobalModifiersMap for {mod.Name} not found");
		//			}
		//
		//			if (lem.Exists(x => x.Value.Name.Equals(globalModifier.Name)))
		//			{
		//				throw new Exception($"You have already added a modifier with the name {globalModifier.Name}");
		//			}
		//
		//			globalModifier.Mod = mod;
		//			globalModifier.Type = ReserveGlobalModifierID();
		//			GlobalModifiers[globalModifier.Type] = globalModifier;
		//			GlobalModifiersMap[mod.Name].Add(new GlobalModifierMap(globalModifier.Name, globalModifier));
		//		}

		internal static Exception ThrowException(string message)
			=> new Exception($"{Loot.Instance.DisplayName ?? "EvenMoreModifiers"}: {message}");

		private static ModifierPool GetNewPoolInstance(ModifierPool pool)
		{
			var instance = pool;
			if (pool != null)
			{
				instance = (ModifierPool)Activator.CreateInstance(pool.GetType());
				instance.Type = pool.Type;
				instance.Mod = pool.Mod;
			}
			return instance;
		}

		public static ModifierPool GetModifierPool(string modname, string poolTypeName)
		{
			var pool = _GetModifierPool(modname, poolTypeName);
			return GetNewPoolInstance(pool);
		}

		public static ModifierPool GetModifierPool(Type type)
		{
			var pool = Pools.Values.FirstOrDefault(x => x.GetType().FullName == type.FullName);
			return GetNewPoolInstance(pool);
		}

		private static Modifier GetNewModifierInstance(Modifier modifier)
		{
			var instance = modifier;
			if (modifier != null)
			{
				instance = (Modifier)Activator.CreateInstance(modifier.GetType());
				instance.Type = modifier.Type;
				instance.Mod = modifier.Mod;
			}
			return instance;
		}

		public static Modifier GetModifier(string modname, string poolTypeName)
		{
			var modifier = _GetModifier(modname, poolTypeName);
			return GetNewModifierInstance(modifier);
		}

		public static Modifier GetModifier(Type type)
		{
			var pool = Modifiers.Values.FirstOrDefault(x => x.GetType().FullName == type.FullName);
			return GetNewModifierInstance(pool);
		}

		private static ModifierRarity GetNewRarityInstance(ModifierRarity rarity)
		{
			var instance = rarity;
			if (rarity != null)
			{
				instance = (ModifierRarity)Activator.CreateInstance(rarity.GetType());
				instance.Type = rarity.Type;
				instance.Mod = rarity.Mod;
			}
			return instance;
		}


		public static ModifierRarity GetModifierRarity(string modname, string rarityTypeName)
		{
			var rarity = _GetModifierRarity(modname, rarityTypeName);
			return GetNewRarityInstance(rarity);
		}

		public static ModifierRarity GetModifierRarity(Type type)
		{
			var rarity = Rarities.Values.FirstOrDefault(x => x.GetType().FullName == type.FullName);
			return GetNewRarityInstance(rarity);
		}

		private static ModifierEffect GetNewEffectInstance(ModifierEffect effect)
		{
			var instance = effect;
			if (effect != null)
			{
				instance = (ModifierEffect)Activator.CreateInstance(effect.GetType());
				instance.Type = effect.Type;
				instance.Mod = effect.Mod;
			}
			return instance;
		}

		public static ModifierEffect GetModifierEffect(string modname, string effectTypeName)
		{
			var effect = _GetModifierEffect(modname, effectTypeName);
			return GetNewEffectInstance(effect);
		}

		public static ModifierEffect GetModifierEffect(Type type)
		{
			var effect = Effects.Values.FirstOrDefault(x => x.GetType().FullName == type.FullName);
			return GetNewEffectInstance(effect);
		}

		private static ModifierPool _GetModifierPool(string modname, string poolTypeName)
		{
			return PoolsMap[modname].FirstOrDefault(x => x.Key.Equals(poolTypeName)).Value;
		}

		private static Modifier _GetModifier(string modname, string modifierTypeName)
		{
			return ModifiersMap[modname].FirstOrDefault(x => x.Key.Equals(modifierTypeName)).Value;
		}

		private static ModifierRarity _GetModifierRarity(string modname, string rarityTypeName)
		{
			return RaritiesMap[modname].FirstOrDefault(x => x.Key.Equals(rarityTypeName)).Value;
		}

		private static ModifierEffect _GetModifierEffect(string modname, string effectTypeName)
		{
			return EffectsMap[modname].FirstOrDefault(x => x.Key.Equals(effectTypeName)).Value;
		}

		/// <summary>
		/// Requests all Modifiers, and returns them as a readonly collection
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyCollection<Modifier> RequestModifiers()
			=> Modifiers.Select(e => (Modifier)e.Value?.Clone()).ToList().AsReadOnly();

		/// <summary>
		/// Requests all ModifierRarities, and returns them as a readonly collection
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyCollection<ModifierRarity> RequestModifierRarities()
			=> Rarities.Select(r => (ModifierRarity)r.Value?.Clone()).ToList().AsReadOnly();

		/// <summary>
		/// Requests all ModifierPools, and returns them as a readonly collection
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyCollection<ModifierPool> RequestModifierPools()
			=> Pools.Select(m => (ModifierPool)m.Value?.Clone()).ToList().AsReadOnly();

		/// <summary>
		/// Requests all ModifierEffects, and returns them as a readonly collection
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyCollection<ModifierEffect> RequestModifierEffects()
			=> Effects.Select(e => (ModifierEffect)e.Value?.Clone()).ToList().AsReadOnly();
	}
}
