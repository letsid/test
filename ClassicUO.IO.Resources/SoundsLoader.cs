using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.IO.Audio;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO.Resources;

internal class SoundsLoader : UOFileLoader
{
	private static readonly char[] _configFileDelimiters = new char[3] { ' ', ',', '\t' };

	private static readonly Dictionary<int, Tuple<string, bool>> _musicData = new Dictionary<int, Tuple<string, bool>>();

	private static SoundsLoader _instance;

	private UOFile _file;

	private readonly Sound[] _musics = new Sound[65535];

	private readonly Sound[] _sounds = new Sound[65535];

	private Dictionary<uint, Sound.SoundGroup> _soundGroupDic = new Dictionary<uint, Sound.SoundGroup>();

	private bool _useDigitalMusicFolder;

	public static SoundsLoader Instance => _instance ?? (_instance = new SoundsLoader());

	private SoundsLoader()
	{
	}

	public Sound.SoundGroup GetSoundGroup(uint index)
	{
		if (_soundGroupDic.TryGetValue(index, out var value))
		{
			return value;
		}
		return Sound.SoundGroup.Standard;
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("soundLegacyMUL.uop");
			if (Client.IsUOPInstallation && File.Exists(uOFilePath))
			{
				_file = new UOFileUop(uOFilePath, "build/soundlegacymul/{0:D8}.dat");
				Entries = new UOFileIndex[Math.Max(((UOFileUop)_file).TotalEntriesCount, 65535)];
			}
			else
			{
				uOFilePath = UOFileManager.GetUOFilePath("sound.mul");
				string uOFilePath2 = UOFileManager.GetUOFilePath("soundidx.mul");
				if (!File.Exists(uOFilePath) || !File.Exists(uOFilePath2))
				{
					throw new FileNotFoundException("no sounds found");
				}
				_file = new UOFileMul(uOFilePath, uOFilePath2, 65535);
			}
			_file.FillEntries(ref Entries);
			string uOFilePath3 = UOFileManager.GetUOFilePath("Sound.def");
			if (File.Exists(uOFilePath3))
			{
				using DefReader defReader = new DefReader(uOFilePath3);
				while (defReader.Next())
				{
					int num = defReader.ReadInt();
					if (num >= 0 && num < 65535 && num < _file.Length && Entries[num].Length == 0)
					{
						int[] array = defReader.ReadGroup();
						if (array != null)
						{
							foreach (int num2 in array)
							{
								if (num2 >= -1 && num2 < 65535)
								{
									ref UOFileIndex reference = ref Entries[num];
									if (num2 == -1)
									{
										reference = default(UOFileIndex);
									}
									else if (Entries[num2].Length != 0)
									{
										Entries[num] = Entries[num2];
									}
								}
							}
						}
					}
				}
			}
			string uOFilePath4 = UOFileManager.GetUOFilePath("soundgroups.def");
			if (File.Exists(uOFilePath4))
			{
				using DefReader defReader2 = new DefReader(uOFilePath4);
				while (defReader2.Next())
				{
					int num3 = defReader2.ReadInt();
					if (num3 >= 0 && num3 < 65535 && num3 < _file.Length)
					{
						int value = defReader2.ReadInt();
						if (_soundGroupDic.ContainsKey((uint)num3))
						{
							_soundGroupDic[(uint)num3] = (Sound.SoundGroup)value;
						}
						else
						{
							_soundGroupDic.Add((uint)num3, (Sound.SoundGroup)value);
						}
					}
				}
			}
			uOFilePath = UOFileManager.GetUOFilePath((Client.Version >= ClientVersion.CV_4011C) ? "Music/Digital/Config.txt" : "Music/Config.txt");
			if (File.Exists(uOFilePath))
			{
				using StreamReader streamReader = new StreamReader(uOFilePath);
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					if (TryParseConfigLine(line, out var songData))
					{
						_musicData[songData.Item1] = new Tuple<string, bool>(songData.Item2, songData.Item3);
					}
				}
			}
			else
			{
				_musicData.Add(0, new Tuple<string, bool>("oldult01", item2: true));
				_musicData.Add(1, new Tuple<string, bool>("create1", item2: false));
				_musicData.Add(2, new Tuple<string, bool>("dragflit", item2: false));
				_musicData.Add(3, new Tuple<string, bool>("oldult02", item2: true));
				_musicData.Add(4, new Tuple<string, bool>("oldult03", item2: true));
				_musicData.Add(5, new Tuple<string, bool>("oldult04", item2: true));
				_musicData.Add(6, new Tuple<string, bool>("oldult05", item2: true));
				_musicData.Add(7, new Tuple<string, bool>("oldult06", item2: true));
				_musicData.Add(8, new Tuple<string, bool>("stones2", item2: true));
				_musicData.Add(9, new Tuple<string, bool>("britain1", item2: true));
				_musicData.Add(10, new Tuple<string, bool>("britain2", item2: true));
				_musicData.Add(11, new Tuple<string, bool>("bucsden", item2: true));
				_musicData.Add(12, new Tuple<string, bool>("jhelom", item2: false));
				_musicData.Add(13, new Tuple<string, bool>("lbcastle", item2: false));
				_musicData.Add(14, new Tuple<string, bool>("linelle", item2: false));
				_musicData.Add(15, new Tuple<string, bool>("magincia", item2: true));
				_musicData.Add(16, new Tuple<string, bool>("minoc", item2: true));
				_musicData.Add(17, new Tuple<string, bool>("ocllo", item2: true));
				_musicData.Add(18, new Tuple<string, bool>("samlethe", item2: false));
				_musicData.Add(19, new Tuple<string, bool>("serpents", item2: true));
				_musicData.Add(20, new Tuple<string, bool>("skarabra", item2: true));
				_musicData.Add(21, new Tuple<string, bool>("trinsic", item2: true));
				_musicData.Add(22, new Tuple<string, bool>("vesper", item2: true));
				_musicData.Add(23, new Tuple<string, bool>("wind", item2: true));
				_musicData.Add(24, new Tuple<string, bool>("yew", item2: true));
				_musicData.Add(25, new Tuple<string, bool>("cave01", item2: false));
				_musicData.Add(26, new Tuple<string, bool>("dungeon9", item2: false));
				_musicData.Add(27, new Tuple<string, bool>("forest_a", item2: false));
				_musicData.Add(28, new Tuple<string, bool>("intown01", item2: false));
				_musicData.Add(29, new Tuple<string, bool>("jungle_a", item2: false));
				_musicData.Add(30, new Tuple<string, bool>("mountn_a", item2: false));
				_musicData.Add(31, new Tuple<string, bool>("plains_a", item2: false));
				_musicData.Add(32, new Tuple<string, bool>("sailing", item2: false));
				_musicData.Add(33, new Tuple<string, bool>("swamp_a", item2: false));
				_musicData.Add(34, new Tuple<string, bool>("tavern01", item2: false));
				_musicData.Add(35, new Tuple<string, bool>("tavern02", item2: false));
				_musicData.Add(36, new Tuple<string, bool>("tavern03", item2: false));
				_musicData.Add(37, new Tuple<string, bool>("tavern04", item2: false));
				_musicData.Add(38, new Tuple<string, bool>("combat1", item2: false));
				_musicData.Add(39, new Tuple<string, bool>("combat2", item2: false));
				_musicData.Add(40, new Tuple<string, bool>("combat3", item2: false));
				_musicData.Add(41, new Tuple<string, bool>("approach", item2: false));
				_musicData.Add(42, new Tuple<string, bool>("death", item2: false));
				_musicData.Add(43, new Tuple<string, bool>("victory", item2: false));
				_musicData.Add(44, new Tuple<string, bool>("btcastle", item2: false));
				_musicData.Add(45, new Tuple<string, bool>("nujelm", item2: true));
				_musicData.Add(46, new Tuple<string, bool>("dungeon2", item2: false));
				_musicData.Add(47, new Tuple<string, bool>("cove", item2: true));
				_musicData.Add(48, new Tuple<string, bool>("moonglow", item2: true));
				_musicData.Add(49, new Tuple<string, bool>("zento", item2: true));
				_musicData.Add(50, new Tuple<string, bool>("tokunodungeon", item2: true));
				_musicData.Add(51, new Tuple<string, bool>("Taiko", item2: true));
				_musicData.Add(52, new Tuple<string, bool>("dreadhornarea", item2: true));
				_musicData.Add(53, new Tuple<string, bool>("elfcity", item2: true));
				_musicData.Add(54, new Tuple<string, bool>("grizzledungeon", item2: true));
				_musicData.Add(55, new Tuple<string, bool>("melisandeslair", item2: true));
				_musicData.Add(56, new Tuple<string, bool>("paroxysmuslair", item2: true));
				_musicData.Add(57, new Tuple<string, bool>("gwennoconversation", item2: true));
				_musicData.Add(58, new Tuple<string, bool>("goodendgame", item2: true));
				_musicData.Add(59, new Tuple<string, bool>("goodvsevil", item2: true));
				_musicData.Add(60, new Tuple<string, bool>("greatearthserpents", item2: true));
				_musicData.Add(61, new Tuple<string, bool>("humanoids_u9", item2: true));
				_musicData.Add(62, new Tuple<string, bool>("minocnegative", item2: true));
				_musicData.Add(63, new Tuple<string, bool>("paws", item2: true));
				_musicData.Add(64, new Tuple<string, bool>("selimsbar", item2: true));
				_musicData.Add(65, new Tuple<string, bool>("serpentislecombat_u7", item2: true));
				_musicData.Add(66, new Tuple<string, bool>("valoriaships", item2: true));
			}
			_useDigitalMusicFolder = Directory.Exists(Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, "Music", "Digital"));
		});
	}

	private bool TryGetSound(int sound, out byte[] data, out string name)
	{
		data = null;
		name = null;
		if (sound < 0)
		{
			return false;
		}
		ref UOFileIndex validRefEntry = ref GetValidRefEntry(sound);
		_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
		_file.Seek(validRefEntry.Offset);
		long position = _file.Position;
		if (position < 0 || validRefEntry.Length <= 0)
		{
			return false;
		}
		_file.Seek(position);
		byte[] bytes = _file.ReadArray<byte>(40);
		data = _file.ReadArray<byte>(validRefEntry.Length - 40);
		name = Encoding.UTF8.GetString(bytes);
		int num = name.IndexOf('\0');
		if (num >= 0)
		{
			name = name.Substring(0, num);
		}
		return true;
	}

	private bool TryParseConfigLine(string line, out Tuple<int, string, bool> songData)
	{
		songData = null;
		string[] array = line.Split(_configFileDelimiters);
		if (array.Length < 2 || array.Length > 3)
		{
			return false;
		}
		int item = int.Parse(array[0]);
		string trueFileName = GetTrueFileName(Path.GetFileNameWithoutExtension(array[1]));
		bool item2 = array.Length == 3 && array[2] == "loop";
		songData = new Tuple<int, string, bool>(item, trueFileName, item2);
		return true;
	}

	private static string GetTrueFileName(string name)
	{
		string path2 = Settings.GlobalSettings.UltimaOnlineDirectory + "/Music";
		Regex pattern = new Regex("^" + name + ".mp3", RegexOptions.IgnoreCase);
		string[] files = Directory.GetFiles(path2, "*.mp3", SearchOption.AllDirectories);
		files = Array.FindAll(files, (string path) => pattern.IsMatch(Path.GetFileName(path)));
		if (files != null && files.Length != 0)
		{
			if (files.Length > 1)
			{
				Log.Warn("Ambiguous File reference for " + name + ". More than one file found with different spellings.");
			}
			return Path.GetFileName(files[0]);
		}
		Log.Warn("No File found known as " + name);
		return name;
	}

	private bool TryGetMusicData(int index, out string name, out bool doesLoop)
	{
		name = null;
		doesLoop = false;
		if (_musicData.ContainsKey(index))
		{
			name = _musicData[index].Item1;
			doesLoop = _musicData[index].Item2;
			return true;
		}
		return false;
	}

	public Sound GetSound(int index)
	{
		if (index >= 0 && index < 65535)
		{
			ref Sound reference = ref _sounds[index];
			if (reference == null && TryGetSound(index, out var data, out var name))
			{
				reference = new UOSound(name, index, data);
			}
			return reference;
		}
		return null;
	}

	public Sound GetMusic(int index)
	{
		if (index >= 0 && index < 65535)
		{
			ref Sound reference = ref _musics[index];
			if (reference == null && TryGetMusicData(index, out var name, out var doesLoop))
			{
				reference = (_useDigitalMusicFolder ? new UOMusic(index, name, doesLoop, "Music/Digital/") : new UOMusic(index, name, doesLoop, "Music/"));
			}
			return reference;
		}
		return null;
	}

	public override void ClearResources()
	{
		for (int i = 0; (float)i < 250f; i++)
		{
			if (_sounds[i] != null)
			{
				_sounds[i].Dispose();
				_sounds[i] = null;
			}
			if (_musics[i] != null)
			{
				_musics[i].Dispose();
				_musics[i] = null;
			}
		}
		_musicData.Clear();
	}
}
