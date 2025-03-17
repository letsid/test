using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace ClassicUO.Utility;

internal static class MathHelper
{
	public static readonly float MachineEpsilonFloat = GetMachineEpsilonFloat();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool InRange(int input, int low, int high)
	{
		if (input >= low)
		{
			return input <= high;
		}
		return false;
	}

	public static int GetDistance(Point current, Point target)
	{
		int num = Math.Abs(target.X - current.X);
		int num2 = Math.Abs(target.Y - current.Y);
		if (num2 > num)
		{
			num = num2;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong Combine(int val1, int val2)
	{
		return (ulong)(val1 | ((long)val2 << 32));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void GetNumbersFromCombine(ulong b, out int val1, out int val2)
	{
		val1 = (int)(0xFFFFFFFFu & b);
		val2 = (int)(b >> 32);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int PercetangeOf(int current, int max)
	{
		if (current <= 0 || max <= 0)
		{
			return 0;
		}
		return current * 100 / max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int PercetangeOf(int max, int current, int maxValue)
	{
		if (max > 0)
		{
			max = current * 100 / max;
			if (max > 100)
			{
				max = 100;
			}
			if (max > 1)
			{
				max = maxValue * max / 100;
			}
		}
		return max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double Hypotenuse(float a, float b)
	{
		return Math.Sqrt(Math.Pow(a, 2.0) + Math.Pow(b, 2.0));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float AngleBetweenVectors(Vector2 from, Vector2 to)
	{
		return (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
	}

	private static float GetMachineEpsilonFloat()
	{
		float num = 1f;
		float num2;
		do
		{
			num *= 0.5f;
			num2 = 1f + num;
		}
		while (num2 > 1f);
		return num;
	}
}
