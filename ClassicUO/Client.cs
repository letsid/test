using System;
using System.IO;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.IO;
using ClassicUO.Network;
using ClassicUO.Network.Encryption;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using SDL2;

namespace ClassicUO;

internal static class Client
{
	public static ClientVersion Version { get; private set; }

	public static ClientFlags Protocol { get; set; }

	public static string ClientPath { get; private set; }

	public static bool IsUOPInstallation { get; private set; }

	public static bool UseUOPGumps { get; set; }

	public static GameController Game { get; private set; }

	public static void Run()
	{
		Load();
		Log.Trace("Running game...");
		SDLHelper.SetUltimaOnlineWindowClass();
		using (Game = new GameController())
		{
			CUOEnviroment.IsHighDPI = Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1";
			if (CUOEnviroment.IsHighDPI)
			{
				Log.Trace("HIGH DPI - ENABLED");
			}
			Log.Trace("Done!");
			UoAssist.Start();
			Game.Run();
		}
		CheckSumCalculator.RequestStop();
		Log.Trace("Exiting game...");
	}

	public static void ShowErrorMessage(string msg)
	{
		SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "ERROR", msg, IntPtr.Zero);
	}

	private static void Load()
	{
		string ultimaOnlineDirectory = Settings.GlobalSettings.UltimaOnlineDirectory;
		Log.Trace("Ultima Online installation folder: " + ultimaOnlineDirectory);
		Log.Trace("Loading files...");
		if (!string.IsNullOrWhiteSpace(Settings.GlobalSettings.ClientVersion))
		{
			Settings.GlobalSettings.ClientVersion = Settings.GlobalSettings.ClientVersion.Replace(",", ".").Replace(" ", "").ToLower();
		}
		string version = Settings.GlobalSettings.ClientVersion;
		if (!Directory.Exists(ultimaOnlineDirectory))
		{
			Log.Error("Invalid client directory: " + ultimaOnlineDirectory);
			ShowErrorMessage(string.Format(ResErrorMessages.ClientPathIsNotAValidUODirectory, ultimaOnlineDirectory));
			throw new InvalidClientDirectory("'" + ultimaOnlineDirectory + "' is not a valid directory");
		}
		if (!ClientVersionHelper.IsClientVersionValid(version, out var version2))
		{
			Log.Warn("Client version [" + version + "] is invalid, let's try to read the client.exe");
			if (!ClientVersionHelper.TryParseFromFile(Path.Combine(ultimaOnlineDirectory, "client.exe"), out version) || !ClientVersionHelper.IsClientVersionValid(version, out version2))
			{
				Log.Error("Invalid client version: " + version);
				ShowErrorMessage(string.Format(ResGumps.ImpossibleToDefineTheClientVersion0, version));
				throw new InvalidClientVersion("Invalid client version: '" + version + "'");
			}
			Log.Trace($"Found a valid client.exe [{version} - {version2}]");
			Settings.GlobalSettings.ClientVersion = version;
		}
		Version = version2;
		ClientPath = ultimaOnlineDirectory;
		IsUOPInstallation = Version >= ClientVersion.CV_7000 && File.Exists(UOFileManager.GetUOFilePath("MainMisc.uop"));
		Protocol = ClientFlags.CF_T2A;
		if (Version >= ClientVersion.CV_200)
		{
			Protocol |= ClientFlags.CF_RE;
		}
		if (Version >= ClientVersion.CV_300)
		{
			Protocol |= ClientFlags.CF_TD;
		}
		if (Version >= ClientVersion.CV_308)
		{
			Protocol |= ClientFlags.CF_LBR;
		}
		if (Version >= ClientVersion.CV_308Z)
		{
			Protocol |= ClientFlags.CF_AOS;
		}
		if (Version >= ClientVersion.CV_405A)
		{
			Protocol |= ClientFlags.CF_SE;
		}
		if (Version >= ClientVersion.CV_60144)
		{
			Protocol |= ClientFlags.CF_SA;
		}
		Log.Trace("Client path: '" + ultimaOnlineDirectory + "'");
		Log.Trace($"Client version: {version2}");
		Log.Trace($"Protocol: {Protocol}");
		Log.Trace("UOP? " + (IsUOPInstallation ? "yes" : "no"));
		UOFileManager.Load();
		StaticFilters.Load();
		BuffTable.Load();
		ChairTable.Load();
		Log.Trace("Network calibration...");
		PacketHandlers.Load();
		UltimaLive.Enable();
		PacketsTable.AdjustPacketSizeByVersion(Version);
		if (Settings.GlobalSettings.Encryption != 0)
		{
			Log.Trace("Calculating encryption by client version...");
			EncryptionHelper.CalculateEncryption(Version);
			Log.Trace($"encryption: {EncryptionHelper.Type}");
			if (EncryptionHelper.Type != (ENCRYPTION_TYPE)Settings.GlobalSettings.Encryption)
			{
				Log.Warn($"Encryption found: {EncryptionHelper.Type}");
				Settings.GlobalSettings.Encryption = (byte)EncryptionHelper.Type;
			}
		}
	}
}
