using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal static class SolidColorTextureCache
{
	private static readonly Dictionary<Color, Texture2D> _textures = new Dictionary<Color, Texture2D>();

	public static Texture2D GetTexture(Color color)
	{
		if (_textures.TryGetValue(color, out var value))
		{
			return value;
		}
		value = new Texture2D(Client.Game.GraphicsDevice, 1, 1, mipMap: false, SurfaceFormat.Color);
		value.SetData(new Color[1] { color });
		_textures[color] = value;
		return value;
	}
}
