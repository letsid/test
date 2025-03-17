using System;
using System.Buffers.Binary;
using System.IO;

namespace ClassicUO.Data;

internal static class ClientVersionHelper
{
	public static bool TryParseFromFile(string clientpath, out string version)
	{
		if (File.Exists(clientpath))
		{
			using FileStream fileStream = new FileStream(clientpath, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] array = new byte[fileStream.Length];
			fileStream.Read(array, 0, (int)fileStream.Length);
			Span<byte> span = stackalloc byte[30]
			{
				86, 0, 83, 0, 95, 0, 86, 0, 69, 0,
				82, 0, 83, 0, 73, 0, 79, 0, 78, 0,
				95, 0, 73, 0, 78, 0, 70, 0, 79, 0
			};
			for (int i = 0; i < array.Length; i++)
			{
				if (span.SequenceEqual(array.AsSpan(i, 30)))
				{
					int num = i + 42;
					ushort num2 = BinaryPrimitives.ReadUInt16LittleEndian(array.AsSpan(num));
					ushort num3 = BinaryPrimitives.ReadUInt16LittleEndian(array.AsSpan(num + 2));
					ushort num4 = BinaryPrimitives.ReadUInt16LittleEndian(array.AsSpan(num + 4));
					ushort num5 = BinaryPrimitives.ReadUInt16LittleEndian(array.AsSpan(num + 6));
					version = $"{num3}.{num2}.{num5}.{num4}";
					return true;
				}
			}
		}
		version = null;
		return false;
	}

	public static bool IsClientVersionValid(string versionText, out ClientVersion version)
	{
		version = (ClientVersion)0;
		if (!string.IsNullOrEmpty(versionText))
		{
			versionText = versionText.ToLower();
			string[] array = versionText.ToLower().Split('.');
			if (array.Length <= 2 || array.Length > 4)
			{
				return false;
			}
			if (int.TryParse(array[0], out var result) && result >= 0 && result <= 255)
			{
				int num = 0;
				if (int.TryParse(array[1], out var result2) && result2 >= 0 && result2 <= 255)
				{
					int num2 = 2;
					int result3 = 0;
					if (array.Length == 4)
					{
						if (!int.TryParse(array[num2], out result3) || result3 < 0 || result3 > 255)
						{
							return false;
						}
						num2++;
					}
					int i;
					for (i = 0; i < array[num2].Length; i++)
					{
						char c = array[num2][i];
						if (char.IsLetter(c))
						{
							num = (byte)c;
							break;
						}
					}
					if (num != 0)
					{
						if (array[num2].Length - i > 1)
						{
							return false;
						}
					}
					else if (i <= 0)
					{
						return false;
					}
					if (!int.TryParse(array[num2].Substring(0, i), out var result4) || result4 < 0 || result4 > 255)
					{
						return false;
					}
					if (num != 0)
					{
						char c2 = 'a';
						int num3 = 0;
						while (c2 != num && c2 <= 'z')
						{
							c2 = (char)(c2 + 1);
							num3++;
						}
						num = num3;
					}
					if (num2 == 2)
					{
						result3 = result4;
						result4 = num;
					}
					version = (ClientVersion)(((result & 0xFF) << 24) | ((result2 & 0xFF) << 16) | ((result3 & 0xFF) << 8) | (result4 & 0xFF));
					return true;
				}
			}
		}
		return false;
	}
}
