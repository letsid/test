using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.IO.Resources;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;

namespace ClassicUO.IO;

internal static class UOFileManager
{
	public static string GetUOFilePath(string file)
	{
		if (!UOFilesOverrideMap.Instance.TryGetValue(file.ToLowerInvariant(), out var value))
		{
			value = Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, file);
		}
		if (!PlatformHelper.IsWindows && !File.Exists(value))
		{
			string fullPath = Path.GetFullPath(new FileInfo(value).DirectoryName ?? Settings.GlobalSettings.UltimaOnlineDirectory);
			if (Directory.Exists(fullPath))
			{
				string[] files = Directory.GetFiles(fullPath);
				int num = 0;
				string[] array = files;
				foreach (string text in array)
				{
					if (string.Equals(text, value, StringComparison.OrdinalIgnoreCase))
					{
						num++;
						value = text;
					}
				}
				if (num > 1)
				{
					Log.Warn("Multiple files with ambiguous case found for " + file + ", using " + Path.GetFileName(value) + ". Check your data directory for duplicate files.");
				}
			}
		}
		return value;
	}

	public unsafe static void Load()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		UOFilesOverrideMap.Instance.Load();
		if (!Task.WhenAll(new List<Task>
		{
			AnimationsLoader.Instance.Load(),
			AnimDataLoader.Instance.Load(),
			ArtLoader.Instance.Load(),
			MapLoader.Instance.Load(),
			ClilocLoader.Instance.Load(Settings.GlobalSettings.Language),
			GumpsLoader.Instance.Load(),
			FontsLoader.Instance.Load(),
			HuesLoader.Instance.Load(),
			TileDataLoader.Instance.Load(),
			MultiLoader.Instance.Load(),
			SkillsLoader.Instance.Load().ContinueWith((Task t) => ProfessionLoader.Instance.Load()),
			TexmapsLoader.Instance.Load(),
			SpeechesLoader.Instance.Load(),
			LightsLoader.Instance.Load(),
			SoundsLoader.Instance.Load(),
			MultiMapLoader.Instance.Load()
		}).Wait(TimeSpan.FromSeconds(10.0)))
		{
			Log.Panic("Loading files timeout.");
		}
		Read_Art_def();
		Read_NoDraw_def();
		Read_AnimPHue_Def();
		UOFileMul file = Verdata.File;
		bool flag = Client.Version < ClientVersion.CV_500A || (file != null && file.Length != 0L && Verdata.Patches.Length != 0);
		if (!Settings.GlobalSettings.UseVerdata && flag)
		{
			Settings.GlobalSettings.UseVerdata = flag;
		}
		Log.Trace("Use verdata.mul: " + (Settings.GlobalSettings.UseVerdata ? "Yes" : "No"));
		if (Settings.GlobalSettings.UseVerdata && file != null && Verdata.Patches.Length != 0)
		{
			Log.Info(">> PATCHING WITH VERDATA.MUL");
			for (int i = 0; i < Verdata.Patches.Length; i++)
			{
				ref UOFileIndex5D reference = ref Verdata.Patches[i];
				Log.Info($">>> patching  FileID: {reference.FileID}  -  BlockID: {reference.BlockID}");
				if (reference.FileID == 0)
				{
					MapLoader.Instance.PatchMapBlock(reference.BlockID, reference.Position);
				}
				else if (reference.FileID == 2)
				{
					MapLoader.Instance.PatchStaticBlock(reference.BlockID, (ulong)(file.StartAddress.ToInt64() + reference.Position), reference.Length);
				}
				else if (reference.FileID == 4)
				{
					if (reference.BlockID < ArtLoader.Instance.Entries.Length)
					{
						ArtLoader.Instance.Entries[reference.BlockID] = new UOFileIndex(file.StartAddress, (uint)file.Length, reference.Position, (int)reference.Length, 0, 0, 0, 0);
					}
				}
				else if (reference.FileID == 12)
				{
					GumpsLoader.Instance.Entries[reference.BlockID] = new UOFileIndex(file.StartAddress, (uint)file.Length, reference.Position, (int)reference.Length, 0, (short)(reference.GumpData >> 16), (short)(reference.GumpData & 0xFFFF), 0);
				}
				else if (reference.FileID == 14 && reference.BlockID < MultiLoader.Instance.Count)
				{
					MultiLoader.Instance.Entries[reference.BlockID] = new UOFileIndex(file.StartAddress, (uint)file.Length, reference.Position, (int)reference.Length, 0, 0, 0, 0);
				}
				else if (reference.FileID == 16 && reference.BlockID < SkillsLoader.Instance.SkillsCount)
				{
					SkillEntry skillEntry = SkillsLoader.Instance.Skills[(int)reference.BlockID];
					if (skillEntry != null)
					{
						StackDataReader stackDataReader = new StackDataReader(new ReadOnlySpan<byte>((void*)file.StartAddress, (int)file.Length));
						skillEntry.HasAction = stackDataReader.ReadUInt8() != 0;
						skillEntry.Name = stackDataReader.ReadASCII((int)(reference.Length - 1));
						stackDataReader.Release();
					}
				}
				else if (reference.FileID == 30)
				{
					file.Seek(0);
					file.Skip((int)reference.Position);
					if (reference.Length == 836)
					{
						int num = (int)(reference.BlockID * 32);
						if (num + 32 <= TileDataLoader.Instance.LandData.Length)
						{
							file.ReadUInt();
							for (int j = 0; j < 32; j++)
							{
								ulong flags = ((Client.Version >= ClientVersion.CV_7090) ? file.ReadULong() : file.ReadUInt());
								TileDataLoader.Instance.LandData[num + j] = new LandTiles(flags, file.ReadUShort(), file.ReadASCII(20));
							}
						}
					}
					else
					{
						if (reference.Length != 1188)
						{
							continue;
						}
						int num2 = (int)((reference.BlockID - 512) * 32);
						if (num2 + 32 <= TileDataLoader.Instance.StaticData.Length)
						{
							file.ReadUInt();
							for (int k = 0; k < 32; k++)
							{
								ulong flags2 = ((Client.Version >= ClientVersion.CV_7090) ? file.ReadULong() : file.ReadUInt());
								TileDataLoader.Instance.StaticData[num2 + k] = new StaticTiles(flags2, file.ReadByte(), file.ReadByte(), file.ReadInt(), file.ReadUShort(), file.ReadUShort(), file.ReadUShort(), file.ReadByte(), file.ReadASCII(20));
							}
						}
					}
				}
				else if (reference.FileID == 32)
				{
					if (reference.BlockID < HuesLoader.Instance.HuesCount)
					{
						VerdataHuesGroup verdataHuesGroup = Marshal.PtrToStructure<VerdataHuesGroup>(file.StartAddress + (int)reference.Position);
						HuesGroup[] huesRange = HuesLoader.Instance.HuesRange;
						huesRange[reference.BlockID].Header = verdataHuesGroup.Header;
						for (int l = 0; l < 8; l++)
						{
							Array.Copy(verdataHuesGroup.Entries[l].ColorTable, huesRange[reference.BlockID].Entries[l].ColorTable, 32);
						}
					}
				}
				else if (reference.FileID != 5 && reference.FileID != 6)
				{
					Log.Warn($"Unused verdata block\tFileID: {reference.FileID}\tBlockID: {reference.BlockID}");
				}
			}
			Log.Info("<< PATCHED.");
		}
		Log.Trace($"Files loaded in: {stopwatch.ElapsedMilliseconds} ms!");
		stopwatch.Stop();
	}

	internal static void MapLoaderReLoad(MapLoader newloader)
	{
		MapLoader.Instance?.Dispose();
		MapLoader.Instance = newloader;
	}

	public static string GetOneTip()
	{
		string[] array = File.ReadAllLines(GetUOFilePath("tips.txt"));
		int num = new Random().Next(0, array.Length - 1);
		return array[num];
	}

	public static void Read_NoDraw_def()
	{
		using StreamReader streamReader = new StreamReader(File.OpenRead(GetUOFilePath("nodraw.def")));
		string text = null;
		TileDataLoader instance = TileDataLoader.Instance;
		while ((text = streamReader.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length < 2 || !(text.Substring(0, 2) == "//"))
			{
				if (text.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
				{
					text = text.Substring(2);
				}
				if (int.TryParse(text, NumberStyles.HexNumber, null, out var result) && !instance.NoDrawTiles.ContainsKey(result))
				{
					instance.NoDrawTiles.Add(result, value: true);
				}
			}
		}
		StackDataWriter stackDataWriter = default(StackDataWriter);
		instance.NoDrawTiles.SortKeys();
		foreach (int key in instance.NoDrawTiles.Keys)
		{
			stackDataWriter.WriteInt16LE((short)key);
		}
		instance.NoDrawTilesVerificationBuffer = new ReadOnlySpan<byte>(stackDataWriter.AllocatedBuffer, 0, stackDataWriter.BytesWritten).ToArray();
	}

	public static void Read_AnimPHue_Def()
	{
		string uOFilePath = GetUOFilePath("animphue.def");
		TileDataLoader instance = TileDataLoader.Instance;
		using DefReader defReader = new DefReader(uOFilePath, 1);
		while (defReader.Next())
		{
			int key = defReader.ReadInt();
			int num = defReader.ReadInt();
			if (!instance.NoDrawTiles.ContainsKey(key) && num > 0)
			{
				instance.AnimPHueTiles.Add(key, value: true);
			}
		}
	}

	private static void Read_Art_def()
	{
		string uOFilePath = GetUOFilePath("art.def");
		if (!File.Exists(uOFilePath))
		{
			return;
		}
		TileDataLoader instance = TileDataLoader.Instance;
		ArtLoader instance2 = ArtLoader.Instance;
		using DefReader defReader = new DefReader(uOFilePath, 1);
		while (defReader.Next())
		{
			int num = defReader.ReadInt();
			if (num < 0 || num >= 16384 + instance.StaticData.Length)
			{
				continue;
			}
			int[] array = defReader.ReadGroup();
			if (array == null)
			{
				continue;
			}
			foreach (int num2 in array)
			{
				if (num2 < 0 || num2 >= 16384 + instance.StaticData.Length)
				{
					continue;
				}
				if (num < instance2.Entries.Length && num2 < instance2.Entries.Length)
				{
					ref UOFileIndex validRefEntry = ref instance2.GetValidRefEntry(num);
					ref UOFileIndex validRefEntry2 = ref instance2.GetValidRefEntry(num2);
					if (validRefEntry.Equals(UOFileIndex.Invalid) && !validRefEntry2.Equals(UOFileIndex.Invalid))
					{
						instance2.Entries[num] = instance2.Entries[num2];
					}
				}
				if (num < 16384 && num2 < 16384 && num2 < instance.LandData.Length && num < instance.LandData.Length && !instance.LandData[num2].Equals(null) && instance.LandData[num].Equals(null))
				{
					instance.LandData[num] = instance.LandData[num2];
					break;
				}
				if (num >= 16384 && num2 >= 16384 && num < instance.StaticData.Length && num2 < instance.StaticData.Length && instance.StaticData[num].Equals(null) && !instance.StaticData[num2].Equals(null))
				{
					instance.StaticData[num] = instance.StaticData[num2];
					break;
				}
			}
		}
	}
}
