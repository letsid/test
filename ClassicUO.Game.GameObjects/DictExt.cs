using System.Collections.Generic;

namespace ClassicUO.Game.GameObjects;

internal static class DictExt
{
	public static T Get<T>(this Dictionary<uint, T> dict, uint serial) where T : Entity
	{
		dict.TryGetValue(serial, out var value);
		return value;
	}

	public static bool Contains<T>(this Dictionary<uint, T> dict, uint serial) where T : Entity
	{
		return dict.ContainsKey(serial);
	}

	public static bool Add<T>(this Dictionary<uint, T> dict, T entity) where T : Entity
	{
		if (dict.ContainsKey(entity.Serial))
		{
			return false;
		}
		dict[entity.Serial] = entity;
		return true;
	}
}
