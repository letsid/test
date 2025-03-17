using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class NameOverheadGump : Gump
{
	private AlphaBlendControl _background;

	private Point _lockedPosition;

	private bool _positionLocked;

	private readonly RenderedText _renderedText;

	private Texture2D _borderColor = SolidColorTextureCache.GetTexture(Color.Black);

	public NameOverheadGump(uint serial)
		: base(serial, 0u)
	{
		CanMove = false;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		Entity entity = World.Get(serial);
		if (entity == null)
		{
			Dispose();
			return;
		}
		_renderedText = RenderedText.Create(string.Empty, (ushort)((entity is Mobile mobile) ? Notoriety.GetHue(mobile.NotorietyFlag) : 1153), byte.MaxValue, isunicode: true, FontStyle.ForcedUnicode, TEXT_ALIGN_TYPE.TS_CENTER, 100, 30, isHTML: true);
		SetTooltip(entity);
		BuildGump();
	}

	public bool SetName()
	{
		Entity entity = World.Get(base.LocalSerial);
		if (entity == null)
		{
			return false;
		}
		if (entity is Item item)
		{
			if (!World.OPL.TryGetNameAndData(item, out var name, out var _))
			{
				name = StringHelper.CapitalizeAllWords(item.ItemData.Name);
				if (string.IsNullOrEmpty(name))
				{
					name = ClilocLoader.Instance.GetString(1020000 + item.Graphic, camelcase: true, name);
				}
			}
			if (!string.IsNullOrEmpty(name) && name.ToLower().Equals("unused") && item.IsMulti && World.Player != null && World.Player.NotorietyFlag == NotorietyFlag.Staff)
			{
				name = "Multi";
			}
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			if (!item.IsCorpse && item.Amount > 1)
			{
				name = name + ": " + item.Amount;
			}
			FontsLoader.Instance.SetUseHTML(value: true);
			FontsLoader.Instance.RecalculateWidthByInfo = true;
			int num = FontsLoader.Instance.GetWidthUnicode(_renderedText.Font, name);
			if (num > 100)
			{
				name = FontsLoader.Instance.GetTextByWidthUnicode(_renderedText.Font, name.AsSpan(), 100, isCropped: true, TEXT_ALIGN_TYPE.TS_CENTER, 8);
				num = 100;
			}
			_renderedText.MaxWidth = num;
			_renderedText.Text = name;
			FontsLoader.Instance.RecalculateWidthByInfo = false;
			FontsLoader.Instance.SetUseHTML(value: false);
			base.Width = (_background.Width = Math.Max(60, _renderedText.Width) + 4);
			base.Height = (_background.Height = 22);
			base.WantUpdateSize = false;
			return true;
		}
		if (!string.IsNullOrEmpty(entity.Name))
		{
			string text = entity.Name;
			int num2 = FontsLoader.Instance.GetWidthUnicode(_renderedText.Font, text);
			if (num2 > 100)
			{
				text = FontsLoader.Instance.GetTextByWidthUnicode(_renderedText.Font, text.AsSpan(), 100, isCropped: true, TEXT_ALIGN_TYPE.TS_CENTER, 8);
				num2 = 100;
			}
			_renderedText.MaxWidth = num2;
			_renderedText.Text = text;
			base.Width = (_background.Width = Math.Max(60, _renderedText.Width) + 4);
			base.Height = (_background.Height = 22);
			base.WantUpdateSize = false;
			return true;
		}
		return false;
	}

	private void BuildGump()
	{
		Entity entity = World.Get(base.LocalSerial);
		if (entity == null)
		{
			Dispose();
			return;
		}
		AlphaBlendControl obj = new AlphaBlendControl(0.7f)
		{
			WantUpdateSize = false,
			Hue = (ushort)((entity is Mobile mobile) ? Notoriety.GetHue(mobile.NotorietyFlag) : 1153)
		};
		AlphaBlendControl c = obj;
		_background = obj;
		Add(c);
	}

	protected override void CloseWithRightClick()
	{
		Entity entity = World.Get(base.LocalSerial);
		if (entity != null)
		{
			entity.ObjectHandlesStatus = ObjectHandlesStatus.CLOSED;
		}
		base.CloseWithRightClick();
	}

	protected override void OnDragBegin(int x, int y)
	{
		_positionLocked = false;
		Entity entity = World.Get(base.LocalSerial);
		if (entity is Mobile || entity is Item { IsDamageable: not false })
		{
			if (!UIManager.IsDragging)
			{
				UIManager.GetGump<BaseHealthBarGump>(base.LocalSerial)?.Dispose();
				if (entity == World.Player)
				{
					StatusGumpBase.GetStatusGump()?.Dispose();
				}
				BaseHealthBarGump control;
				if (ProfileManager.CurrentProfile.CustomBarsToggled)
				{
					Rectangle rectangle = new Rectangle(0, 0, 120, 36);
					HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(entity);
					healthBarGumpCustom.X = Mouse.Position.X - (rectangle.Width >> 1);
					healthBarGumpCustom.Y = Mouse.Position.Y - (rectangle.Height >> 1);
					control = healthBarGumpCustom;
					UIManager.Add(healthBarGumpCustom);
				}
				else
				{
					GumpsLoader.Instance.GetGumpTexture(2052u, out var bounds);
					HealthBarGump healthBarGump = new HealthBarGump(entity);
					healthBarGump.X = Mouse.LClickPosition.X - (bounds.Width >> 1);
					healthBarGump.Y = Mouse.LClickPosition.Y - (bounds.Height >> 1);
					control = healthBarGump;
					UIManager.Add(healthBarGump);
				}
				UIManager.AttemptDragControl(control, attemptAlwaysSuccessful: true);
			}
		}
		else if (entity != null)
		{
			GameActions.PickUp(base.LocalSerial, 0, 0, -1, null);
		}
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			if (SerialHelper.IsMobile(base.LocalSerial))
			{
				if (World.Player.InWarMode)
				{
					GameActions.Attack(base.LocalSerial);
				}
				else
				{
					GameActions.DoubleClick(base.LocalSerial);
				}
			}
			else if (!GameActions.OpenCorpse(base.LocalSerial))
			{
				GameActions.DoubleClick(base.LocalSerial);
			}
			return true;
		}
		return false;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			if (!ItemHold.Enabled && (UIManager.IsDragging || Math.Max(Math.Abs(Mouse.LDragOffset.X), Math.Abs(Mouse.LDragOffset.Y)) >= 1))
			{
				_positionLocked = false;
				return;
			}
			if (TargetManager.IsTargeting)
			{
				switch (TargetManager.TargetingState)
				{
				case CursorTarget.Object:
				case CursorTarget.Position:
				case CursorTarget.Grab:
				case CursorTarget.SetGrabBag:
				case CursorTarget.AddAdditionalJournalTarget:
					TargetManager.Target(base.LocalSerial);
					Mouse.LastLeftButtonClickTime = 0u;
					break;
				case CursorTarget.SetTargetClientSide:
					TargetManager.Target(base.LocalSerial);
					Mouse.LastLeftButtonClickTime = 0u;
					UIManager.Add(new InspectorGump(World.Get(base.LocalSerial)));
					break;
				case CursorTarget.HueCommandTarget:
					CommandManager.OnHueTarget(World.Get(base.LocalSerial));
					break;
				}
			}
			else if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
			{
				uint num = uint.MaxValue;
				bool flag = false;
				ushort num2 = 0;
				ushort num3 = 0;
				sbyte b = 0;
				Entity entity = World.Get(base.LocalSerial);
				if (entity != null)
				{
					flag = entity.Distance <= 3 || World.Player.NotorietyFlag == NotorietyFlag.Staff;
					if (flag)
					{
						if ((entity is Item item && item.ItemData.IsContainer) || entity is Mobile)
						{
							num2 = ushort.MaxValue;
							num3 = ushort.MaxValue;
							b = 0;
							num = entity.Serial;
						}
						else if (entity is Item item2 && (item2.ItemData.IsSurface || (item2.ItemData.IsStackable && item2.DisplayedGraphic == ItemHold.DisplayedGraphic)))
						{
							num2 = entity.X;
							num3 = entity.Y;
							b = entity.Z;
							if (item2.ItemData.IsSurface)
							{
								b += (sbyte)((item2.ItemData.Height != byte.MaxValue) ? item2.ItemData.Height : 0);
							}
							else
							{
								num = entity.Serial;
							}
						}
					}
					else
					{
						Client.Game.Scene.Audio.PlaySound(81);
					}
					if (flag)
					{
						if (num == uint.MaxValue && num2 == 0 && num3 == 0)
						{
							flag = false;
						}
						if (flag)
						{
							GameActions.DropItem(ItemHold.Serial, num2, num3, b, num);
						}
					}
				}
			}
			else if (!DelayedObjectClickManager.IsEnabled)
			{
				DelayedObjectClickManager.Set(base.LocalSerial, Mouse.Position.X, Mouse.Position.Y, Time.Ticks + 350);
			}
		}
		base.OnMouseUp(x, y, button);
	}

	protected override void OnMouseOver(int x, int y)
	{
		if (_positionLocked)
		{
			return;
		}
		if (SerialHelper.IsMobile(base.LocalSerial))
		{
			Mobile mobile = World.Mobiles.Get(base.LocalSerial);
			if (mobile == null)
			{
				Dispose();
				return;
			}
			_positionLocked = true;
			AnimationsLoader.Instance.GetAnimationDimensions(mobile.AnimIndex, mobile.GetGraphicForAnimation(), 0, 0, mobile.IsMounted, 0, out var _, out var centerY, out var _, out var height);
			_lockedPosition.X = (int)((float)mobile.RealScreenPosition.X + mobile.CumulativeOffset.X + 22f + 5f);
			_lockedPosition.Y = (int)((float)mobile.RealScreenPosition.Y + (mobile.CumulativeOffset.Y - mobile.CumulativeOffset.Z) - (float)(height + centerY + 8) + (float)((mobile.IsGargoyle && mobile.IsFlying) ? (-22) : ((!mobile.IsMounted) ? 22 : 0)));
		}
		base.OnMouseOver(x, y);
	}

	protected override void OnMouseExit(int x, int y)
	{
		_positionLocked = false;
		base.OnMouseExit(x, y);
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		Entity entity = World.Get(base.LocalSerial);
		if (entity == null || entity.IsDestroyed || entity.ObjectHandlesStatus == ObjectHandlesStatus.NONE || entity.ObjectHandlesStatus == ObjectHandlesStatus.CLOSED)
		{
			Dispose();
		}
		else if ((uint)entity == TargetManager.LastTargetInfo.Serial)
		{
			_borderColor = SolidColorTextureCache.GetTexture(Color.Red);
			AlphaBlendControl background = _background;
			ushort hue = (_renderedText.Hue = (ushort)((entity is Mobile mobile) ? Notoriety.GetHue(mobile.NotorietyFlag) : 1153));
			background.Hue = hue;
		}
		else
		{
			_borderColor = SolidColorTextureCache.GetTexture(Color.Black);
			AlphaBlendControl background2 = _background;
			ushort hue = (_renderedText.Hue = (ushort)((entity is Mobile mobile2) ? Notoriety.GetHue(mobile2.NotorietyFlag) : 1153));
			background2.Hue = hue;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed || !SetName())
		{
			return false;
		}
		int x2 = ProfileManager.CurrentProfile.GameWindowPosition.X;
		int y2 = ProfileManager.CurrentProfile.GameWindowPosition.Y;
		int x3 = ProfileManager.CurrentProfile.GameWindowSize.X;
		int y3 = ProfileManager.CurrentProfile.GameWindowSize.Y;
		if (SerialHelper.IsMobile(base.LocalSerial))
		{
			Mobile mobile = World.Mobiles.Get(base.LocalSerial);
			if (mobile == null)
			{
				Dispose();
				return false;
			}
			if (_positionLocked)
			{
				x = _lockedPosition.X;
				y = _lockedPosition.Y;
			}
			else
			{
				AnimationsLoader.Instance.GetAnimationDimensions(mobile.AnimIndex, mobile.GetGraphicForAnimation(), 0, 0, mobile.IsMounted, 0, out var _, out var centerY, out var _, out var height);
				x = (int)((float)mobile.RealScreenPosition.X + mobile.CumulativeOffset.X + 22f + 5f);
				y = (int)((float)mobile.RealScreenPosition.Y + (mobile.CumulativeOffset.Y - mobile.CumulativeOffset.Z) - (float)(height + centerY + 8) + (float)((mobile.IsGargoyle && mobile.IsFlying) ? (-22) : ((!mobile.IsMounted) ? 22 : 0)));
			}
		}
		else if (SerialHelper.IsItem(base.LocalSerial))
		{
			Item item = World.Items.Get(base.LocalSerial);
			if (item == null)
			{
				Dispose();
				return false;
			}
			Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(item.Graphic);
			x = item.RealScreenPosition.X + (int)item.CumulativeOffset.X + 22 + 5;
			y = item.RealScreenPosition.Y + (int)(item.CumulativeOffset.Y - item.CumulativeOffset.Z) + (realArtBounds.Height >> 1);
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		Point point = Client.Game.Scene.Camera.WorldToScreen(new Point(x, y));
		x = point.X - (base.Width >> 1);
		y = point.Y - (base.Height >> 1);
		x += x2;
		y += y2;
		if (x < x2 || x + base.Width > x2 + x3)
		{
			return false;
		}
		if (y < y2 || y + base.Height > y2 + y3)
		{
			return false;
		}
		base.X = x;
		base.Y = y;
		batcher.DrawRectangle(_borderColor, x - 1, y - 1, base.Width + 1, base.Height + 1, hueVector);
		base.Draw(batcher, x, y);
		int num = Math.Max(0, base.Width - _renderedText.Width - 4) >> 1;
		return _renderedText.Draw(batcher, base.Width, base.Height, x + 2 + num, y + 2, base.Width, base.Height, 0, 0, 0);
	}

	public override void Dispose()
	{
		_renderedText?.Destroy();
		base.Dispose();
	}
}
