using System;
using System.Runtime.InteropServices;

namespace ClassicUO.Utility;

public static class UnsafeMemoryManager
{
	public unsafe static readonly int SizeOfPointer = sizeof(void*);

	public static readonly int MinimumPoolBlockSize = SizeOfPointer;

	public unsafe static void Memset(void* ptr, byte value, int count)
	{
		long* ptr2 = (long*)ptr;
		count /= 8;
		for (int i = 0; i < count; i++)
		{
			*(ptr2++) = value;
		}
	}

	public static IntPtr Alloc(int size)
	{
		size = (size + 7) & -8;
		return Marshal.AllocHGlobal(size);
	}

	public unsafe static IntPtr Calloc(int size)
	{
		IntPtr intPtr = Alloc(size);
		Memset((void*)intPtr, 0, size);
		return intPtr;
	}

	public unsafe static void* Alloc(ref UnmanagedMemoryPool pool)
	{
		void* free = pool.Free;
		pool.Free = *(void**)pool.Free;
		return free;
	}

	public unsafe static void* Calloc(ref UnmanagedMemoryPool pool)
	{
		void* intPtr = Alloc(ref pool);
		Memset(intPtr, 0, pool.BlockSize);
		return intPtr;
	}

	public unsafe static UnmanagedMemoryPool AllocPool(int blockSize, int numBlocks)
	{
		blockSize = (blockSize + 7) & -8;
		UnmanagedMemoryPool result = default(UnmanagedMemoryPool);
		result.Free = null;
		result.NumBlocks = numBlocks;
		result.BlockSize = blockSize;
		result.Alloc = (byte*)(void*)Alloc(blockSize * numBlocks);
		FreeAll(&result);
		return result;
	}

	public static void Free(IntPtr ptr)
	{
		if (ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(ptr);
		}
	}

	public unsafe static void Free(UnmanagedMemoryPool* pool, void* ptr)
	{
		if (ptr != null)
		{
			*(void**)ptr = pool->Free;
			pool->Free = ptr;
		}
	}

	public unsafe static void Free(ref UnmanagedMemoryPool pool, void* ptr)
	{
		if (ptr != null)
		{
			*(void**)ptr = pool.Free;
			pool.Free = ptr;
		}
	}

	public unsafe static void FreeAll(UnmanagedMemoryPool* pool)
	{
		void** ptr = (void**)pool->Alloc;
		byte* ptr2 = pool->Alloc + pool->BlockSize;
		int i = 0;
		for (int num = pool->NumBlocks - 1; i < num; i++)
		{
			*ptr = ptr2;
			ptr = (void**)ptr2;
			ptr2 += pool->BlockSize;
		}
		*ptr = default(void*);
		pool->Free = pool->Alloc;
	}

	public unsafe static void FreePool(UnmanagedMemoryPool* pool)
	{
		Free((IntPtr)pool->Alloc);
		pool->Alloc = null;
		pool->Free = null;
	}
}
