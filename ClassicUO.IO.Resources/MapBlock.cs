using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 196)]
internal ref struct MapBlock
{
	public uint Header;

	public unsafe MapCells* Cells;
}
