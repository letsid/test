using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class UserMarkersGump : Gump
{
	private enum ButtonsOption
	{
		ADD_BTN,
		EDIT_BTN,
		CANCEL_BTN
	}

	private readonly StbTextBox _textBoxX;

	private readonly StbTextBox _textBoxY;

	private readonly StbTextBox _markerName;

	private const ushort HUE_FONT = ushort.MaxValue;

	private const ushort LABEL_OFFSET = 40;

	private const ushort Y_OFFSET = 30;

	private readonly Combobox _colorsCombo;

	private readonly string[] _colors;

	private readonly Combobox _iconsCombo;

	private readonly string[] _icons;

	private readonly List<WorldMapGump.WMapMarker> _markers;

	private readonly int _markerIdx;

	private const int MAX_CORD_LEN = 10;

	private const int MAX_NAME_LEN = 25;

	private const int MAP_MIN_CORD = 0;

	private readonly int _mapMaxX = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 0];

	private readonly int _mapMaxY = MapLoader.Instance.MapsDefaultSize[World.MapIndex, 1];

	private readonly string _userMarkersFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", "userMarkers.usr");

	public event EventHandler EditEnd;

	internal UserMarkersGump(int x, int y, List<WorldMapGump.WMapMarker> markers, string color = "none", string icon = "exit", bool isEdit = false, int markerIdx = -1)
		: base(0u, 0u)
	{
		CanMove = true;
		_markers = markers;
		_markerIdx = markerIdx;
		_colors = new string[8] { "none", "red", "green", "blue", "purple", "black", "yellow", "white" };
		_icons = WorldMapGump._markerIcons.Keys.ToArray();
		string text = ((_markerIdx < 0) ? ResGumps.MarkerDefName : _markers[_markerIdx].Name);
		int num = Array.IndexOf(_icons, icon);
		if (num < 0)
		{
			num = 0;
		}
		int num2 = (isEdit ? Array.IndexOf(_colors, color) : ((_icons.Length == 0) ? 1 : 0));
		if (num2 < 0)
		{
			num2 = 0;
		}
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl();
		alphaBlendControl.Width = 320;
		alphaBlendControl.Height = 220;
		alphaBlendControl.X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125;
		alphaBlendControl.Y = 150;
		alphaBlendControl.Alpha = 0.7f;
		alphaBlendControl.CanMove = true;
		alphaBlendControl.CanCloseWithRightClick = true;
		alphaBlendControl.AcceptMouseInput = true;
		AlphaBlendControl alphaBlendControl2 = alphaBlendControl;
		Add(alphaBlendControl2);
		if (!isEdit)
		{
			Label label = new Label(ResGumps.AddMarker, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
			label.X = alphaBlendControl2.X + 100;
			label.Y = alphaBlendControl2.Y + 3;
			Add(label);
		}
		else
		{
			Label label2 = new Label(ResGumps.EditMarker, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
			label2.X = alphaBlendControl2.X + 100;
			label2.Y = alphaBlendControl2.Y + 3;
			Add(label2);
		}
		int num3 = alphaBlendControl2.X + 5;
		int num4 = alphaBlendControl2.Y + 25;
		ResizePic resizePic = new ResizePic(3000);
		resizePic.X = num3 + 40;
		resizePic.Y = num4;
		resizePic.Width = 90;
		resizePic.Height = 25;
		Add(resizePic);
		StbTextBox stbTextBox = new StbTextBox(byte.MaxValue, 10, 90, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 0);
		stbTextBox.X = num3 + 40;
		stbTextBox.Y = num4;
		stbTextBox.Width = 90;
		stbTextBox.Height = 25;
		stbTextBox.Text = x.ToString();
		_textBoxX = stbTextBox;
		Add(_textBoxX);
		Label label3 = new Label(ResGumps.MarkerX, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
		label3.X = num3;
		label3.Y = num4;
		Add(label3);
		num4 += 30;
		ResizePic resizePic2 = new ResizePic(3000);
		resizePic2.X = num3 + 40;
		resizePic2.Y = num4;
		resizePic2.Width = 90;
		resizePic2.Height = 25;
		Add(resizePic2);
		StbTextBox stbTextBox2 = new StbTextBox(byte.MaxValue, 10, 90, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 0);
		stbTextBox2.X = num3 + 40;
		stbTextBox2.Y = num4;
		stbTextBox2.Width = 90;
		stbTextBox2.Height = 25;
		stbTextBox2.Text = y.ToString();
		_textBoxY = stbTextBox2;
		Add(_textBoxY);
		Label label4 = new Label(ResGumps.MarkerY, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
		label4.X = num3;
		label4.Y = num4;
		Add(label4);
		num4 += 30;
		ResizePic resizePic3 = new ResizePic(3000);
		resizePic3.X = num3 + 40;
		resizePic3.Y = num4;
		resizePic3.Width = 250;
		resizePic3.Height = 25;
		Add(resizePic3);
		StbTextBox stbTextBox3 = new StbTextBox(byte.MaxValue, 25, 250, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 0);
		stbTextBox3.X = num3 + 40;
		stbTextBox3.Y = num4;
		stbTextBox3.Width = 250;
		stbTextBox3.Height = 25;
		stbTextBox3.Text = text;
		_markerName = stbTextBox3;
		Add(_markerName);
		Label label5 = new Label(ResGumps.MarkerName, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
		label5.X = num3;
		label5.Y = num4;
		Add(label5);
		num4 += 30;
		_colorsCombo = new Combobox(num3 + 40, num4, 250, _colors, num2, 200, showArrow: true, "", 9);
		Add(_colorsCombo);
		Label label6 = new Label(ResGumps.MarkerColor, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
		label6.X = num3;
		label6.Y = num4;
		Add(label6);
		if (_icons.Length != 0)
		{
			num4 += 30;
			_iconsCombo = new Combobox(num3 + 40, num4, 250, _icons, num, 200, showArrow: true, "", 9);
			Add(_iconsCombo);
			Label label7 = new Label(ResGumps.MarkerIcon, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
			label7.X = num3;
			label7.Y = num4;
			Add(label7);
		}
		if (!isEdit)
		{
			Add(new NiceButton(alphaBlendControl2.X + 13, alphaBlendControl2.Y + alphaBlendControl2.Height - 30, 60, 25, ButtonAction.Activate, ResGumps.CreateMarker)
			{
				ButtonParameter = 0,
				IsSelectable = false
			});
		}
		else
		{
			Add(new NiceButton(alphaBlendControl2.X + 13, alphaBlendControl2.Y + alphaBlendControl2.Height - 30, 60, 25, ButtonAction.Activate, ResGumps.Edit)
			{
				ButtonParameter = 1,
				IsSelectable = false
			});
		}
		Add(new NiceButton(alphaBlendControl2.X + 78, alphaBlendControl2.Y + alphaBlendControl2.Height - 30, 60, 25, ButtonAction.Activate, ResGumps.Cancel)
		{
			ButtonParameter = 2,
			IsSelectable = false
		});
		SetInScreen();
	}

	private void EditMarker()
	{
		WorldMapGump.WMapMarker wMapMarker = PrepareMarker();
		if (wMapMarker != null)
		{
			_markers[_markerIdx] = wMapMarker;
			this.EditEnd.Raise(wMapMarker);
			Dispose();
		}
	}

	private void AddNewMarker()
	{
		if (File.Exists(_userMarkersFilePath))
		{
			WorldMapGump.WMapMarker wMapMarker = PrepareMarker();
			if (wMapMarker != null)
			{
				string contents = $"{wMapMarker.X},{wMapMarker.Y},{wMapMarker.MapId},{wMapMarker.Name},{wMapMarker.MarkerIconName},{wMapMarker.ColorName},4\r";
				File.AppendAllText(_userMarkersFilePath, contents);
				_markers.Add(wMapMarker);
				Dispose();
			}
		}
	}

	private WorldMapGump.WMapMarker PrepareMarker()
	{
		if (!int.TryParse(_textBoxX.Text, out var result))
		{
			return null;
		}
		if (!int.TryParse(_textBoxY.Text, out var result2))
		{
			return null;
		}
		if (result > _mapMaxX || result < 0)
		{
			return null;
		}
		if (result2 > _mapMaxY || result2 < 0)
		{
			return null;
		}
		string text = _markerName.Text;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		if (text.Contains(","))
		{
			text = text.Replace(",", "");
		}
		int mapIndex = World.MapIndex;
		string text2 = _colors[_colorsCombo.SelectedIndex];
		string text3 = ((_iconsCombo == null) ? string.Empty : _icons[_iconsCombo.SelectedIndex]);
		WorldMapGump.WMapMarker wMapMarker = new WorldMapGump.WMapMarker
		{
			Name = text,
			X = result,
			Y = result2,
			MapId = mapIndex,
			ColorName = text2,
			Color = WorldMapGump.GetColor(text2)
		};
		if (!WorldMapGump._markerIcons.TryGetValue(text3, out var value))
		{
			return wMapMarker;
		}
		wMapMarker.MarkerIcon = value;
		wMapMarker.MarkerIconName = text3;
		return wMapMarker;
	}

	public override void OnButtonClick(int buttonId)
	{
		switch (buttonId)
		{
		case 0:
			AddNewMarker();
			break;
		case 1:
			EditMarker();
			break;
		case 2:
			Dispose();
			break;
		}
	}
}
