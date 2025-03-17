using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HuesGroup
{
	public uint Header;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public HuesBlock[] Entries;
}
