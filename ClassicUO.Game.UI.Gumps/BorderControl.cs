using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class BorderControl : Control
{
	private readonly int _borderSize;

	private const ushort H_BORDER = 2700;

	private const ushort V_BORDER = 2701;

	public ushort Hue { get; set; }

	public BorderControl(int x, int y, int w, int h, int borderSize)
	{
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		_borderSize = borderSize;
		CanMove = true;
		AcceptMouseInput = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		if (Hue != 0)
		{
			hueVector.X = (int)Hue;
			hueVector.Y = 1f;
		}
		if (_borderSize != 0)
		{
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(2700u, out var bounds);
			batcher.DrawTiled(gumpTexture, new Rectangle(x, y, base.Width, _borderSize), bounds, hueVector);
			batcher.DrawTiled(gumpTexture, new Rectangle(x, y + base.Height - _borderSize, base.Width, _borderSize), bounds, hueVector);
			gumpTexture = GumpsLoader.Instance.GetGumpTexture(2701u, out bounds);
			batcher.DrawTiled(gumpTexture, new Rectangle(x, y, _borderSize, base.Height), bounds, hueVector);
			batcher.DrawTiled(gumpTexture, new Rectangle(x + base.Width - _borderSize, y + (bounds.Width >> 1), _borderSize, base.Height - _borderSize), bounds, hueVector);
		}
		return base.Draw(batcher, x, y);
	}
}
