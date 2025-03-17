using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class ChatGumpChooseName : Gump
{
	private readonly StbTextBox _textBox;

	public ChatGumpChooseName()
		: base(0u, 0u)
	{
		CanMove = false;
		AcceptKeyboardInput = true;
		AcceptMouseInput = true;
		base.WantUpdateSize = true;
		base.X = 250;
		base.Y = 100;
		base.Width = 210;
		base.Height = 330;
		AlphaBlendControl obj = new AlphaBlendControl
		{
			Alpha = 1f
		};
		obj.Width = base.Width;
		obj.Height = base.Height;
		Add(obj);
		Add(new BorderControl(0, 0, base.Width, base.Height, 4));
		Label label = new Label(ResGumps.ChooseName, isunicode: true, 23, base.Width - 17, 3);
		label.X = 6;
		label.Y = 6;
		Label label2 = label;
		Add(label2);
		int num = 4;
		BorderControl borderControl = new BorderControl(0, label2.Y + label2.Height, base.Width, 27, num);
		Add(borderControl);
		Label label3 = new Label(ResGumps.Name, isunicode: true, 51, 0, 3);
		label3.X = 6;
		label3.Y = borderControl.Y + 2;
		label2 = label3;
		Add(label2);
		int num2 = label2.X + label2.Width + 2;
		StbTextBox stbTextBox = new StbTextBox(1, -1, base.Width - num2 - 17, isunicode: true, FontStyle.Fixed, 1153);
		stbTextBox.X = num2;
		stbTextBox.Y = label2.Y;
		stbTextBox.Width = base.Width - -num2 - 17;
		stbTextBox.Height = 27 - num * 2;
		_textBox = stbTextBox;
		Add(_textBox);
		Add(new BorderControl(0, label2.Y + label2.Height, base.Width, 27, num));
		Button button = new Button(0, 2708, 2709, 2708, "", 0);
		button.X = base.Width - 19 - num;
		button.Y = base.Height - 19 - num;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 2714, 2715, 2714, "", 0);
		button2.X = base.Width - 38 - num;
		button2.Y = base.Height - 19 - num;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID != 0 && buttonID == 1 && !string.IsNullOrWhiteSpace(_textBox.Text))
		{
			NetClient.Socket.Send_OpenChat(_textBox.Text);
		}
		Dispose();
	}
}
