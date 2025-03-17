using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class ResizableGump : Gump
{
	private readonly BorderControl _borderControl;

	private readonly Button _button;

	private bool _clicked;

	private Point _lastSize;

	private Point _savedSize;

	private readonly int _minH;

	private readonly int _minW;

	public bool ShowBorder
	{
		get
		{
			return _borderControl.IsVisible;
		}
		set
		{
			BorderControl borderControl = _borderControl;
			bool isVisible = (_button.IsVisible = value);
			borderControl.IsVisible = isVisible;
		}
	}

	protected ResizableGump(int width, int height, int minW, int minH, uint local, uint server, ushort borderHue = 0)
		: base(local, server)
	{
		_borderControl = new BorderControl(0, 0, base.Width, base.Height, 4)
		{
			Hue = borderHue
		};
		Add(_borderControl);
		_button = new Button(0, 2103, 2104, 2104, "", 0);
		Add(_button);
		_button.MouseDown += delegate
		{
			_clicked = true;
		};
		_button.MouseUp += delegate
		{
			ResizeWindow(_lastSize);
			_clicked = false;
		};
		base.WantUpdateSize = false;
		base.Width = (_lastSize.X = width);
		base.Height = (_lastSize.Y = height);
		_savedSize = _lastSize;
		_minW = minW;
		_minH = minH;
		OnResize();
	}

	public Point ResizeWindow(Point newSize)
	{
		if (newSize.X < _minW)
		{
			newSize.X = _minW;
		}
		if (newSize.Y < _minH)
		{
			newSize.Y = _minH;
		}
		_savedSize = newSize;
		return newSize;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		Point lDragOffset = Mouse.LDragOffset;
		_lastSize = _savedSize;
		if (_clicked && lDragOffset != Point.Zero)
		{
			int num = _lastSize.X + lDragOffset.X;
			int num2 = _lastSize.Y + lDragOffset.Y;
			if (num < _minW)
			{
				num = _minW;
			}
			if (num2 < _minH)
			{
				num2 = _minH;
			}
			_lastSize.X = num;
			_lastSize.Y = num2;
		}
		if (base.Width != _lastSize.X || base.Height != _lastSize.Y)
		{
			base.Width = _lastSize.X;
			base.Height = _lastSize.Y;
			OnResize();
		}
		base.Update(totalTime, frameTime);
	}

	public virtual void OnResize()
	{
		_borderControl.Width = base.Width;
		_borderControl.Height = base.Height;
		_button.X = base.Width - _button.Width + 2;
		_button.Y = base.Height - _button.Height + 2;
	}
}
