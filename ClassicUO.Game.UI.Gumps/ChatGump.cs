using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class ChatGump : Gump
{
	private class ChannelCreationBox : Control
	{
		private readonly StbTextBox _textBox;

		public ChannelCreationBox(int x, int y)
		{
			CanMove = true;
			AcceptMouseInput = true;
			AcceptKeyboardInput = false;
			base.Width = 200;
			base.Height = 60;
			base.X = x - base.Width / 2;
			base.Y = y - base.Height / 2;
			AlphaBlendControl alphaBlendControl = new AlphaBlendControl(1f);
			alphaBlendControl.Width = base.Width;
			alphaBlendControl.Height = base.Height;
			Add(alphaBlendControl);
			Add(new BorderControl(0, 0, base.Width, 25, 3));
			Label label = new Label(ResGumps.CreateAChannel, isunicode: true, 35, base.Width - 4, 1);
			label.X = 6;
			label.Y = 3;
			Label c = label;
			Add(c);
			Add(new BorderControl(0, 22, base.Width, 25, 3));
			Label label2 = new Label(ResGumps.Name, isunicode: true, 35, base.Width - 4, 1);
			label2.X = 6;
			label2.Y = 25;
			c = label2;
			Add(c);
			StbTextBox stbTextBox = new StbTextBox(1, -1, base.Width - 50, isunicode: true, FontStyle.Fixed, 1153);
			stbTextBox.X = 45;
			stbTextBox.Y = 25;
			stbTextBox.Width = base.Width - 50;
			stbTextBox.Height = 19;
			_textBox = stbTextBox;
			Add(_textBox);
			Add(new BorderControl(0, 44, base.Width, 25, 3));
			Button button = new Button(0, 2708, 2709, 2708, "", 0);
			button.X = base.Width - 19 - 3;
			button.Y = base.Height - 19 + 6;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Button button2 = new Button(1, 2714, 2715, 2714, "", 0);
			button2.X = base.Width - 38 - 3;
			button2.Y = base.Height - 19 + 6;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
		}

		public override void OnButtonClick(int buttonID)
		{
			if (buttonID != 0 && buttonID == 1)
			{
				NetClient.Socket.Send_ChatCreateChannelCommand(_textBox.Text);
			}
			Dispose();
		}
	}

	private class ChannelListItemControl : Control
	{
		private bool _isSelected;

		private readonly Label _label;

		public readonly string Text;

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					_label.Hue = (ushort)(value ? 34u : 73u);
				}
			}
		}

		public ChannelListItemControl(string text, int width)
		{
			Text = text;
			base.Width = width;
			Label label = new Label(text, isunicode: false, 73, base.Width, 3);
			label.X = 3;
			Label c = label;
			_label = label;
			Add(c);
			base.Height = _label.Height;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			base.OnMouseUp(x, y, button);
			if (base.RootParent is ChatGump chatGump)
			{
				chatGump.OnChannelSelected(Text);
			}
		}

		protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
		{
			base.OnButtonClick(0);
			return true;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			if (base.MouseIsOver)
			{
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Cyan), new Rectangle(x, y, base.Width, base.Height), hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private ChannelCreationBox _channelCreationBox;

	private readonly List<ChannelListItemControl> _channelList = new List<ChannelListItemControl>();

	private readonly Label _currentChannelLabel;

	private readonly DataBox _databox;

	private string _selectedChannelText;

	public ChatGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		base.WantUpdateSize = false;
		base.Width = 345;
		base.Height = 390;
		ResizePic resizePic = new ResizePic(2600);
		resizePic.Width = base.Width;
		resizePic.Height = base.Height;
		Add(resizePic);
		int num = 25;
		Label label = new Label(ResGumps.Channels, isunicode: false, 902, 345, 2, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label.Y = num;
		Label c = label;
		Add(c);
		num += 40;
		Add(new BorderControl(61, num - 3, 228, 206, 3));
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(1f);
		alphaBlendControl.X = 64;
		alphaBlendControl.Y = num;
		alphaBlendControl.Width = 220;
		alphaBlendControl.Height = 200;
		Add(alphaBlendControl);
		ScrollArea scrollArea = new ScrollArea(64, num, 220, 200, normalScrollbar: true)
		{
			ScrollbarBehaviour = ScrollbarBehaviour.ShowAlways
		};
		Add(scrollArea);
		_databox = new DataBox(0, 0, 1, 1);
		_databox.WantUpdateSize = true;
		scrollArea.Add(_databox);
		foreach (KeyValuePair<string, ChatChannel> channel in ChatManager.Channels)
		{
			ChannelListItemControl channelListItemControl = new ChannelListItemControl(channel.Key, 195);
			_databox.Add(channelListItemControl);
			_channelList.Add(channelListItemControl);
		}
		_databox.ReArrangeChildren();
		num = 275;
		Label label2 = new Label(ResGumps.YourCurrentChannel, isunicode: false, 902, 345, 2, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label2.Y = num;
		c = label2;
		Add(c);
		num += 25;
		Label label3 = new Label(ChatManager.CurrentChannelName, isunicode: false, 902, 345, 2, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label3.Y = num;
		_currentChannelLabel = label3;
		Add(_currentChannelLabel);
		num = 337;
		Button button = new Button(0, 2117, 2118, 2117, "", 0);
		button.X = 48;
		button.Y = num + 5;
		button.ButtonAction = ButtonAction.Activate;
		Button c2 = button;
		Add(c2);
		Button button2 = new Button(1, 2117, 2118, 2117, "", 0);
		button2.X = 123;
		button2.Y = num + 5;
		button2.ButtonAction = ButtonAction.Activate;
		c2 = button2;
		Add(c2);
		Button button3 = new Button(2, 2117, 2118, 2117, "", 0);
		button3.X = 216;
		button3.Y = num + 5;
		button3.ButtonAction = ButtonAction.Activate;
		c2 = button3;
		Add(c2);
		Label label4 = new Label(ResGumps.Join, isunicode: false, 902, 0, 2);
		label4.X = 65;
		label4.Y = num;
		c = label4;
		Add(c);
		Label label5 = new Label(ResGumps.Leave, isunicode: false, 902, 0, 2);
		label5.X = 140;
		label5.Y = num;
		c = label5;
		Add(c);
		Label label6 = new Label(ResGumps.Create, isunicode: false, 902, 0, 2);
		label6.X = 233;
		label6.Y = num;
		c = label6;
		Add(c);
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			if (!string.IsNullOrEmpty(_selectedChannelText))
			{
				NetClient.Socket.Send_ChatJoinCommand(_selectedChannelText);
			}
			break;
		case 1:
			NetClient.Socket.Send_ChatLeaveChannelCommand();
			break;
		case 2:
			if (_channelCreationBox == null || _channelCreationBox.IsDisposed)
			{
				_channelCreationBox = new ChannelCreationBox(base.Width / 2, base.Height / 2);
				Add(_channelCreationBox);
			}
			break;
		}
	}

	public void UpdateConference()
	{
		if (_currentChannelLabel.Text != ChatManager.CurrentChannelName)
		{
			_currentChannelLabel.Text = ChatManager.CurrentChannelName;
		}
	}

	protected override void UpdateContents()
	{
		foreach (ChannelListItemControl channel in _channelList)
		{
			channel.Dispose();
		}
		_channelList.Clear();
		foreach (KeyValuePair<string, ChatChannel> channel2 in ChatManager.Channels)
		{
			ChannelListItemControl channelListItemControl = new ChannelListItemControl(channel2.Key, 195);
			_databox.Add(channelListItemControl);
			_channelList.Add(channelListItemControl);
		}
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
	}

	private void OnChannelSelected(string text)
	{
		_selectedChannelText = text;
		foreach (ChannelListItemControl channel in _channelList)
		{
			channel.IsSelected = channel.Text == text;
		}
	}
}
