using System;
using System.Collections.Generic;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class HouseCustomizationGump : Gump
{
	private enum ID_GUMP_CUSTOM_HOUSE
	{
		ID_GCH_STATE_WALL = 1,
		ID_GCH_STATE_DOOR,
		ID_GCH_STATE_FLOOR,
		ID_GCH_STATE_STAIR,
		ID_GCH_STATE_ROOF,
		ID_GCH_STATE_MISC,
		ID_GCH_STATE_ERASE,
		ID_GCH_STATE_EYEDROPPER,
		ID_GCH_STATE_MENU,
		ID_GCH_VISIBILITY_STORY_1,
		ID_GCH_VISIBILITY_STORY_2,
		ID_GCH_VISIBILITY_STORY_3,
		ID_GCH_VISIBILITY_STORY_4,
		ID_GCH_GO_FLOOR_1,
		ID_GCH_GO_FLOOR_2,
		ID_GCH_GO_FLOOR_3,
		ID_GCH_GO_FLOOR_4,
		ID_GCH_LIST_LEFT,
		ID_GCH_LIST_RIGHT,
		ID_GCH_MENU_BACKUP,
		ID_GCH_MENU_RESTORE,
		ID_GCH_MENU_SYNCH,
		ID_GCH_MENU_CLEAR,
		ID_GCH_MENU_COMMIT,
		ID_GCH_MENU_REVERT,
		ID_GCH_GO_CATEGORY,
		ID_GCH_WALL_SHOW_WINDOW,
		ID_GCH_ROOF_Z_UP,
		ID_GCH_ROOF_Z_DOWN,
		ID_GCH_AREA_OBJECTS_INFO,
		ID_GCH_AREA_COST_INFO,
		ID_GCH_AREA_ROOF_Z_INFO,
		ID_GCH_ITEM_IN_LIST
	}

	private readonly HouseCustomizationManager _customHouseManager;

	private readonly DataBox _dataBox;

	private readonly DataBox _dataBoxGUI;

	private readonly GumpPic _gumpPic;

	private readonly Label _textComponents;

	private readonly Label _textCost;

	private readonly Label _textFixtures;

	public HouseCustomizationGump(uint serial, int x, int y)
		: base(serial, 0u)
	{
		base.X = x;
		base.Y = y;
		CanMove = true;
		AcceptMouseInput = false;
		base.CanCloseWithRightClick = true;
		_customHouseManager = new HouseCustomizationManager(serial);
		World.CustomHouseManager = _customHouseManager;
		Add(new GumpPicTiled(121, 36, 397, 120, 3604));
		_dataBox = new DataBox(0, 0, 0, 0)
		{
			WantUpdateSize = true,
			CanMove = false,
			AcceptMouseInput = false
		};
		Add(_dataBox);
		Add(new GumpPic(0, 17, 22000, 0));
		_gumpPic = new GumpPic(486, 17, (ushort)((_customHouseManager.FloorCount == 4) ? 22002u : 22009u), 0);
		Add(_gumpPic);
		Add(new GumpPicTiled(153, 17, 333, 154, 22001));
		Button button = new Button(1, 22100, 22102, 22101, "", 0);
		button.X = 9;
		button.Y = 41;
		button.ButtonAction = ButtonAction.Activate;
		Button button2 = button;
		button2.SetTooltip(ResGumps.Walls);
		Add(button2);
		Button button3 = new Button(2, 22103, 22105, 22104, "", 0);
		button3.X = 39;
		button3.Y = 40;
		button3.ButtonAction = ButtonAction.Activate;
		button2 = button3;
		button2.SetTooltip(ResGumps.Doors);
		Add(button2);
		Button button4 = new Button(3, 22106, 22108, 22107, "", 0);
		button4.X = 70;
		button4.Y = 40;
		button4.ButtonAction = ButtonAction.Activate;
		button2 = button4;
		button2.SetTooltip(ResGumps.Floors);
		Add(button2);
		Button button5 = new Button(4, 22109, 22111, 22110, "", 0);
		button5.X = 9;
		button5.Y = 72;
		button5.ButtonAction = ButtonAction.Activate;
		button2 = button5;
		button2.SetTooltip(ResGumps.Stairs);
		Add(button2);
		Button button6 = new Button(5, 22408, 22410, 22409, "", 0);
		button6.X = 39;
		button6.Y = 72;
		button6.ButtonAction = ButtonAction.Activate;
		button2 = button6;
		button2.SetTooltip(ResGumps.Roofs);
		Add(button2);
		Button button7 = new Button(6, 22115, 22117, 22116, "", 0);
		button7.X = 69;
		button7.Y = 72;
		button7.ButtonAction = ButtonAction.Activate;
		button2 = button7;
		button2.SetTooltip(ResGumps.Miscellaneous);
		Add(button2);
		Button button8 = new Button(9, 22124, 22126, 22125, "", 0);
		button8.X = 69;
		button8.Y = 100;
		button8.ButtonAction = ButtonAction.Activate;
		button2 = button8;
		button2.SetTooltip(ResGumps.SystemMenu);
		Add(button2);
		Label label = new Label(string.Empty, isunicode: false, 1153, 0, 9);
		label.X = 82;
		label.Y = 142;
		label.AcceptMouseInput = true;
		_textComponents = label;
		Add(_textComponents);
		Label label2 = new Label(":", isunicode: false, 1153, 0, 9);
		label2.X = 84;
		label2.Y = 142;
		Label c = label2;
		Add(c);
		Label label3 = new Label(string.Empty, isunicode: false, 1153, 0, 9);
		label3.X = 94;
		label3.Y = 142;
		label3.AcceptMouseInput = true;
		_textFixtures = label3;
		Add(_textFixtures);
		Label label4 = new Label(string.Empty, isunicode: false, 1153, 0, 9);
		label4.X = 524;
		label4.Y = 142;
		label4.AcceptMouseInput = true;
		_textCost = label4;
		_textCost.SetTooltip(ResGumps.Cost);
		Add(_textCost);
		_dataBoxGUI = new DataBox(0, 0, 0, 0)
		{
			WantUpdateSize = true,
			CanMove = false,
			AcceptMouseInput = false
		};
		Add(_dataBoxGUI);
		UpdateMaxPage();
		Update();
	}

	public void Update()
	{
		_dataBox.Clear();
		_dataBoxGUI.Clear();
		_gumpPic.Graphic = (ushort)((_customHouseManager.FloorCount == 4) ? 22002u : 22009u);
		Button button = new Button(7, (ushort)(22118u + (_customHouseManager.Erasing ? 1u : 0u)), 22120, 22119, "", 0);
		button.X = 9;
		button.Y = 100;
		button.ButtonAction = ButtonAction.Activate;
		Button button2 = button;
		button2.SetTooltip(ResGumps.Erase);
		_dataBoxGUI.Add(button2);
		Button button3 = new Button(8, (ushort)(22121u + (_customHouseManager.SeekTile ? 1u : 0u)), 22123, 22122, "", 0);
		button3.X = 39;
		button3.Y = 100;
		button3.ButtonAction = ButtonAction.Activate;
		button2 = button3;
		button2.SetTooltip(ResGumps.EyedropperTool);
		Add(button2);
		ushort[] array = new ushort[3] { 22318, 22324, 22321 };
		ushort[] array2 = new ushort[3] { 22309, 22312, 22315 };
		_ = new ushort[3] { 22300, 22303, 22306 };
		int[] array3 = new int[7] { 0, 1, 2, 1, 2, 1, 2 };
		ushort num = array[array3[_customHouseManager.FloorVisionState[0]]];
		int num2 = ((_customHouseManager.CurrentFloor == 1) ? 3 : 0);
		int num3 = ((_customHouseManager.CurrentFloor == 1) ? 4 : 0);
		Button button4 = new Button(10, num, (ushort)(num + 2), (ushort)(num + 1), "", 0);
		button4.X = 533;
		button4.Y = 108;
		button4.ButtonAction = ButtonAction.Activate;
		button2 = button4;
		button2.SetTooltip(string.Format(ResGumps.Store0Visibility, 1));
		_dataBoxGUI.Add(button2);
		Button button5 = new Button(14, (ushort)(22221 + num3), 22225, (ushort)(22221 + num3), "", 0);
		button5.X = 583;
		button5.Y = 96;
		button5.ButtonAction = ButtonAction.Activate;
		button2 = button5;
		button2.SetTooltip(string.Format(ResGumps.GoToStory0, 1));
		_dataBoxGUI.Add(button2);
		Button button6 = new Button(14, (ushort)(22262 + num2), (ushort)(22264 + num2), (ushort)(22263 + num2), "", 0);
		button6.X = 623;
		button6.Y = 103;
		button6.ButtonAction = ButtonAction.Activate;
		button2 = button6;
		button2.SetTooltip(string.Format(ResGumps.GoToStory0, 1));
		_dataBoxGUI.Add(button2);
		num = array2[array3[_customHouseManager.FloorVisionState[1]]];
		num2 = ((_customHouseManager.CurrentFloor == 2) ? 3 : 0);
		num3 = ((_customHouseManager.CurrentFloor == 2) ? 4 : 0);
		Button button7 = new Button(11, num, (ushort)(num + 2), (ushort)(num + 1), "", 0);
		button7.X = 533;
		button7.Y = 86;
		button7.ButtonAction = ButtonAction.Activate;
		button2 = button7;
		button2.SetTooltip(string.Format(ResGumps.Store0Visibility, 2));
		_dataBoxGUI.Add(button2);
		Button button8 = new Button(15, (ushort)(22222 + num3), 22226, (ushort)(22222 + num3), "", 0);
		button8.X = 583;
		button8.Y = 73;
		button8.ButtonAction = ButtonAction.Activate;
		button2 = button8;
		button2.SetTooltip(string.Format(ResGumps.GoToStory0, 2));
		_dataBoxGUI.Add(button2);
		Button button9 = new Button(15, (ushort)(22256 + num2), (ushort)(22258 + num2), (ushort)(22257 + num2), "", 0);
		button9.X = 623;
		button9.Y = 86;
		button9.ButtonAction = ButtonAction.Activate;
		button2 = button9;
		button2.SetTooltip(string.Format(ResGumps.GoToStory0, 2));
		_dataBoxGUI.Add(button2);
		num2 = ((_customHouseManager.CurrentFloor == 3) ? 3 : 0);
		num3 = ((_customHouseManager.CurrentFloor == 3) ? 4 : 0);
		if (_customHouseManager.FloorCount == 4)
		{
			num = array2[array3[_customHouseManager.FloorVisionState[2]]];
			Button button10 = new Button(12, num, (ushort)(num + 2), (ushort)(num + 1), "", 0);
			button10.X = 533;
			button10.Y = 64;
			button10.ButtonAction = ButtonAction.Activate;
			button2 = button10;
			button2.SetTooltip(string.Format(ResGumps.Store0Visibility, 3));
			_dataBoxGUI.Add(button2);
			Button button11 = new Button(16, (ushort)(22222 + num3), 22226, (ushort)(22222 + num3), "", 0);
			button11.X = 582;
			button11.Y = 56;
			button11.ButtonAction = ButtonAction.Activate;
			button2 = button11;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 3));
			_dataBoxGUI.Add(button2);
			Button button12 = new Button(16, (ushort)(22256 + num2), (ushort)(22258 + num2), (ushort)(22257 + num2), "", 0);
			button12.X = 623;
			button12.Y = 69;
			button12.ButtonAction = ButtonAction.Activate;
			button2 = button12;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 3));
			_dataBoxGUI.Add(button2);
			num = array2[array3[_customHouseManager.FloorVisionState[3]]];
			num2 = ((_customHouseManager.CurrentFloor == 4) ? 3 : 0);
			num3 = ((_customHouseManager.CurrentFloor == 4) ? 4 : 0);
			Button button13 = new Button(13, num, (ushort)(num + 2), (ushort)(num + 1), "", 0);
			button13.X = 533;
			button13.Y = 42;
			button13.ButtonAction = ButtonAction.Activate;
			button2 = button13;
			button2.SetTooltip(string.Format(ResGumps.Store0Visibility, 4));
			_dataBoxGUI.Add(button2);
			Button button14 = new Button(17, (ushort)(22224 + num3), 22228, (ushort)(22224 + num3), "", 0);
			button14.X = 583;
			button14.Y = 42;
			button14.ButtonAction = ButtonAction.Activate;
			button2 = button14;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 4));
			_dataBoxGUI.Add(button2);
			Button button15 = new Button(17, (ushort)(22250 + num2), (ushort)(22252 + num2), (ushort)(22251 + num2), "", 0);
			button15.X = 623;
			button15.Y = 50;
			button15.ButtonAction = ButtonAction.Activate;
			button2 = button15;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 4));
			_dataBoxGUI.Add(button2);
		}
		else
		{
			num = array2[array3[_customHouseManager.FloorVisionState[2]]];
			Button button16 = new Button(12, num, (ushort)(num + 2), (ushort)(num + 1), "", 0);
			button16.X = 533;
			button16.Y = 64;
			button16.ButtonAction = ButtonAction.Activate;
			button2 = button16;
			button2.SetTooltip(string.Format(ResGumps.Store0Visibility, 3));
			_dataBoxGUI.Add(button2);
			Button button17 = new Button(16, (ushort)(22224 + num3), 22228, (ushort)(22224 + num3), "", 0);
			button17.X = 582;
			button17.Y = 56;
			button17.ButtonAction = ButtonAction.Activate;
			button2 = button17;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 3));
			_dataBoxGUI.Add(button2);
			Button button18 = new Button(16, (ushort)(22250 + num2), (ushort)(22252 + num2), (ushort)(22251 + num2), "", 0);
			button18.X = 623;
			button18.Y = 69;
			button18.ButtonAction = ButtonAction.Activate;
			button2 = button18;
			button2.SetTooltip(string.Format(ResGumps.GoToStory0, 3));
			_dataBoxGUI.Add(button2);
		}
		switch (_customHouseManager.State)
		{
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL:
			AddWall();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR:
			AddDoor();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR:
			AddFloor();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR:
			AddStair();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF:
			AddRoof();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC:
			AddMisc();
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_MENU:
			AddMenu();
			break;
		}
		if (_customHouseManager.MaxPage > 1)
		{
			Button button19 = new Button(18, 22053, 22055, 22054, "", 0);
			button19.X = 110;
			button19.Y = 63;
			button19.ButtonAction = ButtonAction.Activate;
			button2 = button19;
			button2.SetTooltip(ResGumps.PreviousPage);
			_dataBoxGUI.Add(button2);
			Button button20 = new Button(19, 22056, 22058, 22057, "", 0);
			button20.X = 510;
			button20.Y = 63;
			button20.ButtonAction = ButtonAction.Activate;
			button2 = button20;
			button2.SetTooltip(ResGumps.NextPage);
			_dataBoxGUI.Add(button2);
		}
		_customHouseManager.Components = 0;
		_customHouseManager.Fixtures = 0;
		if (World.Items.Get(base.LocalSerial) != null && World.HouseManager.TryGetHouse(base.LocalSerial, out var house))
		{
			foreach (Multi component in house.Components)
			{
				if (!component.IsCustom || (component.State & CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL) != 0)
				{
					continue;
				}
				CUSTOM_HOUSE_GUMP_STATE state = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;
				var (num4, num5) = _customHouseManager.ExistsInList(ref state, component.Graphic);
				if (num4 != -1 && num5 != -1)
				{
					if (state == CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR || state == CUSTOM_HOUSE_GUMP_STATE.CHGS_FIXTURE)
					{
						_customHouseManager.Fixtures++;
					}
					else
					{
						_customHouseManager.Components++;
					}
				}
			}
		}
		_textComponents.Hue = (ushort)((_customHouseManager.Components >= _customHouseManager.MaxComponets) ? 38u : 1153u);
		_textComponents.Text = _customHouseManager.Components.ToString();
		_textComponents.X = 82 - _textComponents.Width;
		_textFixtures.Hue = (ushort)((_customHouseManager.Fixtures >= _customHouseManager.MaxFixtures) ? 38u : 1153u);
		_textFixtures.Text = _customHouseManager.Fixtures.ToString();
		string text = ClilocLoader.Instance.Translate(1061039, $"{_customHouseManager.MaxComponets}\t{_customHouseManager.MaxFixtures}", capitalize: true);
		_textComponents.SetTooltip(text);
		_textFixtures.SetTooltip(text);
		_textCost.Text = ((_customHouseManager.Components + _customHouseManager.Fixtures) * 500).ToString();
	}

	public void UpdateMaxPage()
	{
		_customHouseManager.MaxPage = 1;
		switch (_customHouseManager.State)
		{
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL:
			if (_customHouseManager.Category == -1)
			{
				_customHouseManager.MaxPage = (int)Math.Ceiling((float)HouseCustomizationManager.Walls.Count / 16f);
				break;
			}
			{
				foreach (CustomHouseWallCategory wall in HouseCustomizationManager.Walls)
				{
					if (wall.Index == _customHouseManager.Category)
					{
						_customHouseManager.MaxPage = wall.Items.Count;
						break;
					}
				}
				break;
			}
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR:
			_customHouseManager.MaxPage = HouseCustomizationManager.Doors.Count;
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR:
			_customHouseManager.MaxPage = HouseCustomizationManager.Floors.Count;
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR:
			_customHouseManager.MaxPage = HouseCustomizationManager.Stairs.Count;
			break;
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF:
			if (_customHouseManager.Category == -1)
			{
				_customHouseManager.MaxPage = (int)Math.Ceiling((float)HouseCustomizationManager.Roofs.Count / 16f);
				break;
			}
			{
				foreach (CustomHouseRoofCategory roof in HouseCustomizationManager.Roofs)
				{
					if (roof.Index == _customHouseManager.Category)
					{
						_customHouseManager.MaxPage = roof.Items.Count;
						break;
					}
				}
				break;
			}
		case CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC:
			if (_customHouseManager.Category == -1)
			{
				_customHouseManager.MaxPage = (int)Math.Ceiling((float)HouseCustomizationManager.Miscs.Count / 16f);
				break;
			}
			{
				foreach (CustomHouseMiscCategory misc in HouseCustomizationManager.Miscs)
				{
					if (misc.Index == _customHouseManager.Category)
					{
						_customHouseManager.MaxPage = misc.Items.Count;
						break;
					}
				}
				break;
			}
		}
	}

	private void AddWall()
	{
		int num = 0;
		int num2 = 0;
		if (_customHouseManager.Category == -1)
		{
			int num3 = base.Page * 16;
			int num4 = num3 + 16;
			if (num4 > HouseCustomizationManager.Walls.Count)
			{
				num4 = HouseCustomizationManager.Walls.Count;
			}
			_dataBox.Add(new ScissorControl(enabled: true, 121, 36, 384, 60));
			for (int i = num3; i < num4; i++)
			{
				List<CustomHouseWall> items = HouseCustomizationManager.Walls[i].Items;
				if (items.Count != 0)
				{
					ArtLoader.Instance.GetStaticTexture((ushort)items[0].East1, out var bounds);
					int num5 = num + 121 + (48 - bounds.Width) / 2;
					int num6 = num2 + 36;
					StaticPic staticPic = new StaticPic((ushort)items[0].East1, 0);
					staticPic.X = num5;
					staticPic.Y = num6;
					staticPic.CanMove = false;
					staticPic.LocalSerial = (uint)(33 + i);
					staticPic.Height = 60;
					StaticPic pic = staticPic;
					pic.MouseUp += delegate
					{
						OnButtonClick((int)pic.LocalSerial);
					};
					_dataBox.Add(pic);
					num += 48;
					if (num >= 384)
					{
						num = 0;
						num2 += 60;
						_dataBox.Add(new ScissorControl(enabled: false));
						_dataBox.Add(new ScissorControl(enabled: true, 121, 96, 384, 60));
					}
				}
			}
			_dataBox.Add(new ScissorControl(enabled: false));
		}
		else
		{
			if (_customHouseManager.Category < 0 || _customHouseManager.Category >= HouseCustomizationManager.Walls.Count)
			{
				return;
			}
			List<CustomHouseWall> items2 = HouseCustomizationManager.Walls[_customHouseManager.Category].Items;
			if (base.Page >= 0 && base.Page < items2.Count)
			{
				CustomHouseWall customHouseWall = items2[base.Page];
				_dataBox.Add(new ScissorControl(enabled: true, 121, 36, 384, 120));
				for (int j = 0; j < 8; j++)
				{
					ushort num7 = (_customHouseManager.ShowWindow ? customHouseWall.WindowGraphics[j] : customHouseWall.Graphics[j]);
					if (num7 != 0)
					{
						ArtLoader.Instance.GetStaticTexture(num7, out var bounds2);
						int num8 = num + 130 + (48 - bounds2.Width) / 2;
						int num9 = num2 + 36 + (120 - bounds2.Height) / 2;
						StaticPic staticPic2 = new StaticPic(num7, 0);
						staticPic2.X = num8;
						staticPic2.Y = num9;
						staticPic2.CanMove = false;
						staticPic2.LocalSerial = (uint)(33 + j);
						staticPic2.Height = 120;
						StaticPic pic2 = staticPic2;
						pic2.MouseUp += delegate
						{
							OnButtonClick((int)pic2.LocalSerial);
						};
						_dataBox.Add(pic2);
					}
					num += 48;
				}
				_dataBox.Add(new ScissorControl(enabled: false));
			}
			_dataBoxGUI.Add(new GumpPic(152, 0, 22003, 0));
			Button button = new Button(26, 22050, 22052, 22051, "", 0);
			button.X = 167;
			button.Y = 5;
			button.ButtonAction = ButtonAction.Activate;
			Button button2 = button;
			button2.SetTooltip(ResGumps.ToCustomHouseManagerCategory);
			_dataBoxGUI.Add(button2);
			_dataBoxGUI.Add(new GumpPic(218, 4, 22004, 0));
			if (_customHouseManager.ShowWindow)
			{
				Button button3 = new Button(27, 22062, 22064, 22063, "", 0);
				button3.X = 228;
				button3.Y = 9;
				button3.ButtonAction = ButtonAction.Activate;
				button2 = button3;
				button2.SetTooltip(ResGumps.WindowToggle);
				_dataBoxGUI.Add(button2);
			}
			else
			{
				Button button4 = new Button(27, 22059, 22061, 22060, "", 0);
				button4.X = 228;
				button4.Y = 9;
				button4.ButtonAction = ButtonAction.Activate;
				button2 = button4;
				button2.SetTooltip(ResGumps.WindowToggle);
				_dataBoxGUI.Add(button2);
			}
		}
	}

	private void AddDoor()
	{
		if (base.Page < 0 || base.Page >= HouseCustomizationManager.Doors.Count)
		{
			return;
		}
		CustomHouseDoor customHouseDoor = HouseCustomizationManager.Doors[base.Page];
		int num = 0;
		int num2 = 0;
		_dataBox.Add(new ScissorControl(enabled: true, 138, 36, 384, 120));
		for (int i = 0; i < 8; i++)
		{
			ushort num3 = customHouseDoor.Graphics[i];
			if (num3 != 0)
			{
				ArtLoader.Instance.GetStaticTexture(num3, out var bounds);
				int num4 = num + 138 + (48 - bounds.Width) / 2;
				if (i > 3)
				{
					num4 -= 20;
				}
				int num5 = num2 + 36 + (120 - bounds.Height) / 2;
				StaticPic staticPic = new StaticPic(num3, 0);
				staticPic.X = num4;
				staticPic.Y = num5;
				staticPic.CanMove = false;
				staticPic.LocalSerial = (uint)(33 + i);
				staticPic.Height = 120;
				StaticPic pic = staticPic;
				pic.MouseUp += delegate
				{
					OnButtonClick((int)pic.LocalSerial);
				};
				_dataBox.Add(pic);
			}
			num += 48;
		}
		int num6 = 0;
		switch (customHouseDoor.Category)
		{
		case 16:
		case 17:
		case 18:
			num6 = 1;
			break;
		case 15:
			num6 = 2;
			break;
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 26:
		case 27:
		case 28:
		case 29:
		case 31:
		case 32:
		case 34:
			num6 = 3;
			break;
		case 30:
		case 33:
			num6 = 4;
			break;
		}
		switch (num6)
		{
		case 0:
			_dataBox.Add(new GumpPic(151, 39, 22400, 0));
			_dataBox.Add(new GumpPic(196, 39, 22401, 0));
			_dataBox.Add(new GumpPic(219, 133, 22402, 0));
			_dataBox.Add(new GumpPic(266, 136, 22403, 0));
			_dataBox.Add(new GumpPic(357, 136, 22404, 0));
			_dataBox.Add(new GumpPic(404, 133, 22405, 0));
			_dataBox.Add(new GumpPic(431, 39, 22406, 0));
			_dataBox.Add(new GumpPic(474, 39, 22407, 0));
			break;
		case 1:
			_dataBox.Add(new GumpPic(245, 39, 22405, 0));
			_dataBox.Add(new GumpPic(290, 39, 22407, 0));
			_dataBox.Add(new GumpPic(337, 39, 22400, 0));
			_dataBox.Add(new GumpPic(380, 39, 22402, 0));
			break;
		case 2:
			_dataBox.Add(new GumpPic(219, 133, 22402, 0));
			_dataBox.Add(new GumpPic(404, 133, 22405, 0));
			break;
		case 3:
			_dataBox.Add(new GumpPic(245, 39, 22400, 0));
			_dataBox.Add(new GumpPic(290, 39, 22401, 0));
			_dataBox.Add(new GumpPic(337, 39, 22406, 0));
			_dataBox.Add(new GumpPic(380, 39, 22407, 0));
			break;
		case 4:
			_dataBox.Add(new GumpPic(151, 39, 22400, 0));
			_dataBox.Add(new GumpPic(196, 39, 22401, 0));
			_dataBox.Add(new GumpPic(245, 39, 22400, 0));
			_dataBox.Add(new GumpPic(290, 39, 22401, 0));
			_dataBox.Add(new GumpPic(337, 39, 22406, 0));
			_dataBox.Add(new GumpPic(380, 39, 22407, 0));
			_dataBox.Add(new GumpPic(431, 39, 22406, 0));
			_dataBox.Add(new GumpPic(474, 39, 22407, 0));
			break;
		}
		_dataBox.Add(new ScissorControl(enabled: false));
	}

	private void AddFloor()
	{
		if (base.Page < 0 || base.Page >= HouseCustomizationManager.Floors.Count)
		{
			return;
		}
		CustomHouseFloor customHouseFloor = HouseCustomizationManager.Floors[base.Page];
		int num = 0;
		int num2 = 0;
		_dataBox.Add(new ScissorControl(enabled: true, 123, 36, 384, 120));
		int num3 = 0;
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				ushort num4 = customHouseFloor.Graphics[num3];
				if (num4 != 0)
				{
					ArtLoader.Instance.GetStaticTexture(num4, out var bounds);
					int num5 = num + 123 + (48 - bounds.Width) / 2;
					int num6 = num2 + 36 + (60 - bounds.Height) / 2;
					StaticPic staticPic = new StaticPic(num4, 0);
					staticPic.X = num5;
					staticPic.Y = num6;
					staticPic.CanMove = false;
					staticPic.LocalSerial = (uint)(33 + num3);
					StaticPic pic = staticPic;
					pic.MouseUp += delegate
					{
						OnButtonClick((int)pic.LocalSerial);
					};
					_dataBox.Add(pic);
				}
				num += 48;
				num3++;
			}
			num = 0;
			num2 += 60;
		}
		_dataBox.Add(new ScissorControl(enabled: false));
	}

	private void AddStair()
	{
		if (base.Page < 0 || base.Page >= HouseCustomizationManager.Stairs.Count)
		{
			return;
		}
		CustomHouseStair customHouseStair = HouseCustomizationManager.Stairs[base.Page];
		for (int i = 0; i < 2; i++)
		{
			int num = ((i != 0) ? 96 : 192);
			int num2 = ((i != 0) ? 60 : 0);
			_dataBox.Add(new ScissorControl(enabled: true, 121, 36 + num2, 384, 60));
			Label label = new Label(ClilocLoader.Instance.GetString(1062113 + i), isunicode: true, ushort.MaxValue, 90, 0);
			label.X = 137;
			label.Y = ((i != 0) ? 111 : 51);
			Label c = label;
			_dataBox.Add(c);
			int num3 = ((i == 0) ? 5 : 0);
			int num4 = ((i != 0) ? 6 : 9);
			int num5 = ((i == 0) ? 10 : 0);
			for (int j = num3; j < num4; j++)
			{
				ushort num6 = customHouseStair.Graphics[j];
				if (num6 != 0)
				{
					ArtLoader.Instance.GetStaticTexture(num6, out var bounds);
					int num7 = num + 123 + (48 - bounds.Width) / 2;
					int num8 = num2 + 36 + (60 - bounds.Height) / 2;
					StaticPic staticPic = new StaticPic(num6, 0);
					staticPic.X = num7;
					staticPic.Y = num8;
					staticPic.CanMove = false;
					staticPic.LocalSerial = (uint)(33 + j + num5);
					staticPic.Height = 60;
					StaticPic pic = staticPic;
					pic.MouseUp += delegate
					{
						OnButtonClick((int)pic.LocalSerial);
					};
					_dataBox.Add(pic);
				}
				num += 48;
			}
			_dataBox.Add(new ScissorControl(enabled: false));
		}
		DataBox dataBox = _dataBox;
		ColorBox colorBox = new ColorBox(384, 2, 0);
		colorBox.X = 123;
		colorBox.Y = 96;
		dataBox.Add(colorBox);
	}

	private void AddRoof()
	{
		int num = 0;
		int num2 = 0;
		if (_customHouseManager.Category == -1)
		{
			int num3 = base.Page * 16;
			int num4 = num3 + 16;
			if (num4 > HouseCustomizationManager.Roofs.Count)
			{
				num4 = HouseCustomizationManager.Roofs.Count;
			}
			_dataBox.Add(new ScissorControl(enabled: true, 121, 36, 384, 60));
			for (int i = num3; i < num4; i++)
			{
				List<CustomHouseRoof> items = HouseCustomizationManager.Roofs[i].Items;
				if (items.Count != 0)
				{
					ArtLoader.Instance.GetStaticTexture((ushort)items[0].NSCrosspiece, out var bounds);
					int num5 = num + 121 + (48 - bounds.Width) / 2;
					int num6 = num2 + 36;
					StaticPic staticPic = new StaticPic((ushort)items[0].NSCrosspiece, 0);
					staticPic.X = num5;
					staticPic.Y = num6;
					staticPic.CanMove = false;
					staticPic.LocalSerial = (uint)(33 + i);
					staticPic.Height = 60;
					StaticPic pic = staticPic;
					pic.MouseUp += delegate
					{
						OnButtonClick((int)pic.LocalSerial);
					};
					_dataBox.Add(pic);
					num += 48;
					if (num >= 384)
					{
						num = 0;
						num2 += 60;
						_dataBox.Add(new ScissorControl(enabled: false));
						_dataBox.Add(new ScissorControl(enabled: true, 121, 96, 384, 60));
					}
				}
			}
			_dataBox.Add(new ScissorControl(enabled: false));
		}
		else
		{
			if (_customHouseManager.Category < 0 || _customHouseManager.Category >= HouseCustomizationManager.Roofs.Count)
			{
				return;
			}
			List<CustomHouseRoof> items2 = HouseCustomizationManager.Roofs[_customHouseManager.Category].Items;
			if (base.Page >= 0 && base.Page < items2.Count)
			{
				CustomHouseRoof customHouseRoof = items2[base.Page];
				_dataBox.Add(new ScissorControl(enabled: true, 130, 44, 384, 120));
				int num7 = 0;
				for (int j = 0; j < 2; j++)
				{
					for (int k = 0; k < 8; k++)
					{
						ushort num8 = customHouseRoof.Graphics[num7];
						if (num8 != 0)
						{
							ArtLoader.Instance.GetStaticTexture(num8, out var bounds2);
							int num9 = num + 130 + (48 - bounds2.Width) / 2;
							int num10 = num2 + 44 + (60 - bounds2.Height) / 2;
							StaticPic staticPic2 = new StaticPic(num8, 0);
							staticPic2.X = num9;
							staticPic2.Y = num10;
							staticPic2.CanMove = false;
							staticPic2.LocalSerial = (uint)(33 + num7);
							StaticPic pic2 = staticPic2;
							pic2.MouseUp += delegate
							{
								OnButtonClick((int)pic2.LocalSerial);
							};
							_dataBox.Add(pic2);
						}
						num += 48;
						num7++;
					}
					num = 0;
					num2 += 60;
				}
				_dataBox.Add(new ScissorControl(enabled: false));
			}
			_dataBoxGUI.Add(new GumpPic(152, 0, 22003, 0));
			Button button = new Button(26, 22050, 22052, 22051, "", 0);
			button.X = 167;
			button.Y = 5;
			button.ButtonAction = ButtonAction.Activate;
			Button button2 = button;
			button2.SetTooltip(ResGumps.ToCustomHouseManagerCategory);
			_dataBoxGUI.Add(button2);
			Button button3 = new Button(29, 22411, 22413, 22412, "", 0);
			button3.X = 305;
			button3.Y = 0;
			button3.ButtonAction = ButtonAction.Activate;
			button2 = button3;
			button2.SetTooltip(ResGumps.LowerRoofPlacementLevel);
			_dataBoxGUI.Add(button2);
			Button button4 = new Button(28, 22414, 22416, 22415, "", 0);
			button4.X = 349;
			button4.Y = 0;
			button4.ButtonAction = ButtonAction.Activate;
			button2 = button4;
			button2.SetTooltip(ResGumps.RaiseRoofPlacementLevel);
			_dataBoxGUI.Add(button2);
			_dataBoxGUI.Add(new GumpPic(583, 4, 22004, 0));
			Label label = new Label(_customHouseManager.RoofZ.ToString(), isunicode: false, 1257, 0, 3);
			label.X = 405;
			label.Y = 15;
			Label c = label;
			_dataBoxGUI.Add(c);
		}
	}

	private void AddMisc()
	{
		int num = 0;
		int num2 = 0;
		if (_customHouseManager.Category == -1)
		{
			int num3 = base.Page * 16;
			int num4 = num3 + 16;
			if (num4 > HouseCustomizationManager.Miscs.Count)
			{
				num4 = HouseCustomizationManager.Miscs.Count;
			}
			_dataBox.Add(new ScissorControl(enabled: true, 121, 36, 384, 60));
			for (int i = num3; i < num4; i++)
			{
				List<CustomHouseMisc> items = HouseCustomizationManager.Miscs[i].Items;
				if (items.Count != 0)
				{
					ArtLoader.Instance.GetStaticTexture((ushort)items[0].Piece5, out var bounds);
					int num5 = num + 121 + (48 - bounds.Width) / 2;
					int num6 = num2 + 36;
					StaticPic staticPic = new StaticPic((ushort)items[0].Piece5, 0);
					staticPic.X = num5;
					staticPic.Y = num6;
					staticPic.CanMove = false;
					staticPic.LocalSerial = (uint)(33 + i);
					staticPic.Height = 60;
					StaticPic pic = staticPic;
					pic.MouseUp += delegate
					{
						OnButtonClick((int)pic.LocalSerial);
					};
					_dataBox.Add(pic);
					num += 48;
					if (num >= 384)
					{
						num = 0;
						num2 += 60;
						_dataBox.Add(new ScissorControl(enabled: false));
						_dataBox.Add(new ScissorControl(enabled: true, 121, 96, 384, 60));
					}
				}
			}
			_dataBox.Add(new ScissorControl(enabled: false));
		}
		else
		{
			if (_customHouseManager.Category < 0 || _customHouseManager.Category >= HouseCustomizationManager.Miscs.Count)
			{
				return;
			}
			List<CustomHouseMisc> items2 = HouseCustomizationManager.Miscs[_customHouseManager.Category].Items;
			if (base.Page >= 0 && base.Page < items2.Count)
			{
				CustomHouseMisc customHouseMisc = items2[base.Page];
				_dataBox.Add(new ScissorControl(enabled: true, 130, 44, 384, 120));
				for (int j = 0; j < 8; j++)
				{
					ushort num7 = customHouseMisc.Graphics[j];
					if (num7 != 0)
					{
						ArtLoader.Instance.GetStaticTexture(num7, out var bounds2);
						int num8 = num + 130 + (48 - bounds2.Width) / 2;
						int num9 = num2 + 44 + (120 - bounds2.Height) / 2;
						StaticPic staticPic2 = new StaticPic(num7, 0);
						staticPic2.X = num8;
						staticPic2.Y = num9;
						staticPic2.CanMove = false;
						staticPic2.LocalSerial = (uint)(33 + j);
						StaticPic pic2 = staticPic2;
						pic2.MouseUp += delegate
						{
							OnButtonClick((int)pic2.LocalSerial);
						};
						_dataBox.Add(pic2);
					}
					num += 48;
				}
				_dataBox.Add(new ScissorControl(enabled: false));
			}
			_dataBoxGUI.Add(new GumpPic(152, 0, 22003, 0));
			DataBox dataBoxGUI = _dataBoxGUI;
			Button button = new Button(26, 22050, 22052, 22051, "", 0);
			button.X = 167;
			button.Y = 5;
			button.ButtonAction = ButtonAction.Activate;
			dataBoxGUI.Add(button);
		}
	}

	private void AddMenu()
	{
		Button button = new Button(20, 2445, 2445, 2445, ResGumps.Backup, 0, isunicode: true, 0, 54);
		button.X = 150;
		button.Y = 50;
		button.ButtonAction = ButtonAction.Activate;
		button.FontCenter = true;
		Button button2 = button;
		button2.SetTooltip(ResGumps.StoreDesignInProgress);
		_dataBox.Add(button2);
		Button button3 = new Button(21, 2445, 2445, 2445, ResGumps.Restore, 0, isunicode: true, 0, 54);
		button3.X = 150;
		button3.Y = 90;
		button3.ButtonAction = ButtonAction.Activate;
		button3.FontCenter = true;
		button2 = button3;
		button2.SetTooltip(ResGumps.RestoreYourDesign);
		_dataBox.Add(button2);
		Button button4 = new Button(22, 2445, 2445, 2445, ResGumps.Sync, 0, isunicode: true, 0, 54);
		button4.X = 270;
		button4.Y = 50;
		button4.ButtonAction = ButtonAction.Activate;
		button4.FontCenter = true;
		button2 = button4;
		button2.SetTooltip(ResGumps.SynchronizeDesignStateWithServer);
		_dataBox.Add(button2);
		Button button5 = new Button(23, 2445, 2445, 2445, ResGumps.Clear, 0, isunicode: true, 0, 54);
		button5.X = 270;
		button5.Y = 90;
		button5.ButtonAction = ButtonAction.Activate;
		button5.FontCenter = true;
		button2 = button5;
		button2.SetTooltip(ResGumps.ClearAllChanges);
		_dataBox.Add(button2);
		Button button6 = new Button(24, 2445, 2445, 2445, ResGumps.Commit, 0, isunicode: true, 0, 54);
		button6.X = 390;
		button6.Y = 50;
		button6.ButtonAction = ButtonAction.Activate;
		button6.FontCenter = true;
		button2 = button6;
		button2.SetTooltip(ResGumps.SaveExistingChanges);
		_dataBox.Add(button2);
		Button button7 = new Button(25, 2445, 2445, 2445, ResGumps.Revert, 0, isunicode: true, 0, 54);
		button7.X = 390;
		button7.Y = 90;
		button7.ButtonAction = ButtonAction.Activate;
		button7.FontCenter = true;
		button2 = button7;
		button2.SetTooltip(ResGumps.RevertYourDesign);
		_dataBox.Add(button2);
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID >= 33)
		{
			int num = buttonID - 33;
			if (_customHouseManager.Category == -1 && (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL || _customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF || _customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC))
			{
				int num2 = -1;
				if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL && num >= 0 && num < HouseCustomizationManager.Walls.Count)
				{
					num2 = HouseCustomizationManager.Walls[num].Index;
				}
				else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF && num >= 0 && num < HouseCustomizationManager.Roofs.Count)
				{
					num2 = HouseCustomizationManager.Roofs[num].Index;
				}
				else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC && num >= 0 && num < HouseCustomizationManager.Miscs.Count)
				{
					num2 = HouseCustomizationManager.Miscs[num].Index;
				}
				if (num2 != -1)
				{
					_customHouseManager.Category = num2;
					base.Page = 0;
					_customHouseManager.SelectedGraphic = 0;
					_customHouseManager.Erasing = false;
					_customHouseManager.SeekTile = false;
					_customHouseManager.CombinedStair = false;
					UpdateMaxPage();
					Update();
				}
			}
			else
			{
				if (num < 0 || base.Page < 0)
				{
					return;
				}
				bool combinedStair = false;
				ushort num3 = 0;
				if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL || _customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF || _customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC)
				{
					if (_customHouseManager.Category >= 0)
					{
						if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL && _customHouseManager.Category < HouseCustomizationManager.Walls.Count && num < 8)
						{
							List<CustomHouseWall> items = HouseCustomizationManager.Walls[_customHouseManager.Category].Items;
							if (base.Page < items.Count)
							{
								num3 = (_customHouseManager.ShowWindow ? items[base.Page].WindowGraphics[num] : items[base.Page].Graphics[num]);
							}
						}
						else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF && _customHouseManager.Category < HouseCustomizationManager.Roofs.Count && num < 16)
						{
							List<CustomHouseRoof> items2 = HouseCustomizationManager.Roofs[_customHouseManager.Category].Items;
							if (base.Page < items2.Count)
							{
								num3 = items2[base.Page].Graphics[num];
							}
						}
						else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC && _customHouseManager.Category < HouseCustomizationManager.Miscs.Count && num < 8)
						{
							List<CustomHouseMisc> items3 = HouseCustomizationManager.Miscs[_customHouseManager.Category].Items;
							if (base.Page < items3.Count)
							{
								num3 = items3[base.Page].Graphics[num];
							}
						}
					}
				}
				else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR && base.Page < HouseCustomizationManager.Doors.Count && num < 8)
				{
					num3 = HouseCustomizationManager.Doors[base.Page].Graphics[num];
				}
				else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR && base.Page < HouseCustomizationManager.Floors.Count && num < 16)
				{
					num3 = HouseCustomizationManager.Floors[base.Page].Graphics[num];
				}
				else if (_customHouseManager.State == CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR && base.Page < HouseCustomizationManager.Stairs.Count)
				{
					if (num > 10)
					{
						combinedStair = true;
						num -= 10;
					}
					if (num < 9)
					{
						num3 = HouseCustomizationManager.Stairs[base.Page].Graphics[num];
					}
				}
				if (num3 != 0)
				{
					_customHouseManager.SetTargetMulti();
					_customHouseManager.CombinedStair = combinedStair;
					_customHouseManager.SelectedGraphic = num3;
					Update();
				}
			}
			return;
		}
		switch (buttonID)
		{
		case 1:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_WALL;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 2:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_DOOR;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 3:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_FLOOR;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 4:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_STAIR;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 5:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_ROOF;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 6:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_MISC;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 7:
			_customHouseManager.SetTargetMulti();
			_customHouseManager.Erasing = !_customHouseManager.Erasing;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			Update();
			break;
		case 8:
			_customHouseManager.SetTargetMulti();
			_customHouseManager.SeekTile = true;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			Update();
			break;
		case 9:
			_customHouseManager.Category = -1;
			_customHouseManager.State = CUSTOM_HOUSE_GUMP_STATE.CHGS_MENU;
			base.Page = 0;
			_customHouseManager.MaxPage = 1;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			TargetManager.CancelTarget();
			Update();
			break;
		case 10:
		case 11:
		case 12:
		case 13:
		{
			int num4 = buttonID - 10;
			_customHouseManager.FloorVisionState[num4]++;
			if (_customHouseManager.FloorVisionState[num4] > 6)
			{
				_customHouseManager.FloorVisionState[num4] = 0;
			}
			_customHouseManager.GenerateFloorPlace();
			Update();
			break;
		}
		case 14:
		{
			_customHouseManager.CurrentFloor = 1;
			NetClient.Socket.Send_CustomHouseGoToFloor(1);
			for (int i = 0; i < _customHouseManager.FloorVisionState.Length; i++)
			{
				_customHouseManager.FloorVisionState[i] = 0;
			}
			Update();
			break;
		}
		case 15:
		{
			_customHouseManager.CurrentFloor = 2;
			NetClient.Socket.Send_CustomHouseGoToFloor(2);
			for (int l = 0; l < _customHouseManager.FloorVisionState.Length; l++)
			{
				_customHouseManager.FloorVisionState[l] = 0;
			}
			Update();
			break;
		}
		case 16:
		{
			_customHouseManager.CurrentFloor = 3;
			NetClient.Socket.Send_CustomHouseGoToFloor(3);
			for (int k = 0; k < _customHouseManager.FloorVisionState.Length; k++)
			{
				_customHouseManager.FloorVisionState[k] = 0;
			}
			Update();
			break;
		}
		case 17:
		{
			_customHouseManager.CurrentFloor = 4;
			NetClient.Socket.Send_CustomHouseGoToFloor(4);
			for (int j = 0; j < _customHouseManager.FloorVisionState.Length; j++)
			{
				_customHouseManager.FloorVisionState[j] = 0;
			}
			Update();
			break;
		}
		case 18:
			base.Page--;
			if (base.Page < 0)
			{
				base.Page = _customHouseManager.MaxPage - 1;
				if (base.Page < 0)
				{
					base.Page = 0;
				}
			}
			Update();
			break;
		case 19:
			base.Page++;
			if (base.Page >= _customHouseManager.MaxPage)
			{
				base.Page = 0;
			}
			Update();
			break;
		case 20:
			NetClient.Socket.Send_CustomHouseBackup();
			break;
		case 21:
			NetClient.Socket.Send_CustomHouseRestore();
			break;
		case 22:
			NetClient.Socket.Send_CustomHouseSync();
			break;
		case 23:
			NetClient.Socket.Send_CustomHouseClear();
			break;
		case 24:
			NetClient.Socket.Send_CustomHouseCommit();
			break;
		case 25:
			NetClient.Socket.Send_CustomHouseRevert();
			break;
		case 26:
			_customHouseManager.Category = -1;
			base.Page = 0;
			_customHouseManager.SelectedGraphic = 0;
			_customHouseManager.CombinedStair = false;
			UpdateMaxPage();
			TargetManager.CancelTarget();
			Update();
			break;
		case 27:
			_customHouseManager.ShowWindow = !_customHouseManager.ShowWindow;
			Update();
			break;
		case 28:
			if (_customHouseManager.RoofZ < 6)
			{
				_customHouseManager.RoofZ++;
				Update();
			}
			break;
		case 29:
			if (_customHouseManager.RoofZ > 1)
			{
				_customHouseManager.RoofZ--;
				Update();
			}
			break;
		}
	}

	public override void Dispose()
	{
		World.CustomHouseManager = null;
		NetClient.Socket.Send_CustomHouseBuildingExit();
		TargetManager.CancelTarget();
		base.Dispose();
	}
}
