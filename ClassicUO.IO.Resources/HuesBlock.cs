using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HuesBlock
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public ushort[] ColorTable;

	public ushort TableStart;

	public ushort TableEnd;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
	public char[] Name;
}
