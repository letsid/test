using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct RadarMapcells
{
	public ushort Graphic;

	public sbyte Z;

	public bool IsLand;
}
