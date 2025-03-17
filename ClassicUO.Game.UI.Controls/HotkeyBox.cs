using System;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using SDL2;

namespace ClassicUO.Game.UI.Controls;

internal class HotkeyBox : Control
{
	private enum ButtonState
	{
		Ok,
		Cancel
	}

	private bool _actived;

	private readonly Button _buttonOK;

	private readonly Button _buttonCancel;

	private readonly HoveredLabel _label;

	public SDL.SDL_Keycode Key { get; private set; }

	public SDL.SDL_Keymod Mod { get; private set; }

	public bool IsActive
	{
		get
		{
			return _actived;
		}
		set
		{
			_actived = value;
			if (value)
			{
				Button buttonOK = _buttonOK;
				bool isVisible = (_buttonCancel.IsVisible = true);
				buttonOK.IsVisible = isVisible;
				Button buttonOK2 = _buttonOK;
				isVisible = (_buttonCancel.IsEnabled = true);
				buttonOK2.IsEnabled = isVisible;
			}
			else
			{
				Button buttonOK3 = _buttonOK;
				bool isVisible = (_buttonCancel.IsVisible = false);
				buttonOK3.IsVisible = isVisible;
				Button buttonOK4 = _buttonOK;
				isVisible = (_buttonCancel.IsEnabled = false);
				buttonOK4.IsEnabled = isVisible;
			}
		}
	}

	public event EventHandler HotkeyChanged;

	public event EventHandler HotkeyCancelled;

	public HotkeyBox()
	{
		CanMove = false;
		AcceptMouseInput = true;
		AcceptKeyboardInput = true;
		base.Width = 210;
		base.Height = 20;
		ResizePic resizePic = new ResizePic(3000);
		resizePic.Width = 150;
		resizePic.Height = base.Height;
		resizePic.AcceptKeyboardInput = true;
		ResizePic resizePic2 = resizePic;
		Add(resizePic);
		resizePic2.MouseUp += LabelOnMouseUp;
		HoveredLabel hoveredLabel = new HoveredLabel(string.Empty, isunicode: false, 1, 33, 33, 150, 1, FontStyle.Italic, TEXT_ALIGN_TYPE.TS_CENTER);
		hoveredLabel.Y = 5;
		HoveredLabel c = hoveredLabel;
		_label = hoveredLabel;
		Add(c);
		_label.MouseUp += LabelOnMouseUp;
		Button button = new Button(0, 1153, 1155, 1154, "", 0);
		button.X = 152;
		button.ButtonAction = ButtonAction.Activate;
		Button c2 = button;
		_buttonOK = button;
		Add(c2);
		Button button2 = new Button(1, 1150, 1152, 1151, "", 0);
		button2.X = 182;
		button2.ButtonAction = ButtonAction.Activate;
		c2 = button2;
		_buttonCancel = button2;
		Add(c2);
		base.WantUpdateSize = false;
		IsActive = false;
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		if (IsActive)
		{
			SetKey(key, mod);
		}
	}

	public void SetKey(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		if (key == SDL.SDL_Keycode.SDLK_UNKNOWN && mod == SDL.SDL_Keymod.KMOD_NONE)
		{
			Key = key;
			Mod = mod;
			_label.Text = string.Empty;
			return;
		}
		string text = KeysTranslator.TryGetKey(key, mod);
		if (!string.IsNullOrEmpty(text) && key != 0)
		{
			Key = key;
			Mod = mod;
			_label.Text = text;
		}
	}

	private void LabelOnMouseUp(object sender, MouseEventArgs e)
	{
		IsActive = true;
		SetKeyboardFocus();
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((ButtonState)buttonID)
		{
		case ButtonState.Ok:
			this.HotkeyChanged.Raise(this);
			break;
		case ButtonState.Cancel:
			_label.Text = string.Empty;
			this.HotkeyCancelled.Raise(this);
			Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
			Mod = SDL.SDL_Keymod.KMOD_NONE;
			break;
		}
		IsActive = false;
	}
}
