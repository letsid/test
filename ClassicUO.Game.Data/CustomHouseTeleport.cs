using System;

namespace ClassicUO.Game.Data;

internal class CustomHouseTeleport : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 16;

	public int F1;

	public int F2;

	public int F3;

	public int F4;

	public int F5;

	public int F6;

	public int F7;

	public int F8;

	public int F9;

	public int F10;

	public int F11;

	public int F12;

	public int F13;

	public int F14;

	public int F15;

	public int F16;

	public ushort[] Graphics = new ushort[16];

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 17)
		{
			flag = int.TryParse(array[0], out Category) && int.TryParse(array[1], out F1) && int.TryParse(array[2], out F2) && int.TryParse(array[3], out F3) && int.TryParse(array[4], out F4) && int.TryParse(array[5], out F5) && int.TryParse(array[6], out F6) && int.TryParse(array[7], out F7) && int.TryParse(array[8], out F8) && int.TryParse(array[9], out F9) && int.TryParse(array[10], out F10) && int.TryParse(array[11], out F11) && int.TryParse(array[12], out F12) && int.TryParse(array[13], out F13) && int.TryParse(array[14], out F14) && int.TryParse(array[15], out F15) && int.TryParse(array[16], out F16) && int.TryParse(array[17], out FeatureMask);
		}
		if (flag)
		{
			Graphics[0] = (ushort)F1;
			Graphics[1] = (ushort)F2;
			Graphics[2] = (ushort)F3;
			Graphics[3] = (ushort)F4;
			Graphics[4] = (ushort)F5;
			Graphics[5] = (ushort)F6;
			Graphics[6] = (ushort)F7;
			Graphics[7] = (ushort)F8;
			Graphics[8] = (ushort)F9;
			Graphics[9] = (ushort)F10;
			Graphics[10] = (ushort)F11;
			Graphics[11] = (ushort)F12;
			Graphics[12] = (ushort)F13;
			Graphics[13] = (ushort)F14;
			Graphics[14] = (ushort)F15;
			Graphics[15] = (ushort)F16;
		}
		return flag;
	}

	public override int Contains(ushort graphic)
	{
		for (int i = 0; i < 16; i++)
		{
			if (Graphics[i] == graphic)
			{
				return i;
			}
		}
		return -1;
	}
}
