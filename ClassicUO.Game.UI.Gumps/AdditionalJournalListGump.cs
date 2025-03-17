using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class AdditionalJournalListGump : Gump
{
	private enum ButtonsId
	{
		ADD_NEW_CHARACTER
	}

	private sealed class AdditionalDictControl : Control
	{
		private readonly uint _chSerial;

		public event EventHandler RemoveMarkerEvent;

		public AdditionalDictControl(uint serial)
		{
			CanMove = true;
			AcceptMouseInput = false;
			base.CanCloseWithRightClick = true;
			_chSerial = serial;
			Label label = new Label(World.AdditionalJournalFilterDict.First((KeyValuePair<uint, string> k) => k.Key == _chSerial).Value, isunicode: true, ushort.MaxValue, 290);
			label.X = 10;
			Add(label);
			Button button = new Button(1, 4011, 4012, 0, "", 0);
			button.X = 220;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
		}

		public override void OnButtonClick(int buttonId)
		{
			World.AdditionalJournalFilterDict.Remove(_chSerial);
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

	public AdditionalJournalListGump()
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
		Label label = new Label(ResGumps.AdditionalJournalDictName, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label.X = _gumpPosX + 10;
		label.Y = num;
		Add(label);
		Label label2 = new Label(ResGumps.Remove, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label2.X = _gumpPosX + 210;
		label2.Y = num;
		Add(label2);
		Add(new Line(_gumpPosX, num + 20, 300, 1, Color.Gray.PackedValue));
		Add(new NiceButton(_gumpPosX + 20, _gumpPosY + 400 - 30, 260, 25, ButtonAction.Activate, ResGumps.AdditionalJournalDictAddButton));
		DrawArea();
		SetInScreen();
	}

	public override void Dispose()
	{
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
		foreach (AdditionalDictControl item in World.AdditionalJournalFilterDict.Select(delegate(KeyValuePair<uint, string> m)
		{
			AdditionalDictControl additionalDictControl = new AdditionalDictControl(m.Key);
			additionalDictControl.Y = y;
			return additionalDictControl;
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
			TargetManager.SetTargeting(CursorTarget.AddAdditionalJournalTarget, 0u, TargetType.Neutral);
		}
	}

	public static void AddAdditionalJournalDictEntry(Entity entity)
	{
		if (entity is Mobile { Name: var name } mobile)
		{
			if (mobile.Serial == World.Player.Serial)
			{
				GameActions.Print(string.Format(ResGumps.AddAdditionalJournalDictSelf, name), 946, MessageType.Regular, 3);
				return;
			}
			if (World.AdditionalJournalFilterDict.ContainsKey(mobile.Serial))
			{
				GameActions.Print(string.Format(ResGumps.AddToAdditionalJournalDictExist, name), 946, MessageType.Regular, 3);
				return;
			}
			World.AdditionalJournalFilterDict.Add(mobile.Serial, name);
			UIManager.GetGump<AdditionalJournalListGump>(null)?.Redraw();
			GameActions.Print(string.Format(ResGumps.AddToAdditionalJournalDictSuccess, name), 946, MessageType.Regular, 3);
		}
		else
		{
			GameActions.Print(string.Format(ResGumps.AddToAdditionalJournalDictNotMobile), 946, MessageType.Regular, 3);
		}
	}
}
