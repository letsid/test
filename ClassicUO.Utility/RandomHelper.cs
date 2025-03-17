using System;

namespace ClassicUO.Utility;

internal static class RandomHelper
{
	private static readonly Random _random = new Random();

	public static int GetValue(int low, int high)
	{
		return _random.Next(low, high + 1);
	}

	public static int GetValue()
	{
		return _random.Next();
	}
}
