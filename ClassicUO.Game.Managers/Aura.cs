using System;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.Managers;

internal class Aura : IDisposable
{
	private static readonly Lazy<BlendState> _blend = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Blend.SourceAlpha,
		ColorDestinationBlend = Blend.InverseSourceAlpha
	});

	private readonly Texture2D _texture;

	public Aura(int radius)
	{
		short width = 0;
		short height = 0;
		uint[] array = CircleOfTransparency.CreateCircleTexture(radius, ref width, ref height);
		for (int i = 0; i < array.Length; i++)
		{
			ref uint reference = ref array[i];
			if (reference != 0)
			{
				ushort num = (ushort)(reference << 3);
				if (num > 255)
				{
					num = 255;
				}
				reference = (uint)((num << 24) | (num << 16) | (num << 8) | num);
			}
		}
		_texture = new Texture2D(Client.Game.GraphicsDevice, width, height);
		_texture.SetData(array);
	}

	public void Draw(UltimaBatcher2D batcher, int x, int y, ushort hue, float depth)
	{
		x -= _texture.Width >> 1;
		y -= _texture.Height >> 1;
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, partial: false, 1f);
		batcher.SetBlendState(_blend.Value);
		batcher.Draw(_texture, new Vector2(x, y), null, hueVector, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth);
		batcher.SetBlendState(null);
	}

	public void Dispose()
	{
		if (_texture != null && !_texture.IsDisposed)
		{
			_texture.Dispose();
		}
	}
}
