using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class QuestArrowGump : Gump
{
	private GumpPic _arrow;

	private Direction _direction;

	private int _mx;

	private int _my;

	private bool _needHue;

	private float _timer;

	public QuestArrowGump(uint serial, int mx, int my)
		: base(serial, serial)
	{
		CanMove = false;
		base.CanCloseWithRightClick = false;
		AcceptMouseInput = true;
		SetRelativePosition(mx, my);
		base.WantUpdateSize = false;
	}

	public void SetRelativePosition(int x, int y)
	{
		_mx = x;
		_my = y;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (!World.InGame)
		{
			Dispose();
		}
		GameScene scene = Client.Game.GetScene<GameScene>();
		if (base.IsDisposed || ProfileManager.CurrentProfile == null || scene == null)
		{
			return;
		}
		Direction direction = (Direction)GameCursor.GetMouseDirection(World.Player.X, World.Player.Y, _mx, _my, 0);
		ushort graphic = (ushort)(4500 + (int)(direction + 1) % 8);
		if (_direction != direction || _arrow == null)
		{
			_direction = direction;
			if (_arrow == null)
			{
				Add(_arrow = new GumpPic(0, 0, graphic, 0));
			}
			else
			{
				_arrow.Graphic = graphic;
			}
			base.Width = _arrow.Width;
			base.Height = _arrow.Height;
		}
		int num = World.Player.X - _mx;
		int num2 = World.Player.Y - _my;
		int num3 = (ProfileManager.CurrentProfile.GameWindowSize.X >> 1) - (num - num2) * 22;
		int num4 = (ProfileManager.CurrentProfile.GameWindowSize.Y >> 1) - (num + num2) * 22;
		num3 -= (int)World.Player.CumulativeOffset.X;
		num4 -= (int)(World.Player.CumulativeOffset.Y - World.Player.CumulativeOffset.Z);
		num4 += World.Player.Z << 2;
		switch (direction)
		{
		case Direction.North:
			num3 -= _arrow.Width;
			break;
		case Direction.South:
			num4 -= _arrow.Height;
			break;
		case Direction.East:
			num3 -= _arrow.Width;
			num4 -= _arrow.Height;
			break;
		case Direction.Right:
			num3 -= _arrow.Width;
			num4 -= _arrow.Height / 2;
			break;
		case Direction.Left:
			num3 += _arrow.Width / 2;
			num4 -= _arrow.Height / 2;
			break;
		case Direction.Up:
			num3 -= _arrow.Width / 2;
			num4 += _arrow.Height / 2;
			break;
		case Direction.Down:
			num3 -= _arrow.Width / 2;
			num4 -= _arrow.Height;
			break;
		}
		Point point = new Point(num3, num4);
		point = Client.Game.Scene.Camera.WorldToScreen(point);
		point.X += ProfileManager.CurrentProfile.GameWindowPosition.X;
		point.Y += ProfileManager.CurrentProfile.GameWindowPosition.Y;
		num3 = point.X;
		num4 = point.Y;
		if (num3 < ProfileManager.CurrentProfile.GameWindowPosition.X)
		{
			num3 = ProfileManager.CurrentProfile.GameWindowPosition.X;
		}
		else if (num3 > ProfileManager.CurrentProfile.GameWindowPosition.X + ProfileManager.CurrentProfile.GameWindowSize.X - _arrow.Width)
		{
			num3 = ProfileManager.CurrentProfile.GameWindowPosition.X + ProfileManager.CurrentProfile.GameWindowSize.X - _arrow.Width;
		}
		if (num4 < ProfileManager.CurrentProfile.GameWindowPosition.Y)
		{
			num4 = ProfileManager.CurrentProfile.GameWindowPosition.Y;
		}
		else if (num4 > ProfileManager.CurrentProfile.GameWindowPosition.Y + ProfileManager.CurrentProfile.GameWindowSize.Y - _arrow.Height)
		{
			num4 = ProfileManager.CurrentProfile.GameWindowPosition.Y + ProfileManager.CurrentProfile.GameWindowSize.Y - _arrow.Height;
		}
		base.X = num3;
		base.Y = num4;
		if (_timer < (float)Time.Ticks)
		{
			_timer = Time.Ticks + 1000;
			_needHue = !_needHue;
		}
		_arrow.Hue = (ushort)((!_needHue) ? 33u : 0u);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		bool num = button == MouseButtonType.Left;
		bool flag = button == MouseButtonType.Right;
		if (num || flag)
		{
			GameActions.QuestArrow(flag);
		}
	}

	public override bool Contains(int x, int y)
	{
		if (_arrow == null)
		{
			return true;
		}
		return _arrow.Contains(x, y);
	}
}
