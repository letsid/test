using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class RacialAbilitiesBookGump : Gump
{
	private static readonly string[] _humanNames = new string[4] { "Strong Back", "Tough", "Workhorse", "Jack of All Trades" };

	private static readonly string[] _elfNames = new string[6] { "Night Sight", "Infused with Magic", "Knowledge of Nature", "Difficult to Track", "Perception", "Wisdom" };

	private static readonly string[] _gargoyleNames = new string[5] { "Flying", "Berserk", "Master Artisan", "Deadly Aim", "Mystic Insight" };

	private int _abilityCount = 4;

	private float _clickTiming;

	private int _dictionaryPagesCount = 1;

	private Control _lastPressed;

	private GumpPic _pageCornerLeft;

	private GumpPic _pageCornerRight;

	private int _pagesCount = 3;

	private int _tooltipOffset = 1112198;

	public RacialAbilitiesBookGump(int x, int y)
		: base(0u, 0u)
	{
		base.X = x;
		base.Y = y;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		BuildGump();
	}

	private void BuildGump()
	{
		Add(new GumpPic(0, 0, 11049, 0));
		Add(_pageCornerLeft = new GumpPic(50, 8, 2235, 0));
		_pageCornerLeft.LocalSerial = 0u;
		_pageCornerLeft.Page = int.MaxValue;
		_pageCornerLeft.MouseUp += PageCornerOnMouseClick;
		_pageCornerLeft.MouseDoubleClick += PageCornerOnMouseDoubleClick;
		Add(_pageCornerRight = new GumpPic(321, 8, 2236, 0));
		_pageCornerRight.LocalSerial = 1u;
		_pageCornerRight.Page = 1;
		_pageCornerRight.MouseUp += PageCornerOnMouseClick;
		_pageCornerRight.MouseDoubleClick += PageCornerOnMouseDoubleClick;
		int abilityOnPage = 0;
		ushort iconStartGraphic = 0;
		GetSummaryBookInfo(ref abilityOnPage, ref iconStartGraphic);
		_pagesCount = _dictionaryPagesCount + (_abilityCount >> 1);
		int num = 0;
		int i = 1;
		int num2 = _dictionaryPagesCount - 1;
		for (; i <= _dictionaryPagesCount; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				int num3 = 106;
				int num4 = 62;
				int num5 = 0;
				if (j % 2 != 0)
				{
					num3 = 269;
					num4 = 225;
				}
				Label label = new Label(ResGumps.Index, isunicode: false, 648, 0, 6);
				label.X = num3;
				label.Y = 10;
				Label c = label;
				Add(c, i);
				for (int k = 0; k < abilityOnPage; k++)
				{
					if (num >= _abilityCount)
					{
						break;
					}
					if (num % 2 == 0)
					{
						num2++;
					}
					bool passive = true;
					HoveredLabel hoveredLabel = new HoveredLabel(GetAbilityName(num, ref passive), isunicode: false, 648, 51, 648, 0, 9);
					hoveredLabel.X = num4;
					hoveredLabel.Y = 52 + num5;
					hoveredLabel.AcceptMouseInput = true;
					hoveredLabel.LocalSerial = (uint)num2;
					c = hoveredLabel;
					c.MouseUp += OnClicked;
					Add(c, i);
					num5 += 15;
					num++;
				}
			}
		}
		int num6 = _dictionaryPagesCount - 1;
		for (int l = 0; l < _abilityCount; l++)
		{
			int x = 62;
			int num7 = 112;
			if (l > 0 && l % 2 != 0)
			{
				x = 225;
				num7 = 275;
			}
			else
			{
				num6++;
			}
			bool passive2 = true;
			Label label2 = new Label(GetAbilityName(l, ref passive2), isunicode: false, 648, 100, 6);
			label2.X = num7;
			label2.Y = 34;
			Label c2 = label2;
			Add(c2, num6);
			if (passive2)
			{
				Label label3 = new Label(ResGumps.Passive, isunicode: false, 648, 0, 6);
				label3.X = num7;
				label3.Y = 64;
				c2 = label3;
				Add(c2, num6);
			}
			ushort num8 = (ushort)(iconStartGraphic + l);
			GumpPic gumpPic = new GumpPic(x, 40, num8, 0)
			{
				LocalSerial = num8
			};
			if (!passive2)
			{
				gumpPic.DragBegin += delegate(object? sender, MouseEventArgs e)
				{
					if (!UIManager.IsDragging)
					{
						RacialAbilityButton racialAbilityButton = new RacialAbilityButton((ushort)((GumpPic)sender).LocalSerial);
						racialAbilityButton.X = Mouse.LClickPosition.X - 20;
						racialAbilityButton.Y = Mouse.LClickPosition.Y - 20;
						UIManager.Add(racialAbilityButton);
						UIManager.AttemptDragControl(racialAbilityButton, attemptAlwaysSuccessful: true);
					}
				};
				gumpPic.MouseDoubleClick += delegate(object? sender, MouseDoubleClickEventArgs e)
				{
					if ((ushort)((GumpPic)sender).LocalSerial == 24026 && World.Player.Race == RaceType.GARGOYLE)
					{
						NetClient.Socket.Send_ToggleGargoyleFlying();
						e.Result = true;
					}
				};
			}
			Add(gumpPic, num6);
			gumpPic.SetTooltip(ClilocLoader.Instance.GetString(_tooltipOffset + l), 150);
			Add(new GumpPicTiled(x, 88, 120, 4, 2101), num6);
		}
	}

	private void GetSummaryBookInfo(ref int abilityOnPage, ref ushort iconStartGraphic)
	{
		_dictionaryPagesCount = 2;
		abilityOnPage = 3;
		switch (World.Player.Race)
		{
		case RaceType.HUMAN:
			_abilityCount = 4;
			iconStartGraphic = 24016;
			_tooltipOffset = 1112198;
			break;
		case RaceType.ELF:
			_abilityCount = 6;
			iconStartGraphic = 24020;
			_tooltipOffset = 1112202;
			break;
		case RaceType.GARGOYLE:
			_abilityCount = 5;
			iconStartGraphic = 24026;
			_tooltipOffset = 1112208;
			break;
		}
	}

	private string GetAbilityName(int offset, ref bool passive)
	{
		passive = true;
		switch (World.Player.Race)
		{
		case RaceType.HUMAN:
			return _humanNames[offset];
		case RaceType.ELF:
			return _elfNames[offset];
		case RaceType.GARGOYLE:
			if (offset == 0)
			{
				passive = false;
			}
			return _gargoyleNames[offset];
		default:
			return string.Empty;
		}
	}

	private void OnClicked(object sender, MouseEventArgs e)
	{
		if (sender is HoveredLabel lastPressed && e.Button == MouseButtonType.Left)
		{
			_clickTiming += 350f;
			if (_clickTiming > 0f)
			{
				_lastPressed = lastPressed;
			}
		}
	}

	private void PageCornerOnMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? (base.ActivePage - 1) : (base.ActivePage + 1));
		}
	}

	private void PageCornerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? 1 : _pagesCount);
		}
	}

	private void SetActivePage(int page)
	{
		if (page < 1)
		{
			page = 1;
		}
		else if (page > _pagesCount)
		{
			page = _pagesCount;
		}
		base.ActivePage = page;
		_pageCornerLeft.Page = ((base.ActivePage == 1) ? int.MaxValue : 0);
		_pageCornerRight.Page = ((base.ActivePage == _pagesCount) ? int.MaxValue : 0);
		Client.Game.Scene.Audio.PlaySound(85);
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (!base.IsDisposed && _lastPressed != null)
		{
			_clickTiming -= (float)frameTime;
			if (_clickTiming <= 0f)
			{
				_clickTiming = 0f;
				SetActivePage((int)_lastPressed.LocalSerial);
				_lastPressed = null;
			}
		}
	}
}
