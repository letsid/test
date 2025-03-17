using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class HealthBarGump : BaseHealthBarGump
{
	private enum ButtonParty
	{
		Heal1,
		Heal2
	}

	private const ushort BACKGROUND_NORMAL = 2051;

	private const ushort BACKGROUND_WAR = 2055;

	private const ushort LINE_RED = 2053;

	private const ushort LINE_BLUE = 2054;

	private const ushort LINE_POISONED = 2056;

	private const ushort LINE_YELLOWHITS = 2057;

	private const ushort LINE_RED_PARTY = 40;

	private const ushort LINE_BLUE_PARTY = 41;

	private GumpPic _background;

	private GumpPic _hpLineRed;

	private GumpPic _manaLineRed;

	private GumpPic _stamLineRed;

	private readonly GumpPicWithWidth[] _bars = new GumpPicWithWidth[3];

	private Button _buttonHeal1;

	private Button _buttonHeal2;

	private int _oldHits;

	private int _oldStam;

	private int _oldMana;

	private bool _oldWarMode;

	private bool _normalHits;

	private bool _poisoned;

	private bool _yellowHits;

	public override int GroupMatrixWidth
	{
		get
		{
			return base.Width;
		}
		protected set
		{
		}
	}

	public override int GroupMatrixHeight
	{
		get
		{
			return base.Height;
		}
		protected set
		{
		}
	}

	public HealthBarGump(Entity entity)
		: base(entity)
	{
	}

	public HealthBarGump(uint serial)
		: base(serial)
	{
	}

	public HealthBarGump()
		: base(0u, 0u)
	{
	}

	protected override void UpdateContents()
	{
		Clear();
		base.Children.Clear();
		_background = (_hpLineRed = (_manaLineRed = (_stamLineRed = null)));
		_buttonHeal1 = (_buttonHeal2 = null);
		if (_textBox != null)
		{
			_textBox.MouseUp -= base.TextBoxOnMouseUp;
		}
		_textBox = null;
		BuildGump();
	}

	protected override void BuildGump()
	{
		base.WantUpdateSize = false;
		Entity entity = World.Get(base.LocalSerial);
		if (World.Party.Contains(base.LocalSerial) && !(this is TargetFrameHealthbarGump))
		{
			Add(_background = new GumpPic(0, 0, 2051, 0)
			{
				ContainsByBounds = true,
				Alpha = 0f
			});
			base.Width = 115;
			base.Height = 55;
			if (base.LocalSerial == (uint)World.Player)
			{
				StbTextBox stbTextBox = new StbTextBox(3, 32, 120, isunicode: false, FontStyle.Fixed, Notoriety.GetHue(World.Player.NotorietyFlag));
				stbTextBox.X = 0;
				stbTextBox.Y = -2;
				stbTextBox.Width = 120;
				stbTextBox.Height = 50;
				stbTextBox.IsEditable = false;
				stbTextBox.CanMove = true;
				StbTextBox c = stbTextBox;
				_textBox = stbTextBox;
				Add(c);
				_name = ResGumps.Self;
			}
			else
			{
				StbTextBox stbTextBox2 = new StbTextBox(3, 32, 109, isunicode: false, FontStyle.BlackBorder | FontStyle.Fixed, Notoriety.GetHue((entity as Mobile)?.NotorietyFlag ?? NotorietyFlag.Gray));
				stbTextBox2.X = 0;
				stbTextBox2.Y = -2;
				stbTextBox2.Width = 109;
				stbTextBox2.Height = 50;
				stbTextBox2.IsEditable = false;
				stbTextBox2.CanMove = true;
				StbTextBox c = stbTextBox2;
				_textBox = stbTextBox2;
				Add(c);
			}
			Button obj = new Button(0, 2360, 2362, 2360, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj.X = 0;
			obj.Y = 20;
			Button c2 = obj;
			_buttonHeal1 = obj;
			Add(c2);
			Button obj2 = new Button(1, 2361, 2362, 2361, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj2.X = 0;
			obj2.Y = 33;
			c2 = obj2;
			_buttonHeal2 = obj2;
			Add(c2);
			Add(_hpLineRed = new GumpPic(18, 20, 40, 0));
			Add(_manaLineRed = new GumpPic(18, 33, 40, 0));
			Add(_stamLineRed = new GumpPic(18, 45, 40, 0));
			Add(_bars[0] = new GumpPicWithWidth(18, 20, 41, 0, 96));
			Add(_bars[1] = new GumpPicWithWidth(18, 33, 41, 0, 96));
			Add(_bars[2] = new GumpPicWithWidth(18, 45, 41, 0, 96));
		}
		else if (base.LocalSerial == (uint)World.Player)
		{
			_oldWarMode = World.Player.InWarMode;
			Add(_background = new GumpPic(0, 0, (ushort)(_oldWarMode ? 2055 : 2051), 0)
			{
				ContainsByBounds = true
			});
			base.Width = _background.Width;
			base.Height = _background.Height;
			Add(_hpLineRed = new GumpPic(34, 12, 2053, 0));
			Add(new GumpPic(34, 25, 2053, 0));
			Add(new GumpPic(34, 38, 2053, 0));
			Add(_bars[0] = new GumpPicWithWidth(34, 12, 2054, 0, 0));
			Add(_bars[1] = new GumpPicWithWidth(34, 25, 2054, 0, 0));
			Add(_bars[2] = new GumpPicWithWidth(34, 38, 2054, 0, 0));
		}
		else
		{
			ushort hue = 902;
			ushort hue2 = 902;
			Mobile mobile = entity as Mobile;
			if (entity != null)
			{
				hue2 = 0;
				_canChangeName = mobile != null && mobile.IsRenamable;
				if (_canChangeName)
				{
					hue = 14;
				}
			}
			ushort num = (ushort)((!(entity == null) && !(entity == World.Player) && !(mobile == null) && mobile.NotorietyFlag != NotorietyFlag.Criminal && mobile.NotorietyFlag != NotorietyFlag.Gray) ? Notoriety.GetHue(mobile.NotorietyFlag) : 0);
			num &= 0x3FFF;
			Add(_background = new GumpPic(0, 0, 2052, num)
			{
				ContainsByBounds = true
			});
			Add(_hpLineRed = new GumpPic(34, 38, 2053, hue2));
			Add(_bars[0] = new GumpPicWithWidth(34, 38, 2054, 0, 0));
			base.Width = _background.Width;
			base.Height = _background.Height;
			StbTextBox stbTextBox3 = new StbTextBox(1, 32, 120, isunicode: false, FontStyle.Fixed, hue);
			stbTextBox3.X = 16;
			stbTextBox3.Y = 14;
			stbTextBox3.Width = 120;
			stbTextBox3.Height = 15;
			stbTextBox3.IsEditable = false;
			stbTextBox3.AcceptMouseInput = _canChangeName;
			stbTextBox3.AcceptKeyboardInput = _canChangeName;
			stbTextBox3.WantUpdateSize = false;
			stbTextBox3.CanMove = true;
			StbTextBox c = stbTextBox3;
			_textBox = stbTextBox3;
			Add(c);
		}
		if (_textBox != null)
		{
			_textBox.MouseUp += base.TextBoxOnMouseUp;
			_textBox.SetText(_name);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (base.IsDisposed)
		{
			return;
		}
		bool flag = World.Party.Contains(base.LocalSerial) && !(this is TargetFrameHealthbarGump);
		ushort num = 902;
		ushort num2 = 902;
		Entity entity = World.Get(base.LocalSerial);
		if (entity is Item { Layer: Layer.Invalid } item && item.Container == (uint)World.Player)
		{
			entity = null;
		}
		if (entity == null || entity.IsDestroyed)
		{
			if (base.LocalSerial != (uint)World.Player && (ProfileManager.CurrentProfile.CloseHealthBarType == 1 || (ProfileManager.CurrentProfile.CloseHealthBarType == 2 && World.CorpseManager.Exists(0u, base.LocalSerial | 0x80000000u))) && CheckIfAnchoredElseDispose())
			{
				return;
			}
			if (_isDead)
			{
				_isDead = false;
			}
			if (!_outOfRange)
			{
				_outOfRange = true;
				if (TargetManager.LastAttack != base.LocalSerial)
				{
					GameActions.SendCloseStatus(base.LocalSerial);
				}
				if (flag)
				{
					num2 = (num = 912);
					if (_textBox != null && _textBox.Hue != num)
					{
						_textBox.Hue = num;
					}
					Button buttonHeal = _buttonHeal1;
					bool isVisible = (_buttonHeal2.IsVisible = false);
					buttonHeal.IsVisible = isVisible;
					if (_bars.Length >= 2 && _bars[1] != null)
					{
						_bars[1].IsVisible = false;
						_bars[2].IsVisible = false;
					}
				}
				else if (_textBox != null)
				{
					if (_textBox.Hue != num)
					{
						_textBox.Hue = num;
					}
					_textBox.IsEditable = false;
				}
				if (_background.Hue != 0)
				{
					_background.Hue = 0;
				}
				if (_hpLineRed.Hue != num2)
				{
					_hpLineRed.Hue = num2;
					if (_manaLineRed != null && _stamLineRed != null)
					{
						GumpPic manaLineRed = _manaLineRed;
						ushort hue = (_stamLineRed.Hue = num2);
						manaLineRed.Hue = hue;
					}
				}
				_bars[0].IsVisible = false;
			}
		}
		if (entity != null && !entity.IsDestroyed)
		{
			_hpLineRed.IsVisible = entity.HitsMax > 0;
			Mobile mobile = entity as Mobile;
			if (!_isDead && entity != World.Player && mobile != null && mobile.IsDead && ProfileManager.CurrentProfile.CloseHealthBarType == 2 && CheckIfAnchoredElseDispose())
			{
				return;
			}
			if (entity is Mobile mobile2 && _canChangeName != mobile2.IsRenamable)
			{
				_canChangeName = mobile2.IsRenamable;
				if (_textBox != null)
				{
					_textBox.AcceptMouseInput = _canChangeName;
					_textBox.AcceptKeyboardInput = _canChangeName;
					if (!_canChangeName)
					{
						_textBox.IsEditable = false;
					}
				}
			}
			if ((!(mobile != null) || !mobile.IsDead) && _isDead)
			{
				_isDead = false;
			}
			if (!string.IsNullOrEmpty(entity.Name) && (!flag || base.LocalSerial != World.Player.Serial) && _name != entity.Name)
			{
				_name = entity.Name;
				if (_textBox != null)
				{
					_textBox.SetText(_name);
				}
			}
			if (_outOfRange)
			{
				if (entity.HitsMax == 0)
				{
					GameActions.RequestMobileStatus(entity);
				}
				_outOfRange = false;
				_canChangeName = !flag && mobile != null && mobile.IsRenamable;
				num2 = 0;
				if (flag)
				{
					Button buttonHeal2 = _buttonHeal1;
					bool isVisible = (_buttonHeal2.IsVisible = true);
					buttonHeal2.IsVisible = isVisible;
					if (_bars.Length >= 2 && _bars[1] != null)
					{
						_bars[1].IsVisible = true;
						_bars[2].IsVisible = true;
					}
				}
				if (_hpLineRed.Hue != num2)
				{
					_hpLineRed.Hue = num2;
					if (_manaLineRed != null && _stamLineRed != null)
					{
						GumpPic manaLineRed2 = _manaLineRed;
						ushort hue = (_stamLineRed.Hue = num2);
						manaLineRed2.Hue = hue;
					}
				}
				_bars[0].IsVisible = true;
			}
			if (flag && mobile != null)
			{
				num = Notoriety.GetHue(mobile.NotorietyFlag);
			}
			else if (_canChangeName)
			{
				num = 14;
			}
			if (_textBox != null && _textBox.Hue != num)
			{
				_textBox.Hue = num;
			}
			ushort num5 = (ushort)((!(entity == null) && !(entity == World.Player) && !(mobile == null) && mobile.NotorietyFlag != NotorietyFlag.Criminal && mobile.NotorietyFlag != NotorietyFlag.Gray) ? Notoriety.GetHue(mobile.NotorietyFlag) : 0);
			num5 &= 0x3FFF;
			if (_background.Hue != num5)
			{
				_background.Hue = num5;
			}
			if (mobile != null && mobile.IsPoisoned && !_poisoned)
			{
				if (flag)
				{
					_bars[0].Hue = 63;
				}
				else
				{
					_bars[0].Graphic = 2056;
				}
				_poisoned = true;
				_normalHits = false;
			}
			else if (mobile != null && mobile.IsYellowHits && !_yellowHits)
			{
				if (flag)
				{
					_bars[0].Hue = 353;
				}
				else
				{
					_bars[0].Graphic = 2057;
				}
				_yellowHits = true;
				_normalHits = false;
			}
			else if (!_normalHits && mobile != null && !mobile.IsPoisoned && !mobile.IsYellowHits && (_poisoned || _yellowHits))
			{
				if (flag)
				{
					_bars[0].Hue = 0;
				}
				else
				{
					_bars[0].Graphic = 2054;
				}
				_poisoned = false;
				_yellowHits = false;
				_normalHits = true;
			}
			int maxValue = (flag ? 96 : 109);
			int num6 = BaseHealthBarGump.CalculatePercents(entity.HitsMax, entity.Hits, maxValue);
			if (num6 != _oldHits)
			{
				_bars[0].Percent = num6;
				_oldHits = num6;
			}
			if ((flag || base.LocalSerial == (uint)World.Player) && mobile != null)
			{
				int num7 = BaseHealthBarGump.CalculatePercents(mobile.ManaMax, mobile.Mana, maxValue);
				int num8 = BaseHealthBarGump.CalculatePercents(mobile.StaminaMax, mobile.Stamina, maxValue);
				if (num7 != _oldMana && _bars.Length >= 2 && _bars[1] != null)
				{
					_bars[1].Percent = num7;
					_oldMana = num7;
				}
				if (num8 != _oldStam && _bars.Length >= 2 && _bars[2] != null)
				{
					_bars[2].Percent = num8;
					_oldStam = num8;
				}
			}
			if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.RootParent == this)
			{
				SelectedObject.HealthbarObject = entity;
				SelectedObject.Object = entity;
			}
		}
		if (base.LocalSerial == (uint)World.Player && World.Player.InWarMode != _oldWarMode)
		{
			_oldWarMode = !_oldWarMode;
			_background.Graphic = (ushort)(World.Player.InWarMode ? 2055 : 2051);
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((ButtonParty)buttonID)
		{
		case ButtonParty.Heal1:
			GameActions.CastSpell(29);
			World.Party.PartyHealTimer = Time.Ticks + 15;
			World.Party.PartyHealTarget = base.LocalSerial;
			break;
		case ButtonParty.Heal2:
			GameActions.CastSpell(11);
			World.Party.PartyHealTimer = Time.Ticks + 15;
			World.Party.PartyHealTarget = base.LocalSerial;
			break;
		}
		Mouse.CancelDoubleClick = true;
		Mouse.LastLeftButtonClickTime = 0u;
	}
}
