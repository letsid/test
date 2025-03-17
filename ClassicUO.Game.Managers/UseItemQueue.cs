using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Utility.Collections;

namespace ClassicUO.Game.Managers;

internal class UseItemQueue
{
	private readonly Deque<uint> _actions = new Deque<uint>();

	private long _timer;

	public UseItemQueue()
	{
		_timer = Time.Ticks + 1000;
	}

	public void Update(double totalTime, double frameTime)
	{
		if (_timer >= Time.Ticks)
		{
			return;
		}
		_timer = Time.Ticks + 1000;
		if (_actions.Count == 0)
		{
			return;
		}
		while (_actions.Count > 0)
		{
			uint num = _actions.RemoveFromFront();
			Entity entity = World.Get(num);
			if (!(entity != null))
			{
				continue;
			}
			if (SerialHelper.IsMobile(num))
			{
				num |= 0x80000000u;
			}
			else if (entity is Item)
			{
				Item item = (Item)entity;
				if (item.IsCorpse && item.Distance > ProfileManager.CurrentProfile.AutoOpenCorpseRange)
				{
					if (World.Player != null)
					{
						World.Player.AutoOpenedCorpses.Remove(num);
					}
					continue;
				}
			}
			GameActions.DoubleClick(num);
			break;
		}
	}

	public void Add(uint serial)
	{
		foreach (uint action in _actions)
		{
			if (serial == action)
			{
				return;
			}
		}
		_actions.AddToBack(serial);
	}

	public void Clear()
	{
		_actions.Clear();
	}

	public void ClearCorpses()
	{
		for (int i = 0; i < _actions.Count; i++)
		{
			Entity entity = World.Get(_actions[i]);
			if (!(entity == null) && entity is Item { IsCorpse: not false })
			{
				_actions.RemoveAt(i--);
			}
		}
	}
}
