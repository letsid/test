using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal ref struct MapCells
{
	public ushort TileID;

	public sbyte Z;
}
