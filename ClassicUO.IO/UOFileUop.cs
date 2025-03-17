using System;
using System.Collections.Generic;

namespace ClassicUO.IO;

internal class UOFileUop : UOFile
{
	private const uint UOP_MAGIC_NUMBER = 5265741u;

	private readonly bool _hasExtra;

	private readonly Dictionary<ulong, UOFileIndex> _hashes = new Dictionary<ulong, UOFileIndex>();

	private readonly string _pattern;

	public int TotalEntriesCount { get; private set; }

	public UOFileUop(string path, string pattern, bool hasextra = false)
		: base(path)
	{
		_pattern = pattern;
		_hasExtra = hasextra;
		Load();
	}

	public bool TryGetUOPData(ulong hash, out UOFileIndex data)
	{
		return _hashes.TryGetValue(hash, out data);
	}

	protected override void Load()
	{
		base.Load();
		Seek(0);
		if (ReadUInt() != 5265741)
		{
			throw new ArgumentException("Bad uop file");
		}
		ReadUInt();
		ReadUInt();
		long idx = ReadLong();
		ReadUInt();
		ReadInt();
		Seek(idx);
		int num = 0;
		int num2 = 0;
		do
		{
			int num3 = ReadInt();
			idx = ReadLong();
			num += num3;
			for (int i = 0; i < num3; i++)
			{
				long num4 = ReadLong();
				int num5 = ReadInt();
				int num6 = ReadInt();
				int decompressed = ReadInt();
				ulong key = ReadULong();
				ReadUInt();
				ReadShort();
				_ = 1;
				if (num4 != 0L)
				{
					num2++;
					num4 += num5;
					if (_hasExtra)
					{
						long position = base.Position;
						Seek(num4);
						short width = (short)ReadInt();
						short height = (short)ReadInt();
						_hashes.Add(key, new UOFileIndex(base.StartAddress, (uint)base.Length, num4 + 8, num6 - 8, decompressed, width, height, 0));
						Seek(position);
					}
					else
					{
						_hashes.Add(key, new UOFileIndex(base.StartAddress, (uint)base.Length, num4, num6, decompressed, 0, 0, 0));
					}
				}
			}
			Seek(idx);
		}
		while (idx != 0L);
		TotalEntriesCount = num2;
	}

	public void ClearHashes()
	{
		_hashes.Clear();
	}

	public override void Dispose()
	{
		ClearHashes();
		base.Dispose();
	}

	public override void FillEntries(ref UOFileIndex[] entries)
	{
		for (int i = 0; i < entries.Length; i++)
		{
			ulong key = CreateHash(string.Format(_pattern, i));
			if (_hashes.TryGetValue(key, out var value))
			{
				entries[i] = value;
			}
		}
	}

	public void FillEntries(ref UOFileIndex[] entries, bool clearHashes)
	{
		FillEntries(ref entries);
		if (clearHashes)
		{
			ClearHashes();
		}
	}

	internal static ulong CreateHash(string s)
	{
		uint num3;
		uint num5;
		uint num2;
		uint num4;
		uint num;
		uint num6 = (num5 = (num4 = (num3 = (num2 = (num = 0u)))));
		num3 = (num = (num2 = (uint)(s.Length + -559038737)));
		int num7 = 0;
		for (num7 = 0; num7 + 12 < s.Length; num7 += 12)
		{
			num = (((uint)s[num7 + 7] << 24) | ((uint)s[num7 + 6] << 16) | ((uint)s[num7 + 5] << 8) | s[num7 + 4]) + num;
			num2 = (((uint)s[num7 + 11] << 24) | ((uint)s[num7 + 10] << 16) | ((uint)s[num7 + 9] << 8) | s[num7 + 8]) + num2;
			num4 = (((uint)s[num7 + 3] << 24) | ((uint)s[num7 + 2] << 16) | ((uint)s[num7 + 1] << 8) | s[num7]) - num2;
			num4 = (num4 + num3) ^ (num2 >> 28) ^ (num2 << 4);
			num2 += num;
			num = (num - num4) ^ (num4 >> 26) ^ (num4 << 6);
			num4 += num2;
			num2 = (num2 - num) ^ (num >> 24) ^ (num << 8);
			num += num4;
			num3 = (num4 - num2) ^ (num2 >> 16) ^ (num2 << 16);
			num2 += num;
			num = (num - num3) ^ (num3 >> 13) ^ (num3 << 19);
			num3 += num2;
			num2 = (num2 - num) ^ (num >> 28) ^ (num << 4);
			num += num3;
		}
		if (s.Length - num7 > 0)
		{
			switch (s.Length - num7)
			{
			case 12:
				num2 += (uint)s[num7 + 11] << 24;
				goto case 11;
			case 11:
				num2 += (uint)s[num7 + 10] << 16;
				goto case 10;
			case 10:
				num2 += (uint)s[num7 + 9] << 8;
				goto case 9;
			case 9:
				num2 += s[num7 + 8];
				goto case 8;
			case 8:
				num += (uint)s[num7 + 7] << 24;
				goto case 7;
			case 7:
				num += (uint)s[num7 + 6] << 16;
				goto case 6;
			case 6:
				num += (uint)s[num7 + 5] << 8;
				goto case 5;
			case 5:
				num += s[num7 + 4];
				goto case 4;
			case 4:
				num3 += (uint)s[num7 + 3] << 24;
				goto case 3;
			case 3:
				num3 += (uint)s[num7 + 2] << 16;
				goto case 2;
			case 2:
				num3 += (uint)s[num7 + 1] << 8;
				goto case 1;
			case 1:
				num3 += s[num7];
				break;
			}
			num2 = (num2 ^ num) - ((num >> 18) ^ (num << 14));
			num5 = (num2 ^ num3) - ((num2 >> 21) ^ (num2 << 11));
			num = (num ^ num5) - ((num5 >> 7) ^ (num5 << 25));
			num2 = (num2 ^ num) - ((num >> 16) ^ (num << 16));
			num4 = (num2 ^ num5) - ((num2 >> 28) ^ (num2 << 4));
			num = (num ^ num4) - ((num4 >> 18) ^ (num4 << 14));
			num6 = (num2 ^ num) - ((num >> 8) ^ (num << 24));
			return ((ulong)num << 32) | num6;
		}
		return ((ulong)num2 << 32) | num6;
	}
}
