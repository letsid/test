using System;
using System.Runtime.InteropServices;

namespace ClassicUO.Utility.Platforms;

internal static class Native
{
	private abstract class NativeLoader
	{
		public abstract IntPtr LoadLibrary(string name);

		public abstract IntPtr GetProcessAddress(IntPtr module, string name);

		public abstract int FreeLibrary(IntPtr module);
	}

	private class WinNativeLoader : NativeLoader
	{
		[DllImport("kernel32", EntryPoint = "LoadLibrary")]
		private static extern IntPtr LoadLibrary_WIN(string fileName);

		[DllImport("kernel32", EntryPoint = "GetProcAddress")]
		private static extern IntPtr GetProcAddress_WIN(IntPtr module, string procName);

		[DllImport("kernel32", EntryPoint = "FreeLibrary")]
		private static extern int FreeLibrary_WIN(IntPtr module);

		public override IntPtr LoadLibrary(string name)
		{
			return LoadLibrary_WIN(name);
		}

		public override IntPtr GetProcessAddress(IntPtr module, string name)
		{
			return GetProcAddress_WIN(module, name);
		}

		public override int FreeLibrary(IntPtr module)
		{
			return FreeLibrary_WIN(module);
		}
	}

	private class UnixNativeLoader : NativeLoader
	{
		private const string LibName = "libdl";

		public const int RTLD_NOW = 2;

		[DllImport("libdl")]
		private static extern IntPtr dlopen(string fileName, int flags);

		[DllImport("libdl")]
		private static extern IntPtr dlsym(IntPtr handle, string name);

		[DllImport("libdl")]
		private static extern int dlclose(IntPtr handle);

		[DllImport("libdl")]
		private static extern string dlerror();

		public override IntPtr LoadLibrary(string name)
		{
			return dlopen(name, 2);
		}

		public override IntPtr GetProcessAddress(IntPtr module, string name)
		{
			return dlsym(module, name);
		}

		public override int FreeLibrary(IntPtr module)
		{
			return dlclose(module);
		}
	}

	private static readonly NativeLoader _loader;

	static Native()
	{
		if (PlatformHelper.IsWindows)
		{
			_loader = new WinNativeLoader();
		}
		else
		{
			_loader = new UnixNativeLoader();
		}
	}

	public static IntPtr LoadLibrary(string name)
	{
		return _loader.LoadLibrary(name);
	}

	public static IntPtr GetProcessAddress(IntPtr module, string name)
	{
		return _loader.GetProcessAddress(module, name);
	}

	public static int FreeLibrary(IntPtr module)
	{
		return _loader.FreeLibrary(module);
	}
}
