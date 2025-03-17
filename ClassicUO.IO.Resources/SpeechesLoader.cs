using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Data;

namespace ClassicUO.IO.Resources;

internal class SpeechesLoader : UOFileLoader
{
	private static SpeechesLoader _instance;

	private SpeechEntry[] _speech;

	public static SpeechesLoader Instance => _instance ?? (_instance = new SpeechesLoader());

	private SpeechesLoader()
	{
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("speech.mul");
			if (!File.Exists(uOFilePath))
			{
				_speech = Array.Empty<SpeechEntry>();
			}
			else
			{
				UOFileMul uOFileMul = new UOFileMul(uOFilePath);
				List<SpeechEntry> list = new List<SpeechEntry>();
				while (uOFileMul.Position < uOFileMul.Length)
				{
					int id = uOFileMul.ReadUShortReversed();
					int num = uOFileMul.ReadUShortReversed();
					if (num > 0)
					{
						list.Add(new SpeechEntry(id, string.Intern(Encoding.UTF8.GetString((byte*)(void*)uOFileMul.PositionAddress, num))));
						uOFileMul.Skip(num);
					}
				}
				_speech = list.ToArray();
				uOFileMul.Dispose();
			}
		});
	}

	public bool IsMatch(string input, in SpeechEntry entry)
	{
		string[] keywords = entry.Keywords;
		for (int i = 0; i < keywords.Length; i++)
		{
			if (keywords[i].Length > input.Length || keywords[i].Length == 0 || (!entry.CheckStart && input.IndexOf(keywords[i], 0, keywords[i].Length, StringComparison.InvariantCultureIgnoreCase) == -1) || (!entry.CheckEnd && input.IndexOf(keywords[i], input.Length - keywords[i].Length, StringComparison.InvariantCultureIgnoreCase) == -1))
			{
				continue;
			}
			for (int num = input.IndexOf(keywords[i], StringComparison.InvariantCultureIgnoreCase); num >= 0; num = input.IndexOf(keywords[i], num + 1, StringComparison.InvariantCultureIgnoreCase))
			{
				if ((num - 1 < 0 || char.IsWhiteSpace(input[num - 1]) || !char.IsLetter(input[num - 1])) && (num + keywords[i].Length >= input.Length || char.IsWhiteSpace(input[num + keywords[i].Length]) || !char.IsLetter(input[num + keywords[i].Length])))
				{
					return true;
				}
			}
		}
		return false;
	}

	public List<SpeechEntry> GetKeywords(string text)
	{
		List<SpeechEntry> list = new List<SpeechEntry>();
		if (Client.Version < ClientVersion.CV_305D)
		{
			return list;
		}
		text = text.TrimStart(' ').TrimEnd(' ');
		for (int i = 0; i < _speech.Length; i++)
		{
			SpeechEntry entry = _speech[i];
			if (IsMatch(text, in entry))
			{
				list.Add(entry);
			}
		}
		list.Sort();
		return list;
	}
}
