using System;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class IgnoreManagerGump : Gump
{
	private enum ButtonsId
	{
		ADD_NEW_IGNORE
	}

	private sealed class IgnoreListControl : Control
	{
		private readonly string _chName;

		public event EventHandler RemoveMarkerEvent;

		public IgnoreListControl(string chName)
		{
			CanMove = true;
			AcceptMouseInput = false;
			base.CanCloseWithRightClick = true;
			_chName = chName;
			Label label = new Label(chName, isunicode: true, ushort.MaxValue, 290);
			label.X = 10;
			Add(label);
			Button button = new Button(1, 4011, 4012, 0, "", 0);
			button.X = 220;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
		}

		public override void OnButtonClick(int buttonId)
		{
			IgnoreManager.RemoveIgnoredTarget(_chName);
			this.RemoveMarkerEvent.Raise();
		}
	}

	private const ushort HUE_FONT = ushort.MaxValue;

	private const ushort BACKGROUND_COLOR = 999;

	private const ushort GUMP_WIDTH = 300;

	private const ushort GUMP_HEIGHT = 400;

	private readonly int _gumpPosX = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125;

	private readonly int _gumpPosY = 100;

	private static ScrollArea _scrollArea;

	private bool _isListModified;

	public IgnoreManagerGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.05f);
		alphaBlendControl.X = _gumpPosX;
		alphaBlendControl.Y = _gumpPosY;
		alphaBlendControl.Width = 300;
		alphaBlendControl.Height = 400;
		alphaBlendControl.Hue = 999;
		alphaBlendControl.AcceptMouseInput = true;
		alphaBlendControl.CanMove = true;
		alphaBlendControl.CanCloseWithRightClick = true;
		Add(alphaBlendControl);
		Add(new Line(_gumpPosX, _gumpPosY, 300, 1, Color.Gray.PackedValue));
		Add(new Line(_gumpPosX, _gumpPosY, 1, 400, Color.Gray.PackedValue));
		Add(new Line(_gumpPosX, 400 + _gumpPosY, 300, 1, Color.Gray.PackedValue));
		Add(new Line(300 + _gumpPosX, _gumpPosY, 1, 400, Color.Gray.PackedValue));
		int num = _gumpPosY + 10;
		Label label = new Label(ResGumps.IgnoreListName, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label.X = _gumpPosX + 10;
		label.Y = num;
		Add(label);
		Label label2 = new Label(ResGumps.Remove, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label2.X = _gumpPosX + 210;
		label2.Y = num;
		Add(label2);
		Add(new Line(_gumpPosX, num + 20, 300, 1, Color.Gray.PackedValue));
		Add(new NiceButton(_gumpPosX + 20, _gumpPosY + 400 - 30, 260, 25, ButtonAction.Activate, ResGumps.IgnoreListAddButton));
		DrawArea();
		SetInScreen();
	}

	public override void Dispose()
	{
		if (_isListModified)
		{
			IgnoreManager.SaveIgnoreList();
		}
		if (TargetManager.IsTargeting)
		{
			TargetManager.CancelTarget();
		}
		base.Dispose();
	}

	private void DrawArea()
	{
		_scrollArea = new ScrollArea(_gumpPosX + 10, _gumpPosY + 40, 280, 320, normalScrollbar: true);
		int y = 0;
		foreach (IgnoreListControl item in IgnoreManager.IgnoredCharsList.Select(delegate(string m)
		{
			IgnoreListControl ignoreListControl = new IgnoreListControl(m);
			ignoreListControl.Y = y;
			return ignoreListControl;
		}))
		{
			item.RemoveMarkerEvent += MarkerRemoveEventHandler;
			_scrollArea.Add(item);
			y += 25;
		}
		Add(_scrollArea);
	}

	private void MarkerRemoveEventHandler(object sender, EventArgs e)
	{
		Redraw();
	}

	public void Redraw()
	{
		_isListModified = true;
		Remove(_scrollArea);
		DrawArea();
	}

	public override void OnButtonClick(int buttonId)
	{
		if (buttonId == 0)
		{
			TargetManager.SetTargeting(CursorTarget.IgnorePlayerTarget, CursorType.Target, TargetType.Neutral);
		}
	}
}
