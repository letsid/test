using System;
using System.Collections.Generic;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class HSliderBar : Control
{
	private bool _clicked;

	private readonly bool _drawUp;

	private readonly List<HSliderBar> _pairedSliders = new List<HSliderBar>();

	private int _sliderX;

	private readonly HSliderBarStyle _style;

	private readonly RenderedText _text;

	private int _value = -1;

	public int MinValue { get; set; }

	public int MaxValue { get; set; }

	public int BarWidth { get; set; }

	public float Percents { get; private set; }

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				int value2 = _value;
				_value = value;
				if (_value < MinValue)
				{
					_value = MinValue;
				}
				else if (_value > MaxValue)
				{
					_value = MaxValue;
				}
				if (_text != null)
				{
					_text.Text = Value.ToString();
				}
				if (_value != value2)
				{
					ModifyPairedValues(_value - value2);
					CalculateOffset();
				}
				this.ValueChanged.Raise();
			}
		}
	}

	public event EventHandler ValueChanged;

	public HSliderBar(int x, int y, int w, int min, int max, int value, HSliderBarStyle style, bool hasText = false, byte font = 0, ushort color = 0, bool unicode = true, bool drawUp = false)
	{
		base.X = x;
		base.Y = y;
		if (hasText)
		{
			_text = RenderedText.Create(string.Empty, color, font, unicode, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
			_drawUp = drawUp;
		}
		MinValue = min;
		MaxValue = max;
		BarWidth = w;
		_style = style;
		AcceptMouseInput = true;
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture((_style == HSliderBarStyle.MetalWidgetRecessedBar) ? 216u : 2117u, out bounds);
		base.Width = BarWidth;
		if (gumpTexture != null)
		{
			base.Height = bounds.Height;
		}
		CalculateOffset();
		Value = value;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		if (_style == HSliderBarStyle.MetalWidgetRecessedBar)
		{
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(213u, out bounds);
			Rectangle bounds2;
			Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture(214u, out bounds2);
			Rectangle bounds3;
			Texture2D gumpTexture3 = GumpsLoader.Instance.GetGumpTexture(215u, out bounds3);
			Rectangle bounds4;
			Texture2D gumpTexture4 = GumpsLoader.Instance.GetGumpTexture(216u, out bounds4);
			batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hueVector);
			batcher.DrawTiled(gumpTexture2, new Rectangle(x + bounds.Width, y, BarWidth - bounds3.Width - bounds.Width, bounds2.Height), bounds2, hueVector);
			batcher.Draw(gumpTexture3, new Vector2(x + BarWidth - bounds3.Width, y), bounds3, hueVector);
			batcher.Draw(gumpTexture4, new Vector2(x + _sliderX, y), bounds4, hueVector);
		}
		else
		{
			Rectangle bounds5;
			Texture2D gumpTexture5 = GumpsLoader.Instance.GetGumpTexture(2117u, out bounds5);
			batcher.Draw(gumpTexture5, new Vector2(x + _sliderX, y), bounds5, hueVector);
		}
		if (_text != null)
		{
			if (_drawUp)
			{
				_text.Draw(batcher, x, y - _text.Height, 1f, 0);
			}
			else
			{
				_text.Draw(batcher, x + BarWidth + 2, y + (base.Height >> 1) - (_text.Height >> 1), 1f, 0);
			}
		}
		return base.Draw(batcher, x, y);
	}

	private void InternalSetValue(int value)
	{
		_value = value;
		CalculateOffset();
		if (_text != null)
		{
			_text.Text = Value.ToString();
		}
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			_clicked = true;
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			_clicked = false;
			CalculateNew(x);
		}
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		switch (delta)
		{
		case MouseEventType.WheelScrollUp:
			Value++;
			break;
		case MouseEventType.WheelScrollDown:
			Value--;
			break;
		}
		CalculateOffset();
	}

	protected override void OnMouseOver(int x, int y)
	{
		if (_clicked)
		{
			CalculateNew(x);
		}
	}

	private void CalculateNew(int x)
	{
		int barWidth = BarWidth;
		int num = MaxValue - MinValue;
		GumpsLoader.Instance.GetGumpTexture((_style == HSliderBarStyle.MetalWidgetRecessedBar) ? 216u : 2117u, out var bounds);
		barWidth -= bounds.Width;
		float num2 = (float)x / (float)barWidth * 100f;
		Value = (int)((float)num * num2 / 100f) + MinValue;
		CalculateOffset();
	}

	private void CalculateOffset()
	{
		if (Value < MinValue)
		{
			Value = MinValue;
		}
		else if (Value > MaxValue)
		{
			Value = MaxValue;
		}
		int num = Value - MinValue;
		int num2 = MaxValue - MinValue;
		int barWidth = BarWidth;
		GumpsLoader.Instance.GetGumpTexture((_style == HSliderBarStyle.MetalWidgetRecessedBar) ? 216u : 2117u, out var bounds);
		barWidth -= bounds.Width;
		if (num2 > 0)
		{
			Percents = (float)num / (float)num2 * 100f;
		}
		else
		{
			Percents = 0f;
		}
		_sliderX = (int)((float)barWidth * Percents / 100f);
		if (_sliderX < 0)
		{
			_sliderX = 0;
		}
	}

	public void AddParisSlider(HSliderBar s)
	{
		_pairedSliders.Add(s);
	}

	private void ModifyPairedValues(int delta)
	{
		if (_pairedSliders.Count == 0)
		{
			return;
		}
		bool flag = true;
		int num = ((delta <= 0) ? 1 : (-1));
		int num2 = Math.Abs(delta);
		int num3 = Value % _pairedSliders.Count;
		while (num2 > 0)
		{
			if (num > 0)
			{
				if (_pairedSliders[num3].Value < _pairedSliders[num3].MaxValue)
				{
					flag = true;
					_pairedSliders[num3].InternalSetValue(_pairedSliders[num3].Value + num);
					num2--;
				}
			}
			else if (_pairedSliders[num3].Value > _pairedSliders[num3].MinValue)
			{
				flag = true;
				_pairedSliders[num3].InternalSetValue(_pairedSliders[num3]._value + num);
				num2--;
			}
			num3++;
			if (num3 == _pairedSliders.Count)
			{
				if (!flag)
				{
					break;
				}
				flag = false;
				num3 = 0;
			}
		}
	}

	public override void Dispose()
	{
		_text?.Destroy();
		base.Dispose();
	}
}
