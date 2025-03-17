using ClassicUO.Input;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Controls;

internal class ExpandableScroll : Control
{
	private const int c_ExpandableScrollHeight_Min = 274;

	private const int c_ExpandableScrollHeight_Max = 1000;

	private const int c_GumplingExpanderY_Offset = 2;

	private const int c_GumplingExpander_ButtonID = 8371951;

	private readonly GumpPic _gumpBottom;

	private Button _gumpExpander;

	private GumpPic _gumplingTitle;

	private int _gumplingTitleGumpID;

	private bool _gumplingTitleGumpIDDelta;

	private readonly GumpPicTiled _gumpMiddle;

	private readonly GumpPicTiled _gumpRight;

	private readonly GumpPic _gumpTop;

	private bool _isExpanding;

	private int _isExpanding_InitialX;

	private int _isExpanding_InitialY;

	private int _isExpanding_InitialHeight;

	private readonly bool _isResizable = true;

	private int _gumplingMidY => _gumpTop.Height;

	private int _gumplingMidHeight
	{
		get
		{
			int num = SpecialHeight - _gumpTop.Height - _gumpBottom.Height;
			Button gumpExpander = _gumpExpander;
			return num - ((gumpExpander != null) ? gumpExpander.Height : 0);
		}
	}

	private int _gumplingBottomY
	{
		get
		{
			int num = SpecialHeight - _gumpBottom.Height;
			Button gumpExpander = _gumpExpander;
			return num - ((gumpExpander != null) ? gumpExpander.Height : 0);
		}
	}

	private int _gumplingExpanderX
	{
		get
		{
			int width = base.Width;
			Button gumpExpander = _gumpExpander;
			return width - ((gumpExpander != null) ? gumpExpander.Width : 0) >> 1;
		}
	}

	private int _gumplingExpanderY
	{
		get
		{
			int specialHeight = SpecialHeight;
			Button gumpExpander = _gumpExpander;
			return specialHeight - ((gumpExpander != null) ? gumpExpander.Height : 0) - 2;
		}
	}

	public int TitleGumpID
	{
		set
		{
			_gumplingTitleGumpID = value;
			_gumplingTitleGumpIDDelta = true;
		}
	}

	public int SpecialHeight { get; set; }

	public ushort Hue
	{
		get
		{
			return _gumpTop.Hue;
		}
		set
		{
			GumpPic gumpTop = _gumpTop;
			GumpPic gumpBottom = _gumpBottom;
			GumpPicTiled gumpMiddle = _gumpMiddle;
			ushort num2 = (_gumpRight.Hue = value);
			ushort num4 = (gumpMiddle.Hue = num2);
			ushort hue = (gumpBottom.Hue = num4);
			gumpTop.Hue = hue;
		}
	}

	public ExpandableScroll(int x, int y, int height, ushort graphic, bool isResizable = true)
	{
		base.X = x;
		base.Y = y;
		SpecialHeight = height;
		_isResizable = isResizable;
		CanMove = true;
		AcceptMouseInput = true;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < 4; i++)
		{
			if (GumpsLoader.Instance.GetGumpTexture((ushort)(graphic + i), out var bounds) == null)
			{
				Dispose();
				return;
			}
			if (bounds.Width > num)
			{
				num = bounds.Width;
			}
			switch (i)
			{
			case 0:
				num2 = bounds.Width;
				break;
			case 1:
				num3 = bounds.Width;
				break;
			case 3:
				num4 = bounds.Width;
				break;
			}
		}
		Add(_gumpTop = new GumpPic(0, 0, graphic, 0));
		Add(_gumpRight = new GumpPicTiled(0, 0, 0, 0, (ushort)(graphic + 1)));
		Add(_gumpMiddle = new GumpPicTiled(0, 0, 0, 0, (ushort)(graphic + 2)));
		Add(_gumpBottom = new GumpPic(0, 0, (ushort)(graphic + 3), 0));
		if (_isResizable)
		{
			Button obj = new Button(8371951, 2094, 2095, 0, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj.X = 0;
			obj.Y = 0;
			Button c = obj;
			_gumpExpander = obj;
			Add(c);
			_gumpExpander.MouseDown += expander_OnMouseDown;
			_gumpExpander.MouseUp += expander_OnMouseUp;
			_gumpExpander.MouseOver += expander_OnMouseOver;
		}
		int num5 = num2 - num4;
		_gumpRight.X = (_gumpMiddle.X = (num - num3) / 2);
		_gumpRight.Y = (_gumpMiddle.Y = _gumplingMidY);
		_gumpRight.Height = (_gumpMiddle.Height = _gumplingMidHeight);
		_gumpRight.WantUpdateSize = (_gumpMiddle.WantUpdateSize = true);
		_gumpBottom.X = num5 / 2 + num5 / 4;
		base.Width = _gumpMiddle.Width;
		base.WantUpdateSize = true;
	}

	public override void Dispose()
	{
		if (_gumpExpander != null)
		{
			_gumpExpander.MouseDown -= expander_OnMouseDown;
			_gumpExpander.MouseUp -= expander_OnMouseUp;
			_gumpExpander.MouseOver -= expander_OnMouseOver;
			_gumpExpander.Dispose();
			_gumpExpander = null;
		}
		base.Dispose();
	}

	public override bool Contains(int x, int y)
	{
		x += base.ScreenCoordinateX;
		y += base.ScreenCoordinateY;
		Control res = null;
		_gumpTop.HitTest(x, y, ref res);
		if (res != null)
		{
			return true;
		}
		_gumpMiddle.HitTest(x, y, ref res);
		if (res != null)
		{
			return true;
		}
		_gumpRight.HitTest(x, y, ref res);
		if (res != null)
		{
			return true;
		}
		_gumpBottom.HitTest(x, y, ref res);
		if (res != null)
		{
			return true;
		}
		_gumpExpander.HitTest(x, y, ref res);
		if (res != null)
		{
			return true;
		}
		return false;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (SpecialHeight < 274)
		{
			SpecialHeight = 274;
		}
		if (SpecialHeight > 1000)
		{
			SpecialHeight = 1000;
		}
		if (_gumplingTitleGumpIDDelta)
		{
			_gumplingTitleGumpIDDelta = false;
			_gumplingTitle?.Dispose();
			Add(_gumplingTitle = new GumpPic(0, 0, (ushort)_gumplingTitleGumpID, 0));
		}
		_gumpTop.X = 0;
		_gumpTop.Y = 0;
		_gumpTop.WantUpdateSize = true;
		_gumpRight.Y = (_gumpMiddle.Y = _gumplingMidY);
		_gumpRight.Height = (_gumpMiddle.Height = _gumplingMidHeight);
		GumpPicTiled gumpRight = _gumpRight;
		bool wantUpdateSize = (_gumpMiddle.WantUpdateSize = true);
		gumpRight.WantUpdateSize = wantUpdateSize;
		_gumpBottom.Y = _gumplingBottomY;
		_gumpBottom.WantUpdateSize = true;
		if (_isResizable)
		{
			_gumpExpander.X = _gumplingExpanderX;
			_gumpExpander.Y = _gumplingExpanderY;
			_gumpExpander.WantUpdateSize = true;
		}
		if (_gumplingTitle != null)
		{
			_gumplingTitle.X = _gumpTop.Width - _gumplingTitle.Width >> 1;
			_gumplingTitle.Y = _gumpTop.Height - _gumplingTitle.Height >> 1;
			_gumplingTitle.WantUpdateSize = true;
		}
		base.WantUpdateSize = true;
		base.Parent?.OnPageChanged();
		base.Update(totalTime, frameTime);
	}

	private void expander_OnMouseDown(object sender, MouseEventArgs args)
	{
		int y = args.Y;
		int x = args.X;
		y += _gumpExpander.Y + base.ScreenCoordinateY - base.Y;
		if (args.Button == MouseButtonType.Left)
		{
			_isExpanding = true;
			_isExpanding_InitialHeight = SpecialHeight;
			_isExpanding_InitialX = x;
			_isExpanding_InitialY = y;
		}
	}

	private void expander_OnMouseUp(object sender, MouseEventArgs args)
	{
		int y = args.Y;
		y += _gumpExpander.Y + base.ScreenCoordinateY - base.Y;
		if (_isExpanding)
		{
			_isExpanding = false;
			SpecialHeight = _isExpanding_InitialHeight + (y - _isExpanding_InitialY);
		}
	}

	private void expander_OnMouseOver(object sender, MouseEventArgs args)
	{
		int y = args.Y;
		y += _gumpExpander.Y + base.ScreenCoordinateY - base.Y;
		if (_isExpanding && y != _isExpanding_InitialY)
		{
			SpecialHeight = _isExpanding_InitialHeight + (y - _isExpanding_InitialY);
		}
	}
}
