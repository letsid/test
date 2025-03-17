using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class TextEntryDialogGump : Gump
{
	private enum ButtonType
	{
		Ok,
		Cancel
	}

	private readonly StbTextBox _textBox;

	public byte ParentID { get; }

	public byte ButtonID { get; }

	public TextEntryDialogGump(uint serial, int x, int y, byte variant, int maxlen, string text, string description, byte buttonid, byte parentid)
		: base(serial, 0u)
	{
		CanMove = false;
		base.IsFromServer = true;
		base.IsModal = true;
		base.X = x;
		base.Y = y;
		GumpPic gumpPic = new GumpPic(0, 0, 1140, 0);
		Add(gumpPic);
		Label label = new Label(text, isunicode: false, 902, gumpPic.Width - 110, 2);
		label.X = 60;
		label.Y = 50;
		Label c = label;
		Add(c);
		Label label2 = new Label(description, isunicode: false, 902, gumpPic.Width - 110, 2);
		label2.X = 60;
		label2.Y = 108;
		c = label2;
		Add(c);
		Add(new GumpPic(60, 130, 1143, 0));
		StbTextBox stbTextBox = new StbTextBox(1, maxlen, 0, isunicode: false, FontStyle.None, 902);
		stbTextBox.X = 71;
		stbTextBox.Y = 137;
		stbTextBox.Width = 250;
		stbTextBox.NumbersOnly = variant == 2;
		_textBox = stbTextBox;
		Add(_textBox);
		Button button = new Button(0, 1147, 1148, 1149, "", 0);
		button.X = 117;
		button.Y = 190;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 1144, 1144, 1146, "", 0);
		button2.X = 204;
		button2.Y = 190;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		ButtonID = buttonid;
		ParentID = parentid;
		UIManager.KeyboardFocusControl = _textBox;
		_textBox.SetKeyboardFocus();
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((ButtonType)buttonID)
		{
		case ButtonType.Ok:
			NetClient.Socket.Send_TextEntryDialogResponse(base.LocalSerial, ParentID, ButtonID, _textBox.Text, code: true);
			Dispose();
			break;
		case ButtonType.Cancel:
			NetClient.Socket.Send_TextEntryDialogResponse(base.LocalSerial, ParentID, ButtonID, _textBox.Text, code: false);
			Dispose();
			break;
		}
	}
}
