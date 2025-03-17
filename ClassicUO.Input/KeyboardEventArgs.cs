using System;
using SDL2;

namespace ClassicUO.Input;

internal sealed class KeyboardEventArgs : EventArgs
{
	public SDL.SDL_Keycode Key { get; }

	public SDL.SDL_Keymod Mod { get; }

	public KeyboardEventType KeyboardEvent { get; }

	public KeyboardEventArgs(SDL.SDL_Keycode key, SDL.SDL_Keymod mod, KeyboardEventType state)
	{
		Key = key;
		Mod = mod;
		KeyboardEvent = state;
	}
}
