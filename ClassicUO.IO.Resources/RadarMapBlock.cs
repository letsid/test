using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RadarMapBlock
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
	public RadarMapcells[,] Cells;
}
