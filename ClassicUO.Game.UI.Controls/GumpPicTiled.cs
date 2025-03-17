using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class GumpPicTiled : Control
{
	private ushort _graphic;

	public ushort Graphic
	{
		get
		{
			return _graphic;
		}
		set
		{
			if (_graphic != value && value != ushort.MaxValue)
			{
				_graphic = value;
				if (GumpsLoader.Instance.GetGumpTexture(_graphic, out var bounds) == null)
				{
					Dispose();
					return;
				}
				base.Width = bounds.Width;
				base.Height = bounds.Height;
			}
		}
	}

	public ushort Hue { get; set; }

	public GumpPicTiled(ushort graphic)
	{
		CanMove = true;
		AcceptMouseInput = true;
		Graphic = graphic;
	}

	public GumpPicTiled(int x, int y, int width, int heigth, ushort graphic)
		: this(graphic)
	{
		base.X = x;
		base.Y = y;
		if (width > 0)
		{
			base.Width = width;
		}
		if (heigth > 0)
		{
			base.Height = heigth;
		}
	}

	public GumpPicTiled(List<string> parts)
		: this(UInt16Converter.Parse(parts[5]))
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = int.Parse(parts[3]);
		base.Height = int.Parse(parts[4]);
		base.IsFromServer = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, partial: false, base.Alpha, gump: true);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(Graphic, out bounds);
		if (gumpTexture != null)
		{
			batcher.DrawTiled(gumpTexture, new Rectangle(x, y, base.Width, base.Height), bounds, hueVector);
		}
		return base.Draw(batcher, x, y);
	}

	public override bool Contains(int x, int y)
	{
		int num = base.Width;
		int num2 = base.Height;
		x -= base.Offset.X;
		y -= base.Offset.Y;
		if (GumpsLoader.Instance.GetGumpTexture(Graphic, out var bounds) == null)
		{
			return false;
		}
		if (num == 0)
		{
			num = bounds.Width;
		}
		if (num2 == 0)
		{
			num2 = bounds.Height;
		}
		while (x > bounds.Width && num > bounds.Width)
		{
			x -= bounds.Width;
			num -= bounds.Width;
		}
		while (y > bounds.Height && num2 > bounds.Height)
		{
			y -= bounds.Height;
			num2 -= bounds.Height;
		}
		if (x > num || y > num2)
		{
			return false;
		}
		return GumpsLoader.Instance.PixelCheck(Graphic, x, y);
	}
}
