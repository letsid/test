using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal sealed class SpriteFont
{
	private class BinReader : BinaryReader
	{
		public BinReader(Stream input)
			: base(input)
		{
		}

		public BinReader(Stream input, Encoding encoding)
			: base(input, encoding)
		{
		}

		public BinReader(Stream input, Encoding encoding, bool leaveOpen)
			: base(input, encoding, leaveOpen)
		{
		}

		internal new int Read7BitEncodedInt()
		{
			return base.Read7BitEncodedInt();
		}
	}

	public ReadOnlyCollection<char> Characters { get; }

	public char? DefaultCharacter { get; }

	public int LineSpacing { get; }

	public float Spacing { get; }

	public Texture2D Texture { get; }

	public List<Rectangle> GlyphData { get; }

	public List<Rectangle> CroppingData { get; }

	public List<Vector3> Kerning { get; }

	public List<char> CharacterMap { get; }

	private SpriteFont(Texture2D texture, List<Rectangle> glyph, List<Rectangle> cropping, List<char> characters, int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
	{
		Characters = new ReadOnlyCollection<char>(characters.ToArray());
		DefaultCharacter = defaultCharacter;
		LineSpacing = lineSpacing;
		Spacing = spacing;
		Texture = texture;
		GlyphData = glyph;
		CroppingData = cropping;
		Kerning = kerning;
		CharacterMap = characters;
	}

	public Vector2 MeasureString(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (text.Length == 0)
		{
			return Vector2.Zero;
		}
		Vector2 zero = Vector2.Zero;
		float num = 0f;
		float num2 = LineSpacing;
		bool flag = true;
		foreach (char c in text)
		{
			switch (c)
			{
			case '\n':
				zero.X = Math.Max(zero.X, num);
				zero.Y += LineSpacing;
				num = 0f;
				num2 = LineSpacing;
				flag = true;
				continue;
			case '\r':
				continue;
			}
			int num3 = CharacterMap.IndexOf(c);
			if (num3 == -1)
			{
				num3 = (DefaultCharacter.HasValue ? CharacterMap.IndexOf(DefaultCharacter.Value) : CharacterMap.IndexOf('?'));
			}
			Vector3 vector = Kerning[num3];
			if (flag)
			{
				num += Math.Abs(vector.X);
				flag = false;
			}
			else
			{
				num += Spacing + vector.X;
			}
			num += vector.Y + vector.Z;
			int height = CroppingData[num3].Height;
			if ((float)height > num2)
			{
				num2 = height;
			}
		}
		zero.X = Math.Max(zero.X, num);
		zero.Y += num2;
		return zero;
	}

	public static SpriteFont Create(string name)
	{
		using BinReader binReader = new BinReader(typeof(SpriteFont).Assembly.GetManifestResourceStream(name));
		binReader.ReadByte();
		binReader.ReadByte();
		binReader.ReadByte();
		binReader.ReadChar();
		byte b = binReader.ReadByte();
		binReader.ReadByte();
		if (b != 5 && b != 4)
		{
			throw new ContentLoadException("Invalid XNB version");
		}
		binReader.ReadInt32();
		int num = binReader.Read7BitEncodedInt();
		for (int i = 0; i < num; i++)
		{
			binReader.ReadString();
			binReader.ReadInt32();
		}
		binReader.Read7BitEncodedInt();
		binReader.Read7BitEncodedInt();
		binReader.Read7BitEncodedInt();
		int num2 = binReader.ReadInt32();
		int num3 = binReader.ReadInt32();
		int num4 = binReader.ReadInt32();
		binReader.ReadInt32();
		int count = binReader.ReadInt32();
		byte[] array = null;
		int width = num3;
		int height = num4;
		array = binReader.ReadBytes(count);
		if (num2 != 0)
		{
			array = DecompressDxt3(array, width, height);
			count = array.Length;
		}
		Texture2D texture2D = new Texture2D(Client.Game.GraphicsDevice, num3, num4, mipMap: false, SurfaceFormat.Color);
		texture2D.SetData(array);
		binReader.Read7BitEncodedInt();
		int num5 = binReader.ReadInt32();
		List<Rectangle> list = new List<Rectangle>(num5);
		for (int j = 0; j < num5; j++)
		{
			int x = binReader.ReadInt32();
			int y = binReader.ReadInt32();
			int width2 = binReader.ReadInt32();
			int height2 = binReader.ReadInt32();
			list.Add(new Rectangle(x, y, width2, height2));
		}
		binReader.Read7BitEncodedInt();
		int num6 = binReader.ReadInt32();
		List<Rectangle> list2 = new List<Rectangle>(num6);
		for (int k = 0; k < num6; k++)
		{
			int x2 = binReader.ReadInt32();
			int y2 = binReader.ReadInt32();
			int width3 = binReader.ReadInt32();
			int height3 = binReader.ReadInt32();
			list2.Add(new Rectangle(x2, y2, width3, height3));
		}
		binReader.Read7BitEncodedInt();
		int num7 = binReader.ReadInt32();
		List<char> list3 = new List<char>(num7);
		for (int l = 0; l < num7; l++)
		{
			list3.Add(binReader.ReadChar());
		}
		int lineSpacing = binReader.ReadInt32();
		float spacing = binReader.ReadSingle();
		binReader.Read7BitEncodedInt();
		int num8 = binReader.ReadInt32();
		List<Vector3> list4 = new List<Vector3>(num6);
		for (int m = 0; m < num8; m++)
		{
			float x3 = binReader.ReadSingle();
			float y3 = binReader.ReadSingle();
			float z = binReader.ReadSingle();
			list4.Add(new Vector3(x3, y3, z));
		}
		char? defaultCharacter = null;
		if (binReader.ReadBoolean())
		{
			defaultCharacter = binReader.ReadChar();
		}
		return new SpriteFont(texture2D, list, list2, list3, lineSpacing, spacing, list4, defaultCharacter);
	}

	private static byte[] DecompressDxt3(byte[] imageData, int width, int height)
	{
		using MemoryStream imageStream = new MemoryStream(imageData);
		return DecompressDxt3(imageStream, width, height);
	}

	internal static byte[] DecompressDxt3(Stream imageStream, int width, int height)
	{
		byte[] array = new byte[width * height * 4];
		using BinaryReader imageReader = new BinaryReader(imageStream);
		int num = width + 3 >> 2;
		int num2 = height + 3 >> 2;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				DecompressDxt3Block(imageReader, j, i, num, width, height, array);
			}
		}
		return array;
	}

	private static void ConvertRgb565ToRgb888(ushort color, out byte r, out byte g, out byte b)
	{
		int num = (color >> 11) * 255 + 16;
		r = (byte)((num / 32 + num) / 32);
		num = ((color & 0x7E0) >> 5) * 255 + 32;
		g = (byte)((num / 64 + num) / 64);
		num = (color & 0x1F) * 255 + 16;
		b = (byte)((num / 32 + num) / 32);
	}

	private static void DecompressDxt3Block(BinaryReader imageReader, int x, int y, int blockCountX, int width, int height, byte[] imageData)
	{
		byte b = imageReader.ReadByte();
		byte b2 = imageReader.ReadByte();
		byte b3 = imageReader.ReadByte();
		byte b4 = imageReader.ReadByte();
		byte b5 = imageReader.ReadByte();
		byte b6 = imageReader.ReadByte();
		byte b7 = imageReader.ReadByte();
		byte b8 = imageReader.ReadByte();
		ushort color = imageReader.ReadUInt16();
		ushort color2 = imageReader.ReadUInt16();
		ConvertRgb565ToRgb888(color, out var r, out var g, out var b9);
		ConvertRgb565ToRgb888(color2, out var r2, out var g2, out var b10);
		uint num = imageReader.ReadUInt32();
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				byte b11 = 0;
				byte b12 = 0;
				byte b13 = 0;
				byte b14 = 0;
				uint num3 = (num >> 2 * (4 * i + j)) & 3;
				switch (num2)
				{
				case 0:
					b14 = (byte)((b & 0xF) | ((b & 0xF) << 4));
					break;
				case 1:
					b14 = (byte)((b & 0xF0) | ((b & 0xF0) >> 4));
					break;
				case 2:
					b14 = (byte)((b2 & 0xF) | ((b2 & 0xF) << 4));
					break;
				case 3:
					b14 = (byte)((b2 & 0xF0) | ((b2 & 0xF0) >> 4));
					break;
				case 4:
					b14 = (byte)((b3 & 0xF) | ((b3 & 0xF) << 4));
					break;
				case 5:
					b14 = (byte)((b3 & 0xF0) | ((b3 & 0xF0) >> 4));
					break;
				case 6:
					b14 = (byte)((b4 & 0xF) | ((b4 & 0xF) << 4));
					break;
				case 7:
					b14 = (byte)((b4 & 0xF0) | ((b4 & 0xF0) >> 4));
					break;
				case 8:
					b14 = (byte)((b5 & 0xF) | ((b5 & 0xF) << 4));
					break;
				case 9:
					b14 = (byte)((b5 & 0xF0) | ((b5 & 0xF0) >> 4));
					break;
				case 10:
					b14 = (byte)((b6 & 0xF) | ((b6 & 0xF) << 4));
					break;
				case 11:
					b14 = (byte)((b6 & 0xF0) | ((b6 & 0xF0) >> 4));
					break;
				case 12:
					b14 = (byte)((b7 & 0xF) | ((b7 & 0xF) << 4));
					break;
				case 13:
					b14 = (byte)((b7 & 0xF0) | ((b7 & 0xF0) >> 4));
					break;
				case 14:
					b14 = (byte)((b8 & 0xF) | ((b8 & 0xF) << 4));
					break;
				case 15:
					b14 = (byte)((b8 & 0xF0) | ((b8 & 0xF0) >> 4));
					break;
				}
				num2++;
				switch (num3)
				{
				case 0u:
					b11 = r;
					b12 = g;
					b13 = b9;
					break;
				case 1u:
					b11 = r2;
					b12 = g2;
					b13 = b10;
					break;
				case 2u:
					b11 = (byte)((2 * r + r2) / 3);
					b12 = (byte)((2 * g + g2) / 3);
					b13 = (byte)((2 * b9 + b10) / 3);
					break;
				case 3u:
					b11 = (byte)((r + 2 * r2) / 3);
					b12 = (byte)((g + 2 * g2) / 3);
					b13 = (byte)((b9 + 2 * b10) / 3);
					break;
				}
				int num4 = (x << 2) + j;
				int num5 = (y << 2) + i;
				if (num4 < width && num5 < height)
				{
					int num6 = num5 * width + num4 << 2;
					imageData[num6] = b11;
					imageData[num6 + 1] = b12;
					imageData[num6 + 2] = b13;
					imageData[num6 + 3] = b14;
				}
			}
		}
	}
}
