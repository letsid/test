using System;
using System.Collections.Generic;
using System.Text;

namespace ClassicUO.Utility;

internal class TextFileParser
{
	private readonly char[] _delimiters;

	private readonly char[] _comments;

	private readonly char[] _quotes;

	private int _eol;

	private int _pos;

	private readonly StringBuilder _sb = new StringBuilder();

	private int _Size;

	private string _string;

	private bool _trim;

	public TextFileParser(string str, char[] delimiters, char[] comments, char[] quotes)
	{
		_delimiters = delimiters;
		_comments = comments;
		_quotes = quotes;
		_Size = str.Length;
		_string = str;
	}

	internal void Restart()
	{
		_pos = 0;
	}

	internal bool IsDelimiter()
	{
		bool flag = false;
		for (int i = 0; i < _delimiters.Length; i++)
		{
			if (flag)
			{
				break;
			}
			flag = _string[_pos] == _delimiters[i];
		}
		return flag;
	}

	internal bool IsEOF()
	{
		return _pos >= _Size;
	}

	private void GetEOL()
	{
		for (int i = _pos; i < _Size; i++)
		{
			if (_string[i] == '\n' || i + 1 >= _Size)
			{
				_eol = i;
				break;
			}
		}
	}

	private void SkipToData()
	{
		while (_pos < _eol && IsDelimiter())
		{
			_pos++;
		}
	}

	private bool IsComment()
	{
		bool flag = _string[_pos] == '\n';
		for (int i = 0; i < _comments.Length; i++)
		{
			if (flag)
			{
				break;
			}
			flag = _string[_pos] == _comments[i];
			if (flag && i + 1 < _comments.Length && _comments[i] == _comments[i + 1] && _pos + 1 < _eol)
			{
				flag = _string[_pos] == _string[_pos + 1];
				i++;
			}
		}
		return flag;
	}

	private bool IsQuote()
	{
		bool flag = _string[_pos] == '\n';
		for (int i = 0; i < _quotes.Length; i += 2)
		{
			if (flag)
			{
				break;
			}
			if (_string[_pos] == _quotes[i] || (i + 1 < _quotes.Length && _string[_pos] == _quotes[i + 1]))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	private bool IsSecondQuote()
	{
		bool flag = _string[_pos] == '\n';
		for (int i = 0; i + 1 < _quotes.Length; i += 2)
		{
			if (flag)
			{
				break;
			}
			if (_string[_pos] == _quotes[i + 1])
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	private void ObtainData()
	{
		while (_pos < _Size && _string[_pos] != '\n' && !IsDelimiter())
		{
			if (IsComment())
			{
				_pos = _eol;
				break;
			}
			if (_string[_pos] != '\r' && (!_trim || (_string[_pos] != ' ' && _string[_pos] != '\t')))
			{
				for (int i = 0; i < _quotes.Length; i++)
				{
					if (_string[_pos] == _quotes[i])
					{
						return;
					}
				}
				_sb.Append(_string[_pos]);
			}
			_pos++;
		}
	}

	private unsafe void ObtainQuotedData(bool save = true)
	{
		bool flag = false;
		for (int i = 0; i < _quotes.Length; i += 2)
		{
			if (_string[_pos] != _quotes[i])
			{
				continue;
			}
			char c = _quotes[i + 1];
			flag = true;
			int j = _pos + 1;
			int num = j;
			for (; j < _eol && _string[j] != '\n' && _string[j] != c; j++)
			{
				if (_string[j] == _quotes[i])
				{
					_pos = j;
					ObtainQuotedData(save: false);
					j = _pos;
				}
			}
			_pos++;
			int num2 = j - num;
			if (num2 <= 0)
			{
				break;
			}
			if (save)
			{
				ReadOnlySpan<char> span = _string.AsSpan(num, num2);
				int num3 = span.IndexOf('\r');
				int num4 = span.IndexOf('\n');
				if (num3 >= 0)
				{
					span = span.Slice(num, num3);
				}
				else if (num4 >= 0)
				{
					span = span.Slice(num, num4);
				}
				fixed (char* value = span)
				{
					_sb.Append(value, span.Length);
				}
			}
			_pos = j;
			if (_pos < _eol && _string[_pos] == c)
			{
				_pos++;
			}
			break;
		}
		if (!flag)
		{
			ObtainData();
		}
	}

	internal List<string> ReadTokens(bool trim = true)
	{
		_trim = trim;
		List<string> list = new List<string>();
		if (_pos < _Size)
		{
			GetEOL();
			while (_pos < _eol)
			{
				SkipToData();
				if (_pos >= _eol || IsComment())
				{
					break;
				}
				ObtainQuotedData();
				if (_sb.Length > 0)
				{
					list.Add(_sb.ToString());
					_sb.Clear();
				}
				else if (IsSecondQuote())
				{
					_pos++;
				}
			}
			_pos = _eol + 1;
		}
		return list;
	}

	internal List<string> GetTokens(string str, bool trim = true)
	{
		_trim = trim;
		List<string> list = new List<string>();
		_pos = 0;
		_string = str;
		_eol = (_Size = str.Length);
		while (_pos < _eol)
		{
			SkipToData();
			if (_pos >= _eol || IsComment())
			{
				break;
			}
			ObtainQuotedData();
			if (_sb.Length > 0)
			{
				list.Add(_sb.ToString());
				_sb.Clear();
			}
		}
		return list;
	}
}
