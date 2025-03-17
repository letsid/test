using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO;

internal class DataReader
{
	private unsafe byte* _data;

	private GCHandle _handle;

	internal long Position { get; set; }

	internal long Length { get; private set; }

	internal unsafe IntPtr StartAddress => (IntPtr)_data;

	internal unsafe IntPtr PositionAddress => (IntPtr)(_data + Position);

	public bool IsEOF => Position >= Length;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ReleaseData()
	{
		if (_handle.IsAllocated)
		{
			_handle.Free();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void SetData(byte* data, long length)
	{
		ReleaseData();
		_data = data;
		Length = length;
		Position = 0L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void SetData(byte[] data, long length)
	{
		ReleaseData();
		_handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		_data = (byte*)(void*)_handle.AddrOfPinnedObject();
		Length = length;
		Position = 0L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void SetData(IntPtr data, long length)
	{
		SetData((byte*)(void*)data, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void SetData(IntPtr data)
	{
		SetData((byte*)(void*)data, Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Seek(long idx)
	{
		Position = idx;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Seek(int idx)
	{
		Position = idx;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Skip(int count)
	{
		Position += count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe byte ReadByte()
	{
		return _data[Position++];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal sbyte ReadSByte()
	{
		return (sbyte)ReadByte();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal bool ReadBool()
	{
		return ReadByte() != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe short ReadShort()
	{
		short result = *(short*)(_data + Position);
		Position += 2L;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe ushort ReadUShort()
	{
		ushort result = *(ushort*)(_data + Position);
		Position += 2L;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe int ReadInt()
	{
		int result = *(int*)(_data + Position);
		Position += 4L;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe uint ReadUInt()
	{
		int result = *(int*)(_data + Position);
		Position += 4L;
		return (uint)result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe long ReadLong()
	{
		long result = *(long*)(_data + Position);
		Position += 8L;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe ulong ReadULong()
	{
		long result = *(long*)(_data + Position);
		Position += 8L;
		return (ulong)result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe byte[] ReadArray(int count)
	{
		byte[] array = new byte[count];
		fixed (byte* destination = array)
		{
			Buffer.MemoryCopy(_data + Position, destination, count, count);
		}
		Position += count;
		return array;
	}

	internal string ReadASCII(int size)
	{
		Span<char> initialBuffer = stackalloc char[size];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		for (int i = 0; i < size; i++)
		{
			char c = (char)ReadByte();
			if (c != 0)
			{
				valueStringBuilder.Append(c);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	private void EnsureSize(int size)
	{
		if (Position + size > Length)
		{
			Log.Error($"size out of range. {Position + size} > {Length}");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort ReadUShortReversed()
	{
		return (ushort)((ReadByte() << 8) | ReadByte());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint ReadUIntReversed()
	{
		return (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
	}
}
