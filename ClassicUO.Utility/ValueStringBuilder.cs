using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ClassicUO.Utility;

internal ref struct ValueStringBuilder
{
	private char[] _arrayToReturnToPool;

	private Span<char> _chars;

	private int _pos;

	public int Length
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public int Capacity => _chars.Length;

	public ref char this[int index] => ref _chars[index];

	public Span<char> RawChars => _chars;

	public ValueStringBuilder(ReadOnlySpan<char> initialString)
		: this(initialString.Length)
	{
		Append(initialString);
	}

	public ValueStringBuilder(ReadOnlySpan<char> initialString, Span<char> initialBuffer)
		: this(initialBuffer)
	{
		Append(initialString);
	}

	public ValueStringBuilder(Span<char> initialBuffer)
	{
		_arrayToReturnToPool = null;
		_chars = initialBuffer;
		_pos = 0;
	}

	public ValueStringBuilder(int initialCapacity)
	{
		_arrayToReturnToPool = ArrayPool<char>.Shared.Rent(initialCapacity, zero: true);
		_chars = _arrayToReturnToPool;
		_pos = 0;
	}

	public void EnsureCapacity(int capacity)
	{
		if (capacity > _chars.Length)
		{
			Grow(capacity - _chars.Length);
		}
	}

	public ref char GetPinnableReference()
	{
		return ref MemoryMarshal.GetReference(_chars);
	}

	public ref char GetPinnableReference(bool terminate)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_chars[Length] = '\0';
		}
		return ref MemoryMarshal.GetReference(_chars);
	}

	public override string ToString()
	{
		string result = _chars.Slice(0, _pos).ToString();
		Dispose();
		return result;
	}

	public ReadOnlySpan<char> AsSpan(bool terminate)
	{
		if (terminate)
		{
			EnsureCapacity(Length + 1);
			_chars[Length] = '\0';
		}
		return _chars.Slice(0, _pos);
	}

	public ReadOnlySpan<char> AsSpan()
	{
		return _chars.Slice(0, _pos);
	}

	public ReadOnlySpan<char> AsSpan(int start)
	{
		return _chars.Slice(start, _pos - start);
	}

	public ReadOnlySpan<char> AsSpan(int start, int length)
	{
		return _chars.Slice(start, length);
	}

	public bool TryCopyTo(Span<char> destination, out int charsWritten)
	{
		if (_chars.Slice(0, _pos).TryCopyTo(destination))
		{
			charsWritten = _pos;
			Dispose();
			return true;
		}
		charsWritten = 0;
		Dispose();
		return false;
	}

	public void Insert(int index, char value, int count)
	{
		if (_pos > _chars.Length - count)
		{
			Grow(count);
		}
		int length = _pos - index;
		_chars.Slice(index, length).CopyTo(_chars.Slice(index + count));
		_chars.Slice(index, count).Fill(value);
		_pos += count;
	}

	public void Insert(int index, ReadOnlySpan<char> s)
	{
		int length = s.Length;
		if (_pos > _chars.Length - length)
		{
			Grow(length);
		}
		int length2 = _pos - index;
		_chars.Slice(index, length2).CopyTo(_chars.Slice(index + length));
		s.CopyTo(_chars.Slice(index));
		_pos += length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(char c)
	{
		int pos = _pos;
		if ((uint)pos < (uint)_chars.Length)
		{
			_chars[pos] = c;
			_pos = pos + 1;
		}
		else
		{
			GrowAndAppend(c);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(string s)
	{
		int pos = _pos;
		if (s.Length == 1 && (uint)pos < (uint)_chars.Length)
		{
			_chars[pos] = s[0];
			_pos = pos + 1;
		}
		else
		{
			AppendSlow(s);
		}
	}

	private void AppendSlow(string s)
	{
		int pos = _pos;
		if (pos > _chars.Length - s.Length)
		{
			Grow(s.Length);
		}
		s.AsSpan().CopyTo(_chars.Slice(pos));
		_pos += s.Length;
	}

	public void Append(char c, int count)
	{
		if (_pos > _chars.Length - count)
		{
			Grow(count);
		}
		Span<char> span = _chars.Slice(_pos, count);
		for (int i = 0; i < span.Length; i++)
		{
			span[i] = c;
		}
		_pos += count;
	}

	public void Append(ReadOnlySpan<char> value)
	{
		if (_pos > _chars.Length - value.Length)
		{
			Grow(value.Length);
		}
		value.CopyTo(_chars.Slice(_pos));
		_pos += value.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<char> AppendSpan(int length)
	{
		int pos = _pos;
		if (pos > _chars.Length - length)
		{
			Grow(length);
		}
		_pos = pos + length;
		return _chars.Slice(pos, length);
	}

	public void Replace(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars)
	{
		Replace(oldChars, newChars, 0, _pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Replace(ReadOnlySpan<char> oldChars, ReadOnlySpan<char> newChars, int startIndex, int count)
	{
		Span<char> span = _chars.Slice(startIndex, count);
		int num = span.IndexOf(oldChars);
		if (num == -1)
		{
			return;
		}
		if (newChars.Length > oldChars.Length)
		{
			int i;
			for (i = 0; i < oldChars.Length; i++)
			{
				span[num + i] = newChars[i];
			}
			Insert(num + i, newChars.Slice(i));
		}
		else if (newChars.Length < oldChars.Length)
		{
			int j;
			for (j = 0; j < newChars.Length; j++)
			{
				span[num + j] = newChars[j];
			}
			Remove(num + j, oldChars.Length - j);
		}
		else
		{
			newChars.CopyTo(span.Slice(0, oldChars.Length));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Replace(char oldChar, char newChar)
	{
		Span<char> chars = _chars;
		int num = chars.IndexOf(oldChar);
		if (num != -1)
		{
			chars[num] = newChar;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Remove(int startIndex, int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (startIndex < 0)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (length > _pos - startIndex)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		if (startIndex == 0)
		{
			_chars = _chars.Slice(length);
		}
		else if (startIndex + length == _pos)
		{
			_chars = _chars.Slice(0, startIndex);
		}
		else
		{
			_chars.Slice(startIndex + length).CopyTo(_chars.Slice(startIndex));
		}
		_pos -= length;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void GrowAndAppend(char c)
	{
		Grow(1);
		Append(c);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void Grow(int requiredAdditionalCapacity)
	{
		char[] array = ArrayPool<char>.Shared.Rent(Math.Max(_pos + requiredAdditionalCapacity, _chars.Length * 2), zero: true);
		_chars.CopyTo(array);
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		_chars = (_arrayToReturnToPool = array);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		char[] arrayToReturnToPool = _arrayToReturnToPool;
		this = default(ValueStringBuilder);
		if (arrayToReturnToPool != null)
		{
			ArrayPool<char>.Shared.Return(arrayToReturnToPool);
		}
	}
}
