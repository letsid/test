using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class ClickableColorBox : ColorBox
{
	public ClickableColorBox(int x, int y, int w, int h, ushort hue)
		: base(w, h, hue)
	{
		base.X = x;
		base.Y = y;
		base.WantUpdateSize = false;
		GumpPic gumpPic = new GumpPic(0, 0, 212, 0);
		Add(gumpPic);
		base.Width = gumpPic.Width;
		base.Height = gumpPic.Height;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.Children.Count != 0)
		{
			base.Children[0].Draw(batcher, x, y);
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(base.Hue);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(x + 3, y + 3, base.Width - 6, base.Height - 6), hueVector);
		return true;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			UIManager.GetGump<ColorPickerGump>(null)?.Dispose();
			UIManager.Add(new ColorPickerGump(0u, 0, 100, 100, delegate(ushort s)
			{
				base.Hue = s;
			}));
		}
	}
}
