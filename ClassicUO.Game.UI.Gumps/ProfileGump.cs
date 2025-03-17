using System;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class ProfileGump : Gump
{
	private const int _diffY = 22;

	private readonly DataBox _databox;

	private readonly GumpPic _gumpPic;

	private readonly HitBox _hitBox;

	private bool _isMinimized;

	private readonly string _originalText;

	private readonly ScrollArea _scrollArea;

	private readonly StbTextBox _textBox;

	public bool IsMinimized
	{
		get
		{
			return _isMinimized;
		}
		set
		{
			if (_isMinimized == value)
			{
				return;
			}
			_isMinimized = value;
			_gumpPic.Graphic = (ushort)(value ? 2516 : 2093);
			if (value)
			{
				_gumpPic.X = 0;
			}
			else
			{
				_gumpPic.X = 143;
			}
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			_gumpPic.IsVisible = true;
			base.WantUpdateSize = true;
		}
	}

	public ProfileGump(uint serial, string header, string footer, string body, bool canEdit)
		: base(serial, 0u)
	{
		base.Height = 322;
		CanMove = true;
		AcceptKeyboardInput = true;
		base.CanCloseWithRightClick = true;
		Add(_gumpPic = new GumpPic(143, 0, 2093, 0));
		_gumpPic.MouseDoubleClick += _picBase_MouseDoubleClick;
		Add(new ExpandableScroll(0, 22, base.Height - 22, 2080));
		_scrollArea = new ScrollArea(22, 54, 250, base.Height - 118, normalScrollbar: false);
		Label label = new Label(header, isunicode: true, 0, 140, 1);
		label.X = 53;
		label.Y = 6;
		Label label2 = label;
		_scrollArea.Add(label2);
		int num = label2.Height - 15;
		_scrollArea.Add(new GumpPic(4, num, 92, 0));
		_scrollArea.Add(new GumpPicTiled(56, num, 138, 0, 93));
		_scrollArea.Add(new GumpPic(194, num, 94, 0));
		num += 44;
		StbTextBox stbTextBox = new StbTextBox(1, -1, 220, isunicode: true, FontStyle.None, 0);
		stbTextBox.Width = 220;
		stbTextBox.X = 4;
		stbTextBox.Y = num;
		stbTextBox.IsEditable = canEdit;
		stbTextBox.Multiline = true;
		_textBox = stbTextBox;
		_originalText = body;
		_textBox.TextChanged += _textBox_TextChanged;
		_textBox.SetText(body);
		_scrollArea.Add(_textBox);
		_databox = new DataBox(4, _textBox.Height + 3, 1, 1);
		_databox.WantUpdateSize = true;
		_databox.Add(new GumpPic(4, 0, 95, 0));
		_databox.Add(new GumpPicTiled(13, 9, 197, 0, 96));
		_databox.Add(new GumpPic(210, 0, 97, 0));
		DataBox databox = _databox;
		Label label3 = new Label(footer, isunicode: true, 0, 220, 1);
		label3.X = 2;
		label3.Y = 26;
		databox.Add(label3);
		Add(_scrollArea);
		_scrollArea.Add(_databox);
		Add(_hitBox = new HitBox(143, 0, 23, 24));
		_hitBox.MouseUp += _hitBox_MouseUp;
	}

	public override void Update(double totalTime, double frameTime)
	{
		_scrollArea.Height = base.Height - 118;
		_databox.Y = _textBox.Bounds.Bottom + 3;
		_databox.WantUpdateSize = true;
		base.Update(totalTime, frameTime);
	}

	private void _textBox_TextChanged(object sender, EventArgs e)
	{
		_textBox.Height = Math.Max(FontsLoader.Instance.GetHeightUnicode(1, _textBox.Text, 220, TEXT_ALIGN_TYPE.TS_LEFT, 0) + 5, 20);
	}

	private void _hitBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && !IsMinimized)
		{
			IsMinimized = true;
		}
	}

	private void _picBase_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && IsMinimized)
		{
			IsMinimized = false;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
	}

	public override void Dispose()
	{
		if (_originalText != _textBox.Text && World.Player != null && !World.Player.IsDestroyed && !NetClient.Socket.IsDisposed && NetClient.Socket.IsConnected)
		{
			NetClient.Socket.Send_ProfileUpdate(base.LocalSerial, _textBox.Text);
		}
		base.Dispose();
	}
}
