using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct LandGroup
{
	public uint Unknown;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public LandTiles[] Tiles;
}
