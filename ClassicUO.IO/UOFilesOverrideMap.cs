using System;
using System.Collections.Generic;
using System.IO;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO;

internal class UOFilesOverrideMap : Dictionary<string, string>
{
	public static string OverrideFile { get; set; }

	public static UOFilesOverrideMap Instance { get; private set; } = new UOFilesOverrideMap();

	private UOFilesOverrideMap()
	{
	}

	internal void Load()
	{
		if (!File.Exists(OverrideFile))
		{
			Log.Trace("No Override File found, ignoring.");
			return;
		}
		Log.Trace("Loading Override File:\t\t" + OverrideFile);
		using FileStream stream = new FileStream(OverrideFile, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream);
		while (!streamReader.EndOfStream)
		{
			try
			{
				string text = streamReader.ReadLine();
				string text2 = text.TrimStart(' ');
				if (text2.IndexOf(';') != 0 && text2.IndexOf('#') != 0)
				{
					string[] array = text.Split('=');
					if (array.Length == 2)
					{
						string text3 = array[0].ToLowerInvariant();
						string text4 = array[1];
						Log.Trace("Override entry: " + text3 + " => " + text4 + ".");
						Add(text3, text4);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn("Something went wrong when trying to parse UOFileOverride file.");
				Log.Warn(ex.ToString());
			}
		}
	}
}
