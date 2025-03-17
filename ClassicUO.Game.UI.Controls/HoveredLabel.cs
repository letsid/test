using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class HoveredLabel : Label
{
	private readonly ushort _overHue;

	private readonly ushort _normalHue;

	private readonly ushort _selectedHue;

	public bool DrawBackgroundCurrentIndex;

	public bool IsSelected;

	public bool ForceHover;

	public HoveredLabel(string text, bool isunicode, ushort hue, ushort overHue, ushort selectedHue, int maxwidth = 0, byte font = byte.MaxValue, FontStyle style = FontStyle.None, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT)
		: base(" " + text, isunicode, hue, maxwidth, font, style, align)
	{
		_overHue = overHue;
		_normalHue = hue;
		_selectedHue = selectedHue;
		AcceptMouseInput = true;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (IsSelected)
		{
			if (base.Hue != _selectedHue)
			{
				base.Hue = _selectedHue;
			}
		}
		else if (base.MouseIsOver || ForceHover)
		{
			if (base.Hue != _overHue)
			{
				base.Hue = _overHue;
			}
		}
		else if (base.Hue != _normalHue)
		{
			base.Hue = _normalHue;
		}
		base.Update(totalTime, frameTime);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (DrawBackgroundCurrentIndex && base.MouseIsOver && !string.IsNullOrWhiteSpace(base.Text))
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.Gray), new Rectangle(x, y + 2, base.Width - 4, base.Height - 4), hueVector);
		}
		return base.Draw(batcher, x, y);
	}
}
