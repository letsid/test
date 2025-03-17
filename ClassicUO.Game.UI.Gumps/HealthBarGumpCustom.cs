using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class HealthBarGumpCustom : BaseHealthBarGump
{
	private enum ButtonParty
	{
		Heal1,
		Heal2
	}

	private class LineCHB : Line
	{
		public int LineWidth { get; set; }

		public Texture2D LineColor { get; set; }

		public LineCHB(int x, int y, int w, int h, uint color)
			: base(x, y, w, h, color)
		{
			LineWidth = w;
			LineColor = SolidColorTextureCache.GetTexture(new Color
			{
				PackedValue = color
			});
			CanMove = true;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha);
			batcher.Draw(LineColor, new Rectangle(x, y, LineWidth, base.Height), hueVector);
			return true;
		}
	}

	internal const int HPB_WIDTH = 120;

	internal const int HPB_HEIGHT_MULTILINE = 60;

	internal const int HPB_HEIGHT_SINGLELINE = 36;

	private const int HPB_BORDERSIZE = 1;

	private const int HPB_OUTLINESIZE = 1;

	internal const int HPB_BAR_WIDTH = 100;

	private const int HPB_BAR_HEIGHT = 8;

	private const int HPB_BAR_SPACELEFT = 10;

	private static Color HPB_COLOR_DRAW_RED = Color.Red;

	private static Color HPB_COLOR_DRAW_BLUE = Color.DodgerBlue;

	private static Color HPB_COLOR_DRAW_BLACK = Color.Black;

	private static readonly Texture2D HPB_COLOR_BLUE = SolidColorTextureCache.GetTexture(Color.DodgerBlue);

	private static readonly Texture2D HPB_COLOR_GRAY = SolidColorTextureCache.GetTexture(Color.Gray);

	private static readonly Texture2D HPB_COLOR_RED = SolidColorTextureCache.GetTexture(Color.Red);

	private static readonly Texture2D HPB_COLOR_YELLOW = SolidColorTextureCache.GetTexture(Color.Orange);

	private static readonly Texture2D HPB_COLOR_POISON = SolidColorTextureCache.GetTexture(Color.LimeGreen);

	private static readonly Texture2D HPB_COLOR_BLACK = SolidColorTextureCache.GetTexture(Color.Black);

	private readonly LineCHB[] _bars = new LineCHB[3];

	private readonly LineCHB[] _border = new LineCHB[4];

	private LineCHB _hpLineRed;

	private LineCHB _manaLineRed;

	private LineCHB _stamLineRed;

	private LineCHB _outline;

	private Button _buttonHeal1;

	private Button _buttonHeal2;

	private bool _oldWarMode;

	private bool _normalHits;

	private bool _poisoned;

	private bool _yellowHits;

	protected AlphaBlendControl _background;

	public HealthBarGumpCustom(Entity entity)
		: base(entity)
	{
	}

	public HealthBarGumpCustom(uint serial)
		: base(serial)
	{
	}

	public HealthBarGumpCustom()
		: base(0u, 0u)
	{
	}

	protected override void UpdateContents()
	{
		Clear();
		base.Children.Clear();
		_background = null;
		_hpLineRed = (_manaLineRed = (_stamLineRed = null));
		_buttonHeal1 = (_buttonHeal2 = null);
		if (_textBox != null)
		{
			_textBox.MouseUp -= base.TextBoxOnMouseUp;
		}
		_textBox = null;
		BuildGump();
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (base.IsDisposed)
		{
			return;
		}
		bool flag = World.Party.Contains(base.LocalSerial) && !(this is TargetFrameHealthbarGumpCustom);
		ushort num = 902;
		Entity entity = World.Get(base.LocalSerial);
		if (entity is Item { Layer: Layer.Invalid } item && item.Container == (uint)World.Player)
		{
			entity = null;
		}
		if (entity == null || entity.IsDestroyed)
		{
			if (base.LocalSerial != (uint)World.Player && (ProfileManager.CurrentProfile.CloseHealthBarType == 1 || (ProfileManager.CurrentProfile.CloseHealthBarType == 2 && World.CorpseManager.Exists(0u, base.LocalSerial | 0x80000000u))) && !flag && CheckIfAnchoredElseDispose())
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
				num = 912;
				if (TargetManager.LastAttack != base.LocalSerial)
				{
					GameActions.SendCloseStatus(base.LocalSerial);
				}
				if (flag)
				{
					if (_textBox != null && _textBox.Hue != num)
					{
						_textBox.Hue = num;
					}
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
				if (_background.Hue != 912)
				{
					_background.Hue = 912;
				}
				if (_hpLineRed.LineColor != HPB_COLOR_GRAY)
				{
					_hpLineRed.LineColor = HPB_COLOR_GRAY;
					LineCHB obj = _border[0];
					LineCHB obj2 = _border[1];
					LineCHB obj3 = _border[2];
					Texture2D texture2D = (_border[3].LineColor = HPB_COLOR_BLACK);
					Texture2D texture2D3 = (obj3.LineColor = texture2D);
					Texture2D lineColor = (obj2.LineColor = texture2D3);
					obj.LineColor = lineColor;
					if (_manaLineRed != null && _stamLineRed != null)
					{
						LineCHB manaLineRed = _manaLineRed;
						lineColor = (_stamLineRed.LineColor = HPB_COLOR_GRAY);
						manaLineRed.LineColor = lineColor;
					}
				}
				_bars[0].IsVisible = false;
			}
		}
		if (entity != null && !entity.IsDestroyed)
		{
			_hpLineRed.IsVisible = entity.HitsMax > 0;
			Mobile mobile = entity as Mobile;
			if (!_isDead && entity != World.Player && mobile != null && mobile.IsDead && ProfileManager.CurrentProfile.CloseHealthBarType == 2 && !flag && CheckIfAnchoredElseDispose())
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
			if (!string.IsNullOrEmpty(entity.Name) && _name != entity.Name)
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
				_canChangeName = mobile != null && mobile.IsRenamable;
				if (_canChangeName)
				{
					num = 14;
				}
				if (flag && _buttonHeal1 != null && _buttonHeal2 != null)
				{
					Button buttonHeal = _buttonHeal1;
					bool isVisible = (_buttonHeal2.IsVisible = true);
					buttonHeal.IsVisible = isVisible;
				}
				if (flag && _bars.Length >= 2 && _bars[1] != null)
				{
					_bars[1].IsVisible = true;
					_bars[2].IsVisible = true;
				}
				if (_hpLineRed.LineColor != HPB_COLOR_RED)
				{
					_hpLineRed.LineColor = HPB_COLOR_RED;
					LineCHB obj4 = _border[0];
					LineCHB obj5 = _border[1];
					LineCHB obj6 = _border[2];
					Texture2D texture2D = (_border[3].LineColor = HPB_COLOR_BLACK);
					Texture2D texture2D3 = (obj6.LineColor = texture2D);
					Texture2D lineColor = (obj5.LineColor = texture2D3);
					obj4.LineColor = lineColor;
					if (_manaLineRed != null && _stamLineRed != null)
					{
						LineCHB manaLineRed2 = _manaLineRed;
						lineColor = (_stamLineRed.LineColor = HPB_COLOR_RED);
						manaLineRed2.LineColor = lineColor;
					}
				}
				_bars[0].IsVisible = true;
			}
			if (TargetManager.LastTargetInfo.Serial != (uint)World.Player && !_outOfRange && mobile != null)
			{
				if ((uint)mobile == TargetManager.LastTargetInfo.Serial)
				{
					_border[0].LineColor = HPB_COLOR_RED;
					if (_border.Length >= 3)
					{
						LineCHB obj7 = _border[1];
						LineCHB obj8 = _border[2];
						Texture2D texture2D3 = (_border[3].LineColor = HPB_COLOR_RED);
						Texture2D lineColor = (obj8.LineColor = texture2D3);
						obj7.LineColor = lineColor;
					}
				}
				else if ((uint)mobile != TargetManager.LastTargetInfo.Serial)
				{
					_border[0].LineColor = HPB_COLOR_BLACK;
					if (_border.Length >= 3)
					{
						LineCHB obj9 = _border[1];
						LineCHB obj10 = _border[2];
						Texture2D texture2D3 = (_border[3].LineColor = HPB_COLOR_BLACK);
						Texture2D lineColor = (obj10.LineColor = texture2D3);
						obj9.LineColor = lineColor;
					}
				}
			}
			if (mobile != null)
			{
				num = Notoriety.GetHue(mobile.NotorietyFlag);
			}
			if (_textBox != null && _textBox.Hue != num)
			{
				_textBox.Hue = num;
			}
			ushort num2 = (ushort)((mobile != null) ? Notoriety.GetHue(mobile.NotorietyFlag) : 912);
			if (_background.Hue != num2)
			{
				if (mobile != null && mobile.IsDead)
				{
					_background.Hue = 912;
				}
				else if (!ProfileManager.CurrentProfile.CBBlackBGToggled)
				{
					_background.Hue = num2;
				}
			}
			if (((mobile != null && mobile.IsDead) || ProfileManager.CurrentProfile.CBBlackBGToggled) && _background.Hue != 912)
			{
				_background.Hue = 912;
			}
			if (mobile != null)
			{
				if (mobile.IsPoisoned && !_poisoned)
				{
					_bars[0].LineColor = HPB_COLOR_POISON;
					_poisoned = true;
					_normalHits = false;
				}
				else if (mobile.IsYellowHits && !_yellowHits)
				{
					_bars[0].LineColor = HPB_COLOR_YELLOW;
					_yellowHits = true;
					_normalHits = false;
				}
				else if (!_normalHits && !mobile.IsPoisoned && !mobile.IsYellowHits && (_poisoned || _yellowHits))
				{
					_bars[0].LineColor = HPB_COLOR_BLUE;
					_poisoned = false;
					_yellowHits = false;
					_normalHits = true;
				}
			}
			int num3 = BaseHealthBarGump.CalculatePercents(entity.HitsMax, entity.Hits, 100);
			if (num3 != _bars[0].LineWidth)
			{
				_bars[0].LineWidth = num3;
			}
			if ((flag || base.LocalSerial == (uint)World.Player) && mobile != null && _bars != null)
			{
				int num4 = BaseHealthBarGump.CalculatePercents(mobile.ManaMax, mobile.Mana, 100);
				int num5 = BaseHealthBarGump.CalculatePercents(mobile.StaminaMax, mobile.Stamina, 100);
				if (_bars.Length >= 2 && _bars[1] != null && num4 != _bars[1].LineWidth)
				{
					_bars[1].LineWidth = num4;
				}
				if (_bars.Length >= 2 && _bars[2] != null && num5 != _bars[2].LineWidth)
				{
					_bars[2].LineWidth = num5;
				}
			}
			if (UIManager.MouseOverControl != null && UIManager.MouseOverControl.RootParent == this)
			{
				SelectedObject.HealthbarObject = entity;
				SelectedObject.Object = entity;
			}
		}
		if (base.LocalSerial != (uint)World.Player || World.Player.InWarMode == _oldWarMode)
		{
			return;
		}
		_oldWarMode = !_oldWarMode;
		if (World.Player.InWarMode)
		{
			_border[0].LineColor = HPB_COLOR_RED;
			if (_border.Length >= 3)
			{
				LineCHB obj11 = _border[1];
				LineCHB obj12 = _border[2];
				Texture2D texture2D3 = (_border[3].LineColor = HPB_COLOR_RED);
				Texture2D lineColor = (obj12.LineColor = texture2D3);
				obj11.LineColor = lineColor;
			}
		}
		else
		{
			_border[0].LineColor = HPB_COLOR_BLACK;
			if (_border.Length >= 3)
			{
				LineCHB obj13 = _border[1];
				LineCHB obj14 = _border[2];
				Texture2D texture2D3 = (_border[3].LineColor = HPB_COLOR_BLACK);
				Texture2D lineColor = (obj14.LineColor = texture2D3);
				obj13.LineColor = lineColor;
			}
		}
	}

	protected override void BuildGump()
	{
		base.WantUpdateSize = false;
		Entity entity = World.Get(base.LocalSerial);
		if (World.Party.Contains(base.LocalSerial) && !(this is TargetFrameHealthbarGumpCustom))
		{
			base.Height = 60;
			base.Width = 120;
			AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.7f);
			alphaBlendControl.Width = base.Width;
			alphaBlendControl.Height = base.Height;
			alphaBlendControl.AcceptMouseInput = true;
			alphaBlendControl.CanMove = true;
			AlphaBlendControl c = alphaBlendControl;
			_background = alphaBlendControl;
			Add(c);
			if (base.LocalSerial == (uint)World.Player)
			{
				StbTextBox stbTextBox = new StbTextBox(1, 32, 120, isunicode: true, FontStyle.BlackBorder | FontStyle.Cropped, Notoriety.GetHue(World.Player.NotorietyFlag), TEXT_ALIGN_TYPE.TS_CENTER);
				stbTextBox.X = 0;
				stbTextBox.Y = 3;
				stbTextBox.Width = 100;
				stbTextBox.IsEditable = false;
				stbTextBox.CanMove = true;
				StbTextBox c2 = stbTextBox;
				_textBox = stbTextBox;
				Add(c2);
			}
			else
			{
				StbTextBox stbTextBox2 = new StbTextBox(1, 32, 120, isunicode: true, FontStyle.BlackBorder | FontStyle.Cropped, Notoriety.GetHue((entity as Mobile)?.NotorietyFlag ?? NotorietyFlag.Gray), TEXT_ALIGN_TYPE.TS_CENTER);
				stbTextBox2.X = 0;
				stbTextBox2.Y = 3;
				stbTextBox2.Width = 100;
				stbTextBox2.IsEditable = false;
				stbTextBox2.CanMove = true;
				StbTextBox c2 = stbTextBox2;
				_textBox = stbTextBox2;
				Add(c2);
			}
			Add(_outline = new LineCHB(9, 26, 102, 28, HPB_COLOR_DRAW_BLACK.PackedValue));
			Button obj = new Button(0, 2360, 2362, 2360, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj.X = 0;
			obj.Y = 28;
			Button c3 = obj;
			_buttonHeal1 = obj;
			Add(c3);
			Button obj2 = new Button(1, 2361, 2362, 2361, "", 0)
			{
				ButtonAction = ButtonAction.Activate
			};
			obj2.X = 0;
			obj2.Y = 41;
			c3 = obj2;
			_buttonHeal2 = obj2;
			Add(c3);
			Add(_hpLineRed = new LineCHB(10, 27, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(_manaLineRed = new LineCHB(10, 36, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(_stamLineRed = new LineCHB(10, 45, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(_bars[0] = new LineCHB(10, 27, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_bars[1] = new LineCHB(10, 36, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_bars[2] = new LineCHB(10, 45, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_border[0] = new LineCHB(0, 0, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[1] = new LineCHB(0, 59, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[2] = new LineCHB(0, 0, 1, 60, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[3] = new LineCHB(119, 0, 1, 60, HPB_COLOR_DRAW_BLACK.PackedValue));
		}
		else if (base.LocalSerial == (uint)World.Player)
		{
			_oldWarMode = World.Player.InWarMode;
			base.Height = 60;
			base.Width = 120;
			AlphaBlendControl alphaBlendControl2 = new AlphaBlendControl(0.7f);
			alphaBlendControl2.Width = base.Width;
			alphaBlendControl2.Height = base.Height;
			alphaBlendControl2.AcceptMouseInput = true;
			alphaBlendControl2.CanMove = true;
			AlphaBlendControl c = alphaBlendControl2;
			_background = alphaBlendControl2;
			Add(c);
			StbTextBox obj3 = new StbTextBox(1, 32, hue: Notoriety.GetHue((entity as Mobile)?.NotorietyFlag ?? NotorietyFlag.Gray), maxWidth: base.Width, isunicode: true, style: FontStyle.BlackBorder | FontStyle.Cropped, align: TEXT_ALIGN_TYPE.TS_CENTER);
			obj3.X = 0;
			obj3.Y = 3;
			obj3.Width = 100;
			obj3.IsEditable = false;
			obj3.CanMove = true;
			StbTextBox c2 = obj3;
			_textBox = obj3;
			Add(c2);
			Add(_outline = new LineCHB(9, 26, 102, 28, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_hpLineRed = new LineCHB(10, 27, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(new LineCHB(10, 36, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(new LineCHB(10, 45, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(_bars[0] = new LineCHB(10, 27, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_bars[1] = new LineCHB(10, 36, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_bars[2] = new LineCHB(10, 45, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_border[0] = new LineCHB(0, 0, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[1] = new LineCHB(0, 59, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[2] = new LineCHB(0, 0, 1, 60, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[3] = new LineCHB(119, 0, 1, 60, HPB_COLOR_DRAW_BLACK.PackedValue));
			LineCHB obj4 = _border[0];
			LineCHB obj5 = _border[1];
			LineCHB obj6 = _border[2];
			Texture2D texture2D2 = (_border[3].LineColor = (_oldWarMode ? HPB_COLOR_RED : HPB_COLOR_BLACK));
			Texture2D texture2D4 = (obj6.LineColor = texture2D2);
			Texture2D lineColor = (obj5.LineColor = texture2D4);
			obj4.LineColor = lineColor;
		}
		else
		{
			Mobile mobile = entity as Mobile;
			if (entity != null)
			{
				_canChangeName = mobile != null && mobile.IsRenamable;
			}
			base.Height = 36;
			base.Width = 120;
			AlphaBlendControl alphaBlendControl3 = new AlphaBlendControl(0.7f);
			alphaBlendControl3.Width = base.Width;
			alphaBlendControl3.Height = base.Height;
			alphaBlendControl3.AcceptMouseInput = true;
			alphaBlendControl3.CanMove = true;
			AlphaBlendControl c = alphaBlendControl3;
			_background = alphaBlendControl3;
			Add(c);
			Add(_outline = new LineCHB(9, 20, 102, 10, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_hpLineRed = new LineCHB(10, 21, 100, 8, HPB_COLOR_DRAW_RED.PackedValue));
			Add(_bars[0] = new LineCHB(10, 21, 100, 8, HPB_COLOR_DRAW_BLUE.PackedValue)
			{
				LineWidth = 0
			});
			Add(_border[0] = new LineCHB(0, 0, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[1] = new LineCHB(0, 35, 120, 1, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[2] = new LineCHB(0, 0, 1, 36, HPB_COLOR_DRAW_BLACK.PackedValue));
			Add(_border[3] = new LineCHB(119, 0, 1, 36, HPB_COLOR_DRAW_BLACK.PackedValue));
			StbTextBox stbTextBox3 = new StbTextBox(1, 32, 120, isunicode: true, FontStyle.BlackBorder | FontStyle.Cropped, Notoriety.GetHue((entity as Mobile)?.NotorietyFlag ?? NotorietyFlag.Gray), TEXT_ALIGN_TYPE.TS_CENTER);
			stbTextBox3.X = 0;
			stbTextBox3.Y = 0;
			stbTextBox3.Width = 120;
			stbTextBox3.Height = 15;
			stbTextBox3.IsEditable = false;
			stbTextBox3.AcceptMouseInput = _canChangeName;
			stbTextBox3.AcceptKeyboardInput = _canChangeName;
			stbTextBox3.WantUpdateSize = false;
			stbTextBox3.CanMove = true;
			StbTextBox c2 = stbTextBox3;
			_textBox = stbTextBox3;
			Add(c2);
		}
		_textBox.MouseUp += base.TextBoxOnMouseUp;
		_textBox.SetText(_name);
		if (!(entity == null))
		{
			return;
		}
		StbTextBox textBox = _textBox;
		ushort hue2 = (_background.Hue = 912);
		textBox.Hue = hue2;
		if (_hpLineRed.LineColor != HPB_COLOR_GRAY)
		{
			_hpLineRed.LineColor = HPB_COLOR_GRAY;
			LineCHB obj7 = _border[0];
			LineCHB obj8 = _border[1];
			LineCHB obj9 = _border[2];
			Texture2D texture2D2 = (_border[3].LineColor = HPB_COLOR_BLACK);
			Texture2D texture2D4 = (obj9.LineColor = texture2D2);
			Texture2D lineColor = (obj8.LineColor = texture2D4);
			obj7.LineColor = lineColor;
			if (_manaLineRed != null && _stamLineRed != null)
			{
				LineCHB manaLineRed = _manaLineRed;
				lineColor = (_stamLineRed.LineColor = HPB_COLOR_GRAY);
				manaLineRed.LineColor = lineColor;
			}
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

	public override bool Contains(int x, int y)
	{
		return true;
	}
}
