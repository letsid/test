using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal sealed class AlphaBlendControl : Control
{
	public ushort Hue { get; set; }

	public AlphaBlendControl(float alpha = 0.5f)
	{
		base.Alpha = alpha;
		AcceptMouseInput = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, partial: false, base.Alpha);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), new Rectangle(x, y, base.Width, base.Height), hueVector);
		return true;
	}
}
