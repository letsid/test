using System;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login;

internal class LoadingGump : Gump
{
	private readonly Action<int> _buttonClick;

	private readonly Label _label;

	public LoadingGump(string labelText, LoginButtons showButtons, Action<int> buttonClick = null)
		: base(0u, 0u)
	{
		_buttonClick = buttonClick;
		base.CanCloseWithRightClick = false;
		base.CanCloseWithEsc = false;
		bool isunicode;
		bool num = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
		byte font = (byte)(num ? 1u : 2u);
		ushort hue = (ushort)(num ? 65535u : 902u);
		Label label = new Label(labelText, isunicode, hue, 326, font, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label.X = 162;
		label.Y = 178;
		_label = label;
		ResizePic resizePic = new ResizePic(2600);
		resizePic.X = 142;
		resizePic.Y = 134;
		resizePic.Width = 366;
		resizePic.Height = 212;
		Add(resizePic);
		Add(_label);
		switch (showButtons)
		{
		case LoginButtons.OK:
		{
			Button button4 = new Button(2, 1153, 1155, 1154, "", 0);
			button4.X = 306;
			button4.Y = 304;
			button4.ButtonAction = ButtonAction.Activate;
			Add(button4);
			break;
		}
		case LoginButtons.Cancel:
		{
			Button button3 = new Button(4, 1150, 1152, 1151, "", 0);
			button3.X = 306;
			button3.Y = 304;
			button3.ButtonAction = ButtonAction.Activate;
			Add(button3);
			break;
		}
		case LoginButtons.OK | LoginButtons.Cancel:
		{
			Button button = new Button(2, 1153, 1155, 1154, "", 0);
			button.X = 264;
			button.Y = 304;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Button button2 = new Button(4, 1150, 1152, 1151, "", 0);
			button2.X = 348;
			button2.Y = 304;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
			break;
		}
		}
	}

	public void SetText(string text)
	{
		_label.Text = text;
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		if (key == SDL.SDL_Keycode.SDLK_KP_ENTER || key == SDL.SDL_Keycode.SDLK_RETURN)
		{
			OnButtonClick(2);
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		_buttonClick?.Invoke(buttonID);
		base.OnButtonClick(buttonID);
	}
}
