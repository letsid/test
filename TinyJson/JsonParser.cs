using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TinyJson;

public class JsonParser : IDisposable
{
	private enum Token
	{
		None,
		CurlyOpen,
		CurlyClose,
		SquareOpen,
		SquareClose,
		Colon,
		Comma,
		String,
		Number,
		BoolOrNull
	}

	private StringReader json;

	private readonly StringBuilder sb = new StringBuilder();

	internal JsonParser(string jsonString)
	{
		json = new StringReader(jsonString);
	}

	public void Dispose()
	{
		json.Dispose();
		json = null;
	}

	public static object ParseValue(string jsonString)
	{
		using JsonParser jsonParser = new JsonParser(jsonString);
		return jsonParser.ParseValue();
	}

	private bool EndReached()
	{
		return json.Peek() == -1;
	}

	private bool PeekWordbreak()
	{
		char c = PeekChar();
		if (c != ' ' && c != ',' && c != ':' && c != '"' && c != '{' && c != '}' && c != '[' && c != ']' && c != '\t' && c != '\n')
		{
			return c == '\r';
		}
		return true;
	}

	private bool PeekWhitespace()
	{
		char c = PeekChar();
		if (c != ' ' && c != '\t' && c != '\n')
		{
			return c == '\r';
		}
		return true;
	}

	private char PeekChar()
	{
		return Convert.ToChar(json.Peek());
	}

	private char ReadChar()
	{
		return Convert.ToChar(json.Read());
	}

	private string ReadWord()
	{
		sb.Clear();
		while (!PeekWordbreak() && !EndReached())
		{
			sb.Append(ReadChar());
		}
		if (!EndReached())
		{
			return sb.ToString();
		}
		return null;
	}

	private void EatWhitespace()
	{
		while (PeekWhitespace())
		{
			json.Read();
		}
	}

	private Token PeekToken()
	{
		EatWhitespace();
		if (EndReached())
		{
			return Token.None;
		}
		switch (PeekChar())
		{
		case '{':
			return Token.CurlyOpen;
		case '}':
			return Token.CurlyClose;
		case '[':
			return Token.SquareOpen;
		case ']':
			return Token.SquareClose;
		case ',':
			return Token.Comma;
		case '"':
			return Token.String;
		case ':':
			return Token.Colon;
		case '-':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return Token.Number;
		case 'f':
		case 'n':
		case 't':
			return Token.BoolOrNull;
		default:
			return Token.None;
		}
	}

	private object ParseBoolOrNull()
	{
		if (PeekToken() == Token.BoolOrNull)
		{
			string text = ReadWord();
			switch (text)
			{
			case "true":
				return true;
			case "false":
				return false;
			case "null":
				return null;
			default:
				Console.WriteLine("Unexpected bool value: " + text);
				return null;
			}
		}
		Console.WriteLine("Unexpected bool token: " + PeekToken());
		return null;
	}

	private object ParseNumber()
	{
		if (PeekToken() == Token.Number)
		{
			string text = ReadWord();
			long result2;
			if (text.Contains("."))
			{
				if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
				{
					return result;
				}
			}
			else if (long.TryParse(text, out result2))
			{
				return result2;
			}
			Console.WriteLine("Unexpected number value: " + text);
			return null;
		}
		Console.WriteLine("Unexpected number token: " + PeekToken());
		return null;
	}

	private string ParseString()
	{
		if (PeekToken() == Token.String)
		{
			ReadChar();
			sb.Clear();
			while (!EndReached())
			{
				char c = ReadChar();
				switch (c)
				{
				case '"':
					return sb.ToString();
				case '\\':
					if (EndReached())
					{
						return null;
					}
					c = ReadChar();
					switch (c)
					{
					case '"':
					case '/':
					case '\\':
						sb.Append(c);
						break;
					case 'b':
						sb.Append('\b');
						break;
					case 'f':
						sb.Append('\f');
						break;
					case 'n':
						sb.Append('\n');
						break;
					case 'r':
						sb.Append('\r');
						break;
					case 't':
						sb.Append('\t');
						break;
					case 'u':
					{
						string value = string.Concat(ReadChar(), ReadChar(), ReadChar(), ReadChar());
						sb.Append((char)Convert.ToInt32(value, 16));
						break;
					}
					}
					break;
				default:
					sb.Append(c);
					break;
				}
			}
			return null;
		}
		Console.WriteLine("Unexpected string token: " + PeekToken());
		return null;
	}

	private Dictionary<string, object> ParseObject()
	{
		if (PeekToken() == Token.CurlyOpen)
		{
			json.Read();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			while (true)
			{
				switch (PeekToken())
				{
				case Token.None:
					return null;
				case Token.Comma:
					json.Read();
					continue;
				case Token.CurlyClose:
					json.Read();
					return dictionary;
				}
				string text = ParseString();
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				if (PeekToken() != Token.Colon)
				{
					return null;
				}
				json.Read();
				dictionary[text] = ParseValue();
			}
		}
		Console.WriteLine("Unexpected object token: " + PeekToken());
		return null;
	}

	private List<object> ParseArray()
	{
		if (PeekToken() == Token.SquareOpen)
		{
			json.Read();
			List<object> list = new List<object>();
			while (true)
			{
				switch (PeekToken())
				{
				case Token.None:
					return null;
				case Token.Comma:
					json.Read();
					break;
				case Token.SquareClose:
					json.Read();
					return list;
				default:
					list.Add(ParseValue());
					break;
				}
			}
		}
		Console.WriteLine("Unexpected array token: " + PeekToken());
		return null;
	}

	private object ParseValue()
	{
		switch (PeekToken())
		{
		case Token.String:
			return ParseString();
		case Token.Number:
			return ParseNumber();
		case Token.BoolOrNull:
			return ParseBoolOrNull();
		case Token.CurlyOpen:
			return ParseObject();
		case Token.SquareOpen:
			return ParseArray();
		default:
			Console.WriteLine("Unexpected value token: " + PeekToken());
			return null;
		}
	}
}
