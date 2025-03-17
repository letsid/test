using System;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Data;

internal static class DirectionHelper
{
	public static Direction DirectionFromPoints(Point from, Point to)
	{
		return DirectionFromVectors(new Vector2(from.X, from.Y), new Vector2(to.X, to.Y));
	}

	public static Direction DirectionFromVectors(Vector2 fromPosition, Vector2 toPosition)
	{
		double num = Math.Atan2(toPosition.Y - fromPosition.Y, toPosition.X - fromPosition.X);
		if (num < 0.0)
		{
			num = Math.PI + (Math.PI + num);
		}
		double num2 = Math.PI / 4.0;
		double num3 = Math.PI / 8.0;
		int num4 = int.MaxValue;
		for (int i = 0; i < 8; i++)
		{
			if (num >= num3 && num <= num3 + num2)
			{
				num4 = i + 1;
				break;
			}
			num3 += num2;
		}
		if (num4 == int.MaxValue)
		{
			num4 = 0;
		}
		num4 = ((num4 >= 7) ? (num4 - 7) : (num4 + 1));
		return (Direction)num4;
	}

	public static Direction GetDirectionAB(int AAx, int AAy, int BBx, int BBy)
	{
		int num = AAx - BBx;
		int num2 = AAy - BBy;
		int num3 = (num - num2) * 44;
		int num4 = (num + num2) * 44;
		int num5 = Math.Abs(num3);
		int num6 = Math.Abs(num4);
		if ((num6 >> 1) - num5 >= 0)
		{
			return (num4 > 0) ? Direction.Up : Direction.Down;
		}
		if ((num5 >> 1) - num6 >= 0)
		{
			return (num3 <= 0) ? Direction.Right : Direction.Left;
		}
		if (num3 >= 0 && num4 >= 0)
		{
			return Direction.West;
		}
		if (num3 >= 0 && num4 < 0)
		{
			return Direction.South;
		}
		if (num3 < 0 && num4 < 0)
		{
			return Direction.East;
		}
		return Direction.North;
	}

	public static Direction CalculateDirection(int curX, int curY, int newX, int newY)
	{
		int num = newX - curX;
		int num2 = newY - curY;
		if (num > 0)
		{
			if (num2 > 0)
			{
				return Direction.Down;
			}
			if (num2 != 0)
			{
				return Direction.Right;
			}
			return Direction.East;
		}
		if (num == 0)
		{
			if (num2 > 0)
			{
				return Direction.South;
			}
			if (num2 != 0)
			{
				return Direction.North;
			}
			return Direction.NONE;
		}
		if (num2 > 0)
		{
			return Direction.Left;
		}
		if (num2 != 0)
		{
			return Direction.Up;
		}
		return Direction.West;
	}

	public static Direction DirectionFromKeyboardArrows(bool upPressed, bool downPressed, bool leftPressed, bool rightPressed)
	{
		int num = 237;
		if (upPressed)
		{
			num = (leftPressed ? 6 : ((!rightPressed) ? 7 : 0));
		}
		else if (downPressed)
		{
			num = (leftPressed ? 4 : ((!rightPressed) ? 3 : 2));
		}
		else if (leftPressed)
		{
			num = 5;
		}
		else if (rightPressed)
		{
			num = 1;
		}
		return (Direction)num;
	}

	public static Direction GetCardinal(Direction inDirection)
	{
		return inDirection & Direction.West;
	}

	public static Direction Reverse(Direction inDirection)
	{
		return (inDirection + 4) & Direction.Up;
	}
}
