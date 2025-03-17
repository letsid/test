namespace ClassicUO.IO.Resources;

internal struct MultilinesFontData
{
	public uint Color;

	public ushort Flags;

	public byte Font;

	public char Item;

	public ushort LinkID;

	public MultilinesFontData(uint color, ushort flags, byte font, char item, ushort linkid)
	{
		Color = color;
		Flags = flags;
		Font = font;
		Item = item;
		LinkID = linkid;
	}
}
