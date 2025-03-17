using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using ClassicUO.Utility;

namespace ClassicUO.IO;

internal ref struct StackDataWriter
{
	private const MethodImplOptions IMPL_OPTION = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

	private byte[] _allocatedBuffer;

	private Span<byte> _buffer;

	private int _position;

	public byte[] AllocatedBuffer => _allocatedBuffer;

	public Span<byte> RawBuffer => _buffer;

	public ReadOnlySpan<byte> Buffer => _buffer.Slice(0, Position);

	public int Position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			return _position;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		set
		{
			_position = value;
			BytesWritten = Math.Max(value, BytesWritten);
		}
	}

	public int BytesWritten { get; private set; }

	public StackDataWriter(int initialCapacity)
	{
		this = default(StackDataWriter);
		Position = 0;
		EnsureSize(initialCapacity);
	}

	public StackDataWriter(Span<byte> span)
	{
		this = default(StackDataWriter);
		Write(span);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Seek(int position, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = position;
			break;
		case SeekOrigin.Current:
			Position += position;
			break;
		case SeekOrigin.End:
			Position = BytesWritten + position;
			break;
		}
		EnsureSize(Position - _buffer.Length + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt8(byte b)
	{
		EnsureSize(1);
		_buffer[Position] = b;
		Position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt8(sbyte b)
	{
		EnsureSize(1);
		_buffer[Position] = (byte)b;
		Position++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteBool(bool b)
	{
		WriteUInt8(b ? ((byte)1) : ((byte)0));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt16LE(ushort b)
	{
		EnsureSize(2);
		BinaryPrimitives.WriteUInt16LittleEndian(_buffer.Slice(Position), b);
		Position += 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt16LE(short b)
	{
		EnsureSize(2);
		BinaryPrimitives.WriteInt16LittleEndian(_buffer.Slice(Position), b);
		Position += 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt32LE(uint b)
	{
		EnsureSize(4);
		BinaryPrimitives.WriteUInt32LittleEndian(_buffer.Slice(Position), b);
		Position += 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt32LE(int b)
	{
		EnsureSize(4);
		BinaryPrimitives.WriteInt32LittleEndian(_buffer.Slice(Position), b);
		Position += 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt64LE(ulong b)
	{
		EnsureSize(8);
		BinaryPrimitives.WriteUInt64LittleEndian(_buffer.Slice(Position), b);
		Position += 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt64LE(long b)
	{
		EnsureSize(8);
		BinaryPrimitives.WriteInt64LittleEndian(_buffer.Slice(Position), b);
		Position += 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUnicodeLE(string str)
	{
		WriteString<char>(Encoding.Unicode, str, -1);
		WriteUInt16LE(0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUnicodeLE(string str, int length)
	{
		WriteString<char>(Encoding.Unicode, str, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt16BE(ushort b)
	{
		EnsureSize(2);
		BinaryPrimitives.WriteUInt16BigEndian(_buffer.Slice(Position), b);
		Position += 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt16BE(short b)
	{
		EnsureSize(2);
		BinaryPrimitives.WriteInt16BigEndian(_buffer.Slice(Position), b);
		Position += 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt32BE(uint b)
	{
		EnsureSize(4);
		BinaryPrimitives.WriteUInt32BigEndian(_buffer.Slice(Position), b);
		Position += 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt32BE(int b)
	{
		EnsureSize(4);
		BinaryPrimitives.WriteInt32BigEndian(_buffer.Slice(Position), b);
		Position += 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUInt64BE(ulong b)
	{
		EnsureSize(8);
		BinaryPrimitives.WriteUInt64BigEndian(_buffer.Slice(Position), b);
		Position += 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteInt64BE(long b)
	{
		EnsureSize(8);
		BinaryPrimitives.WriteInt64BigEndian(_buffer.Slice(Position), b);
		Position += 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUnicodeBE(string str)
	{
		WriteString<char>(Encoding.BigEndianUnicode, str, -1);
		WriteUInt16BE(0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUnicodeBE(string str, int length)
	{
		WriteString<char>(Encoding.BigEndianUnicode, str, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteUTF8(string str, int len)
	{
		WriteString<byte>(Encoding.UTF8, str, len);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteASCII(string str)
	{
		WriteString<byte>(StringHelper.Cp1252Encoding, str, -1);
		WriteUInt8(0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteASCII(string str, int length)
	{
		WriteString<byte>(StringHelper.Cp1252Encoding, str, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void WriteZero(int count)
	{
		if (count > 0)
		{
			EnsureSize(count);
			_buffer.Slice(Position, count).Fill(0);
			Position += count;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Write(ReadOnlySpan<byte> span)
	{
		EnsureSize(span.Length);
		span.CopyTo(_buffer.Slice(Position));
		Position += span.Length;
	}

	private void WriteString<T>(Encoding encoding, string str, int length) where T : struct, IEquatable<T>
	{
		int num = Unsafe.SizeOf<T>();
		if (num > 2)
		{
			throw new InvalidConstraintException("WriteString only accepts byte, sbyte, char, short, and ushort as a constraint");
		}
		if (str == null)
		{
			str = string.Empty;
		}
		int num2 = ((length > -1) ? (length * num) : encoding.GetByteCount(str));
		if (num2 != 0)
		{
			EnsureSize(num2);
			int charCount = Math.Min((length > -1) ? length : str.Length, str.Length);
			int bytes = encoding.GetBytes(str, 0, charCount, _allocatedBuffer, Position);
			Position += bytes;
			if (length > -1)
			{
				WriteZero(length * num - bytes);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void EnsureSize(int size)
	{
		if (Position + size > _buffer.Length)
		{
			Rent(Math.Max(BytesWritten + size, _buffer.Length * 2));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Rent(int size)
	{
		byte[] array = ArrayPool<byte>.Shared.Rent(size);
		if (_allocatedBuffer != null)
		{
			_buffer.Slice(0, BytesWritten).CopyTo(array);
			Return();
		}
		_buffer = (_allocatedBuffer = array);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Return()
	{
		if (_allocatedBuffer != null)
		{
			ArrayPool<byte>.Shared.Return(_allocatedBuffer);
			_allocatedBuffer = null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public void Dispose()
	{
		Return();
	}
}
