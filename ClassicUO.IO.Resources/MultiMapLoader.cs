using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Game;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class MultiMapLoader : UOFileLoader
{
	private static MultiMapLoader _instance;

	private readonly UOFileMul[] _facets = new UOFileMul[6];

	private UOFile _file;

	public static MultiMapLoader Instance => _instance ?? (_instance = new MultiMapLoader());

	private MultiMapLoader()
	{
	}

	internal bool HasFacet(int map)
	{
		if (map >= 0 && map < _facets.Length)
		{
			return _facets[map] != null;
		}
		return false;
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("Multimap.rle");
			if (File.Exists(uOFilePath))
			{
				_file = new UOFile(uOFilePath, loadFile: true);
			}
			for (int i = 0; i < 6; i++)
			{
				uOFilePath = UOFileManager.GetUOFilePath($"facet0{i}.mul");
				if (File.Exists(uOFilePath))
				{
					_facets[i] = new UOFileMul(uOFilePath);
				}
			}
		});
	}

	public unsafe Texture2D LoadMap(int width, int height, int startx, int starty, int endx, int endy)
	{
		if (_file == null || _file.Length == 0L)
		{
			Log.Warn("MultiMap.rle is not loaded!");
			return null;
		}
		_file.Seek(0);
		int num = _file.ReadInt();
		int num2 = _file.ReadInt();
		if (num < 1 || num2 < 1)
		{
			Log.Warn("Failed to load bounds from MultiMap.rle");
			return null;
		}
		int num3 = width * height;
		startx >>= 1;
		endx >>= 1;
		int num4 = endx - startx;
		if (num4 == 0)
		{
			num4++;
		}
		starty >>= 1;
		endy >>= 1;
		int num5 = endy - starty;
		if (num5 == 0)
		{
			num5++;
		}
		int num6 = (width << 8) / num4;
		int num7 = (height << 8) / num5;
		byte[] array = new byte[num3];
		int num8 = 0;
		int num9 = 0;
		int num10 = 1;
		int num11 = starty * num7;
		while (_file.Position < _file.Length)
		{
			byte num12 = _file.ReadByte();
			byte b = (byte)(num12 & 0x7F);
			bool flag = (num12 & 0x80) != 0;
			int num13 = num9 * num7;
			int num14 = width * (num13 - num11 >> 8);
			for (int i = 0; i < b; i++)
			{
				if (flag && num8 >= startx && num8 < endx && num9 >= starty && num9 < endy)
				{
					int num15 = num14 + (num6 * (num8 - startx) >> 8);
					ref byte reference = ref array[num15];
					if (reference < byte.MaxValue)
					{
						if (reference == num10)
						{
							num10++;
						}
						reference++;
					}
				}
				num8++;
				if (num8 >= num)
				{
					num8 = 0;
					num9++;
					num13 += num7;
					num14 = width * (num13 - num11 >> 8);
				}
			}
		}
		if (num10 >= 1)
		{
			int num16 = Marshal.SizeOf<HuesGroup>();
			IntPtr intPtr = Marshal.AllocHGlobal(num16 * HuesLoader.Instance.HuesRange.Length);
			for (int j = 0; j < HuesLoader.Instance.HuesRange.Length; j++)
			{
				Marshal.StructureToPtr(HuesLoader.Instance.HuesRange[j], intPtr + j * num16, fDeleteOld: false);
			}
			ushort* ptr = (ushort*)(void*)(intPtr + 30800);
			uint[] array2 = ArrayPool<uint>.Shared.Rent(num10, zero: true);
			Texture2D texture2D = new Texture2D(Client.Game.GraphicsDevice, width, height, mipMap: false, SurfaceFormat.Color);
			try
			{
				int num17 = 31 * num10;
				for (int k = 0; k < num10; k++)
				{
					num17 -= 31;
					array2[k] = HuesHelper.Color16To32(ptr[num17 / num10]) | 0xFF000000u;
				}
				uint[] array3 = ArrayPool<uint>.Shared.Rent(num3, zero: true);
				try
				{
					for (int l = 0; l < num3; l++)
					{
						byte b2 = array[l];
						array3[l] = ((b2 != 0) ? array2[b2 - 1] : 0u);
					}
					texture2D.SetData(array3, 0, width * height);
					return texture2D;
				}
				finally
				{
					ArrayPool<uint>.Shared.Return(array3, clearArray: true);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
				ArrayPool<uint>.Shared.Return(array2, clearArray: true);
			}
		}
		return null;
	}

	public Texture2D LoadFacet(int facet, int width, int height, int startx, int starty, int endx, int endy)
	{
		if (_file == null || facet < 0 || facet > Constants.MAPS_COUNT || _facets[facet] == null)
		{
			return null;
		}
		_facets[facet].Seek(0);
		short num = _facets[facet].ReadShort();
		int num2 = _facets[facet].ReadShort();
		if (num < 1 || num2 < 1)
		{
			return null;
		}
		int num3 = ((endx <= 0) ? width : endx);
		int num4 = ((endy <= 0) ? height : endy);
		int num5 = num3 - startx;
		int num6 = num4 - starty;
		uint[] array = ArrayPool<uint>.Shared.Rent(num5 * num6, zero: true);
		Texture2D texture2D = new Texture2D(Client.Game.GraphicsDevice, num5, num6, mipMap: false, SurfaceFormat.Color);
		try
		{
			for (int i = 0; i < num2; i++)
			{
				int num7 = 0;
				int num8 = _facets[facet].ReadInt() / 3;
				for (int j = 0; j < num8; j++)
				{
					int num9 = _facets[facet].ReadByte();
					uint num10 = HuesHelper.Color16To32(_facets[facet].ReadUShort()) | 0xFF000000u;
					for (int k = 0; k < num9; k++)
					{
						if (num7 >= startx && num7 < num3 && i >= starty && i < num4)
						{
							array[(i - starty) * num5 + (num7 - startx)] = num10;
						}
						num7++;
					}
				}
			}
			texture2D.SetData(array, 0, width * height);
			return texture2D;
		}
		finally
		{
			ArrayPool<uint>.Shared.Return(array, clearArray: true);
		}
	}
}
