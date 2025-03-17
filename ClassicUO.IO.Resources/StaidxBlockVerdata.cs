using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct StaidxBlockVerdata
{
	public uint Position;

	public ushort Size;

	public byte Unknown;
}
