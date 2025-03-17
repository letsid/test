using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Utility.Logging;
using Microsoft.Win32;

namespace ClassicUO.Utility.Platforms;

internal static class PlatformHelper
{
	public static readonly bool IsMonoRuntime = Type.GetType("Mono.Runtime") != null;

	public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

	public static readonly bool IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

	public static readonly bool IsOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

	public static void LaunchBrowser(string url)
	{
		try
		{
			string text = "http";
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\http\\UserChoice", writable: false);
			if (registryKey != null)
			{
				object value = registryKey.GetValue("ProgId");
				if (value != null)
				{
					text = value.ToString();
				}
			}
			RegistryKey registryKey2 = Registry.ClassesRoot.OpenSubKey(text + "\\shell\\open\\command", writable: false);
			if (registryKey2 == null)
			{
				return;
			}
			string exeName = registryKey2.GetValue(null).ToString().ToLower()
				.Replace("\"", "");
			if (!exeName.EndsWith("exe"))
			{
				exeName = exeName.Substring(0, exeName.LastIndexOf(".exe") + 4);
			}
			bool flag = true;
			if (url.StartsWith("www.alathair.de/staff_db/") || url.StartsWith("www.alathair.de/html/redirect.php?url="))
			{
				flag = false;
			}
			if (flag)
			{
				UIManager.Add(new MessageBoxGump(300, 200, "Der Server möchte folgende URL öffnen: \n \n" + url.ToString(), delegate(bool s)
				{
					if (s)
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = exeName,
							Arguments = url,
							UseShellExecute = true
						});
					}
				}, hasBackground: true, MessageButtonType.OK_CANCEL, 32));
			}
			else
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = exeName,
					Arguments = url,
					UseShellExecute = true
				});
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
	}
}
