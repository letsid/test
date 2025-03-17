using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.IO.Resources;

internal class ArtLoader : UOFileLoader
{
	private struct SpriteInfo
	{
		public Texture2D Texture;

		public Rectangle UV;

		public Rectangle ArtBounds;
	}

	private static ArtLoader _instance;

	private UOFile _file;

	private readonly ushort _graphicMask;

	private readonly PixelPicker _picker = new PixelPicker();

	private SpriteInfo[] _spriteInfos;

	public static ArtLoader Instance => _instance ?? (_instance = new ArtLoader(81920, 16384));

	private ArtLoader(int staticCount, int landCount)
	{
		_graphicMask = (ushort)(Client.IsUOPInstallation ? ushort.MaxValue : 16383);
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("artLegacyMUL.uop");
			if (Client.IsUOPInstallation && File.Exists(uOFilePath))
			{
				_file = new UOFileUop(uOFilePath, "build/artlegacymul/{0:D8}.tga");
				Entries = new UOFileIndex[Math.Max(((UOFileUop)_file).TotalEntriesCount, 81920)];
			}
			else
			{
				uOFilePath = UOFileManager.GetUOFilePath("art.mul");
				string uOFilePath2 = UOFileManager.GetUOFilePath("artidx.mul");
				if (File.Exists(uOFilePath) && File.Exists(uOFilePath2))
				{
					_file = new UOFileMul(uOFilePath, uOFilePath2, 81920);
				}
			}
			_file.FillEntries(ref Entries);
			_spriteInfos = new SpriteInfo[Entries.Length];
		});
	}

	public Rectangle GetRealArtBounds(int index)
	{
		if (index + 16384 < _spriteInfos.Length)
		{
			return _spriteInfos[index + 16384].ArtBounds;
		}
		return Rectangle.Empty;
	}

	private void AddSpriteToAtlas(TextureAtlas atlas, int g, bool isTerrain)
	{
		ref UOFileIndex validRefEntry = ref GetValidRefEntry(g);
		if (isTerrain)
		{
			if (validRefEntry.Length == 0)
			{
				return;
			}
			Span<uint> pixels = stackalloc uint[1936];
			_file.SetData(validRefEntry.Address, validRefEntry.FileSize);
			_file.Seek(validRefEntry.Offset);
			for (int i = 0; i < 22; i++)
			{
				int num = 22 - (i + 1);
				int num2 = i * 44 + num;
				int num3 = num + (i + 1 << 1);
				for (int j = num; j < num3; j++)
				{
					pixels[num2++] = HuesHelper.Color16To32(_file.ReadUShort()) | 0xFF000000u;
				}
			}
			for (int k = 0; k < 22; k++)
			{
				int num4 = (k + 22) * 44 + k;
				int num5 = k + (22 - k << 1);
				for (int l = k; l < num5; l++)
				{
					pixels[num4++] = HuesHelper.Color16To32(_file.ReadUShort()) | 0xFF000000u;
				}
			}
			ref SpriteInfo reference = ref _spriteInfos[g];
			if (StaticFilters.IsWater(g) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableCaveBorder)
			{
				AddBlackBorder(pixels, 44, 44);
			}
			reference.Texture = atlas.AddSprite(pixels, 44, 44, out reference.UV);
		}
		else
		{
			if (!ReadHeader(_file, ref validRefEntry, out var width, out var height))
			{
				return;
			}
			uint[] array = null;
			Span<uint> span = ((width * height > 1024) ? ((Span<uint>)(array = ArrayPool<uint>.Shared.Rent(width * height, zero: true))) : stackalloc uint[1024]);
			Span<uint> pixels2 = span;
			try
			{
				ushort num6 = (ushort)(g - 16384);
				if (!ReadData(pixels2, width, height, _file))
				{
					return;
				}
				if ((num6 >= 8275 && num6 <= 8290) || (num6 >= 8298 && num6 <= 8313))
				{
					for (int m = 0; m < width; m++)
					{
						pixels2[m] = 0u;
						pixels2[(height - 1) * width + m] = 0u;
					}
					for (int n = 0; n < height; n++)
					{
						pixels2[n * width] = 0u;
						pixels2[n * width + width - 1] = 0u;
					}
				}
				ref SpriteInfo reference2 = ref _spriteInfos[g];
				FinalizeData(pixels2, ref validRefEntry, num6, width, height, out reference2.ArtBounds);
				if (StaticFilters.IsWater(num6) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableCaveBorder)
				{
					AddBlackBorder(pixels2, width, height);
				}
				_picker.Set(num6, width, height, pixels2);
				reference2.Texture = atlas.AddSprite(pixels2, width, height, out reference2.UV);
			}
			finally
			{
				if (array != null)
				{
					ArrayPool<uint>.Shared.Return(array, clearArray: true);
				}
			}
		}
	}

	public Texture2D GetLandTexture(uint g, out Rectangle bounds)
	{
		g &= _graphicMask;
		TextureAtlas shared = TextureAtlas.Shared;
		ref SpriteInfo reference = ref _spriteInfos[g];
		if (reference.Texture == null)
		{
			AddSpriteToAtlas(shared, (int)g, isTerrain: true);
		}
		bounds = reference.UV;
		return reference.Texture;
	}

	public Texture2D GetStaticTexture(uint g, out Rectangle bounds)
	{
		g += 16384;
		TextureAtlas shared = TextureAtlas.Shared;
		ref SpriteInfo reference = ref _spriteInfos[g];
		if (reference.Texture == null)
		{
			AddSpriteToAtlas(shared, (int)g, isTerrain: false);
		}
		bounds = reference.UV;
		return reference.Texture;
	}

	public unsafe IntPtr CreateCursorSurfacePtr(int index, ushort customHue, out int hotX, out int hotY)
	{
		hotX = (hotY = 0);
		ref UOFileIndex validRefEntry = ref GetValidRefEntry(index + 16384);
		if (ReadHeader(_file, ref validRefEntry, out var width, out var height))
		{
			Span<uint> pixels = new uint[width * height];
			if (ReadData(pixels, width, height, _file))
			{
				FinalizeData(pixels, ref validRefEntry, (ushort)index, width, height, out var _);
				fixed (uint* ptr = pixels)
				{
					SDL.SDL_Surface* ptr2 = (SDL.SDL_Surface*)(void*)SDL.SDL_CreateRGBSurfaceWithFormatFrom((IntPtr)ptr, width, height, 32, 4 * width, SDL.SDL_PIXELFORMAT_ABGR8888);
					int num = ptr2->pitch >> 2;
					uint* ptr3 = (uint*)(void*)ptr2->pixels;
					uint* ptr4 = ptr3 + width;
					uint* ptr5 = ptr3 + num * height;
					int num2 = num - width;
					short num3 = 0;
					short num4 = 0;
					Color c = default(Color);
					while (ptr3 < ptr5)
					{
						num3 = 0;
						while (ptr3 < ptr4)
						{
							if (*ptr3 != 0 && *ptr3 != 4278190080u)
							{
								if (num3 >= width - 1 || num4 >= height - 1)
								{
									*ptr3 = 0u;
								}
								else if (num3 == 0 || num4 == 0)
								{
									if (*ptr3 == 4278255360u)
									{
										if (num3 == 0)
										{
											hotY = num4;
										}
										if (num4 == 0)
										{
											hotX = num3;
										}
									}
									*ptr3 = 0u;
								}
								else if (customHue > 0)
								{
									c.PackedValue = *ptr3;
									*ptr3 = HuesHelper.Color16To32(HuesLoader.Instance.GetColor16(HuesHelper.ColorToHue(c), customHue)) | 0xFF000000u;
								}
							}
							ptr3++;
							num3++;
						}
						ptr3 += num2;
						ptr4 += num;
						num4++;
					}
					return (IntPtr)ptr2;
				}
			}
		}
		return IntPtr.Zero;
	}

	public bool PixelCheck(int index, int x, int y)
	{
		return _picker.Get((ulong)index, x, y);
	}

	private bool ReadHeader(DataReader file, ref UOFileIndex entry, out short width, out short height)
	{
		if (entry.Length == 0)
		{
			width = 0;
			height = 0;
			return false;
		}
		file.SetData(entry.Address, entry.FileSize);
		file.Seek(entry.Offset);
		file.Skip(4);
		width = file.ReadShort();
		height = file.ReadShort();
		if (width > 0)
		{
			return height > 0;
		}
		return false;
	}

	private unsafe bool ReadData(Span<uint> pixels, int width, int height, DataReader file)
	{
		ushort* ptr = (ushort*)(void*)file.PositionAddress;
		ushort* ptr2 = ptr;
		byte* ptr3 = (byte*)ptr + height * 2;
		int num = 0;
		int num2 = 0;
		ptr = (ushort*)(ptr3 + *ptr2 * 2);
		while (num2 < height)
		{
			ushort num3 = *(ptr++);
			ushort num4 = *(ptr++);
			if (num3 + num4 >= 2048)
			{
				return false;
			}
			if (num3 + num4 != 0)
			{
				num += num3;
				int num5 = num2 * width + num;
				int num6 = 0;
				while (num6 < num4)
				{
					ushort num7 = *(ptr++);
					if (num7 != 0)
					{
						pixels[num5] = HuesHelper.Color16To32(num7) | 0xFF000000u;
					}
					num6++;
					num5++;
				}
				num += num4;
			}
			else
			{
				num = 0;
				num2++;
				ptr = (ushort*)(ptr3 + ptr2[num2] * 2);
			}
		}
		return true;
	}

	private void FinalizeData(Span<uint> pixels, ref UOFileIndex entry, ushort graphic, int width, int height, out Rectangle bounds)
	{
		int num = 0;
		int num2 = width;
		int num3 = height;
		int num4 = 0;
		int num5 = 0;
		if (StaticFilters.IsCave(graphic) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableCaveBorder)
		{
			AddBlackBorder(pixels, width, height);
		}
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (pixels[num++] != 0)
				{
					num2 = Math.Min(num2, j);
					num4 = Math.Max(num4, j);
					num3 = Math.Min(num3, i);
					num5 = Math.Max(num5, i);
				}
			}
		}
		entry.Width = (short)((width >> 1) - 22);
		entry.Height = (short)(height - 44);
		bounds.X = num2;
		bounds.Y = num3;
		bounds.Width = num4 - num2;
		bounds.Height = num5 - num3;
	}

	public void AddBlackBorder(Span<uint> pixels, int width, int height)
	{
		for (int i = 0; i < height; i++)
		{
			int num = ((i != 0) ? (-1) : 0);
			int num2 = ((i + 1 >= height) ? 1 : 2);
			for (int j = 0; j < width; j++)
			{
				ref uint reference = ref pixels[i * width + j];
				if (reference == 0)
				{
					continue;
				}
				int num3 = ((j != 0) ? (-1) : 0);
				int num4 = ((j + 1 >= width) ? 1 : 2);
				for (int k = num; k < num2; k++)
				{
					int num5 = i + k;
					for (int l = num3; l < num4; l++)
					{
						int num6 = j + l;
						if (pixels[num5 * width + num6] == 0)
						{
							reference = 4278190080u;
						}
					}
				}
			}
		}
	}
}
