using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbRectPackSharp;

namespace ClassicUO.Renderer;

internal class TextureAtlas : IDisposable
{
	private readonly int _width;

	private readonly int _height;

	private readonly SurfaceFormat _format;

	private readonly GraphicsDevice _device;

	private readonly List<Texture2D> _textureList;

	private Packer _packer;

	public static TextureAtlas Shared { get; }

	public int TexturesCount => _textureList.Count;

	static TextureAtlas()
	{
		Shared = new TextureAtlas(Client.Game.GraphicsDevice, 4096, 4096, SurfaceFormat.Color);
	}

	public TextureAtlas(GraphicsDevice device, int width, int height, SurfaceFormat format)
	{
		_device = device;
		_width = width;
		_height = height;
		_format = format;
		_textureList = new List<Texture2D>();
	}

	public unsafe Texture2D AddSprite<T>(Span<T> pixels, int width, int height, out Rectangle pr) where T : unmanaged
	{
		int num = _textureList.Count - 1;
		if (num < 0)
		{
			num = 0;
			CreateNewTexture2D();
		}
		while (!_packer.PackRect(width, height, out pr))
		{
			CreateNewTexture2D();
			num = _textureList.Count - 1;
		}
		Texture2D texture2D = _textureList[num];
		fixed (T* ptr = pixels)
		{
			texture2D.SetDataPointerEXT(0, pr, (IntPtr)ptr, sizeof(T) * pixels.Length);
		}
		return texture2D;
	}

	private void CreateNewTexture2D()
	{
		Texture2D item = new Texture2D(_device, _width, _height, mipMap: false, _format);
		_textureList.Add(item);
		_packer?.Dispose();
		_packer = new Packer(_width, _height);
	}

	public void SaveImages(string name)
	{
		int i = 0;
		for (int texturesCount = TexturesCount; i < texturesCount; i++)
		{
			Texture2D texture2D = _textureList[i];
			using FileStream stream = File.Create($"atlas/{name}_atlas_{i}.png");
			texture2D.SaveAsPng(stream, texture2D.Width, texture2D.Height);
		}
	}

	public void Dispose()
	{
		foreach (Texture2D texture in _textureList)
		{
			if (!texture.IsDisposed)
			{
				texture.Dispose();
			}
		}
		_packer.Dispose();
		_textureList.Clear();
	}
}
