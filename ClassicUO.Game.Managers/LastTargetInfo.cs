namespace ClassicUO.Game.Managers;

internal class LastTargetInfo
{
	public ushort Graphic;

	public uint Serial;

	public ushort X;

	public ushort Y;

	public sbyte Z;

	public bool IsEntity => SerialHelper.IsValid(Serial);

	public bool IsStatic
	{
		get
		{
			if (!IsEntity && Graphic != 0)
			{
				return Graphic != ushort.MaxValue;
			}
			return false;
		}
	}

	public bool IsLand => !IsStatic;

	public void SetEntity(uint serial)
	{
		Serial = serial;
		Graphic = ushort.MaxValue;
		X = (Y = ushort.MaxValue);
		Z = sbyte.MinValue;
	}

	public void SetStatic(ushort graphic, ushort x, ushort y, sbyte z)
	{
		Serial = 0u;
		Graphic = graphic;
		X = x;
		Y = y;
		Z = z;
	}

	public void SetLand(ushort x, ushort y, sbyte z)
	{
		Serial = 0u;
		Graphic = ushort.MaxValue;
		X = x;
		Y = y;
		Z = z;
	}

	public void Clear()
	{
		Serial = 0u;
		Graphic = ushort.MaxValue;
		X = (Y = ushort.MaxValue);
		Z = sbyte.MinValue;
	}
}
