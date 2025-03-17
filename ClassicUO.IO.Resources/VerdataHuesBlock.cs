using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct VerdataHuesBlock
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public readonly ushort[] ColorTable;

	public readonly ushort TableStart;

	public readonly ushort TableEnd;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
	public readonly char[] Name;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
	public readonly ushort[] Unk;
}
