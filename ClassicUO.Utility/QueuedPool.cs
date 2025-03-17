using System;
using System.Collections.Generic;

namespace ClassicUO.Utility;

internal class QueuedPool<T> where T : class, new()
{
	private readonly Action<T> _on_pickup;

	private readonly Stack<T> _pool;

	public int MaxSize { get; }

	public int Remains => MaxSize - _pool.Count;

	public QueuedPool(int size, Action<T> onpickup = null)
	{
		MaxSize = size;
		_pool = new Stack<T>(size);
		_on_pickup = onpickup;
		for (int i = 0; i < size; i++)
		{
			_pool.Push(new T());
		}
	}

	public T GetOne()
	{
		T val = ((_pool.Count == 0) ? new T() : _pool.Pop());
		_on_pickup?.Invoke(val);
		return val;
	}

	public void ReturnOne(T obj)
	{
		if (obj != null)
		{
			_pool.Push(obj);
		}
	}

	public void Clear()
	{
		_pool.Clear();
	}
}
