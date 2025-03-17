using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ClassicUO.Utility;

namespace ClassicUO.IO;

internal ref struct StackDataReader
{
	private const MethodImplOptions IMPL_OPTION = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

	private readonly ReadOnlySpan<byte> _data;

	public int Position { get; private set; }

	public long Length { get; }

	public int Remaining => (int)(Length - Position);

	public unsafe IntPtr StartAddress => (IntPtr)Unsafe.AsPointer(ref GetPinnableReference());

	public unsafe IntPtr PositionAddress
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			return (IntPtr)((byte*)Unsafe.AsPointer(ref GetPinnableReference()) + Position);
		}
	}

	public byte this[int index] => _data[0];

	public ReadOnlySpan<byte> Buffer => _data;

	public StackDataReader(ReadOnlySpan<byte> data)
	{
		_data = data;
		Length = data.Length;
		Position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ref byte GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(_data);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Release()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Seek(long p)
	{
		Position = (int)p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Skip(int count)
	{
		Position += count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public byte ReadUInt8()
	{
		if (Position + 1 > Length)
		{
			return 0;
		}
		return _data[Position++];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public sbyte ReadInt8()
	{
		if (Position + 1 > Length)
		{
			return 0;
		}
		return (sbyte)_data[Position++];
	}

	public bool ReadBool()
	{
		return ReadUInt8() != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ushort ReadUInt16LE()
	{
		if (Position + 2 > Length)
		{
			return 0;
		}
		BinaryPrimitives.TryReadUInt16LittleEndian(_data.Slice(Position), out var value);
		Skip(2);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public short ReadInt16LE()
	{
		if (Position + 2 > Length)
		{
			return 0;
		}
		BinaryPrimitives.TryReadInt16LittleEndian(_data.Slice(Position), out var value);
		Skip(2);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public uint ReadUInt32LE()
	{
		if (Position + 4 > Length)
		{
			return 0u;
		}
		BinaryPrimitives.TryReadUInt32LittleEndian(_data.Slice(Position), out var value);
		Skip(4);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public int ReadInt32LE()
	{
		if (Position + 4 > Length)
		{
			return 0;
		}
		int result = BinaryPrimitives.ReadInt32LittleEndian(_data.Slice(Position));
		Skip(4);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ulong ReadUInt64LE()
	{
		if (Position + 8 > Length)
		{
			return 0uL;
		}
		BinaryPrimitives.TryReadUInt64LittleEndian(_data.Slice(Position), out var value);
		Skip(8);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public long ReadInt64LE()
	{
		if (Position + 8 > Length)
		{
			return 0L;
		}
		BinaryPrimitives.TryReadInt64LittleEndian(_data.Slice(Position), out var value);
		Skip(8);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ushort ReadUInt16BE()
	{
		if (Position + 2 > Length)
		{
			return 0;
		}
		BinaryPrimitives.TryReadUInt16BigEndian(_data.Slice(Position), out var value);
		Skip(2);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public short ReadInt16BE()
	{
		if (Position + 2 > Length)
		{
			return 0;
		}
		BinaryPrimitives.TryReadInt16BigEndian(_data.Slice(Position), out var value);
		Skip(2);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public uint ReadUInt32BE()
	{
		if (Position + 4 > Length)
		{
			return 0u;
		}
		BinaryPrimitives.TryReadUInt32BigEndian(_data.Slice(Position), out var value);
		Skip(4);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public int ReadInt32BE()
	{
		if (Position + 4 > Length)
		{
			return 0;
		}
		BinaryPrimitives.TryReadInt32BigEndian(_data.Slice(Position), out var value);
		Skip(4);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public ulong ReadUInt64BE()
	{
		if (Position + 8 > Length)
		{
			return 0uL;
		}
		BinaryPrimitives.TryReadUInt64BigEndian(_data.Slice(Position), out var value);
		Skip(8);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public long ReadInt64BE()
	{
		if (Position + 8 > Length)
		{
			return 0L;
		}
		BinaryPrimitives.TryReadInt64BigEndian(_data.Slice(Position), out var value);
		Skip(8);
		return value;
	}

	public string ReadASCII(bool safe = false)
	{
		return ReadString(StringHelper.Cp1252Encoding, -1, 1, safe);
	}

	public string ReadASCII(int length, bool safe = false)
	{
		return ReadString(StringHelper.Cp1252Encoding, length, 1, safe);
	}

	public string ReadUnicodeBE(bool safe = false)
	{
		return ReadString(Encoding.BigEndianUnicode, -1, 2, safe);
	}

	public string ReadUnicodeBE(int length, bool safe = false)
	{
		return ReadString(Encoding.BigEndianUnicode, length, 2, safe);
	}

	public string ReadUnicodeLE(bool safe = false)
	{
		return ReadString(Encoding.Unicode, -1, 2, safe);
	}

	public string ReadUnicodeLE(int length, bool safe = false)
	{
		return ReadString(Encoding.Unicode, length, 2, safe);
	}

	public string ReadUTF8(bool safe = false)
	{
		return ReadString(Encoding.UTF8, -1, 1, safe);
	}

	public string ReadUTF8(int length, bool safe = false)
	{
		return ReadString(Encoding.UTF8, length, 1, safe);
	}

	public void Read(Span<byte> data, int offset, int count)
	{
		_data.Slice(Position + offset, count).CopyTo(data);
	}

	private unsafe string ReadString(Encoding encoding, int length, int sizeT, bool safe)
	{
		if (length == 0 || Position + sizeT > Length)
		{
			return string.Empty;
		}
		bool flag = length > 0;
		int remaining = Remaining;
		int num;
		if (flag)
		{
			num = length * sizeT;
			if (num > remaining)
			{
				num = remaining;
			}
		}
		else
		{
			num = remaining - (remaining & (sizeT - 1));
		}
		ReadOnlySpan<byte> span = _data.Slice(Position, num);
		int indexOfZero = GetIndexOfZero(span, sizeT);
		num = ((indexOfZero < 0) ? num : indexOfZero);
		string text;
		fixed (byte* bytes = span)
		{
			text = encoding.GetString(bytes, num);
		}
		if (safe)
		{
			Span<char> initialBuffer = stackalloc char[256];
			ReadOnlySpan<char> readOnlySpan = text.AsSpan();
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			bool flag2 = false;
			int num2 = 0;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				if (!StringHelper.IsSafeChar(readOnlySpan[i]))
				{
					flag2 = true;
					valueStringBuilder.Append(readOnlySpan.Slice(num2, i - num2));
					num2 = i + 1;
				}
			}
			if (flag2)
			{
				if (num2 < readOnlySpan.Length)
				{
					valueStringBuilder.Append(readOnlySpan.Slice(num2, readOnlySpan.Length - num2));
				}
				text = valueStringBuilder.ToString();
			}
			valueStringBuilder.Dispose();
		}
		Position += Math.Max(num + ((!flag && indexOfZero >= 0) ? sizeT : 0), length * sizeT);
		return text;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static int GetIndexOfZero(ReadOnlySpan<byte> span, int sizeT)
	{
		return sizeT switch
		{
			2 => MemoryMarshal.Cast<byte, char>(span).IndexOf('\0') * 2, 
			4 => MemoryMarshal.Cast<byte, uint>(span).IndexOf(0u) * 4, 
			_ => span.IndexOf<byte>(0), 
		};
	}
}
