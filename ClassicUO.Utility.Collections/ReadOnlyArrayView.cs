using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassicUO.Utility.Collections;

public readonly struct ReadOnlyArrayView<T> : IEnumerable<T>, IEnumerable
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly ReadOnlyArrayView<T> _view;

		private int _currentIndex;

		public T Current => _view._items[_currentIndex];

		object IEnumerator.Current => Current;

		public Enumerator(ReadOnlyArrayView<T> view)
		{
			_view = view;
			_currentIndex = (int)view._start;
		}

		public bool MoveNext()
		{
			if (_currentIndex != _view._start + _view.Count - 1)
			{
				_currentIndex++;
				return true;
			}
			return false;
		}

		public void Reset()
		{
			_currentIndex = (int)_view._start;
		}

		public void Dispose()
		{
		}
	}

	private readonly T[] _items;

	private readonly uint _start;

	public readonly uint Count;

	public T this[uint index] => _items[index + _start];

	public ReadOnlyArrayView(T[] items, uint start, uint count)
	{
		_items = items;
		_start = start;
		Count = count;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
