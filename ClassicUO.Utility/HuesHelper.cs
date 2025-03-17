using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace ClassicUO.Utility;

internal static class HuesHelper
{
	private static readonly byte[] _table = new byte[32]
	{
		0, 8, 16, 24, 32, 41, 49, 57, 65, 74,
		82, 90, 98, 106, 115, 123, 131, 139, 148, 156,
		164, 172, 180, 189, 197, 205, 213, 222, 230, 238,
		246, 255
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (byte, byte, byte, byte) GetBGRA(uint cl)
	{
		return ((byte)(cl & 0xFF), (byte)((cl >> 8) & 0xFF), (byte)((cl >> 16) & 0xFF), (byte)((cl >> 24) & 0xFF));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint RgbaToArgb(uint rgba)
	{
		return (rgba >> 8) | (rgba << 24);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Color16To32(ushort c)
	{
		return (uint)(_table[(c >> 10) & 0x1F] | (_table[(c >> 5) & 0x1F] << 8) | (_table[c & 0x1F] << 16));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Color16To32R(ushort c)
	{
		return (uint)((_table[(c >> 10) & 0x1F] << 16) | (_table[(c >> 5) & 0x1F] << 8) | _table[c & 0x1F]);
	}

	public static uint BGRA2ARGB(uint c)
	{
		return ((c & 0xFF00) << 8) | ((c & 0xFF0000) >> 8) | ((c & 0xFF000000u) >> 24) | ((c & 0xFF) << 24);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort Color32To16(uint c)
	{
		return (ushort)(((c & 0xFF) << 5 >> 8) | (((c >> 16) & 0xFF) << 5 >> 8 << 10) | (((c >> 8) & 0xFF) << 5 >> 8 << 5));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ConvertToGray(ushort c)
	{
		return (ushort)(((c & 0x1F) * 299 + ((c >> 5) & 0x1F) * 587 + ((c >> 10) & 0x1F) * 114) / 1000);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ushort ColorToHue(Color c)
	{
		ushort r = c.R;
		ushort g = c.G;
		ushort b = c.B;
		ushort num = (ushort)((double)(int)r * (31.0 / 255.0));
		if (num == 0 && r != 0)
		{
			num = 1;
		}
		ushort num2 = (ushort)((double)(int)g * (31.0 / 255.0));
		if (num2 == 0 && g != 0)
		{
			num2 = 1;
		}
		ushort num3 = (ushort)((double)(int)b * (31.0 / 255.0));
		if (num3 == 0 && b != 0)
		{
			num3 = 1;
		}
		return (ushort)((num << 10) | (num2 << 5) | num3);
	}
}
