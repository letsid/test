using System.Runtime.CompilerServices;
using ClassicUO.Game.GameObjects;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal static class SelectedObject
{
	public static Point TranslatedMousePositionByViewport;

	public static BaseGameObject Object;

	public static BaseGameObject LastObject;

	public static BaseGameObject LastLeftDownObject;

	public static Entity HealthbarObject;

	public static Item SelectedContainer;

	public static Item CorpseObject;

	private static readonly bool[,] _InternalArea;

	static SelectedObject()
	{
		_InternalArea = new bool[44, 44];
		int num = 21;
		int num2 = 0;
		while (num >= 0)
		{
			for (int i = 0; i < 22; i++)
			{
				if (i >= num2)
				{
					_InternalArea[i, num] = (_InternalArea[43 - i, 43 - num] = (_InternalArea[43 - i, num] = (_InternalArea[i, 43 - num] = true)));
				}
			}
			num--;
			num2++;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsPointInLand(int x, int y)
	{
		x = TranslatedMousePositionByViewport.X - x;
		y = TranslatedMousePositionByViewport.Y - y;
		if (x >= 0 && x < 44 && y >= 0 && y < 44)
		{
			return _InternalArea[x, y];
		}
		return false;
	}

	public static bool IsPointInStretchedLand(ref UltimaBatcher2D.YOffsets yOffsets, int x, int y)
	{
		x += 22;
		int num = TranslatedMousePositionByViewport.X - x;
		int y2 = TranslatedMousePositionByViewport.Y;
		int num2 = -yOffsets.Top;
		int num3 = 22 - yOffsets.Left;
		int num4 = 44 - yOffsets.Bottom;
		int num5 = 22 - yOffsets.Right;
		if (y2 >= num * (num3 - num2) / -22 + y + num2 && y2 >= num * (num5 - num2) / 22 + y + num2 && y2 <= num * (num5 - num4) / 22 + y + num4)
		{
			return y2 <= num * (num3 - num4) / -22 + y + num4;
		}
		return false;
	}
}
