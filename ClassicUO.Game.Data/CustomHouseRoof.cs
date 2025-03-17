using System;

namespace ClassicUO.Game.Data;

internal class CustomHouseRoof : CustomHouseObject
{
	public const int GRAPHICS_COUNT = 16;

	public ushort[] Graphics = new ushort[16];

	public int Style;

	public int TID;

	public int North;

	public int East;

	public int South;

	public int West;

	public int NSCrosspiece;

	public int EWCrosspiece;

	public int NDent;

	public int SDent;

	public int WDent;

	public int NTPiece;

	public int ETPiece;

	public int STPiece;

	public int WTPiece;

	public int XPiece;

	public int Extra;

	public int Piece;

	public override bool Parse(string text)
	{
		string[] array = text.Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		bool flag = false;
		if (array.Length >= 19)
		{
			flag = int.TryParse(array[0], out Category) && int.TryParse(array[1], out Style) && int.TryParse(array[2], out TID) && int.TryParse(array[3], out North) && int.TryParse(array[4], out East) && int.TryParse(array[5], out South) && int.TryParse(array[6], out West) && int.TryParse(array[7], out NSCrosspiece) && int.TryParse(array[8], out EWCrosspiece) && int.TryParse(array[9], out NDent) && int.TryParse(array[10], out SDent) && int.TryParse(array[11], out WDent) && int.TryParse(array[12], out NTPiece) && int.TryParse(array[13], out ETPiece) && int.TryParse(array[14], out STPiece) && int.TryParse(array[15], out WTPiece) && int.TryParse(array[16], out XPiece) && int.TryParse(array[17], out Extra) && int.TryParse(array[18], out Piece) && int.TryParse(array[19], out FeatureMask);
		}
		if (flag)
		{
			Graphics[0] = (ushort)North;
			Graphics[1] = (ushort)East;
			Graphics[2] = (ushort)South;
			Graphics[3] = (ushort)West;
			Graphics[4] = (ushort)NSCrosspiece;
			Graphics[5] = (ushort)EWCrosspiece;
			Graphics[6] = (ushort)NDent;
			Graphics[7] = (ushort)SDent;
			Graphics[8] = (ushort)WDent;
			Graphics[9] = (ushort)NTPiece;
			Graphics[10] = (ushort)ETPiece;
			Graphics[11] = (ushort)STPiece;
			Graphics[12] = (ushort)WTPiece;
			Graphics[13] = (ushort)XPiece;
			Graphics[14] = (ushort)Extra;
			Graphics[15] = (ushort)Piece;
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
