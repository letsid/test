using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ClassicUO.Utility.Collections;

public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IOrderedDictionary, ICollection, IDictionary
{
	private KeyedCollection2<TKey, KeyValuePair<TKey, TValue>> _keyedCollection;

	public TValue this[TKey key]
	{
		get
		{
			return GetValue(key);
		}
		set
		{
			SetValue(key, value);
		}
	}

	public TValue this[int index]
	{
		get
		{
			return GetItem(index).Value;
		}
		set
		{
			SetItem(index, value);
		}
	}

	public int Count => _keyedCollection.Count;

	public ICollection<TKey> Keys => _keyedCollection.Select((KeyValuePair<TKey, TValue> x) => x.Key).ToList();

	public ICollection<TValue> Values => _keyedCollection.Select((KeyValuePair<TKey, TValue> x) => x.Value).ToList();

	public IEqualityComparer<TKey> Comparer { get; private set; }

	ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

	ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

	TValue IDictionary<TKey, TValue>.this[TKey key]
	{
		get
		{
			return this[key];
		}
		set
		{
			this[key] = value;
		}
	}

	int ICollection<KeyValuePair<TKey, TValue>>.Count => _keyedCollection.Count;

	bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

	object IOrderedDictionary.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			this[index] = (TValue)value;
		}
	}

	bool IDictionary.IsFixedSize => false;

	bool IDictionary.IsReadOnly => false;

	ICollection IDictionary.Keys => (ICollection)Keys;

	ICollection IDictionary.Values => (ICollection)Values;

	object IDictionary.this[object key]
	{
		get
		{
			return this[(TKey)key];
		}
		set
		{
			this[(TKey)key] = (TValue)value;
		}
	}

	int ICollection.Count => ((ICollection)_keyedCollection).Count;

	bool ICollection.IsSynchronized => ((ICollection)_keyedCollection).IsSynchronized;

	object ICollection.SyncRoot => ((ICollection)_keyedCollection).SyncRoot;

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public OrderedDictionary()
	{
		Initialize();
	}

	public OrderedDictionary(IEqualityComparer<TKey> comparer)
	{
		Initialize(comparer);
	}

	public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary)
	{
		Initialize();
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			_keyedCollection.Add(item);
		}
	}

	public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		Initialize(comparer);
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			_keyedCollection.Add(item);
		}
	}

	private void Initialize(IEqualityComparer<TKey> comparer = null)
	{
		Comparer = comparer;
		if (comparer != null)
		{
			_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>((KeyValuePair<TKey, TValue> x) => x.Key, comparer);
		}
		else
		{
			_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>((KeyValuePair<TKey, TValue> x) => x.Key);
		}
	}

	public void Add(TKey key, TValue value)
	{
		_keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
	}

	public void Clear()
	{
		_keyedCollection.Clear();
	}

	public void Insert(int index, TKey key, TValue value)
	{
		_keyedCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
	}

	public int IndexOf(TKey key)
	{
		if (_keyedCollection.Contains(key))
		{
			return _keyedCollection.IndexOf(_keyedCollection[key]);
		}
		return -1;
	}

	public bool ContainsValue(TValue value)
	{
		return Values.Contains(value);
	}

	public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer)
	{
		return Values.Contains(value, comparer);
	}

	public bool ContainsKey(TKey key)
	{
		return _keyedCollection.Contains(key);
	}

	public KeyValuePair<TKey, TValue> GetItem(int index)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
		}
		return _keyedCollection[index];
	}

	public void SetItem(int index, TValue value)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index is outside the bounds of the dictionary: {index}");
		}
		KeyValuePair<TKey, TValue> value2 = new KeyValuePair<TKey, TValue>(_keyedCollection[index].Key, value);
		_keyedCollection[index] = value2;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _keyedCollection.GetEnumerator();
	}

	public bool Remove(TKey key)
	{
		return _keyedCollection.Remove(key);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= _keyedCollection.Count)
		{
			throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
		}
		_keyedCollection.RemoveAt(index);
	}

	public TValue GetValue(TKey key)
	{
		if (!_keyedCollection.Contains(key))
		{
			throw new ArgumentException($"The given key is not present in the dictionary: {key}");
		}
		return _keyedCollection[key].Value;
	}

	public void SetValue(TKey key, TValue value)
	{
		KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
		int num = IndexOf(key);
		if (num > -1)
		{
			_keyedCollection[num] = keyValuePair;
		}
		else
		{
			_keyedCollection.Add(keyValuePair);
		}
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_keyedCollection.Contains(key))
		{
			value = _keyedCollection[key].Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	public void SortKeys()
	{
		_keyedCollection.SortByKeys();
	}

	public void SortKeys(IComparer<TKey> comparer)
	{
		_keyedCollection.SortByKeys(comparer);
	}

	public void SortKeys(Comparison<TKey> comparison)
	{
		_keyedCollection.SortByKeys(comparison);
	}

	public void SortValues()
	{
		Comparer<TValue> @default = Comparer<TValue>.Default;
		SortValues(@default);
	}

	public void SortValues(IComparer<TValue> comparer)
	{
		_keyedCollection.Sort((KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => comparer.Compare(x.Value, y.Value));
	}

	public void SortValues(Comparison<TValue> comparison)
	{
		_keyedCollection.Sort((KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => comparison(x.Value, y.Value));
	}

	void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		Add(key, value);
	}

	bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
	{
		return ContainsKey(key);
	}

	bool IDictionary<TKey, TValue>.Remove(TKey key)
	{
		return Remove(key);
	}

	bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
	{
		return TryGetValue(key, out value);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		_keyedCollection.Add(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.Clear()
	{
		_keyedCollection.Clear();
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		return _keyedCollection.Contains(item);
	}

	void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		_keyedCollection.CopyTo(array, arrayIndex);
	}

	bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		return _keyedCollection.Remove(item);
	}

	IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator<TKey, TValue>(this);
	}

	void IOrderedDictionary.Insert(int index, object key, object value)
	{
		Insert(index, (TKey)key, (TValue)value);
	}

	void IOrderedDictionary.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	void IDictionary.Add(object key, object value)
	{
		Add((TKey)key, (TValue)value);
	}

	void IDictionary.Clear()
	{
		Clear();
	}

	bool IDictionary.Contains(object key)
	{
		return _keyedCollection.Contains((TKey)key);
	}

	IDictionaryEnumerator IDictionary.GetEnumerator()
	{
		return new DictionaryEnumerator<TKey, TValue>(this);
	}

	void IDictionary.Remove(object key)
	{
		Remove((TKey)key);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_keyedCollection).CopyTo(array, index);
	}
}
