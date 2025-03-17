using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class MarkersManagerGump : Gump
{
	private enum ButtonsOption
	{
		SEARCH_BTN = 100,
		CLEAR_SEARCH_BTN
	}

	internal class DrawTexture : Control
	{
		public Texture2D Texture;

		public DrawTexture(Texture2D texture)
		{
			Texture = texture;
		}
	}

	private sealed class MakerManagerControl : Control
	{
		private enum ButtonsOption
		{
			EDIT_MARKER_BTN,
			REMOVE_MARKER_BTN,
			GOTO_MARKER_BTN
		}

		private readonly WorldMapGump.WMapMarker _marker;

		private readonly int _y;

		private readonly int _idx;

		private readonly bool _isEditable;

		private Label _labelName;

		private Label _labelX;

		private Label _labelY;

		private Label _labelColor;

		private DrawTexture _iconTexture;

		public event EventHandler RemoveMarkerEvent;

		public event EventHandler EditMarkerEvent;

		public MakerManagerControl(WorldMapGump.WMapMarker marker, int y, int idx, bool isEditable)
		{
			CanMove = true;
			_idx = idx;
			_marker = marker;
			_y = y;
			_isEditable = isEditable;
			DrawData();
		}

		private void DrawData()
		{
			if (_marker.MarkerIcon != null)
			{
				DrawTexture drawTexture = new DrawTexture(_marker.MarkerIcon);
				drawTexture.X = 0;
				drawTexture.Y = _y - 5;
				_iconTexture = drawTexture;
				Add(_iconTexture);
			}
			Label label = new Label(_marker.Name ?? "", isunicode: true, ushort.MaxValue, 280);
			label.X = 30;
			label.Y = _y;
			_labelName = label;
			Add(_labelName);
			Label label2 = new Label($"{_marker.X}", isunicode: true, ushort.MaxValue, 35);
			label2.X = 305;
			label2.Y = _y;
			_labelX = label2;
			Add(_labelX);
			Label label3 = new Label($"{_marker.Y}", isunicode: true, ushort.MaxValue, 35);
			label3.X = 350;
			label3.Y = _y;
			_labelY = label3;
			Add(_labelY);
			Label label4 = new Label(_marker.ColorName ?? "", isunicode: true, ushort.MaxValue, 35);
			label4.X = 410;
			label4.Y = _y;
			_labelColor = label4;
			Add(_labelColor);
			if (_isEditable)
			{
				Button button = new Button(0, 4011, 4012, 0, "", 0);
				button.X = 470;
				button.Y = _y;
				button.ButtonAction = ButtonAction.Activate;
				Add(button);
				Button button2 = new Button(1, 4017, 4018, 0, "", 0);
				button2.X = 505;
				button2.Y = _y;
				button2.ButtonAction = ButtonAction.Activate;
				Add(button2);
			}
		}

		private void OnEditEnd(object sender, EventArgs e)
		{
			if (sender is WorldMapGump.WMapMarker wMapMarker)
			{
				_labelName.Text = wMapMarker.Name;
				_labelColor.Text = wMapMarker.ColorName;
				_labelX.Text = wMapMarker.X.ToString();
				_labelY.Text = wMapMarker.Y.ToString();
				if (wMapMarker.MarkerIcon != null)
				{
					_iconTexture.Texture = wMapMarker.MarkerIcon;
				}
				this.EditMarkerEvent.Raise();
			}
		}

		public override void OnButtonClick(int buttonId)
		{
			switch (buttonId)
			{
			case 0:
			{
				UIManager.GetGump<UserMarkersGump>(null)?.Dispose();
				UserMarkersGump userMarkersGump = new UserMarkersGump(_marker.X, _marker.Y, _markers, _marker.ColorName, _marker.MarkerIconName, isEdit: true, _idx);
				userMarkersGump.EditEnd += OnEditEnd;
				UIManager.Add(userMarkersGump);
				break;
			}
			case 1:
				this.RemoveMarkerEvent.Raise(_idx);
				break;
			}
		}
	}

	private sealed class SearchTextBoxControl : Control
	{
		private readonly StbTextBox _textBox;

		public string SearchText => _textBox.Text;

		public SearchTextBoxControl(int x, int y)
		{
			AcceptMouseInput = true;
			AcceptKeyboardInput = true;
			Label label = new Label(ResGumps.MarkerSearch, isunicode: true, ushort.MaxValue, 50, 1);
			label.X = x;
			label.Y = y;
			Add(label);
			ResizePic resizePic = new ResizePic(3000);
			resizePic.X = x + 50;
			resizePic.Y = y;
			resizePic.Width = 200;
			resizePic.Height = 25;
			Add(resizePic);
			StbTextBox stbTextBox = new StbTextBox(byte.MaxValue, 30, 200, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, 0);
			stbTextBox.X = x + 53;
			stbTextBox.Y = y + 3;
			stbTextBox.Width = 200;
			stbTextBox.Height = 25;
			_textBox = stbTextBox;
			Add(_textBox);
			Button button = new Button(100, 4023, 4025, 0, "", 0);
			button.X = x + 250;
			button.Y = y + 1;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Button button2 = new Button(101, 4017, 4018, 0, "", 0);
			button2.X = x + 285;
			button2.Y = y + 1;
			button2.ButtonAction = ButtonAction.Activate;
			Add(button2);
		}

		public void ClearText()
		{
			_textBox.SetText("");
		}
	}

	private const int WIDTH = 620;

	private const int HEIGHT = 500;

	private const ushort HUE_FONT = ushort.MaxValue;

	private bool _isMarkerListModified;

	private ScrollArea _scrollArea;

	private readonly SearchTextBoxControl _searchTextBox;

	private string _searchText = "";

	private int _categoryId;

	private static List<WorldMapGump.WMapMarker> _markers = new List<WorldMapGump.WMapMarker>();

	private static readonly List<WorldMapGump.WMapMarkerFile> _markerFiles = WorldMapGump._markerFiles;

	private readonly string _userMarkersFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client", "userMarkers.usr");

	private readonly int MARKERS_CATEGORY_GROUP_INDEX = 10;

	internal MarkersManagerGump()
		: base(0u, 0u)
	{
		base.X = 50;
		base.Y = 50;
		CanMove = true;
		AcceptMouseInput = true;
		int num = 50;
		if (_markerFiles.Count > 0)
		{
			_markers = _markerFiles[0].Markers;
			num = 620 / _markerFiles.Count;
		}
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.05f);
		alphaBlendControl.X = 1;
		alphaBlendControl.Y = 1;
		alphaBlendControl.Width = 620;
		alphaBlendControl.Height = 500;
		alphaBlendControl.Hue = 999;
		alphaBlendControl.AcceptMouseInput = true;
		alphaBlendControl.CanCloseWithRightClick = true;
		alphaBlendControl.CanMove = true;
		Add(alphaBlendControl);
		Add(new Line(0, 0, 620, 1, Color.Gray.PackedValue));
		Add(new Line(0, 0, 1, 500, Color.Gray.PackedValue));
		Add(new Line(0, 500, 620, 1, Color.Gray.PackedValue));
		Add(new Line(620, 0, 1, 500, Color.Gray.PackedValue));
		int num2 = 10;
		Label label = new Label(ResGumps.MarkerIcon, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label.X = 5;
		label.Y = num2;
		Add(label);
		Label label2 = new Label(ResGumps.MarkerName, isunicode: true, ushort.MaxValue, 185, byte.MaxValue, FontStyle.BlackBorder);
		label2.X = 50;
		label2.Y = num2;
		Add(label2);
		Label label3 = new Label(ResGumps.MarkerX, isunicode: true, ushort.MaxValue, 35, byte.MaxValue, FontStyle.BlackBorder);
		label3.X = 315;
		label3.Y = num2;
		Add(label3);
		Label label4 = new Label(ResGumps.MarkerY, isunicode: true, ushort.MaxValue, 35, byte.MaxValue, FontStyle.BlackBorder);
		label4.X = 380;
		label4.Y = num2;
		Add(label4);
		Label label5 = new Label(ResGumps.MarkerColor, isunicode: true, ushort.MaxValue, 35, byte.MaxValue, FontStyle.BlackBorder);
		label5.X = 420;
		label5.Y = num2;
		Add(label5);
		Label label6 = new Label(ResGumps.Edit, isunicode: true, ushort.MaxValue, 35, byte.MaxValue, FontStyle.BlackBorder);
		label6.X = 475;
		label6.Y = num2;
		Add(label6);
		Label label7 = new Label(ResGumps.Remove, isunicode: true, ushort.MaxValue, 40, byte.MaxValue, FontStyle.BlackBorder);
		label7.X = 505;
		label7.Y = num2;
		Add(label7);
		Label label8 = new Label(ResGumps.MarkerGoTo, isunicode: true, ushort.MaxValue, 40, byte.MaxValue, FontStyle.BlackBorder);
		label8.X = 550;
		label8.Y = num2;
		Add(label8);
		Add(new Line(0, num2 + 20, 620, 1, Color.Gray.PackedValue));
		Add(_searchTextBox = new SearchTextBoxControl(160, 40));
		DrawArea(_markerFiles[_categoryId].IsEditable);
		int num3 = 0;
		foreach (WorldMapGump.WMapMarkerFile markerFile in _markerFiles)
		{
			NiceButton niceButton = new NiceButton(num * num3, 460, num, 40, ButtonAction.Activate, markerFile.Name, MARKERS_CATEGORY_GROUP_INDEX)
			{
				ButtonParameter = num3,
				IsSelectable = true
			};
			niceButton.SetTooltip(markerFile.Name);
			if (num3 == 0)
			{
				niceButton.IsSelected = true;
			}
			Add(niceButton);
			Add(new Line(niceButton.X, niceButton.Y, 1, niceButton.Height, Color.Gray.PackedValue));
			num3++;
		}
		Add(new Line(0, 460, 620, 1, Color.Gray.PackedValue));
		SetInScreen();
	}

	private void DrawArea(bool isEditable)
	{
		_scrollArea = new ScrollArea(10, 80, 600, 370, normalScrollbar: true);
		int num = 0;
		foreach (var item in _markers.Select((WorldMapGump.WMapMarker value, int idx) => new { idx, value }))
		{
			if (string.IsNullOrEmpty(_searchText) || item.value.Name.ToLower().Contains(_searchText.ToLower()))
			{
				MakerManagerControl makerManagerControl = new MakerManagerControl(item.value, num, item.idx, isEditable);
				makerManagerControl.RemoveMarkerEvent += MarkerRemoveEventHandler;
				makerManagerControl.EditMarkerEvent += MarkerEditEventHandler;
				_scrollArea.Add(makerManagerControl);
				num += 25;
			}
		}
		Add(_scrollArea);
	}

	private void MarkerRemoveEventHandler(object sender, EventArgs e)
	{
		if (sender is int index)
		{
			_markers.RemoveAt(index);
			Remove(_scrollArea);
			DrawArea(_markerFiles[_categoryId].IsEditable);
			_isMarkerListModified = true;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 100:
			if (_searchText.Equals(_searchTextBox.SearchText))
			{
				return;
			}
			_searchText = _searchTextBox.SearchText;
			break;
		case 101:
			_searchTextBox.ClearText();
			_searchText = "";
			break;
		default:
			_categoryId = buttonID;
			_markers = _markerFiles[buttonID].Markers;
			break;
		}
		_scrollArea.Clear();
		DrawArea(_markerFiles[_categoryId].IsEditable);
	}

	public override void OnKeyboardReturn(int textID, string text)
	{
		if (!_searchText.Equals(_searchTextBox.SearchText))
		{
			_scrollArea.Clear();
			_searchText = _searchTextBox.SearchText;
			DrawArea(_markerFiles[_categoryId].IsEditable);
		}
	}

	private void MarkerEditEventHandler(object sender, EventArgs e)
	{
		_isMarkerListModified = true;
	}

	public override void Dispose()
	{
		if (_isMarkerListModified)
		{
			using (StreamWriter streamWriter = new StreamWriter(_userMarkersFilePath, append: false))
			{
				foreach (WorldMapGump.WMapMarker marker in _markers)
				{
					string value = $"{marker.X},{marker.Y},{marker.MapId},{marker.Name},{marker.MarkerIconName},{marker.ColorName},4";
					streamWriter.WriteLine(value);
				}
			}
			_isMarkerListModified = false;
			WorldMapGump.ReloadUserMarkers();
		}
		base.Dispose();
	}
}
