using System.Collections.Generic;

namespace ClassicUO.Game.Managers;

internal class ActiveSpellIconsManager
{
	private readonly HashSet<ushort> _activeIcons = new HashSet<ushort>();

	public void Add(ushort id)
	{
		if (!IsActive(id))
		{
			_activeIcons.Add(id);
		}
	}

	public void Remove(ushort id)
	{
		if (IsActive(id))
		{
			_activeIcons.Remove(id);
		}
	}

	public bool IsActive(ushort id)
	{
		if (_activeIcons.Count != 0)
		{
			return _activeIcons.Contains(id);
		}
		return false;
	}

	public void Clear()
	{
		_activeIcons.Clear();
	}
}
