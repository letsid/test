using System;
using ClassicUO.Game.GameObjects;

namespace ClassicUO.Game.Managers;

internal class PartyMember : IEquatable<PartyMember>
{
	private string _name;

	public uint Serial;

	public string Name
	{
		get
		{
			Mobile mobile = World.Mobiles.Get(Serial);
			if (mobile != null)
			{
				_name = mobile.Name;
				if (string.IsNullOrEmpty(_name))
				{
					_name = string.Empty;
				}
			}
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public PartyMember(uint serial)
	{
		Serial = serial;
		_name = Name;
	}

	public bool Equals(PartyMember other)
	{
		if (other == null)
		{
			return false;
		}
		return other.Serial == Serial;
	}
}
