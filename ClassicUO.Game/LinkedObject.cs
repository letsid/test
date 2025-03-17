using System;

namespace ClassicUO.Game;

internal abstract class LinkedObject
{
	public LinkedObject Previous;

	public LinkedObject Next;

	public LinkedObject Items;

	public bool IsEmpty => Items == null;

	public void PushToBack(LinkedObject item)
	{
		if (item != null)
		{
			Remove(item);
			if (Items == null)
			{
				Items = item;
				return;
			}
			LinkedObject last = GetLast();
			last.Next = item;
			item.Next = null;
			item.Previous = last;
		}
	}

	public void Remove(LinkedObject item)
	{
		if (item != null)
		{
			Unlink(item);
			item.Next = null;
			item.Previous = null;
		}
	}

	public void Unlink(LinkedObject item)
	{
		if (item == null)
		{
			return;
		}
		if (item == Items)
		{
			Items = Items.Next;
			if (Items != null)
			{
				Items.Previous = null;
			}
			return;
		}
		if (item.Previous != null)
		{
			item.Previous.Next = item.Next;
		}
		if (item.Next != null)
		{
			item.Next.Previous = item.Previous;
		}
	}

	public void Insert(LinkedObject first, LinkedObject item)
	{
		if (first == null)
		{
			item.Next = Items;
			item.Previous = null;
			if (Items != null)
			{
				Items.Previous = item;
			}
			Items = item;
		}
		else
		{
			LinkedObject linkedObject = (item.Next = first.Next);
			item.Previous = first;
			first.Next = item;
			if (linkedObject != null)
			{
				linkedObject.Previous = item;
			}
		}
	}

	public void MoveToFront(LinkedObject item)
	{
		if (item != null && item != Items)
		{
			Unlink(item);
			if (Items != null)
			{
				Items.Previous = item;
			}
			item.Next = Items;
			item.Previous = null;
			Items = item;
		}
	}

	public void MoveToBack(LinkedObject item)
	{
		if (item != null)
		{
			Unlink(item);
			LinkedObject last = GetLast();
			if (last == null)
			{
				Items = item;
			}
			else
			{
				last.Next = item;
			}
			item.Previous = last;
			item.Next = null;
		}
	}

	public LinkedObject GetLast()
	{
		LinkedObject linkedObject = Items;
		while (linkedObject != null && linkedObject.Next != null)
		{
			linkedObject = linkedObject.Next;
		}
		return linkedObject;
	}

	public void Clear()
	{
		if (Items != null)
		{
			LinkedObject linkedObject = Items;
			Items = null;
			while (linkedObject != null)
			{
				LinkedObject next = linkedObject.Next;
				linkedObject.Next = null;
				linkedObject = next;
			}
		}
	}

	public LinkedObject SortContents<T>(Comparison<T> comparison) where T : LinkedObject
	{
		if (Items == null)
		{
			return null;
		}
		int num = 1;
		T val = null;
		T val2 = null;
		T val3 = null;
		T val4 = (T)Items;
		T val5 = null;
		while (true)
		{
			val = val4;
			int num2 = 0;
			val4 = null;
			val5 = null;
			while (val != null)
			{
				num2++;
				val2 = val;
				int num3 = 0;
				for (int i = 0; i < num; i++)
				{
					num3++;
					val2 = (T)val2.Next;
					if (val2 == null)
					{
						break;
					}
				}
				int num4 = num;
				while (num3 > 0 || (num4 > 0 && val2 != null))
				{
					if (num3 == 0)
					{
						val3 = val2;
						val2 = (T)val2.Next;
						num4--;
					}
					else if (num4 == 0 || val2 == null)
					{
						val3 = val;
						val = (T)val.Next;
						num3--;
					}
					else if (comparison(val, val2) <= 0)
					{
						val3 = val;
						val = (T)val.Next;
						num3--;
					}
					else
					{
						val3 = val2;
						val2 = (T)val2.Next;
						num4--;
					}
					if (val5 != null)
					{
						val5.Next = val3;
					}
					else
					{
						val4 = val3;
					}
					val3.Previous = val5;
					val5 = val3;
				}
				val = val2;
			}
			val5.Next = null;
			if (num2 <= 1)
			{
				break;
			}
			num *= 2;
		}
		Items = val4;
		return val4;
	}
}
