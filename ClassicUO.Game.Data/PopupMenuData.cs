using ClassicUO.IO;

namespace ClassicUO.Game.Data;

internal class PopupMenuData
{
	public PopupMenuItem[] Items { get; }

	public uint Serial { get; }

	public PopupMenuItem this[int i] => Items[i];

	public PopupMenuData(uint serial, PopupMenuItem[] items)
	{
		Serial = serial;
		Items = items;
	}

	public static PopupMenuData Parse(ref StackDataReader p)
	{
		bool flag = p.ReadUInt16BE() >= 2;
		uint serial = p.ReadUInt32BE();
		byte b = p.ReadUInt8();
		PopupMenuItem[] array = new PopupMenuItem[b];
		for (int i = 0; i < b; i++)
		{
			ushort hue = ushort.MaxValue;
			ushort replaced = 0;
			int cliloc;
			ushort index;
			ushort num;
			if (flag)
			{
				cliloc = (int)p.ReadUInt32BE();
				index = p.ReadUInt16BE();
				num = p.ReadUInt16BE();
			}
			else
			{
				index = p.ReadUInt16BE();
				cliloc = p.ReadUInt16BE() + 3000000;
				num = p.ReadUInt16BE();
				if ((num & 0x84) != 0)
				{
					p.Skip(2);
				}
				if ((num & 0x40) != 0)
				{
					p.Skip(2);
				}
				if ((num & 0x20) != 0)
				{
					replaced = p.ReadUInt16BE();
				}
			}
			if ((num & 1) != 0)
			{
				hue = 902;
			}
			array[i] = new PopupMenuItem(cliloc, index, hue, replaced, num);
		}
		return new PopupMenuData(serial, array);
	}
}
