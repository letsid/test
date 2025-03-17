using System;
using System.Text;

namespace ClassicUO.Utility;

internal static class Crypter
{
	public static string Encrypt(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(source);
		int num = 0;
		string text = CalculateKey();
		if (text == string.Empty)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(source.Length * 2 + 2);
		stringBuilder.Append("1+");
		for (int i = 0; i < bytes.Length; i++)
		{
			stringBuilder.AppendFormat("{0:X2}", (byte)(bytes[i] ^ (byte)text[num++]));
			if (num >= text.Length)
			{
				num = 0;
			}
		}
		return stringBuilder.ToString();
	}

	public static string Decrypt(string source)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		byte[] array = null;
		if (source.Length > 2 && source[0] == '1' && source[1] == '+')
		{
			array = new byte[source.Length - 2 >> 1];
			string text = CalculateKey();
			if (text == string.Empty)
			{
				return string.Empty;
			}
			int num = 0;
			for (int i = 2; i < source.Length; i += 2)
			{
				byte b;
				try
				{
					b = Convert.ToByte(source.Substring(i, 2), 16);
				}
				catch
				{
					continue;
				}
				array[i - 2 >> 1] = (byte)(b ^ (byte)text[num++]);
				if (num >= text.Length)
				{
					num = 0;
				}
			}
		}
		else
		{
			byte b2 = (byte)(source.Length >> 1);
			array = new byte[b2];
			for (int j = 0; j < source.Length; j += 2)
			{
				byte b3;
				try
				{
					b3 = Convert.ToByte(source.Substring(j, 2), 16);
				}
				catch
				{
					continue;
				}
				array[j >> 1] = (byte)(b3 ^ b2++);
			}
		}
		return Encoding.ASCII.GetString(array);
	}

	private static string CalculateKey()
	{
		return Environment.MachineName;
	}
}
