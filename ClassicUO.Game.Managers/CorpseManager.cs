using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Utility.Collections;

namespace ClassicUO.Game.Managers;

internal class CorpseManager
{
	private readonly Deque<CorpseInfo> _corpses = new Deque<CorpseInfo>();

	public void Add(uint corpse, uint obj, Direction dir, bool run)
	{
		for (int i = 0; i < _corpses.Count; i++)
		{
			if (_corpses.GetAt(i).CorpseSerial == corpse)
			{
				return;
			}
		}
		_corpses.AddToBack(new CorpseInfo(corpse, obj, dir, run));
	}

	public void Remove(uint corpse, uint obj)
	{
		int num = 0;
		while (num < _corpses.Count)
		{
			ref CorpseInfo at = ref _corpses.GetAt(num);
			if (at.CorpseSerial == corpse || at.ObjectSerial == obj)
			{
				if (corpse != 0)
				{
					Item item = World.Items.Get(corpse);
					if (item != null)
					{
						item.Layer = (Layer)((uint)(at.Direction & Direction.Up) | (uint)(at.IsRunning ? 128 : 0));
					}
				}
				_corpses.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	public bool Exists(uint corpse, uint obj)
	{
		for (int i = 0; i < _corpses.Count; i++)
		{
			ref CorpseInfo at = ref _corpses.GetAt(i);
			if (at.CorpseSerial == corpse || at.ObjectSerial == obj)
			{
				return true;
			}
		}
		return false;
	}

	public Item GetCorpseObject(uint serial)
	{
		for (int i = 0; i < _corpses.Count; i++)
		{
			ref CorpseInfo at = ref _corpses.GetAt(i);
			if (at.ObjectSerial == serial)
			{
				return World.Items.Get(at.CorpseSerial);
			}
		}
		return null;
	}

	public void Clear()
	{
		_corpses.Clear();
	}
}
