using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class StaticPic : Control
{
	private enum StaticPicXMode
	{
		StaticPicXMode_Center,
		StaticPicXMode_LeftMost,
		StaticPicXMode_RightMost
	}

	private enum StaticPicYMode
	{
		StaticPicYMode_Center,
		StaticPicYMode_TopMost,
		StaticPicYMode_BottomMost
	}

	private ushort _graphic;

	private StaticPicXMode xMode { get; set; }

	private StaticPicYMode yMode { get; set; }

	private int WidthForMode { get; set; }

	private int HeightForMode { get; set; }

	public ushort Hue { get; set; }

	public bool IsPartialHue { get; set; }

	public ushort Graphic
	{
		get
		{
			return _graphic;
		}
		set
		{
			_graphic = value;
			if (ArtLoader.Instance.GetStaticTexture(value, out var bounds) == null)
			{
				Dispose();
				return;
			}
			base.Width = bounds.Width;
			base.Height = bounds.Height;
			IsPartialHue = TileDataLoader.Instance.StaticData[value].IsPartialHue;
		}
	}

	public StaticPic(ushort graphic, ushort hue)
	{
		Hue = hue;
		Graphic = graphic;
		CanMove = true;
		base.WantUpdateSize = false;
	}

	public StaticPic(List<string> parts)
		: this(UInt16Converter.Parse(parts[3]), (ushort)((parts.Count > 4) ? UInt16Converter.Parse(parts[4]) : 0))
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.IsFromServer = true;
		if (parts.Count <= 5)
		{
			return;
		}
		string text = parts[5];
		WidthForMode = int.Parse(parts[6]);
		HeightForMode = int.Parse(parts[7]);
		if (string.IsNullOrEmpty(text) || !text.StartsWith('c'))
		{
			return;
		}
		xMode = StaticPicXMode.StaticPicXMode_Center;
		yMode = StaticPicYMode.StaticPicYMode_Center;
		if (text.EndsWith('m'))
		{
			if (text.Contains('t'))
			{
				yMode = StaticPicYMode.StaticPicYMode_TopMost;
			}
			else if (text.Contains('b'))
			{
				yMode = StaticPicYMode.StaticPicYMode_BottomMost;
			}
			if (text.Contains('l'))
			{
				xMode = StaticPicXMode.StaticPicXMode_LeftMost;
			}
			else if (text.Contains('r'))
			{
				xMode = StaticPicXMode.StaticPicXMode_RightMost;
			}
		}
		Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(Graphic);
		if (realArtBounds.Width > WidthForMode && xMode == StaticPicXMode.StaticPicXMode_RightMost)
		{
			base.X = base.X - realArtBounds.Left + WidthForMode - realArtBounds.Width;
		}
		else if (realArtBounds.Width > WidthForMode && xMode == StaticPicXMode.StaticPicXMode_LeftMost)
		{
			base.X -= realArtBounds.Left;
		}
		else
		{
			base.X = base.X - realArtBounds.Left + WidthForMode / 2 - realArtBounds.Width / 2;
		}
		if (realArtBounds.Height > HeightForMode && yMode == StaticPicYMode.StaticPicYMode_TopMost)
		{
			base.Y -= realArtBounds.Top;
		}
		else
		{
			base.Y = base.Y - realArtBounds.Top + HeightForMode / 2 - realArtBounds.Height / 2;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, IsPartialHue, 1f);
		Rectangle bounds;
		Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(Graphic, out bounds);
		if (staticTexture != null)
		{
			batcher.Draw(staticTexture, new Rectangle(x, y, base.Width, base.Height), bounds, hueVector);
		}
		return base.Draw(batcher, x, y);
	}

	public override bool Contains(int x, int y)
	{
		return ArtLoader.Instance.PixelCheck(Graphic, x - base.Offset.X, y - base.Offset.Y);
	}
}
