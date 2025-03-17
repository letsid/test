using System;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game;

internal static class CircleOfTransparency
{
	private static readonly Lazy<DepthStencilState> _stencil = new Lazy<DepthStencilState>(() => new DepthStencilState
	{
		StencilEnable = true,
		StencilFunction = CompareFunction.Always,
		StencilPass = StencilOperation.Replace,
		ReferenceStencil = 1
	});

	private static Texture2D _texture;

	private static short _width;

	private static short _height;

	private static int _radius;

	public static uint[] CreateCircleTexture(int radius, ref short width, ref short height)
	{
		int num = radius + 1;
		int num2 = num * 2;
		uint[] array = new uint[num2 * num2];
		width = (short)num2;
		height = (short)num2;
		for (int i = -num; i < num; i++)
		{
			int num3 = i * i;
			int num4 = (i + num) * num2 + num;
			for (int j = -num; j < num; j++)
			{
				int num5 = (int)Math.Sqrt(num3 + j * j);
				uint num6 = ((num5 <= radius) ? ((uint)((radius - num5) & 0xFF)) : 0u);
				int num7 = num4 + j;
				array[num7] = num6;
			}
		}
		return array;
	}

	public static void Draw(UltimaBatcher2D batcher, Vector2 pos, ushort hue = 0)
	{
		if (_texture != null)
		{
			pos.X -= _width >> 1;
			pos.Y -= _height >> 1;
			Vector3 color = default(Vector3);
			if (hue == 0)
			{
				color.X = 0f;
				color.Y = 0f;
			}
			else
			{
				color.X = (int)hue;
				color.Y = 1f;
			}
			batcher.SetStencil(_stencil.Value);
			batcher.Draw(_texture, pos, color);
			batcher.SetStencil(null);
		}
	}

	public static void Create(int radius)
	{
		if (radius < 50)
		{
			radius = 50;
		}
		else if (radius > 200)
		{
			radius = 200;
		}
		if (_radius == radius && _texture != null && !_texture.IsDisposed)
		{
			return;
		}
		_radius = radius;
		_texture?.Dispose();
		_texture = null;
		uint[] array = CreateCircleTexture(radius, ref _width, ref _height);
		for (int i = 0; i < array.Length; i++)
		{
			ref uint reference = ref array[i];
			if (reference != 0)
			{
				reference = HuesHelper.RgbaToArgb(reference);
			}
		}
		_texture = new Texture2D(Client.Game.GraphicsDevice, _width, _height);
		_texture.SetData(array);
	}
}
