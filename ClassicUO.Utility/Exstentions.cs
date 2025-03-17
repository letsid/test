using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Utility;

internal static class Exstentions
{
	public static void Raise(this EventHandler handler, object sender = null)
	{
		handler?.Invoke(sender, EventArgs.Empty);
	}

	public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
	{
		handler?.Invoke(sender, e);
	}

	public static void RaiseAsync(this EventHandler handler, object sender = null)
	{
		if (handler != null)
		{
			Task.Run(delegate
			{
				handler(sender, EventArgs.Empty);
			}).Catch();
		}
	}

	public static void RaiseAsync<T>(this EventHandler<T> handler, T e, object sender = null)
	{
		if (handler != null)
		{
			Task.Run(delegate
			{
				handler(sender, e);
			}).Catch();
		}
	}

	public static Task Catch(this Task task)
	{
		return task.ContinueWith(delegate(Task t)
		{
			t.Exception?.Handle(delegate(Exception e)
			{
				Log.Panic(e.ToString());
				return true;
			});
		}, TaskContinuationOptions.OnlyOnFaulted);
	}

	public static void Resize<T>(this List<T> list, int size, T element = default(T))
	{
		int count = list.Count;
		if (size < count)
		{
			list.RemoveRange(size, count - size);
		}
		else if (size > count)
		{
			if (size > list.Capacity)
			{
				list.Capacity = size;
			}
			list.AddRange(Enumerable.Repeat(element, size - count));
		}
	}

	public static void ForEach<T>(this T[] array, Action<T> func)
	{
		foreach (T obj in array)
		{
			func(obj);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool InRect(ref Rectangle rect, ref Rectangle r)
	{
		bool flag = false;
		if (rect.X < r.X)
		{
			if (r.X < rect.Right)
			{
				flag = true;
			}
		}
		else if (rect.X < r.Right)
		{
			flag = true;
		}
		if (flag)
		{
			flag = ((rect.Y >= r.Y) ? (rect.Y < r.Bottom) : (r.Y < rect.Bottom));
		}
		return flag;
	}

	public static T[] Rent<T>(this ArrayPool<T> pool, int length, bool zero)
	{
		T[] array = pool.Rent(length);
		if (zero)
		{
			Array.Clear(array, 0, array.Length);
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToHex(this uint serial)
	{
		return $"0x{serial:X8}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToHex(this ushort s)
	{
		return $"0x{s:X4}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ToHex(this byte b)
	{
		return $"0x{b:X2}";
	}
}
