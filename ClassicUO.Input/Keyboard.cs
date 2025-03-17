using ClassicUO.Utility;
using SDL2;

namespace ClassicUO.Input;

internal static class Keyboard
{
	private static SDL.SDL_Keycode _code;

	public static SDL.SDL_Keymod IgnoreKeyMod { get; } = SDL.SDL_Keymod.KMOD_NUM | SDL.SDL_Keymod.KMOD_CAPS | SDL.SDL_Keymod.KMOD_MODE | SDL.SDL_Keymod.KMOD_SCROLL;

	public static bool Alt { get; private set; }

	public static bool Shift { get; private set; }

	public static bool Ctrl { get; private set; }

	public static void OnKeyUp(SDL.SDL_KeyboardEvent e)
	{
		if (((uint)e.keysym.mod & (uint)(ushort)(~(int)IgnoreKeyMod) & 0x240) == 576)
		{
			e.keysym.sym = SDL.SDL_Keycode.SDLK_UNKNOWN;
			e.keysym.mod = SDL.SDL_Keymod.KMOD_NONE;
		}
		Shift = (e.keysym.mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
		Alt = (e.keysym.mod & SDL.SDL_Keymod.KMOD_ALT) != 0;
		Ctrl = (e.keysym.mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
		_code = SDL.SDL_Keycode.SDLK_UNKNOWN;
	}

	public static void OnKeyDown(SDL.SDL_KeyboardEvent e)
	{
		AwayStateHelper.NotifyPresent();
		if (((uint)e.keysym.mod & (uint)(ushort)(~(int)IgnoreKeyMod) & 0x240) == 576)
		{
			e.keysym.sym = SDL.SDL_Keycode.SDLK_UNKNOWN;
			e.keysym.mod = SDL.SDL_Keymod.KMOD_NONE;
		}
		Shift = (e.keysym.mod & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
		Alt = (e.keysym.mod & SDL.SDL_Keymod.KMOD_ALT) != 0;
		Ctrl = (e.keysym.mod & SDL.SDL_Keymod.KMOD_CTRL) != 0;
		if (e.keysym.sym != 0)
		{
			_code = e.keysym.sym;
		}
	}
}
