using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct LandTilesOld
{
	public uint Flags;

	public ushort TexID;

	[MarshalAs(UnmanagedType.LPStr)]
	public string Name;
}
