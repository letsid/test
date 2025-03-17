using System;
using System.IO;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Configuration;

internal static class ProfileManager
{
	public static Profile CurrentProfile { get; private set; }

	public static string ProfilePath { get; private set; }

	public static void Load(string servername, string username, string charactername)
	{
		string path = ((!string.IsNullOrWhiteSpace(Settings.GlobalSettings.ProfilesPath)) ? Settings.GlobalSettings.ProfilesPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Alathair", "Einstellungen", "ProfileClassicUO"));
		string text = FileSystemHelper.CreateFolderIfNotExists(path, username, charactername);
		string file = Path.Combine(text, "profile.json");
		ProfilePath = text;
		CurrentProfile = ConfigurationResolver.Load<Profile>(file) ?? new Profile();
		CurrentProfile.Username = username;
		CurrentProfile.ServerName = servername;
		CurrentProfile.CharacterName = charactername;
		ValidateFields(CurrentProfile);
	}

	private static void ValidateFields(Profile profile)
	{
		if (profile != null)
		{
			if (string.IsNullOrEmpty(profile.ServerName))
			{
				throw new InvalidDataException();
			}
			if (string.IsNullOrEmpty(profile.Username))
			{
				throw new InvalidDataException();
			}
			if (string.IsNullOrEmpty(profile.CharacterName))
			{
				throw new InvalidDataException();
			}
			if (profile.WindowClientBounds.X < 600)
			{
				profile.WindowClientBounds = new Point(600, profile.WindowClientBounds.Y);
			}
			if (profile.WindowClientBounds.Y < 480)
			{
				profile.WindowClientBounds = new Point(profile.WindowClientBounds.X, 480);
			}
			SDLHelper.SetNoCloseOnAltF4Hint(profile.PreventAltF4ToCloseClient);
		}
	}

	public static void UnLoadProfile()
	{
		CurrentProfile = null;
	}
}
