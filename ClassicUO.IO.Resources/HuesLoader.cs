using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClassicUO.Utility;

namespace ClassicUO.IO.Resources;

internal class HuesLoader : UOFileLoader
{
	private static HuesLoader _instance;

	public static HuesLoader Instance => _instance ?? (_instance = new HuesLoader());

	public HuesGroup[] HuesRange { get; private set; }

	public int HuesCount { get; private set; }

	public FloatHues[] Palette { get; private set; }

	public ushort[] RadarCol { get; private set; }

	private HuesLoader()
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("hues.mul");
			FileSystemHelper.EnsureFileExists(uOFilePath);
			UOFileMul uOFileMul = new UOFileMul(uOFilePath);
			int num = Marshal.SizeOf<HuesGroup>();
			int num2 = (int)uOFileMul.Length / num;
			HuesCount = num2 * 8;
			HuesRange = new HuesGroup[num2];
			ulong num3 = (ulong)(long)uOFileMul.StartAddress;
			for (int i = 0; i < num2; i++)
			{
				HuesRange[i] = Marshal.PtrToStructure<HuesGroup>((IntPtr)((long)num3 + (long)(i * num)));
			}
			string uOFilePath2 = UOFileManager.GetUOFilePath("radarcol.mul");
			FileSystemHelper.EnsureFileExists(uOFilePath2);
			UOFileMul uOFileMul2 = new UOFileMul(uOFilePath2);
			RadarCol = uOFileMul2.ReadArray<ushort>((int)uOFileMul2.Length >> 1);
			uOFileMul.Dispose();
			uOFileMul2.Dispose();
		});
	}

	public float[] CreateHuesPalette()
	{
		float[] array = new float[96 * HuesCount];
		Palette = new FloatHues[HuesCount];
		int num = HuesCount >> 3;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				int num2 = i * 8 + j;
				Palette[num2].Palette = new float[96];
				for (int k = 0; k < 32; k++)
				{
					int num3 = k * 3;
					ushort num4 = HuesRange[i].Entries[j].ColorTable[k];
					Palette[num2].Palette[num3] = (float)((num4 >> 10) & 0x1F) / 31f;
					Palette[num2].Palette[num3 + 1] = (float)((num4 >> 5) & 0x1F) / 31f;
					Palette[num2].Palette[num3 + 2] = (float)(num4 & 0x1F) / 31f;
					array[num2 * 96 + num3] = Palette[num2].Palette[num3];
					array[num2 * 96 + num3 + 1] = Palette[num2].Palette[num3 + 1];
					array[num2 * 96 + num3 + 2] = Palette[num2].Palette[num3 + 2];
				}
			}
		}
		return array;
	}

	public void CreateShaderColors(uint[] buffer)
	{
		int num = HuesRange.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < 32; k++)
				{
					buffer[num2++] = HuesHelper.Color16To32(HuesRange[i].Entries[j].ColorTable[k]) | 0xFF000000u;
					if (num2 >= buffer.Length)
					{
						return;
					}
				}
			}
		}
	}

	public ushort GetColor16(ushort c, ushort color)
	{
		if (color != 0 && color < HuesCount)
		{
			color--;
			int num = color >> 3;
			int num2 = color % 8;
			return HuesRange[num].Entries[num2].ColorTable[(c >> 10) & 0x1F];
		}
		return c;
	}

	public uint GetPolygoneColor(ushort c, ushort color)
	{
		if (color != 0 && color < HuesCount)
		{
			color--;
			int num = color >> 3;
			int num2 = color % 8;
			return HuesHelper.Color16To32(HuesRange[num].Entries[num2].ColorTable[c]);
		}
		return 4278255873u;
	}

	public uint GetUnicodeFontColor(ushort c, ushort color, bool unicode)
	{
		if (color != 0 && color < HuesCount)
		{
			color--;
			int num = color >> 3;
			int num2 = color % 8;
			byte b = 29;
			if (!unicode)
			{
				b = 20;
			}
			return HuesHelper.Color16To32R(HuesRange[num].Entries[num2].ColorTable[b]);
		}
		return HuesHelper.Color16To32R(c);
	}

	public uint GetColor(ushort c, ushort color)
	{
		if (color != 0 && color < HuesCount)
		{
			color--;
			int num = color >> 3;
			int num2 = color % 8;
			return HuesHelper.Color16To32(HuesRange[num].Entries[num2].ColorTable[(c >> 10) & 0x1F]);
		}
		if (color == 0)
		{
			return HuesHelper.Color16To32(c);
		}
		return HuesHelper.Color16To32(color);
	}

	public uint GetPartialHueColor(ushort c, ushort color)
	{
		if (color != 0 && color < HuesCount)
		{
			color--;
			int num = color >> 3;
			int num2 = color % 8;
			uint num3 = HuesHelper.Color16To32(c);
			byte b = (byte)(num3 & 0xFF);
			byte b2 = (byte)((num3 >> 8) & 0xFF);
			byte b3 = (byte)((num3 >> 16) & 0xFF);
			if (b == b2 && b == b3)
			{
				num3 = HuesHelper.Color16To32(HuesRange[num].Entries[num2].ColorTable[(c >> 10) & 0x1F]);
			}
			return num3;
		}
		return HuesHelper.Color16To32(c);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ushort GetRadarColorData(int c)
	{
		if (c >= 0 && c < RadarCol.Length)
		{
			return RadarCol[c];
		}
		return 0;
	}
}
