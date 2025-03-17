using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class LightsLoader : UOFileLoader
{
	private struct SpriteInfo
	{
		public Texture2D Texture;

		public Rectangle UV;
	}

	public struct LightDefStruct
	{
		public ushort ID;

		public ushort Renderer;

		public ushort AutoCustomRenderer;

		public ushort DefaultCustomHue;
	}

	private static LightsLoader _instance;

	private UOFileMul _file;

	private TextureAtlas _atlas;

	private SpriteInfo[] _spriteInfos;

	public Dictionary<ushort, LightDefStruct> LightDefDict = new Dictionary<ushort, LightDefStruct>();

	public static LightsLoader Instance => _instance ?? (_instance = new LightsLoader(500));

	private LightsLoader(int count)
	{
	}

	public void CreateAtlas(GraphicsDevice device)
	{
		_atlas = new TextureAtlas(device, 2048, 2048, SurfaceFormat.Color);
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("light.mul");
			string uOFilePath2 = UOFileManager.GetUOFilePath("lightidx.mul");
			LoadLightsDef();
			FileSystemHelper.EnsureFileExists(uOFilePath);
			FileSystemHelper.EnsureFileExists(uOFilePath2);
			_file = new UOFileMul(uOFilePath, uOFilePath2, 500);
			_file.FillEntries(ref Entries);
			_spriteInfos = new SpriteInfo[Entries.Length];
		});
	}

	public void LoadLightsDef()
	{
		string uOFilePath = UOFileManager.GetUOFilePath("lights.def");
		if (!File.Exists(uOFilePath))
		{
			return;
		}
		using DefReader defReader = new DefReader(uOFilePath);
		while (defReader.Next())
		{
			ushort num = (ushort)defReader.ReadInt();
			ushort renderer = (ushort)defReader.ReadInt();
			ushort autoCustomRenderer = (ushort)defReader.ReadInt();
			ushort defaultCustomHue = (ushort)defReader.ReadInt();
			LightDefStruct lightDefStruct = default(LightDefStruct);
			lightDefStruct.ID = num;
			lightDefStruct.Renderer = renderer;
			lightDefStruct.AutoCustomRenderer = autoCustomRenderer;
			lightDefStruct.DefaultCustomHue = defaultCustomHue;
			LightDefStruct value = lightDefStruct;
			LightDefDict.Add(num, value);
		}
	}

	public Texture2D GetLightTexture(uint id, out Rectangle bounds)
	{
		if (id >= _spriteInfos.Length)
		{
			bounds = Rectangle.Empty;
			return null;
		}
		ref SpriteInfo reference = ref _spriteInfos[id];
		if (reference.Texture == null)
		{
			AddSpriteLightToAtlas(_atlas, id);
		}
		bounds = reference.UV;
		return reference.Texture;
	}

	private bool AddSpriteLightToAtlas(TextureAtlas atlas, uint idx)
	{
		ref UOFileIndex validRefEntry = ref GetValidRefEntry((int)idx);
		if (validRefEntry.Width == 0 && validRefEntry.Height == 0)
		{
			return false;
		}
		uint[] array = null;
		Span<uint> span = ((validRefEntry.Width * validRefEntry.Height > 1024) ? ((Span<uint>)(array = ArrayPool<uint>.Shared.Rent(validRefEntry.Width * validRefEntry.Height, zero: true))) : stackalloc uint[1024]);
		Span<uint> pixels = span;
		try
		{
			_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
			_file.Seek(validRefEntry.Offset);
			for (int i = 0; i < validRefEntry.Height; i++)
			{
				int num = i * validRefEntry.Width;
				for (int j = 0; j < validRefEntry.Width; j++)
				{
					ushort num2 = _file.ReadByte();
					if (num2 > 31)
					{
						num2 = (ushort)(~num2 & 0x1F);
					}
					uint num3 = (uint)((num2 << 19) | (num2 << 11) | (num2 << 3));
					if (num2 != 0)
					{
						pixels[num + j] = num3 | 0xFF000000u;
					}
				}
			}
			ref SpriteInfo reference = ref _spriteInfos[idx];
			reference.Texture = atlas.AddSprite(pixels, validRefEntry.Width, validRefEntry.Height, out reference.UV);
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<uint>.Shared.Return(array, clearArray: true);
			}
		}
		return true;
	}
}
