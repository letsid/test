using System.Runtime.InteropServices;
using System.Text;
using SDL2;

namespace ClassicUO.Utility;

public static class SDLHelper
{
	[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_RegisterApp")]
	private unsafe static extern int SDLRegisterApp([MarshalAs(UnmanagedType.LPArray)] byte[] name, uint flags, void* hInst);

	public unsafe static void SetUltimaOnlineWindowClass()
	{
		SDLRegisterApp(Encoding.UTF8.GetBytes("Ultima Online\0"), 4096u, null);
	}

	public static void SetNoCloseOnAltF4Hint(bool mode)
	{
		string value = (mode ? "1" : "0");
		SDL.SDL_SetHint("SDL_WINDOWS_NO_CLOSE_ON_ALT_F4", value);
	}
}
