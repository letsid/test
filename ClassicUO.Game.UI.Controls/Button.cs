using System.Collections.Generic;
using ClassicUO.Game.Scenes;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class Button : Control
{
	private readonly string _caption;

	private bool _entered;

	private readonly RenderedText[] _fontTexture;

	private ushort _normal;

	private ushort _pressed;

	private ushort _over;

	public bool IsClicked { get; set; }

	public int ButtonID { get; }

	public ButtonAction ButtonAction { get; set; }

	public int ToPage { get; set; }

	public override ClickPriority Priority => ClickPriority.High;

	public ushort ButtonGraphicNormal
	{
		get
		{
			return _normal;
		}
		set
		{
			_normal = value;
			GumpsLoader.Instance.GetGumpTexture(value, out var bounds);
			base.Width = bounds.Width;
			base.Height = bounds.Height;
		}
	}

	public ushort ButtonGraphicPressed
	{
		get
		{
			return _pressed;
		}
		set
		{
			_pressed = value;
			GumpsLoader.Instance.GetGumpTexture(value, out var bounds);
			base.Width = bounds.Width;
			base.Height = bounds.Height;
		}
	}

	public ushort ButtonGraphicOver
	{
		get
		{
			return _over;
		}
		set
		{
			_over = value;
			GumpsLoader.Instance.GetGumpTexture(value, out var bounds);
			base.Width = bounds.Width;
			base.Height = bounds.Height;
		}
	}

	public int Hue { get; set; }

	public ushort FontHue { get; }

	public ushort HueHover { get; }

	public bool FontCenter { get; set; }

	public bool ContainsByBounds { get; set; }

	public Button(int buttonID, ushort normal, ushort pressed, ushort over = 0, string caption = "", byte font = 0, bool isunicode = true, ushort normalHue = ushort.MaxValue, ushort hoverHue = ushort.MaxValue)
	{
		ButtonID = buttonID;
		_normal = normal;
		_pressed = pressed;
		_over = over;
		if (GumpsLoader.Instance.GetGumpTexture(normal, out var bounds) == null)
		{
			Dispose();
			return;
		}
		base.Width = bounds.Width;
		base.Height = bounds.Height;
		FontHue = (ushort)((normalHue != ushort.MaxValue) ? normalHue : 0);
		HueHover = ((hoverHue == ushort.MaxValue) ? normalHue : hoverHue);
		if (!string.IsNullOrEmpty(caption) && normalHue != ushort.MaxValue)
		{
			_fontTexture = new RenderedText[2];
			_caption = caption;
			_fontTexture[0] = RenderedText.Create(caption, FontHue, font, isunicode, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
			if (hoverHue != ushort.MaxValue)
			{
				_fontTexture[1] = RenderedText.Create(caption, HueHover, font, isunicode, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
			}
		}
		CanMove = false;
		AcceptMouseInput = true;
		base.CanCloseWithEsc = false;
	}

	public Button(List<string> parts)
		: this((parts.Count >= 8) ? int.Parse(parts[7]) : 0, UInt16Converter.Parse(parts[3]), UInt16Converter.Parse(parts[4]), 0, "", 0)
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		if (parts.Count >= 6)
		{
			int num = int.Parse(parts[5]);
			ButtonAction = ((num != 0) ? ButtonAction.Activate : ButtonAction.Default);
		}
		ToPage = ((parts.Count >= 7) ? int.Parse(parts[6]) : 0);
		base.WantUpdateSize = false;
		ContainsByBounds = true;
		base.IsFromServer = true;
	}

	protected override void OnMouseEnter(int x, int y)
	{
		_entered = true;
	}

	protected override void OnMouseExit(int x, int y)
	{
		_entered = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Texture2D texture2D = null;
		Rectangle bounds = Rectangle.Empty;
		if (_entered || IsClicked)
		{
			if (IsClicked && _pressed > 0)
			{
				texture2D = GumpsLoader.Instance.GetGumpTexture(_pressed, out bounds);
			}
			if (texture2D == null && _over > 0)
			{
				texture2D = GumpsLoader.Instance.GetGumpTexture(_over, out bounds);
			}
		}
		if (texture2D == null)
		{
			texture2D = GumpsLoader.Instance.GetGumpTexture(_normal, out bounds);
		}
		if (texture2D == null)
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, partial: false, base.Alpha, gump: true);
		batcher.Draw(texture2D, new Rectangle(x, y, base.Width, base.Height), bounds, hueVector);
		if (!string.IsNullOrEmpty(_caption))
		{
			RenderedText renderedText = _fontTexture[_entered ? 1u : 0u];
			if (FontCenter)
			{
				int num = (IsClicked ? 1 : 0);
				renderedText.Draw(batcher, x + (base.Width - renderedText.Width >> 1), y + num + (base.Height - renderedText.Height >> 1), 1f, 0);
			}
			else
			{
				renderedText.Draw(batcher, x, y, 1f, 0);
			}
		}
		return base.Draw(batcher, x, y);
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			IsClicked = true;
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return;
		}
		IsClicked = false;
		if (base.MouseIsOver && (_entered || Client.Game.Scene is GameScene))
		{
			switch (ButtonAction)
			{
			case ButtonAction.Default:
				ChangePage(ToPage);
				break;
			case ButtonAction.Activate:
				OnButtonClick(ButtonID);
				break;
			}
			Mouse.LastLeftButtonClickTime = 0u;
			Mouse.CancelDoubleClick = true;
		}
	}

	public override bool Contains(int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		if (!ContainsByBounds)
		{
			return GumpsLoader.Instance.PixelCheck(_normal, x - base.Offset.X, y - base.Offset.Y);
		}
		return base.Contains(x, y);
	}

	public sealed override void Dispose()
	{
		if (_fontTexture != null)
		{
			RenderedText[] fontTexture = _fontTexture;
			for (int i = 0; i < fontTexture.Length; i++)
			{
				fontTexture[i]?.Destroy();
			}
		}
		base.Dispose();
	}
}
