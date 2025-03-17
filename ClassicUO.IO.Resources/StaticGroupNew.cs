using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StaticGroupNew
{
	public uint Unk;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public StaticTilesNew[] Tiles;
}
