using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal class UOFontRenderer
{
	private struct SpriteInfo
	{
		public Texture2D Texture;

		public char Char;

		public Rectangle UV;

		public FontSettings Settings;
	}

	private struct FontHeader
	{
		public byte Width;

		public byte Height;

		public byte Unknown;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct CharacterInfo
	{
		public byte Width;

		public byte Height;

		public unsafe void* Data;
	}

	private const int ASCII_CHARS_COUNT = 224;

	private const int DEFAULT_SPACE_SIZE = 8;

	private readonly TextureAtlas _atlas;

	private UOFile _asciiFontFile;

	private UOFile[] _unicodeFontFiles = new UOFile[20];

	private PixelPicker _picker = new PixelPicker();

	private readonly Dictionary<uint, SpriteInfo> _spriteKeyInfo = new Dictionary<uint, SpriteInfo>();

	private CharacterInfo[,] _asciiCharsInfo;

	private int _asciiFontCount;

	private static readonly int[] _offsetCharTable = new int[10] { 2, 0, 2, 2, 0, 0, 2, 2, 0, 0 };

	private static readonly int[] _offsetSymbolTable = new int[10] { 1, 0, 1, 1, -1, 0, 1, 1, 0, 0 };

	public UOFontRenderer(GraphicsDevice device)
	{
		_atlas = new TextureAtlas(device, 1024, 1024, SurfaceFormat.Color);
		string uOFilePath = UOFileManager.GetUOFilePath("fonts.mul");
		if (File.Exists(uOFilePath))
		{
			_asciiFontFile = new UOFile(uOFilePath, loadFile: true);
			_asciiFontCount = GetFontCount(_asciiFontFile, 224);
		}
		for (int i = 0; i < 20; i++)
		{
			uOFilePath = UOFileManager.GetUOFilePath($"unifont{((i == 0) ? string.Empty : i.ToString())}.mul");
			if (File.Exists(uOFilePath))
			{
				_unicodeFontFiles[i] = new UOFile(uOFilePath, loadFile: true);
			}
		}
	}

	public void Draw(UltimaBatcher2D batcher, ReadOnlySpan<char> text, Vector2 position, float scale, in FontSettings settings, Vector3 hue, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT)
	{
		bool mouseIsOver;
		float maxHeight;
		Vector2 vector = MeasureStringAdvanced(text, in settings, scale, position, out mouseIsOver, out maxHeight);
		FixVectorColor(ref hue, in settings);
		if (mouseIsOver)
		{
			hue.X = 53f;
			FixVectorColor(ref hue, in settings);
		}
		Vector2 vector2 = position;
		if (align == TEXT_ALIGN_TYPE.TS_CENTER)
		{
			vector2.X += vector.X / 2f;
		}
		else
		{
			_ = 2;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\r')
			{
				continue;
			}
			if (text[i] == '\n')
			{
				position.X = vector2.X;
				position.Y += maxHeight;
				continue;
			}
			if (text[i] == ' ')
			{
				position.X += 8f * scale;
				continue;
			}
			Rectangle uv;
			uint key;
			Texture2D texture2D = ReadChar(text[i], in settings, out uv, out key);
			if (texture2D != null)
			{
				batcher.Draw(texture2D, position, uv, hue, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				position.X += (float)uv.Width * scale;
			}
		}
		if (settings.Underline)
		{
			Vector2 end = vector2;
			end.X += vector.X;
			vector2.Y += vector.Y;
			end.Y += vector.Y;
			Texture2D texture = SolidColorTextureCache.GetTexture(Color.White);
			float num = 1f;
			if (settings.Border)
			{
				Vector2 position2 = vector2;
				position2.X -= num * scale;
				position2.Y -= num * scale;
				batcher.Draw(sourceRectangle: new Rectangle(0, 0, (int)((end.X + num * scale - position2.X) / scale), (int)((end.Y + num * 2f * scale - position2.Y) / scale)), texture: texture, position: position2, color: new Vector3(0f, 1f, 0f), rotation: 0f, origin: Vector2.Zero, scale: scale, effects: SpriteEffects.None, layerDepth: 0f);
			}
			batcher.DrawLine(texture, vector2, end, hue, num * scale);
		}
	}

	public Vector2 MeasureString(ReadOnlySpan<char> text, in FontSettings settings, float scale)
	{
		Vector2 result = default(Vector2);
		int num = 0;
		float val = 0f;
		float num2 = 0f;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] != '\r')
			{
				Rectangle uv;
				uint key;
				if (text[i] == '\n')
				{
					num++;
					val = result.X;
					result.X = 0f;
				}
				else if (text[i] == ' ')
				{
					result.X += 8f * scale;
				}
				else if (ReadChar(text[i], in settings, out uv, out key) != null)
				{
					num2 = Math.Max(num2, uv.Height);
					result.X += (float)uv.Width * scale;
				}
			}
		}
		result.X = Math.Max(result.X, val);
		result.Y += (float)(num + 1) * num2;
		return result;
	}

	public Vector2 MeasureStringAdvanced(ReadOnlySpan<char> text, in FontSettings settings, float scale, Vector2 position, out bool mouseIsOver, out float maxHeight)
	{
		Vector2 result = default(Vector2);
		int num = 0;
		float val = 0f;
		ReadChar('W', in settings, out var uv, out var key);
		maxHeight = uv.Height;
		ReadChar('g', in settings, out uv, out key);
		maxHeight = Math.Max(maxHeight, uv.Height);
		maxHeight += 1f;
		maxHeight *= scale;
		Point position2 = Mouse.Position;
		mouseIsOver = false;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\r')
			{
				continue;
			}
			uint key2;
			if (text[i] == '\n')
			{
				num++;
				val = result.X;
				result.X = 0f;
			}
			else if (text[i] == ' ')
			{
				result.X += 8f * scale;
			}
			else if (ReadChar(text[i], in settings, out uv, out key2) != null)
			{
				maxHeight = Math.Max(maxHeight, uv.Height);
				if (!mouseIsOver)
				{
					mouseIsOver = _picker.Get(key2, (int)(((float)position2.X - (position.X + result.X)) / scale), (int)(((float)position2.Y - (position.Y + maxHeight * (float)num)) / scale));
				}
				result.X += (float)uv.Width * scale;
			}
		}
		result.X = Math.Max(result.X, val);
		result.Y += (float)(num + 1) * maxHeight;
		return result;
	}

	private Texture2D ReadChar(char c, in FontSettings settings, out Rectangle uv, out uint key)
	{
		if (!settings.IsUnicode)
		{
			return ReadCharASCII(c, in settings, out uv, out key);
		}
		return ReadCharUnicode(c, in settings, out uv, out key);
	}

	private unsafe Texture2D ReadCharUnicode(char c, in FontSettings settings, out Rectangle uv, out uint key)
	{
		key = CreateKey(c, in settings);
		if (_spriteKeyInfo.TryGetValue(key, out var value))
		{
			uv = value.UV;
			return value.Texture;
		}
		uv = Rectangle.Empty;
		uint* ptr = (uint*)(void*)_unicodeFontFiles[settings.FontIndex].StartAddress;
		if (c == '\r')
		{
			return null;
		}
		if ((ptr[(int)c] == 0 || ptr[(int)c] == uint.MaxValue) && c != ' ')
		{
			c = '?';
		}
		bool italic = settings.Italic;
		bool bold = settings.Bold;
		bool underline = settings.Underline;
		bool border = settings.Border;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		Point zero = Point.Zero;
		if (border)
		{
			zero.X++;
			zero.Y++;
		}
		if (italic)
		{
			zero.X += 3;
		}
		if (bold)
		{
			zero.X++;
			zero.Y++;
		}
		if (underline)
		{
			zero.Y++;
		}
		int num6 = num2;
		byte* ptr2 = (byte*)(void*)((IntPtr)ptr + (int)ptr[(int)c]);
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		if (c == ' ')
		{
			num9 = 8;
			num10 = num5;
			num2 += 8;
		}
		else
		{
			num7 = (sbyte)(*ptr2) + 1;
			num8 = (sbyte)ptr2[1];
			num9 = ptr2[2];
			num10 = ptr2[3];
		}
		if (num9 <= 0 || num10 <= 0)
		{
			return null;
		}
		num3 = num9 + num7 + zero.X;
		num4 = num10 + num8 + zero.Y;
		Span<uint> pixels = stackalloc uint[num3 * num4];
		if (c != ' ')
		{
			ptr2 += 4;
			int num11 = (num9 - 1 >> 3) + 1;
			for (int i = 0; i < num10; i++)
			{
				int num12 = num8 + num + i;
				if (num12 < 0)
				{
					num12 = 0;
				}
				if (num12 >= num4)
				{
					break;
				}
				byte* ptr3 = ptr2;
				ptr2 += num11;
				int num13 = 0;
				if (italic)
				{
					num13 = (int)((float)(num10 - i) / 3.3f);
				}
				int num14 = num2 + num7 + num13 + (bold ? 1 : 0);
				for (int j = 0; j < num11; j++)
				{
					int num15 = j << 3;
					for (int k = 0; k < 8; k++)
					{
						int num16 = num15 + k;
						if (num16 >= num9)
						{
							break;
						}
						int num17 = num14 + num16;
						if (num17 >= num3)
						{
							break;
						}
						byte num18 = (byte)(ptr3[j] & (1 << 7 - k));
						int index = num12 * num3 + num17;
						if (num18 != 0)
						{
							pixels[index] = uint.MaxValue;
						}
					}
				}
			}
			if (bold)
			{
				uint num19 = 4278255873u;
				if (num19 == uint.MaxValue)
				{
					num19++;
				}
				int num20 = ((num2 + num7 > 0) ? (-1) : 0);
				int num21 = ((num2 + num7 + num9 < num3) ? 1 : 0);
				num21 += num9;
				for (int l = 0; l < num10; l++)
				{
					int num22 = num8 + num + l;
					if (num22 >= num4)
					{
						break;
					}
					if (num22 < 0)
					{
						num22 = 0;
					}
					int num23 = 0;
					if (italic && l < num10)
					{
						num23 = (int)((float)(num10 - l) / 3.3f);
					}
					for (int m = num20; m < num21; m++)
					{
						int num24 = m + num2 + num7 + num23;
						if (num24 >= num3)
						{
							break;
						}
						int index2 = num22 * num3 + num24;
						if (pixels[index2] != 0 || pixels[index2] == num19)
						{
							continue;
						}
						int num25 = ((m >= num9) ? 1 : 2);
						if (num25 == 2 && num24 + 1 >= num3)
						{
							num25--;
						}
						for (int n = 0; n < num25; n++)
						{
							int num26 = num24 + n;
							int index3 = num22 * num3 + num26;
							if (pixels[index3] != 0 && pixels[index3] != num19)
							{
								pixels[index2] = num19;
								break;
							}
						}
					}
				}
				for (int num27 = 0; num27 < num10; num27++)
				{
					int num28 = num8 + num + num27;
					if (num28 >= num4)
					{
						break;
					}
					if (num28 < 0)
					{
						num28 = 0;
					}
					int num29 = 0;
					if (italic)
					{
						num29 = (int)((float)(num10 - num27) / 3.3f);
					}
					for (int num30 = 0; num30 < num9; num30++)
					{
						int num31 = num30 + num2 + num7 + num29;
						if (num31 >= num3)
						{
							break;
						}
						int index4 = num28 * num3 + num31;
						if (pixels[index4] == num19)
						{
							pixels[index4] = uint.MaxValue;
						}
					}
				}
			}
			if (border)
			{
				int num32 = ((num2 + num7 > 0) ? (-1) : 0);
				int num33 = ((num8 > 0) ? (-1) : 0);
				int num34 = ((num2 + num7 + num9 < num3) ? 1 : 0);
				int num35 = ((num8 + num + num10 < num4) ? 1 : 0);
				num34 += num9;
				num35 += num10;
				for (int num36 = num33; num36 < num35; num36++)
				{
					int num37 = num8 + num36;
					if (num37 < 0)
					{
						num37 = 0;
					}
					if (num37 >= num4)
					{
						break;
					}
					int num38 = 0;
					if (italic && num36 >= 0 && num36 < num10)
					{
						num38 = (int)((float)(num10 - num36) / 3.3f);
					}
					for (int num39 = num32; num39 < num34; num39++)
					{
						int num40 = num39 + num2 + num7 + num38;
						if (num40 >= num3)
						{
							break;
						}
						int index5 = num37 * num3 + num40;
						if (pixels[index5] != 0 || pixels[index5] == 4278255873u)
						{
							continue;
						}
						int num41 = ((num39 > 0) ? (-1) : 0);
						int num42 = ((num36 > 0) ? (-1) : 0);
						int num43 = ((num39 >= num9 - 1) ? 1 : 2);
						int num44 = ((num36 >= num10 - 1) ? 1 : 2);
						if (num43 == 2 && num40 + 1 >= num3)
						{
							num43--;
						}
						bool flag = false;
						int num45 = num3 * num4;
						for (int num46 = num41; num46 < num43; num46++)
						{
							int num47 = num40 + num46;
							for (int num48 = num42; num48 < num44; num48++)
							{
								int num49 = (num37 + num48) * num3 + num47;
								if (num49 >= 0 && num49 < num45 && pixels[num49] != 0 && pixels[num49] != 4278255873u)
								{
									pixels[index5] = 4278255873u;
									flag = true;
									break;
								}
							}
							if (flag)
							{
								break;
							}
						}
					}
				}
			}
			num2 += num9 + num7 + (bold ? 1 : 0) + 4;
		}
		value = default(SpriteInfo);
		value.Char = c;
		value.Settings = settings;
		value.Texture = _atlas.AddSprite(pixels, num3, num4, out value.UV);
		_spriteKeyInfo[key] = value;
		_picker.Set(key, num3, num4, pixels);
		uv = value.UV;
		return value.Texture;
	}

	private unsafe Texture2D ReadCharASCII(char c, in FontSettings settings, out Rectangle uv, out uint key)
	{
		if (settings.IsUnicode || settings.FontIndex >= _asciiFontCount)
		{
			key = 0u;
			uv = Rectangle.Empty;
			return null;
		}
		if (c > 'ÿ')
		{
			c = '?';
		}
		key = CreateKey(c, in settings);
		if (_spriteKeyInfo.TryGetValue(key, out var value))
		{
			uv = value.UV;
			return value.Texture;
		}
		int num = 0;
		int num2 = 0;
		ref CharacterInfo reference = ref _asciiCharsInfo[settings.FontIndex, GetASCIIIndex(c)];
		int width = reference.Width;
		int height = reference.Height;
		int num3 = width;
		int num4 = height;
		int num5 = 0;
		if (num3 <= 0 || num4 <= 0)
		{
			uv = Rectangle.Empty;
			return null;
		}
		num4++;
		Span<uint> pixels = stackalloc uint[num3 * num4];
		int fontOffsetY = GetFontOffsetY(settings.FontIndex, (byte)c);
		for (int i = 0; i < height; i++)
		{
			int num6 = i + num + fontOffsetY;
			if (num6 >= num4)
			{
				break;
			}
			int num7 = i * width;
			for (int j = 0; j < width; j++)
			{
				if (j + num2 >= num3)
				{
					num += num5;
					num2 = 0;
					break;
				}
				ushort num8 = *(ushort*)((byte*)reference.Data + (nint)(num7 + j) * (nint)2);
				if (num8 != 0)
				{
					uint num9 = HuesHelper.Color16To32(num8) | 0xFF000000u;
					int num10 = num6 * num3 + j + num2;
					if (num10 >= 0)
					{
						pixels[num10] = num9;
					}
				}
			}
		}
		num2 += width;
		value = default(SpriteInfo);
		value.Char = c;
		value.Settings = settings;
		value.Texture = _atlas.AddSprite(pixels, num3, num4, out value.UV);
		_spriteKeyInfo[key] = value;
		_picker.Set(key, num3, num4, pixels);
		uv = value.UV;
		return value.Texture;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FixVectorColor(ref Vector3 color, in FontSettings settings)
	{
		if (color.X != 0f)
		{
			if (settings.IsUnicode)
			{
				color.Y = 3f;
			}
			else if (settings.FontIndex != 8 && settings.FontIndex != 5)
			{
				color.Y = 2f;
			}
			else
			{
				color.Y = 1f;
			}
		}
		else
		{
			color.Y = 0f;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint CreateKey(char c, in FontSettings settings)
	{
		return (uint)((uint)((int)((uint)((int)((uint)((int)((uint)((int)((uint)((int)((uint)((int)(527u + (int)c) * 31) + settings.FontIndex.GetHashCode()) * 31) + settings.Bold.GetHashCode()) * 31) + settings.Italic.GetHashCode()) * 31) + settings.Border.GetHashCode()) * 31) + settings.IsHtml.GetHashCode()) * 31) + settings.IsUnicode.GetHashCode());
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetASCIIIndex(char c)
	{
		byte b = (byte)c;
		if (b < 32)
		{
			return 0;
		}
		return b - 32;
	}

	private static int GetFontOffsetY(byte font, byte index)
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

	private unsafe int GetFontCount(UOFile file, int charactersCount)
	{
		file.Seek(0);
		bool flag = false;
		int num = 0;
		int num2 = sizeof(FontHeader);
		while (file.Position < file.Length)
		{
			file.Skip(1);
			for (int i = 0; i < charactersCount; i++)
			{
				FontHeader* ptr = (FontHeader*)(void*)file.PositionAddress;
				if (file.Position + num2 < file.Length)
				{
					file.Skip(num2);
					int num3 = ptr->Width * ptr->Height * 2;
					if (file.Position + num3 > file.Length)
					{
						flag = true;
						break;
					}
					file.Skip(num3);
				}
			}
			if (flag)
			{
				break;
			}
			num++;
		}
		_asciiCharsInfo = new CharacterInfo[num, 224];
		file.Seek(0);
		for (int j = 0; j < num; j++)
		{
			file.ReadByte();
			for (int k = 0; k < charactersCount; k++)
			{
				if (file.Position + 3 < file.Length)
				{
					ref CharacterInfo reference = ref _asciiCharsInfo[j, k];
					reference.Width = file.ReadByte();
					reference.Height = file.ReadByte();
					file.Skip(1);
					reference.Data = (void*)file.PositionAddress;
					file.Skip(reference.Width * reference.Height * 2);
				}
			}
		}
		return num;
	}
}
