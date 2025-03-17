using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class HitBox : Control
{
	protected readonly Texture2D _texture;

	public override ClickPriority Priority { get; set; }

	public HitBox(int x, int y, int w, int h, string tooltip = null, float alpha = 0.25f)
	{
		CanMove = false;
		AcceptMouseInput = true;
		base.Alpha = alpha;
		_texture = SolidColorTextureCache.GetTexture(Color.White);
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		base.WantUpdateSize = false;
		SetTooltip(tooltip);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		if (base.MouseIsOver)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha, gump: true);
			batcher.Draw(_texture, new Vector2(x, y), new Rectangle(0, 0, base.Width, base.Height), hueVector);
		}
		return base.Draw(batcher, x, y);
	}
}
