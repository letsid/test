using System;
using System.IO;
using System.Threading.Tasks;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class TexmapsLoader : UOFileLoader
{
	private struct SpriteInfo
	{
		public Texture2D Texture;

		public Rectangle UV;
	}

	private static TexmapsLoader _instance;

	private UOFile _file;

	private SpriteInfo[] _spriteInfos;

	public static TexmapsLoader Instance => _instance ?? (_instance = new TexmapsLoader(16384));

	private TexmapsLoader(int count)
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("texmaps.mul");
			string uOFilePath2 = UOFileManager.GetUOFilePath("texidx.mul");
			FileSystemHelper.EnsureFileExists(uOFilePath);
			FileSystemHelper.EnsureFileExists(uOFilePath2);
			_file = new UOFileMul(uOFilePath, uOFilePath2, 16384, 10);
			_file.FillEntries(ref Entries);
			string uOFilePath3 = UOFileManager.GetUOFilePath("TexTerr.def");
			if (File.Exists(uOFilePath3))
			{
				using (DefReader defReader = new DefReader(uOFilePath3))
				{
					while (defReader.Next())
					{
						int num = defReader.ReadInt();
						if (num >= 0 && num < Entries.Length)
						{
							int[] array = defReader.ReadGroup();
							if (array != null)
							{
								foreach (int num2 in array)
								{
									if (num2 >= 0 && num2 < Entries.Length)
									{
										Entries[num] = Entries[num2];
									}
								}
							}
						}
					}
				}
				_spriteInfos = new SpriteInfo[Entries.Length];
			}
		});
	}

	public Texture2D GetLandTexture(uint g, out Rectangle bounds)
	{
		TextureAtlas shared = TextureAtlas.Shared;
		ref SpriteInfo reference = ref _spriteInfos[g];
		if (reference.Texture == null)
		{
			AddSpriteToAtlas(shared, g);
		}
		bounds = reference.UV;
		return reference.Texture;
	}

	private void AddSpriteToAtlas(TextureAtlas atlas, uint index)
	{
		ref UOFileIndex validRefEntry = ref GetValidRefEntry((int)index);
		if (validRefEntry.Length <= 0)
		{
			return;
		}
		_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
		_file.Seek(validRefEntry.Offset);
		int num = ((validRefEntry.Length == 8192) ? 64 : 128);
		Span<uint> pixels = stackalloc uint[num * num];
		for (int i = 0; i < num; i++)
		{
			int num2 = i * num;
			for (int j = 0; j < num; j++)
			{
				pixels[num2 + j] = HuesHelper.Color16To32(_file.ReadUShort()) | 0xFF000000u;
			}
		}
		ref SpriteInfo reference = ref _spriteInfos[index];
		reference.Texture = atlas.AddSprite(pixels, num, num, out reference.UV);
	}
}
