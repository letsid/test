using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.IO.Resources;

internal class ClilocLoader : UOFileLoader
{
	private static ClilocLoader _instance;

	private string _cliloc;

	private readonly Dictionary<int, string> _entries = new Dictionary<int, string>();

	public static ClilocLoader Instance => _instance ?? (_instance = new ClilocLoader());

	private ClilocLoader()
	{
	}

	public Task Load(string lang)
	{
		if (string.IsNullOrEmpty(lang))
		{
			lang = "enu";
		}
		_cliloc = "Cliloc." + lang;
		Log.Trace("searching for: '" + _cliloc + "'");
		if (!File.Exists(UOFileManager.GetUOFilePath(_cliloc)))
		{
			Log.Warn("'" + _cliloc + "' not found. Rolled back to Cliloc.enu");
			_cliloc = "Cliloc.enu";
		}
		return Load();
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			if (string.IsNullOrEmpty(_cliloc))
			{
				_cliloc = "Cliloc.enu";
			}
			string uOFilePath = UOFileManager.GetUOFilePath(_cliloc);
			if (!File.Exists(uOFilePath))
			{
				Log.Error("cliloc not found: '" + uOFilePath + "'");
				return;
			}
			if (string.Compare(_cliloc, "cliloc.enu", StringComparison.InvariantCultureIgnoreCase) != 0)
			{
				using BinaryReader binaryReader = new BinaryReader(new FileStream(UOFileManager.GetUOFilePath("Cliloc.enu"), FileMode.Open, FileAccess.Read));
				binaryReader.ReadInt32();
				binaryReader.ReadInt16();
				byte[] array = ArrayPool<byte>.Shared.Rent(1024, zero: true);
				try
				{
					while (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
					{
						int key = binaryReader.ReadInt32();
						binaryReader.ReadByte();
						int num = binaryReader.ReadInt16();
						if (num > array.Length)
						{
							ArrayPool<byte>.Shared.Return(array);
							array = ArrayPool<byte>.Shared.Rent((num + 1023) & -1024, zero: true);
						}
						binaryReader.Read(array, 0, num);
						string value = string.Intern(Encoding.UTF8.GetString(array, 0, num));
						_entries[key] = value;
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return(array);
				}
			}
			using BinaryReader binaryReader2 = new BinaryReader(new FileStream(uOFilePath, FileMode.Open, FileAccess.Read));
			binaryReader2.ReadInt32();
			binaryReader2.ReadInt16();
			byte[] array2 = ArrayPool<byte>.Shared.Rent(1024, zero: true);
			try
			{
				while (binaryReader2.BaseStream.Length != binaryReader2.BaseStream.Position)
				{
					int key2 = binaryReader2.ReadInt32();
					binaryReader2.ReadByte();
					int num2 = binaryReader2.ReadInt16();
					if (num2 > array2.Length)
					{
						ArrayPool<byte>.Shared.Return(array2);
						array2 = ArrayPool<byte>.Shared.Rent((num2 + 1023) & -1024, zero: true);
					}
					binaryReader2.Read(array2, 0, num2);
					string value2 = string.Intern(Encoding.UTF8.GetString(array2, 0, num2));
					_entries[key2] = value2;
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(array2);
			}
		});
	}

	public override void ClearResources()
	{
		_entries.Clear();
	}

	public string GetString(int number)
	{
		_entries.TryGetValue(number, out var value);
		return value;
	}

	public string GetString(int number, string replace)
	{
		string text = GetString(number);
		if (string.IsNullOrEmpty(text))
		{
			text = replace;
		}
		return text;
	}

	public string GetString(int number, bool camelcase, string replace = "")
	{
		string text = GetString(number);
		if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(replace))
		{
			text = replace;
		}
		if (camelcase && !string.IsNullOrEmpty(text))
		{
			text = StringHelper.CapitalizeAllWords(text);
		}
		return text;
	}

	public unsafe string Translate(int clilocNum, string arg = "", bool capitalize = false)
	{
		string @string = GetString(clilocNum);
		if (@string == null)
		{
			return null;
		}
		if (arg == null)
		{
			arg = "";
		}
		ReadOnlySpan<char> readOnlySpan = arg.AsSpan();
		int i = 0;
		int num = 0;
		int num2 = -1;
		for (; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i] != '\t')
			{
				if (num2 == -1)
				{
					num2 = i;
				}
			}
			else if (num2 >= 0)
			{
				num++;
			}
		}
		if (num2 == -1)
		{
			num2 = 0;
		}
		Point* ptr = stackalloc Point[++num];
		i = num2;
		int num3 = 0;
		for (; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i] == '\t')
			{
				ptr[num3].X = num2;
				ptr[num3].Y = i;
				num2 = i + 1;
				num3++;
			}
		}
		bool flag = num - 1 > 0;
		ptr[num - 1].X = num2;
		ptr[num - 1].Y = i;
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(@string.AsSpan());
		int num4 = 0;
		while (num4 < valueStringBuilder.Length)
		{
			int num5 = num4;
			Span<char> span = valueStringBuilder.RawChars;
			num4 = span.Slice(num4, valueStringBuilder.Length - num4).IndexOf('~');
			if (num4 == -1)
			{
				break;
			}
			num4 += num5;
			span = valueStringBuilder.RawChars;
			int num6 = span.Slice(num4 + 1, valueStringBuilder.Length - (num4 + 1)).IndexOf('~');
			if (num6 == -1)
			{
				break;
			}
			num6 += num4 + 1;
			span = valueStringBuilder.RawChars;
			int num7 = span.Slice(num4 + 1, num6 - (num4 + 1)).IndexOf('_');
			num7 = ((num7 != -1) ? (num7 + (num4 + 1)) : num6);
			int num8 = num4 + 1;
			int num9 = num7 - num8;
			int j;
			for (j = 0; j < num9; j++)
			{
				span = valueStringBuilder.RawChars;
				if (!char.IsNumber(span[num8 + j]))
				{
					break;
				}
			}
			span = valueStringBuilder.RawChars;
			span = span.Slice(num8, j);
			if (!int.TryParse(span.ToString(), out num7))
			{
				return $"MegaCliloc: error for {clilocNum}";
			}
			num7--;
			ReadOnlySpan<char> readOnlySpan2;
			ReadOnlySpan<char> readOnlySpan3;
			if (num7 >= 0 && num7 < num)
			{
				readOnlySpan2 = arg.AsSpan();
				readOnlySpan3 = readOnlySpan2.Slice(ptr[num7].X, ptr[num7].Y - ptr[num7].X);
			}
			else
			{
				readOnlySpan3 = string.Empty.AsSpan();
			}
			ReadOnlySpan<char> s = readOnlySpan3;
			if (s.Length > 1)
			{
				int result2;
				string value;
				if (s[0] == '#')
				{
					readOnlySpan2 = s.Slice(1);
					if (int.TryParse(readOnlySpan2.ToString(), out var result))
					{
						string string2 = GetString(result);
						s = ((!string.IsNullOrEmpty(string2)) ? string2.AsSpan() : string.Empty.AsSpan());
					}
				}
				else if (flag && int.TryParse(s.ToString(), out result2) && _entries.TryGetValue(result2, out value) && !string.IsNullOrEmpty(value))
				{
					s = value.AsSpan();
				}
			}
			valueStringBuilder.Remove(num4, num6 - num4 + 1);
			if (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"')
			{
				s = s.Slice(1, s.Length - 2);
			}
			valueStringBuilder.Insert(num4, s);
			if (num7 >= 0 && num7 < num)
			{
				num4 += s.Length;
			}
		}
		@string = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		if (capitalize)
		{
			@string = StringHelper.CapitalizeAllWords(@string);
		}
		return @string;
	}
}
