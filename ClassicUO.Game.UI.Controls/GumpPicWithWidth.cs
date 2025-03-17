using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class GumpPicWithWidth : GumpPic
{
	public int Percent { get; set; }

	public GumpPicWithWidth(int x, int y, ushort graphic, ushort hue, int perc)
		: base(x, y, graphic, hue)
	{
		Percent = perc;
		CanMove = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(base.Hue);
		Rectangle bounds;
		Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(base.Graphic, out bounds);
		if (gumpTexture != null)
		{
			batcher.DrawTiled(gumpTexture, new Rectangle(x, y, Percent, base.Height), bounds, hueVector);
			return true;
		}
		return false;
	}
}
