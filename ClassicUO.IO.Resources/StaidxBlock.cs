using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct StaidxBlock
{
	public uint Position;

	public uint Size;

	public uint Unknown;
}
