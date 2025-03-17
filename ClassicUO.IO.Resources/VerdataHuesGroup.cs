using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct VerdataHuesGroup
{
	public readonly uint Header;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public readonly VerdataHuesBlock[] Entries;
}
