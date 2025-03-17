using System.Globalization;
using System.Runtime.CompilerServices;

namespace ClassicUO.Game;

internal static class SerialHelper
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsValid(uint serial)
	{
		if (serial != 0)
		{
			return serial < 2147483648u;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsMobile(uint serial)
	{
		if (serial != 0)
		{
			return serial < 1073741824;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsItem(uint serial)
	{
		if (serial >= 1073741824)
		{
			return serial < 2147483648u;
		}
		return false;
	}

	public static uint Parse(string str)
	{
		if (str.StartsWith("0x"))
		{
			return uint.Parse(str.Remove(0, 2), NumberStyles.HexNumber);
		}
		if (str.Length > 1 && str[0] == '-')
		{
			return (uint)int.Parse(str);
		}
		return uint.Parse(str);
	}
}
