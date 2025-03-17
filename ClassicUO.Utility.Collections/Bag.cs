using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassicUO.Utility.Collections;

internal class Bag<T> : IEnumerable<T>, IEnumerable
{
	internal struct BagEnumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private volatile Bag<T> _bag;

		private volatile int _index;

		T IEnumerator<T>.Current => _bag[_index];

		object IEnumerator.Current => _bag[_index];

		public BagEnumerator(Bag<T> bag)
		{
			_bag = bag;
			_index = -1;
		}

		public bool MoveNext()
		{
			return ++_index < _bag.Count;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
		}
	}

	private readonly bool _isPrimitive;

	private T[] _items;

	public int Capacity => _items.Length;

	public bool IsEmpty => Count == 0;

	public int Count { get; private set; }

	public T this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			EnsureCapacity(index + 1);
			if (index >= Count)
			{
				Count = index + 1;
			}
			_items[index] = value;
		}
	}

	public Bag(int capacity = 16)
	{
		_isPrimitive = typeof(T).IsPrimitive;
		_items = new T[capacity];
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new BagEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new BagEnumerator(this);
	}

	public void Add(T element)
	{
		EnsureCapacity(Count + 1);
		_items[Count] = element;
		int count = Count + 1;
		Count = count;
	}

	public void AddRange(Bag<T> range)
	{
		int i = 0;
		for (int count = range.Count; count > i; i++)
		{
			Add(range[i]);
		}
	}

	public void Clear()
	{
		if (Count != 0)
		{
			Count = 0;
			if (!_isPrimitive)
			{
				Array.Clear(_items, 0, Count);
			}
		}
	}

	public bool Contains(T element)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			ref T reference = ref element;
			T val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			if (reference.Equals(_items[num]))
			{
				return true;
			}
		}
		return false;
	}

	public T RemoveAt(int index)
	{
		T result = _items[index];
		int count = Count - 1;
		Count = count;
		_items[index] = _items[Count];
		_items[Count] = default(T);
		return result;
	}

	public bool Remove(T element)
	{
		for (int num = Count - 1; num >= 0; num--)
		{
			ref T reference = ref element;
			T val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			if (reference.Equals(_items[num]))
			{
				int count = Count - 1;
				Count = count;
				_items[num] = _items[Count];
				val = (_items[Count] = default(T));
				return true;
			}
		}
		return false;
	}

	public bool RemoveAll(Bag<T> bag)
	{
		bool result = false;
		for (int num = bag.Count - 1; num >= 0; num--)
		{
			if (Remove(bag[num]))
			{
				result = true;
			}
		}
		return result;
	}

	private void EnsureCapacity(int capacity)
	{
		if (capacity >= _items.Length)
		{
			int num = Math.Max((int)((double)_items.Length * 1.5), capacity);
			T[] items = _items;
			_items = new T[num];
			Array.Copy(items, 0, _items, 0, items.Length);
		}
	}
}
