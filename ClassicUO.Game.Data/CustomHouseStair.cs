using System;

namespace ClassicUO.Game.Data;

internal class CustomHouseStair : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 9;

	public int Block;

	public int North;

	public int East;

	public int South;

	public int West;

	public int Squared1;

	public int Squared2;

	public int Rounded1;

	public int Rounded2;

	public int MultiNorth;

	public int MultiEast;

	public int MultiSouth;

	public int MultiWest;

	public ushort[] Graphics = new ushort[9];

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 14)
		{
			flag = int.TryParse(array[0], out Category) && int.TryParse(array[1], out Block) && int.TryParse(array[2], out North) && int.TryParse(array[3], out East) && int.TryParse(array[4], out South) && int.TryParse(array[5], out West) && int.TryParse(array[6], out Squared1) && int.TryParse(array[7], out Squared2) && int.TryParse(array[8], out Rounded1) && int.TryParse(array[9], out Rounded2) && int.TryParse(array[10], out MultiNorth) && int.TryParse(array[11], out MultiEast) && int.TryParse(array[12], out MultiSouth) && int.TryParse(array[13], out MultiWest) && int.TryParse(array[14], out FeatureMask);
		}
		if (flag)
		{
			Graphics[0] = (ushort)((MultiNorth != 0) ? ((uint)Squared1) : 0u);
			Graphics[1] = (ushort)((MultiEast != 0) ? ((uint)Squared2) : 0u);
			Graphics[2] = (ushort)((MultiSouth != 0) ? ((uint)Rounded1) : 0u);
			Graphics[3] = (ushort)((MultiWest != 0) ? ((uint)Rounded2) : 0u);
			Graphics[4] = (ushort)Block;
			Graphics[5] = (ushort)North;
			Graphics[6] = (ushort)East;
			Graphics[7] = (ushort)South;
			Graphics[8] = (ushort)West;
		}
		return flag;
	}

	public override int Contains(ushort graphic)
	{
		for (int i = 0; i < 9; i++)
		{
			if (Graphics[i] == graphic)
			{
				return i;
			}
		}
		return -1;
	}
}
