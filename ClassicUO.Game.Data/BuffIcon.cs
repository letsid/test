using System;

namespace ClassicUO.Game.Data;

internal class BuffIcon : IEquatable<BuffIcon>
{
	public readonly ushort Graphic;

	public readonly string Text;

	public readonly long Timer;

	public readonly uint Type;

	public readonly ushort Hue;

	public BuffIcon(uint type, ushort graphic, ushort hue, long timer, string text)
	{
		Type = type;
		Graphic = graphic;
		Timer = ((timer <= 0) ? uint.MaxValue : (Time.Ticks + timer * 1000));
		Text = text;
		Hue = hue;
	}

	public bool Equals(BuffIcon other)
	{
		if (other != null)
		{
			return Type == other.Type;
		}
		return false;
	}
}
