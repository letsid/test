using System;

namespace ClassicUO.IO;

internal struct UOFileIndex : IEquatable<UOFileIndex>
{
	public IntPtr Address;

	public uint FileSize;

	public long Offset;

	public int Length;

	public int DecompressedLength;

	public short Width;

	public short Height;

	public ushort Hue;

	public sbyte AnimOffset;

	public static UOFileIndex Invalid = new UOFileIndex(IntPtr.Zero, 0u, 0L, 0, 0, 0, 0, 0);

	public UOFileIndex(IntPtr address, uint fileSize, long offset, int length, int decompressed, short width = 0, short height = 0, ushort hue = 0)
	{
		Address = address;
		FileSize = fileSize;
		Offset = offset;
		Length = length;
		DecompressedLength = decompressed;
		Width = width;
		Height = height;
		Hue = hue;
		AnimOffset = 0;
	}

	public bool Equals(UOFileIndex other)
	{
		IntPtr address = Address;
		long offset = Offset;
		int length = Length;
		int decompressedLength = DecompressedLength;
		IntPtr address2 = other.Address;
		long offset2 = other.Offset;
		int length2 = other.Length;
		int decompressedLength2 = other.DecompressedLength;
		if (address == address2 && offset == offset2 && length == length2)
		{
			return decompressedLength == decompressedLength2;
		}
		return false;
	}
}
