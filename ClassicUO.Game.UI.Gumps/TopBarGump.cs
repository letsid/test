using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class TopBarGump : Gump
{
	private enum Buttons
	{
		Map,
		Paperdoll,
		Inventory,
		Journal,
		Chat,
		Help,
		WorldMap,
		Info,
		Debug,
		NetStats,
		UOStore,
		GlobalChat,
		AdditionalJournal
	}

	private class RighClickableButton : Button
	{
		public RighClickableButton(int buttonID, ushort normal, ushort pressed, ushort over = 0, string caption = "", byte font = 0, bool isunicode = true, ushort normalHue = ushort.MaxValue, ushort hoverHue = ushort.MaxValue)
			: base(buttonID, normal, pressed, over, caption, font, isunicode, normalHue, hoverHue)
		{
		}

		public RighClickableButton(List<string> parts)
			: base(parts)
		{
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			base.OnMouseUp(x, y, button);
			base.Parent?.InvokeMouseUp(new Point(x, y), button);
		}
	}

	public bool IsMinimized { get; private set; }

	private TopBarGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = false;
		ResizePic resizePic = new ResizePic(5054);
		resizePic.Width = 30;
		resizePic.Height = 27;
		Add(resizePic, 2);
		Button button = new Button(0, 5537, 5537, 5537, "", 0);
		button.X = 5;
		button.Y = 3;
		button.ToPage = 1;
		Add(button, 2);
		int num = 50;
		if (GumpsLoader.Instance.GetGumpTexture(2443u, out var bounds) != null)
		{
			num = bounds.Width;
		}
		int num2 = 100;
		if (GumpsLoader.Instance.GetGumpTexture(2445u, out bounds) != null)
		{
			num2 = bounds.Width;
		}
		int[][] array = new int[9][]
		{
			new int[2],
			new int[2] { 1, 1 },
			new int[2] { 1, 2 },
			new int[2] { 1, 3 },
			new int[2] { 1, 12 },
			new int[2] { 0, 5 },
			new int[2] { 1, 6 },
			new int[2] { 0, 8 },
			new int[2] { 1, 9 }
		};
		string[] array2 = new string[9]
		{
			ResGumps.Map,
			ResGumps.Paperdoll,
			ResGumps.Inventory,
			ResGumps.Journal,
			ResGumps.AdditionalJournal,
			ResGumps.Help,
			ResGumps.WorldMap,
			ResGumps.Debug,
			ResGumps.NetStats
		};
		bool flag = Client.Version >= ClientVersion.CV_706400;
		ResizePic resizePic2 = new ResizePic(5054);
		resizePic2.Height = 27;
		ResizePic resizePic3 = resizePic2;
		Add(resizePic2, 1);
		Button button2 = new Button(0, 5540, 5540, 5540, "", 0);
		button2.X = 5;
		button2.Y = 3;
		button2.ToPage = 2;
		Add(button2, 1);
		int num3 = 30;
		for (int i = 0; i < array.Length && (flag || i < 10); i++)
		{
			ushort num4 = (ushort)((array[i][0] != 0) ? 2445u : 2443u);
			RighClickableButton obj = new RighClickableButton(array[i][1], num4, num4, num4, array2[i], 1, isunicode: true, 0, 54)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj.X = num3;
			obj.Y = 1;
			obj.FontCenter = true;
			Add(obj, 1);
			num3 += ((array[i][0] != 0) ? num2 : num) + 1;
			resizePic3.Width = num3;
		}
		resizePic3.Width = num3 + 1;
		base.LayerOrder = UILayer.Over;
	}

	public static void Create()
	{
		TopBarGump gump = UIManager.GetGump<TopBarGump>(null);
		if (gump == null)
		{
			if (ProfileManager.CurrentProfile.TopbarGumpPosition.X < 0 || ProfileManager.CurrentProfile.TopbarGumpPosition.Y < 0)
			{
				ProfileManager.CurrentProfile.TopbarGumpPosition = Point.Zero;
			}
			TopBarGump topBarGump = new TopBarGump();
			topBarGump.X = ProfileManager.CurrentProfile.TopbarGumpPosition.X;
			topBarGump.Y = ProfileManager.CurrentProfile.TopbarGumpPosition.Y;
			gump = topBarGump;
			UIManager.Add(topBarGump);
			if (ProfileManager.CurrentProfile.TopbarGumpIsMinimized)
			{
				gump.ChangePage(2);
			}
		}
		else
		{
			Log.Error(ResGumps.TopBarGumpAlreadyExists);
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Right && (base.X != 0 || base.Y != 0))
		{
			base.X = 0;
			base.Y = 0;
			ProfileManager.CurrentProfile.TopbarGumpPosition = base.Location;
		}
	}

	public override void OnPageChanged()
	{
		Profile currentProfile = ProfileManager.CurrentProfile;
		bool topbarGumpIsMinimized = (IsMinimized = base.ActivePage == 2);
		currentProfile.TopbarGumpIsMinimized = topbarGumpIsMinimized;
		base.WantUpdateSize = true;
	}

	protected override void OnDragEnd(int x, int y)
	{
		base.OnDragEnd(x, y);
		ProfileManager.CurrentProfile.TopbarGumpPosition = base.Location;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((Buttons)buttonID)
		{
		case Buttons.Map:
			GameActions.OpenMiniMap();
			break;
		case Buttons.Paperdoll:
			GameActions.OpenPaperdoll(World.Player);
			break;
		case Buttons.Inventory:
			GameActions.OpenBackpack();
			break;
		case Buttons.Journal:
			GameActions.OpenJournal();
			break;
		case Buttons.AdditionalJournal:
			GameActions.OpenAdditionalJournal();
			break;
		case Buttons.Chat:
			GameActions.OpenChat();
			break;
		case Buttons.GlobalChat:
			Log.Warn(ResGumps.ChatButtonPushedNotImplementedYet);
			GameActions.Print(ResGumps.GlobalChatNotImplementedYet, 35, MessageType.System, 3);
			break;
		case Buttons.UOStore:
			if (Client.Version >= ClientVersion.CV_706400)
			{
				NetClient.Socket.Send_OpenUOStore();
			}
			break;
		case Buttons.Help:
			GameActions.RequestHelp();
			break;
		case Buttons.Debug:
		{
			DebugGump gump2 = UIManager.GetGump<DebugGump>(null);
			if (gump2 == null)
			{
				gump2 = new DebugGump(100, 100);
				UIManager.Add(gump2);
			}
			else
			{
				gump2.IsVisible = !gump2.IsVisible;
				gump2.SetInScreen();
			}
			break;
		}
		case Buttons.NetStats:
		{
			NetworkStatsGump gump = UIManager.GetGump<NetworkStatsGump>(null);
			if (gump == null)
			{
				gump = new NetworkStatsGump(100, 100);
				UIManager.Add(gump);
			}
			else
			{
				gump.IsVisible = !gump.IsVisible;
				gump.SetInScreen();
			}
			break;
		}
		case Buttons.WorldMap:
			GameActions.OpenWorldMap();
			break;
		case Buttons.Info:
			break;
		}
	}
}
