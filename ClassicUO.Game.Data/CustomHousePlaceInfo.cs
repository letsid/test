using System;

namespace ClassicUO.Game.Data;

internal class CustomHousePlaceInfo : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 1;

	public int Graphic;

	public int Top;

	public int Bottom;

	public int AdjUN;

	public int AdjLN;

	public int AdjUE;

	public int AdjLE;

	public int AdjUS;

	public int AdjLS;

	public int AdjUW;

	public int AdjLW;

	public int DirectSupports;

	public int CanGoW;

	public int CanGoN;

	public int CanGoNWS;

	public ushort[] Graphics = new ushort[1];

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 16)
		{
			flag = int.TryParse(array[1], out Graphic) && int.TryParse(array[2], out Top) && int.TryParse(array[3], out Bottom) && int.TryParse(array[4], out AdjUN) && int.TryParse(array[5], out AdjLN) && int.TryParse(array[6], out AdjUE) && int.TryParse(array[7], out AdjLE) && int.TryParse(array[8], out AdjUS) && int.TryParse(array[9], out AdjLS) && int.TryParse(array[10], out AdjUW) && int.TryParse(array[11], out AdjLW) && int.TryParse(array[12], out DirectSupports) && int.TryParse(array[13], out CanGoW) && int.TryParse(array[14], out CanGoN) && int.TryParse(array[15], out CanGoNWS);
		}
		if (flag)
		{
			Graphics[0] = (ushort)Graphic;
		}
		return flag;
	}

	public override int Contains(ushort graphic)
	{
		for (int i = 0; i < 1; i++)
		{
			if (Graphics[i] == graphic)
			{
				return i;
			}
		}
		return -1;
	}
}
