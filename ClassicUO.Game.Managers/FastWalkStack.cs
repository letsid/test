namespace ClassicUO.Game.Managers;

internal class FastWalkStack
{
	private readonly uint[] _keys = new uint[5];

	public void SetValue(int index, uint value)
	{
		if (index >= 0 && index < 5)
		{
			_keys[index] = value;
		}
	}

	public void AddValue(uint value)
	{
		for (int i = 0; i < 5; i++)
		{
			if (_keys[i] == 0)
			{
				_keys[i] = value;
				break;
			}
		}
	}

	public uint GetValue()
	{
		for (int i = 0; i < 5; i++)
		{
			uint num = _keys[i];
			if (num != 0)
			{
				_keys[i] = 0u;
				return num;
			}
		}
		return 0u;
	}
}
