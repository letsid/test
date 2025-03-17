using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Threading.Tasks;
using ClassicUO.Game;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Map;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO;

internal class UltimaLive
{
	private class ULFileMul : UOFileMul
	{
		public ULFileMul(string file, bool isStaticMul)
			: base(file)
		{
			LoadFile(isStaticMul);
		}

		protected override void Load()
		{
		}

		private unsafe void LoadFile(bool isStaticMul)
		{
			FileInfo fileInfo = new FileInfo(base.FilePath);
			if (!fileInfo.Exists)
			{
				throw new FileNotFoundException(fileInfo.FullName);
			}
			uint num = (uint)fileInfo.Length;
			Log.Trace("UltimaLive -> ReLoading file:\t" + base.FilePath);
			if (num != 0 || isStaticMul)
			{
				if (isStaticMul)
				{
					MemoryMappedFile memoryMappedFile;
					try
					{
						memoryMappedFile = MemoryMappedFile.OpenExisting(_UL.RealShardName + fileInfo.Name);
					}
					catch
					{
						memoryMappedFile = MemoryMappedFile.CreateNew(_UL.RealShardName + fileInfo.Name, 200000000L, MemoryMappedFileAccess.ReadWrite);
						using FileStream fileStream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						using Stream destination = memoryMappedFile.CreateViewStream(0L, fileStream.Length, MemoryMappedFileAccess.Write);
						fileStream.CopyTo(destination);
					}
					_file = memoryMappedFile;
				}
				else
				{
					MemoryMappedFile memoryMappedFile;
					try
					{
						memoryMappedFile = MemoryMappedFile.OpenExisting(_UL.RealShardName + fileInfo.Name);
					}
					catch
					{
						memoryMappedFile = MemoryMappedFile.CreateFromFile(File.Open(base.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite), _UL.RealShardName + fileInfo.Name, num, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: false);
					}
					_file = memoryMappedFile;
				}
				_accessor = _file.CreateViewAccessor(0L, isStaticMul ? 200000000 : num, MemoryMappedFileAccess.ReadWrite);
				byte* pointer = null;
				try
				{
					_accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
					SetData(pointer, (long)_accessor.SafeMemoryMappedViewHandle.ByteLength);
					return;
				}
				catch
				{
					_file.Dispose();
					_accessor.SafeMemoryMappedViewHandle.ReleasePointer();
					_accessor.Dispose();
					throw new Exception("Something goes wrong...");
				}
			}
			throw new Exception(base.FilePath + " size must be > 0");
		}

		public override void Dispose()
		{
			MapLoader.Instance.Dispose();
		}

		internal void WriteArray(long position, ArraySegment<byte> seg)
		{
			if (_accessor.CanWrite && seg.Array != null)
			{
				_accessor.WriteArray(position, seg.Array, seg.Offset, seg.Count);
				_accessor.Flush();
			}
		}

		internal void WriteArray(long position, byte[] array)
		{
			if (_accessor.CanWrite)
			{
				_accessor.WriteArray(position, array, 0, array.Length);
				_accessor.Flush();
			}
		}
	}

	internal class ULMapLoader : MapLoader
	{
		internal class AsyncWriterTasked
		{
			private readonly ULMapLoader _Map;

			private readonly CancellationTokenSource _token;

			private readonly AutoResetEvent m_Signal = new AutoResetEvent(initialState: false);

			internal readonly ConcurrentQueue<(int, long, byte[])> _toWrite = new ConcurrentQueue<(int, long, byte[])>();

			public AsyncWriterTasked(ULMapLoader map, CancellationTokenSource token)
			{
				_Map = map;
				_token = token;
			}

			public void Loop()
			{
				while (_UL != null && !_Map.IsDisposed && !_token.IsCancellationRequested)
				{
					(int, long, byte[]) result;
					while (_toWrite.TryDequeue(out result))
					{
						WriteArray(result.Item1, result.Item2, result.Item3);
					}
					m_Signal.WaitOne(10, exitContext: false);
				}
			}

			internal void WriteArray(int map, long position, byte[] array)
			{
				_Map._filesStaticsStream[map].Seek(position, SeekOrigin.Begin);
				_Map._filesStaticsStream[map].Write(array, 0, array.Length);
				_Map._filesStaticsStream[map].Flush();
			}
		}

		private readonly CancellationTokenSource _feedCancel;

		private FileStream[] _filesStaticsStream;

		private readonly Task _writerTask;

		private new UOFileIndex[][] Entries;

		internal readonly AsyncWriterTasked _writer;

		internal (UOFile[], UOFileMul[], UOFileMul[]) GetFilesReference => (_filesMap, _filesIdxStatics, _filesStatics);

		private uint NumMaps { get; }

		public ULMapLoader(uint maps)
		{
			Initialize();
			_feedCancel = new CancellationTokenSource();
			NumMaps = maps;
			int[,] mapsDefaultSize = base.MapsDefaultSize;
			base.MapsDefaultSize = new int[NumMaps, 2];
			for (int i = 0; i < NumMaps; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					base.MapsDefaultSize[i, j] = ((i < mapsDefaultSize.GetLength(0)) ? mapsDefaultSize[i, j] : mapsDefaultSize[0, j]);
				}
			}
			_writer = new AsyncWriterTasked(this, _feedCancel);
			_writerTask = Task.Run((Action)_writer.Loop);
		}

		public override void ClearResources()
		{
			try
			{
				_feedCancel?.Cancel();
				_writerTask?.Wait();
				_feedCancel?.Dispose();
				_writerTask?.Dispose();
			}
			catch
			{
			}
			if (_filesStaticsStream != null)
			{
				for (int num = _filesStaticsStream.Length - 1; num >= 0; num--)
				{
					_filesStaticsStream[num]?.Dispose();
				}
				_filesStaticsStream = null;
			}
		}

		public override Task Load()
		{
			return Task.Run(delegate
			{
				if (!(MapLoader.Instance is ULMapLoader))
				{
					UOFileManager.MapLoaderReLoad(this);
					_UL._EOF = new uint[NumMaps];
					_filesStaticsStream = new FileStream[NumMaps];
					bool flag = false;
					for (int i = 0; i < _UL._ValidMaps.Count; i++)
					{
						int num = _UL._ValidMaps[i];
						string text = Path.Combine(_UL.ShardName, $"map{num}.mul");
						if (File.Exists(text))
						{
							_filesMap[num] = new ULFileMul(text, isStaticMul: false);
							flag = true;
						}
						text = Path.Combine(_UL.ShardName, $"statics{num}.mul");
						if (!File.Exists(text))
						{
							flag = false;
							break;
						}
						_filesStatics[num] = new ULFileMul(text, isStaticMul: true);
						_filesStaticsStream[num] = File.Open(text, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
						_UL._EOF[num] = (uint)new FileInfo(text).Length;
						text = Path.Combine(_UL.ShardName, $"staidx{num}.mul");
						if (!File.Exists(text))
						{
							flag = false;
							break;
						}
						_filesIdxStatics[num] = new ULFileMul(text, isStaticMul: false);
					}
					if (!flag)
					{
						throw new FileNotFoundException("No maps, staidx or statics found on " + _UL.ShardName + ".");
					}
					for (int j = 0; j < _UL._ValidMaps.Count; j++)
					{
						int num2 = _UL._ValidMaps[j];
						base.MapBlocksSize[num2, 0] = base.MapsDefaultSize[num2, 0] >> 3;
						base.MapBlocksSize[num2, 1] = base.MapsDefaultSize[num2, 1] >> 3;
						LoadMap(num2);
					}
				}
			});
		}

		internal void CheckForShardMapFile(int mapId)
		{
			if (Entries == null)
			{
				Entries = new UOFileIndex[Constants.MAPS_COUNT][];
			}
			string uOFilePath = UOFileManager.GetUOFilePath($"map{mapId}.mul");
			string uOFilePath2 = UOFileManager.GetUOFilePath($"staidx{mapId}.mul");
			string uOFilePath3 = UOFileManager.GetUOFilePath($"statics{mapId}.mul");
			string text = Path.Combine(_UL.ShardName, $"map{mapId}.mul");
			string text2 = Path.Combine(_UL.ShardName, $"staidx{mapId}.mul");
			string text3 = Path.Combine(_UL.ShardName, $"statics{mapId}.mul");
			if (!File.Exists(text))
			{
				UOFile mapFile = MapLoader.GetMapFile(mapId);
				if (mapFile == null)
				{
					CreateNewPersistentMap(mapId, text, text2, text3);
				}
				else if (mapFile is UOFileUop uOFileUop)
				{
					Entries[mapId] = new UOFileIndex[uOFileUop.TotalEntriesCount];
					uOFileUop.FillEntries(ref Entries[mapId]);
					Log.Trace("UltimaLive -> converting file:\t" + text + " from " + uOFileUop.FilePath);
					using FileStream fileStream = File.Create(text);
					for (int i = 0; i < Entries[mapId].Length; i++)
					{
						uOFileUop.Seek(Entries[mapId][i].Offset);
						fileStream.Write(uOFileUop.ReadArray(Entries[mapId][i].Length), 0, Entries[mapId][i].Length);
					}
					fileStream.Flush();
				}
				else
				{
					CopyFile(uOFilePath, text);
				}
			}
			if (!File.Exists(text3))
			{
				CopyFile(uOFilePath3, text3);
			}
			if (!File.Exists(text2))
			{
				CopyFile(uOFilePath2, text2);
			}
		}

		private static void CreateNewPersistentMap(int mapId, string mapPath, string staIdxPath, string staticsPath)
		{
			int num = MapLoader.Instance.MapBlocksSize[(MapLoader.Instance.MapBlocksSize.GetLength(0) > mapId) ? mapId : 0, 0];
			int num2 = MapLoader.Instance.MapBlocksSize[(MapLoader.Instance.MapBlocksSize.GetLength(0) > mapId) ? mapId : 0, 1];
			int num3 = 196 * num2;
			byte[] array = new byte[num3];
			byte[] sourceArray = new byte[196]
			{
				0, 0, 0, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0, 68, 2, 0, 68,
				2, 0, 68, 2, 0, 68, 2, 0, 68, 2,
				0, 68, 2, 0, 68, 2, 0, 68, 2, 0,
				68, 2, 0, 68, 2, 0
			};
			for (int i = 0; i < num2; i++)
			{
				Array.Copy(sourceArray, 0, array, 196 * i, 196);
			}
			using (FileStream fileStream = File.Create(mapPath))
			{
				Log.Trace("UltimaLive -> creating new blank map:\t" + mapPath);
				Log.Trace($"Writing {num} blocks by {num2} blocks");
				for (int j = 0; j < num; j++)
				{
					fileStream.Write(array, 0, num3);
				}
				fileStream.Flush();
			}
			num3 = 12 * num2;
			array = new byte[num3];
			sourceArray = new byte[12]
			{
				255, 255, 255, 255, 0, 0, 0, 0, 0, 0,
				0, 0
			};
			for (int k = 0; k < num2; k++)
			{
				Array.Copy(sourceArray, 0, array, 12 * k, 12);
			}
			using (FileStream fileStream2 = File.Create(staIdxPath))
			{
				Log.Trace("UltimaLive -> creating new index file");
				for (int l = 0; l < num; l++)
				{
					fileStream2.Write(array, 0, num3);
				}
				fileStream2.Flush();
			}
			using (File.Create(staticsPath))
			{
				Log.Trace("UltimaLive -> creating empty static file");
			}
		}

		private static void CopyFile(string fromFilePath, string toFilePath)
		{
			if (!File.Exists(toFilePath) || new FileInfo(toFilePath).Length == 0L)
			{
				Log.Trace("UltimaLive -> copying file:\t" + toFilePath + " from " + fromFilePath);
				File.Copy(fromFilePath, toFilePath, overwrite: true);
			}
		}

		internal unsafe void ReloadBlock(int map, int blockNumber)
		{
			int num = sizeof(MapBlock);
			int num2 = sizeof(StaidxBlock);
			int num3 = sizeof(StaticsBlock);
			UOFile uOFile = _filesMap[map];
			UOFile uOFile2 = _filesIdxStatics[map];
			UOFile uOFile3 = _filesStatics[map];
			long num4 = (long)uOFile2.StartAddress;
			ulong num5 = (ulong)(num4 + uOFile2.Length);
			ulong num6 = (ulong)(long)uOFile3.StartAddress;
			ulong num7 = num6 + (ulong)uOFile3.Length;
			long num8 = (long)uOFile.StartAddress;
			ulong num9 = (ulong)(num8 + uOFile.Length);
			ulong num10 = 0uL;
			int num11 = -1;
			bool num12 = uOFile is UOFileUop;
			ulong num13 = 0uL;
			ulong num14 = 0uL;
			uint num15 = 0u;
			int num16 = blockNumber;
			if (num12)
			{
				blockNumber &= 0xFFF;
				int num17 = num16 >> 12;
				if (num11 != num17)
				{
					num11 = num17;
					if (num17 < Entries.Length)
					{
						num10 = (ulong)Entries[map][num17].Offset;
					}
				}
			}
			ulong num18 = (ulong)(num8 + (long)num10 + blockNumber * num);
			if (num18 < num9)
			{
				num13 = num18;
			}
			long num19 = num4 + num16 * num2;
			StaidxBlock* ptr = (StaidxBlock*)num19;
			if ((ulong)num19 < num5 && ptr->Size != 0 && ptr->Position != uint.MaxValue)
			{
				ulong num20 = num6 + ptr->Position;
				if (num20 < num7)
				{
					num14 = num20;
					num15 = (uint)(ptr->Size / num3);
					if (num15 > 1024)
					{
						num15 = 1024u;
					}
				}
			}
			ref IndexMap reference = ref base.BlockData[map][num16];
			reference.MapAddress = num13;
			reference.StaticAddress = num14;
			reference.StaticCount = num15;
			reference.OriginalMapAddress = num13;
			reference.OriginalStaticAddress = num14;
			reference.OriginalStaticCount = num15;
		}
	}

	private const int STATICS_MEMORY_SIZE = 200000000;

	private const int CRC_LENGTH = 25;

	private const int LAND_BLOCK_LENGTH = 192;

	private static UltimaLive _UL;

	private static readonly char[] _pathSeparatorChars = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private uint[] _EOF;

	private ULFileMul[] _filesIdxStatics;

	private ULFileMul[] _filesMap;

	private ULFileMul[] _filesStatics;

	private uint _SentWarning;

	private ULMapLoader _ULMap;

	private List<int> _ValidMaps = new List<int>();

	private ConcurrentQueue<(int, long, byte[])> _writequeue;

	private ushort[][] MapCRCs;

	private ushort[,] MapSizeWrapSize;

	protected string RealShardName;

	protected string ShardName;

	internal static bool UltimaLiveActive
	{
		get
		{
			if (_UL != null)
			{
				return !string.IsNullOrEmpty(_UL.ShardName);
			}
			return false;
		}
	}

	internal static void Enable()
	{
		Log.Trace("Setup packet for UltimaLive");
		PacketHandlers.Handlers.Add(63, OnUltimaLivePacket);
		PacketHandlers.Handlers.Add(64, OnUpdateTerrainPacket);
	}

	private static void OnUltimaLivePacket(ref StackDataReader p)
	{
		p.Seek(13L);
		switch (p.ReadUInt8())
		{
		case byte.MaxValue:
		{
			if (_UL == null || p.Length < 15)
			{
				break;
			}
			p.Seek(3L);
			int num = (int)p.ReadUInt32BE();
			p.Seek(14L);
			int num2 = p.ReadUInt8();
			if (num2 >= _UL._filesMap.Length)
			{
				if (Time.Ticks >= _UL._SentWarning)
				{
					Log.Trace($"The server is requesting access to MAP: {num2} but we only have {_UL._filesMap.Length} maps!");
					_UL._SentWarning = Time.Ticks + 100000;
				}
			}
			else
			{
				if (World.Map == null || num2 != World.Map.Index)
				{
					break;
				}
				int num3 = MapLoader.Instance.MapBlocksSize[num2, 0];
				int num4 = MapLoader.Instance.MapBlocksSize[num2, 1];
				int num5 = num3 * num4;
				if (num < 0 || num >= num5)
				{
					break;
				}
				if (_UL.MapCRCs[num2] == null)
				{
					_UL.MapCRCs[num2] = new ushort[num5];
					for (int j = 0; j < num5; j++)
					{
						_UL.MapCRCs[num2][j] = ushort.MaxValue;
					}
				}
				int num6 = num / num4;
				int num7 = num % num4;
				num3 = ((num6 < _UL.MapSizeWrapSize[num2, 2] >> 3) ? (_UL.MapSizeWrapSize[num2, 2] >> 3) : num3);
				num4 = ((num7 < _UL.MapSizeWrapSize[num2, 3] >> 3) ? (_UL.MapSizeWrapSize[num2, 3] >> 3) : num4);
				ushort[] array2 = new ushort[25];
				for (int k = -2; k <= 2; k++)
				{
					int num8 = (num6 + k) % num3;
					if (num8 < 0 && num8 > -3)
					{
						num8 += num3;
					}
					for (int l = -2; l <= 2; l++)
					{
						int num9 = (num7 + l) % num4;
						if (num9 < 0)
						{
							num9 += num4;
						}
						uint num10 = (uint)(num8 * num4 + num9);
						if (num10 < num5)
						{
							ushort num11 = _UL.MapCRCs[num2][num10];
							if (num11 == ushort.MaxValue)
							{
								num11 = (ushort)((num8 < num3 && num9 < num4) ? GetBlockCrc(num10) : 0);
								_UL.MapCRCs[num2][num10] = num11;
							}
							array2[(k + 2) * 5 + l + 2] = num11;
						}
						else
						{
							array2[(k + 2) * 5 + l + 2] = 0;
						}
					}
				}
				NetClient.Socket.Send_UOLive_HashResponse((uint)num, (byte)num2, array2.AsSpan(0, 25));
			}
			break;
		}
		case 0:
		{
			if (_UL == null || p.Length < 15)
			{
				break;
			}
			p.Seek(3L);
			int num12 = (int)p.ReadUInt32BE();
			int num13 = (int)(p.ReadUInt32BE() * 7);
			if (p.Length < num13 + 15)
			{
				break;
			}
			p.Seek(14L);
			int num14 = p.ReadUInt8();
			if (num14 >= _UL._filesMap.Length)
			{
				if (Time.Ticks >= _UL._SentWarning)
				{
					Log.Trace($"The server is requesting access to MAP: {num14} but we only have {_UL._filesMap.Length} maps!");
					_UL._SentWarning = Time.Ticks + 100000;
				}
			}
			else
			{
				if (World.Map == null || num14 != World.Map.Index)
				{
					break;
				}
				byte[] array3 = new byte[num13];
				p.Read(array3, 0, num13);
				if (num12 < 0 || num12 >= MapLoader.Instance.MapBlocksSize[num14, 0] * MapLoader.Instance.MapBlocksSize[num14, 1])
				{
					break;
				}
				int num15 = num12 * 12;
				if (num13 <= 0)
				{
					_UL._filesIdxStatics[num14].WriteArray(num15, new byte[8] { 255, 255, 255, 255, 0, 0, 0, 0 });
					Log.Trace($"writing zero length statics to index at 0x{num15:X8}");
				}
				else
				{
					_UL._filesIdxStatics[num14].Seek(num15);
					uint num16 = _UL._filesIdxStatics[num14].ReadUInt();
					if (_UL._filesIdxStatics[num14].ReadUInt() >= num13 && num16 != uint.MaxValue)
					{
						Log.Trace($"writing statics to existing file location at 0x{num16:X8}, length:{num13}");
					}
					else
					{
						num16 = _UL._EOF[num14];
						_UL._EOF[num14] += (uint)num13;
						Log.Trace($"writing statics to end of file at 0x{num16:X8}, length:{num13}");
					}
					_UL._filesStatics[num14].WriteArray(num16, array3);
					_UL._writequeue.Enqueue((num14, num16, array3));
					byte[] array4 = new byte[8]
					{
						(byte)num16,
						(byte)(num16 >> 8),
						(byte)(num16 >> 16),
						(byte)(num16 >> 24),
						(byte)num13,
						(byte)(num13 >> 8),
						(byte)(num13 >> 16),
						(byte)(num13 >> 24)
					};
					_UL._filesIdxStatics[num14].WriteArray(num12 * 12, array4);
					Chunk chunk = World.Map.GetChunk(num12);
					if (chunk == null)
					{
						break;
					}
					_ = chunk.Node?.List;
					List<GameObject> list = new List<GameObject>();
					for (int m = 0; m < 8; m++)
					{
						for (int n = 0; n < 8; n++)
						{
							GameObject gameObject = chunk.GetHeadObject(m, n);
							while (gameObject != null)
							{
								GameObject gameObject2 = gameObject;
								gameObject = gameObject.TNext;
								if (!(gameObject2 is Land) && !(gameObject2 is Static))
								{
									list.Add(gameObject2);
									gameObject2.RemoveFromTile();
								}
							}
						}
					}
					chunk.Clear();
					_UL._ULMap.ReloadBlock(num14, num12);
					chunk.Load(num14);
					foreach (GameObject item in list)
					{
						chunk.AddGameObject(item, item.X % 8, item.Y % 8);
					}
				}
				UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
				_UL.MapCRCs[num14][num12] = ushort.MaxValue;
			}
			break;
		}
		case 1:
		{
			if (_UL == null || string.IsNullOrEmpty(_UL.ShardName) || p.Length < 15)
			{
				break;
			}
			if (!Directory.Exists(_UL.ShardName))
			{
				Directory.CreateDirectory(_UL.ShardName);
				if (!Directory.Exists(_UL.ShardName))
				{
					_UL = null;
					break;
				}
			}
			p.Seek(7L);
			uint num17 = p.ReadUInt32BE() * 7 / 9;
			if (p.Length < num17 * 9 + 15)
			{
				break;
			}
			_UL.MapCRCs = new ushort[127][];
			_UL.MapSizeWrapSize = new ushort[127, 4];
			p.Seek(15L);
			List<int> list2 = new List<int>();
			for (int num18 = 0; num18 < num17; num18++)
			{
				int num19 = p.ReadUInt8();
				list2.Add(num19);
				_UL.MapSizeWrapSize[num19, 0] = Math.Min((ushort)MapLoader.Instance.MapsDefaultSize[0, 0], p.ReadUInt16BE());
				_UL.MapSizeWrapSize[num19, 1] = Math.Min((ushort)MapLoader.Instance.MapsDefaultSize[0, 1], p.ReadUInt16BE());
				_UL.MapSizeWrapSize[num19, 2] = Math.Min(p.ReadUInt16BE(), _UL.MapSizeWrapSize[num19, 0]);
				_UL.MapSizeWrapSize[num19, 3] = Math.Min(p.ReadUInt16BE(), _UL.MapSizeWrapSize[num19, 1]);
			}
			if (_UL._ValidMaps.Count == 0 || list2.Count > _UL._ValidMaps.Count || !list2.TrueForAll((int i) => _UL._ValidMaps.Contains(i)))
			{
				_UL._ValidMaps = list2;
				Constants.MAPS_COUNT = 127;
				ULMapLoader uLMapLoader = new ULMapLoader((uint)Constants.MAPS_COUNT);
				for (int num20 = 0; num20 < list2.Count; num20++)
				{
					uLMapLoader.CheckForShardMapFile(list2[num20]);
				}
				uLMapLoader.Load().Wait();
				_UL._ULMap = uLMapLoader;
				_UL._filesMap = new ULFileMul[Constants.MAPS_COUNT];
				_UL._filesIdxStatics = new ULFileMul[Constants.MAPS_COUNT];
				_UL._filesStatics = new ULFileMul[Constants.MAPS_COUNT];
				(UOFile[], UOFileMul[], UOFileMul[]) getFilesReference = uLMapLoader.GetFilesReference;
				for (int num21 = 0; num21 < list2.Count; num21++)
				{
					_UL._filesMap[list2[num21]] = getFilesReference.Item1[list2[num21]] as ULFileMul;
					_UL._filesIdxStatics[list2[num21]] = getFilesReference.Item2[list2[num21]] as ULFileMul;
					_UL._filesStatics[list2[num21]] = getFilesReference.Item3[list2[num21]] as ULFileMul;
				}
				_UL._writequeue = uLMapLoader._writer._toWrite;
			}
			break;
		}
		case 2:
			if (p.Length >= 43)
			{
				p.Seek(15L);
				string text = ValidatePath(p.ReadASCII());
				if (string.IsNullOrWhiteSpace(text))
				{
					_UL = null;
				}
				else if (_UL == null || !(_UL.ShardName == text))
				{
					string[] array = text.Split(_pathSeparatorChars, StringSplitOptions.RemoveEmptyEntries);
					_UL = new UltimaLive
					{
						ShardName = text,
						RealShardName = array[^1]
					};
				}
			}
			break;
		}
	}

	private static void OnUpdateTerrainPacket(ref StackDataReader p)
	{
		int num = (int)p.ReadUInt32BE();
		byte[] array = new byte[192];
		for (int i = 0; i < 192; i++)
		{
			array[i] = p.ReadUInt8();
		}
		p.Seek(200L);
		byte b = p.ReadUInt8();
		if (World.Map == null || b != World.Map.Index)
		{
			return;
		}
		ushort num2 = (ushort)MapLoader.Instance.MapBlocksSize[b, 0];
		ushort num3 = (ushort)MapLoader.Instance.MapBlocksSize[b, 1];
		if (num < 0 || num >= num2 * num3)
		{
			return;
		}
		_UL._filesMap[b].WriteArray(num * 196 + 4, array);
		_UL.MapCRCs[b][num] = ushort.MaxValue;
		int num4 = num / num3;
		int num5 = num % num3;
		int num6 = Math.Max(0, num4 - 1);
		int num7 = Math.Max(0, num5 - 1);
		num4 = Math.Min(num2, num4 + 1);
		num5 = Math.Min(num3, num5 + 1);
		while (num4 >= num6)
		{
			for (int num8 = num5; num8 >= num7; num8--)
			{
				Chunk chunk = World.Map.GetChunk(num4 * num3 + num8);
				if (chunk != null)
				{
					_ = chunk.Node?.List;
					List<GameObject> list = new List<GameObject>();
					for (int j = 0; j < 8; j++)
					{
						for (int k = 0; k < 8; k++)
						{
							GameObject gameObject = chunk.GetHeadObject(j, k);
							while (gameObject != null)
							{
								GameObject gameObject2 = gameObject;
								gameObject = gameObject.TNext;
								if (!(gameObject2 is Land) && !(gameObject2 is Static))
								{
									list.Add(gameObject2);
									gameObject2.RemoveFromTile();
								}
							}
						}
					}
					chunk.Clear();
					chunk.Load(b);
					foreach (GameObject item in list)
					{
						chunk.AddGameObject(item, item.X % 8, item.Y % 8);
					}
				}
			}
			num4--;
		}
		UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
	}

	private static ushort GetBlockCrc(uint block)
	{
		int index = World.Map.Index;
		_UL._filesIdxStatics[index].Seek(block * 12);
		uint num = _UL._filesIdxStatics[index].ReadUInt();
		int num2 = Math.Max(0, _UL._filesIdxStatics[index].ReadInt());
		byte[] array = new byte[192 + num2];
		_UL._filesMap[index].Seek(block * 196 + 4);
		for (int i = 0; i < 192; i++)
		{
			if (_UL._filesMap[index].Position + 1 >= _UL._filesMap[index].Length)
			{
				break;
			}
			array[i] = _UL._filesMap[index].ReadByte();
		}
		if (num != uint.MaxValue && num2 > 0 && num < _UL._filesStatics[index].Length)
		{
			_UL._filesStatics[index].Seek(num);
			for (int j = 192; j < array.Length; j++)
			{
				if (_UL._filesStatics[index].Position + 1 >= _UL._filesStatics[index].Length)
				{
					break;
				}
				array[j] = _UL._filesStatics[index].ReadByte();
			}
		}
		return Fletcher16(array);
	}

	private static ushort Fletcher16(byte[] data)
	{
		ushort num = 0;
		ushort num2 = 0;
		for (int i = 0; i < data.Length; i++)
		{
			num = (ushort)((num + data[i]) % 255);
			num2 = (ushort)((num2 + num) % 255);
		}
		return (ushort)((num2 << 8) | num);
	}

	private static string ValidatePath(string shardName)
	{
		try
		{
			if (!string.IsNullOrEmpty(shardName) && shardName.IndexOfAny(_pathSeparatorChars) == -1)
			{
				string fullPath = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(CUOEnviroment.IsUnix ? Environment.SpecialFolder.LocalApplicationData : Environment.SpecialFolder.CommonApplicationData), shardName));
				if (!string.IsNullOrEmpty(fullPath))
				{
					return fullPath;
				}
			}
		}
		catch
		{
		}
		return null;
	}
}
