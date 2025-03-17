using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class CreateCharAppearanceGump : Gump
{
	private struct CharacterInfo
	{
		public bool IsFemale;

		public RaceType Race;
	}

	private enum Buttons
	{
		MaleButton,
		FemaleButton,
		HumanButton,
		Prev,
		Finish
	}

	private class ColorSelectedEventArgs : EventArgs
	{
		public Layer Layer { get; }

		private ushort[] Pallet { get; }

		public int SelectedIndex { get; }

		public ushort SelectedHue
		{
			get
			{
				if (Pallet == null || SelectedIndex < 0 || SelectedIndex >= Pallet.Length)
				{
					return ushort.MaxValue;
				}
				return Pallet[SelectedIndex];
			}
		}

		public ColorSelectedEventArgs(Layer layer, ushort[] pallet, int selectedIndex)
		{
			Layer = layer;
			Pallet = pallet;
			SelectedIndex = selectedIndex;
		}
	}

	private class CustomColorPicker : Control
	{
		private readonly int _cellH;

		private readonly int _cellW;

		private readonly ColorBox _colorPicker;

		private ColorPickerBox _colorPickerBox;

		private readonly int _columns;

		private readonly int _rows;

		private int _lastSelectedIndex;

		private readonly Layer _layer;

		private readonly ushort[] _pallet;

		public ushort HueSelected => _colorPicker.Hue;

		public event EventHandler<ColorSelectedEventArgs> ColorSelected;

		public CustomColorPicker(Layer layer, int label, ushort[] pallet, int rows, int columns)
		{
			base.Width = 121;
			base.Height = 25;
			_cellW = 125 / columns;
			_cellH = 280 / rows;
			_columns = columns;
			_rows = rows;
			_layer = layer;
			_pallet = pallet;
			bool isunicode;
			bool num = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
			byte font = (byte)(num ? 3u : 9u);
			ushort hue = (ushort)(num ? 65535u : 0u);
			Label label2 = new Label(ClilocLoader.Instance.GetString(label), isunicode, hue, 0, font);
			label2.X = 0;
			label2.Y = 0;
			Add(label2);
			ColorBox colorBox = new ColorBox(121, 23, (ushort)(((pallet == null) ? 1 : pallet[0]) + 1));
			colorBox.X = 1;
			colorBox.Y = 15;
			ColorBox c = colorBox;
			_colorPicker = colorBox;
			Add(c);
			_colorPicker.MouseUp += ColorPicker_MouseClick;
		}

		public void SetSelectedIndex(int index)
		{
			if (_colorPickerBox != null)
			{
				_colorPickerBox.SelectedIndex = index;
				SetCurrentHue();
			}
		}

		private void SetCurrentHue()
		{
			_colorPicker.Hue = _colorPickerBox.SelectedHue;
			_lastSelectedIndex = _colorPickerBox.SelectedIndex;
			_colorPickerBox.Dispose();
		}

		private void ColorPickerBoxOnMouseUp(object sender, MouseEventArgs e)
		{
			int num = e.X / _cellW;
			int num2 = e.Y / _cellH * _columns + num;
			if (num2 >= 0 && num2 < _colorPickerBox.Hues.Length)
			{
				this.ColorSelected?.Invoke(this, new ColorSelectedEventArgs(_layer, _colorPickerBox.Hues, num2));
				SetCurrentHue();
			}
		}

		private void ColorPicker_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtonType.Left)
			{
				_colorPickerBox?.Dispose();
				_colorPickerBox = null;
				if (_colorPickerBox == null)
				{
					_colorPickerBox = new ColorPickerBox(489, 141, _rows, _columns, _cellW, _cellH, _pallet)
					{
						IsModal = true,
						LayerOrder = UILayer.Over,
						ModalClickOutsideAreaClosesThisControl = true,
						ShowLivePreview = false,
						SelectedIndex = _lastSelectedIndex
					};
					UIManager.Add(_colorPickerBox);
					_colorPickerBox.ColorSelectedIndex += ColorPickerBoxOnColorSelectedIndex;
					_colorPickerBox.MouseUp += ColorPickerBoxOnMouseUp;
				}
			}
		}

		private void ColorPickerBoxOnColorSelectedIndex(object sender, EventArgs e)
		{
			this.ColorSelected?.Invoke(this, new ColorSelectedEventArgs(_layer, _colorPickerBox.Hues, _colorPickerBox.SelectedIndex));
		}
	}

	private PlayerMobile _character;

	private CharacterInfo _characterInfo;

	private readonly Button _humanRadio;

	private readonly Button _elfRadio;

	private readonly Button _gargoyleRadio;

	private readonly Button _maleRadio;

	private readonly Button _femaleRadio;

	private Combobox _hairCombobox;

	private Combobox _facialCombobox;

	private Label _hairLabel;

	private Label _facialLabel;

	private readonly StbTextBox _nameTextBox;

	private PaperDollInteractable _paperDoll;

	private readonly Button _finishButton;

	private readonly Dictionary<Layer, Tuple<int, ushort>> CurrentColorOption = new Dictionary<Layer, Tuple<int, ushort>>();

	private readonly Dictionary<Layer, int> CurrentOption = new Dictionary<Layer, int>
	{
		{
			Layer.Hair,
			1
		},
		{
			Layer.Beard,
			0
		}
	};

	private static readonly char[] _SpaceDashPeriodQuote = new char[4] { ' ', '-', '.', '\'' };

	private static string[] _StartDisallowed = new string[6] { "seer", "counselor", "gm", "admin", "lady", "lord" };

	private static readonly string[] _Disallowed = new string[75]
	{
		"jigaboo", "chigaboo", "wop", "kyke", "kike", "tit", "spic", "prick", "piss", "lezbo",
		"lesbo", "felatio", "dyke", "dildo", "chinc", "chink", "cunnilingus", "cum", "cocksucker", "cock",
		"clitoris", "clit", "ass", "hitler", "penis", "nigga", "nigger", "klit", "kunt", "jiz",
		"jism", "jerkoff", "jackoff", "goddamn", "fag", "blowjob", "bitch", "asshole", "dick", "pussy",
		"snatch", "cunt", "twat", "shit", "fuck", "tailor", "smith", "scholar", "rogue", "novice",
		"neophyte", "merchant", "medium", "master", "mage", "lb", "journeyman", "grandmaster", "fisherman", "expert",
		"chef", "carpenter", "british", "blackthorne", "blackthorn", "beggar", "archer", "apprentice", "adept", "gamemaster",
		"frozen", "squelched", "invulnerable", "osi", "origin"
	};

	public CreateCharAppearanceGump()
		: base(0u, 0u)
	{
		ResizePic resizePic = new ResizePic(3600);
		resizePic.X = 82;
		resizePic.Y = 125;
		resizePic.Width = 151;
		resizePic.Height = 310;
		Add(resizePic, 1);
		Add(new GumpPic(280, 53, 1801, 0), 1);
		Add(new GumpPic(240, 73, 1802, 0), 1);
		Add(new GumpPicTiled(248, 73, 215, 16, 1803), 1);
		Add(new GumpPic(463, 73, 1804, 0), 1);
		Add(new GumpPic(238, 98, 1800, 0), 1);
		ResizePic resizePic2 = new ResizePic(3600);
		resizePic2.X = 475;
		resizePic2.Y = 125;
		resizePic2.Width = 151;
		resizePic2.Height = 310;
		Add(resizePic2, 1);
		Button button = new Button(0, 1896, 1895, 0, "", 0);
		button.X = 425;
		button.Y = 435;
		button.ButtonAction = ButtonAction.Activate;
		Button c = button;
		_maleRadio = button;
		Add(c, 1);
		Button button2 = new Button(1, 1896, 1895, 0, "", 0);
		button2.X = 425;
		button2.Y = 455;
		button2.ButtonAction = ButtonAction.Activate;
		c = button2;
		_femaleRadio = button2;
		Add(c, 1);
		Button button3 = new Button(0, 1808, 1810, 1809, "", 0);
		button3.X = 445;
		button3.Y = 435;
		button3.ButtonAction = ButtonAction.Activate;
		Add(button3, 1);
		Button button4 = new Button(1, 1805, 1807, 1806, "", 0);
		button4.X = 445;
		button4.Y = 455;
		button4.ButtonAction = ButtonAction.Activate;
		Add(button4, 1);
		StbTextBox stbTextBox = new StbTextBox(5, 16, 200, isunicode: false, FontStyle.Fixed, 1);
		stbTextBox.X = 257;
		stbTextBox.Y = 65;
		stbTextBox.Width = 200;
		stbTextBox.Height = 20;
		StbTextBox c2 = stbTextBox;
		_nameTextBox = stbTextBox;
		Add(c2, 1);
		Button button5 = new Button(2, 1896, 1895, 0, "", 0);
		button5.X = 180;
		button5.Y = 435;
		button5.ButtonAction = ButtonAction.Activate;
		c = button5;
		_humanRadio = button5;
		Add(c, 1);
		Button button6 = new Button(2, 1794, 1796, 1795, "", 0);
		button6.X = 200;
		button6.Y = 435;
		button6.ButtonAction = ButtonAction.Activate;
		Add(button6, 1);
		Button button7 = new Button(3, 5537, 5539, 5538, "", 0);
		button7.X = 586;
		button7.Y = 445;
		button7.ButtonAction = ButtonAction.Activate;
		Add(button7, 1);
		Button button8 = new Button(4, 5540, 5542, 5541, "", 0);
		button8.X = 610;
		button8.Y = 445;
		button8.ButtonAction = ButtonAction.Activate;
		Add(button8);
		_maleRadio.IsClicked = true;
		_humanRadio.IsClicked = true;
		_characterInfo.IsFemale = false;
		_characterInfo.Race = RaceType.HUMAN;
		HandleGenreChange();
		HandleRaceChanged();
	}

	private void CreateCharacter(bool isFemale, RaceType race)
	{
		if (_character == null)
		{
			_character = new PlayerMobile(1u);
			World.Mobiles.Add(_character);
		}
		LinkedObject linkedObject = _character.Items;
		while (linkedObject != null)
		{
			LinkedObject next = linkedObject.Next;
			World.RemoveItem((Item)linkedObject, forceRemove: true);
			linkedObject = next;
		}
		_character.Clear();
		_character.Race = race;
		_character.IsFemale = isFemale;
		if (isFemale)
		{
			_character.Flags |= Flags.Female;
		}
		else
		{
			_character.Flags &= ~Flags.Female;
		}
		switch (race)
		{
		case RaceType.GARGOYLE:
		{
			_character.Graphic = (ushort)(isFemale ? 667 : 666);
			Item item = CreateItem(16385, CurrentColorOption[Layer.Shirt].Item2, Layer.Robe);
			_character.PushToBack(item);
			break;
		}
		case RaceType.ELF:
			if (isFemale)
			{
				_character.Graphic = 606;
				Item item = CreateItem(5904, 900, Layer.Shoes);
				_character.PushToBack(item);
				item = CreateItem(5425, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);
				_character.PushToBack(item);
				item = CreateItem(5400, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);
				_character.PushToBack(item);
			}
			else
			{
				_character.Graphic = 605;
				Item item = CreateItem(5904, 900, Layer.Shoes);
				_character.PushToBack(item);
				item = CreateItem(5423, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);
				_character.PushToBack(item);
				item = CreateItem(5400, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);
				_character.PushToBack(item);
			}
			break;
		default:
			if (isFemale)
			{
				_character.Graphic = 401;
				Item item = CreateItem(5904, 900, Layer.Shoes);
				_character.PushToBack(item);
				item = CreateItem(5425, CurrentColorOption[Layer.Pants].Item2, Layer.Skirt);
				_character.PushToBack(item);
				item = CreateItem(5400, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);
				_character.PushToBack(item);
			}
			else
			{
				_character.Graphic = 400;
				Item item = CreateItem(5904, 900, Layer.Shoes);
				_character.PushToBack(item);
				item = CreateItem(5423, CurrentColorOption[Layer.Pants].Item2, Layer.Pants);
				_character.PushToBack(item);
				item = CreateItem(5400, CurrentColorOption[Layer.Shirt].Item2, Layer.Shirt);
				_character.PushToBack(item);
			}
			break;
		}
	}

	private void UpdateEquipments()
	{
		RaceType race = _characterInfo.Race;
		_character.Hue = CurrentColorOption[Layer.Invalid].Item2;
		Layer layer;
		CharacterCreationValues.ComboContent facialHairComboContent;
		if (!_characterInfo.IsFemale && race != RaceType.ELF)
		{
			layer = Layer.Beard;
			facialHairComboContent = CharacterCreationValues.GetFacialHairComboContent(race);
			Item item = CreateItem(facialHairComboContent.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);
			_character.PushToBack(item);
		}
		layer = Layer.Hair;
		facialHairComboContent = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);
		Item item2 = CreateItem(facialHairComboContent.GetGraphic(CurrentOption[layer]), CurrentColorOption[layer].Item2, layer);
		_character.PushToBack(item2);
	}

	private void HandleRaceChanged()
	{
		CurrentColorOption.Clear();
		HandleGenreChange();
		_ = _characterInfo;
		_ = World.ClientFeatures.Flags;
		_ = World.ClientLockedFeatures.Flags;
	}

	private void HandleGenreChange()
	{
		RaceType race = _characterInfo.Race;
		CurrentOption[Layer.Beard] = 0;
		CurrentOption[Layer.Hair] = 1;
		if (_paperDoll != null)
		{
			Remove(_paperDoll);
		}
		if (_hairCombobox != null)
		{
			Remove(_hairCombobox);
			Remove(_hairLabel);
		}
		if (_facialCombobox != null)
		{
			Remove(_facialCombobox);
			Remove(_facialLabel);
		}
		foreach (CustomColorPicker item in base.Children.OfType<CustomColorPicker>().ToList())
		{
			Remove(item);
		}
		CharacterCreationValues.ComboContent hairComboContent = CharacterCreationValues.GetHairComboContent(_characterInfo.IsFemale, race);
		bool isunicode;
		bool num = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
		byte font = (byte)(num ? 3u : 9u);
		ushort hue = (ushort)(num ? 65535u : 0u);
		Label label = new Label(ClilocLoader.Instance.GetString((race == RaceType.GARGOYLE) ? 1112309 : 3000121), isunicode, hue, 0, font);
		label.X = 98;
		label.Y = 140;
		Label c = label;
		_hairLabel = label;
		Add(c, 1);
		Add(_hairCombobox = new Combobox(97, 155, 120, hairComboContent.Labels, CurrentOption[Layer.Hair], 200, showArrow: true, "", 9), 1);
		_hairCombobox.OnOptionSelected += Hair_OnOptionSelected;
		if (!_characterInfo.IsFemale && race != RaceType.ELF)
		{
			hairComboContent = CharacterCreationValues.GetFacialHairComboContent(race);
			Label label2 = new Label(ClilocLoader.Instance.GetString((race == RaceType.GARGOYLE) ? 1112511 : 3000122), isunicode, hue, 0, font);
			label2.X = 98;
			label2.Y = 184;
			c = label2;
			_facialLabel = label2;
			Add(c, 1);
			Add(_facialCombobox = new Combobox(97, 199, 120, hairComboContent.Labels, CurrentOption[Layer.Beard], 200, showArrow: true, "", 9), 1);
			_facialCombobox.OnOptionSelected += Facial_OnOptionSelected;
		}
		else
		{
			_facialCombobox = null;
			_facialLabel = null;
		}
		ushort[] skinPallet = CharacterCreationValues.GetSkinPallet(race);
		AddCustomColorPicker(489, 141, skinPallet, Layer.Invalid, 3000183, 8, skinPallet.Length >> 3);
		AddCustomColorPicker(489, 183, null, Layer.Shirt, 3000440, 10, 20);
		if (race != RaceType.GARGOYLE)
		{
			AddCustomColorPicker(489, 225, null, Layer.Pants, 3000441, 10, 20);
		}
		skinPallet = CharacterCreationValues.GetHairPallet(race);
		AddCustomColorPicker(489, 267, skinPallet, Layer.Hair, (race == RaceType.GARGOYLE) ? 1112322 : 3000184, 8, skinPallet.Length >> 3);
		if (!_characterInfo.IsFemale && race != RaceType.ELF)
		{
			skinPallet = CharacterCreationValues.GetHairPallet(race);
			AddCustomColorPicker(489, 309, skinPallet, Layer.Beard, (race == RaceType.GARGOYLE) ? 1112512 : 3000446, 8, skinPallet.Length >> 3);
		}
		CreateCharacter(_characterInfo.IsFemale, race);
		UpdateEquipments();
		Add(_paperDoll = new PaperDollInteractable(262, 135, _character, null)
		{
			AcceptMouseInput = false
		}, 1);
		_paperDoll.Update();
	}

	private void AddCustomColorPicker(int x, int y, ushort[] pallet, Layer layer, int clilocLabel, int rows, int columns)
	{
		CustomColorPicker customColorPicker = new CustomColorPicker(layer, clilocLabel, pallet, rows, columns);
		customColorPicker.X = x;
		customColorPicker.Y = y;
		CustomColorPicker customColorPicker2 = customColorPicker;
		Add(customColorPicker, 1);
		if (!CurrentColorOption.ContainsKey(layer))
		{
			CurrentColorOption[layer] = new Tuple<int, ushort>(0, customColorPicker2.HueSelected);
		}
		else
		{
			customColorPicker2.SetSelectedIndex(CurrentColorOption[layer].Item1);
		}
		customColorPicker2.ColorSelected += ColorPicker_ColorSelected;
	}

	private void ColorPicker_ColorSelected(object sender, ColorSelectedEventArgs e)
	{
		if (e.SelectedIndex == 65535)
		{
			return;
		}
		CurrentColorOption[e.Layer] = new Tuple<int, ushort>(e.SelectedIndex, e.SelectedHue);
		if (e.Layer != 0)
		{
			Item item = ((_character.Race != RaceType.GARGOYLE || e.Layer != Layer.Shirt) ? _character.FindItemByLayer((_characterInfo.IsFemale && e.Layer == Layer.Pants) ? Layer.Skirt : e.Layer) : _character.FindItemByLayer(Layer.Robe));
			if (item != null)
			{
				item.Hue = e.SelectedHue;
			}
		}
		else
		{
			_character.Hue = e.SelectedHue;
		}
		_paperDoll.Update();
	}

	private void Facial_OnOptionSelected(object sender, int e)
	{
		CurrentOption[Layer.Beard] = e;
		UpdateEquipments();
		_paperDoll.Update();
	}

	private void Hair_OnOptionSelected(object sender, int e)
	{
		CurrentOption[Layer.Hair] = e;
		UpdateEquipments();
		_paperDoll.Update();
	}

	public override void OnButtonClick(int buttonID)
	{
		CharCreationGump gump = UIManager.GetGump<CharCreationGump>(null);
		_characterInfo.Race = RaceType.HUMAN;
		switch ((Buttons)buttonID)
		{
		case Buttons.FemaleButton:
			_femaleRadio.IsClicked = true;
			_maleRadio.IsClicked = false;
			_characterInfo.IsFemale = true;
			HandleGenreChange();
			break;
		case Buttons.MaleButton:
			_maleRadio.IsClicked = true;
			_femaleRadio.IsClicked = false;
			_characterInfo.IsFemale = false;
			HandleGenreChange();
			break;
		case Buttons.Finish:
			_character.Name = _nameTextBox.Text;
			if (ValidateCharacter(_character))
			{
				gump.SetCharacter(_character);
				gump.CreateCharacter();
				gump.IsVisible = false;
			}
			break;
		case Buttons.Prev:
			gump.StepBack();
			break;
		}
		base.OnButtonClick(buttonID);
	}

	private bool ValidateCharacter(PlayerMobile character)
	{
		int num = Validate(character.Name);
		if (num > 0)
		{
			UIManager.GetGump<CharCreationGump>(null)?.ShowMessage(ClilocLoader.Instance.GetString(num));
			return false;
		}
		return true;
	}

	public static int Validate(string name)
	{
		return Validate(name, 2, 16, allowLetters: true, allowDigits: false, noExceptionsAtStart: true, 1, _SpaceDashPeriodQuote, (Client.Version >= ClientVersion.CV_5020) ? _Disallowed : new string[0], _StartDisallowed);
	}

	public static int Validate(string name, int minLength, int maxLength, bool allowLetters, bool allowDigits, bool noExceptionsAtStart, int maxExceptions, char[] exceptions, string[] disallowed, string[] startDisallowed)
	{
		if (string.IsNullOrEmpty(name) || name.Length < minLength)
		{
			return 3000612;
		}
		if (name.Length > maxLength)
		{
			return 3000611;
		}
		int num = 0;
		name = name.ToLowerInvariant();
		if (!allowLetters || !allowDigits || (exceptions.Length != 0 && (noExceptionsAtStart || maxExceptions < int.MaxValue)))
		{
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				if (c >= 'a' && c <= 'z')
				{
					num = 0;
					continue;
				}
				if (c >= '0' && c <= '9')
				{
					if (!allowDigits)
					{
						return 3000611;
					}
					num = 0;
					continue;
				}
				bool flag = false;
				int num2 = 0;
				while (!flag && num2 < exceptions.Length)
				{
					if (c == exceptions[num2])
					{
						flag = true;
					}
					num2++;
				}
				if (!flag || (i == 0 && noExceptionsAtStart))
				{
					return 3000611;
				}
				if (num++ == maxExceptions)
				{
					return 3000611;
				}
			}
		}
		for (int j = 0; j < disallowed.Length; j++)
		{
			int num3 = name.IndexOf(disallowed[j]);
			if (num3 == -1)
			{
				continue;
			}
			bool flag2 = num3 == 0;
			int num4 = 0;
			while (!flag2 && num4 < exceptions.Length)
			{
				flag2 = name[num3 - 1] == exceptions[num4];
				num4++;
			}
			if (flag2)
			{
				bool flag3 = num3 + disallowed[j].Length >= name.Length;
				int num5 = 0;
				while (!flag3 && num5 < exceptions.Length)
				{
					flag3 = name[num3 + disallowed[j].Length] == exceptions[num5];
					num5++;
				}
				if (flag3)
				{
					return 3000611;
				}
			}
		}
		for (int k = 0; k < startDisallowed.Length; k++)
		{
			if (name.StartsWith(startDisallowed[k]))
			{
				return 3000611;
			}
		}
		return 0;
	}

	private Item CreateItem(int id, ushort hue, Layer layer)
	{
		Item item = _character.FindItemByLayer(layer);
		if (item != null)
		{
			World.RemoveItem(item, forceRemove: true);
			_character.Remove(item);
		}
		if (id == 0)
		{
			return null;
		}
		Item orCreateItem = World.GetOrCreateItem(1073741824u + (uint)layer);
		_character.Remove(orCreateItem);
		orCreateItem.Graphic = (ushort)id;
		orCreateItem.Hue = hue;
		orCreateItem.Layer = layer;
		orCreateItem.Container = _character;
		return orCreateItem;
	}
}
