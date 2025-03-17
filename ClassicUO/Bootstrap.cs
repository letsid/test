using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.IO;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;

namespace ClassicUO;

internal static class Bootstrap
{
	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetDllDirectory(string lpPathName);

	[STAThread]
	public static void Main(string[] args)
	{
		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
		Log.Start(LogTypes.All);
		CUOEnviroment.GameThread = Thread.CurrentThread;
		CUOEnviroment.GameThread.Name = "CUO_MAIN_THREAD";
		AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("######################## [START LOG] ########################");
			stringBuilder.AppendLine($"ClassicUO [STANDARD_BUILD] - {CUOEnviroment.Version} - {DateTime.Now}");
			stringBuilder.AppendLine(string.Format("OS: {0} {1}", Environment.OSVersion.Platform, Environment.Is64BitOperatingSystem ? "x64" : "x86"));
			stringBuilder.AppendLine("Thread: " + Thread.CurrentThread.Name);
			stringBuilder.AppendLine();
			if (Settings.GlobalSettings != null)
			{
				stringBuilder.AppendLine("Shard: " + Settings.GlobalSettings.IP);
				stringBuilder.AppendLine("ClientVersion: " + Settings.GlobalSettings.ClientVersion);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendFormat("Exception:\n{0}\n", e.ExceptionObject);
			stringBuilder.AppendLine("######################## [END LOG] ########################");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			Log.Panic(e.ExceptionObject.ToString());
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Alathair", "Fehlermeldungen");
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			using LogFile logFile = new LogFile(text, "crash.txt");
			logFile.WriteAsync(stringBuilder.ToString()).RunSynchronously();
		};
		ReadSettingsFromArgs(args);
		if (CUOEnviroment.IsHighDPI)
		{
			Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
		}
		Environment.SetEnvironmentVariable("FNA3D_BACKBUFFER_SCALE_NEAREST", "1");
		Environment.SetEnvironmentVariable("FNA3D_OPENGL_FORCE_COMPATIBILITY_PROFILE", "1");
		Environment.SetEnvironmentVariable("SDL_MOUSE_FOCUS_CLICKTHROUGH", "1");
		Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Plugins"));
		string settingsFilepath = Settings.GetSettingsFilepath();
		if (!Directory.Exists(Path.GetDirectoryName(settingsFilepath)) || !File.Exists(settingsFilepath))
		{
			Settings.GlobalSettings.Save();
		}
		Settings.GlobalSettings = ConfigurationResolver.Load<Settings>(settingsFilepath);
		CUOEnviroment.IsOutlands = Settings.GlobalSettings.ShardType == 2;
		ReadSettingsFromArgs(args);
		if (Settings.GlobalSettings == null)
		{
			Settings.GlobalSettings = new Settings();
			Settings.GlobalSettings.Save();
		}
		if (!CUOEnviroment.IsUnix)
		{
			SetDllDirectory(Path.Combine(CUOEnviroment.ExecutablePath, Environment.Is64BitProcess ? "x64" : "x86"));
		}
		if (string.IsNullOrWhiteSpace(Settings.GlobalSettings.Language))
		{
			Log.Trace("language is not set. Trying to get the OS language.");
			try
			{
				Settings.GlobalSettings.Language = CultureInfo.InstalledUICulture.ThreeLetterWindowsLanguageName;
				if (string.IsNullOrWhiteSpace(Settings.GlobalSettings.Language))
				{
					Log.Warn("cannot read the OS language. Rolled back to ENU");
					Settings.GlobalSettings.Language = "ENU";
				}
				Log.Trace("language set: '" + Settings.GlobalSettings.Language + "'");
			}
			catch
			{
				Log.Warn("cannot read the OS language. Rolled back to ENU");
				Settings.GlobalSettings.Language = "ENU";
			}
		}
		if (string.IsNullOrWhiteSpace(Settings.GlobalSettings.UltimaOnlineDirectory))
		{
			Settings.GlobalSettings.UltimaOnlineDirectory = CUOEnviroment.ExecutablePath;
		}
		uint num = 0u;
		if (!Directory.Exists(Settings.GlobalSettings.UltimaOnlineDirectory) || !File.Exists(UOFileManager.GetUOFilePath("tiledata.mul")))
		{
			num |= 0x100;
		}
		string version = Settings.GlobalSettings.ClientVersion;
		if (!ClientVersionHelper.IsClientVersionValid(Settings.GlobalSettings.ClientVersion, out var version2))
		{
			Log.Warn("Client version [" + version + "] is invalid, let's try to read the client.exe");
			if (!ClientVersionHelper.TryParseFromFile(Path.Combine(Settings.GlobalSettings.UltimaOnlineDirectory, "client.exe"), out version) || !ClientVersionHelper.IsClientVersionValid(version, out version2))
			{
				Log.Error("Invalid client version: " + version);
				num |= 0x200;
			}
			else
			{
				Log.Trace($"Found a valid client.exe [{version} - {version2}]");
				Settings.GlobalSettings.ClientVersion = version;
			}
		}
		if (num != 0)
		{
			if ((num & 0x100) != 0)
			{
				Client.ShowErrorMessage(ResGeneral.YourUODirectoryIsInvalid);
			}
			else if ((num & 0x200) != 0)
			{
				Client.ShowErrorMessage(ResGeneral.YourUOClientVersionIsInvalid);
			}
			PlatformHelper.LaunchBrowser(ResGeneral.ClassicUOLink);
		}
		else
		{
			switch (Settings.GlobalSettings.ForceDriver)
			{
			case 1:
				Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "OpenGL");
				break;
			case 2:
				Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "Vulkan");
				break;
			}
			Client.Run();
		}
		Log.Trace("Closing...");
	}

	private static void ReadSettingsFromArgs(string[] args)
	{
		for (int i = 0; i <= args.Length - 1; i++)
		{
			string text = args[i].ToLower();
			if (text.Length == 0 || text[0] != '-')
			{
				continue;
			}
			text = text.Remove(0, 1);
			string text2 = string.Empty;
			if (i < args.Length - 1 && !string.IsNullOrWhiteSpace(args[i + 1]) && !args[i + 1].StartsWith("-"))
			{
				text2 = args[++i];
			}
			Log.Trace("ARG: " + text + ", VALUE: " + text2);
			switch (text)
			{
			case "settings":
				Settings.CustomSettingsFilepath = text2;
				break;
			case "highdpi":
				CUOEnviroment.IsHighDPI = true;
				break;
			case "username":
				Settings.GlobalSettings.Username = text2;
				break;
			case "password":
				Settings.GlobalSettings.Password = Crypter.Encrypt(text2);
				break;
			case "password_enc":
				Settings.GlobalSettings.Password = text2;
				break;
			case "ip":
				Settings.GlobalSettings.IP = text2;
				break;
			case "port":
				Settings.GlobalSettings.Port = ushort.Parse(text2);
				break;
			case "filesoverride":
			case "uofilesoverride":
				UOFilesOverrideMap.OverrideFile = text2;
				break;
			case "ultimaonlinedirectory":
			case "uopath":
				Settings.GlobalSettings.UltimaOnlineDirectory = text2;
				break;
			case "profilespath":
				Settings.GlobalSettings.ProfilesPath = text2;
				break;
			case "clientversion":
				Settings.GlobalSettings.ClientVersion = text2;
				break;
			case "lastcharname":
			case "lastcharactername":
				LastCharacterManager.OverrideLastCharacter(text2);
				break;
			case "lastservernum":
				Settings.GlobalSettings.LastServerNum = ushort.Parse(text2);
				break;
			case "last_server_name":
				Settings.GlobalSettings.LastServerName = text2;
				break;
			case "fps":
			{
				int num = int.Parse(text2);
				if (num < 12)
				{
					num = 12;
				}
				else if (num > 250)
				{
					num = 250;
				}
				Settings.GlobalSettings.FPS = num;
				break;
			}
			case "debug":
				CUOEnviroment.Debug = true;
				break;
			case "profiler":
				CUOEnviroment.Profiler = bool.Parse(text2);
				break;
			case "saveaccount":
				Settings.GlobalSettings.SaveAccount = bool.Parse(text2);
				break;
			case "autologin":
				Settings.GlobalSettings.AutoLogin = bool.Parse(text2);
				break;
			case "reconnect":
				Settings.GlobalSettings.Reconnect = bool.Parse(text2);
				break;
			case "reconnect_time":
			{
				if (!int.TryParse(text2, out var result2) || result2 < 1000)
				{
					result2 = 1000;
				}
				Settings.GlobalSettings.ReconnectTime = result2;
				break;
			}
			case "music":
			case "login_music":
				Settings.GlobalSettings.LoginMusic = bool.Parse(text2);
				break;
			case "music_volume":
			case "login_music_volume":
				Settings.GlobalSettings.LoginMusicVolume = int.Parse(text2);
				break;
			case "shard":
			case "shard_type":
				Settings.GlobalSettings.ShardType = int.Parse(text2);
				break;
			case "outlands":
				CUOEnviroment.IsOutlands = true;
				break;
			case "fixed_time_step":
				Settings.GlobalSettings.FixedTimeStep = bool.Parse(text2);
				break;
			case "skiploginscreen":
				CUOEnviroment.SkipLoginScreen = true;
				break;
			case "use_verdata":
				Settings.GlobalSettings.UseVerdata = bool.Parse(text2);
				break;
			case "maps_layouts":
				Settings.GlobalSettings.MapsLayouts = text2;
				break;
			case "encryption":
				Settings.GlobalSettings.Encryption = byte.Parse(text2);
				break;
			case "force_driver":
			{
				if (byte.TryParse(text2, out var result))
				{
					switch (result)
					{
					case 1:
						Settings.GlobalSettings.ForceDriver = 1;
						break;
					case 2:
						Settings.GlobalSettings.ForceDriver = 2;
						break;
					default:
						Settings.GlobalSettings.ForceDriver = 0;
						break;
					}
				}
				else
				{
					Settings.GlobalSettings.ForceDriver = 0;
				}
				break;
			}
			case "packetlog":
				CUOEnviroment.PacketLog = true;
				break;
			case "language":
				switch (text2?.ToUpperInvariant())
				{
				case "RUS":
					Settings.GlobalSettings.Language = "RUS";
					break;
				case "FRA":
					Settings.GlobalSettings.Language = "FRA";
					break;
				case "DEU":
					Settings.GlobalSettings.Language = "DEU";
					break;
				case "ESP":
					Settings.GlobalSettings.Language = "ESP";
					break;
				case "JPN":
					Settings.GlobalSettings.Language = "JPN";
					break;
				case "KOR":
					Settings.GlobalSettings.Language = "KOR";
					break;
				case "PTB":
					Settings.GlobalSettings.Language = "PTB";
					break;
				case "ITA":
					Settings.GlobalSettings.Language = "ITA";
					break;
				case "CHT":
					Settings.GlobalSettings.Language = "CHT";
					break;
				default:
					Settings.GlobalSettings.Language = "ENU";
					break;
				}
				break;
			case "no_server_ping":
				CUOEnviroment.NoServerPing = true;
				break;
			}
		}
	}
}
