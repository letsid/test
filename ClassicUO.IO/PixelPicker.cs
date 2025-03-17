using System;
using System.Collections.Generic;

namespace ClassicUO.IO;

internal class PixelPicker
{
	private const int InitialDataCount = 262144;

	private Dictionary<ulong, int> m_IDs = new Dictionary<ulong, int>();

	private readonly List<byte> m_Data = new List<byte>(262144);

	public bool Get(ulong textureID, int x, int y, int extraRange = 0)
	{
		if (!m_IDs.TryGetValue(textureID, out var value))
		{
			return false;
		}
		int num = ReadIntegerFromData(ref value);
		if (x < 0 || x >= num)
		{
			return false;
		}
		int num2 = ReadIntegerFromData(ref value);
		if (y < 0 || y >= num2)
		{
			return false;
		}
		int num3 = 0;
		int num4 = x + y * num;
		bool flag = true;
		while (num3 <= num4)
		{
			int num5 = ReadIntegerFromData(ref value);
			num3 += num5;
			if (extraRange == 0)
			{
				if (num4 < num3)
				{
					return !flag;
				}
			}
			else if (!flag)
			{
				int num6 = num3 / num;
				int num7 = num3 % num;
				int num8 = num7 - num5;
				for (int i = -extraRange; i <= extraRange; i++)
				{
					if (y + i == num6 && x + extraRange >= num8 && x - extraRange <= num7)
					{
						return true;
					}
				}
			}
			flag = !flag;
		}
		return false;
	}

	public void GetDimensions(ulong textureID, out int width, out int height)
	{
		if (!m_IDs.TryGetValue(textureID, out var value))
		{
			width = (height = 0);
			return;
		}
		width = ReadIntegerFromData(ref value);
		height = ReadIntegerFromData(ref value);
	}

	public void Set(ulong textureID, int width, int height, Span<uint> pixels)
	{
		if (Has(textureID))
		{
			return;
		}
		int count = m_Data.Count;
		WriteIntegerToData(width);
		WriteIntegerToData(height);
		bool flag = true;
		int num = 0;
		int i = 0;
		for (int num2 = width * height; i < num2; i++)
		{
			bool flag2 = (pixels[i] & 0xFF000000u) == 0;
			if (flag != flag2)
			{
				WriteIntegerToData(num);
				flag = !flag;
				num = 0;
			}
			num++;
		}
		WriteIntegerToData(num);
		m_IDs[textureID] = count;
	}

	public bool Has(ulong textureID)
	{
		return m_IDs.ContainsKey(textureID);
	}

	private void WriteIntegerToData(int value)
	{
		while (value > 127)
		{
			m_Data.Add((byte)((value & 0x7F) | 0x80));
			value >>= 7;
		}
		m_Data.Add((byte)value);
	}

	private int ReadIntegerFromData(ref int index)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			byte b = m_Data[index++];
			num += (b & 0x7F) << num2;
			if ((b & 0x80) == 0)
			{
				break;
			}
			num2 += 7;
		}
		return num;
	}
}
