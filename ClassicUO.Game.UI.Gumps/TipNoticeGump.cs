using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class TipNoticeGump : Gump
{
	private readonly ExpandableScroll _background;

	private readonly Button _prev;

	private readonly Button _next;

	private readonly ScrollArea _scrollArea;

	private readonly StbTextBox _textBox;

	public TipNoticeGump(uint serial, byte type, string text)
		: base(serial, 0u)
	{
		base.Height = 300;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		_scrollArea = new ScrollArea(0, 32, 272, base.Height - 96, normalScrollbar: false);
		StbTextBox stbTextBox = new StbTextBox(6, -1, 220, isunicode: false, FontStyle.None, 0);
		stbTextBox.Height = 20;
		stbTextBox.X = 35;
		stbTextBox.Y = 0;
		stbTextBox.Width = 220;
		stbTextBox.IsEditable = false;
		_textBox = stbTextBox;
		_textBox.SetText(text);
		Add(_background = new ExpandableScroll(0, 0, base.Height, 2080));
		_scrollArea.Add(_textBox);
		Add(_scrollArea);
		if (type == 0)
		{
			_background.TitleGumpID = 2506;
			Button button = new Button(1, 2508, 2508, 0, "", 0);
			button.X = 35;
			button.ContainsByBounds = true;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Button button2 = new Button(2, 2509, 2509, 0, "", 0);
			button2.X = 240;
			button2.ContainsByBounds = true;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
		}
		else
		{
			_background.TitleGumpID = 2514;
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		base.Height = _background.SpecialHeight;
		_scrollArea.Height = _background.Height - 96;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 1:
			NetClient.Socket.Send_TipRequest((ushort)base.LocalSerial, 0);
			Dispose();
			break;
		case 2:
			NetClient.Socket.Send_TipRequest((ushort)base.LocalSerial, 1);
			Dispose();
			break;
		}
	}
}
