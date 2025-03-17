namespace ClassicUO.Utility;

public struct UnmanagedMemoryPool
{
	public unsafe byte* Alloc;

	public unsafe void* Free;

	public int BlockSize;

	public int NumBlocks;
}
