using System;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class BulletinBoardItem : Gump
{
	private enum ButtonType
	{
		Post,
		Remove,
		Reply
	}

	private readonly ExpandableScroll _articleContainer;

	private readonly Button _buttonPost;

	private readonly Button _buttonRemove;

	private readonly Button _buttonReply;

	private readonly DataBox _databox;

	private readonly string _datatime;

	private readonly uint _msgSerial;

	private readonly StbTextBox _subjectTextbox;

	private readonly StbTextBox _textBox;

	public BulletinBoardItem(uint serial, uint msgSerial, string poster, string subject, string datatime, string data, byte variant)
		: base(serial, 0u)
	{
		_msgSerial = msgSerial;
		AcceptKeyboardInput = true;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		_datatime = datatime;
		_articleContainer = new ExpandableScroll(0, 0, 408, 2080)
		{
			TitleGumpID = 2080,
			AcceptMouseInput = true
		};
		Add(_articleContainer);
		ScrollArea scrollArea = new ScrollArea(0, 120, 272, 224, normalScrollbar: false);
		Add(scrollArea);
		_databox = new DataBox(0, 0, 1, 1);
		scrollArea.Add(_databox);
		bool flag = Client.Version >= ClientVersion.CV_305D;
		byte b = 1;
		int num = 0;
		ushort num2 = 902;
		if (flag)
		{
			num = -6;
			num2 = 0;
		}
		Label label = new Label(ResGumps.Author, flag, num2, 0, (byte)(flag ? b : 6));
		label.X = 30;
		label.Y = 40;
		Label label2 = label;
		Add(label2);
		Label label3 = new Label(poster, flag, num2, 0, (byte)(flag ? b : 9));
		label3.X = 30 + label2.Width;
		label3.Y = 46 + num;
		label2 = label3;
		Add(label2);
		Label label4 = new Label(ResGumps.Date, flag, num2, 0, (byte)(flag ? b : 6));
		label4.X = 30;
		label4.Y = 58;
		label2 = label4;
		Add(label2);
		Label label5 = new Label(datatime, flag, num2, 0, (byte)(flag ? b : 9));
		label5.X = 32 + label2.Width;
		label5.Y = 64 + num;
		label2 = label5;
		Add(label2);
		Label label6 = new Label(ResGumps.Title, flag, num2, 0, (byte)(flag ? b : 6));
		label6.X = 30;
		label6.Y = 77;
		label2 = label6;
		Add(label2);
		ushort hue = num2;
		if (variant == 0)
		{
			hue = 8;
		}
		StbTextBox stbTextBox = new StbTextBox((byte)(flag ? b : 9), -1, 150, flag, FontStyle.None, hue);
		stbTextBox.X = 30 + label2.Width;
		stbTextBox.Y = 83 + num;
		stbTextBox.Width = 150;
		stbTextBox.IsEditable = variant == 0;
		StbTextBox c = stbTextBox;
		_subjectTextbox = stbTextBox;
		Add(c);
		_subjectTextbox.SetText(subject);
		Add(new GumpPicTiled(30, 106, 235, 4, 2101));
		DataBox databox = _databox;
		int font = (flag ? b : 9);
		ushort hue2 = num2;
		StbTextBox stbTextBox2 = new StbTextBox((byte)font, -1, 220, flag, FontStyle.None, hue2);
		stbTextBox2.X = 40;
		stbTextBox2.Y = 0;
		stbTextBox2.Width = 220;
		stbTextBox2.Height = 300;
		stbTextBox2.IsEditable = variant == 0;
		stbTextBox2.Multiline = true;
		c = stbTextBox2;
		_textBox = stbTextBox2;
		databox.Add(c);
		_textBox.SetText(data);
		_textBox.TextChanged += _textBox_TextChanged;
		switch (variant)
		{
		case 0:
		{
			Add(new GumpPic(97, 12, 2179, 0));
			Button button3 = new Button(0, 2182, 2182, 0, "", 0);
			button3.X = 37;
			button3.Y = base.Height - 50;
			button3.ButtonAction = ButtonAction.Activate;
			button3.ContainsByBounds = true;
			Button c2 = button3;
			_buttonPost = button3;
			Add(c2);
			break;
		}
		case 1:
		{
			Button button2 = new Button(2, 2180, 2180, 0, "", 0);
			button2.X = 37;
			button2.Y = base.Height - 50;
			button2.ButtonAction = ButtonAction.Activate;
			button2.ContainsByBounds = true;
			Button c2 = button2;
			_buttonReply = button2;
			Add(c2);
			break;
		}
		case 2:
		{
			Button button = new Button(1, 2181, 2181, 0, "", 0);
			button.X = 235;
			button.Y = base.Height - 50;
			button.ButtonAction = ButtonAction.Activate;
			button.ContainsByBounds = true;
			Button c2 = button;
			_buttonRemove = button;
			Add(c2);
			break;
		}
		}
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
	}

	private void _textBox_TextChanged(object sender, EventArgs e)
	{
		_textBox.Height = Math.Max(FontsLoader.Instance.GetHeightUnicode(1, _textBox.Text, 220, TEXT_ALIGN_TYPE.TS_LEFT, 0) + 5, 20);
		foreach (Control child in _databox.Children)
		{
			if (child is BulletinBoardItem)
			{
				child.OnPageChanged();
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (_buttonPost != null)
		{
			_buttonPost.Y = base.Height - 50;
		}
		if (_buttonReply != null)
		{
			_buttonReply.Y = base.Height - 50;
		}
		if (_buttonRemove != null)
		{
			_buttonRemove.Y = base.Height - 50;
		}
		base.Update(totalTime, frameTime);
	}

	public override void OnButtonClick(int buttonID)
	{
		if (_subjectTextbox != null)
		{
			switch ((ButtonType)buttonID)
			{
			case ButtonType.Post:
				NetClient.Socket.Send_BulletinBoardPostMessage(base.LocalSerial, _msgSerial, _subjectTextbox.Text, _textBox.Text);
				Dispose();
				break;
			case ButtonType.Reply:
			{
				BulletinBoardItem bulletinBoardItem = new BulletinBoardItem(base.LocalSerial, _msgSerial, World.Player.Name, ResGumps.RE + _subjectTextbox.Text, _datatime, string.Empty, 0);
				bulletinBoardItem.X = 400;
				bulletinBoardItem.Y = 335;
				UIManager.Add(bulletinBoardItem);
				Dispose();
				break;
			}
			case ButtonType.Remove:
				NetClient.Socket.Send_BulletinBoardRemoveMessage(base.LocalSerial, _msgSerial);
				Dispose();
				break;
			}
		}
	}

	public override void OnPageChanged()
	{
		base.Height = _articleContainer.SpecialHeight;
		_databox.Parent.Height = (_databox.Height = _articleContainer.SpecialHeight - 184);
		foreach (Control child in _databox.Children)
		{
			if (child is BulletinBoardItem)
			{
				child.OnPageChanged();
			}
		}
	}
}
