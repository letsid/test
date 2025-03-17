using System.Collections.Generic;
using ClassicUO.Network;

namespace ClassicUO.Game.Managers;

internal sealed class ObjectPropertiesListManager
{
	private readonly Dictionary<uint, ItemProperty> _itemsProperties = new Dictionary<uint, ItemProperty>();

	public void Add(uint serial, uint revision, string name, string data)
	{
		if (!_itemsProperties.TryGetValue(serial, out var value))
		{
			value = new ItemProperty();
			_itemsProperties[serial] = value;
		}
		value.Serial = serial;
		value.Revision = revision;
		value.Name = name;
		value.Data = data;
	}

	public bool Contains(uint serial)
	{
		if (_itemsProperties.TryGetValue(serial, out var _))
		{
			return true;
		}
		PacketHandlers.AddMegaClilocRequest(serial);
		return false;
	}

	public bool IsRevisionEquals(uint serial, uint revision)
	{
		if (_itemsProperties.TryGetValue(serial, out var value))
		{
			if ((revision & -1073741825) != value.Revision)
			{
				return revision == value.Revision;
			}
			return true;
		}
		return false;
	}

	public bool TryGetRevision(uint serial, out uint revision)
	{
		if (_itemsProperties.TryGetValue(serial, out var value))
		{
			revision = value.Revision;
			return true;
		}
		revision = 0u;
		return false;
	}

	public bool TryGetNameAndData(uint serial, out string name, out string data)
	{
		if (_itemsProperties.TryGetValue(serial, out var value))
		{
			name = value.Name;
			data = value.Data;
			return true;
		}
		name = (data = null);
		return false;
	}

	public void Remove(uint serial)
	{
		_itemsProperties.Remove(serial);
	}

	public void Clear()
	{
		_itemsProperties.Clear();
	}
}
