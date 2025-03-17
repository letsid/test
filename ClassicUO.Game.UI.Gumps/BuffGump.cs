using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class BuffGump : Gump
{
	private enum GumpDirection
	{
		LEFT_VERTICAL,
		LEFT_HORIZONTAL,
		RIGHT_VERTICAL,
		RIGHT_HORIZONTAL
	}

	private class BuffControlEntry : GumpPic
	{
		private byte _alpha;

		private bool _decreaseAlpha;

		private readonly RenderedText _gText;

		private float _updateTooltipTime;

		public BuffIcon Icon { get; }

		public BuffControlEntry(BuffIcon icon)
			: base(0, 0, icon.Graphic, 0)
		{
			if (!base.IsDisposed)
			{
				base.Hue = icon.Hue;
				Icon = icon;
				_alpha = byte.MaxValue;
				_decreaseAlpha = true;
				_gText = RenderedText.Create("", ushort.MaxValue, 2, isunicode: true, FontStyle.BlackBorder | FontStyle.Fixed, TEXT_ALIGN_TYPE.TS_CENTER, base.Width, 30);
				AcceptMouseInput = true;
				base.WantUpdateSize = false;
				CanMove = true;
				SetTooltip(icon.Text);
			}
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (base.IsDisposed || Icon == null)
			{
				return;
			}
			int num = (int)((double)Icon.Timer - totalTime);
			if ((double)_updateTooltipTime < totalTime && num > 0)
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(num);
				SetTooltip(string.Format(ResGumps.TimeLeft, Icon.Text, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));
				_updateTooltipTime = (float)totalTime + 1000f;
				if (timeSpan.Hours > 0)
				{
					_gText.Text = string.Format(ResGumps.Span0Hours, timeSpan.Hours);
				}
				else
				{
					_gText.Text = ((timeSpan.Minutes > 0) ? $"{timeSpan.Minutes}:{timeSpan.Seconds:00}" : $"{timeSpan.Seconds:00}s");
				}
			}
			if (Icon.Timer == uint.MaxValue || num >= 5000)
			{
				return;
			}
			if (num <= 0)
			{
				((BuffGump)base.Parent.Parent)?.RequestUpdateContents();
			}
			else
			{
				if (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.BuffEndWithAlphaBlinks)
				{
					return;
				}
				int alpha = _alpha;
				int num2 = (5000 - num) / 600;
				if (_decreaseAlpha)
				{
					alpha -= num2;
					if (alpha <= 60)
					{
						_decreaseAlpha = false;
						alpha = 60;
					}
				}
				else
				{
					alpha += num2;
					if (alpha >= 255)
					{
						_decreaseAlpha = true;
						alpha = 255;
					}
				}
				_alpha = (byte)alpha;
			}
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(base.Hue, partial: false, (float)(int)_alpha / 255f, gump: true);
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(base.Graphic, out bounds);
			if (gumpTexture != null)
			{
				batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hueVector);
				if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.BuffBarTime)
				{
					_gText.Draw(batcher, x, y + bounds.Height - 18, hueVector.Z, 0);
				}
			}
			return true;
		}

		public override void Dispose()
		{
			_gText?.Destroy();
			base.Dispose();
		}
	}

	private GumpPic _background;

	private Button _button;

	private GumpDirection _direction;

	private ushort _graphic;

	private DataBox _box;

	public override GumpType GumpType => GumpType.Buff;

	public BuffGump()
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		AcceptMouseInput = true;
	}

	public BuffGump(int x, int y)
		: this()
	{
		base.X = x;
		base.Y = y;
		_direction = GumpDirection.LEFT_HORIZONTAL;
		_graphic = 30080;
		SetInScreen();
		BuildGump();
	}

	private void BuildGump()
	{
		base.WantUpdateSize = true;
		_box?.Clear();
		_box?.Children.Clear();
		Clear();
		Add(_background = new GumpPic(0, 0, _graphic, 0)
		{
			LocalSerial = 1u
		});
		Add(_button = new Button(0, 30085, 30089, 30089, "", 0)
		{
			ButtonAction = ButtonAction.Activate
		});
		switch (_direction)
		{
		case GumpDirection.LEFT_HORIZONTAL:
			_button.X = -2;
			_button.Y = 36;
			break;
		case GumpDirection.RIGHT_VERTICAL:
			_button.X = 34;
			_button.Y = 78;
			break;
		case GumpDirection.RIGHT_HORIZONTAL:
			_button.X = 76;
			_button.Y = 36;
			break;
		default:
			_button.X = 0;
			_button.Y = 0;
			break;
		}
		Add(_box = new DataBox(0, -10, 0, 0)
		{
			WantUpdateSize = true
		});
		if (World.Player != null)
		{
			foreach (KeyValuePair<ushort, BuffIcon> item in World.Player.BuffIcons.OrderBy((KeyValuePair<ushort, BuffIcon> bi) => bi.Value.Timer))
			{
				_box.Add(new BuffControlEntry(World.Player.BuffIcons[item.Key]));
			}
		}
		_background.Graphic = _graphic;
		_background.X = 0;
		_background.Y = 0;
		UpdateElements();
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("graphic", _graphic.ToString());
		int direction = (int)_direction;
		writer.WriteAttributeString("direction", direction.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		_graphic = ushort.Parse(xml.GetAttribute("graphic"));
		_direction = (GumpDirection)byte.Parse(xml.GetAttribute("direction"));
		BuildGump();
	}

	protected override void UpdateContents()
	{
		BuildGump();
	}

	private void UpdateElements()
	{
		int num = 0;
		int num2 = 0;
		while (num < _box.Children.Count)
		{
			Control control = _box.Children[num];
			switch (_direction)
			{
			case GumpDirection.LEFT_VERTICAL:
				control.X = 25;
				control.Y = 30 + num2;
				break;
			case GumpDirection.LEFT_HORIZONTAL:
				control.X = 22 + num2;
				control.Y = 5;
				break;
			case GumpDirection.RIGHT_VERTICAL:
				control.X = -10;
				control.Y = _background.Height - 53 - num2;
				break;
			case GumpDirection.RIGHT_HORIZONTAL:
				control.X = _background.Width - 65 - num2;
				control.Y = 5;
				break;
			}
			num++;
			num2 += 45;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 0)
		{
			_graphic++;
			if (_graphic > 30082)
			{
				_graphic = 30079;
			}
			switch (_graphic)
			{
			case 30080:
				_direction = GumpDirection.LEFT_HORIZONTAL;
				break;
			case 30081:
				_direction = GumpDirection.RIGHT_VERTICAL;
				break;
			case 30082:
				_direction = GumpDirection.RIGHT_HORIZONTAL;
				break;
			default:
				_direction = GumpDirection.LEFT_VERTICAL;
				break;
			}
			RequestUpdateContents();
		}
	}
}
