using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct MultiBlock
{
	public ushort ID;

	public short X;

	public short Y;

	public short Z;

	public uint Flags;

	public uint Hue;
}
