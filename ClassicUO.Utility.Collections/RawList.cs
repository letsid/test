using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ClassicUO.Utility.Collections;

public class RawList<T> : IEnumerable<T>, IEnumerable
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly RawList<T> _list;

		private uint _currentIndex;

		public T Current => _list._items[_currentIndex];

		object IEnumerator.Current => Current;

		public Enumerator(RawList<T> list)
		{
			_list = list;
			_currentIndex = uint.MaxValue;
		}

		public bool MoveNext()
		{
			_currentIndex++;
			return _currentIndex < _list._count;
		}

		public void Reset()
		{
			_currentIndex = 0u;
		}

		public void Dispose()
		{
		}
	}

	public const uint DefaultCapacity = 4u;

	private const float GrowthFactor = 2f;

	private uint _count;

	private T[] _items;

	public uint Count
	{
		get
		{
			return _count;
		}
		set
		{
			Resize(value);
		}
	}

	public T[] Items => _items;

	public ArraySegment<T> ArraySegment => new ArraySegment<T>(_items, 0, (int)_count);

	public ref T this[uint index]
	{
		get
		{
			ValidateIndex(index);
			return ref _items[index];
		}
	}

	public ref T this[int index]
	{
		get
		{
			ValidateIndex(index);
			return ref _items[index];
		}
	}

	public RawList()
		: this(4u)
	{
	}

	public RawList(uint capacity)
	{
		if (capacity > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_items = ((capacity == 0) ? Array.Empty<T>() : new T[capacity]);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(ref T item)
	{
		if (_count == _items.Length)
		{
			Array.Resize(ref _items, (int)((float)_items.Length * 2f));
		}
		_items[_count] = item;
		_count++;
	}

	public void Add(T item)
	{
		if (_count == _items.Length)
		{
			Array.Resize(ref _items, (int)((float)_items.Length * 2f));
		}
		_items[_count] = item;
		_count++;
	}

	public void AddRange(T[] items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		int num = (int)(_count + items.Length);
		if (num > _items.Length)
		{
			Array.Resize(ref _items, (int)((float)num * 2f));
		}
		Array.Copy(items, 0, _items, (int)_count, items.Length);
		_count += (uint)items.Length;
	}

	public void AddRange(IEnumerable<T> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		foreach (T item in items)
		{
			Add(item);
		}
	}

	public void Replace(uint index, ref T item)
	{
		ValidateIndex(index);
		_items[index] = item;
	}

	public void Resize(uint count)
	{
		Array.Resize(ref _items, (int)count);
		_count = count;
	}

	public void Replace(uint index, T item)
	{
		Replace(index, ref item);
	}

	public bool Remove(ref T item)
	{
		uint index;
		bool index2 = GetIndex(item, out index);
		if (index2)
		{
			CoreRemoveAt(index);
		}
		return index2;
	}

	public bool Remove(T item)
	{
		uint index;
		bool index2 = GetIndex(item, out index);
		if (index2)
		{
			CoreRemoveAt(index);
		}
		return index2;
	}

	public void RemoveAt(uint index)
	{
		ValidateIndex(index);
		CoreRemoveAt(index);
	}

	public void Clear()
	{
		Array.Clear(_items, 0, _items.Length);
	}

	public bool GetIndex(T item, out uint index)
	{
		return (index = (uint)Array.IndexOf(_items, item)) != uint.MaxValue;
	}

	public void Sort()
	{
		Sort(null);
	}

	public void Sort(IComparer<T> comparer)
	{
		if (comparer == null)
		{
			throw new ArgumentNullException("comparer");
		}
		Array.Sort(_items, 0, (int)Count, comparer);
	}

	public void TransformAll(Func<T, T> transformation)
	{
		if (transformation == null)
		{
			throw new ArgumentNullException("transformation");
		}
		for (int i = 0; i < _count; i++)
		{
			_items[i] = transformation(_items[i]);
		}
	}

	public ReadOnlyArrayView<T> GetReadOnlyView()
	{
		return new ReadOnlyArrayView<T>(_items, 0u, _count);
	}

	public ReadOnlyArrayView<T> GetReadOnlyView(uint start, uint count)
	{
		if (start + count >= _count)
		{
			throw new ArgumentOutOfRangeException();
		}
		return new ReadOnlyArrayView<T>(_items, start, count);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CoreRemoveAt(uint index)
	{
		_count--;
		Array.Copy(_items, (int)(index + 1), _items, (int)index, (int)(_count - index));
		_items[_count] = default(T);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ValidateIndex(uint index)
	{
		if (index >= _count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ValidateIndex(int index)
	{
		if (index < 0 || index >= _count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
