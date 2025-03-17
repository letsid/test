using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct StaticsBlock
{
	public ushort Color;

	public byte X;

	public byte Y;

	public sbyte Z;

	public ushort Hue;
}
