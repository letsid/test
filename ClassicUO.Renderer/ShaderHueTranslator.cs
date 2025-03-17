using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace ClassicUO.Renderer;

internal static class ShaderHueTranslator
{
	public const byte SHADER_NONE = 0;

	public const byte SHADER_HUED = 1;

	public const byte SHADER_PARTIAL_HUED = 2;

	public const byte SHADER_TEXT_HUE_NO_BLACK = 3;

	public const byte SHADER_TEXT_HUE = 4;

	public const byte SHADER_LAND = 5;

	public const byte SHADER_LAND_HUED = 6;

	public const byte SHADER_SPECTRAL = 7;

	public const byte SHADER_SHADOW = 8;

	public const byte SHADER_LIGHTS = 9;

	public const byte SHADER_EFFECT_HUED = 10;

	private const byte GUMP_OFFSET = 20;

	private const ushort SPECTRAL_COLOR_FLAG = 16384;

	public static readonly Vector3 SelectedHue = new Vector3(23f, 1f, 0f);

	public static readonly Vector3 SelectedItemHue = new Vector3(53f, 1f, 0f);

	public static Vector3 GetHueVector(int hue)
	{
		return GetHueVector(hue, partial: false, 1f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3 GetHueVector(int hue, bool partial, float alpha, bool gump = false, bool effect = false)
	{
		if ((hue & 0x8000) != 0)
		{
			partial = !partial;
			hue &= 0x7FFF;
		}
		if (hue == 0)
		{
			partial = false;
		}
		byte b;
		if ((hue & 0x4000) != 0)
		{
			b = 7;
		}
		else if (hue != 0)
		{
			hue--;
			b = (byte)(effect ? 10 : ((!partial) ? 1 : 2));
			if (gump && !effect)
			{
				b += 20;
			}
		}
		else
		{
			b = 0;
		}
		Vector3 result = default(Vector3);
		result.X = hue;
		result.Y = (int)b;
		result.Z = alpha;
		return result;
	}
}
