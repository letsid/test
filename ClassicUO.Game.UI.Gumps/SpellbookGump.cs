using System;
using System.Collections.Generic;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class SpellbookGump : Gump
{
	private enum ButtonCircle
	{
		Circle_1_2,
		Circle_3_4,
		Circle_5_6,
		Circle_7_8
	}

	private class HueGumpPic : GumpPic
	{
		private readonly MacroManager _mm;

		private readonly ushort _spellID;

		private readonly string _spellName;

		private bool ShowEdit
		{
			get
			{
				if (Keyboard.Ctrl && Keyboard.Alt)
				{
					return ProfileManager.CurrentProfile.FastSpellsAssign;
				}
				return false;
			}
		}

		public HueGumpPic(int x, int y, ushort graphic, ushort hue, ushort spellID, string spellName)
			: base(x, y, graphic, hue)
		{
			_spellID = spellID;
			_spellName = spellName;
			_mm = Client.Game.GetScene<GameScene>().Macros;
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (World.ActiveSpellIcons.IsActive(_spellID))
			{
				base.Hue = 38;
			}
			else if (base.Hue != 0)
			{
				base.Hue = 0;
			}
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			base.Draw(batcher, x, y);
			if (ShowEdit)
			{
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
				Rectangle bounds;
				Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(2511u, out bounds);
				if (gumpTexture != null)
				{
					if (UIManager.MouseOverControl != null && (UIManager.MouseOverControl == this || UIManager.MouseOverControl.RootParent == this))
					{
						hueVector.X = 34f;
						hueVector.Y = 1f;
					}
					else
					{
						hueVector.X = 68f;
						hueVector.Y = 1f;
					}
					batcher.Draw(gumpTexture, new Vector2(x + (base.Width - bounds.Width), y), bounds, hueVector);
				}
			}
			return true;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left && ShowEdit)
			{
				Macro item = Macro.CreateFastMacro(_spellName, MacroType.CastSpell, (MacroSubType)(GetSpellsId() + SpellBookDefinition.GetSpellsGroup(_spellID)));
				if (_mm.FindMacro(_spellName) == null)
				{
					_mm.MoveToBack(item);
				}
				GameActions.OpenMacroGump(_spellName);
			}
		}

		private int GetSpellsId()
		{
			return _spellID % 100;
		}
	}

	private float _clickTiming;

	private DataBox _dataBox;

	private HitBox _hitBox;

	private bool _isMinimized;

	private Control _lastPressed;

	private int _maxPage;

	private GumpPic _pageCornerLeft;

	private GumpPic _pageCornerRight;

	private GumpPic _picBase;

	private SpellBookType _spellBookType;

	private readonly bool[] _spells = new bool[64];

	public bool IsMinimized
	{
		get
		{
			return _isMinimized;
		}
		set
		{
			if (_isMinimized == value)
			{
				return;
			}
			_isMinimized = value;
			GetBookInfo(_spellBookType, out var bookGraphic, out var minimizedGraphic, out var _, out var _, out var _, out var _);
			_picBase.Graphic = (value ? minimizedGraphic : bookGraphic);
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			_picBase.IsVisible = true;
			base.WantUpdateSize = true;
		}
	}

	public override GumpType GumpType => GumpType.SpellBook;

	public SpellbookGump(uint item)
		: this()
	{
		base.LocalSerial = item;
		BuildGump();
	}

	public SpellbookGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = false;
		base.CanCloseWithRightClick = true;
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("isminimized", IsMinimized.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		Client.Game.GetScene<GameScene>().DoubleClickDelayed(base.LocalSerial);
		Dispose();
	}

	private void BuildGump()
	{
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null)
		{
			Dispose();
			return;
		}
		AssignGraphic(item);
		GetBookInfo(_spellBookType, out var bookGraphic, out var _, out var _, out var _, out var _, out var _);
		Add(_picBase = new GumpPic(0, 0, bookGraphic, 0));
		_picBase.MouseDoubleClick += _picBase_MouseDoubleClick;
		_dataBox = new DataBox(0, 0, 0, 0)
		{
			CanMove = true,
			AcceptMouseInput = true,
			WantUpdateSize = true
		};
		Add(_dataBox);
		_hitBox = new HitBox(0, 98, 27, 23);
		Add(_hitBox);
		_hitBox.MouseUp += _hitBox_MouseUp;
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
		RequestUpdateContents();
		Client.Game.Scene.Audio.PlaySound(85);
	}

	private void _picBase_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && IsMinimized)
		{
			IsMinimized = false;
		}
	}

	private void _hitBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && !IsMinimized)
		{
			IsMinimized = true;
		}
	}

	public override void Dispose()
	{
		Client.Game.Scene.Audio.PlaySound(85);
		UIManager.SavePosition(base.LocalSerial, base.Location);
		base.Dispose();
	}

	private void CreateBook()
	{
		_dataBox.Clear();
		_dataBox.WantUpdateSize = true;
		GetBookInfo(_spellBookType, out var _, out var _, out var iconStartGraphic, out var maxSpellsCount, out var spellsOnPage, out var dictionaryPagesCount);
		int num = 0;
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null)
		{
			Dispose();
			return;
		}
		for (LinkedObject linkedObject = item.Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			int amount = ((Item)linkedObject).Amount;
			if (amount > 0 && amount <= maxSpellsCount)
			{
				_spells[amount - 1] = true;
				num++;
			}
		}
		int num2 = ((_spellBookType == SpellBookType.Mastery) ? dictionaryPagesCount : (dictionaryPagesCount >> 1));
		_maxPage = num2 + (num + 1 >> 1);
		int num3 = 0;
		if (_spellBookType == SpellBookType.Magery)
		{
			DataBox dataBox = _dataBox;
			Button button = new Button(0, 2225, 2225, 0, "", 0);
			button.X = 58;
			button.Y = 175;
			button.ButtonAction = ButtonAction.Activate;
			button.ToPage = 1;
			dataBox.Add(button);
			DataBox dataBox2 = _dataBox;
			Button button2 = new Button(0, 2226, 2226, 0, "", 0);
			button2.X = 93;
			button2.Y = 175;
			button2.ButtonAction = ButtonAction.Activate;
			button2.ToPage = 1;
			dataBox2.Add(button2);
			DataBox dataBox3 = _dataBox;
			Button button3 = new Button(1, 2227, 2227, 0, "", 0);
			button3.X = 130;
			button3.Y = 175;
			button3.ButtonAction = ButtonAction.Activate;
			button3.ToPage = 2;
			dataBox3.Add(button3);
			DataBox dataBox4 = _dataBox;
			Button button4 = new Button(1, 2228, 2228, 0, "", 0);
			button4.X = 164;
			button4.Y = 175;
			button4.ButtonAction = ButtonAction.Activate;
			button4.ToPage = 2;
			dataBox4.Add(button4);
			DataBox dataBox5 = _dataBox;
			Button button5 = new Button(2, 2229, 2229, 0, "", 0);
			button5.X = 227;
			button5.Y = 175;
			button5.ButtonAction = ButtonAction.Activate;
			button5.ToPage = 3;
			dataBox5.Add(button5);
			DataBox dataBox6 = _dataBox;
			Button button6 = new Button(2, 2230, 2230, 0, "", 0);
			button6.X = 260;
			button6.Y = 175;
			button6.ButtonAction = ButtonAction.Activate;
			button6.ToPage = 3;
			dataBox6.Add(button6);
			DataBox dataBox7 = _dataBox;
			Button button7 = new Button(3, 2231, 2231, 0, "", 0);
			button7.X = 297;
			button7.Y = 175;
			button7.ButtonAction = ButtonAction.Activate;
			button7.ToPage = 4;
			dataBox7.Add(button7);
			DataBox dataBox8 = _dataBox;
			Button button8 = new Button(3, 2232, 2232, 0, "", 0);
			button8.X = 332;
			button8.Y = 175;
			button8.ButtonAction = ButtonAction.Activate;
			button8.ToPage = 4;
			dataBox8.Add(button8);
		}
		int num4 = 0;
		for (int i = 1; i <= num2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				if (i == 1 && _spellBookType == SpellBookType.Chivalry)
				{
					Label label = new Label(ResGumps.TithingPointsAvailable + World.Player.TithingPoints, isunicode: false, 648, 0, 6);
					label.X = 62;
					label.Y = 162;
					Label c = label;
					_dataBox.Add(c, i);
				}
				int num5 = 106;
				int num6 = 62;
				int num7 = 0;
				if (j % 2 != 0)
				{
					num5 = 269;
					num6 = 225;
				}
				Label label2 = new Label(ResGumps.Index, isunicode: false, 648, 0, 6);
				label2.X = num5;
				label2.Y = 10;
				Label c2 = label2;
				_dataBox.Add(c2, i);
				if (_spellBookType == SpellBookType.Mastery && j >= 1)
				{
					Label label3 = new Label(ResGumps.Abilities, isunicode: false, 648, 0, 6);
					label3.X = num6;
					label3.Y = 30;
					c2 = label3;
					_dataBox.Add(c2, i);
					if (!World.OPL.TryGetNameAndData(base.LocalSerial, out var _, out var data))
					{
						break;
					}
					data = data.ToLower();
					string[] array = data.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k] == null)
						{
							continue;
						}
						int num8 = array[k].IndexOf("mastery", StringComparison.InvariantCulture);
						if (--num8 < 0)
						{
							continue;
						}
						string text = array[k].Substring(0, num8);
						if (string.IsNullOrEmpty(text))
						{
							break;
						}
						List<int> spellListByGroupName = SpellsMastery.GetSpellListByGroupName(text);
						for (int l = 0; l < spellListByGroupName.Count; l++)
						{
							int num9 = spellListByGroupName[l];
							SpellDefinition spell = SpellsMastery.GetSpell(num9);
							if (spell != null)
							{
								ushort graphic = (ushort)spell.GumpIconID;
								int num10 = ((num9 >= 0 && num9 < 6) ? 1115689 : 1155932);
								int num11 = 55 + 44 * l;
								GumpPic gumpPic = new GumpPic(225, num11, graphic, 0)
								{
									LocalSerial = (uint)(num9 - 1)
								};
								_dataBox.Add(gumpPic, i);
								gumpPic.MouseDoubleClick += OnIconDoubleClick;
								gumpPic.DragBegin += OnIconDragBegin;
								Label label4 = new Label(spell.Name, isunicode: false, 648, 80, 6);
								label4.X = 273;
								label4.Y = num11 + 2;
								c2 = label4;
								_dataBox.Add(c2, i);
								if (num10 > 0)
								{
									string @string = ClilocLoader.Instance.GetString(num10 + num9);
									gumpPic.SetTooltip(@string, 250);
								}
							}
						}
						break;
					}
					break;
				}
				if (_spellBookType == SpellBookType.Magery)
				{
					Label label5 = new Label(SpellsMagery.CircleNames[(i - 1) * 2 + j % 2], isunicode: false, 648, 0, 6);
					label5.X = num6;
					label5.Y = 30;
					c2 = label5;
					_dataBox.Add(c2, i);
				}
				else if (_spellBookType == SpellBookType.Mastery)
				{
					Label label6 = new Label((i == num2) ? ResGumps.Passive : ResGumps.Activated, isunicode: false, 648, 0, 6);
					label6.X = num6;
					label6.Y = 30;
					c2 = label6;
					_dataBox.Add(c2, i);
				}
				int num12 = num2 + (num4 + 1 >> 1);
				if (_spellBookType == SpellBookType.Mastery)
				{
					int num13 = SpellsMastery.SpellbookIndices[i - 1].Length;
					for (int m = 0; m < num13; m++)
					{
						num3 = SpellsMastery.SpellbookIndices[i - 1][m] - 1;
						if (_spells[num3])
						{
							GetSpellNames(num3, out var name2, out var _, out var _);
							if (num4 % 2 == 0)
							{
								num12++;
							}
							num4++;
							HoveredLabel hoveredLabel = new HoveredLabel(name2, isunicode: false, 648, 51, 648, 130, 9, FontStyle.Cropped);
							hoveredLabel.X = num6;
							hoveredLabel.Y = 52 + num7;
							hoveredLabel.LocalSerial = (uint)(num2 + num3 / 2 + 1);
							hoveredLabel.AcceptMouseInput = true;
							hoveredLabel.Tag = num3 + 1;
							hoveredLabel.CanMove = true;
							c2 = hoveredLabel;
							c2.MouseUp += OnClicked;
							c2.MouseDoubleClick += OnDoubleClicked;
							_dataBox.Add(c2, i);
							num7 += 15;
						}
					}
					continue;
				}
				int num14 = 0;
				while (num14 < spellsOnPage)
				{
					if (_spells[num3])
					{
						GetSpellNames(num3, out var name3, out var _, out var _);
						if (num4 % 2 == 0)
						{
							num12++;
						}
						num4++;
						HoveredLabel hoveredLabel2 = new HoveredLabel(name3, isunicode: false, 648, 51, 648, 130, 9, FontStyle.Cropped);
						hoveredLabel2.X = num6;
						hoveredLabel2.Y = 52 + num7;
						hoveredLabel2.LocalSerial = (uint)num12;
						hoveredLabel2.AcceptMouseInput = true;
						hoveredLabel2.Tag = num3 + 1;
						hoveredLabel2.CanMove = true;
						c2 = hoveredLabel2;
						c2.MouseUp += OnClicked;
						c2.MouseDoubleClick += OnDoubleClicked;
						_dataBox.Add(c2, i);
						num7 += 15;
					}
					num14++;
					num3++;
				}
			}
		}
		int num15 = num2 + 1;
		int num16 = 6;
		int n = 0;
		int num17 = 0;
		for (; n < maxSpellsCount; n++)
		{
			if (!_spells[n])
			{
				continue;
			}
			int num18 = 62;
			int num19 = 87;
			int num20 = 112;
			uint num21 = (uint)(100 + n);
			if (num17 > 0)
			{
				if (num17 % 2 != 0)
				{
					num18 = 225;
					num19 = 224;
					num20 = 275;
					num21 = (uint)(1000 + n);
				}
				else
				{
					num15++;
				}
			}
			num17++;
			GetSpellNames(n, out var name4, out var abbreviature3, out var reagents3);
			switch (_spellBookType)
			{
			case SpellBookType.Magery:
			{
				Label label9 = new Label(SpellsMagery.CircleNames[n >> 3], isunicode: false, 648, 0, 6);
				label9.X = num19;
				label9.Y = num16 + 4;
				Label c4 = label9;
				_dataBox.Add(c4, num15);
				Label label10 = new Label(name4, isunicode: false, 648, 80, 6);
				label10.X = num20;
				label10.Y = 34;
				c4 = label10;
				_dataBox.Add(c4, num15);
				int num22 = 26;
				if (c4.Height < 24)
				{
					num22 = 31;
				}
				num22 += c4.Height;
				Label label11 = new Label(abbreviature3, isunicode: false, 648, 0, 8);
				label11.X = num20;
				label11.Y = num22;
				c4 = label11;
				_dataBox.Add(c4, num15);
				break;
			}
			case SpellBookType.Mastery:
			{
				Label label12 = new Label(SpellsMastery.GetMasteryGroupByID(n + 1), isunicode: false, 648, 0, 6);
				label12.X = num19;
				label12.Y = num16 + 4;
				Label c5 = label12;
				_dataBox.Add(c5, num15);
				Label label13 = new Label(name4, isunicode: false, 648, 80, 6);
				label13.X = num20;
				label13.Y = 34;
				c5 = label13;
				_dataBox.Add(c5, num15);
				if (!string.IsNullOrEmpty(abbreviature3))
				{
					int num23 = 26;
					if (c5.Height < 24)
					{
						num23 = 31;
					}
					num23 += c5.Height;
					Label label14 = new Label(abbreviature3, isunicode: false, 648, 80, 6);
					label14.X = num20;
					label14.Y = num23;
					c5 = label14;
					_dataBox.Add(c5, num15);
				}
				break;
			}
			default:
			{
				Label label7 = new Label(name4, isunicode: false, 648, 0, 6);
				label7.X = num19;
				label7.Y = num16;
				Label c3 = label7;
				_dataBox.Add(c3, num15);
				if (!string.IsNullOrEmpty(abbreviature3))
				{
					Label label8 = new Label(abbreviature3, isunicode: false, 648, 80, 6);
					label8.X = num20;
					label8.Y = 34;
					c3 = label8;
					_dataBox.Add(c3, num15);
				}
				break;
			}
			}
			ushort graphic2;
			int offset;
			if (_spellBookType == SpellBookType.Mastery)
			{
				graphic2 = (ushort)SpellsMastery.GetSpell(n + 1).GumpIconID;
				offset = ((n >= 0 && n < 6) ? 1115689 : 1155932);
			}
			else
			{
				graphic2 = (ushort)(iconStartGraphic + n);
				GetSpellToolTip(out offset);
			}
			SpellDefinition spellDefinition = GetSpellDefinition(num21);
			HueGumpPic hueGumpPic = new HueGumpPic(num18, 40, graphic2, 0, (ushort)spellDefinition.ID, spellDefinition.Name);
			hueGumpPic.X = num18;
			hueGumpPic.Y = 40;
			hueGumpPic.LocalSerial = num21;
			HueGumpPic hueGumpPic2 = hueGumpPic;
			if (offset > 0)
			{
				string string2 = ClilocLoader.Instance.GetString(offset + n);
				hueGumpPic2.SetTooltip(string2, 250);
			}
			hueGumpPic2.MouseDoubleClick += OnIconDoubleClick;
			hueGumpPic2.DragBegin += OnIconDragBegin;
			_dataBox.Add(hueGumpPic2, num15);
			if (!string.IsNullOrEmpty(reagents3))
			{
				if (_spellBookType != SpellBookType.Mastery)
				{
					_dataBox.Add(new GumpPicTiled(num18, 88, 120, 5, 2101), num15);
				}
				Label label15 = new Label(ResGumps.Reagents, isunicode: false, 648, 0, 6);
				label15.X = num18;
				label15.Y = 92;
				Label c6 = label15;
				_dataBox.Add(c6, num15);
				Label label16 = new Label(reagents3, isunicode: false, 648, 0, 9);
				label16.X = num18;
				label16.Y = 114;
				c6 = label16;
				_dataBox.Add(c6, num15);
			}
			if (_spellBookType != 0)
			{
				GetSpellRequires(n, out var y, out var text2);
				Label label17 = new Label(text2, isunicode: false, 648, 0, 6);
				label17.X = num18;
				label17.Y = y;
				Label c7 = label17;
				_dataBox.Add(c7, num15);
			}
		}
		SetActivePage(1);
	}

	protected override void UpdateContents()
	{
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null)
		{
			Dispose();
			return;
		}
		AssignGraphic(item);
		CreateBook();
	}

	private void OnIconDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left)
		{
			SpellDefinition spellDefinition = GetSpellDefinition((sender as Control).LocalSerial);
			if (spellDefinition != null)
			{
				GameActions.CastSpell(spellDefinition.ID);
			}
		}
	}

	private void OnIconDragBegin(object sender, EventArgs e)
	{
		if (!UIManager.IsDragging)
		{
			SpellDefinition spellDefinition = GetSpellDefinition((sender as Control).LocalSerial);
			if (spellDefinition != null)
			{
				GetSpellFloatingButton(spellDefinition.ID)?.Dispose();
				UseSpellButtonGump useSpellButtonGump = new UseSpellButtonGump(spellDefinition);
				useSpellButtonGump.X = Mouse.LClickPosition.X - 22;
				useSpellButtonGump.Y = Mouse.LClickPosition.Y - 22;
				UIManager.Add(useSpellButtonGump);
				UIManager.AttemptDragControl(useSpellButtonGump, attemptAlwaysSuccessful: true);
			}
		}
	}

	private static UseSpellButtonGump GetSpellFloatingButton(int id)
	{
		for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value is UseSpellButtonGump useSpellButtonGump && useSpellButtonGump.SpellID == id)
			{
				return useSpellButtonGump;
			}
		}
		return null;
	}

	private SpellDefinition GetSpellDefinition(uint serial)
	{
		int idx = (int)(((serial > 1000) ? (serial - 1000) : ((serial >= 100) ? (serial - 100) : serial)) + 1);
		return GetSpellDefinition(idx);
	}

	private SpellDefinition GetSpellDefinition(int idx)
	{
		SpellDefinition result = null;
		switch (_spellBookType)
		{
		case SpellBookType.Magery:
			result = SpellsMagery.GetSpell(idx);
			break;
		case SpellBookType.Necromancy:
			result = SpellsNecromancy.GetSpell(idx);
			break;
		case SpellBookType.Chivalry:
			result = SpellsChivalry.GetSpell(idx);
			break;
		case SpellBookType.Bushido:
			result = SpellsBushido.GetSpell(idx);
			break;
		case SpellBookType.Ninjitsu:
			result = SpellsNinjitsu.GetSpell(idx);
			break;
		case SpellBookType.Spellweaving:
			result = SpellsSpellweaving.GetSpell(idx);
			break;
		case SpellBookType.Mysticism:
			result = SpellsMysticism.GetSpell(idx);
			break;
		case SpellBookType.Mastery:
			result = SpellsMastery.GetSpell(idx);
			break;
		}
		return result;
	}

	private static void GetBookInfo(SpellBookType type, out ushort bookGraphic, out ushort minimizedGraphic, out ushort iconStartGraphic, out int maxSpellsCount, out int spellsOnPage, out int dictionaryPagesCount)
	{
		switch (type)
		{
		default:
			maxSpellsCount = SpellsMagery.MaxSpellCount;
			bookGraphic = 2220;
			minimizedGraphic = 2234;
			iconStartGraphic = 2240;
			break;
		case SpellBookType.Necromancy:
			maxSpellsCount = SpellsNecromancy.MaxSpellCount;
			bookGraphic = 11008;
			minimizedGraphic = 11011;
			iconStartGraphic = 20480;
			break;
		case SpellBookType.Chivalry:
			maxSpellsCount = SpellsChivalry.MaxSpellCount;
			bookGraphic = 11009;
			minimizedGraphic = 11012;
			iconStartGraphic = 20736;
			break;
		case SpellBookType.Bushido:
			maxSpellsCount = SpellsBushido.MaxSpellCount;
			bookGraphic = 11015;
			minimizedGraphic = 11017;
			iconStartGraphic = 21504;
			break;
		case SpellBookType.Ninjitsu:
			maxSpellsCount = SpellsNinjitsu.MaxSpellCount;
			bookGraphic = 11014;
			minimizedGraphic = 11016;
			iconStartGraphic = 21248;
			break;
		case SpellBookType.Spellweaving:
			maxSpellsCount = SpellsSpellweaving.MaxSpellCount;
			bookGraphic = 11055;
			minimizedGraphic = 11053;
			iconStartGraphic = 23000;
			break;
		case SpellBookType.Mysticism:
			maxSpellsCount = SpellsMysticism.MaxSpellCount;
			bookGraphic = 11058;
			minimizedGraphic = 11056;
			iconStartGraphic = 24000;
			break;
		case SpellBookType.Mastery:
			maxSpellsCount = SpellsMastery.MaxSpellCount;
			bookGraphic = 2220;
			minimizedGraphic = 2234;
			iconStartGraphic = 2373;
			break;
		}
		spellsOnPage = Math.Min(maxSpellsCount >> 1, 8);
		dictionaryPagesCount = (int)Math.Ceiling((float)maxSpellsCount / 8f);
		if (dictionaryPagesCount % 2 != 0)
		{
			dictionaryPagesCount++;
		}
	}

	private void GetSpellToolTip(out int offset)
	{
		switch (_spellBookType)
		{
		case SpellBookType.Magery:
			offset = 1061290;
			break;
		case SpellBookType.Necromancy:
			offset = 1061390;
			break;
		case SpellBookType.Chivalry:
			offset = 1061490;
			break;
		case SpellBookType.Bushido:
			offset = 1063263;
			break;
		case SpellBookType.Ninjitsu:
			offset = 1063279;
			break;
		case SpellBookType.Spellweaving:
			offset = 1072042;
			break;
		case SpellBookType.Mysticism:
			offset = 1095193;
			break;
		case SpellBookType.Mastery:
			offset = 0;
			break;
		default:
			offset = 0;
			break;
		}
	}

	private void GetSpellNames(int offset, out string name, out string abbreviature, out string reagents)
	{
		switch (_spellBookType)
		{
		default:
		{
			SpellDefinition spell = SpellsMagery.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = SpellsMagery.SpecialReagentsChars[offset];
			reagents = spell.CreateReagentListString("\n");
			break;
		}
		case SpellBookType.Necromancy:
		{
			SpellDefinition spell = SpellsNecromancy.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = spell.CreateReagentListString("\n");
			break;
		}
		case SpellBookType.Chivalry:
		{
			SpellDefinition spell = SpellsChivalry.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = string.Empty;
			break;
		}
		case SpellBookType.Bushido:
		{
			SpellDefinition spell = SpellsBushido.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = string.Empty;
			break;
		}
		case SpellBookType.Ninjitsu:
		{
			SpellDefinition spell = SpellsNinjitsu.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = string.Empty;
			break;
		}
		case SpellBookType.Spellweaving:
		{
			SpellDefinition spell = SpellsSpellweaving.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = string.Empty;
			break;
		}
		case SpellBookType.Mysticism:
		{
			SpellDefinition spell = SpellsMysticism.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = spell.CreateReagentListString("\n");
			break;
		}
		case SpellBookType.Mastery:
		{
			SpellDefinition spell = SpellsMastery.GetSpell(offset + 1);
			name = spell.Name;
			abbreviature = spell.PowerWords;
			reagents = spell.CreateReagentListString("\n");
			break;
		}
		}
	}

	private void GetSpellRequires(int offset, out int y, out string text)
	{
		y = 162;
		int num = 0;
		int num2 = 0;
		switch (_spellBookType)
		{
		case SpellBookType.Necromancy:
		{
			SpellDefinition spell = SpellsNecromancy.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Chivalry:
		{
			SpellDefinition spell = SpellsChivalry.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Bushido:
		{
			SpellDefinition spell = SpellsBushido.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Ninjitsu:
		{
			SpellDefinition spell = SpellsNinjitsu.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Spellweaving:
		{
			SpellDefinition spell = SpellsSpellweaving.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Mysticism:
		{
			SpellDefinition spell = SpellsMysticism.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			break;
		}
		case SpellBookType.Mastery:
		{
			SpellDefinition spell = SpellsMastery.GetSpell(offset + 1);
			num = spell.ManaCost;
			num2 = spell.MinSkill;
			if (spell.TithingCost > 0)
			{
				y = 148;
				text = string.Format(ResGumps.Upkeep0Mana1MinSkill2, spell.TithingCost, num, num2);
			}
			else
			{
				text = string.Format(ResGumps.ManaCost0MinSkill1, num, num2);
			}
			return;
		}
		}
		text = string.Format(ResGumps.ManaCost0MinSkill1, num, num2);
	}

	private void SetActivePage(int page)
	{
		if (page != _dataBox.ActivePage)
		{
			if (page < 1)
			{
				page = 1;
			}
			else if (page > _maxPage)
			{
				page = _maxPage;
			}
			_dataBox.ActivePage = page;
			_pageCornerLeft.Page = ((_dataBox.ActivePage == 1) ? int.MaxValue : 0);
			_pageCornerRight.Page = ((_dataBox.ActivePage == _maxPage) ? int.MaxValue : 0);
			Client.Game.Scene.Audio.PlaySound(85);
		}
	}

	private void OnClicked(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && Mouse.LDragOffset == Point.Zero && sender is HoveredLabel lastPressed)
		{
			_clickTiming += 350f;
			if (_clickTiming > 0f)
			{
				_lastPressed = lastPressed;
			}
		}
	}

	private void OnDoubleClicked(object sender, MouseDoubleClickEventArgs e)
	{
		if (_lastPressed != null && e.Button == MouseButtonType.Left)
		{
			_clickTiming = -350f;
			SpellDefinition spellDefinition = GetSpellDefinition((int)_lastPressed.Tag);
			if (spellDefinition != null)
			{
				GameActions.CastSpell(spellDefinition.ID);
			}
			_lastPressed = null;
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (World.Items.Get(base.LocalSerial) == null)
		{
			Dispose();
		}
		else if (!base.IsDisposed && _lastPressed != null)
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

	private void AssignGraphic(Item item)
	{
		switch (item.Graphic)
		{
		default:
			_spellBookType = SpellBookType.Magery;
			break;
		case 8787:
			_spellBookType = SpellBookType.Necromancy;
			break;
		case 8786:
			_spellBookType = SpellBookType.Chivalry;
			break;
		case 9100:
			if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) != 0)
			{
				_spellBookType = SpellBookType.Bushido;
			}
			break;
		case 9120:
			if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) != 0)
			{
				_spellBookType = SpellBookType.Ninjitsu;
			}
			break;
		case 11600:
			_spellBookType = SpellBookType.Spellweaving;
			break;
		case 11677:
			_spellBookType = SpellBookType.Mysticism;
			break;
		case 8794:
		case 8795:
			_spellBookType = SpellBookType.Mastery;
			break;
		}
	}

	private void PageCornerOnMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && Mouse.LDragOffset == Point.Zero && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? (_dataBox.ActivePage - 1) : (_dataBox.ActivePage + 1));
		}
	}

	private void PageCornerOnMouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && sender is Control control)
		{
			SetActivePage((control.LocalSerial == 0) ? 1 : _maxPage);
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((ButtonCircle)buttonID)
		{
		case ButtonCircle.Circle_1_2:
			SetActivePage(1);
			break;
		case ButtonCircle.Circle_3_4:
			SetActivePage(2);
			break;
		case ButtonCircle.Circle_5_6:
			SetActivePage(3);
			break;
		case ButtonCircle.Circle_7_8:
			SetActivePage(4);
			break;
		}
	}
}
