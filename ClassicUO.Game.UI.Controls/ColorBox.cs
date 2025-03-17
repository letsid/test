using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class ColorBox : Control
{
	public ushort Hue { get; set; }

	public ColorBox(int width, int height, ushort hue)
	{
		CanMove = false;
		base.Width = width;
		base.Height = height;
		Hue = hue;
		base.WantUpdateSize = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(x, y, base.Width, base.Height), hueVector);
		return true;
	}
}
