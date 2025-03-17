using System;

namespace ClassicUO.Game.Data;

internal class CustomHouseDoor : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 8;

	public ushort[] Graphics = new ushort[8];

	public int Piece1;

	public int Piece2;

	public int Piece3;

	public int Piece4;

	public int Piece5;

	public int Piece6;

	public int Piece7;

	public int Piece8;

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 9)
		{
			flag = int.TryParse(array[0], out Category) && int.TryParse(array[1], out Piece1) && int.TryParse(array[2], out Piece2) && int.TryParse(array[3], out Piece3) && int.TryParse(array[4], out Piece4) && int.TryParse(array[5], out Piece5) && int.TryParse(array[6], out Piece6) && int.TryParse(array[7], out Piece7) && int.TryParse(array[8], out Piece8) && int.TryParse(array[9], out FeatureMask);
		}
		if (flag)
		{
			Graphics[0] = (ushort)Piece1;
			Graphics[1] = (ushort)Piece2;
			Graphics[2] = (ushort)Piece3;
			Graphics[3] = (ushort)Piece4;
			Graphics[4] = (ushort)Piece5;
			Graphics[5] = (ushort)Piece6;
			Graphics[6] = (ushort)Piece7;
			Graphics[7] = (ushort)Piece8;
		}
		return flag;
	}

	public override int Contains(ushort graphic)
	{
		for (int i = 0; i < 8; i++)
		{
			if (Graphics[i] == graphic)
			{
				return i;
			}
		}
		return -1;
	}
}
