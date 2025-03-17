using System;

namespace ClassicUO.IO.Resources;

internal struct EquipConvData : IEquatable<EquipConvData>
{
	public ushort Graphic;

	public ushort Gump;

	public ushort Color;

	public EquipConvData(ushort graphic, ushort gump, ushort color)
	{
		Graphic = graphic;
		Gump = gump;
		Color = color;
	}

	public override int GetHashCode()
	{
		return (Graphic, Gump, Color).GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is EquipConvData other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(EquipConvData other)
	{
		ushort graphic = Graphic;
		ushort gump = Gump;
		ushort color = Color;
		ushort graphic2 = other.Graphic;
		ushort gump2 = other.Gump;
		ushort color2 = other.Color;
		if (graphic == graphic2 && gump == gump2)
		{
			return color == color2;
		}
		return false;
	}
}
