namespace ClassicUO.IO;

internal class UOFileMul : UOFile
{
	private class UOFileIdxMul : UOFile
	{
		public UOFileIdxMul(string idxpath)
			: base(idxpath)
		{
			Load();
		}

		public override void FillEntries(ref UOFileIndex[] entries)
		{
		}
	}

	private readonly int _count;

	private readonly int _patch;

	private readonly UOFileIdxMul _idxFile;

	public UOFile IdxFile => _idxFile;

	public UOFileMul(string file, string idxfile, int count, int patch = -1)
		: this(file)
	{
		_idxFile = new UOFileIdxMul(idxfile);
		_count = count;
		_patch = patch;
	}

	public UOFileMul(string file)
		: base(file)
	{
		Load();
	}

	public override void FillEntries(ref UOFileIndex[] entries)
	{
		UOFile uOFile = (UOFile)(((object)_idxFile) ?? ((object)this));
		int num = (int)uOFile.Length / 12;
		entries = new UOFileIndex[num];
		for (int i = 0; i < num; i++)
		{
			ref UOFileIndex reference = ref entries[i];
			reference.Address = base.StartAddress;
			reference.FileSize = (uint)base.Length;
			reference.Offset = uOFile.ReadUInt();
			reference.Length = uOFile.ReadInt();
			reference.DecompressedLength = 0;
			int num2 = uOFile.ReadInt();
			if (num2 > 0)
			{
				reference.Width = (short)(num2 >> 16);
				reference.Height = (short)(num2 & 0xFFFF);
			}
		}
	}

	public override void Dispose()
	{
		_idxFile?.Dispose();
		base.Dispose();
	}
}
