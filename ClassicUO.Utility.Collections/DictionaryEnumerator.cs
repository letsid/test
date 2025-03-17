using System;
using System.Collections;
using System.Collections.Generic;

namespace ClassicUO.Utility.Collections;

public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IEnumerator, IDisposable
{
	private readonly IEnumerator<KeyValuePair<TKey, TValue>> impl;

	public DictionaryEntry Entry
	{
		get
		{
			KeyValuePair<TKey, TValue> current = impl.Current;
			return new DictionaryEntry(current.Key, current.Value);
		}
	}

	public object Key => impl.Current.Key;

	public object Value => impl.Current.Value;

	public object Current => Entry;

	public DictionaryEnumerator(IDictionary<TKey, TValue> value)
	{
		impl = value.GetEnumerator();
	}

	public void Reset()
	{
		impl.Reset();
	}

	public bool MoveNext()
	{
		return impl.MoveNext();
	}

	public void Dispose()
	{
		impl.Dispose();
	}
}
