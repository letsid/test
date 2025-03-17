using System;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ScrollBar : ScrollBarBase
{
	private Rectangle _rectSlider;

	private Rectangle _emptySpace;

	private const ushort BUTTON_UP_0 = 251;

	private const ushort BUTTON_UP_1 = 250;

	private const ushort BUTTON_DOWN_0 = 253;

	private const ushort BUTTON_DOWN_1 = 252;

	private const ushort BACKGROUND_0 = 257;

	private const ushort BACKGROUND_1 = 256;

	private const ushort BACKGROUND_2 = 255;

	private const ushort SLIDER = 254;

	public ScrollBar(int x, int y, int height)
	{
		base.Height = height;
		base.Location = new Point(x, y);
		AcceptMouseInput = true;
		GumpsLoader.Instance.GetGumpTexture(251u, out var bounds);
		GumpsLoader.Instance.GetGumpTexture(253u, out var bounds2);
		GumpsLoader.Instance.GetGumpTexture(257u, out var bounds3);
		GumpsLoader.Instance.GetGumpTexture(254u, out var bounds4);
		base.Width = bounds3.Width;
		_rectDownButton = new Rectangle(0, base.Height - bounds2.Height, bounds2.Width, bounds2.Height);
		_rectUpButton = new Rectangle(0, 0, bounds.Width, bounds.Height);
		_rectSlider = new Rectangle(bounds3.Width - bounds4.Width >> 1, bounds.Height + _sliderPosition, bounds4.Width, bounds4.Height);
		_emptySpace.X = 0;
		_emptySpace.Y = bounds.Height;
		_emptySpace.Width = bounds4.Width;
		_emptySpace.Height = base.Height - (bounds2.Height + bounds.Height);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.Height <= 0 || !base.IsVisible)
		{
			return false;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(251u, out bounds);
		Rectangle bounds2;
		Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture(250u, out bounds2);
		Rectangle bounds3;
		Texture2D gumpTexture3 = GumpsLoader.Instance.GetGumpTexture(253u, out bounds3);
		Rectangle bounds4;
		Texture2D gumpTexture4 = GumpsLoader.Instance.GetGumpTexture(252u, out bounds4);
		Rectangle bounds5;
		Texture2D gumpTexture5 = GumpsLoader.Instance.GetGumpTexture(257u, out bounds5);
		Rectangle bounds6;
		Texture2D gumpTexture6 = GumpsLoader.Instance.GetGumpTexture(256u, out bounds6);
		Rectangle bounds7;
		Texture2D gumpTexture7 = GumpsLoader.Instance.GetGumpTexture(255u, out bounds7);
		Rectangle bounds8;
		Texture2D gumpTexture8 = GumpsLoader.Instance.GetGumpTexture(254u, out bounds8);
		int num = base.Height - bounds.Height - bounds3.Height - bounds5.Height - bounds7.Height;
		if (num > 0)
		{
			batcher.Draw(gumpTexture5, new Vector2(x, y + bounds.Height), bounds5, hueVector);
			batcher.DrawTiled(gumpTexture6, new Rectangle(x, y + bounds2.Height + bounds5.Height, bounds5.Width, num), bounds6, hueVector);
			batcher.Draw(gumpTexture7, new Vector2(x, y + base.Height - bounds3.Height - bounds7.Height), bounds7, hueVector);
		}
		else
		{
			num = base.Height - bounds.Height - bounds3.Height;
			batcher.DrawTiled(gumpTexture6, new Rectangle(x, y + bounds.Height, bounds5.Width, num), bounds6, hueVector);
		}
		if (_btUpClicked)
		{
			batcher.Draw(gumpTexture2, new Vector2(x, y), bounds2, hueVector);
		}
		else
		{
			batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hueVector);
		}
		if (_btDownClicked)
		{
			batcher.Draw(gumpTexture4, new Vector2(x, y + base.Height - bounds3.Height), bounds4, hueVector);
		}
		else
		{
			batcher.Draw(gumpTexture3, new Vector2(x, y + base.Height - bounds3.Height), bounds3, hueVector);
		}
		if (base.MaxValue > base.MinValue && num > 0)
		{
			batcher.Draw(gumpTexture8, new Vector2(x + (bounds5.Width - bounds8.Width >> 1), y + bounds.Height + _sliderPosition), bounds8, hueVector);
		}
		return base.Draw(batcher, x, y);
	}

	protected override int GetScrollableArea()
	{
		GumpsLoader.Instance.GetGumpTexture(251u, out var bounds);
		GumpsLoader.Instance.GetGumpTexture(253u, out var bounds2);
		GumpsLoader.Instance.GetGumpTexture(254u, out var bounds3);
		return base.Height - bounds.Height - bounds2.Height - bounds3.Height;
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		base.OnMouseDown(x, y, button);
		if (_btnSliderClicked && _emptySpace.Contains(x, y))
		{
			CalculateByPosition(x, y);
		}
	}

	protected override void CalculateByPosition(int x, int y)
	{
		if (y != _clickPosition.Y)
		{
			y -= _emptySpace.Y + (_rectSlider.Height >> 1);
			if (y < 0)
			{
				y = 0;
			}
			int scrollableArea = GetScrollableArea();
			if (y > scrollableArea)
			{
				y = scrollableArea;
			}
			_sliderPosition = y;
			_clickPosition.X = x;
			_clickPosition.Y = y;
			GumpsLoader.Instance.GetGumpTexture(251u, out var bounds);
			GumpsLoader.Instance.GetGumpTexture(253u, out var bounds2);
			GumpsLoader.Instance.GetGumpTexture(254u, out var bounds3);
			if (y == 0 && _clickPosition.Y < bounds.Height + (bounds3.Height >> 1))
			{
				_clickPosition.Y = bounds.Height + (bounds3.Height >> 1);
			}
			else if (y == scrollableArea && _clickPosition.Y > base.Height - bounds2.Height - (bounds3.Height >> 1))
			{
				_clickPosition.Y = base.Height - bounds2.Height - (bounds3.Height >> 1);
			}
			_value = (int)Math.Round((float)y / (float)scrollableArea * (float)(base.MaxValue - base.MinValue) + (float)base.MinValue);
		}
	}

	public override bool Contains(int x, int y)
	{
		if (x >= 0 && x <= base.Width && y >= 0)
		{
			return y <= base.Height;
		}
		return false;
	}
}
