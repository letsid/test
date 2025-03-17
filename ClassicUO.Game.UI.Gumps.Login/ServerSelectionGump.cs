using System.Linq;
using System.Net.NetworkInformation;
using ClassicUO.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;
using SDL2;

namespace ClassicUO.Game.UI.Gumps.Login;

internal class ServerSelectionGump : Gump
{
	private enum Buttons
	{
		Prev = 0,
		Next = 1,
		SortTimeZone = 2,
		SortFull = 3,
		SortConnection = 4,
		Earth = 5,
		Server = 99
	}

	private class ServerEntryGump : Control
	{
		private readonly int _buttonId;

		private readonly ServerListEntry _entry;

		private readonly HoveredLabel _server_packet_loss;

		private readonly HoveredLabel _server_ping;

		private readonly HoveredLabel _serverName;

		private uint _pingCheckTime;

		public ServerEntryGump(ServerListEntry entry, byte font, ushort normal_hue, ushort selected_hue)
		{
			_entry = entry;
			_buttonId = entry.Index;
			HoveredLabel hoveredLabel = new HoveredLabel(entry.Name, isunicode: false, normal_hue, selected_hue, selected_hue, 0, font);
			hoveredLabel.X = 74;
			hoveredLabel.AcceptMouseInput = false;
			HoveredLabel c = hoveredLabel;
			_serverName = hoveredLabel;
			Add(c);
			HoveredLabel hoveredLabel2 = new HoveredLabel(CUOEnviroment.NoServerPing ? string.Empty : "-", isunicode: false, normal_hue, selected_hue, selected_hue, 0, font);
			hoveredLabel2.X = 250;
			hoveredLabel2.AcceptMouseInput = false;
			c = hoveredLabel2;
			_server_ping = hoveredLabel2;
			Add(c);
			HoveredLabel hoveredLabel3 = new HoveredLabel(CUOEnviroment.NoServerPing ? string.Empty : "-", isunicode: false, normal_hue, selected_hue, selected_hue, 0, font);
			hoveredLabel3.X = 320;
			hoveredLabel3.AcceptMouseInput = false;
			c = hoveredLabel3;
			_server_packet_loss = hoveredLabel3;
			Add(c);
			AcceptMouseInput = true;
			base.Width = 370;
			base.Height = 25;
			base.WantUpdateSize = false;
		}

		protected override void OnMouseEnter(int x, int y)
		{
			base.OnMouseEnter(x, y);
			_serverName.IsSelected = true;
			_server_packet_loss.IsSelected = true;
			_server_ping.IsSelected = true;
		}

		protected override void OnMouseExit(int x, int y)
		{
			base.OnMouseExit(x, y);
			_serverName.IsSelected = false;
			_server_packet_loss.IsSelected = false;
			_server_ping.IsSelected = false;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				OnButtonClick(99 + _buttonId);
			}
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (!CUOEnviroment.NoServerPing && _pingCheckTime < Time.Ticks)
			{
				_pingCheckTime = Time.Ticks + 2000;
				_entry.DoPing();
				switch (_entry.PingStatus)
				{
				case IPStatus.Success:
					_server_ping.Text = ((_entry.Ping == -1) ? "-" : _entry.Ping.ToString());
					break;
				case IPStatus.DestinationNetworkUnreachable:
				case IPStatus.DestinationHostUnreachable:
				case IPStatus.DestinationProtocolUnreachable:
				case IPStatus.DestinationPortUnreachable:
				case IPStatus.DestinationUnreachable:
					_server_ping.Text = "unreach.";
					break;
				case IPStatus.TimedOut:
					_server_ping.Text = "time out";
					break;
				default:
					_server_ping.Text = $"unk. [{(int)_entry.PingStatus}]";
					break;
				}
				_server_packet_loss.Text = $"{_entry.PacketLoss}%";
			}
		}
	}

	private const ushort SELECTED_COLOR = 33;

	private const ushort NORMAL_COLOR = 847;

	public ServerSelectionGump()
		: base(0u, 0u)
	{
		Button button = new Button(0, 5537, 5539, 5538, "", 0);
		button.X = 586;
		button.Y = 445;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 5540, 5542, 5541, "", 0);
		button2.X = 610;
		button2.Y = 445;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		if (Client.Version >= ClientVersion.CV_500A)
		{
			ushort hue = ushort.MaxValue;
			Label label = new Label(ClilocLoader.Instance.GetString(1044579), isunicode: true, hue, 0, 1);
			label.X = 155;
			label.Y = 70;
			Add(label);
			if (!CUOEnviroment.NoServerPing)
			{
				Label label2 = new Label(ClilocLoader.Instance.GetString(1044577), isunicode: true, hue, 0, 1);
				label2.X = 400;
				label2.Y = 70;
				Add(label2);
				Label label3 = new Label(ClilocLoader.Instance.GetString(1044578), isunicode: true, hue, 0, 1);
				label3.X = 470;
				label3.Y = 70;
				Add(label3);
			}
			Label label4 = new Label(ClilocLoader.Instance.GetString(1044580), isunicode: true, hue, 0, 1);
			label4.X = 153;
			label4.Y = 368;
			Add(label4);
		}
		else
		{
			ushort hue2 = 1153;
			Label label5 = new Label(ResGumps.SelectWhichShardToPlayOn, isunicode: false, hue2, 0, 9);
			label5.X = 155;
			label5.Y = 70;
			Add(label5);
			Label label6 = new Label(ResGumps.Latency, isunicode: false, hue2, 0, 9);
			label6.X = 400;
			label6.Y = 70;
			Add(label6);
			Label label7 = new Label(ResGumps.PacketLoss, isunicode: false, hue2, 0, 9);
			label7.X = 470;
			label7.Y = 70;
			Add(label7);
			Label label8 = new Label(ResGumps.SortBy, isunicode: false, hue2, 0, 9);
			label8.X = 153;
			label8.Y = 368;
			Add(label8);
		}
		Button button3 = new Button(2, 2363, 2364, 2365, "", 0);
		button3.X = 230;
		button3.Y = 366;
		Add(button3);
		Button button4 = new Button(3, 2366, 2367, 2368, "", 0);
		button4.X = 338;
		button4.Y = 366;
		Add(button4);
		Button button5 = new Button(4, 2369, 2370, 2371, "", 0);
		button5.X = 446;
		button5.Y = 366;
		Add(button5);
		Add(new GumpPic(150, 390, 1417, 0));
		Button button6 = new Button(5, 5608, 5610, 5609, "", 0);
		button6.X = 160;
		button6.Y = 400;
		button6.ButtonAction = ButtonAction.Activate;
		Add(button6);
		ResizePic resizePic = new ResizePic(3500);
		resizePic.X = 150;
		resizePic.Y = 90;
		resizePic.Width = 379;
		resizePic.Height = 271;
		Add(resizePic);
		ScrollArea scrollArea = new ScrollArea(150, 90, 393, 271, normalScrollbar: true);
		DataBox dataBox = new DataBox(0, 0, 1, 1)
		{
			WantUpdateSize = true
		};
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		scrollArea.ScissorRectangle.Y = 16;
		scrollArea.ScissorRectangle.Height = -32;
		ServerListEntry[] servers = scene.Servers;
		foreach (ServerListEntry entry in servers)
		{
			dataBox.Add(new ServerEntryGump(entry, 5, 847, 33));
		}
		dataBox.ReArrangeChildren();
		Add(scrollArea);
		scrollArea.Add(dataBox);
		if (scene.Servers.Length != 0)
		{
			int serverIndexFromSettings = scene.GetServerIndexFromSettings();
			Label label9 = new Label(scene.Servers[serverIndexFromSettings].Name, isunicode: false, 1153, 0, 9);
			label9.X = 243;
			label9.Y = 420;
			Add(label9);
		}
		AcceptKeyboardInput = true;
		base.CanCloseWithRightClick = false;
	}

	public override void OnButtonClick(int buttonID)
	{
		LoginScene scene = Client.Game.GetScene<LoginScene>();
		if (buttonID >= 99)
		{
			int num = buttonID - 99;
			scene.SelectServer((byte)num);
			return;
		}
		switch ((Buttons)buttonID)
		{
		case Buttons.Next:
		case Buttons.Earth:
			if (scene.Servers.Length != 0)
			{
				int serverIndexFromSettings = scene.GetServerIndexFromSettings();
				scene.SelectServer((byte)scene.Servers[serverIndexFromSettings].Index);
			}
			break;
		case Buttons.Prev:
			scene.StepBack();
			break;
		}
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		if (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER)
		{
			LoginScene scene = Client.Game.GetScene<LoginScene>();
			if (scene.Servers.Any())
			{
				int serverIndexFromSettings = scene.GetServerIndexFromSettings();
				scene.SelectServer((byte)scene.Servers[serverIndexFromSettings].Index);
			}
		}
	}
}
