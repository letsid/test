using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ButtonTileArt : Button
{
	private readonly ushort _hue;

	private readonly bool _isPartial;

	private readonly int _tileX;

	private readonly int _tileY;

	private ushort _graphic;

	public ButtonTileArt(List<string> gparams)
		: base(gparams)
	{
		base.X = int.Parse(gparams[1]);
		base.Y = int.Parse(gparams[2]);
		_graphic = UInt16Converter.Parse(gparams[8]);
		_hue = UInt16Converter.Parse(gparams[9]);
		_tileX = int.Parse(gparams[10]);
		_tileY = int.Parse(gparams[11]);
		base.ContainsByBounds = true;
		base.IsFromServer = true;
		if (ArtLoader.Instance.GetStaticTexture(_graphic, out var _) == null)
		{
			Dispose();
		}
		else
		{
			_isPartial = TileDataLoader.Instance.StaticData[_graphic].IsPartialHue;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(_hue, _isPartial, 1f);
		Rectangle bounds;
		Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(_graphic, out bounds);
		if (staticTexture != null)
		{
			batcher.Draw(staticTexture, new Vector2(x + _tileX, y + _tileY), bounds, hueVector);
			return true;
		}
		return false;
	}
}
