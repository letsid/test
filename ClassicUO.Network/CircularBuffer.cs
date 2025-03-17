using System;
using System.Runtime.CompilerServices;

namespace ClassicUO.Network;

internal sealed class CircularBuffer
{
	private byte[] _buffer;

	private int _head;

	private int _tail;

	public int Length { get; private set; }

	public CircularBuffer(int size = 4096)
	{
		_buffer = new byte[size];
	}

	internal void Clear()
	{
		_head = 0;
		_tail = 0;
		Length = 0;
	}

	private void SetCapacity(int capacity)
	{
		byte[] array = new byte[capacity];
		if (Length > 0)
		{
			if (_head < _tail)
			{
				_buffer.AsSpan(_head, Length).CopyTo(array.AsSpan());
			}
			else
			{
				_buffer.AsSpan(_head, _buffer.Length - _head).CopyTo(array.AsSpan());
				_buffer.AsSpan(0, _tail).CopyTo(array.AsSpan(_buffer.Length - _head));
			}
		}
		_head = 0;
		_tail = Length;
		_buffer = array;
	}

	public void Enqueue(Span<byte> buffer)
	{
		Enqueue(buffer, 0, buffer.Length);
	}

	internal void Enqueue(Span<byte> buffer, int offset, int size)
	{
		if (Length + size > _buffer.Length)
		{
			SetCapacity((Length + size + 2047) & -2048);
		}
		if (_head < _tail)
		{
			int num = _buffer.Length - _tail;
			if (num >= size)
			{
				buffer.Slice(offset, size).CopyTo(_buffer.AsSpan(_tail));
			}
			else
			{
				buffer.Slice(offset, num).CopyTo(_buffer.AsSpan(_tail));
				buffer.Slice(offset + num, size - num).CopyTo(_buffer.AsSpan());
			}
		}
		else
		{
			buffer.Slice(offset, size).CopyTo(_buffer.AsSpan(_tail));
		}
		_tail = (_tail + size) % _buffer.Length;
		Length += size;
	}

	internal int Dequeue(Span<byte> buffer, int offset, int size)
	{
		if (size > Length)
		{
			size = Length;
		}
		if (size == 0)
		{
			return 0;
		}
		if (_head < _tail)
		{
			_buffer.AsSpan(_head, size).CopyTo(buffer.Slice(offset));
		}
		else
		{
			int num = _buffer.Length - _head;
			if (num >= size)
			{
				_buffer.AsSpan(_head, size).CopyTo(buffer.Slice(offset));
			}
			else
			{
				_buffer.AsSpan(_head, num).CopyTo(buffer.Slice(offset));
				_buffer.AsSpan(0, size - num).CopyTo(buffer.Slice(offset + num));
			}
		}
		_head = (_head + size) % _buffer.Length;
		Length -= size;
		if (Length == 0)
		{
			_head = 0;
			_tail = 0;
		}
		return size;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetID()
	{
		if (Length >= 1)
		{
			return _buffer[_head];
		}
		return byte.MaxValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetLength()
	{
		if (Length >= 3)
		{
			return _buffer[(_head + 2) % _buffer.Length] | (_buffer[(_head + 1) % _buffer.Length] << 8);
		}
		return 0;
	}
}
