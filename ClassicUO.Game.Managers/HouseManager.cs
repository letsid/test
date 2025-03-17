using System;
using System.Collections.Generic;
using ClassicUO.Game.GameObjects;

namespace ClassicUO.Game.Managers;

internal class HouseManager
{
	private readonly Dictionary<uint, House> _houses = new Dictionary<uint, House>();

	public IReadOnlyCollection<House> Houses => _houses.Values;

	public void Add(uint serial, House revision)
	{
		_houses[serial] = revision;
	}

	public bool TryGetHouse(uint serial, out House house)
	{
		return _houses.TryGetValue(serial, out house);
	}

	public bool TryToRemove(uint serial, int distance)
	{
		if (!IsHouseInRange(serial, distance))
		{
			if (_houses.TryGetValue(serial, out var value))
			{
				value.ClearComponents();
				_houses.Remove(serial);
			}
			return true;
		}
		return false;
	}

	public bool IsHouseInRange(uint serial, int distance)
	{
		if (TryGetHouse(serial, out var _))
		{
			int x = World.RangeSize.X;
			int y = World.RangeSize.Y;
			Item item = World.Items.Get(serial);
			if (item == null)
			{
				return true;
			}
			distance += item.MultiDistanceBonus;
			if (Math.Abs(item.X - x) <= distance)
			{
				return Math.Abs(item.Y - y) <= distance;
			}
			return false;
		}
		return false;
	}

	public bool EntityIntoHouse(uint house, GameObject obj)
	{
		if (obj != null && TryGetHouse(house, out var _))
		{
			Item item = World.Items.Get(house);
			if (item == null || !item.MultiInfo.HasValue)
			{
				return true;
			}
			int num = item.X + item.MultiInfo.Value.X;
			int num2 = item.X + item.MultiInfo.Value.Width;
			int num3 = item.Y + item.MultiInfo.Value.Y;
			int num4 = item.Y + item.MultiInfo.Value.Height;
			if (obj.X >= num && obj.X <= num2 && obj.Y >= num3)
			{
				return obj.Y <= num4;
			}
			return false;
		}
		return false;
	}

	public void Remove(uint serial)
	{
		if (TryGetHouse(serial, out var house))
		{
			house.ClearComponents();
			_houses.Remove(serial);
		}
	}

	public void RemoveMultiTargetHouse()
	{
		if (_houses.TryGetValue(0u, out var value))
		{
			value.ClearComponents();
			_houses.Remove(0u);
		}
	}

	public bool Exists(uint serial)
	{
		return _houses.ContainsKey(serial);
	}

	public void Clear()
	{
		foreach (KeyValuePair<uint, House> house in _houses)
		{
			house.Value.ClearComponents();
		}
		_houses.Clear();
	}
}
