namespace ClassicUO.Game.Data;

internal readonly struct PopupMenuItem
{
	public readonly int Cliloc;

	public readonly ushort Index;

	public readonly ushort Hue;

	public readonly ushort ReplacedHue;

	public readonly ushort Flags;

	public PopupMenuItem(int cliloc, ushort index, ushort hue, ushort replaced, ushort flags)
	{
		Cliloc = cliloc;
		Index = index;
		Hue = hue;
		ReplacedHue = replaced;
		Flags = flags;
	}
}
