using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Loot.Core.ContentMod
{
	public class ContentManager
	{
		private IDictionary<string, ContentMod> _contents;
		private IEnumerable<ContentMod> _ContentMods => _contents.Select(x => x.Value);

		public T GetContent<T>() where T : ContentMod
		{
			T content = (T)_ContentMods.FirstOrDefault(x => x.GetType() == typeof(T));
			return content;
		}

		public ContentMod GetContent(Type type)
		{
			ContentMod content = _ContentMods.FirstOrDefault(x => x.GetType() == type);
			return content;
		}

		public ContentMod GetContent(string key)
		{
			ContentMod content;
			return _contents.TryGetValue(key, out content) ? content : null;
		}

		public void AddContent(string key, ContentMod ContentMod)
		{
			if (_contents.ContainsKey(key))
			{
				throw new Exception($"Key '{key}' already present in ContentManager");
			}

			if (_contents.Values.Contains(ContentMod))
			{
				// TODO warn
				Loot.Logger.Error($"ContentMod with registry key {ContentMod.GetRegistryKey()} was already present");
			}

			ContentMod._Initialize();
			_contents.Add(key, ContentMod);
		}

		internal void Initialize(Mod mod)
		{
			_contents = new Dictionary<string, ContentMod>();
			var assembly = mod.Code;
			var ContentMods = assembly.GetTypes().Where(x => x.BaseType == typeof(ContentMod) && !x.IsAbstract);
			foreach (var ContentMod in ContentMods)
			{
				ContentMod obj = (ContentMod)Activator.CreateInstance(ContentMod);
				AddContent(obj.GetRegistryKey(), obj);
			}
		}

		internal void Load()
		{
			foreach (var content in _ContentMods)
			{
				content._Load();
			}
		}

		internal void Unload()
		{
			foreach (var content in _ContentMods)
			{
				content._Unload();
			}
			_contents = null;
		}
	}
}
