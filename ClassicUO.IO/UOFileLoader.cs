using System;
using System.Threading.Tasks;

namespace ClassicUO.IO;

internal abstract class UOFileLoader : IDisposable
{
	public UOFileIndex[] Entries;

	public bool IsDisposed { get; private set; }

	public virtual void Dispose()
	{
		if (!IsDisposed)
		{
			IsDisposed = true;
			ClearResources();
		}
	}

	public abstract Task Load();

	public virtual void ClearResources()
	{
	}

	public ref UOFileIndex GetValidRefEntry(int index)
	{
		if (index < 0 || Entries == null || index >= Entries.Length)
		{
			return ref UOFileIndex.Invalid;
		}
		ref UOFileIndex reference = ref Entries[index];
		if (reference.Offset < 0 || reference.Length <= 0 || reference.Offset == uint.MaxValue)
		{
			return ref UOFileIndex.Invalid;
		}
		return ref reference;
	}
}
