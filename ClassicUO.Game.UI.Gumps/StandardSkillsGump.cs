using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal class StandardSkillsGump : Gump
{
	private class SkillsGroupControl : Control
	{
		private readonly DataBox _box;

		private readonly Button _button;

		private readonly SkillsGroup _group;

		private readonly GumpPicTiled _gumpPic;

		private bool _isMinimized;

		private readonly List<SkillItemControl> _skills = new List<SkillItemControl>();

		private byte _status;

		private readonly StbTextBox _textbox;

		public int Count => _skills.Count;

		public bool IsMinimized
		{
			get
			{
				return _isMinimized;
			}
			set
			{
				ushort num = (ushort)(value ? 2087u : 2086u);
				_button.ButtonGraphicNormal = num;
				_button.ButtonGraphicOver = num;
				_button.ButtonGraphicPressed = num;
				_box.IsVisible = !value;
				_box.WantUpdateSize = true;
				base.Parent.WantUpdateSize = true;
				_isMinimized = value;
				base.WantUpdateSize = true;
			}
		}

		public SkillsGroupControl(SkillsGroup group, int x, int y)
		{
			CanMove = false;
			AcceptMouseInput = true;
			base.WantUpdateSize = true;
			AcceptKeyboardInput = true;
			base.X = x;
			base.Y = y;
			base.Width = 200;
			base.Height = 20;
			_group = group;
			_button = new Button(1000, 2087, 2087, 2087, "", 0)
			{
				ButtonAction = ButtonAction.Activate,
				ContainsByBounds = true,
				IsVisible = false
			};
			Add(_button);
			int widthASCII = FontsLoader.Instance.GetWidthASCII(6, group.Name);
			StbTextBox stbTextBox = new StbTextBox(6, -1, 200, isunicode: false, FontStyle.Fixed, 0);
			stbTextBox.X = 16;
			stbTextBox.Y = -3;
			stbTextBox.Width = 200;
			stbTextBox.Height = 17;
			stbTextBox.IsEditable = false;
			StbTextBox c = stbTextBox;
			_textbox = stbTextBox;
			Add(c);
			_textbox.SetText(group.Name);
			int num = widthASCII + 11 + 16;
			GumpPicTiled gumpPicTiled = new GumpPicTiled(2101);
			gumpPicTiled.X = num;
			gumpPicTiled.Y = 5;
			gumpPicTiled.Width = 215 - num;
			gumpPicTiled.AcceptMouseInput = false;
			_gumpPic = gumpPicTiled;
			Add(_gumpPic);
			Add(_box = new DataBox(0, 0, 0, 0));
			_textbox.IsEditable = false;
			_textbox.MouseDown += delegate
			{
				if (!_textbox.IsEditable || _status != 2)
				{
					_status++;
					if (_status >= 3)
					{
						_status = 0;
					}
					switch (_status)
					{
					default:
						_gumpPic.IsVisible = true;
						_textbox.IsEditable = false;
						_textbox.AllowSelection = false;
						UIManager.KeyboardFocusControl = this;
						UIManager.SystemChat.SetFocus();
						break;
					case 1:
						_gumpPic.IsVisible = true;
						_textbox.IsEditable = false;
						_textbox.AllowSelection = false;
						UIManager.KeyboardFocusControl = this;
						break;
					case 2:
						_gumpPic.IsVisible = false;
						_textbox.IsEditable = true;
						_textbox.AllowSelection = true;
						UIManager.KeyboardFocusControl = _textbox;
						_textbox.SetKeyboardFocus();
						break;
					}
				}
			};
			_textbox.FocusLost += delegate
			{
				_status = 0;
				_gumpPic.IsVisible = true;
				_textbox.IsEditable = false;
				_textbox.AllowSelection = false;
				UIManager.KeyboardFocusControl = null;
				UIManager.SystemChat.SetFocus();
			};
		}

		public void AddSkill(int index, int x, int y)
		{
			SkillItemControl skillItemControl = new SkillItemControl(index, x, y);
			_skills.Add(skillItemControl);
			_box.Add(skillItemControl);
			_box.WantUpdateSize = true;
			base.WantUpdateSize = true;
			if (!_button.IsVisible)
			{
				_button.IsVisible = true;
			}
		}

		public void UpdateAllSkillsValues(bool showReal, bool showCaps)
		{
			foreach (SkillItemControl skill in _skills)
			{
				skill.UpdateValueText(showReal, showCaps);
			}
		}

		public bool UpdateSkillValue(int index, bool showReal, bool showCaps)
		{
			foreach (SkillItemControl skill2 in _skills)
			{
				if (skill2.Index == index && index >= 0 && index < World.Player.Skills.Length)
				{
					Skill skill = World.Player.Skills[index];
					if (skill == null)
					{
						return true;
					}
					skill2.UpdateValueText(showReal, showCaps);
					skill2.SetStatus(skill.Lock);
					return true;
				}
			}
			return false;
		}

		protected override void OnMouseOver(int x, int y)
		{
			if (UIManager.LastControlMouseDown(MouseButtonType.Left) is SkillItemControl skillItemControl && skillItemControl.Parent.Parent != this)
			{
				SkillsGroupControl skillsGroupControl = (SkillsGroupControl)skillItemControl.Parent.Parent;
				if (skillsGroupControl != null && !_group.Contains((byte)skillItemControl.Index))
				{
					byte b = (byte)skillItemControl.Index;
					skillsGroupControl._skills.Remove(skillItemControl);
					skillsGroupControl._group.Remove(b);
					_group.Add(b);
					_group.Sort();
					skillsGroupControl._button.IsVisible = skillsGroupControl._skills.Count != 0;
					int count = _group.Count;
					for (int i = 0; i < count; i++)
					{
						if (_group.GetSkill(i) == b)
						{
							_skills.Insert(i, skillItemControl);
							_box.Insert(i, skillItemControl);
							if (!_button.IsVisible)
							{
								_button.IsVisible = true;
							}
							break;
						}
					}
					UpdateSkillsPosition();
					skillsGroupControl.UpdateSkillsPosition();
				}
			}
			base.OnMouseOver(x, y);
		}

		public override void OnKeyboardReturn(int textID, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				text = ResGumps.NoName;
				_textbox.SetText(text);
			}
			int num = FontsLoader.Instance.GetWidthASCII(6, text) + 11 + 16;
			if (num > 0)
			{
				_gumpPic.IsVisible = true;
				_gumpPic.X = num;
				_gumpPic.Width = 215 - num;
			}
			else
			{
				_gumpPic.IsVisible = false;
			}
			UIManager.KeyboardFocusControl = null;
			UIManager.SystemChat.SetFocus();
			_group.Name = text;
			base.OnKeyboardReturn(textID, text);
		}

		protected override void OnKeyUp(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
		{
			base.OnKeyUp(key, mod);
			if (key != SDL.SDL_Keycode.SDLK_DELETE || _status != 1 || !SkillsGroupManager.Remove(_group) || !(base.RootParent is StandardSkillsGump standardSkillsGump))
			{
				return;
			}
			SkillsGroupControl skillsGroupControl = standardSkillsGump._skillsControl[0];
			while (_box.Children.Count != 0)
			{
				SkillItemControl skillItemControl = (SkillItemControl)_box.Children[0];
				int count = skillsGroupControl._group.Count;
				for (int i = 0; i < count; i++)
				{
					if (skillsGroupControl._group.GetSkill(i) == skillItemControl.Index)
					{
						skillsGroupControl._skills.Insert(i, skillItemControl);
						skillsGroupControl._box.Insert(i, skillItemControl);
						if (!skillsGroupControl._button.IsVisible)
						{
							skillsGroupControl._button.IsVisible = true;
						}
						break;
					}
				}
			}
			_skills.Clear();
			Dispose();
			skillsGroupControl.UpdateSkillsPosition();
		}

		public override void OnButtonClick(int buttonID)
		{
			if (buttonID == 1000)
			{
				IsMinimized = !IsMinimized;
			}
		}

		private void UpdateSkillsPosition()
		{
			int num = 17;
			foreach (SkillItemControl skill in _skills)
			{
				skill.Y = num;
				num += 17;
			}
			_box.WantUpdateSize = true;
			base.WantUpdateSize = true;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			if (_status == 2)
			{
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Beige), new Rectangle(x, y, base.Width, 17), hueVector);
			}
			else if (_status == 1)
			{
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Bisque), new Rectangle(x + 16, y, 200, 17), hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private class SkillItemControl : Control
	{
		private readonly Button _buttonStatus;

		private Lock _status;

		private readonly Label _value;

		public readonly int Index;

		public SkillItemControl(int index, int x, int y)
		{
			Index = index;
			base.X = x;
			base.Y = y;
			if (index < 0 || index >= SkillsLoader.Instance.Skills.Count)
			{
				Dispose();
				return;
			}
			Skill skill = World.Player.Skills[Index];
			if (skill != null)
			{
				if (skill.IsClickable)
				{
					Button obj = new Button(0, 2103, 2104, 2104, "", 0)
					{
						ButtonAction = ButtonAction.Activate
					};
					obj.X = 8;
					Button c = obj;
					Add(c);
				}
				_status = skill.Lock;
				ushort statusButtonGraphic = GetStatusButtonGraphic();
				Button obj2 = new Button(1, statusButtonGraphic, statusButtonGraphic, statusButtonGraphic, "", 0)
				{
					ButtonAction = ButtonAction.Activate
				};
				obj2.X = 251;
				obj2.ContainsByBounds = true;
				_buttonStatus = obj2;
				Add(_buttonStatus);
				Label label;
				Add(label = new Label(skill.Name, isunicode: false, 648, 0, 9));
				label.X = 22;
				Add(_value = new Label("", isunicode: false, 648, 0, 9));
				UpdateValueText(showReal: false, showCap: false);
				base.Width = 255;
				base.Height = 17;
				base.WantUpdateSize = true;
				AcceptMouseInput = true;
				CanMove = false;
			}
			else
			{
				Dispose();
			}
		}

		public override void OnButtonClick(int buttonID)
		{
			switch (buttonID)
			{
			case 0:
				GameActions.UseSkill(Index);
				break;
			case 1:
				if (!(World.Player == null))
				{
					Skill obj = World.Player.Skills[Index];
					byte @lock = (byte)obj.Lock;
					@lock = (byte)((@lock < 2) ? ((byte)(@lock + 1)) : 0);
					NetClient.Socket.Send_SkillStatusChangeRequest((ushort)Index, @lock);
					obj.Lock = (Lock)@lock;
					SetStatus((Lock)@lock);
				}
				break;
			}
		}

		public void SetStatus(Lock status)
		{
			_status = status;
			ushort statusButtonGraphic = GetStatusButtonGraphic();
			_buttonStatus.ButtonGraphicNormal = statusButtonGraphic;
			_buttonStatus.ButtonGraphicOver = statusButtonGraphic;
			_buttonStatus.ButtonGraphicPressed = statusButtonGraphic;
		}

		public void UpdateValueText(bool showReal, bool showCap)
		{
			if (World.Player == null || Index < 0 || Index >= World.Player.Skills.Length)
			{
				return;
			}
			Skill skill = World.Player.Skills[Index];
			if (skill != null)
			{
				double num = skill.Value;
				if (showReal)
				{
					num = skill.Base;
				}
				else if (showCap)
				{
					num = skill.Cap;
				}
				_value.Text = $"{num:F1}";
				_value.X = 250 - _value.Width;
			}
		}

		private ushort GetStatusButtonGraphic()
		{
			return _status switch
			{
				Lock.Down => 2438, 
				Lock.Locked => 2092, 
				_ => 2436, 
			};
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (button != MouseButtonType.Left)
			{
				return;
			}
			UIManager.GameCursor.IsDraggingCursorForced = false;
			if (UIManager.LastControlMouseDown(MouseButtonType.Left) == this && World.Player.Skills[Index].IsClickable && (UIManager.MouseOverControl == null || UIManager.MouseOverControl.RootParent != base.RootParent))
			{
				GetSpellFloatingButton(Index)?.Dispose();
				if (Index >= 0 && Index < World.Player.Skills.Length)
				{
					UIManager.Add(new SkillButtonGump(World.Player.Skills[Index], Mouse.Position.X - 44, Mouse.Position.Y - 22));
				}
			}
		}

		private static SkillButtonGump GetSpellFloatingButton(int id)
		{
			for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
			{
				if (linkedListNode.Value is SkillButtonGump skillButtonGump && skillButtonGump.SkillID == id)
				{
					return skillButtonGump;
				}
			}
			return null;
		}

		protected override void OnMouseDown(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				UIManager.GameCursor.IsDraggingCursorForced = true;
			}
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			if (UIManager.LastControlMouseDown(MouseButtonType.Left) == this)
			{
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Wheat), new Rectangle(x, y, base.Width, base.Height), hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private const int _diffY = 22;

	private readonly ScrollArea _area;

	private readonly GumpPic _bottomComment;

	private readonly GumpPic _bottomLine;

	private readonly DataBox _container;

	private readonly GumpPic _gumpPic;

	private readonly HitBox _hitBox;

	private bool _isMinimized;

	private readonly Button _newGroupButton;

	private readonly ExpandableScroll _scrollArea;

	private readonly List<SkillsGroupControl> _skillsControl = new List<SkillsGroupControl>();

	private readonly Label _skillsLabelSum;

	private readonly NiceButton _resetGroups;

	internal Checkbox _checkReal;

	internal Checkbox _checkCaps;

	public override GumpType GumpType => GumpType.SkillMenu;

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
			_gumpPic.Graphic = (ushort)(value ? 2105 : 2093);
			if (value)
			{
				_gumpPic.X = 0;
			}
			else
			{
				_gumpPic.X = 160;
			}
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			_gumpPic.IsVisible = true;
			base.WantUpdateSize = true;
			_container.WantUpdateSize = true;
			_container.ReArrangeChildren();
		}
	}

	public StandardSkillsGump()
		: base(0u, 0u)
	{
		AcceptMouseInput = false;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		base.Height = 222;
		Add(_gumpPic = new GumpPic(160, 0, 2093, 0));
		_gumpPic.MouseDoubleClick += _picBase_MouseDoubleClick;
		_scrollArea = new ExpandableScroll(0, 22, base.Height, 8000)
		{
			TitleGumpID = 2100,
			AcceptMouseInput = true
		};
		Add(_scrollArea);
		Add(new GumpPic(50, 57, 2091, 0));
		Add(_bottomLine = new GumpPic(50, base.Height - 98, 2091, 0));
		Add(_bottomComment = new GumpPic(25, base.Height - 85, 2102, 0));
		_area = new ScrollArea(22, 67 + _bottomLine.Height - 10, _scrollArea.Width - 14, _scrollArea.Height - 105, normalScrollbar: false)
		{
			AcceptMouseInput = true,
			CanMove = true
		};
		Add(_area);
		_container = new DataBox(0, 0, 1, 1);
		_container.WantUpdateSize = true;
		_container.AcceptMouseInput = true;
		_container.CanMove = true;
		_area.Add(_container);
		Label label = new Label(World.Player.Skills.Sum((Skill s) => s.Value).ToString("F1"), isunicode: false, 600, 0, 3);
		label.X = _bottomComment.X + _bottomComment.Width + 5;
		label.Y = _bottomComment.Y - 5;
		Label c = label;
		_skillsLabelSum = label;
		Add(c);
		int num = 60;
		Button button = new Button(0, 2106, 2106, 2106, "", 0);
		button.X = num;
		button.Y = base.Height;
		button.ContainsByBounds = true;
		button.ButtonAction = ButtonAction.Activate;
		Button c2 = button;
		_newGroupButton = button;
		Add(c2);
		Checkbox checkbox = new Checkbox(2360, 2361, ResGumps.ShowReal, 1, 902, isunicode: false);
		checkbox.X = _newGroupButton.X + _newGroupButton.Width + 30;
		checkbox.Y = _newGroupButton.Y - 6;
		Checkbox c3 = checkbox;
		_checkReal = checkbox;
		Add(c3);
		Checkbox checkbox2 = new Checkbox(2360, 2361, ResGumps.ShowCaps, 1, 902, isunicode: false);
		checkbox2.X = _newGroupButton.X + _newGroupButton.Width + 30;
		checkbox2.Y = _newGroupButton.Y + 7;
		c3 = checkbox2;
		_checkCaps = checkbox2;
		Add(c3);
		_checkReal.ValueChanged += UpdateSkillsValues;
		_checkCaps.ValueChanged += UpdateSkillsValues;
		LoadSkills();
		Add(_resetGroups = new NiceButton(_scrollArea.X + 25, _scrollArea.Y + 7, 100, 18, ButtonAction.Activate, ResGumps.ResetGroups, 0, TEXT_ALIGN_TYPE.TS_CENTER, ushort.MaxValue, unicode: false, 6)
		{
			ButtonParameter = 1,
			IsSelectable = false
		});
		_hitBox = new HitBox(160, 0, 23, 24);
		Add(_hitBox);
		_hitBox.MouseUp += _hitBox_MouseUp;
		_container.ReArrangeChildren();
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

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
		{
			SkillsGroup skillsGroup = new SkillsGroup
			{
				Name = ResGumps.NewGroup
			};
			SkillsGroupManager.Add(skillsGroup);
			SkillsGroupControl skillsGroupControl = new SkillsGroupControl(skillsGroup, 3, 3);
			_skillsControl.Add(skillsGroupControl);
			_container.Add(skillsGroupControl);
			skillsGroupControl.IsMinimized = !skillsGroup.IsMaximized;
			_container.WantUpdateSize = true;
			_container.ReArrangeChildren();
			break;
		}
		case 1:
			UIManager.Add(new MessageBoxGump(300, 200, "Skills will be placed in default groups.\nDo you want reset all groups?", delegate(bool b)
			{
				if (b)
				{
					_skillsControl.Clear();
					_container.Clear();
					SkillsGroupManager.Groups.Clear();
					SkillsGroupManager.MakeDefault();
					LoadSkills();
					_container.WantUpdateSize = true;
					_container.ReArrangeChildren();
				}
			}, hasBackground: false, MessageButtonType.OK_CANCEL, 1));
			break;
		}
	}

	private void LoadSkills()
	{
		if (!(World.Player != null))
		{
			return;
		}
		foreach (SkillsGroup group in SkillsGroupManager.Groups)
		{
			SkillsGroupControl skillsGroupControl = new SkillsGroupControl(group, 3, 3);
			_skillsControl.Add(skillsGroupControl);
			_container.Add(skillsGroupControl);
			skillsGroupControl.IsMinimized = true;
			int count = group.Count;
			for (int i = 0; i < count; i++)
			{
				byte skill = group.GetSkill(i);
				if (skill < SkillsLoader.Instance.SkillsCount)
				{
					skillsGroupControl.AddSkill(skill, 0, 17 + i * 17);
				}
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.WantUpdateSize = true;
		bool wantUpdateSize = _container.WantUpdateSize;
		_bottomLine.Y = base.Height - 98;
		_bottomComment.Y = base.Height - 85;
		_area.Height = (_container.Height = base.Height - 172);
		_newGroupButton.Y = base.Height - 52;
		_skillsLabelSum.Y = _bottomComment.Y + 2;
		_checkReal.Y = _newGroupButton.Y - 6;
		_checkCaps.Y = _newGroupButton.Y + 7;
		base.Update(totalTime, frameTime);
		if (wantUpdateSize)
		{
			_container.ReArrangeChildren();
		}
	}

	public void Update(int skillIndex)
	{
		using (List<SkillsGroupControl>.Enumerator enumerator = _skillsControl.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.UpdateSkillValue(skillIndex, _checkReal.IsChecked, _checkCaps.IsChecked))
			{
			}
		}
		SumTotalSkills();
	}

	private void UpdateSkillsValues(object sender, EventArgs e)
	{
		Checkbox checkbox = (Checkbox)sender;
		if (_checkReal.IsChecked && _checkCaps.IsChecked)
		{
			if (checkbox == _checkReal)
			{
				_checkCaps.IsChecked = false;
			}
			else
			{
				_checkReal.IsChecked = false;
			}
		}
		foreach (SkillsGroupControl item in _skillsControl)
		{
			item.UpdateAllSkillsValues(_checkReal.IsChecked, _checkCaps.IsChecked);
		}
		SumTotalSkills();
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("isminimized", IsMinimized.ToString());
		writer.WriteAttributeString("height", _scrollArea.SpecialHeight.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		ref int height = ref _scrollArea.Height;
		int num2 = (_scrollArea.SpecialHeight = int.Parse(xml.GetAttribute("height")));
		height = num2;
	}

	private void SumTotalSkills()
	{
		_skillsLabelSum.Text = World.Player.Skills.Sum((Skill s) => (!_checkReal.IsChecked) ? s.Value : s.Base).ToString("F1");
	}
}
