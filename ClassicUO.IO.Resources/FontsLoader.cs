using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Collections;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.IO.Resources;

internal class FontsLoader : UOFileLoader
{
	private struct HtmlStatus
	{
		public uint BackgroundColor;

		public uint VisitedWebLinkColor;

		public uint WebLinkColor;

		public uint Color;

		public Rectangle Margins;

		public bool IsHtmlBackgroundColored;
	}

	private const int UOFONT_SOLID = 1;

	private const int UOFONT_ITALIC = 2;

	private const int UOFONT_INDENTION = 4;

	private const int UOFONT_BLACK_BORDER = 8;

	private const int UOFONT_UNDERLINE = 16;

	private const int UOFONT_FIXED = 32;

	private const int UOFONT_CROPPED = 64;

	private const int UOFONT_BQ = 128;

	private const int UOFONT_EXTRAHEIGHT = 256;

	private const int UOFONT_CROPTEXTURE = 512;

	private const int UOFONT_FIXEDHEIGHT = 1024;

	private const int UNICODE_SPACE_WIDTH = 8;

	private const int MAX_HTML_TEXT_HEIGHT = 18;

	private const byte NOPRINT_CHARS = 32;

	private const float ITALIC_FONT_KOEFFICIENT = 3.3f;

	private static FontsLoader _instance;

	private HtmlStatus _htmlStatus;

	private FontCharacterData[,] _fontData;

	private readonly IntPtr[] _unicodeFontAddress = new IntPtr[20];

	private readonly long[] _unicodeFontSize = new long[20];

	private readonly Dictionary<ushort, WebLink> _webLinks = new Dictionary<ushort, WebLink>();

	private readonly int[] _offsetCharTable = new int[10] { 2, 0, 2, 2, 0, 0, 2, 2, 0, 0 };

	private readonly int[] _offsetSymbolTable = new int[10] { 1, 0, 1, 1, -1, 0, 1, 1, 0, 0 };

	public static FontsLoader Instance => _instance ?? (_instance = new FontsLoader());

	public int FontCount { get; private set; }

	public bool UnusePartialHue { get; set; }

	public bool RecalculateWidthByInfo { get; set; }

	public bool IsUsingHTML { get; set; }

	private FontsLoader()
	{
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			UOFileMul uOFileMul = new UOFileMul(UOFileManager.GetUOFilePath("fonts.mul"));
			UOFileMul[] array = new UOFileMul[20];
			for (int i = 0; i < 20; i++)
			{
				string uOFilePath = UOFileManager.GetUOFilePath("unifont" + ((i == 0) ? "" : i.ToString()) + ".mul");
				if (File.Exists(uOFilePath))
				{
					array[i] = new UOFileMul(uOFilePath);
					_unicodeFontAddress[i] = array[i].StartAddress;
					_unicodeFontSize[i] = array[i].Length;
				}
			}
			int num = sizeof(FontHeader);
			FontCount = 0;
			while (uOFileMul.Position < uOFileMul.Length)
			{
				bool flag = false;
				uOFileMul.Skip(1);
				for (int j = 0; j < 224; j++)
				{
					FontHeader* ptr = (FontHeader*)(void*)uOFileMul.PositionAddress;
					if (uOFileMul.Position + num < uOFileMul.Length)
					{
						uOFileMul.Skip(num);
						int num2 = ptr->Width * ptr->Height * 2;
						if (uOFileMul.Position + num2 > uOFileMul.Length)
						{
							flag = true;
							break;
						}
						uOFileMul.Skip(num2);
					}
				}
				if (flag)
				{
					break;
				}
				FontCount++;
			}
			if (FontCount < 1)
			{
				FontCount = 0;
			}
			else
			{
				_fontData = new FontCharacterData[FontCount, 224];
				uOFileMul.Seek(0);
				for (int k = 0; k < FontCount; k++)
				{
					uOFileMul.ReadByte();
					for (int l = 0; l < 224; l++)
					{
						if (uOFileMul.Position + 3 < uOFileMul.Length)
						{
							byte b = uOFileMul.ReadByte();
							byte b2 = uOFileMul.ReadByte();
							uOFileMul.Skip(1);
							_fontData[k, l] = new FontCharacterData(b, b2, (ushort*)(void*)uOFileMul.PositionAddress);
							uOFileMul.Skip(b * b2 * 2);
						}
					}
				}
				if (_unicodeFontAddress[1] == IntPtr.Zero)
				{
					_unicodeFontAddress[1] = _unicodeFontAddress[0];
					_unicodeFontSize[1] = _unicodeFontSize[0];
				}
			}
		});
	}

	public bool UnicodeFontExists(byte font)
	{
		if (font < 20)
		{
			return _unicodeFontAddress[font] != IntPtr.Zero;
		}
		return false;
	}

	public (int, int) MeasureText(string text, byte font, bool isunicode, TEXT_ALIGN_TYPE align, ushort flags, int maxWidth = 200)
	{
		int num;
		int item;
		if (isunicode)
		{
			num = GetWidthUnicode(font, text.AsSpan());
			if (num > maxWidth)
			{
				num = GetWidthExUnicode(font, text, maxWidth, align, flags);
			}
			item = GetHeightUnicode(font, text, num, align, flags);
		}
		else
		{
			num = GetWidthASCII(font, text);
			if (num > maxWidth)
			{
				num = GetWidthExASCII(font, text, maxWidth, align, flags);
			}
			item = GetHeightASCII(font, text, num, align, flags);
		}
		return (num, item);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int GetASCIIIndex(char c)
	{
		byte b = (byte)c;
		if (b < 32)
		{
			return 0;
		}
		return b - 32;
	}

	public int GetWidthASCII(byte font, string str)
	{
		if (font >= FontCount || string.IsNullOrEmpty(str))
		{
			return 0;
		}
		int num = 0;
		foreach (char c in str)
		{
			num += _fontData[font, GetASCIIIndex(c)].Width;
		}
		return num;
	}

	public int GetCharWidthASCII(byte font, char c)
	{
		if (font >= FontCount || c == '\0' || c == '\r')
		{
			return 0;
		}
		if (c < ' ')
		{
			return _fontData[font, 0].Width;
		}
		int num = c - 32;
		if (num < _fontData.GetLength(1))
		{
			return _fontData[font, num].Width;
		}
		return 0;
	}

	public int GetWidthExASCII(byte font, string text, int maxwidth, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font > FontCount || string.IsNullOrEmpty(text))
		{
			return 0;
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoASCII(font, text, text.Length, align, flags, maxwidth);
		int num = 0;
		while (multilinesFontInfo != null)
		{
			if (multilinesFontInfo.Width > num)
			{
				num = multilinesFontInfo.Width;
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
			multilinesFontInfo2 = null;
		}
		return num;
	}

	private int GetHeightASCII(MultilinesFontInfo info)
	{
		int num = 0;
		while (info != null)
		{
			num += info.MaxHeight;
			info = info.Next;
		}
		return num;
	}

	public int GetHeightASCII(byte font, string str, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (width == 0)
		{
			width = GetWidthASCII(font, str);
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoASCII(font, str, str.Length, align, flags, width);
		int num = 0;
		while (multilinesFontInfo != null)
		{
			num = ((!IsUsingHTML) ? (num + multilinesFontInfo.MaxHeight) : (num + 18));
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		return num;
	}

	public void GenerateASCII(RenderedText renderedText, byte font, string str, ushort color, int width, TEXT_ALIGN_TYPE align, ushort flags, bool saveHitmap, int height, PixelPicker picker)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		if ((flags & 0x20) != 0 || (flags & 0x40) != 0 || (flags & 0x200) != 0)
		{
			if (width == 0 || string.IsNullOrEmpty(str))
			{
				return;
			}
			if (GetWidthASCII(font, str) > width)
			{
				string text = GetTextByWidthASCII(font, str, width, (flags & 0x40) != 0, align, flags);
				if ((flags & 0x200) != 0 && !string.IsNullOrEmpty(text))
				{
					int num = 0;
					while (num < height)
					{
						num += GetHeightASCII(font, text, width, align, flags);
						if (str.Length <= text.Length)
						{
							break;
						}
						text += GetTextByWidthASCII(font, str.Substring(text.Length), width, (flags & 0x40) != 0, align, flags);
					}
				}
				GeneratePixelsASCII(renderedText, font, text, color, width, align, flags, saveHitmap, picker, (flags & 0x800) != 0);
				return;
			}
		}
		GeneratePixelsASCII(renderedText, font, str, color, width, align, flags, saveHitmap, picker, (flags & 0x800) != 0);
	}

	public unsafe string GetTextByWidthASCII(byte font, string str, int width, bool isCropped, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font >= FontCount || string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		int len = str.Length;
		Span<char> initialBuffer = stackalloc char[len];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		if (IsUsingHTML)
		{
			HTMLChar* data = stackalloc HTMLChar[len];
			GetHTMLData(data, font, str.AsSpan(), ref len, align, flags);
			int num = str.Length - len;
			if (num > 0)
			{
				valueStringBuilder.Append(str.Substring(0, num));
				str = str.Substring(str.Length - len, len);
				if (GetWidthASCII(font, str) < width)
				{
					isCropped = false;
				}
			}
		}
		if (isCropped)
		{
			width -= _fontData[font, 14].Width * 3;
		}
		int num2 = 0;
		string text = str;
		foreach (char c in text)
		{
			num2 += _fontData[font, GetASCIIIndex(c)].Width;
			if (num2 > width)
			{
				break;
			}
			valueStringBuilder.Append(c);
		}
		if (isCropped)
		{
			valueStringBuilder.Append("...");
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	private unsafe void GeneratePixelsASCII(RenderedText renderedText, byte font, string str, ushort color, int width, TEXT_ALIGN_TYPE align, ushort flags, bool saveHitmap, PixelPicker picker, bool noPartialHue = false)
	{
		if (font >= FontCount)
		{
			return;
		}
		int length = str.Length;
		if (length == 0)
		{
			return;
		}
		if (width <= 0)
		{
			width = GetWidthASCII(font, str);
		}
		if (width <= 0)
		{
			return;
		}
		MultilinesFontInfo infoASCII = GetInfoASCII(font, str, length, align, flags, width);
		if (infoASCII == null)
		{
			return;
		}
		width += 4;
		int heightASCII = GetHeightASCII(infoASCII);
		if (heightASCII <= 0)
		{
			MultilinesFontInfo multilinesFontInfo = infoASCII;
			while (multilinesFontInfo != null)
			{
				infoASCII = multilinesFontInfo;
				multilinesFontInfo = multilinesFontInfo.Next;
				infoASCII.Data.Clear();
				infoASCII.Data.Count = 0u;
				infoASCII = null;
			}
			return;
		}
		int length2 = heightASCII * width;
		uint[] array = ArrayPool<uint>.Shared.Rent(length2, zero: true);
		try
		{
			int num = 0;
			MultilinesFontInfo multilinesFontInfo2 = infoASCII;
			bool flag = font != 5 && font != 8 && !UnusePartialHue && !noPartialHue;
			int num2 = ((font == 6) ? 7 : 0);
			int num3 = 0;
			while (multilinesFontInfo2 != null)
			{
				infoASCII = multilinesFontInfo2;
				num3++;
				int num4 = 0;
				switch (multilinesFontInfo2.Align)
				{
				case TEXT_ALIGN_TYPE.TS_CENTER:
					num4 = width - multilinesFontInfo2.Width >> 1;
					if (num4 < 0)
					{
						num4 = 0;
					}
					break;
				case TEXT_ALIGN_TYPE.TS_RIGHT:
					num4 = width - 10 - multilinesFontInfo2.Width;
					if (num4 < 0)
					{
						num4 = width;
					}
					break;
				case TEXT_ALIGN_TYPE.TS_LEFT:
					if ((flags & 4) != 0)
					{
						num4 = multilinesFontInfo2.IndentionOffset;
					}
					break;
				}
				uint count = multilinesFontInfo2.Data.Count;
				for (int i = 0; i < count; i++)
				{
					byte index = (byte)multilinesFontInfo2.Data[i].Item;
					int fontOffsetY = GetFontOffsetY(font, index);
					ref FontCharacterData reference = ref _fontData[font, GetASCIIIndex(multilinesFontInfo2.Data[i].Item)];
					int width2 = reference.Width;
					int height = reference.Height;
					ushort color2 = color;
					for (int j = 0; j < height; j++)
					{
						int num5 = j + num + fontOffsetY;
						if (num5 >= heightASCII)
						{
							break;
						}
						for (int k = 0; k < width2 && k + num4 < width; k++)
						{
							ushort num6 = reference.Data[j * width2 + k];
							if (num6 != 0)
							{
								uint num7 = ((!flag) ? HuesLoader.Instance.GetColor(num6, color2) : HuesLoader.Instance.GetPartialHueColor(num6, color2));
								int num8 = num5 * width + k + num4;
								if (num8 >= 0)
								{
									array[num8] = num7 | 0xFF000000u;
								}
							}
						}
					}
					num4 += width2;
				}
				num += multilinesFontInfo2.MaxHeight - num2;
				multilinesFontInfo2 = multilinesFontInfo2.Next;
				infoASCII.Data.Clear();
				infoASCII.Data.Count = 0u;
				infoASCII = null;
			}
			if (renderedText.Texture == null || renderedText.Texture.IsDisposed)
			{
				renderedText.Texture = new Texture2D(Client.Game.GraphicsDevice, width, heightASCII, mipMap: false, SurfaceFormat.Color);
			}
			renderedText.Links.Clear();
			renderedText.LinesCount = num3;
			renderedText.Texture.SetData(array, 0, width * heightASCII);
			if (saveHitmap)
			{
				ulong textureID = (ulong)(int)((uint)(str.GetHashCode() ^ color) ^ (uint)align ^ flags ^ font ^ 0);
				picker.Set(textureID, width, heightASCII, array);
			}
		}
		finally
		{
			ArrayPool<uint>.Shared.Return(array, clearArray: true);
		}
	}

	private int GetFontOffsetY(byte font, byte index)
	{
		switch (index)
		{
		case 184:
			return 1;
		default:
			if ((index >= 192 && index <= 223) || index == 168)
			{
				break;
			}
			if (font < 10)
			{
				if (index >= 97 && index <= 122)
				{
					return _offsetCharTable[font];
				}
				return _offsetSymbolTable[font];
			}
			return 2;
		case 65:
		case 66:
		case 67:
		case 68:
		case 69:
		case 70:
		case 71:
		case 72:
		case 73:
		case 74:
		case 75:
		case 76:
		case 77:
		case 78:
		case 79:
		case 80:
		case 81:
		case 82:
		case 83:
		case 84:
		case 85:
		case 86:
		case 87:
		case 88:
		case 89:
		case 90:
			break;
		}
		return 0;
	}

	public MultilinesFontInfo GetInfoASCII(byte font, string str, int len, TEXT_ALIGN_TYPE align, ushort flags, int width, bool countret = false, bool countspaces = false)
	{
		if (font >= FontCount)
		{
			return null;
		}
		MultilinesFontInfo multilinesFontInfo = new MultilinesFontInfo();
		multilinesFontInfo.Reset();
		multilinesFontInfo.Align = align;
		MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
		int num = 0;
		multilinesFontInfo2.IndentionOffset = 0;
		bool flag = (flags & 0x20) != 0;
		bool flag2 = (flags & 0x40) != 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = (countret ? 1 : 0);
		for (int i = 0; i < len; i++)
		{
			char c = str[i];
			if (c == '\n' && (c == '\r' || flag || flag2))
			{
				continue;
			}
			if (c == ' ')
			{
				num3 = i;
				multilinesFontInfo2.Width += num4;
				num4 = 0;
				multilinesFontInfo2.CharCount += num2;
				num2 = 0;
			}
			ref FontCharacterData reference = ref _fontData[font, GetASCIIIndex(c)];
			int num6 = multilinesFontInfo2.CharStart;
			if (c == '\n' || multilinesFontInfo2.Width + num4 + reference.Width > width)
			{
				if (num3 == multilinesFontInfo2.CharStart && num3 == 0 && c != '\n')
				{
					num6++;
				}
				if (c == '\n')
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2 + num5;
					num3 = i;
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14;
					}
					multilinesFontInfo2.Data.Resize((uint)(multilinesFontInfo2.CharCount - num5));
					MultilinesFontInfo multilinesFontInfo3 = new MultilinesFontInfo();
					multilinesFontInfo3.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo3;
					multilinesFontInfo2 = multilinesFontInfo3;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i + 1;
					num4 = 0;
					num2 = 0;
					num = 0;
					multilinesFontInfo2.IndentionOffset = 0;
					continue;
				}
				if (num3 + 1 == num6 && !flag && !flag2)
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2;
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14;
					}
					MultilinesFontInfo multilinesFontInfo4 = new MultilinesFontInfo();
					multilinesFontInfo4.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo4;
					multilinesFontInfo2 = multilinesFontInfo4;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i;
					num3 = i - 1;
					num2 = 0;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (flags & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
				else
				{
					if (flag)
					{
						MultilinesFontData item = new MultilinesFontData(uint.MaxValue, flags, font, c, 0);
						multilinesFontInfo2.Data.Add(item);
						num4 += reference.Width;
						if (reference.Height > multilinesFontInfo2.MaxHeight)
						{
							multilinesFontInfo2.MaxHeight = reference.Height;
						}
						num2++;
						multilinesFontInfo2.Width += num4;
						multilinesFontInfo2.CharCount += num2;
					}
					i = num3 + 1;
					c = ((i < len) ? str[i] : '\0');
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					else if (countspaces && c != 0 && num3 - num6 == multilinesFontInfo2.CharCount)
					{
						multilinesFontInfo2.CharCount++;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14;
					}
					num2 = 0;
					multilinesFontInfo2.Data.Resize((uint)multilinesFontInfo2.CharCount);
					if (flag || flag2)
					{
						break;
					}
					MultilinesFontInfo multilinesFontInfo5 = new MultilinesFontInfo();
					multilinesFontInfo5.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo5;
					multilinesFontInfo2 = multilinesFontInfo5;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (flags & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
			}
			MultilinesFontData item2 = new MultilinesFontData(uint.MaxValue, flags, font, c, 0);
			multilinesFontInfo2.Data.Add(item2);
			num4 += ((c != '\r') ? reference.Width : 0);
			if (reference.Height > multilinesFontInfo2.MaxHeight)
			{
				multilinesFontInfo2.MaxHeight = reference.Height;
			}
			num2++;
		}
		multilinesFontInfo2.Width += num4;
		multilinesFontInfo2.CharCount += num2;
		if (num4 == 0 && len > 0 && (str[len - 1] == '\n' || str[len - 1] == '\r'))
		{
			multilinesFontInfo2.Width = 1;
			multilinesFontInfo2.MaxHeight = 14;
		}
		if (font == 4)
		{
			for (multilinesFontInfo2 = multilinesFontInfo; multilinesFontInfo2 != null; multilinesFontInfo2 = multilinesFontInfo2.Next)
			{
				if (multilinesFontInfo2.Width > 1)
				{
					multilinesFontInfo2.MaxHeight += 2;
				}
				else
				{
					multilinesFontInfo2.MaxHeight += 6;
				}
			}
		}
		return multilinesFontInfo;
	}

	public void SetUseHTML(bool value, uint htmlStartColor = uint.MaxValue, bool backgroundCanBeColored = false)
	{
		IsUsingHTML = value;
		_htmlStatus.Color = htmlStartColor;
		_htmlStatus.IsHtmlBackgroundColored = backgroundCanBeColored;
	}

	public void GenerateUnicode(RenderedText renderedText, byte font, string str, ushort color, byte cell, int width, TEXT_ALIGN_TYPE align, ushort flags, bool saveHitmap, int height, PixelPicker picker)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		if ((flags & 0x20) != 0 || (flags & 0x40) != 0 || (flags & 0x200) != 0)
		{
			if (width == 0)
			{
				return;
			}
			if (GetWidthUnicode(font, str.AsSpan()) > width)
			{
				string text = GetTextByWidthUnicode(font, str.AsSpan(), width, (flags & 0x40) != 0, align, flags);
				if ((flags & 0x200) != 0 && !string.IsNullOrEmpty(text))
				{
					int num = 0;
					while (num < height)
					{
						num += GetHeightUnicode(font, text, width, align, flags);
						if (str.Length <= text.Length)
						{
							break;
						}
						text += GetTextByWidthUnicode(font, str.AsSpan(0, text.Length), width, (flags & 0x40) != 0, align, flags);
					}
				}
				GeneratePixelsUnicode(renderedText, font, text, color, cell, width, align, flags, saveHitmap, picker);
				return;
			}
		}
		GeneratePixelsUnicode(renderedText, font, str, color, cell, width, align, flags, saveHitmap, picker);
	}

	public unsafe string GetTextByWidthUnicode(byte font, ReadOnlySpan<char> str, int width, bool isCropped, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || str.IsEmpty)
		{
			return string.Empty;
		}
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		int len = str.Length;
		Span<char> initialBuffer = stackalloc char[len];
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
		if (IsUsingHTML)
		{
			HTMLChar* data = stackalloc HTMLChar[len];
			GetHTMLData(data, font, str, ref len, align, flags);
			int num = str.Length - len;
			if (num > 0)
			{
				valueStringBuilder.Append(str.Slice(0, num));
				str = str.Slice(str.Length - len, len);
				if (GetWidthUnicode(font, str) < width)
				{
					isCropped = false;
				}
			}
		}
		if (isCropped)
		{
			uint num2 = ptr[46];
			if (num2 != 0 && num2 != uint.MaxValue)
			{
				width -= *(byte*)(void*)((IntPtr)ptr + (int)num2 + 2) * 3 + 3;
			}
		}
		int num3 = 0;
		ReadOnlySpan<char> readOnlySpan = str;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			uint num4 = ptr[(int)c];
			sbyte b = 0;
			if (num4 != 0 && num4 != uint.MaxValue)
			{
				byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)num4);
				b = (sbyte)((sbyte)(*ptr2) + (sbyte)ptr2[2] + 1);
			}
			else if (c == ' ')
			{
				b = 8;
			}
			if (b != 0)
			{
				num3 += b;
				if (num3 > width)
				{
					break;
				}
				valueStringBuilder.Append(c);
			}
		}
		if (isCropped)
		{
			valueStringBuilder.Append("...");
		}
		string result = valueStringBuilder.ToString();
		valueStringBuilder.Dispose();
		return result;
	}

	public int GetWidthUnicode(byte font, string str)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || string.IsNullOrEmpty(str))
		{
			return 0;
		}
		return GetWidthUnicode(font, str.AsSpan());
	}

	private unsafe int GetWidthUnicode(byte font, ReadOnlySpan<char> str)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || str.IsEmpty)
		{
			return 0;
		}
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		int num = 0;
		int val = 0;
		ReadOnlySpan<char> readOnlySpan = str;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			char c = readOnlySpan[i];
			uint num2 = ptr[(int)c];
			if (c != '\r' && num2 != 0 && num2 != uint.MaxValue)
			{
				byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)num2);
				num += (sbyte)(*ptr2) + (sbyte)ptr2[2] + 1;
				continue;
			}
			switch (c)
			{
			case ' ':
				num += 8;
				break;
			case '\n':
				val = Math.Max(val, num);
				num = 0;
				break;
			}
		}
		return Math.Max(val, num);
	}

	public unsafe int GetCharWidthUnicode(byte font, char c)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || c == '\0' || c == '\r')
		{
			return 0;
		}
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		uint num = ptr[(int)c];
		if (num != 0 && num != uint.MaxValue)
		{
			byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)num);
			return (sbyte)(*ptr2) + (sbyte)ptr2[2] + 1;
		}
		if (c == ' ')
		{
			return 8;
		}
		return 0;
	}

	public int GetWidthExUnicode(byte font, string text, int maxwidth, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || string.IsNullOrEmpty(text))
		{
			return 0;
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, text, text.Length, align, flags, maxwidth);
		int num = 0;
		while (multilinesFontInfo != null)
		{
			if (multilinesFontInfo.Width > num)
			{
				num = multilinesFontInfo.Width;
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
			multilinesFontInfo2 = null;
		}
		return num + 4;
	}

	public unsafe MultilinesFontInfo GetInfoUnicode(byte font, string str, int len, TEXT_ALIGN_TYPE align, ushort flags, int width, bool countret = false, bool countspaces = false)
	{
		_htmlStatus.WebLinkColor = 4278190335u;
		_htmlStatus.VisitedWebLinkColor = 65535u;
		_htmlStatus.BackgroundColor = 0u;
		_htmlStatus.Margins = Rectangle.Empty;
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero)
		{
			return null;
		}
		if (IsUsingHTML)
		{
			return GetInfoHTML(font, str, len, align, flags, width);
		}
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		MultilinesFontInfo multilinesFontInfo = new MultilinesFontInfo();
		multilinesFontInfo.Reset();
		multilinesFontInfo.Align = align;
		MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
		int num = 0;
		multilinesFontInfo2.IndentionOffset = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = (countret ? 1 : 0);
		int num6 = (((flags & 0x100) != 0) ? 4 : 0);
		bool flag = (flags & 0x20) != 0;
		bool flag2 = (flags & 0x40) != 0;
		ushort num7 = flags;
		byte font2 = font;
		uint num8 = uint.MaxValue;
		uint color = uint.MaxValue;
		uint num9 = uint.MaxValue;
		for (int i = 0; i < len; i++)
		{
			char c = str[i];
			if (c == '\n' && (flag || flag2))
			{
				c = '\0';
			}
			if ((ptr[(int)c] == 0 || ptr[(int)c] == uint.MaxValue) && c != ' ' && c != '\n' && c != '\r')
			{
				continue;
			}
			byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)ptr[(int)c]);
			if (c == ' ')
			{
				num3 = i;
				multilinesFontInfo2.Width += num4;
				num4 = 0;
				multilinesFontInfo2.CharCount += num2;
				num2 = 0;
				num9 = num8;
			}
			int num10 = multilinesFontInfo2.CharStart;
			if (multilinesFontInfo2.Width + num4 + (sbyte)(*ptr2) + (sbyte)ptr2[2] > width || c == '\n')
			{
				if (num3 == multilinesFontInfo2.CharStart && num3 == 0 && c != '\n')
				{
					num10++;
				}
				if (c == '\n')
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2 + num5;
					num3 = i;
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14 + num6;
					}
					multilinesFontInfo2.Data.Resize((uint)(multilinesFontInfo2.CharCount - num5));
					MultilinesFontInfo multilinesFontInfo3 = new MultilinesFontInfo();
					multilinesFontInfo3.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo3;
					multilinesFontInfo2 = multilinesFontInfo3;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i + 1;
					num4 = 0;
					num2 = 0;
					num = 0;
					multilinesFontInfo2.IndentionOffset = 0;
					continue;
				}
				if (num3 + 1 == num10 && !flag && !flag2)
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2;
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14 + num6;
					}
					MultilinesFontInfo multilinesFontInfo4 = new MultilinesFontInfo();
					multilinesFontInfo4.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo4;
					multilinesFontInfo2 = multilinesFontInfo4;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i;
					num3 = i - 1;
					num2 = 0;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (num7 & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
				else
				{
					if (flag)
					{
						MultilinesFontData item = new MultilinesFontData(color, num7, font2, c, 0);
						multilinesFontInfo2.Data.Add(item);
						num4 += ((c != '\r') ? ((sbyte)(*ptr2) + (sbyte)ptr2[2] + 1) : 0);
						if ((sbyte)ptr2[1] + (sbyte)ptr2[3] > multilinesFontInfo2.MaxHeight)
						{
							multilinesFontInfo2.MaxHeight = (sbyte)ptr2[1] + (sbyte)ptr2[3] + num6;
						}
						num2++;
						multilinesFontInfo2.Width += num4;
						multilinesFontInfo2.CharCount += num2;
					}
					i = num3 + 1;
					num8 = num9;
					color = num9;
					c = ((i < str.Length) ? str[i] : '\0');
					if (multilinesFontInfo2.Width == 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					else if (countspaces && c != 0 && num3 - num10 == multilinesFontInfo2.CharCount)
					{
						multilinesFontInfo2.CharCount++;
					}
					if (multilinesFontInfo2.MaxHeight == 0)
					{
						multilinesFontInfo2.MaxHeight = 14 + num6;
					}
					num2 = 0;
					multilinesFontInfo2.Data.Resize((uint)multilinesFontInfo2.CharCount);
					if (flag || flag2)
					{
						break;
					}
					MultilinesFontInfo multilinesFontInfo5 = new MultilinesFontInfo();
					multilinesFontInfo5.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo5;
					multilinesFontInfo2 = multilinesFontInfo5;
					multilinesFontInfo2.Align = align;
					multilinesFontInfo2.CharStart = i;
					num2 = 0;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (num7 & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
			}
			MultilinesFontData item2 = new MultilinesFontData(color, num7, font2, c, 0);
			multilinesFontInfo2.Data.Add(item2);
			if (c == ' ')
			{
				num4 += 8;
				if (multilinesFontInfo2.MaxHeight <= 0)
				{
					multilinesFontInfo2.MaxHeight = 5 + num6;
				}
			}
			else
			{
				num4 += ((c != '\r') ? ((sbyte)(*ptr2) + (sbyte)ptr2[2] + 1) : 0);
				if ((sbyte)ptr2[1] + (sbyte)ptr2[3] > multilinesFontInfo2.MaxHeight)
				{
					multilinesFontInfo2.MaxHeight = (sbyte)ptr2[1] + (sbyte)ptr2[3] + num6;
				}
			}
			num2++;
		}
		multilinesFontInfo2.Width += num4;
		multilinesFontInfo2.CharCount += num2;
		if (num4 == 0 && len != 0)
		{
			char c2 = str[len - 1];
			if (c2 != '\n')
			{
				if (c2 != '\r')
				{
					goto IL_0561;
				}
			}
			else
			{
				multilinesFontInfo2.CharCount += num5;
			}
			multilinesFontInfo2.Width = 1;
			multilinesFontInfo2.MaxHeight = 14;
		}
		goto IL_0561;
		IL_0561:
		return multilinesFontInfo;
	}

	private unsafe void GeneratePixelsUnicode(RenderedText renderedText, byte font, string str, ushort color, byte cell, int width, TEXT_ALIGN_TYPE align, ushort flags, bool saveHitmap, PixelPicker picker)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero)
		{
			return;
		}
		int length = str.Length;
		if (length == 0)
		{
			return;
		}
		int num = width;
		if (width == 0)
		{
			width = GetWidthUnicode(font, str.AsSpan());
			if (width == 0)
			{
				return;
			}
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, str, length, align, flags, width);
		if (multilinesFontInfo == null)
		{
			return;
		}
		if (IsUsingHTML && (_htmlStatus.Margins.X != 0 || _htmlStatus.Margins.Width != 0))
		{
			while (multilinesFontInfo != null)
			{
				MultilinesFontInfo next = multilinesFontInfo.Next;
				multilinesFontInfo.Data.Clear();
				multilinesFontInfo.Data.Count = 0u;
				multilinesFontInfo = null;
				multilinesFontInfo = next;
			}
			int num2 = width - _htmlStatus.Margins.Right;
			if (num2 < 10)
			{
				num2 = 10;
			}
			multilinesFontInfo = GetInfoUnicode(font, str, length, align, flags, num2);
			if (multilinesFontInfo == null)
			{
				return;
			}
		}
		if (num == 0 && RecalculateWidthByInfo)
		{
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			width = 0;
			while (multilinesFontInfo2 != null)
			{
				if (multilinesFontInfo2.Width > width)
				{
					width = multilinesFontInfo2.Width;
				}
				multilinesFontInfo2 = multilinesFontInfo2.Next;
			}
		}
		width += 4;
		int heightUnicode = GetHeightUnicode(multilinesFontInfo);
		if (heightUnicode == 0)
		{
			while (multilinesFontInfo != null)
			{
				MultilinesFontInfo multilinesFontInfo3 = multilinesFontInfo;
				multilinesFontInfo = multilinesFontInfo.Next;
				multilinesFontInfo3.Data.Clear();
				multilinesFontInfo3.Data.Count = 0u;
			}
			return;
		}
		heightUnicode += _htmlStatus.Margins.Y + _htmlStatus.Margins.Height + 4;
		int length2 = heightUnicode * width;
		uint[] array = ArrayPool<uint>.Shared.Rent(length2, zero: true);
		try
		{
			uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
			int num3 = _htmlStatus.Margins.Y;
			MultilinesFontInfo multilinesFontInfo4 = multilinesFontInfo;
			uint num4 = 0u;
			num4 = ((color != ushort.MaxValue) ? HuesHelper.RgbaToArgb((HuesLoader.Instance.GetPolygoneColor(cell, color) << 8) | 0xFF) : 4278190079u);
			bool flag = (flags & 2) != 0;
			bool flag2 = (flags & 1) != 0;
			bool flag3 = (flags & 8) != 0;
			bool flag4 = (flags & 0x10) != 0;
			uint num5 = 4278255873u;
			bool flag5 = false;
			int x = 0;
			int num6 = 0;
			int num7 = 0;
			RawList<WebLinkRect> rawList = new RawList<WebLinkRect>();
			while (multilinesFontInfo4 != null)
			{
				multilinesFontInfo = multilinesFontInfo4;
				num7++;
				int num8 = _htmlStatus.Margins.Y;
				switch (multilinesFontInfo4.Align)
				{
				case TEXT_ALIGN_TYPE.TS_CENTER:
					num8 += (width - 8) / 2 - multilinesFontInfo4.Width / 2;
					if (num8 < 0)
					{
						num8 = 0;
					}
					break;
				case TEXT_ALIGN_TYPE.TS_RIGHT:
					num8 += width - 10 - multilinesFontInfo4.Width;
					if (num8 < 0)
					{
						num8 = 0;
					}
					break;
				case TEXT_ALIGN_TYPE.TS_LEFT:
					if ((flags & 4) != 0)
					{
						num8 += multilinesFontInfo4.IndentionOffset;
					}
					break;
				}
				ushort num9 = 0;
				uint count = multilinesFontInfo4.Data.Count;
				for (int i = 0; i < count; i++)
				{
					ref MultilinesFontData reference = ref multilinesFontInfo4.Data[i];
					char item = reference.Item;
					ptr = (uint*)(void*)_unicodeFontAddress[reference.Font];
					if (!flag5)
					{
						num9 = reference.LinkID;
						if (num9 != 0)
						{
							flag5 = true;
							x = num8;
							num6 = num3 + 3;
						}
					}
					else if (reference.LinkID == 0 || i + 1 == count)
					{
						flag5 = false;
						int num10 = num3 - num6;
						if (num10 < 14)
						{
							num10 = 14;
						}
						int num11 = 0;
						if (item == ' ')
						{
							num11 = 8;
						}
						else if ((ptr[(int)item] != 0 && ptr[(int)item] != uint.MaxValue) || item == ' ')
						{
							byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)ptr[(int)item]);
							num11 = (sbyte)ptr2[2];
						}
						WebLinkRect webLinkRect = default(WebLinkRect);
						webLinkRect.LinkID = num9;
						webLinkRect.Bounds = new Rectangle(x, num6, num8 - num11, num10);
						WebLinkRect item2 = webLinkRect;
						rawList.Add(item2);
						num9 = 0;
					}
					if ((ptr[(int)item] == 0 || ptr[(int)item] == uint.MaxValue) && item != ' ')
					{
						continue;
					}
					byte* ptr3 = (byte*)(void*)((IntPtr)ptr + (int)ptr[(int)item]);
					int num12 = 0;
					int num13 = 0;
					int num14 = 0;
					int num15 = 0;
					if (item == ' ')
					{
						num12 = 0;
						num14 = 8;
					}
					else
					{
						num12 = (sbyte)(*ptr3) + 1;
						num13 = (sbyte)ptr3[1];
						num14 = ptr3[2];
						num15 = ptr3[3];
						ptr3 += 4;
					}
					int num16 = num8;
					uint num17 = num4;
					bool flag6 = (num17 & 0xFF) <= 8 && ((num17 >> 8) & 0xFF) <= 8 && ((num17 >> 16) & 0xFF) <= 8;
					if (item != ' ')
					{
						if (IsUsingHTML && i < multilinesFontInfo4.Data.Count)
						{
							flag = (reference.Flags & 2) != 0;
							flag2 = (reference.Flags & 1) != 0;
							flag3 = (reference.Flags & 8) != 0;
							flag4 = (reference.Flags & 0x10) != 0;
							if (reference.Color != uint.MaxValue)
							{
								num17 = HuesHelper.RgbaToArgb(reference.Color);
								flag6 = (num17 & 0xFF) <= 8 && ((num17 >> 8) & 0xFF) <= 8 && ((num17 >> 16) & 0xFF) <= 8;
							}
						}
						int num18 = (num14 - 1 >> 3) + 1;
						for (int j = 0; j < num15; j++)
						{
							int num19 = num13 + num3 + j;
							if (num19 < 0)
							{
								num19 = 0;
							}
							if (num19 >= heightUnicode)
							{
								break;
							}
							byte* ptr4 = ptr3;
							ptr3 += num18;
							int num20 = 0;
							if (flag)
							{
								num20 = (int)((float)(num15 - j) / 3.3f);
							}
							int num21 = num8 + num12 + num20 + (flag2 ? 1 : 0);
							for (int k = 0; k < num18; k++)
							{
								int num22 = k << 3;
								for (int l = 0; l < 8; l++)
								{
									int num23 = num22 + l;
									if (num23 >= num14)
									{
										break;
									}
									int num24 = num21 + num23;
									if (num24 >= width)
									{
										break;
									}
									byte num25 = (byte)(ptr4[k] & (1 << 7 - l));
									int num26 = num19 * width + num24;
									if (num25 != 0)
									{
										array[num26] = num17;
									}
								}
							}
						}
						if (flag2)
						{
							uint num27 = num5;
							if (num27 == num17)
							{
								num27++;
							}
							int num28 = ((num8 + num12 > 0) ? (-1) : 0);
							int num29 = ((num8 + num12 + num14 < width) ? 1 : 0);
							num29 += num14;
							for (int m = 0; m < num15; m++)
							{
								int num30 = num13 + num3 + m;
								if (num30 >= heightUnicode)
								{
									break;
								}
								if (num30 < 0)
								{
									num30 = 0;
								}
								int num31 = 0;
								if (flag && m < num15)
								{
									num31 = (int)((float)(num15 - m) / 3.3f);
								}
								for (int n = num28; n < num29; n++)
								{
									int num32 = n + num8 + num12 + num31;
									if (num32 >= width)
									{
										break;
									}
									int num33 = num30 * width + num32;
									if (array[num33] != 0 || array[num33] == num27)
									{
										continue;
									}
									int num34 = ((n >= num14) ? 1 : 2);
									if (num34 == 2 && num32 + 1 >= width)
									{
										num34--;
									}
									for (int num35 = 0; num35 < num34; num35++)
									{
										int num36 = num32 + num35;
										int num37 = num30 * width + num36;
										if (array[num37] != 0 && array[num37] != num27)
										{
											array[num33] = num27;
											break;
										}
									}
								}
							}
							for (int num38 = 0; num38 < num15; num38++)
							{
								int num39 = num13 + num3 + num38;
								if (num39 >= heightUnicode)
								{
									break;
								}
								if (num39 < 0)
								{
									num39 = 0;
								}
								int num40 = 0;
								if (flag)
								{
									num40 = (int)((float)(num15 - num38) / 3.3f);
								}
								for (int num41 = 0; num41 < num14; num41++)
								{
									int num42 = num41 + num8 + num12 + num40;
									if (num42 >= width)
									{
										break;
									}
									int num43 = num39 * width + num42;
									if (array[num43] == num27)
									{
										array[num43] = num17;
									}
								}
							}
						}
						if (flag3 && !flag6)
						{
							int num44 = ((num8 + num12 > 0) ? (-1) : 0);
							int num45 = ((num13 + num3 > 0) ? (-1) : 0);
							int num46 = ((num8 + num12 + num14 < width) ? 1 : 0);
							int num47 = ((num13 + num3 + num15 < heightUnicode) ? 1 : 0);
							num46 += num14;
							num47 += num15;
							for (int num48 = num45; num48 < num47; num48++)
							{
								int num49 = num13 + num3 + num48;
								if (num49 < 0)
								{
									num49 = 0;
								}
								if (num49 >= heightUnicode)
								{
									break;
								}
								int num50 = 0;
								if (flag && num48 >= 0 && num48 < num15)
								{
									num50 = (int)((float)(num15 - num48) / 3.3f);
								}
								for (int num51 = num44; num51 < num46; num51++)
								{
									int num52 = num51 + num8 + num12 + num50;
									if (num52 >= width)
									{
										break;
									}
									int num53 = num49 * width + num52;
									if (array[num53] != 0 || array[num53] == num5)
									{
										continue;
									}
									int num54 = ((num51 > 0) ? (-1) : 0);
									int num55 = ((num48 > 0) ? (-1) : 0);
									int num56 = ((num51 >= num14 - 1) ? 1 : 2);
									int num57 = ((num48 >= num15 - 1) ? 1 : 2);
									if (num56 == 2 && num52 + 1 >= width)
									{
										num56--;
									}
									bool flag7 = false;
									for (int num58 = num54; num58 < num56; num58++)
									{
										int num59 = num52 + num58;
										for (int num60 = num55; num60 < num57; num60++)
										{
											int num61 = (num49 + num60) * width + num59;
											if (num61 >= 0 && num61 < array.Length && array[num61] != 0 && array[num61] != num5)
											{
												array[num53] = num5;
												flag7 = true;
												break;
											}
										}
										if (flag7)
										{
											break;
										}
									}
								}
							}
						}
						num8 += num14 + num12 + (flag2 ? 1 : 0);
					}
					else if (item == ' ')
					{
						num8 += 8;
						if (IsUsingHTML)
						{
							flag4 = (reference.Flags & 0x10) != 0;
							if (reference.Color != uint.MaxValue)
							{
								num17 = HuesHelper.RgbaToArgb(reference.Color);
								flag6 = (num17 & 0xFF) <= 8 && ((num17 >> 8) & 0xFF) <= 8 && ((num17 >> 16) & 0xFF) <= 8;
							}
						}
					}
					if (!flag4)
					{
						continue;
					}
					int num62 = ((num16 + num12 > 0) ? (-1) : 0);
					int num63 = ((num8 + num12 + num14 < width) ? 1 : 0);
					byte* ptr5 = (byte*)(void*)((IntPtr)ptr + (int)ptr[97]);
					int num64 = num3 + (sbyte)ptr5[1] + (sbyte)ptr5[3];
					if (num64 >= heightUnicode)
					{
						break;
					}
					if (num64 < 0)
					{
						num64 = 0;
					}
					for (int num65 = num62; num65 < num14 + num63; num65++)
					{
						int num66 = num65 + num16 + num12 + (flag2 ? 1 : 0);
						if (num66 >= width)
						{
							break;
						}
						int num67 = num64 * width + num66;
						array[num67] = num17;
					}
				}
				num3 += multilinesFontInfo4.MaxHeight;
				multilinesFontInfo4 = multilinesFontInfo4.Next;
				multilinesFontInfo.Data.Clear();
				multilinesFontInfo.Data.Count = 0u;
				multilinesFontInfo = null;
			}
			if (IsUsingHTML && _htmlStatus.IsHtmlBackgroundColored && _htmlStatus.BackgroundColor != 0)
			{
				_htmlStatus.BackgroundColor |= 255u;
				uint num68 = HuesHelper.RgbaToArgb(_htmlStatus.BackgroundColor);
				for (int num69 = 0; num69 < heightUnicode; num69++)
				{
					int num70 = num69 * width;
					for (int num71 = 0; num71 < width; num71++)
					{
						ref uint reference2 = ref array[num70 + num71];
						if (reference2 == 0)
						{
							reference2 = num68;
						}
					}
				}
			}
			if (renderedText.Texture == null || renderedText.Texture.IsDisposed)
			{
				renderedText.Texture = new Texture2D(Client.Game.GraphicsDevice, width, heightUnicode, mipMap: false, SurfaceFormat.Color);
			}
			renderedText.Links.Clear();
			renderedText.Links.AddRange(rawList);
			renderedText.LinesCount = num7;
			renderedText.Texture.SetData(array, 0, width * heightUnicode);
			if (saveHitmap)
			{
				ulong textureID = (ulong)(int)((uint)(str.GetHashCode() ^ color) ^ (uint)align ^ flags ^ font ^ 1);
				picker.Set(textureID, width, heightUnicode, array);
			}
		}
		finally
		{
			ArrayPool<uint>.Shared.Return(array, clearArray: true);
		}
	}

	private unsafe MultilinesFontInfo GetInfoHTML(byte font, string str, int len, TEXT_ALIGN_TYPE align, ushort flags, int width)
	{
		if (len <= 0)
		{
			return null;
		}
		HTMLChar* ptr = stackalloc HTMLChar[len];
		GetHTMLData(ptr, font, str.AsSpan(), ref len, align, flags);
		if (len <= 0)
		{
			return null;
		}
		MultilinesFontInfo multilinesFontInfo = new MultilinesFontInfo();
		multilinesFontInfo.Reset();
		multilinesFontInfo.Align = align;
		MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
		int num = (multilinesFontInfo2.IndentionOffset = 0);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		bool flag = (flags & 0x20) != 0;
		bool flag2 = (flags & 0x40) != 0;
		if (len != 0)
		{
			multilinesFontInfo2.Align = ptr->Align;
		}
		for (int i = 0; i < len; i++)
		{
			char c = ptr[i].Char;
			uint* ptr2 = (uint*)(void*)_unicodeFontAddress[ptr[i].Font];
			if (c == '\r' || c == '\n')
			{
				c = ((!(c == '\r' || flag || flag2)) ? '\n' : '\0');
			}
			if ((ptr2[(int)c] == 0 || ptr2[(int)c] == uint.MaxValue) && c != ' ' && c != '\n')
			{
				continue;
			}
			byte* ptr3 = (byte*)(void*)((IntPtr)ptr2 + (int)ptr2[(int)c]);
			if (c == ' ')
			{
				num3 = i;
				multilinesFontInfo2.Width += num4;
				num4 = 0;
				multilinesFontInfo2.CharCount += num2;
				num2 = 0;
			}
			int num5 = ptr[i].Flags & 1;
			if (multilinesFontInfo2.Width + num4 + (sbyte)(*ptr3) + (sbyte)ptr3[2] + num5 > width || c == '\n')
			{
				if (num3 == multilinesFontInfo2.CharStart && num3 == 0 && c != '\n')
				{
					multilinesFontInfo2.CharStart = 1;
				}
				if (c == '\n')
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2;
					num3 = i;
					if (multilinesFontInfo2.Width <= 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					multilinesFontInfo2.MaxHeight = 18;
					multilinesFontInfo2.Data.Resize((uint)multilinesFontInfo2.CharCount);
					MultilinesFontInfo multilinesFontInfo3 = new MultilinesFontInfo();
					multilinesFontInfo3.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo3;
					multilinesFontInfo2 = multilinesFontInfo3;
					multilinesFontInfo2.Align = ptr[i].Align;
					multilinesFontInfo2.CharStart = i + 1;
					num4 = 0;
					num2 = 0;
					num = (multilinesFontInfo2.IndentionOffset = 0);
					continue;
				}
				if (num3 + 1 == multilinesFontInfo2.CharStart && !flag && !flag2)
				{
					multilinesFontInfo2.Width += num4;
					multilinesFontInfo2.CharCount += num2;
					if (multilinesFontInfo2.Width <= 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					multilinesFontInfo2.MaxHeight = 18;
					MultilinesFontInfo multilinesFontInfo4 = new MultilinesFontInfo();
					multilinesFontInfo4.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo4;
					multilinesFontInfo2 = multilinesFontInfo4;
					multilinesFontInfo2.Align = ptr[i].Align;
					multilinesFontInfo2.CharStart = i;
					num3 = i - 1;
					num2 = 0;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (ptr[i].Flags & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
				else
				{
					if (flag)
					{
						MultilinesFontData item = new MultilinesFontData(ptr[i].Color, ptr[i].Flags, ptr[i].Font, c, ptr[i].LinkID);
						multilinesFontInfo2.Data.Add(item);
						num4 += (sbyte)(*ptr3) + (sbyte)ptr3[2] + 1;
						multilinesFontInfo2.MaxHeight = 18;
						num2++;
						multilinesFontInfo2.Width += num4;
						multilinesFontInfo2.CharCount += num2;
					}
					i = num3 + 1;
					if (i >= len)
					{
						break;
					}
					c = ptr[i].Char;
					num5 = ptr[i].Flags & 1;
					if (multilinesFontInfo2.Width <= 0)
					{
						multilinesFontInfo2.Width = 1;
					}
					multilinesFontInfo2.MaxHeight = 18;
					multilinesFontInfo2.Data.Resize((uint)multilinesFontInfo2.CharCount);
					num2 = 0;
					if (flag || flag2)
					{
						break;
					}
					MultilinesFontInfo multilinesFontInfo5 = new MultilinesFontInfo();
					multilinesFontInfo5.Reset();
					multilinesFontInfo2.Next = multilinesFontInfo5;
					multilinesFontInfo2 = multilinesFontInfo5;
					multilinesFontInfo2.Align = ptr[i].Align;
					multilinesFontInfo2.CharStart = i;
					if (multilinesFontInfo2.Align == TEXT_ALIGN_TYPE.TS_LEFT && (ptr[i].Flags & 4) != 0)
					{
						num = 14;
					}
					multilinesFontInfo2.IndentionOffset = num;
					num4 = num;
				}
			}
			MultilinesFontData item2 = new MultilinesFontData(ptr[i].Color, ptr[i].Flags, ptr[i].Font, c, ptr[i].LinkID);
			multilinesFontInfo2.Data.Add(item2);
			num4 = ((c != ' ') ? (num4 + ((sbyte)(*ptr3) + (sbyte)ptr3[2] + 1 + num5)) : (num4 + 8));
			num2++;
		}
		multilinesFontInfo2.Width += num4;
		multilinesFontInfo2.CharCount += num2;
		multilinesFontInfo2.MaxHeight = 18;
		return multilinesFontInfo;
	}

	private unsafe void GetHTMLData(HTMLChar* data, byte font, ReadOnlySpan<char> str, ref int len, TEXT_ALIGN_TYPE align, ushort flags)
	{
		int num = 0;
		HTMLDataInfo hTMLDataInfo = default(HTMLDataInfo);
		hTMLDataInfo.Tag = HTML_TAG_TYPE.HTT_NONE;
		hTMLDataInfo.Align = align;
		hTMLDataInfo.Flags = flags;
		hTMLDataInfo.Font = font;
		hTMLDataInfo.Color = _htmlStatus.Color;
		hTMLDataInfo.Link = 0;
		HTMLDataInfo hTMLDataInfo2 = hTMLDataInfo;
		RawList<HTMLDataInfo> list = new RawList<HTMLDataInfo>();
		list.Add(hTMLDataInfo2);
		HTMLDataInfo info = hTMLDataInfo2;
		for (int i = 0; i < len; i++)
		{
			char c = str[i];
			if (c == '<')
			{
				bool endTag = false;
				hTMLDataInfo = default(HTMLDataInfo);
				hTMLDataInfo.Tag = HTML_TAG_TYPE.HTT_NONE;
				hTMLDataInfo.Align = TEXT_ALIGN_TYPE.TS_LEFT;
				hTMLDataInfo.Flags = 0;
				hTMLDataInfo.Font = byte.MaxValue;
				hTMLDataInfo.Color = 0u;
				hTMLDataInfo.Link = 0;
				HTMLDataInfo info2 = hTMLDataInfo;
				HTML_TAG_TYPE hTML_TAG_TYPE = ParseHTMLTag(str, len, ref i, ref endTag, ref info2);
				if (hTML_TAG_TYPE == HTML_TAG_TYPE.HTT_NONE)
				{
					continue;
				}
				if (!endTag)
				{
					if (info2.Font == byte.MaxValue)
					{
						info2.Font = list[list.Count - 1].Font;
					}
					if (hTML_TAG_TYPE != HTML_TAG_TYPE.HTT_BODY)
					{
						list.Add(info2);
					}
					else
					{
						list.Clear();
						num = 0;
						if (info2.Color != 0)
						{
							hTMLDataInfo2.Color = info2.Color;
						}
						list.Add(hTMLDataInfo2);
					}
				}
				else if (list.Count > 1)
				{
					for (uint num2 = list.Count - 1; num2 >= 1; num2--)
					{
						if (list[num2].Tag == hTML_TAG_TYPE)
						{
							list.RemoveAt(num2);
							break;
						}
					}
				}
				GetCurrentHTMLInfo(ref list, ref info);
				switch (hTML_TAG_TYPE)
				{
				case HTML_TAG_TYPE.HTT_LEFT:
				case HTML_TAG_TYPE.HTT_CENTER:
				case HTML_TAG_TYPE.HTT_RIGHT:
					if (num != 0)
					{
						endTag = true;
					}
					goto case HTML_TAG_TYPE.HTT_P;
				case HTML_TAG_TYPE.HTT_P:
					c = (endTag ? '\n' : '\0');
					break;
				case HTML_TAG_TYPE.HTT_BR:
				case HTML_TAG_TYPE.HTT_BQ:
				case HTML_TAG_TYPE.HTT_BODYBGCOLOR:
					c = '\n';
					break;
				default:
					c = '\0';
					break;
				}
			}
			if (c != 0)
			{
				HTMLChar* num3 = data + num;
				num3->Char = c;
				num3->Font = info.Font;
				num3->Align = info.Align;
				num3->Flags = info.Flags;
				num3->Color = info.Color;
				num3->LinkID = info.Link;
				num++;
			}
		}
		len = num;
	}

	private void GetCurrentHTMLInfo(ref RawList<HTMLDataInfo> list, ref HTMLDataInfo info)
	{
		info.Tag = HTML_TAG_TYPE.HTT_NONE;
		info.Align = TEXT_ALIGN_TYPE.TS_LEFT;
		info.Flags = 0;
		info.Font = byte.MaxValue;
		info.Color = 0u;
		info.Link = 0;
		for (int i = 0; i < list.Count; i++)
		{
			ref HTMLDataInfo reference = ref list[i];
			switch (reference.Tag)
			{
			case HTML_TAG_TYPE.HTT_NONE:
				info = reference;
				break;
			case HTML_TAG_TYPE.HTT_B:
			case HTML_TAG_TYPE.HTT_I:
			case HTML_TAG_TYPE.HTT_U:
			case HTML_TAG_TYPE.HTT_P:
				info.Flags |= reference.Flags;
				info.Align = reference.Align;
				break;
			case HTML_TAG_TYPE.HTT_A:
				info.Flags |= reference.Flags;
				info.Color = reference.Color;
				info.Link = reference.Link;
				break;
			case HTML_TAG_TYPE.HTT_BIG:
			case HTML_TAG_TYPE.HTT_SMALL:
				if (reference.Font != byte.MaxValue && _unicodeFontAddress[reference.Font] != IntPtr.Zero)
				{
					info.Font = reference.Font;
				}
				break;
			case HTML_TAG_TYPE.HTT_BASEFONT:
				if (reference.Font != byte.MaxValue && _unicodeFontAddress[reference.Font] != IntPtr.Zero)
				{
					info.Font = reference.Font;
				}
				if (reference.Color != 0)
				{
					info.Color = reference.Color;
				}
				break;
			case HTML_TAG_TYPE.HTT_H1:
			case HTML_TAG_TYPE.HTT_H2:
			case HTML_TAG_TYPE.HTT_H4:
			case HTML_TAG_TYPE.HTT_H5:
				info.Flags |= reference.Flags;
				goto case HTML_TAG_TYPE.HTT_H3;
			case HTML_TAG_TYPE.HTT_H3:
			case HTML_TAG_TYPE.HTT_H6:
				if (reference.Font != byte.MaxValue && _unicodeFontAddress[reference.Font] != IntPtr.Zero)
				{
					info.Font = reference.Font;
				}
				break;
			case HTML_TAG_TYPE.HTT_BQ:
				info.Color = reference.Color;
				info.Flags |= reference.Flags;
				break;
			case HTML_TAG_TYPE.HTT_LEFT:
			case HTML_TAG_TYPE.HTT_CENTER:
			case HTML_TAG_TYPE.HTT_RIGHT:
				info.Align = reference.Align;
				break;
			case HTML_TAG_TYPE.HTT_DIV:
				info.Align = reference.Align;
				break;
			}
		}
	}

	private HTML_TAG_TYPE ParseHTMLTag(ReadOnlySpan<char> str, int len, ref int i, ref bool endTag, ref HTMLDataInfo info)
	{
		HTML_TAG_TYPE hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_NONE;
		i++;
		if (i < len && str[i] == '/')
		{
			endTag = true;
			i++;
		}
		while (i < len && str[i] == ' ')
		{
			i++;
		}
		int num = i;
		while (i < len)
		{
			if (str[i] == '/')
			{
				endTag = true;
				break;
			}
			if (str[i] == ' ' || str[i] == '>')
			{
				break;
			}
			i++;
		}
		if (num != i && i < len)
		{
			int length = i - num;
			int start = num;
			num = i;
			while (i < len && str[i] != '>')
			{
				i++;
			}
			ReadOnlySpan<char> span = str.Slice(start, length);
			if (MemoryExtensions.Equals(span, "b".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_B;
			}
			else if (MemoryExtensions.Equals(span, "i".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_I;
			}
			else if (MemoryExtensions.Equals(span, "a".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_A;
			}
			else if (MemoryExtensions.Equals(span, "u".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_U;
			}
			else if (MemoryExtensions.Equals(span, "p".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_P;
			}
			else if (MemoryExtensions.Equals(span, "big".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BIG;
			}
			else if (MemoryExtensions.Equals(span, "small".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_SMALL;
			}
			else if (MemoryExtensions.Equals(span, "body".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BODY;
			}
			else if (MemoryExtensions.Equals(span, "basefont".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BASEFONT;
			}
			else if (MemoryExtensions.Equals(span, "h1".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H1;
			}
			else if (MemoryExtensions.Equals(span, "h2".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H2;
			}
			else if (MemoryExtensions.Equals(span, "h3".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H3;
			}
			else if (MemoryExtensions.Equals(span, "h4".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H4;
			}
			else if (MemoryExtensions.Equals(span, "h5".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H5;
			}
			else if (MemoryExtensions.Equals(span, "h6".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_H6;
			}
			else if (MemoryExtensions.Equals(span, "br".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BR;
			}
			else if (MemoryExtensions.Equals(span, "bq".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BQ;
			}
			else if (MemoryExtensions.Equals(span, "left".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_LEFT;
			}
			else if (MemoryExtensions.Equals(span, "center".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_CENTER;
			}
			else if (MemoryExtensions.Equals(span, "right".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_RIGHT;
			}
			else if (MemoryExtensions.Equals(span, "div".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_DIV;
			}
			else if (str.IndexOf("bodybgcolor".AsSpan(), StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BODYBGCOLOR;
				num = str.IndexOf("bodybgcolor".AsSpan(), StringComparison.InvariantCultureIgnoreCase) + "bodybgcolor".Length;
				endTag = false;
			}
			else if (str.IndexOf("basefont".AsSpan(), StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BASEFONT;
				num = str.IndexOf("basefont".AsSpan(), StringComparison.InvariantCultureIgnoreCase) + "basefont".Length;
				endTag = false;
			}
			else if (str.IndexOf("bodytext".AsSpan(), StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				hTML_TAG_TYPE = HTML_TAG_TYPE.HTT_BODY;
				num = str.IndexOf("bodytext".AsSpan(), StringComparison.InvariantCultureIgnoreCase) + "bodytext".Length;
				endTag = false;
			}
			else
			{
				Log.Warn("Unhandled HTML param:\t" + str);
			}
			if (!endTag)
			{
				GetHTMLInfoFromTag(hTML_TAG_TYPE, ref info);
				if (i < len && num != i)
				{
					switch (hTML_TAG_TYPE)
					{
					case HTML_TAG_TYPE.HTT_A:
					case HTML_TAG_TYPE.HTT_P:
					case HTML_TAG_TYPE.HTT_BODY:
					case HTML_TAG_TYPE.HTT_BASEFONT:
					case HTML_TAG_TYPE.HTT_DIV:
					case HTML_TAG_TYPE.HTT_BODYBGCOLOR:
						length = i - num;
						if (str.Length != 0 && str.Length > num && str.Length >= length)
						{
							GetHTMLInfoFromContent(ref info, str, num, length);
						}
						break;
					}
				}
			}
		}
		return hTML_TAG_TYPE;
	}

	private unsafe void GetHTMLInfoFromContent(ref HTMLDataInfo info, ReadOnlySpan<char> content, int start, int length)
	{
		int i = 0;
		if (content.IsEmpty)
		{
			return;
		}
		for (; i < length && char.IsWhiteSpace(content[i + start]); i++)
		{
		}
		char* ptr = stackalloc char[128];
		char* ptr2 = stackalloc char[128];
		int length2 = 0;
		for (; i < length; i++)
		{
			char c = content[i + start];
			ptr[length2++] = (char.IsLetter(c) ? char.ToLowerInvariant(c) : c);
			if (c != ' ' && c != '=' && c != '\\')
			{
				continue;
			}
			i++;
			bool flag = false;
			int num = 0;
			for (; i < length; i++)
			{
				c = content[i + start];
				if (c == ' ' || c == '\\' || c == '<' || c == '>' || (c == '=' && !flag))
				{
					break;
				}
				if (c != '"')
				{
					ptr2[num++] = (char.IsLetter(c) ? char.ToLowerInvariant(c) : c);
				}
				else
				{
					flag = !flag;
				}
			}
			if (num != 0)
			{
				switch (info.Tag)
				{
				case HTML_TAG_TYPE.HTT_BODY:
				case HTML_TAG_TYPE.HTT_BODYBGCOLOR:
					if (StringHelper.UnsafeCompare(ptr, "text", length2))
					{
						ReadColorFromTextBuffer(ptr2, num, ref info.Color);
					}
					else if (StringHelper.UnsafeCompare(ptr, "bgcolor", length2))
					{
						if (_htmlStatus.IsHtmlBackgroundColored)
						{
							ReadColorFromTextBuffer(ptr2, num, ref _htmlStatus.BackgroundColor);
						}
					}
					else if (StringHelper.UnsafeCompare(ptr, "link", length2))
					{
						ReadColorFromTextBuffer(ptr2, num, ref _htmlStatus.WebLinkColor);
					}
					else if (StringHelper.UnsafeCompare(ptr, "vlink", length2))
					{
						ReadColorFromTextBuffer(ptr2, num, ref _htmlStatus.VisitedWebLinkColor);
					}
					else if (StringHelper.UnsafeCompare(ptr, "leftmargin", length2))
					{
						_htmlStatus.Margins.X = int.Parse(new string(ptr2, 0, num));
					}
					else if (StringHelper.UnsafeCompare(ptr, "topmargin", length2))
					{
						_htmlStatus.Margins.Y = int.Parse(new string(ptr2, 0, num));
					}
					else if (StringHelper.UnsafeCompare(ptr, "rightmargin", length2))
					{
						_htmlStatus.Margins.Width = int.Parse(new string(ptr2, 0, num));
					}
					else if (StringHelper.UnsafeCompare(ptr, "bottommargin", length2))
					{
						_htmlStatus.Margins.Height = int.Parse(new string(ptr2, 0, num));
					}
					break;
				case HTML_TAG_TYPE.HTT_BASEFONT:
					if (StringHelper.UnsafeCompare(ptr, "color", length2))
					{
						ReadColorFromTextBuffer(ptr2, num, ref info.Color);
					}
					else if (StringHelper.UnsafeCompare(ptr, "size", length2))
					{
						byte b = byte.Parse(new string(ptr2, 0, num));
						if (b == 0 || b == 4)
						{
							info.Font = 1;
						}
						else if (b < 4)
						{
							info.Font = 2;
						}
						else
						{
							info.Font = 0;
						}
					}
					break;
				case HTML_TAG_TYPE.HTT_A:
					if (StringHelper.UnsafeCompare(ptr, "href", length2))
					{
						info.Flags = 16;
						info.Color = _htmlStatus.WebLinkColor;
						info.Link = GetWebLinkID(ptr2, num, ref info.Color);
					}
					break;
				case HTML_TAG_TYPE.HTT_P:
				case HTML_TAG_TYPE.HTT_DIV:
					if (StringHelper.UnsafeCompare(ptr, "align", length2))
					{
						if (StringHelper.UnsafeCompare(ptr2, "left", num))
						{
							info.Align = TEXT_ALIGN_TYPE.TS_LEFT;
						}
						else if (StringHelper.UnsafeCompare(ptr2, "center", num))
						{
							info.Align = TEXT_ALIGN_TYPE.TS_CENTER;
						}
						else if (StringHelper.UnsafeCompare(ptr2, "right", num))
						{
							info.Align = TEXT_ALIGN_TYPE.TS_RIGHT;
						}
					}
					break;
				}
			}
			length2 = 0;
		}
	}

	private unsafe ushort GetWebLinkID(char* link, int linkLength, ref uint color)
	{
		foreach (KeyValuePair<ushort, WebLink> webLink in _webLinks)
		{
			if (webLink.Value.Link.Length == linkLength && StringHelper.UnsafeCompare(link, webLink.Value.Link, linkLength))
			{
				if (webLink.Value.IsVisited)
				{
					color = _htmlStatus.VisitedWebLinkColor;
				}
				return webLink.Key;
			}
		}
		ushort num = (ushort)(_webLinks.Count + 1);
		if (!_webLinks.TryGetValue(num, out var value))
		{
			value = new WebLink();
			value.IsVisited = false;
			value.Link = new string(link, 0, linkLength);
			_webLinks[num] = value;
		}
		return num;
	}

	public bool GetWebLink(ushort link, out WebLink result)
	{
		if (!_webLinks.TryGetValue(link, out result))
		{
			return false;
		}
		result.IsVisited = true;
		return true;
	}

	private unsafe void ReadColorFromTextBuffer(char* buffer, int length, ref uint color)
	{
		color = 0u;
		if (length <= 0)
		{
			return;
		}
		if (*buffer == '#')
		{
			if (length > 1)
			{
				int num = ((buffer[1] != '0' || buffer[2] != 'x') ? 1 : 3);
				uint.TryParse(new string(buffer, num, length - num), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result);
				byte* ptr = (byte*)(&result);
				color = (uint)((*ptr << 24) | (ptr[1] << 16) | (ptr[2] << 8) | 0xFF);
			}
		}
		else if (char.IsNumber(*buffer))
		{
			color = Convert.ToUInt32(new string(buffer, 0, length), 16);
		}
		else if (StringHelper.UnsafeCompare(buffer, "red", length))
		{
			color = 65535u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "cyan", length))
		{
			color = 4294902015u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "blue", length))
		{
			color = 4278190335u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "darkblue", length))
		{
			color = 2684354815u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "lightblue", length))
		{
			color = 3872959999u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "purple", length))
		{
			color = 2147516671u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "yellow", length))
		{
			color = 16777215u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "lime", length))
		{
			color = 16711935u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "magenta", length))
		{
			color = 4278255615u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "white", length))
		{
			color = 4294901503u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "silver", length))
		{
			color = 3233857791u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "gray", length) || StringHelper.UnsafeCompare(buffer, "grey", length))
		{
			color = 2155905279u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "black", length))
		{
			color = 16843263u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "orange", length))
		{
			color = 10878975u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "brown", length))
		{
			color = 707438079u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "maroon", length))
		{
			color = 33023u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "green", length))
		{
			color = 8388863u;
		}
		else if (StringHelper.UnsafeCompare(buffer, "olive", length))
		{
			color = 8421631u;
		}
	}

	private void GetHTMLInfoFromTag(HTML_TAG_TYPE tag, ref HTMLDataInfo info)
	{
		info.Tag = tag;
		info.Align = TEXT_ALIGN_TYPE.TS_LEFT;
		info.Flags = 0;
		info.Font = byte.MaxValue;
		info.Color = 0u;
		info.Link = 0;
		switch (tag)
		{
		case HTML_TAG_TYPE.HTT_B:
			info.Flags = 1;
			break;
		case HTML_TAG_TYPE.HTT_I:
			info.Flags = 2;
			break;
		case HTML_TAG_TYPE.HTT_U:
			info.Flags = 16;
			break;
		case HTML_TAG_TYPE.HTT_P:
			info.Flags = 4;
			break;
		case HTML_TAG_TYPE.HTT_BIG:
			info.Font = 0;
			break;
		case HTML_TAG_TYPE.HTT_SMALL:
			info.Font = 2;
			break;
		case HTML_TAG_TYPE.HTT_H1:
			info.Flags = 17;
			info.Font = 0;
			break;
		case HTML_TAG_TYPE.HTT_H2:
			info.Flags = 1;
			info.Font = 0;
			break;
		case HTML_TAG_TYPE.HTT_H3:
			info.Font = 0;
			break;
		case HTML_TAG_TYPE.HTT_H4:
			info.Flags = 1;
			info.Font = 2;
			break;
		case HTML_TAG_TYPE.HTT_H5:
			info.Flags = 2;
			info.Font = 2;
			break;
		case HTML_TAG_TYPE.HTT_H6:
			info.Font = 2;
			break;
		case HTML_TAG_TYPE.HTT_BQ:
			info.Flags = 128;
			info.Color = 8388863u;
			break;
		case HTML_TAG_TYPE.HTT_LEFT:
			info.Align = TEXT_ALIGN_TYPE.TS_LEFT;
			break;
		case HTML_TAG_TYPE.HTT_CENTER:
			info.Align = TEXT_ALIGN_TYPE.TS_CENTER;
			break;
		case HTML_TAG_TYPE.HTT_RIGHT:
			info.Align = TEXT_ALIGN_TYPE.TS_RIGHT;
			break;
		case HTML_TAG_TYPE.HTT_A:
		case HTML_TAG_TYPE.HTT_BODY:
		case HTML_TAG_TYPE.HTT_BASEFONT:
		case HTML_TAG_TYPE.HTT_BR:
			break;
		}
	}

	private int GetHeightUnicode(MultilinesFontInfo info)
	{
		int num = 0;
		while (info != null)
		{
			num = ((!IsUsingHTML) ? (num + info.MaxHeight) : (num + 18));
			info = info.Next;
		}
		return num;
	}

	public int GetHeightUnicode(byte font, string str, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || string.IsNullOrEmpty(str))
		{
			return 0;
		}
		if (width <= 0)
		{
			width = GetWidthUnicode(font, str.AsSpan());
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, str, str.Length, align, flags, width);
		int num = 0;
		while (multilinesFontInfo != null)
		{
			num = ((!IsUsingHTML) ? (num + multilinesFontInfo.MaxHeight) : (num + 18));
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		return num;
	}

	public unsafe int CalculateCaretPosUnicode(byte font, string str, int x, int y, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (x < 0 || y < 0 || font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || string.IsNullOrEmpty(str))
		{
			return align switch
			{
				TEXT_ALIGN_TYPE.TS_CENTER => width >> 1, 
				TEXT_ALIGN_TYPE.TS_RIGHT => width, 
				_ => 0, 
			};
		}
		if (width == 0)
		{
			width = GetWidthUnicode(font, str.AsSpan());
		}
		if (x >= width)
		{
			return str.Length;
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, str, str.Length, align, flags, width);
		if (multilinesFontInfo == null)
		{
			return align switch
			{
				TEXT_ALIGN_TYPE.TS_CENTER => width >> 1, 
				TEXT_ALIGN_TYPE.TS_RIGHT => width, 
				_ => 0, 
			};
		}
		int num = 0;
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		int num2 = 0;
		bool flag = false;
		int num3 = width;
		while (multilinesFontInfo != null)
		{
			num += multilinesFontInfo.MaxHeight;
			switch (multilinesFontInfo.Align)
			{
			case TEXT_ALIGN_TYPE.TS_CENTER:
				width = num3 - multilinesFontInfo.Width >> 1;
				if (width < 0)
				{
					width = 0;
				}
				break;
			case TEXT_ALIGN_TYPE.TS_RIGHT:
				width = num3;
				break;
			default:
				width = 0;
				break;
			}
			if (!flag)
			{
				if (y < num)
				{
					int charCount = multilinesFontInfo.CharCount;
					for (int i = 0; i < charCount && i < multilinesFontInfo.Data.Count; i++)
					{
						char item = multilinesFontInfo.Data[i].Item;
						uint num4 = ptr[(int)item];
						if (item != '\r' && num4 != 0 && num4 != uint.MaxValue)
						{
							byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)num4);
							width += (sbyte)(*ptr2) + (sbyte)ptr2[2] + 1;
						}
						else if (item == ' ')
						{
							width += 8;
						}
						if (width > x)
						{
							break;
						}
						num2++;
					}
					flag = true;
				}
				else
				{
					num2 += multilinesFontInfo.CharCount;
					num2++;
				}
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		if (num2 > str.Length)
		{
			num2 = str.Length;
		}
		return num2;
	}

	public unsafe (int, int) GetCaretPosUnicode(byte font, string str, int pos, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		int num = 0;
		int num2 = 0;
		switch (align)
		{
		case TEXT_ALIGN_TYPE.TS_CENTER:
			num = width >> 1;
			break;
		case TEXT_ALIGN_TYPE.TS_RIGHT:
			num = width;
			break;
		}
		if (font >= 20 || _unicodeFontAddress[font] == IntPtr.Zero || string.IsNullOrEmpty(str))
		{
			return (num, num2);
		}
		if (width == 0)
		{
			width = GetWidthUnicode(font, str.AsSpan());
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, str, str.Length, align, flags, width);
		if (multilinesFontInfo == null)
		{
			return (num, num2);
		}
		uint* ptr = (uint*)(void*)_unicodeFontAddress[font];
		while (multilinesFontInfo != null)
		{
			switch (multilinesFontInfo.Align)
			{
			case TEXT_ALIGN_TYPE.TS_CENTER:
				num = width - multilinesFontInfo.Width >> 1;
				if (num < 0)
				{
					num = 0;
				}
				break;
			case TEXT_ALIGN_TYPE.TS_RIGHT:
				num = width;
				break;
			default:
				num = 0;
				break;
			}
			int charCount = multilinesFontInfo.CharCount;
			if (multilinesFontInfo.CharStart == pos)
			{
				return (num, num2);
			}
			if (pos <= multilinesFontInfo.CharStart + charCount && multilinesFontInfo.Data.Count >= charCount)
			{
				for (int i = 0; i < charCount; i++)
				{
					char item = multilinesFontInfo.Data[i].Item;
					uint num3 = ptr[(int)item];
					if (item != '\r' && num3 != 0 && num3 != uint.MaxValue)
					{
						byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)num3);
						num += (sbyte)(*ptr2) + (sbyte)ptr2[2] + 1;
					}
					else if (item == ' ')
					{
						num += 8;
					}
					if (multilinesFontInfo.CharStart + i + 1 == pos)
					{
						return (num, num2);
					}
				}
			}
			else
			{
				num = width;
			}
			if (multilinesFontInfo.Next != null)
			{
				num2 += multilinesFontInfo.MaxHeight;
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		return (num, num2);
	}

	public int CalculateCaretPosASCII(byte font, string str, int x, int y, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		if (font >= FontCount || x < 0 || y < 0 || string.IsNullOrEmpty(str))
		{
			return align switch
			{
				TEXT_ALIGN_TYPE.TS_CENTER => width >> 1, 
				TEXT_ALIGN_TYPE.TS_RIGHT => width, 
				_ => 0, 
			};
		}
		if (width <= 0)
		{
			width = GetWidthASCII(font, str);
		}
		if (x >= width)
		{
			return str.Length;
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoASCII(font, str, str.Length, align, flags, width);
		if (multilinesFontInfo == null)
		{
			return align switch
			{
				TEXT_ALIGN_TYPE.TS_CENTER => width >> 1, 
				TEXT_ALIGN_TYPE.TS_RIGHT => width, 
				_ => 0, 
			};
		}
		int num = 0;
		int num2 = 0;
		bool flag = false;
		int num3 = width;
		while (multilinesFontInfo != null)
		{
			num += multilinesFontInfo.MaxHeight;
			switch (multilinesFontInfo.Align)
			{
			case TEXT_ALIGN_TYPE.TS_CENTER:
				width = num3 - multilinesFontInfo.Width >> 1;
				if (width < 0)
				{
					width = 0;
				}
				break;
			case TEXT_ALIGN_TYPE.TS_RIGHT:
				width = num3;
				break;
			default:
				width = 0;
				break;
			}
			if (!flag)
			{
				if (y < num)
				{
					int charCount = multilinesFontInfo.CharCount;
					for (int i = 0; i < charCount && i < multilinesFontInfo.Data.Count; i++)
					{
						width += _fontData[font, GetASCIIIndex(multilinesFontInfo.Data[i].Item)].Width;
						if (width > x)
						{
							break;
						}
						num2++;
					}
					flag = true;
				}
				else
				{
					num2 += multilinesFontInfo.CharCount;
					num2++;
				}
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		if (num2 > str.Length)
		{
			num2 = str.Length;
		}
		return num2;
	}

	public (int, int) GetCaretPosASCII(byte font, string str, int pos, int width, TEXT_ALIGN_TYPE align, ushort flags)
	{
		int num = 0;
		int num2 = 0;
		switch (align)
		{
		case TEXT_ALIGN_TYPE.TS_CENTER:
			num = width >> 1;
			break;
		case TEXT_ALIGN_TYPE.TS_RIGHT:
			num = width;
			break;
		}
		if (font >= FontCount || string.IsNullOrEmpty(str))
		{
			return (num, num2);
		}
		if (width == 0)
		{
			width = GetWidthASCII(font, str);
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoASCII(font, str, str.Length, align, flags, width);
		if (multilinesFontInfo == null)
		{
			return (num, num2);
		}
		while (multilinesFontInfo != null)
		{
			switch (multilinesFontInfo.Align)
			{
			case TEXT_ALIGN_TYPE.TS_CENTER:
				num = width - multilinesFontInfo.Width >> 1;
				if (num < 0)
				{
					num = 0;
				}
				break;
			case TEXT_ALIGN_TYPE.TS_RIGHT:
				num = width;
				break;
			default:
				num = 0;
				break;
			}
			int charCount = multilinesFontInfo.CharCount;
			if (multilinesFontInfo.CharStart == pos)
			{
				return (num, num2);
			}
			if (pos <= multilinesFontInfo.CharStart + charCount && multilinesFontInfo.Data.Count >= charCount)
			{
				for (int i = 0; i < charCount; i++)
				{
					num += _fontData[font, GetASCIIIndex(multilinesFontInfo.Data[i].Item)].Width;
					if (multilinesFontInfo.CharStart + i + 1 == pos)
					{
						return (num, num2);
					}
				}
			}
			else
			{
				num = width;
			}
			if (multilinesFontInfo.Next != null)
			{
				num2 += multilinesFontInfo.MaxHeight;
			}
			MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
			multilinesFontInfo = multilinesFontInfo.Next;
			multilinesFontInfo2.Data.Clear();
			multilinesFontInfo2.Data.Count = 0u;
		}
		return (num, num2);
	}

	public int[] GetLinesCharsCountASCII(byte font, string str, TEXT_ALIGN_TYPE align, ushort flags, int width, bool countret = false, bool countspaces = false)
	{
		if (width == 0)
		{
			width = GetWidthASCII(font, str);
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoASCII(font, str, str.Length, align, flags, width, countret, countspaces);
		if (multilinesFontInfo == null)
		{
			return new int[0];
		}
		MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
		int num = 0;
		while (multilinesFontInfo != null)
		{
			multilinesFontInfo = multilinesFontInfo.Next;
			num++;
		}
		int[] array = new int[num];
		num = 0;
		while (multilinesFontInfo2 != null)
		{
			array[num++] = multilinesFontInfo2.CharCount;
			multilinesFontInfo2 = multilinesFontInfo2.Next;
		}
		return array;
	}

	public int[] GetLinesCharsCountUnicode(byte font, string str, TEXT_ALIGN_TYPE align, ushort flags, int width, bool countret = false, bool countspaces = false)
	{
		if (width == 0)
		{
			width = GetWidthUnicode(font, str.AsSpan());
		}
		MultilinesFontInfo multilinesFontInfo = GetInfoUnicode(font, str, str.Length, align, flags, width, countret, countspaces);
		if (multilinesFontInfo == null)
		{
			return new int[0];
		}
		MultilinesFontInfo multilinesFontInfo2 = multilinesFontInfo;
		int num = 0;
		while (multilinesFontInfo != null)
		{
			num++;
			multilinesFontInfo = multilinesFontInfo.Next;
		}
		int[] array = new int[num];
		num = 0;
		while (multilinesFontInfo2 != null)
		{
			array[num++] = multilinesFontInfo2.CharCount;
			multilinesFontInfo2 = multilinesFontInfo2.Next;
		}
		return array;
	}
}
