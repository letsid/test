using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class Line : Control
{
	private readonly Texture2D _texture;

	public Line(int x, int y, int w, int h, uint color)
	{
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		_texture = SolidColorTextureCache.GetTexture(new Color
		{
			PackedValue = color
		});
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha);
		batcher.Draw(_texture, new Rectangle(x, y, base.Width, base.Height), hueVector);
		return true;
	}
}
