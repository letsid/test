using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ClassicUO.Utility.Logging;

namespace ClassicUO.IO;

internal class DefReader : IDisposable
{
	private const char COMMENT = '#';

	private static readonly char[] _tokens = new char[2] { '\t', ' ' };

	private static readonly char[] _tokensGroup = new char[4] { ',', ' ', '{', '}' };

	private readonly string _file;

	private List<string[]> _groups = new List<string[]>();

	private readonly int _minSize;

	private List<string[]> _parts = new List<string[]>();

	private StreamReader _reader;

	public int Line { get; private set; }

	public int Position { get; private set; }

	public int LinesCount => _parts.Count;

	public int PartsCount => _parts[Line].Length;

	private bool IsEOF => Line + 1 >= LinesCount;

	public DefReader(string file, int minsize = 2)
	{
		_file = file;
		_reader = new StreamReader(File.OpenRead(file));
		Line = -1;
		Position = 0;
		_minSize = minsize;
		Parse();
	}

	public void Dispose()
	{
		if (_reader != null)
		{
			_reader.Dispose();
			_reader = null;
			_parts = null;
			_groups = null;
		}
	}

	public bool Next()
	{
		if (!IsEOF)
		{
			Line++;
			Position = 0;
			return true;
		}
		return false;
	}

	private void Parse()
	{
		if (_parts.Count > 0)
		{
			_parts.Clear();
		}
		if (_groups.Count > 0)
		{
			_groups.Clear();
		}
		string text;
		while ((text = _reader.ReadLine()) != null)
		{
			text = text.Trim();
			if (text.Length > 0 && text[0] != '#' && char.IsNumber(text[0]))
			{
				int num = text.IndexOf('#');
				if (num >= 0)
				{
					text = text.Substring(0, num);
				}
				int num2 = text.IndexOf('{');
				int num3 = text.IndexOf('}');
				string[] array;
				if (num2 >= 0 && num3 >= 0)
				{
					string[] first = text.Substring(0, num2).Split(_tokens, StringSplitOptions.RemoveEmptyEntries);
					string text2 = text.Substring(num2, num3 - num2 + 1);
					array = Enumerable.Concat(second: text.Substring(num3 + 1, text.Length - num3 - 1).Split(_tokens, StringSplitOptions.RemoveEmptyEntries), first: first.Concat(new string[1] { text2 })).ToArray();
				}
				else
				{
					array = text.Split(_tokens, StringSplitOptions.RemoveEmptyEntries);
				}
				if (array.Length >= _minSize)
				{
					_parts.Add(array);
				}
			}
		}
	}

	private string[] GetTokensAtLine(int line)
	{
		if (line >= _parts.Count || line < 0)
		{
			Log.Error($"Index out of range [Line: {line}]. Returned '0'");
			return new string[1] { "0" };
		}
		return _parts[line];
	}

	private string TokenAt(int line, int index)
	{
		string[] tokensAtLine = GetTokensAtLine(line);
		if (index >= tokensAtLine.Length || index < 0)
		{
			Log.Error($"Index out of range [Line: {line}]. Returned '0'");
			return "0";
		}
		return tokensAtLine[index];
	}

	public int ReadInt()
	{
		return ReadInt(Line, Position++);
	}

	public int ReadGroupInt(int index = 0)
	{
		if (!TryReadGroup(TokenAt(Line, Position++), out var group))
		{
			throw new Exception("It's not a group");
		}
		if (index >= group.Length)
		{
			throw new IndexOutOfRangeException();
		}
		SanitizeStringNumber(ref group[index]);
		return int.Parse(group[index]);
	}

	public int[] ReadGroup()
	{
		string text = TokenAt(Line, Position++);
		if (text.Length > 0 && text[0] == '{')
		{
			if (text[text.Length - 1] == '}')
			{
				List<int> list = new List<int>();
				string[] array = text.Split(_tokensGroup, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					if (!string.IsNullOrEmpty(array[i]) && char.IsNumber(array[i][0]))
					{
						NumberStyles style = NumberStyles.Any;
						if (array[i].Length > 1 && array[i][0] == '0' && array[i][1] == 'x')
						{
							style = NumberStyles.HexNumber;
						}
						if (int.TryParse(array[i], style, null, out var result))
						{
							list.Add(result);
						}
					}
				}
				return list.ToArray();
			}
			Log.Error($"Missing }} at line {Line + 1}, in '{_file}'");
		}
		return null;
	}

	private static bool TryReadGroup(string s, out string[] group)
	{
		if (s.Length > 0 && s[0] == '{' && s[s.Length - 1] == '}')
		{
			group = s.Split(_tokensGroup, StringSplitOptions.RemoveEmptyEntries);
			return true;
		}
		group = null;
		return false;
	}

	private int ReadInt(int line, int index)
	{
		string token = TokenAt(line, index);
		if (!string.IsNullOrEmpty(token))
		{
			SanitizeStringNumber(ref token);
			if (!token.StartsWith("0x"))
			{
				return int.Parse(token);
			}
			return int.Parse(token.Remove(0, 2), NumberStyles.HexNumber);
		}
		return -1;
	}

	private static void SanitizeStringNumber(ref string token)
	{
		if (string.IsNullOrEmpty(token))
		{
			return;
		}
		int i = 0;
		if (token.StartsWith("0x"))
		{
			return;
		}
		for (; i < token.Length; i++)
		{
			char c = token[i];
			if (!char.IsNumber(c) && c != '-' && c != '+')
			{
				token = token.Substring(0, i);
				break;
			}
		}
	}
}
