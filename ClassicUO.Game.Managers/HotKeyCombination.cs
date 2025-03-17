using SDL2;

namespace ClassicUO.Game.Managers;

internal class HotKeyCombination
{
	public SDL.SDL_Keycode Key { get; set; }

	public SDL.SDL_Keymod Mod { get; set; }

	public HotkeyAction KeyAction { get; set; }
}
