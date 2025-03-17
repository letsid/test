using System;

namespace ClassicUO.Game.Data;

internal class CustomHouseWall : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 8;

	public ushort[] Graphics = new ushort[8];

	public int Style;

	public int TID;

	public int South1;

	public int South2;

	public int South3;

	public int Corner;

	public int East1;

	public int East2;

	public int East3;

	public int Post;

	public int WindowS;

	public int AltWindowS;

	public int WindowE;

	public int AltWindowE;

	public int SecondAltWindowS;

	public int SecondAltWindowE;

	public ushort[] WindowGraphics = new ushort[8];

	public override int Contains(ushort graphic)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Graphics[i] == graphic || WindowGraphics[i] == graphic)
			{
				return i;
			}
		}
		return -1;
	}

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 17)
		{
			flag = int.TryParse(array[0], out Category) && int.TryParse(array[1], out Style) && int.TryParse(array[2], out TID) && int.TryParse(array[3], out South1) && int.TryParse(array[4], out South2) && int.TryParse(array[5], out South3) && int.TryParse(array[6], out Corner) && int.TryParse(array[7], out East1) && int.TryParse(array[8], out East2) && int.TryParse(array[9], out East3) && int.TryParse(array[10], out Post) && int.TryParse(array[11], out WindowS) && int.TryParse(array[12], out AltWindowS) && int.TryParse(array[13], out WindowE) && int.TryParse(array[14], out AltWindowE) && int.TryParse(array[15], out SecondAltWindowS) && int.TryParse(array[16], out SecondAltWindowE) && int.TryParse(array[17], out FeatureMask);
		}
		if (flag)
		{
			WindowGraphics[0] = (Graphics[0] = (ushort)South1);
			WindowGraphics[1] = (Graphics[1] = (ushort)South2);
			WindowGraphics[2] = (Graphics[2] = (ushort)South3);
			WindowGraphics[3] = (Graphics[3] = (ushort)Corner);
			WindowGraphics[4] = (Graphics[4] = (ushort)East1);
			WindowGraphics[5] = (Graphics[5] = (ushort)East2);
			WindowGraphics[6] = (Graphics[6] = (ushort)East3);
			WindowGraphics[7] = (Graphics[7] = (ushort)Post);
		}
		if (AltWindowE == 0 && WindowE != 0)
		{
			AltWindowE = WindowE;
			WindowE = 0;
		}
		if (WindowS != 0)
		{
			WindowGraphics[0] = (ushort)WindowS;
		}
		if (AltWindowS != 0)
		{
			WindowGraphics[1] = (ushort)AltWindowS;
		}
		if (SecondAltWindowS != 0)
		{
			WindowGraphics[2] = (ushort)SecondAltWindowS;
		}
		if (WindowE != 0)
		{
			WindowGraphics[4] = (ushort)WindowE;
		}
		if (AltWindowE != 0)
		{
			WindowGraphics[5] = (ushort)AltWindowE;
		}
		if (SecondAltWindowE != 0)
		{
			WindowGraphics[6] = (ushort)SecondAltWindowE;
		}
		return flag;
	}
}
