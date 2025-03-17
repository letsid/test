using System;
using System.Runtime.CompilerServices;
using System.Text;
using SDL2;

namespace ClassicUO.Utility;

internal static class StringHelper
{
	private static readonly char[] _dots = new char[4] { '.', ',', ';', '!' };

	private static Encoding _cp1252Encoding;

	public static Encoding Cp1252Encoding
	{
		get
		{
			if (_cp1252Encoding == null)
			{
				_cp1252Encoding = Encoding.ASCII;
			}
			return _cp1252Encoding;
		}
	}

	public static string CapitalizeFirstCharacter(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		if (str.Length == 1)
		{
			return char.ToUpper(str[0]).ToString();
		}
		return char.ToUpper(str[0]) + str.Substring(1);
	}

	public static string CapitalizeAllWords(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		Span<char> initialBuffer = stackalloc char[str.Length];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		bool flag = true;
		for (int i = 0; i < str.Length; i++)
		{
			valueStringBuilder.Append(flag ? char.ToUpper(str[i]) : str[i]);
			if (!char.IsWhiteSpace(str[i]))
			{
				flag = i + 1 < str.Length && char.IsWhiteSpace(str[i + 1]);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public static string CapitalizeWordsByLimitator(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		Span<char> initialBuffer = stackalloc char[str.Length];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		bool flag = true;
		for (int i = 0; i < str.Length; i++)
		{
			valueStringBuilder.Append(flag ? char.ToUpper(str[i]) : str[i]);
			flag = false;
			for (int j = 0; j < _dots.Length; j++)
			{
				if (str[i] == _dots[j])
				{
					flag = true;
					break;
				}
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSafeChar(int c)
	{
		if (c >= 32)
		{
			return c < 65534;
		}
		return false;
	}

	public static void AddSpaceBeforeCapital(string[] str, bool checkAcronyms = true)
	{
		for (int i = 0; i < str.Length; i++)
		{
			str[i] = AddSpaceBeforeCapital(str[i], checkAcronyms);
		}
	}

	public static string AddSpaceBeforeCapital(string str, bool checkAcronyms = true)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return "";
		}
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(str.Length * 2);
		valueStringBuilder.Append(str[0]);
		int i = 1;
		for (int num = str.Length - 1; i <= num; i++)
		{
			if (char.IsUpper(str[i]) && ((str[i - 1] != ' ' && !char.IsUpper(str[i - 1])) || (checkAcronyms && char.IsUpper(str[i - 1]) && i < num && !char.IsUpper(str[i + 1]))))
			{
				valueStringBuilder.Append(' ');
			}
			valueStringBuilder.Append(str[i]);
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public static string RemoveUpperLowerChars(string str, bool removelower = true)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			return "";
		}
		Span<char> initialBuffer = stackalloc char[str.Length];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		for (int i = 0; i < str.Length; i++)
		{
			if (char.IsUpper(str[i]) == removelower || str[i] == ' ')
			{
				valueStringBuilder.Append(str[i]);
			}
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public static string IntToAbbreviatedString(int num)
	{
		if (num > 999999)
		{
			return $"{num / 1000000}M+";
		}
		if (num > 999)
		{
			return $"{num / 1000}K+";
		}
		return num.ToString();
	}

	public static string GetClipboardText(bool multiline)
	{
		if (SDL.SDL_HasClipboardText() != 0)
		{
			string text = (multiline ? SDL.SDL_GetClipboardText() : (SDL.SDL_GetClipboardText()?.Replace('\n', ' ') ?? null));
			if (!string.IsNullOrEmpty(text))
			{
				if (text.IndexOf('\r') >= 0)
				{
					text = text.Replace("\r", "");
				}
				if (text.IndexOf('\t') >= 0)
				{
					return text.Replace("\t", "   ");
				}
				return text;
			}
		}
		return null;
	}

	public static string GetPluralAdjustedString(string str, bool plural = false)
	{
		if (str.Contains("%"))
		{
			string[] array = str.Split(new char[1] { '%' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length < 2)
			{
				return str;
			}
			Span<char> initialBuffer = stackalloc char[str.Length];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			valueStringBuilder.Append(array[0]);
			if (array[1].Contains("/"))
			{
				string[] array2 = array[1].Split('/');
				if (plural)
				{
					valueStringBuilder.Append(array2[0]);
				}
				else if (array2.Length > 1)
				{
					valueStringBuilder.Append(array2[1]);
				}
			}
			else if (plural)
			{
				valueStringBuilder.Append(array[1]);
			}
			if (array.Length == 3)
			{
				valueStringBuilder.Append(array[2]);
			}
			string result = valueStringBuilder.ToString();
			valueStringBuilder.Dispose();
			return result;
		}
		return str;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static bool UnsafeCompare(char* buffer, string str, int length)
	{
		for (int i = 0; i < length && i < str.Length; i++)
		{
			if (buffer[i] != str[i])
			{
				return false;
			}
		}
		return true;
	}
}
