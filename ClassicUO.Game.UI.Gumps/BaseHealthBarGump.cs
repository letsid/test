using System;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class BaseHealthBarGump : AnchorableGump
{
	private bool _targetBroke;

	protected bool _canChangeName;

	protected bool _isDead;

	protected string _name;

	protected bool _outOfRange;

	protected StbTextBox _textBox;

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

	public override GumpType GumpType => GumpType.HealthBar;

	protected BaseHealthBarGump(Entity entity)
		: this(0u, 0u)
	{
		if (entity == null || entity.IsDestroyed)
		{
			Dispose();
			return;
		}
		GameActions.RequestMobileStatus(entity.Serial, force: true);
		base.LocalSerial = entity.Serial;
		base.CanCloseWithRightClick = true;
		_name = entity.Name;
		_isDead = entity is Mobile mobile && mobile.IsDead;
		BuildGump();
	}

	protected BaseHealthBarGump(uint serial)
		: this(World.Get(serial))
	{
	}

	protected BaseHealthBarGump(uint local, uint server)
		: base(local, server)
	{
		CanMove = true;
		base.AnchorType = ANCHOR_TYPE.HEALTHBAR;
	}

	protected abstract void BuildGump();

	public override void Dispose()
	{
		_textBox?.Dispose();
		_textBox = null;
		base.Dispose();
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		if (ProfileManager.CurrentProfile.SaveHealthbars)
		{
			writer.WriteAttributeString("name", _name);
		}
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		if (base.LocalSerial == (uint)World.Player)
		{
			_name = World.Player.Name;
			BuildGump();
		}
		else if (ProfileManager.CurrentProfile.SaveHealthbars)
		{
			_name = xml.GetAttribute("name");
			_outOfRange = true;
			BuildGump();
		}
		else
		{
			Dispose();
		}
	}

	protected void TextBoxOnMouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtonType.Left || World.Get(base.LocalSerial) == null)
		{
			return;
		}
		Point lDragOffset = Mouse.LDragOffset;
		if (Math.Max(Math.Abs(lDragOffset.X), Math.Abs(lDragOffset.Y)) < 1)
		{
			if (TargetManager.IsTargeting)
			{
				TargetManager.Target(base.LocalSerial);
				Mouse.LastLeftButtonClickTime = 0u;
			}
			else if (_canChangeName && !_targetBroke)
			{
				_textBox.IsEditable = true;
				_textBox.SetKeyboardFocus();
			}
			_targetBroke = false;
		}
	}

	protected static int CalculatePercents(int max, int current, int maxValue)
	{
		if (max > 0)
		{
			max = current * 100 / max;
			if (max > 100)
			{
				max = 100;
			}
			if (max > 1)
			{
				max = maxValue * max / 100;
			}
		}
		return max;
	}

	protected override void OnDragEnd(int x, int y)
	{
		if (TargetManager.IsTargeting)
		{
			Mouse.LastLeftButtonClickTime = 0u;
			Mouse.CancelDoubleClick = true;
		}
		base.OnDragEnd(x, y);
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return;
		}
		if (TargetManager.IsTargeting)
		{
			_targetBroke = true;
			TargetManager.Target(base.LocalSerial);
			Mouse.LastLeftButtonClickTime = 0u;
		}
		else if (_canChangeName)
		{
			if (_textBox != null)
			{
				_textBox.IsEditable = false;
			}
			UIManager.KeyboardFocusControl = null;
			UIManager.SystemChat?.SetFocus();
		}
		base.OnMouseDown(x, y, button);
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return false;
		}
		if (_canChangeName)
		{
			if (_textBox != null)
			{
				_textBox.IsEditable = false;
			}
			UIManager.KeyboardFocusControl = null;
			UIManager.SystemChat?.SetFocus();
		}
		Entity entity = World.Get(base.LocalSerial);
		if (entity != null)
		{
			if (entity != World.Player)
			{
				if (World.Player.InWarMode)
				{
					GameActions.Attack(entity);
				}
				else if (!GameActions.OpenCorpse(entity))
				{
					GameActions.DoubleClick(entity);
				}
			}
			else if (UIManager.GetGump<StatusGumpBase>(null) == null)
			{
				UIManager.Add(StatusGumpBase.AddStatusGump(base.ScreenCoordinateX, base.ScreenCoordinateY));
			}
		}
		return true;
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		Entity entity = World.Get(base.LocalSerial);
		if (!(entity == null) && !SerialHelper.IsItem(entity.Serial) && (key == SDL.SDL_Keycode.SDLK_RETURN || key == SDL.SDL_Keycode.SDLK_KP_ENTER) && _textBox != null && _textBox.IsEditable)
		{
			GameActions.Rename(entity, _textBox.Text);
			UIManager.KeyboardFocusControl = null;
			UIManager.SystemChat?.SetFocus();
			_textBox.IsEditable = false;
		}
	}

	protected override void OnMouseOver(int x, int y)
	{
		Entity entity = World.Get(base.LocalSerial);
		if (entity != null)
		{
			SelectedObject.HealthbarObject = entity;
			SelectedObject.Object = entity;
		}
		base.OnMouseOver(x, y);
	}

	protected bool CheckIfAnchoredElseDispose()
	{
		if (UIManager.AnchorManager[this] == null && base.LocalSerial != (uint)World.Player)
		{
			Dispose();
			return true;
		}
		return false;
	}
}
