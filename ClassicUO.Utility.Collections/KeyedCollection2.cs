using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClassicUO.Utility.Collections;

public class KeyedCollection2<TKey, TItem> : KeyedCollection<TKey, TItem>
{
	private const string DelegateNullExceptionMessage = "Delegate passed cannot be null";

	private readonly Func<TItem, TKey> _getKeyForItemDelegate;

	public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate)
	{
		_getKeyForItemDelegate = getKeyForItemDelegate ?? throw new ArgumentNullException("Delegate passed cannot be null");
	}

	public KeyedCollection2(Func<TItem, TKey> getKeyForItemDelegate, IEqualityComparer<TKey> comparer)
		: base(comparer)
	{
		_getKeyForItemDelegate = getKeyForItemDelegate ?? throw new ArgumentNullException("Delegate passed cannot be null");
	}

	protected override TKey GetKeyForItem(TItem item)
	{
		return _getKeyForItemDelegate(item);
	}

	public void SortByKeys()
	{
		Comparer<TKey> @default = Comparer<TKey>.Default;
		SortByKeys(@default);
	}

	public void SortByKeys(IComparer<TKey> keyComparer)
	{
		Comparer2<TItem> comparer = new Comparer2<TItem>((TItem x, TItem y) => keyComparer.Compare(GetKeyForItem(x), GetKeyForItem(y)));
		Sort(comparer);
	}

	public void SortByKeys(Comparison<TKey> keyComparison)
	{
		Comparer2<TItem> comparer = new Comparer2<TItem>((TItem x, TItem y) => keyComparison(GetKeyForItem(x), GetKeyForItem(y)));
		Sort(comparer);
	}

	public void Sort()
	{
		Comparer<TItem> @default = Comparer<TItem>.Default;
		Sort(@default);
	}

	public void Sort(Comparison<TItem> comparison)
	{
		Comparer2<TItem> comparer = new Comparer2<TItem>((TItem x, TItem y) => comparison(x, y));
		Sort(comparer);
	}

	public void Sort(IComparer<TItem> comparer)
	{
		if (base.Items is List<TItem> list)
		{
			list.Sort(comparer);
		}
	}
}
