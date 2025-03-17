using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO.Resources;

internal class MapLoader : UOFileLoader
{
	private static MapLoader _instance;

	private UOFileMul[] _mapDif;

	private UOFileMul[] _mapDifl;

	private UOFileMul[] _staDif;

	private UOFileMul[] _staDifi;

	private UOFileMul[] _staDifl;

	public new UOFileIndex[][] Entries;

	private protected UOFileMul[] _filesIdxStatics;

	private protected UOFile[] _filesMap;

	private protected UOFileMul[] _filesStatics;

	public static MapLoader Instance
	{
		get
		{
			return _instance ?? (_instance = new MapLoader());
		}
		set
		{
			_instance?.Dispose();
			_instance = value;
		}
	}

	public IndexMap[][] BlockData { get; private set; }

	public int[,] MapBlocksSize { get; private set; }

	public int[,] MapsDefaultSize { get; private protected set; } = new int[6, 2]
	{
		{ 7168, 4096 },
		{ 7168, 4096 },
		{ 2304, 1600 },
		{ 2560, 2048 },
		{ 1448, 1448 },
		{ 1280, 4096 }
	};

	public int PatchesCount { get; private set; }

	public int[] MapPatchCount { get; private set; }

	public int[] StaticPatchCount { get; private set; }

	private protected MapLoader()
	{
	}

	protected static UOFile GetMapFile(int map)
	{
		if (map >= Instance._filesMap.Length)
		{
			return null;
		}
		return Instance._filesMap[map];
	}

	protected void Initialize()
	{
		_filesMap = new UOFile[Constants.MAPS_COUNT];
		_filesStatics = new UOFileMul[Constants.MAPS_COUNT];
		_filesIdxStatics = new UOFileMul[Constants.MAPS_COUNT];
		Entries = new UOFileIndex[Constants.MAPS_COUNT][];
		MapPatchCount = new int[Constants.MAPS_COUNT];
		StaticPatchCount = new int[Constants.MAPS_COUNT];
		MapBlocksSize = new int[Constants.MAPS_COUNT, 2];
		BlockData = new IndexMap[Constants.MAPS_COUNT][];
		_mapDif = new UOFileMul[Constants.MAPS_COUNT];
		_mapDifl = new UOFileMul[Constants.MAPS_COUNT];
		_staDif = new UOFileMul[Constants.MAPS_COUNT];
		_staDifi = new UOFileMul[Constants.MAPS_COUNT];
		_staDifl = new UOFileMul[Constants.MAPS_COUNT];
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(Settings.GlobalSettings.MapsLayouts))
			{
				string[] array = Settings.GlobalSettings.MapsLayouts.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
				Constants.MAPS_COUNT = array.Length;
				MapsDefaultSize = new int[array.Length, 2];
				Log.Trace($"default maps size overraided. [count: {Constants.MAPS_COUNT}]");
				int num = 0;
				char[] separator = new char[1] { ',' };
				string[] array2 = array;
				foreach (string text in array2)
				{
					string[] array3 = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
					if (array3.Length >= 2 && int.TryParse(array3[0], out var result) && int.TryParse(array3[1], out var result2))
					{
						MapsDefaultSize[num, 0] = result;
						MapsDefaultSize[num, 1] = result2;
						Log.Trace($"overraided map size: {result},{result2}  [index: {num}]");
					}
					else
					{
						Log.Error("Error parsing 'width,height' values: '" + text + "'");
					}
					num++;
				}
			}
			Initialize();
			for (int k = 0; k < Constants.MAPS_COUNT; k++)
			{
				string uOFilePath = UOFileManager.GetUOFilePath($"map{k}LegacyMUL.uop");
				if (Client.IsUOPInstallation && File.Exists(uOFilePath))
				{
					_filesMap[k] = new UOFileUop(uOFilePath, $"build/map{k}legacymul/{{0:D8}}.dat");
					Entries[k] = new UOFileIndex[((UOFileUop)_filesMap[k]).TotalEntriesCount];
					((UOFileUop)_filesMap[k]).FillEntries(ref Entries[k], clearHashes: false);
					flag = true;
				}
				else
				{
					uOFilePath = UOFileManager.GetUOFilePath($"map{k}.mul");
					if (File.Exists(uOFilePath))
					{
						_filesMap[k] = new UOFileMul(uOFilePath);
						flag = true;
					}
					uOFilePath = UOFileManager.GetUOFilePath($"mapdifl{k}.mul");
					if (File.Exists(uOFilePath))
					{
						_mapDifl[k] = new UOFileMul(uOFilePath);
						_mapDif[k] = new UOFileMul(UOFileManager.GetUOFilePath($"mapdif{k}.mul"));
						_staDifl[k] = new UOFileMul(UOFileManager.GetUOFilePath($"stadifl{k}.mul"));
						_staDifi[k] = new UOFileMul(UOFileManager.GetUOFilePath($"stadifi{k}.mul"));
						_staDif[k] = new UOFileMul(UOFileManager.GetUOFilePath($"stadif{k}.mul"));
					}
				}
				uOFilePath = UOFileManager.GetUOFilePath($"statics{k}.mul");
				if (File.Exists(uOFilePath))
				{
					_filesStatics[k] = new UOFileMul(uOFilePath);
				}
				uOFilePath = UOFileManager.GetUOFilePath($"staidx{k}.mul");
				if (File.Exists(uOFilePath))
				{
					_filesIdxStatics[k] = new UOFileMul(uOFilePath);
				}
			}
			if (!flag)
			{
				throw new FileNotFoundException("No maps found.");
			}
			int num2 = sizeof(MapBlock);
			if (_filesMap[0].Length / num2 == 393216 || Client.Version < ClientVersion.CV_4011D)
			{
				MapsDefaultSize[0, 0] = (MapsDefaultSize[1, 0] = 6144);
			}
			if (_filesMap[1] == null || _filesMap[1].StartAddress == IntPtr.Zero)
			{
				_filesMap[1] = _filesMap[0];
				_filesStatics[1] = _filesStatics[0];
				_filesIdxStatics[1] = _filesIdxStatics[0];
			}
			Parallel.For(0, Constants.MAPS_COUNT, delegate(int i)
			{
				MapBlocksSize[i, 0] = MapsDefaultSize[i, 0] >> 3;
				MapBlocksSize[i, 1] = MapsDefaultSize[i, 1] >> 3;
				LoadMap(i);
			});
			Entries = null;
		});
	}

	internal unsafe void LoadMap(int i)
	{
		if (i < 0 || i + 1 > Constants.MAPS_COUNT || _filesMap[i] == null)
		{
			i = 0;
		}
		if (BlockData[i] != null || _filesMap[i] == null)
		{
			return;
		}
		int num = sizeof(MapBlock);
		int num2 = sizeof(StaidxBlock);
		int num3 = sizeof(StaticsBlock);
		int num4 = MapBlocksSize[i, 0];
		int num5 = MapBlocksSize[i, 1];
		int num6 = num4 * num5;
		BlockData[i] = new IndexMap[num6];
		UOFile uOFile = _filesMap[i];
		UOFile uOFile2 = _filesIdxStatics[i];
		UOFile uOFile3 = _filesStatics[i];
		if (uOFile2 == null && i == 1)
		{
			uOFile2 = _filesIdxStatics[0];
		}
		if (uOFile3 == null && i == 1)
		{
			uOFile3 = _filesStatics[0];
		}
		ulong num7 = (ulong)(long)uOFile2.StartAddress;
		ulong num8 = num7 + (ulong)uOFile2.Length;
		ulong num9 = (ulong)(long)uOFile3.StartAddress;
		ulong num10 = num9 + (ulong)uOFile3.Length;
		ulong num11 = (ulong)(long)uOFile.StartAddress;
		ulong num12 = num11 + (ulong)uOFile.Length;
		ulong num13 = 0uL;
		int num14 = -1;
		bool flag = uOFile is UOFileUop;
		for (int j = 0; j < num6; j++)
		{
			ulong num15 = 0uL;
			ulong num16 = 0uL;
			uint num17 = 0u;
			int num18 = j;
			if (flag)
			{
				num18 &= 0xFFF;
				int num19 = j >> 12;
				if (num14 != num19)
				{
					num14 = num19;
					if (num19 < Entries[i].Length)
					{
						num13 = (ulong)Entries[i][num19].Offset;
					}
				}
			}
			ulong num20 = num11 + num13 + (ulong)(num18 * num);
			if (num20 < num12)
			{
				num15 = num20;
			}
			long num21 = (long)num7 + (long)(j * num2);
			StaidxBlock* ptr = (StaidxBlock*)num21;
			if ((ulong)num21 < num8 && ptr->Size != 0 && ptr->Position != uint.MaxValue)
			{
				ulong num22 = num9 + ptr->Position;
				if (num22 < num10)
				{
					num16 = num22;
					num17 = (uint)(ptr->Size / num3);
					if (num17 > 1024)
					{
						num17 = 1024u;
					}
				}
			}
			ref IndexMap reference = ref BlockData[i][j];
			reference.MapAddress = num15;
			reference.StaticAddress = num16;
			reference.StaticCount = num17;
			reference.OriginalMapAddress = num15;
			reference.OriginalStaticAddress = num16;
			reference.OriginalStaticCount = num17;
		}
	}

	public void PatchMapBlock(ulong block, ulong address)
	{
		int num = MapBlocksSize[0, 0];
		int num2 = MapBlocksSize[0, 1];
		if (num * num2 >= 1)
		{
			BlockData[0][block].OriginalMapAddress = address;
			BlockData[0][block].MapAddress = address;
		}
	}

	public unsafe void PatchStaticBlock(ulong block, ulong address, uint count)
	{
		int num = MapBlocksSize[0, 0];
		int num2 = MapBlocksSize[0, 1];
		if (num * num2 >= 1)
		{
			BlockData[0][block].StaticAddress = (BlockData[0][block].OriginalStaticAddress = address);
			count = (uint)(count / sizeof(StaidxBlockVerdata));
			if (count > 1024)
			{
				count = 1024u;
			}
			BlockData[0][block].StaticCount = (BlockData[0][block].OriginalStaticCount = count);
		}
	}

	public unsafe bool ApplyPatches(ref StackDataReader reader)
	{
		ResetPatchesInBlockTable();
		PatchesCount = (int)reader.ReadUInt32BE();
		if (PatchesCount < 0)
		{
			PatchesCount = 0;
		}
		if (PatchesCount > Constants.MAPS_COUNT)
		{
			PatchesCount = Constants.MAPS_COUNT;
		}
		Array.Clear(MapPatchCount, 0, MapPatchCount.Length);
		Array.Clear(StaticPatchCount, 0, StaticPatchCount.Length);
		bool result = false;
		for (int i = 0; i < PatchesCount; i++)
		{
			int num = i;
			if (_filesMap[num] == null || _filesMap[num].StartAddress == IntPtr.Zero)
			{
				reader.Skip(8);
				continue;
			}
			int num2 = (int)reader.ReadUInt32BE();
			MapPatchCount[i] = num2;
			int num3 = (int)reader.ReadUInt32BE();
			StaticPatchCount[i] = num3;
			int num4 = MapBlocksSize[i, 0];
			int num5 = MapBlocksSize[i, 1];
			int num6 = num4 * num5;
			if (num2 != 0)
			{
				UOFileMul uOFileMul = _mapDifl[i];
				UOFileMul uOFileMul2 = _mapDif[i];
				if (uOFileMul == null || uOFileMul2 == null || uOFileMul.Length == 0L || uOFileMul2.Length == 0L)
				{
					continue;
				}
				num2 = Math.Min(num2, (int)uOFileMul.Length >> 2);
				uOFileMul.Seek(0);
				uOFileMul2.Seek(0);
				for (int j = 0; j < num2; j++)
				{
					uint num7 = uOFileMul.ReadUInt();
					if (num7 < num6)
					{
						BlockData[num][num7].MapAddress = (ulong)(long)uOFileMul2.PositionAddress;
						result = true;
					}
					uOFileMul2.Skip(sizeof(MapBlock));
				}
			}
			if (num3 == 0)
			{
				continue;
			}
			UOFileMul uOFileMul3 = _staDifl[i];
			UOFileMul uOFileMul4 = _staDifi[i];
			if (uOFileMul3 == null || uOFileMul4 == null || _staDif[i] == null || uOFileMul3.Length == 0L || uOFileMul4.Length == 0L || _staDif[i].Length == 0L)
			{
				continue;
			}
			ulong num8 = (ulong)(long)_staDif[i].StartAddress;
			num3 = Math.Min(num3, (int)uOFileMul3.Length >> 2);
			uOFileMul3.Seek(0);
			uOFileMul4.Seek(0);
			int num9 = sizeof(StaticsBlock);
			int count = sizeof(StaidxBlock);
			for (int k = 0; k < num3; k++)
			{
				if (uOFileMul3.IsEOF)
				{
					break;
				}
				if (uOFileMul4.IsEOF)
				{
					break;
				}
				uint num10 = uOFileMul3.ReadUInt();
				StaidxBlock* ptr = (StaidxBlock*)(void*)uOFileMul4.PositionAddress;
				uOFileMul4.Skip(count);
				if (num10 >= num6)
				{
					continue;
				}
				ulong staticAddress = 0uL;
				int num11 = 0;
				if (ptr->Size != 0 && ptr->Position != uint.MaxValue)
				{
					staticAddress = num8 + ptr->Position;
					num11 = (int)(ptr->Size / num9);
					if (num11 > 0 && num11 > 1024)
					{
						num11 = 1024;
					}
				}
				BlockData[num][num10].StaticAddress = staticAddress;
				BlockData[num][num10].StaticCount = (uint)num11;
				result = true;
			}
		}
		return result;
	}

	private void ResetPatchesInBlockTable()
	{
		for (int i = 0; i < Constants.MAPS_COUNT; i++)
		{
			IndexMap[] array = BlockData[i];
			if (array == null)
			{
				continue;
			}
			int num = MapBlocksSize[i, 0];
			int num2 = MapBlocksSize[i, 1];
			int num3 = num * num2;
			if (num3 < 1)
			{
				break;
			}
			if (!(_filesMap[i] is UOFileMul uOFileMul) || !(uOFileMul.StartAddress != IntPtr.Zero))
			{
				continue;
			}
			UOFileMul uOFileMul2 = _filesIdxStatics[i];
			if (uOFileMul2 == null || !(uOFileMul2.StartAddress != IntPtr.Zero))
			{
				continue;
			}
			UOFileMul uOFileMul3 = _filesStatics[i];
			if (uOFileMul3 != null && uOFileMul3.StartAddress != IntPtr.Zero)
			{
				for (int j = 0; j < num3; j++)
				{
					ref IndexMap reference = ref array[j];
					reference.MapAddress = reference.OriginalMapAddress;
					reference.StaticAddress = reference.OriginalStaticAddress;
					reference.StaticCount = reference.OriginalStaticCount;
				}
			}
		}
	}

	public void SanitizeMapIndex(ref int map)
	{
		if (map == 1 && (_filesMap[1] == null || _filesMap[1].StartAddress == IntPtr.Zero || _filesStatics[1] == null || _filesStatics[1].StartAddress == IntPtr.Zero || _filesIdxStatics[1] == null || _filesIdxStatics[1].StartAddress == IntPtr.Zero))
		{
			map = 0;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref IndexMap GetIndex(int map, int x, int y)
	{
		int num = x * MapBlocksSize[map, 1] + y;
		return ref BlockData[map][num];
	}
}
