using System;
using System.Reflection;
using System.Threading;

namespace ClassicUO;

internal static class CUOEnviroment
{
	public static Thread GameThread;

	public static float DPIScaleFactor = 1f;

	public static bool NoSound;

	public static string[] Args;

	public static string[] Plugins;

	public static bool Debug;

	public static bool Profiler;

	public static bool IsHighDPI;

	public static uint CurrentRefreshRate;

	public static bool SkipLoginScreen;

	public static bool IsOutlands;

	public static bool PacketLog;

	public static bool NoServerPing;

	public static readonly bool IsUnix = Environment.OSVersion.Platform != PlatformID.Win32NT && Environment.OSVersion.Platform != PlatformID.Win32Windows && Environment.OSVersion.Platform != 0 && Environment.OSVersion.Platform != PlatformID.WinCE;

	public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

	public static readonly string ExecutablePath = Environment.CurrentDirectory;
}
