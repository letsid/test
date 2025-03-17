using System;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal class MessageBoxGump : Gump
{
	private readonly Action<bool> _action;

	public MessageBoxGump(int w, int h, string message, Action<bool> action, bool hasBackground = false, MessageButtonType menuType = MessageButtonType.OK, ushort fontColor = 902)
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = false;
		base.CanCloseWithEsc = false;
		AcceptMouseInput = true;
		AcceptKeyboardInput = true;
		base.IsModal = true;
		base.LayerOrder = UILayer.Over;
		base.WantUpdateSize = false;
		base.Width = w;
		base.Height = h;
		_action = action;
		ResizePic resizePic = new ResizePic(2600);
		resizePic.Width = w;
		resizePic.Height = h;
		Add(resizePic);
		if (hasBackground)
		{
			ResizePic resizePic2 = new ResizePic(3000);
			resizePic2.X = base.X + 30;
			resizePic2.Y = base.Y + 40;
			resizePic2.Width = base.Width - 60;
			resizePic2.Height = base.Height - 100;
			ResizePic c = resizePic2;
			Add(c);
		}
		Label label = new Label(message, isunicode: true, fontColor, base.Width - 90, 1);
		label.X = 40;
		label.Y = 45;
		Add(label);
		base.X = Client.Game.Window.ClientBounds.Width - base.Width >> 1;
		base.Y = Client.Game.Window.ClientBounds.Height - base.Height >> 1;
		Button button = new Button(0, 1153, 1155, 1154, "", 0);
		button.Y = base.Height - 45;
		button.ButtonAction = ButtonAction.Activate;
		Button button2 = button;
		Add(button);
		button2.X = base.Width - button2.Width >> 1;
		if (menuType == MessageButtonType.OK_CANCEL)
		{
			Button button3 = new Button(1, 1150, 1151, 1152, "", 0);
			button3.Y = base.Height - 45;
			button3.ButtonAction = ButtonAction.Activate;
			Button button4 = button3;
			Add(button3);
			button2.X = base.Width / 2 - button4.Width;
			button4.X = base.Width / 2 - button4.Width;
			button4.X += button2.Width + 5;
		}
		base.WantUpdateSize = false;
		UIManager.KeyboardFocusControl = this;
		UIManager.KeyboardFocusControl.SetKeyboardFocus();
	}

	protected override void OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		base.OnKeyUp(key, mod);
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			_action?.Invoke(obj: true);
			Dispose();
			break;
		case 1:
			_action?.Invoke(obj: false);
			Dispose();
			break;
		}
	}
}
