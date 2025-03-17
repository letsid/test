using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class GumpsLoader : UOFileLoader
{
	private struct SpriteInfo
	{
		public Texture2D Texture;

		public Rectangle UV;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private ref struct GumpBlock
	{
		public readonly ushort Value;

		public readonly ushort Run;
	}

	private static GumpsLoader _instance;

	private UOFile _file;

	private PixelPicker _picker = new PixelPicker();

	public List<Tuple<ushort, CustomBook>> BooksDefList = new List<Tuple<ushort, CustomBook>>();

	private const int ATLAS_SIZE = 4096;

	private TextureAtlas _atlas;

	private SpriteInfo[] _spriteInfos;

	public static GumpsLoader Instance => _instance ?? (_instance = new GumpsLoader(65536));

	private GumpsLoader(int count)
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("gumpartLegacyMUL.uop");
			if (Client.IsUOPInstallation && File.Exists(uOFilePath))
			{
				_file = new UOFileUop(uOFilePath, "build/gumpartlegacymul/{0:D8}.tga", hasextra: true);
				Entries = new UOFileIndex[Math.Max(((UOFileUop)_file).TotalEntriesCount, 65536)];
				Client.UseUOPGumps = true;
			}
			else
			{
				uOFilePath = UOFileManager.GetUOFilePath("gumpart.mul");
				string uOFilePath2 = UOFileManager.GetUOFilePath("gumpidx.mul");
				if (!File.Exists(uOFilePath))
				{
					uOFilePath = UOFileManager.GetUOFilePath("Gumpart.mul");
				}
				if (!File.Exists(uOFilePath2))
				{
					uOFilePath2 = UOFileManager.GetUOFilePath("Gumpidx.mul");
				}
				_file = new UOFileMul(uOFilePath, uOFilePath2, 65536, 12);
				Client.UseUOPGumps = false;
			}
			_file.FillEntries(ref Entries);
			string uOFilePath3 = UOFileManager.GetUOFilePath("books.def");
			if (File.Exists(uOFilePath3))
			{
				using DefReader defReader = new DefReader(uOFilePath3);
				while (defReader.Next())
				{
					ushort item = (ushort)defReader.ReadInt();
					ushort gumpID = (ushort)defReader.ReadInt();
					ushort textCol = (ushort)defReader.ReadInt();
					ushort renderer = (ushort)defReader.ReadInt();
					CustomBook item2 = new CustomBook(gumpID, textCol, renderer);
					BooksDefList.Add(new Tuple<ushort, CustomBook>(item, item2));
				}
			}
			if (File.Exists(UOFileManager.GetUOFilePath("gump.def")))
			{
				_spriteInfos = new SpriteInfo[Entries.Length];
			}
		});
	}

	public void CreateAtlas(GraphicsDevice device)
	{
		_atlas = new TextureAtlas(device, 4096, 4096, SurfaceFormat.Color);
	}

	public Texture2D GetGumpTexture(uint g, out Rectangle bounds)
	{
		ref SpriteInfo reference = ref _spriteInfos[g];
		if (reference.Texture == null)
		{
			AddSpriteToAtlas(_atlas, g);
		}
		bounds = reference.UV;
		return reference.Texture;
	}

	private unsafe void AddSpriteToAtlas(TextureAtlas atlas, uint index)
	{
		ref UOFileIndex validRefEntry = ref GetValidRefEntry((int)index);
		if (validRefEntry.Width <= 0 && validRefEntry.Height <= 0)
		{
			return;
		}
		ushort hue = validRefEntry.Hue;
		_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
		_file.Seek(validRefEntry.Offset);
		IntPtr positionAddress = _file.PositionAddress;
		uint[] array = null;
		Span<uint> span = ((validRefEntry.Width * validRefEntry.Height > 1024) ? ((Span<uint>)(array = ArrayPool<uint>.Shared.Rent(validRefEntry.Width * validRefEntry.Height, zero: true))) : stackalloc uint[1024]);
		Span<uint> pixels = span;
		try
		{
			int* ptr = (int*)(void*)positionAddress;
			int i = 0;
			int num = validRefEntry.Length >> 2;
			for (; i < validRefEntry.Height; i++)
			{
				int num2 = ((i >= validRefEntry.Height - 1) ? (num - ptr[i]) : (ptr[i + 1] - ptr[i]));
				GumpBlock* ptr2 = (GumpBlock*)(void*)(positionAddress + (ptr[i] << 2));
				int num3 = i * validRefEntry.Width;
				for (int j = 0; j < num2; j++)
				{
					uint num4 = ptr2[j].Value;
					if (hue != 0 && num4 != 0)
					{
						num4 = HuesLoader.Instance.GetColor16(ptr2[j].Value, hue);
					}
					if (num4 != 0)
					{
						num4 = HuesHelper.Color16To32(ptr2[j].Value) | 0xFF000000u;
					}
					int run = ptr2[j].Run;
					for (int k = 0; k < run; k++)
					{
						pixels[num3++] = num4;
					}
				}
			}
			ref SpriteInfo reference = ref _spriteInfos[index];
			reference.Texture = atlas.AddSprite(pixels, validRefEntry.Width, validRefEntry.Height, out reference.UV);
			_picker.Set(index, validRefEntry.Width, validRefEntry.Height, pixels);
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<uint>.Shared.Return(array, clearArray: true);
			}
		}
	}

	public bool PixelCheck(int index, int x, int y)
	{
		return _picker.Get((ulong)index, x, y);
	}
}
