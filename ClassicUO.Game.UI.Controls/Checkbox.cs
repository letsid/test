using System;
using System.Collections.Generic;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class Checkbox : Control
{
	private bool _isChecked;

	private readonly RenderedText _text;

	private ushort _inactive;

	private ushort _active;

	public bool IsChecked
	{
		get
		{
			return _isChecked;
		}
		set
		{
			if (_isChecked != value)
			{
				_isChecked = value;
				OnCheckedChanged();
			}
		}
	}

	public override ClickPriority Priority => ClickPriority.High;

	public string Text => _text.Text;

	public event EventHandler ValueChanged;

	public Checkbox(ushort inactive, ushort active, string text = "", byte font = 0, ushort color = 0, bool isunicode = true, int maxWidth = 0)
	{
		_inactive = inactive;
		_active = active;
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(inactive, out bounds);
		Rectangle bounds2;
		Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture(active, out bounds2);
		if (gumpTexture == null || gumpTexture2 == null)
		{
			Dispose();
			return;
		}
		base.Width = bounds.Width;
		_text = RenderedText.Create(text, color, font, isunicode, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, maxWidth, 30);
		base.Width += _text.Width;
		base.Height = Math.Max(bounds.Width, _text.Height);
		CanMove = false;
		AcceptMouseInput = true;
	}

	public Checkbox(List<string> parts, string[] lines)
		: this(ushort.Parse(parts[3]), ushort.Parse(parts[4]), "", 0, 0)
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		IsChecked = parts[5] == "1";
		base.LocalSerial = SerialHelper.Parse(parts[6]);
		base.IsFromServer = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		bool result = base.Draw(batcher, x, y);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(IsChecked ? _active : _inactive, out bounds);
		batcher.Draw(gumpTexture, new Vector2(x, y), bounds, ShaderHueTranslator.GetHueVector(0));
		_text.Draw(batcher, x + bounds.Width + 2, y, 1f, 0);
		return result;
	}

	protected virtual void OnCheckedChanged()
	{
		this.ValueChanged.Raise(this);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && base.MouseIsOver)
		{
			IsChecked = !IsChecked;
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		_text?.Destroy();
	}
}
