using System;
using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ResizePic : Control
{
	private int _maxIndex;

	public ushort Graphic { get; }

	public ushort Hue { get; set; }

	public ResizePic(ushort graphic)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		Graphic = graphic;
		_maxIndex = 0;
		Rectangle bounds;
		while (_maxIndex < 9 && GumpsLoader.Instance.GetGumpTexture((ushort)(Graphic + _maxIndex), out bounds) != null)
		{
			_maxIndex++;
		}
	}

	public ResizePic(List<string> parts)
		: this(UInt16Converter.Parse(parts[3]))
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = int.Parse(parts[4]);
		base.Height = int.Parse(parts[5]);
		base.IsFromServer = true;
	}

	public override bool Contains(int x, int y)
	{
		x -= base.Offset.X;
		y -= base.Offset.Y;
		GetTexture(0, out var bounds);
		GetTexture(1, out var bounds2);
		GetTexture(2, out var bounds3);
		GetTexture(3, out var bounds4);
		GetTexture(4, out var bounds5);
		GetTexture(5, out var bounds6);
		GetTexture(6, out var bounds7);
		GetTexture(7, out var bounds8);
		GetTexture(8, out var bounds9);
		int num = Math.Max(bounds.Height, bounds3.Height) - bounds2.Height;
		int num2 = Math.Max(bounds6.Height, bounds8.Height) - bounds7.Height;
		int num3 = Math.Abs(Math.Max(bounds.Width, bounds6.Width) - bounds3.Width);
		int num4 = Math.Max(bounds3.Width, bounds8.Width) - bounds5.Width;
		if (PixelsInXY(ref bounds, Graphic, x, y))
		{
			return true;
		}
		int num5 = base.Width - bounds.Width - bounds3.Width;
		if (num5 >= 1 && PixelsInXY(ref bounds2, (ushort)(Graphic + 1), x - bounds.Width, y, num5))
		{
			return true;
		}
		if (PixelsInXY(ref bounds3, (ushort)(Graphic + 2), x - (base.Width - bounds3.Width), y - num))
		{
			return true;
		}
		int num6 = base.Height - bounds.Height - bounds6.Height;
		if (num6 >= 1 && PixelsInXY(ref bounds4, (ushort)(Graphic + 3), x, y - bounds.Height, 0, num6))
		{
			return true;
		}
		num6 = base.Height - bounds3.Height - bounds8.Height;
		if (num6 >= 1 && PixelsInXY(ref bounds5, (ushort)(Graphic + 5), x - (base.Width - bounds5.Width), y - bounds3.Height, 0, num6))
		{
			return true;
		}
		if (PixelsInXY(ref bounds6, (ushort)(Graphic + 6), x, y - (base.Height - bounds6.Height)))
		{
			return true;
		}
		num5 = base.Width - bounds6.Width - bounds3.Width;
		if (num6 >= 1 && PixelsInXY(ref bounds7, (ushort)(Graphic + 7), x - bounds6.Width, y - (base.Height - bounds7.Height - num2), num5))
		{
			return true;
		}
		if (PixelsInXY(ref bounds8, (ushort)(Graphic + 8), x - (base.Width - bounds8.Width), y - (base.Height - bounds8.Height)))
		{
			return true;
		}
		num5 = base.Width - bounds.Width - bounds3.Width;
		num5 += num3 + num4;
		num6 = base.Height - bounds3.Height - bounds8.Height;
		if (num5 >= 1 && num6 >= 1 && PixelsInXY(ref bounds9, (ushort)(Graphic + 4), x - bounds.Width, y - bounds.Height, num5, num6))
		{
			return true;
		}
		return false;
	}

	private static bool PixelsInXY(ref Rectangle bounds, ushort graphic, int x, int y, int width = 0, int height = 0)
	{
		if (x < 0 || y < 0 || (width > 0 && x >= width) || (height > 0 && y >= height))
		{
			return false;
		}
		if (bounds.Width == 0 || bounds.Height == 0)
		{
			return false;
		}
		int width2 = bounds.Width;
		int height2 = bounds.Height;
		if (width == 0)
		{
			width = width2;
		}
		if (height == 0)
		{
			height = height2;
		}
		while (x >= width2 && width >= width2)
		{
			x -= width2;
			width -= width2;
		}
		if (x < 0 || x > width)
		{
			return false;
		}
		while (y >= height2 && height >= height2)
		{
			y -= height2;
			height -= height2;
		}
		if (y < 0 || y > height)
		{
			return false;
		}
		return GumpsLoader.Instance.PixelCheck(graphic, x, y);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (batcher.ClipBegin(x, y, base.Width, base.Height))
		{
			Vector3 color = ((Hue == 0) ? ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha, gump: true) : ShaderHueTranslator.GetHueVector(Hue, partial: false, base.Alpha, gump: true));
			DrawInternal(batcher, x, y, color);
			base.Draw(batcher, x, y);
			batcher.ClipEnd();
		}
		return true;
	}

	private void DrawInternal(UltimaBatcher2D batcher, int x, int y, Vector3 color)
	{
		Rectangle bounds;
		Texture2D texture = GetTexture(0, out bounds);
		Rectangle bounds2;
		Texture2D texture2 = GetTexture(1, out bounds2);
		Rectangle bounds3;
		Texture2D texture3 = GetTexture(2, out bounds3);
		Rectangle bounds4;
		Texture2D texture4 = GetTexture(3, out bounds4);
		Rectangle bounds5;
		Texture2D texture5 = GetTexture(4, out bounds5);
		Rectangle bounds6;
		Texture2D texture6 = GetTexture(5, out bounds6);
		Rectangle bounds7;
		Texture2D texture7 = GetTexture(6, out bounds7);
		Rectangle bounds8;
		Texture2D texture8 = GetTexture(7, out bounds8);
		Rectangle bounds9;
		Texture2D texture9 = GetTexture(8, out bounds9);
		int num = Math.Max(bounds.Height, bounds3.Height) - bounds2.Height;
		int num2 = Math.Max(bounds6.Height, bounds8.Height) - bounds7.Height;
		int num3 = Math.Abs(Math.Max(bounds.Width, bounds6.Width) - bounds3.Width);
		int num4 = Math.Max(bounds3.Width, bounds8.Width) - bounds5.Width;
		if (texture != null)
		{
			batcher.Draw(texture, new Vector2(x, y), bounds, color);
		}
		if (texture2 != null)
		{
			batcher.DrawTiled(texture2, new Rectangle(x + bounds.Width, y, base.Width - bounds.Width - bounds3.Width, bounds2.Height), bounds2, color);
		}
		if (texture3 != null)
		{
			batcher.Draw(texture3, new Vector2(x + (base.Width - bounds3.Width), y + num), bounds3, color);
		}
		if (texture4 != null)
		{
			batcher.DrawTiled(texture4, new Rectangle(x, y + bounds.Height, bounds4.Width, base.Height - bounds.Height - bounds6.Height), bounds4, color);
		}
		if (texture5 != null)
		{
			batcher.DrawTiled(texture5, new Rectangle(x + (base.Width - bounds5.Width), y + bounds3.Height, bounds5.Width, base.Height - bounds3.Height - bounds8.Height), bounds5, color);
		}
		if (texture6 != null)
		{
			batcher.Draw(texture6, new Vector2(x, y + (base.Height - bounds6.Height)), bounds6, color);
		}
		if (texture7 != null)
		{
			batcher.DrawTiled(texture7, new Rectangle(x + bounds6.Width, y + (base.Height - bounds7.Height - num2), base.Width - bounds6.Width - bounds8.Width, bounds7.Height), bounds7, color);
		}
		if (texture8 != null)
		{
			batcher.Draw(texture8, new Vector2(x + (base.Width - bounds8.Width), y + (base.Height - bounds8.Height)), bounds8, color);
		}
		if (texture9 != null)
		{
			batcher.DrawTiled(texture9, new Rectangle(x + bounds.Width, y + bounds.Height, base.Width - bounds.Width - bounds3.Width + (num3 + num4), base.Height - bounds3.Height - bounds8.Height), bounds9, color);
		}
	}

	private Texture2D GetTexture(int index, out Rectangle bounds)
	{
		if (index >= 0 && index <= _maxIndex)
		{
			if (index >= 8)
			{
				index = 4;
			}
			else if (index >= 4)
			{
				index++;
			}
			return GumpsLoader.Instance.GetGumpTexture((ushort)(Graphic + index), out bounds);
		}
		bounds = Rectangle.Empty;
		return null;
	}
}
