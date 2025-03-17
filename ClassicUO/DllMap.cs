using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO;

public static class DllMap
{
	private enum DllMapOS
	{
		Linux = 1,
		OSX = 2,
		Solaris = 4,
		FreeBSD = 8,
		OpenBSD = 0x10,
		NetBSD = 0x20,
		Windows = 0x40,
		AIX = 0x80,
		HPUX = 0x100
	}

	private enum DllMapArchitecture
	{
		x86 = 1,
		x86_64 = 2,
		SPARC = 4,
		PPC = 8,
		S390 = 0x10,
		S390X = 0x20,
		ARM = 0x40,
		ARMV8 = 0x80,
		MIPS = 0x100,
		Alpha = 0x200,
		HPPA = 0x400,
		IA64 = 0x800
	}

	private const int LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 4096;

	public static Dictionary<string, string> MapDictionary;

	public static string OS;

	public static string CPU;

	public static bool Optimise;

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetDefaultDllDirectories(int directoryFlags);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern void AddDllDirectory(string lpPathName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetDllDirectory(string lpPathName);

	public static void Initialise(bool optimise = true)
	{
		Optimise = optimise;
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		{
			try
			{
				SetDefaultDllDirectories(4096);
				AddDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.Is64BitProcess ? "x64" : "x86"));
			}
			catch
			{
				SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Environment.Is64BitProcess ? "x64" : "x86"));
			}
		}
		Register(Assembly.GetAssembly(typeof(ColorWriteChannels)));
	}

	private static void Register(Assembly assembly)
	{
		NativeLibrary.SetDllImportResolver(assembly, MapAndLoad);
		OS = GetCurrentPlatform().ToString().ToLowerInvariant();
		CPU = GetCurrentRuntimeArchitecture().ToString().ToLowerInvariant();
		if (OS == "windows" && Optimise)
		{
			MapDictionary = new Dictionary<string, string>
			{
				{ "SDL2", "SDL2.dll" },
				{ "SDL_image", "SDL_image.dll" },
				{ "FAudio", "FAudio.dll" }
			};
			return;
		}
		string text = Path.Combine(Path.GetDirectoryName(assembly.Location), Path.GetFileNameWithoutExtension(assembly.Location) + ".dll.config");
		if (!File.Exists(text))
		{
			Console.WriteLine("=== Cannot find XML: " + text);
			return;
		}
		XElement root = XElement.Load(text);
		MapDictionary = new Dictionary<string, string>();
		ParseXml(root, matchCPU: true);
		ParseXml(root, matchCPU: false);
	}

	private static void ParseXml(XContainer root, bool matchCPU)
	{
		foreach (XElement item in root.Elements("dllmap"))
		{
			if (item.Attribute("os").ToString().IndexOf(OS) < 0)
			{
				continue;
			}
			if (matchCPU)
			{
				if (item.Attribute("cpu") == null || item.Attribute("cpu").ToString().IndexOf(CPU) < 0)
				{
					continue;
				}
			}
			else if (item.Attribute("cpu") != null && item.Attribute("cpu").ToString().IndexOf(CPU) < 0)
			{
				continue;
			}
			string value = item.Attribute("dll").Value;
			string value2 = item.Attribute("target").Value;
			if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(value2) && !MapDictionary.ContainsKey(value))
			{
				MapDictionary.Add(value, value2);
			}
		}
	}

	private static IntPtr MapAndLoad(string libraryName, Assembly assembly, DllImportSearchPath? dllImportSearchPath)
	{
		string mappedLibName = (MapLibraryName(libraryName, out mappedLibName) ? mappedLibName : libraryName);
		return NativeLibrary.Load(mappedLibName, assembly, dllImportSearchPath);
	}

	private static bool MapLibraryName(string originalLibName, out string mappedLibName)
	{
		return MapDictionary.TryGetValue(originalLibName, out mappedLibName);
	}

	private static DllMapOS GetCurrentPlatform()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return DllMapOS.Linux;
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return DllMapOS.Windows;
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return DllMapOS.OSX;
		}
		string text = RuntimeInformation.OSDescription.ToUpperInvariant();
		foreach (DllMapOS item in Enum.GetValues(typeof(DllMapOS)).Cast<DllMapOS>().Except(new DllMapOS[3]
		{
			DllMapOS.Linux,
			DllMapOS.Windows,
			DllMapOS.OSX
		}))
		{
			if (text.Contains(item.ToString().ToUpperInvariant()))
			{
				return item;
			}
		}
		throw new PlatformNotSupportedException("Couldn't detect platform: " + RuntimeInformation.OSDescription);
	}

	private static DllMapArchitecture GetCurrentRuntimeArchitecture()
	{
		switch (RuntimeInformation.ProcessArchitecture)
		{
		case Architecture.Arm:
			return DllMapArchitecture.ARM;
		case Architecture.X64:
			return DllMapArchitecture.x86_64;
		case Architecture.X86:
			return DllMapArchitecture.x86;
		default:
		{
			typeof(object).Module.GetPEKind(out var _, out var machine);
			return machine switch
			{
				ImageFileMachine.I386 => DllMapArchitecture.x86, 
				ImageFileMachine.AMD64 => DllMapArchitecture.x86_64, 
				ImageFileMachine.ARM => DllMapArchitecture.ARM, 
				ImageFileMachine.IA64 => DllMapArchitecture.IA64, 
				_ => throw new PlatformNotSupportedException("Couldn't detect the current architecture."), 
			};
		}
		}
	}
}
