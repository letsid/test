using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct StaticTilesNew
{
	public TileFlag Flags;

	public byte Weight;

	public byte Layer;

	public int Count;

	public ushort AnimID;

	public ushort Hue;

	public ushort LightIndex;

	public byte Height;

	[MarshalAs(UnmanagedType.LPStr)]
	public string Name;
}
