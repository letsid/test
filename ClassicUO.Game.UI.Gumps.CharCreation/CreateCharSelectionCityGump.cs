using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class CreateCharSelectionCityGump : Gump
{
	private enum Buttons
	{
		PreviousScreen,
		Finish
	}

	private class CityControl : Control
	{
		private readonly Button _button;

		private bool _isSelected;

		private readonly HoveredLabel _label;

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					_label.IsSelected = value;
					_button.IsClicked = value;
				}
			}
		}

		public CityControl(CityInfo c, int x, int y, int index)
		{
			CanMove = false;
			Button obj = new Button(2 + index, 1209, 1210, 1210, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj.X = x;
			obj.Y = y;
			Button c2 = obj;
			_button = obj;
			Add(c2);
			y -= 20;
			HoveredLabel hoveredLabel = new HoveredLabel(c.City, isunicode: false, 88, 153, 1153, 0, 3);
			hoveredLabel.X = x;
			hoveredLabel.Y = y;
			hoveredLabel.Tag = index;
			_label = hoveredLabel;
			if (_label.X + _label.Width >= 383)
			{
				_label.X -= 60;
			}
			_label.MouseUp += delegate
			{
				_label.IsSelected = true;
				int num = (int)_label.Tag;
				OnButtonClick(num + 2);
			};
			Add(_label);
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (!_isSelected)
			{
				_button.IsClicked = _button.MouseIsOver || _label.MouseIsOver;
				_label.ForceHover = _button.MouseIsOver;
			}
		}

		public override bool Contains(int x, int y)
		{
			Control res = null;
			_label.HitTest(x, y, ref res);
			if (res != null)
			{
				return true;
			}
			_button.HitTest(x, y, ref res);
			return res != null;
		}
	}

	private readonly List<CityControl> _cityControls = new List<CityControl>();

	private readonly string[] _cityNames = new string[6] { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "Ter Mur" };

	private readonly Label _facetName;

	private readonly HtmlControl _htmlControl;

	private readonly LoginScene _scene;

	private CityInfo _selectedCity;

	private readonly byte _selectedProfession;

	private readonly Point[] _townButtonsText = new Point[9]
	{
		new Point(105, 130),
		new Point(245, 90),
		new Point(165, 200),
		new Point(395, 160),
		new Point(200, 305),
		new Point(335, 250),
		new Point(160, 395),
		new Point(100, 250),
		new Point(270, 130)
	};

	public CreateCharSelectionCityGump(byte profession, LoginScene scene)
		: base(0u, 0u)
	{
		CanMove = false;
		base.CanCloseWithRightClick = false;
		base.CanCloseWithEsc = false;
		_scene = scene;
		_selectedProfession = profession;
		CityInfo city;
		if (Client.Version >= ClientVersion.CV_70130)
		{
			city = scene.GetCity(0);
		}
		else
		{
			city = scene.GetCity(3);
			if (city == null)
			{
				city = scene.GetCity(0);
			}
		}
		if (city == null)
		{
			Log.Error(ResGumps.NoCityFound);
			Dispose();
			return;
		}
		uint num = 0u;
		if (city.IsNewCity)
		{
			num = city.Map;
		}
		Label label = new Label("", isunicode: true, 1153, 0, 0, FontStyle.BlackBorder);
		label.X = 240;
		label.Y = 440;
		_facetName = label;
		if (Client.Version >= ClientVersion.CV_70130)
		{
			Add(new GumpPic(62, 54, (ushort)(5593 + num), 0));
			Add(new GumpPic(57, 49, 5599, 0));
			_facetName.Text = _cityNames[num];
		}
		else
		{
			Add(new GumpPic(57, 49, 5528, 0));
			_facetName.IsVisible = false;
		}
		if (CUOEnviroment.IsOutlands)
		{
			_facetName.IsVisible = false;
		}
		Add(_facetName);
		Button button = new Button(0, 5537, 5539, 5538, "", 0);
		button.X = 586;
		button.Y = 445;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 5540, 5542, 5541, "", 0);
		button2.X = 610;
		button2.Y = 445;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		_htmlControl = new HtmlControl(452, 60, 175, 367, hasbackground: true, hasscrollbar: true, useflagscrollbar: false, city.Description, 0, ishtml: true, 1);
		Add(_htmlControl);
		if (CUOEnviroment.IsOutlands)
		{
			_htmlControl.IsVisible = false;
		}
		for (int i = 0; i < scene.Cities.Length; i++)
		{
			CityInfo city2 = scene.GetCity(i);
			if (city2 == null)
			{
				continue;
			}
			int x = 0;
			int y = 0;
			if (city2.IsNewCity)
			{
				uint num2 = city2.Map;
				if (num2 > 5)
				{
					num2 = 5u;
				}
				x = 62 + ClassicUO.Utility.MathHelper.PercetangeOf(MapLoader.Instance.MapsDefaultSize[num2, 0] - 2048, city2.X, 383);
				y = 54 + ClassicUO.Utility.MathHelper.PercetangeOf(MapLoader.Instance.MapsDefaultSize[num2, 1], city2.Y, 384);
			}
			else if (i < _townButtonsText.Length)
			{
				x = _townButtonsText[i].X;
				y = _townButtonsText[i].Y;
			}
			CityControl cityControl = new CityControl(city2, x, y, i);
			Add(cityControl);
			_cityControls.Add(cityControl);
			if (CUOEnviroment.IsOutlands)
			{
				cityControl.IsVisible = false;
			}
		}
		SetCity(city);
	}

	private void SetCity(int index)
	{
		SetCity(_scene.GetCity(index));
	}

	private void SetCity(CityInfo city)
	{
		if (city != null)
		{
			_selectedCity = city;
			_htmlControl.Text = city.Description;
			SetFacet(city.Map);
		}
	}

	private void SetFacet(uint index)
	{
		if (Client.Version >= ClientVersion.CV_70130)
		{
			if (index >= _cityNames.Length)
			{
				index = (uint)(_cityNames.Length - 1);
			}
			_facetName.Text = _cityNames[index];
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		CharCreationGump gump = UIManager.GetGump<CharCreationGump>(null);
		if (gump == null)
		{
			return;
		}
		switch ((Buttons)buttonID)
		{
		case Buttons.PreviousScreen:
			gump.StepBack((_selectedProfession <= 0) ? 1 : 2);
			break;
		case Buttons.Finish:
			if (_selectedCity != null)
			{
				gump.SetCity(_selectedCity.Index);
			}
			gump.CreateCharacter();
			gump.IsVisible = false;
			break;
		default:
			if (buttonID >= 2)
			{
				buttonID -= 2;
				SetCity(buttonID);
				SetSelectedLabel(buttonID);
			}
			break;
		}
	}

	private void SetSelectedLabel(int index)
	{
		for (int i = 0; i < _cityControls.Count; i++)
		{
			_cityControls[i].IsSelected = index == i;
		}
	}
}
