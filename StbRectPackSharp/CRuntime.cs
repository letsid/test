using System;
using System.Runtime.InteropServices;

namespace StbRectPackSharp;

internal static class CRuntime
{
	public unsafe delegate int QSortComparer(void* a, void* b);

	public unsafe static void* malloc(ulong size)
	{
		return malloc((long)size);
	}

	public unsafe static void* malloc(long size)
	{
		return Marshal.AllocHGlobal((int)size).ToPointer();
	}

	public unsafe static void free(void* a)
	{
		Marshal.FreeHGlobal(new IntPtr(a));
	}

	private unsafe static void qsortSwap(byte* data, long size, long pos1, long pos2)
	{
		byte* ptr = data + size * pos1;
		byte* ptr2 = data + size * pos2;
		for (long num = 0L; num < size; num++)
		{
			byte b = *ptr;
			*ptr = *ptr2;
			*ptr2 = b;
			ptr++;
			ptr2++;
		}
	}

	private unsafe static long qsortPartition(byte* data, long size, QSortComparer comparer, long left, long right)
	{
		void* b = data + size * left;
		long num = left - 1;
		long num2 = right + 1;
		while (true)
		{
			num++;
			if (comparer(data + size * num, b) >= 0)
			{
				do
				{
					num2--;
				}
				while (comparer(data + size * num2, b) > 0);
				if (num >= num2)
				{
					break;
				}
				qsortSwap(data, size, num, num2);
			}
		}
		return num2;
	}

	private unsafe static void qsortInternal(byte* data, long size, QSortComparer comparer, long left, long right)
	{
		if (left < right)
		{
			long num = qsortPartition(data, size, comparer, left, right);
			qsortInternal(data, size, comparer, left, num);
			qsortInternal(data, size, comparer, num + 1, right);
		}
	}

	public unsafe static void qsort(void* data, ulong count, ulong size, QSortComparer comparer)
	{
		qsortInternal((byte*)data, (long)size, comparer, 0L, (long)(count - 1));
	}
}
