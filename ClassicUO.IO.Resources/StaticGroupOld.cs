using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StaticGroupOld
{
	public uint Unk;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public StaticTilesOld[] Tiles;
}
