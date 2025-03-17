using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class EntryDialog : Gump
{
	private readonly Action<string> _action;

	private readonly StbTextBox _textBox;

	public EntryDialog(int w, int h, string message, Action<string> action)
		: base(0u, 0u)
	{
		CanMove = false;
		base.CanCloseWithRightClick = false;
		base.CanCloseWithEsc = false;
		AcceptMouseInput = false;
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
		Label label = new Label(message, isunicode: false, 902, base.Width - 90, 1);
		label.X = 40;
		label.Y = 45;
		Label label2 = label;
		Add(label);
		ResizePic resizePic2 = new ResizePic(3000);
		resizePic2.X = 40;
		resizePic2.Y = 45 + label2.Height + 5;
		resizePic2.Width = w - 90;
		resizePic2.Height = 25;
		Add(resizePic2);
		int num = w - 94;
		StbTextBox stbTextBox = new StbTextBox(byte.MaxValue, -1, num, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 0);
		stbTextBox.X = 42;
		stbTextBox.Y = 45 + label2.Height + 7;
		stbTextBox.Width = num;
		stbTextBox.Height = 25;
		_textBox = stbTextBox;
		Add(_textBox);
		base.X = Client.Game.Window.ClientBounds.Width - base.Width >> 1;
		base.Y = Client.Game.Window.ClientBounds.Height - base.Height >> 1;
		Button button = new Button(0, 1153, 1154, 1155, "", 0);
		button.Y = base.Height - 45;
		button.ButtonAction = ButtonAction.Activate;
		Button button2 = button;
		Add(button);
		button2.X = base.Width - button2.Width >> 1;
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 0)
		{
			_action?.Invoke(_textBox.Text);
			Dispose();
		}
	}
}
