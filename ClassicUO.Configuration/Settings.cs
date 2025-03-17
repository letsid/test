using System.IO;
using Microsoft.Xna.Framework;
using TinyJson;

namespace ClassicUO.Configuration;

internal sealed class Settings
{
	public const string SETTINGS_FILENAME = "settings.json";

	public static Settings GlobalSettings = new Settings();

	public static string CustomSettingsFilepath = null;

	[JsonProperty("username")]
	public string Username { get; set; } = string.Empty;

	[JsonProperty("password")]
	public string Password { get; set; } = string.Empty;

	[JsonProperty("ip")]
	public string IP { get; set; } = "srv.alathair.de";

	[JsonProperty("port")]
	public ushort Port { get; set; } = 2593;

	[JsonProperty("ultimaonlinedirectory")]
	public string UltimaOnlineDirectory { get; set; } = "..\\";

	[JsonProperty("profilespath")]
	public string ProfilesPath { get; set; } = string.Empty;

	[JsonProperty("clientversion")]
	public string ClientVersion { get; set; } = "7.0.18.0";

	[JsonProperty("lang")]
	public string Language { get; set; } = "DEU";

	[JsonProperty("lastservernum")]
	public ushort LastServerNum { get; set; } = 1;

	[JsonProperty("last_server_name")]
	public string LastServerName { get; set; } = string.Empty;

	[JsonProperty("fps")]
	public int FPS { get; set; } = 60;

	[JsonProperty("window_position")]
	public Point? WindowPosition { get; set; }

	[JsonProperty("window_size")]
	public Point? WindowSize { get; set; }

	[JsonProperty("is_win_maximized")]
	public bool IsWindowMaximized { get; set; } = true;

	[JsonProperty("saveaccount")]
	public bool SaveAccount { get; set; }

	[JsonProperty("autologin")]
	public bool AutoLogin { get; set; }

	[JsonProperty("reconnect")]
	public bool Reconnect { get; set; }

	[JsonProperty("reconnect_time")]
	public int ReconnectTime { get; set; } = 1;

	[JsonProperty("login_music")]
	public bool LoginMusic { get; set; }

	[JsonProperty("login_music_volume")]
	public int LoginMusicVolume { get; set; } = 50;

	[JsonProperty("shard_type")]
	public int ShardType { get; set; }

	[JsonProperty("fixed_time_step")]
	public bool FixedTimeStep { get; set; } = true;

	[JsonProperty("run_mouse_in_separate_thread")]
	public bool RunMouseInASeparateThread { get; set; } = true;

	[JsonProperty("force_driver")]
	public byte ForceDriver { get; set; }

	[JsonProperty("use_verdata")]
	public bool UseVerdata { get; set; }

	[JsonProperty("maps_layouts")]
	public string MapsLayouts { get; set; }

	[JsonProperty("encryption")]
	public byte Encryption { get; set; } = 1;

	public static string GetSettingsFilepath()
	{
		if (CustomSettingsFilepath != null)
		{
			if (Path.IsPathRooted(CustomSettingsFilepath))
			{
				return CustomSettingsFilepath;
			}
			return Path.Combine(CUOEnviroment.ExecutablePath, CustomSettingsFilepath);
		}
		return Path.Combine(CUOEnviroment.ExecutablePath, "settings.json");
	}

	public void Save()
	{
		Settings settings = this.Encode(pretty: true).Decode<Settings>();
		if (!settings.SaveAccount)
		{
			settings.Username = string.Empty;
			settings.Password = string.Empty;
		}
		settings.ProfilesPath = string.Empty;
		ConfigurationResolver.Save(settings, GetSettingsFilepath());
	}
}
