using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ClassicUO.Utility.Collections;

[DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
[DebuggerTypeProxy(typeof(Deque<>.DebugView))]
internal sealed class Deque<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>, IList, ICollection
{
	[DebuggerNonUserCode]
	private sealed class DebugView
	{
		private readonly Deque<T> deque;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] Items => deque.ToArray();

		public DebugView(Deque<T> deque)
		{
			this.deque = deque;
		}
	}

	private const int DefaultCapacity = 8;

	private T[] _buffer;

	private int _offset;

	private bool IsEmpty => Count == 0;

	private bool IsFull => Count == Capacity;

	private bool IsSplit => _offset > Capacity - Count;

	public int Capacity
	{
		get
		{
			return _buffer.Length;
		}
		set
		{
			if (value < Count)
			{
				throw new ArgumentOutOfRangeException("value", "Capacity cannot be set to a value less than Count");
			}
			if (value != _buffer.Length)
			{
				T[] array = new T[value];
				CopyToArray(array);
				_buffer = array;
				_offset = 0;
			}
		}
	}

	public int Count { get; private set; }

	bool ICollection<T>.IsReadOnly => false;

	public T this[int index]
	{
		get
		{
			CheckExistingIndexArgument(Count, index);
			return DoGetItem(index);
		}
		set
		{
			CheckExistingIndexArgument(Count, index);
			DoSetItem(index, value);
		}
	}

	bool IList.IsFixedSize => false;

	bool IList.IsReadOnly => false;

	object IList.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			if (value == null && default(T) != null)
			{
				throw new ArgumentNullException("value", "Value cannot be null.");
			}
			if (!IsT(value))
			{
				throw new ArgumentException("Value is of incorrect type.", "value");
			}
			this[index] = (T)value;
		}
	}

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public Deque(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException("capacity", "Capacity may not be negative.");
		}
		_buffer = new T[capacity];
	}

	public Deque(IEnumerable<T> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		IReadOnlyCollection<T> readOnlyCollection = CollectionHelper.ReifyCollection(collection);
		int count = readOnlyCollection.Count;
		if (count > 0)
		{
			_buffer = new T[count];
			DoInsertRange(0, readOnlyCollection);
		}
		else
		{
			_buffer = new T[8];
		}
	}

	public Deque()
		: this(8)
	{
	}

	public void Clear()
	{
		_offset = 0;
		Count = 0;
	}

	private int DequeIndexToBufferIndex(int index)
	{
		return (index + _offset) % Capacity;
	}

	private ref T DoGetItem(int index)
	{
		return ref _buffer[DequeIndexToBufferIndex(index)];
	}

	private void DoSetItem(int index, T item)
	{
		_buffer[DequeIndexToBufferIndex(index)] = item;
	}

	private void DoInsert(int index, T item)
	{
		EnsureCapacityForOneElement();
		if (index == 0)
		{
			DoAddToFront(item);
			return;
		}
		if (index == Count)
		{
			DoAddToBack(item);
			return;
		}
		DoInsertRange(index, (IReadOnlyCollection<T>)(object)new T[1] { item });
	}

	private void DoRemoveAt(int index)
	{
		if (index == 0)
		{
			DoRemoveFromFront();
		}
		else if (index == Count - 1)
		{
			DoRemoveFromBack();
		}
		else
		{
			DoRemoveRange(index, 1);
		}
	}

	private int PostIncrement(int value)
	{
		int offset = _offset;
		_offset += value;
		_offset %= Capacity;
		return offset;
	}

	private int PreDecrement(int value)
	{
		_offset -= value;
		if (_offset < 0)
		{
			_offset += Capacity;
		}
		return _offset;
	}

	private void DoAddToBack(T value)
	{
		_buffer[DequeIndexToBufferIndex(Count)] = value;
		int count = Count + 1;
		Count = count;
	}

	private void DoAddToFront(T value)
	{
		_buffer[PreDecrement(1)] = value;
		int count = Count + 1;
		Count = count;
	}

	private T DoRemoveFromBack()
	{
		T result = _buffer[DequeIndexToBufferIndex(Count - 1)];
		int count = Count - 1;
		Count = count;
		return result;
	}

	private T DoRemoveFromFront()
	{
		int count = Count - 1;
		Count = count;
		return _buffer[PostIncrement(1)];
	}

	private void DoInsertRange(int index, IReadOnlyCollection<T> collection)
	{
		int count = collection.Count;
		if (index < Count >> 1)
		{
			int num = Capacity - count;
			for (int i = 0; i != index; i++)
			{
				_buffer[DequeIndexToBufferIndex(num + i)] = _buffer[DequeIndexToBufferIndex(i)];
			}
			PreDecrement(count);
		}
		else
		{
			int num2 = Count - index;
			int num3 = index + count;
			for (int num4 = num2 - 1; num4 != -1; num4--)
			{
				_buffer[DequeIndexToBufferIndex(num3 + num4)] = _buffer[DequeIndexToBufferIndex(index + num4)];
			}
		}
		int num5 = index;
		foreach (T item in collection)
		{
			_buffer[DequeIndexToBufferIndex(num5)] = item;
			num5++;
		}
		Count += count;
	}

	private void DoRemoveRange(int index, int collectionCount)
	{
		if (index == 0)
		{
			PostIncrement(collectionCount);
			Count -= collectionCount;
			return;
		}
		if (index == Count - collectionCount)
		{
			Count -= collectionCount;
			return;
		}
		if (index + (collectionCount >> 1) < Count >> 1)
		{
			for (int num = index - 1; num != -1; num--)
			{
				_buffer[DequeIndexToBufferIndex(collectionCount + num)] = _buffer[DequeIndexToBufferIndex(num)];
			}
			PostIncrement(collectionCount);
		}
		else
		{
			int num2 = Count - collectionCount - index;
			int num3 = index + collectionCount;
			for (int i = 0; i != num2; i++)
			{
				_buffer[DequeIndexToBufferIndex(index + i)] = _buffer[DequeIndexToBufferIndex(num3 + i)];
			}
		}
		Count -= collectionCount;
	}

	private void EnsureCapacityForOneElement()
	{
		if (IsFull)
		{
			Capacity = ((Capacity == 0) ? 1 : (Capacity * 2));
		}
	}

	public void AddToBack(T value)
	{
		EnsureCapacityForOneElement();
		DoAddToBack(value);
	}

	public void AddToFront(T value)
	{
		EnsureCapacityForOneElement();
		DoAddToFront(value);
	}

	public void InsertRange(int index, IEnumerable<T> collection)
	{
		CheckNewIndexArgument(Count, index);
		IReadOnlyCollection<T> readOnlyCollection = CollectionHelper.ReifyCollection(collection);
		int count = readOnlyCollection.Count;
		if (count > Capacity - Count)
		{
			Capacity = checked(Count + count);
		}
		if (count != 0)
		{
			DoInsertRange(index, readOnlyCollection);
		}
	}

	public void RemoveRange(int offset, int count)
	{
		CheckRangeArguments(Count, offset, count);
		if (count != 0)
		{
			DoRemoveRange(offset, count);
		}
	}

	public T RemoveFromBack()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException("The deque is empty.");
		}
		return DoRemoveFromBack();
	}

	public T RemoveFromFront()
	{
		if (IsEmpty)
		{
			throw new InvalidOperationException("The deque is empty.");
		}
		return DoRemoveFromFront();
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		((ICollection<T>)this).CopyTo(array, 0);
		return array;
	}

	public ref T GetAt(int index)
	{
		return ref _buffer[DequeIndexToBufferIndex(index)];
	}

	public ref T Front()
	{
		return ref DoGetItem(0);
	}

	public ref T Back()
	{
		return ref DoGetItem(Count - 1);
	}

	public void Insert(int index, T item)
	{
		CheckNewIndexArgument(Count, index);
		DoInsert(index, item);
	}

	public void RemoveAt(int index)
	{
		CheckExistingIndexArgument(Count, index);
		DoRemoveAt(index);
	}

	public int IndexOf(T item)
	{
		EqualityComparer<T> @default = EqualityComparer<T>.Default;
		int num = 0;
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (@default.Equals(item, current))
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	void ICollection<T>.Add(T item)
	{
		DoInsert(Count, item);
	}

	bool ICollection<T>.Contains(T item)
	{
		EqualityComparer<T> @default = EqualityComparer<T>.Default;
		using (IEnumerator<T> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				if (@default.Equals(item, current))
				{
					return true;
				}
			}
		}
		return false;
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		int count = Count;
		CheckRangeArguments(array.Length, arrayIndex, count);
		CopyToArray(array, arrayIndex);
	}

	private void CopyToArray(Array array, int arrayIndex = 0)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (IsSplit)
		{
			int num = Capacity - _offset;
			Array.Copy(_buffer, _offset, array, arrayIndex, num);
			Array.Copy(_buffer, 0, array, arrayIndex + num, Count - num);
		}
		else
		{
			Array.Copy(_buffer, _offset, array, arrayIndex, Count);
		}
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		DoRemoveAt(num);
		return true;
	}

	public IEnumerator<T> GetEnumerator()
	{
		int count = Count;
		int i = 0;
		while (i != count)
		{
			yield return DoGetItem(i);
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private static bool IsT(object value)
	{
		if (value is T)
		{
			return true;
		}
		if (value != null)
		{
			return false;
		}
		return default(T) == null;
	}

	int IList.Add(object value)
	{
		if (value == null && default(T) != null)
		{
			throw new ArgumentNullException("value", "Value cannot be null.");
		}
		if (!IsT(value))
		{
			throw new ArgumentException("Value is of incorrect type.", "value");
		}
		AddToBack((T)value);
		return Count - 1;
	}

	bool IList.Contains(object value)
	{
		if (IsT(value))
		{
			return ((ICollection<T>)this).Contains((T)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (!IsT(value))
		{
			return -1;
		}
		return IndexOf((T)value);
	}

	void IList.Insert(int index, object value)
	{
		if (value == null && default(T) != null)
		{
			throw new ArgumentNullException("value", "Value cannot be null.");
		}
		if (!IsT(value))
		{
			throw new ArgumentException("Value is of incorrect type.", "value");
		}
		Insert(index, (T)value);
	}

	void IList.Remove(object value)
	{
		if (IsT(value))
		{
			Remove((T)value);
		}
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array", "Destination array cannot be null.");
		}
		CheckRangeArguments(array.Length, index, Count);
		try
		{
			CopyToArray(array, index);
		}
		catch (ArrayTypeMismatchException innerException)
		{
			throw new ArgumentException("Destination array is of incorrect type.", "array", innerException);
		}
		catch (RankException innerException2)
		{
			throw new ArgumentException("Destination array must be single dimensional.", "array", innerException2);
		}
	}

	private static void CheckNewIndexArgument(int sourceLength, int index)
	{
		if (index < 0 || index > sourceLength)
		{
			throw new ArgumentOutOfRangeException("index", "Invalid new index " + index + " for source length " + sourceLength);
		}
	}

	private static void CheckExistingIndexArgument(int sourceLength, int index)
	{
		if (index < 0 || index >= sourceLength)
		{
			throw new ArgumentOutOfRangeException("index", "Invalid existing index " + index + " for source length " + sourceLength);
		}
	}

	private static void CheckRangeArguments(int sourceLength, int offset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", "Invalid offset " + offset);
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Invalid count " + count);
		}
		if (sourceLength - offset < count)
		{
			throw new ArgumentException("Invalid offset (" + offset + ") or count + (" + count + ") for source length " + sourceLength);
		}
	}
}
