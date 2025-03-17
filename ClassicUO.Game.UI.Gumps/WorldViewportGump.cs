using System;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class WorldViewportGump : Gump
{
	private const int BORDER_WIDTH = 5;

	private readonly BorderControl _borderControl;

	private readonly Button _button;

	private bool _clicked;

	private Point _lastSize;

	private Point _savedSize;

	private readonly GameScene _scene;

	private readonly SystemChatControl _systemChatControl;

	private int _worldHeight;

	private int _worldWidth;

	public WorldViewportGump(GameScene scene)
		: base(0u, 0u)
	{
		_scene = scene;
		AcceptMouseInput = false;
		CanMove = !ProfileManager.CurrentProfile.GameWindowLock;
		base.CanCloseWithEsc = false;
		base.CanCloseWithRightClick = false;
		base.LayerOrder = UILayer.Under;
		base.X = ProfileManager.CurrentProfile.GameWindowPosition.X;
		base.Y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
		_worldWidth = ProfileManager.CurrentProfile.GameWindowSize.X;
		_worldHeight = ProfileManager.CurrentProfile.GameWindowSize.Y;
		_savedSize = (_lastSize = ProfileManager.CurrentProfile.GameWindowSize);
		_button = new Button(0, 2103, 2104, 2104, "", 0);
		_button.MouseDown += delegate
		{
			if (!ProfileManager.CurrentProfile.GameWindowLock)
			{
				_clicked = true;
			}
		};
		_button.MouseUp += delegate
		{
			if (!ProfileManager.CurrentProfile.GameWindowLock)
			{
				Point point = ResizeGameWindow(_lastSize);
				UIManager.GetGump<OptionsGump>(null)?.UpdateVideo();
				if (Client.Version >= ClientVersion.CV_200)
				{
					NetClient.Socket.Send_GameWindowSize((uint)point.X, (uint)point.Y);
				}
				_clicked = false;
			}
		};
		_button.SetTooltip(ResGumps.ResizeGameWindow);
		base.Width = _worldWidth + 10;
		base.Height = _worldHeight + 10;
		_borderControl = new BorderControl(0, 0, base.Width, base.Height, 4);
		_borderControl.DragEnd += delegate
		{
			UIManager.GetGump<OptionsGump>(null)?.UpdateVideo();
		};
		UIManager.SystemChat = (_systemChatControl = new SystemChatControl(5 + ProfileManager.CurrentProfile.ChatPosition.X, 5 - ProfileManager.CurrentProfile.ChatPosition.Y, _worldWidth, _worldHeight));
		Add(_borderControl);
		Add(_button);
		Add(_systemChatControl);
		Resize();
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (base.IsDisposed || !Mouse.IsDragging)
		{
			return;
		}
		Point lDragOffset = Mouse.LDragOffset;
		_lastSize = _savedSize;
		if (_clicked && lDragOffset != Point.Zero)
		{
			int num = _lastSize.X + lDragOffset.X;
			int num2 = _lastSize.Y + lDragOffset.Y;
			if (num < 640)
			{
				num = 640;
			}
			if (num2 < 480)
			{
				num2 = 480;
			}
			if (num > Client.Game.Window.ClientBounds.Width - 5)
			{
				num = Client.Game.Window.ClientBounds.Width - 5;
			}
			if (num2 > Client.Game.Window.ClientBounds.Height - 5)
			{
				num2 = Client.Game.Window.ClientBounds.Height - 5;
			}
			_lastSize.X = num;
			_lastSize.Y = num2;
		}
		if (_worldWidth != _lastSize.X || _worldHeight != _lastSize.Y)
		{
			_worldWidth = _lastSize.X;
			_worldHeight = _lastSize.Y;
			base.Width = _worldWidth + 10;
			base.Height = _worldHeight + 10;
			ProfileManager.CurrentProfile.GameWindowSize = _lastSize;
			Resize();
		}
	}

	protected override void OnDragEnd(int x, int y)
	{
		base.OnDragEnd(x, y);
		Point location = base.Location;
		if (location.X + base.Width - 5 > Client.Game.Window.ClientBounds.Width)
		{
			location.X = Client.Game.Window.ClientBounds.Width - (base.Width - 5);
		}
		if (location.X < -5)
		{
			location.X = -5;
		}
		if (location.Y + base.Height - 5 > Client.Game.Window.ClientBounds.Height)
		{
			location.Y = Client.Game.Window.ClientBounds.Height - (base.Height - 5);
		}
		if (location.Y < -5)
		{
			location.Y = -5;
		}
		base.Location = location;
		ProfileManager.CurrentProfile.GameWindowPosition = location;
		UIManager.GetGump<OptionsGump>(null)?.UpdateVideo();
		UpdateGameWindowPos();
	}

	protected override void OnMove(int x, int y)
	{
		base.OnMove(x, y);
		ProfileManager.CurrentProfile.GameWindowPosition = new Point(base.ScreenCoordinateX, base.ScreenCoordinateY);
		UpdateGameWindowPos();
	}

	private void UpdateGameWindowPos()
	{
		if (_scene != null)
		{
			PacketHandlers.ResendGameWindow = true;
			PacketHandlers.ResendGameWindowTimeout = DateTime.Now + TimeSpan.FromSeconds(1.0);
			_scene.UpdateDrawPosition = true;
		}
	}

	private void Resize()
	{
		_borderControl.Width = base.Width;
		_borderControl.Height = base.Height;
		_button.X = base.Width - (_button.Width >> 1);
		_button.Y = base.Height - (_button.Height >> 1);
		_worldWidth = base.Width - 10;
		_worldHeight = base.Height - 10;
		_systemChatControl.Width = _worldWidth;
		_systemChatControl.Height = _worldHeight;
		_systemChatControl.Resize();
		base.WantUpdateSize = true;
		UpdateGameWindowPos();
	}

	public Point ResizeGameWindow(Point newSize)
	{
		if (newSize.X < 640)
		{
			newSize.X = 640;
		}
		if (newSize.Y < 480)
		{
			newSize.Y = 480;
		}
		Point savedSize = (ProfileManager.CurrentProfile.GameWindowSize = newSize);
		_lastSize = (_savedSize = savedSize);
		if (_worldWidth != _lastSize.X || _worldHeight != _lastSize.Y)
		{
			_worldWidth = _lastSize.X;
			_worldHeight = _lastSize.Y;
			base.Width = _worldWidth + 10;
			base.Height = _worldHeight + 10;
			ProfileManager.CurrentProfile.GameWindowSize = _lastSize;
			Resize();
		}
		return newSize;
	}

	public override bool Contains(int x, int y)
	{
		if (x >= 5 && x < base.Width - 10 && y >= 5 && y < base.Height - 10 - ((_systemChatControl?.TextBoxControl != null && _systemChatControl.IsActive) ? _systemChatControl.TextBoxControl.Height : 0))
		{
			return false;
		}
		return base.Contains(x, y);
	}
}
