using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct MultiBlockNew
{
	public ushort ID;

	public short X;

	public short Y;

	public short Z;

	public ushort Flags;

	public uint Unknown;
}
