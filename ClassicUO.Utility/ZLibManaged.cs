using System;
using System.IO;
using System.IO.Compression;
using ZLibNative;

namespace ClassicUO.Utility;

public static class ZLibManaged
{
	public static void Decompress(byte[] source, int sourceStart, int sourceLength, int offset, byte[] dest, int length)
	{
		using MemoryStream stream = new MemoryStream(source, sourceStart, sourceLength - offset, writable: true);
		using ZLIBStream zLIBStream = new ZLIBStream(stream, CompressionMode.Decompress);
		int num = 0;
		int num2 = zLIBStream.ReadByte();
		while (num < length && num2 >= 0)
		{
			dest[num] = (byte)num2;
			num++;
			num2 = zLIBStream.ReadByte();
		}
	}

	public unsafe static void Decompress(IntPtr source, int sourceLength, int offset, IntPtr dest, int length)
	{
		using UnmanagedMemoryStream stream = new UnmanagedMemoryStream((byte*)source.ToPointer(), sourceLength - offset);
		using ZLIBStream zLIBStream = new ZLIBStream(stream, CompressionMode.Decompress);
		byte* ptr = (byte*)dest.ToPointer();
		int num = 0;
		int num2 = zLIBStream.ReadByte();
		while (num < length && num2 >= 0)
		{
			ptr[num] = (byte)num2;
			num++;
			num2 = zLIBStream.ReadByte();
		}
	}

	public static void Compress(byte[] dest, ref int destLength, byte[] source)
	{
		using MemoryStream memoryStream = new MemoryStream(dest, writable: true);
		using (ZLIBStream zLIBStream = new ZLIBStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			zLIBStream.Write(source, 0, source.Length);
			zLIBStream.Flush();
		}
		destLength = (int)memoryStream.Position;
	}
}
