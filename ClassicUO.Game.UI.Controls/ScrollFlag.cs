using System;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ScrollFlag : ScrollBarBase
{
	private readonly bool _showButtons;

	private const ushort BUTTON_UP = 2084;

	private const ushort BUTTON_DOWN = 2085;

	private const ushort BUTTON_FLAG = 2088;

	public override ClickPriority Priority { get; set; }

	public ScrollFlag(int x, int y, int height, bool showbuttons)
		: this()
	{
		base.X = x;
		base.Y = y;
		base.Height = height;
		_showButtons = false;
	}

	public ScrollFlag()
	{
		AcceptMouseInput = true;
		if (GumpsLoader.Instance.GetGumpTexture(2088u, out var bounds) == null)
		{
			Dispose();
			return;
		}
		GumpsLoader.Instance.GetGumpTexture(2084u, out var bounds2);
		GumpsLoader.Instance.GetGumpTexture(2085u, out var bounds3);
		base.Width = bounds.Width;
		base.Height = bounds.Height;
		_rectUpButton = new Rectangle(0, 0, bounds2.Width, bounds2.Height);
		_rectDownButton = new Rectangle(0, base.Height, bounds3.Width, bounds3.Height);
		base.WantUpdateSize = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(2088u, out bounds);
		Rectangle bounds2;
		Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture(2084u, out bounds2);
		Rectangle bounds3;
		Texture2D gumpTexture3 = GumpsLoader.Instance.GetGumpTexture(2085u, out bounds3);
		if (base.MaxValue != base.MinValue && gumpTexture != null)
		{
			batcher.Draw(gumpTexture, new Vector2(x, y + _sliderPosition), bounds, hueVector);
		}
		if (_showButtons)
		{
			if (gumpTexture2 != null)
			{
				batcher.Draw(gumpTexture2, new Vector2(x, y), bounds2, hueVector);
			}
			if (gumpTexture3 != null)
			{
				batcher.Draw(gumpTexture3, new Vector2(x, y + base.Height), bounds3, hueVector);
			}
		}
		return base.Draw(batcher, x, y);
	}

	protected override int GetScrollableArea()
	{
		GumpsLoader.Instance.GetGumpTexture(2088u, out var bounds);
		return base.Height - bounds.Height;
	}

	protected override void CalculateByPosition(int x, int y)
	{
		if (y != _clickPosition.Y)
		{
			GumpsLoader.Instance.GetGumpTexture(2088u, out var bounds);
			int height = bounds.Height;
			y -= height >> 1;
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
			if (y == 0 && _clickPosition.Y < height >> 1)
			{
				_clickPosition.Y = height >> 1;
			}
			else if (y == scrollableArea && _clickPosition.Y > base.Height - (height >> 1))
			{
				_clickPosition.Y = base.Height - (height >> 1);
			}
			_value = (int)Math.Round((float)y / (float)scrollableArea * (float)(base.MaxValue - base.MinValue) + (float)base.MinValue);
		}
	}

	public override bool Contains(int x, int y)
	{
		if (GumpsLoader.Instance.GetGumpTexture(2088u, out var _) == null)
		{
			return false;
		}
		y -= _sliderPosition;
		return GumpsLoader.Instance.PixelCheck(2088, x, y);
	}
}
