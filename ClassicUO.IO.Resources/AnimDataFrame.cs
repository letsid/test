using System.Runtime.InteropServices;

namespace ClassicUO.IO.Resources;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct AnimDataFrame
{
	public unsafe fixed sbyte FrameData[64];

	public byte Unknown;

	public byte FrameCount;

	public byte FrameInterval;

	public byte FrameStart;
}
