using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct LandGroupNew
{
	public uint Unknown;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public LandTilesNew[] Tiles;
}
