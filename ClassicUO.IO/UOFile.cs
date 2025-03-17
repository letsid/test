using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO;

internal class UOFile : DataReader
{
	private protected MemoryMappedViewAccessor _accessor;

	private protected MemoryMappedFile _file;

	public string FilePath { get; }

	public UOFile(string filepath, bool loadFile = false)
	{
		FilePath = filepath;
		if (loadFile)
		{
			Load();
		}
	}

	protected unsafe virtual void Load()
	{
		Log.Trace("Loading file:\t\t" + FilePath);
		FileInfo fileInfo = new FileInfo(FilePath);
		if (!fileInfo.Exists)
		{
			Log.Error(FilePath + "  not exists.");
			return;
		}
		long length = fileInfo.Length;
		if (length > 0)
		{
			_file = MemoryMappedFile.CreateFromFile(File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), null, 0L, MemoryMappedFileAccess.Read, HandleInheritability.None, leaveOpen: false);
			_accessor = _file.CreateViewAccessor(0L, length, MemoryMappedFileAccess.Read);
			byte* pointer = null;
			try
			{
				_accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
				SetData(pointer, (long)_accessor.SafeMemoryMappedViewHandle.ByteLength);
				return;
			}
			catch
			{
				_accessor.SafeMemoryMappedViewHandle.ReleasePointer();
				throw new Exception("Something goes wrong...");
			}
		}
		Log.Error(FilePath + "  size must be > 0");
	}

	public virtual void FillEntries(ref UOFileIndex[] entries)
	{
	}

	public virtual void Dispose()
	{
		_accessor.SafeMemoryMappedViewHandle.ReleasePointer();
		_accessor.Dispose();
		_file.Dispose();
		Log.Trace("Unloaded:\t\t" + FilePath);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe void Fill(ref byte[] buffer, int count)
	{
		byte* ptr = (byte*)(void*)base.PositionAddress;
		for (int i = 0; i < count; i++)
		{
			buffer[i] = ptr[i];
		}
		base.Position += count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal T[] ReadArray<T>(int count) where T : struct
	{
		T[] result = ReadArray<T>(base.Position, count);
		base.Position += Unsafe.SizeOf<T>() * count;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private T[] ReadArray<T>(long position, int count) where T : struct
	{
		T[] array = new T[count];
		_accessor.ReadArray(position, array, 0, count);
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal T ReadStruct<T>(long position) where T : struct
	{
		_accessor.Read<T>(position, out var structure);
		return structure;
	}
}
