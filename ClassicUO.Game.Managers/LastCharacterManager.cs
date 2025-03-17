using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Configuration;

namespace ClassicUO.Game.Managers;

public static class LastCharacterManager
{
	private static readonly string _lastCharacterFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Profiles");

	private static readonly string _lastCharacterFile = Path.Combine(_lastCharacterFilePath, "lastcharacter.json");

	private static List<LastCharacterInfo> LastCharacters { get; set; }

	private static string LastCharacterNameOverride { get; set; }

	public static void Load()
	{
		LastCharacters = new List<LastCharacterInfo>();
		if (!File.Exists(_lastCharacterFile))
		{
			ConfigurationResolver.Save(LastCharacters, _lastCharacterFile);
		}
		LastCharacters = ConfigurationResolver.Load<List<LastCharacterInfo>>(_lastCharacterFile);
		if (LastCharacters == null)
		{
			LastCharacters = new List<LastCharacterInfo>();
		}
	}

	public static void Save(string account, string server, string name)
	{
		LastCharacterInfo lastCharacterInfo = LastCharacters.FirstOrDefault((LastCharacterInfo c) => c.AccountName.Equals(account) && c.ServerName == server);
		if (!string.IsNullOrEmpty(LastCharacterNameOverride) && !LastCharacterNameOverride.Equals(name))
		{
			LastCharacterNameOverride = string.Empty;
		}
		if (lastCharacterInfo != null)
		{
			lastCharacterInfo.LastCharacterName = name;
		}
		else
		{
			LastCharacters.Add(new LastCharacterInfo
			{
				ServerName = server,
				LastCharacterName = name,
				AccountName = account
			});
		}
		ConfigurationResolver.Save(LastCharacters, _lastCharacterFile);
	}

	public static string GetLastCharacter(string account, string server)
	{
		if (LastCharacters == null)
		{
			Load();
		}
		if (!string.IsNullOrEmpty(LastCharacterNameOverride))
		{
			return LastCharacterNameOverride;
		}
		LastCharacterInfo lastCharacterInfo = LastCharacters.FirstOrDefault((LastCharacterInfo c) => c.AccountName.Equals(account) && c.ServerName == server);
		if (lastCharacterInfo == null)
		{
			return string.Empty;
		}
		return lastCharacterInfo.LastCharacterName;
	}

	public static void OverrideLastCharacter(string name)
	{
		LastCharacterNameOverride = name;
	}
}
