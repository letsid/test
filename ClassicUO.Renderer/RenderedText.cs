using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StbTextEditSharp;

namespace ClassicUO.Renderer;

internal sealed class RenderedText
{
	private static readonly QueuedPool<RenderedText> _pool = new QueuedPool<RenderedText>(3000, delegate(RenderedText r)
	{
		r.IsDestroyed = false;
		r.Links.Clear();
	});

	private static PixelPicker _picker = new PixelPicker();

	private byte _font;

	private MultilinesFontInfo _info;

	private string _text;

	public bool IsUnicode { get; set; }

	public byte Font
	{
		get
		{
			return _font;
		}
		set
		{
			if (value == byte.MaxValue)
			{
				value = ((Client.Version >= ClientVersion.CV_305D) ? ((byte)1) : ((byte)0));
			}
			_font = value;
		}
	}

	public TEXT_ALIGN_TYPE Align { get; set; }

	public int MaxWidth { get; set; }

	public int MaxHeight { get; set; }

	public FontStyle FontStyle { get; set; }

	public byte Cell { get; set; }

	public bool IsHTML { get; set; }

	public bool RecalculateWidthByInfo { get; set; }

	public List<WebLinkRect> Links { get; set; } = new List<WebLinkRect>();

	public ushort Hue { get; set; }

	public uint HTMLColor { get; set; } = uint.MaxValue;

	public bool HasBackgroundColor { get; set; }

	public string Text
	{
		get
		{
			return _text;
		}
		set
		{
			if (!(_text != value))
			{
				return;
			}
			_text = value;
			if (string.IsNullOrEmpty(value))
			{
				Width = 0;
				Height = 0;
				if (IsHTML)
				{
					FontsLoader.Instance.SetUseHTML(value: false);
				}
				Links.Clear();
				Texture?.Dispose();
				Texture = null;
				_info = null;
			}
			else
			{
				CreateTexture();
				if (IsUnicode)
				{
					_info = FontsLoader.Instance.GetInfoUnicode(Font, Text, Text.Length, Align, (ushort)FontStyle, (MaxWidth > 0) ? MaxWidth : Width, countret: true, countspaces: true);
				}
				else
				{
					_info = FontsLoader.Instance.GetInfoASCII(Font, Text, Text.Length, Align, (ushort)FontStyle, (MaxWidth > 0) ? MaxWidth : Width, countret: true, countspaces: true);
				}
			}
		}
	}

	public int LinesCount { get; set; }

	public bool SaveHitMap { get; private set; }

	public bool IsDestroyed { get; private set; }

	public int Width { get; private set; }

	public int Height { get; private set; }

	public Texture2D Texture { get; set; }

	public static RenderedText Create(string text, ushort hue = ushort.MaxValue, byte font = byte.MaxValue, bool isunicode = true, FontStyle style = FontStyle.None, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT, int maxWidth = 0, byte cell = 30, bool isHTML = false, bool recalculateWidthByInfo = false, bool saveHitmap = false)
	{
		if ((style & FontStyle.ForcedUnicode) != 0)
		{
			cell = 21;
		}
		RenderedText one = _pool.GetOne();
		one.Hue = hue;
		one.Font = font;
		one.IsUnicode = isunicode;
		one.FontStyle = style;
		one.Cell = cell;
		one.Align = align;
		one.MaxWidth = maxWidth;
		one.IsHTML = isHTML;
		one.RecalculateWidthByInfo = recalculateWidthByInfo;
		one.Width = 0;
		one.Height = 0;
		one.SaveHitMap = saveHitmap;
		one.HTMLColor = uint.MaxValue;
		one.HasBackgroundColor = false;
		if (one.Text != text)
		{
			one.Text = text;
		}
		else
		{
			one.CreateTexture();
		}
		return one;
	}

	public Point GetCaretPosition(int caret_index)
	{
		Point result = default(Point);
		if (IsUnicode)
		{
			ref int x = ref result.X;
			ref int y = ref result.Y;
			(x, y) = FontsLoader.Instance.GetCaretPosUnicode(Font, Text, caret_index, MaxWidth, Align, (ushort)FontStyle);
		}
		else
		{
			ref int x2 = ref result.X;
			ref int y = ref result.Y;
			(x2, y) = FontsLoader.Instance.GetCaretPosASCII(Font, Text, caret_index, MaxWidth, Align, (ushort)FontStyle);
		}
		return result;
	}

	public MultilinesFontInfo GetInfo()
	{
		return _info;
	}

	public bool PixelCheck(int x, int y)
	{
		if (string.IsNullOrWhiteSpace(Text))
		{
			return false;
		}
		ushort num = Hue;
		if (!IsUnicode && SaveHitMap)
		{
			num = 32767;
		}
		ulong textureID = (ulong)(int)((uint)(Text.GetHashCode() ^ num) ^ (uint)Align ^ (uint)FontStyle ^ Font ^ (IsUnicode ? 1u : 0u));
		return _picker.Get(textureID, x, y);
	}

	public TextEditRow GetLayoutRow(int startIndex)
	{
		TextEditRow result = default(TextEditRow);
		if (string.IsNullOrEmpty(Text))
		{
			return result;
		}
		MultilinesFontInfo multilinesFontInfo = _info;
		if (multilinesFontInfo == null)
		{
			return result;
		}
		switch (Align)
		{
		case TEXT_ALIGN_TYPE.TS_LEFT:
			result.x0 = 0f;
			result.x1 = Width;
			break;
		case TEXT_ALIGN_TYPE.TS_CENTER:
			result.x0 = Width - multilinesFontInfo.Width >> 1;
			if (result.x0 < 0f)
			{
				result.x0 = 0f;
			}
			result.x1 = result.x0;
			break;
		case TEXT_ALIGN_TYPE.TS_RIGHT:
			result.x0 = Width;
			break;
		}
		int num = 0;
		while (multilinesFontInfo != null)
		{
			if (startIndex >= num && startIndex < num + multilinesFontInfo.CharCount)
			{
				result.num_chars = multilinesFontInfo.CharCount;
				result.ymax = multilinesFontInfo.MaxHeight;
				result.baseline_y_delta = multilinesFontInfo.MaxHeight;
				break;
			}
			num += multilinesFontInfo.CharCount;
			multilinesFontInfo = multilinesFontInfo.Next;
		}
		return result;
	}

	public int GetCharWidthAtIndex(int index)
	{
		if (string.IsNullOrEmpty(Text))
		{
			return 0;
		}
		MultilinesFontInfo multilinesFontInfo = _info;
		int num = 0;
		while (multilinesFontInfo != null)
		{
			if (index >= num && index < num + multilinesFontInfo.CharCount)
			{
				int num2 = index - num;
				if (num2 >= 0)
				{
					char c = ((num2 >= multilinesFontInfo.Data.Count) ? '\n' : multilinesFontInfo.Data[num2].Item);
					if (IsUnicode)
					{
						return FontsLoader.Instance.GetCharWidthUnicode(Font, c);
					}
					return FontsLoader.Instance.GetCharWidthASCII(Font, c);
				}
			}
			num += multilinesFontInfo.CharCount;
			multilinesFontInfo = multilinesFontInfo.Next;
		}
		return 0;
	}

	public int GetCharWidth(char c)
	{
		if (IsUnicode)
		{
			return FontsLoader.Instance.GetCharWidthUnicode(Font, c);
		}
		return FontsLoader.Instance.GetCharWidthASCII(Font, c);
	}

	public bool Draw(UltimaBatcher2D batcher, int swidth, int sheight, int dx, int dy, int dwidth, int dheight, int offsetX, int offsetY, ushort hue = 0)
	{
		if (string.IsNullOrEmpty(Text) || Texture == null || IsDestroyed || Texture.IsDisposed)
		{
			return false;
		}
		if (offsetX > swidth || offsetX < -swidth || offsetY > sheight || offsetY < -sheight)
		{
			return false;
		}
		int num;
		if (offsetX + dwidth <= swidth)
		{
			num = dwidth;
		}
		else
		{
			num = swidth - offsetX;
			dwidth = num;
		}
		int num2;
		if (offsetY + dheight <= sheight)
		{
			num2 = dheight;
		}
		else
		{
			num2 = sheight - offsetY;
			dheight = num2;
		}
		if (!IsUnicode && SaveHitMap && hue == 0)
		{
			hue = Hue;
		}
		if (hue > 0)
		{
			hue--;
		}
		Vector3 color = new Vector3((int)hue, 0f, 1f);
		if (hue != 0)
		{
			if (IsUnicode)
			{
				color.Y = 3f;
			}
			else if (Font != 5 && Font != 8)
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
		batcher.Draw(Texture, new Rectangle(dx, dy, dwidth, dheight), new Rectangle(offsetX, offsetY, num, num2), color);
		return true;
	}

	public bool Draw(UltimaBatcher2D batcher, int dx, int dy, int sx, int sy, int swidth, int sheight, int hue = -1)
	{
		if (string.IsNullOrEmpty(Text) || Texture == null || IsDestroyed || Texture.IsDisposed)
		{
			return false;
		}
		if (sx > Texture.Width || sy > Texture.Height)
		{
			return false;
		}
		if (!IsUnicode && SaveHitMap && hue == -1)
		{
			hue = Hue;
		}
		if (hue > 0)
		{
			hue--;
		}
		Vector3 color = new Vector3(hue, 0f, 1f);
		if (hue != -1)
		{
			color.X = hue;
			if (hue != 0)
			{
				if (IsUnicode)
				{
					color.Y = 3f;
				}
				else if (Font != 5 && Font != 8)
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
		batcher.Draw(Texture, new Vector2(dx, dy), new Rectangle(sx, sy, swidth, sheight), color);
		return true;
	}

	public bool Draw(UltimaBatcher2D batcher, int x, int y, float alpha = 1f, ushort hue = 0)
	{
		if (string.IsNullOrEmpty(Text) || Texture == null || IsDestroyed || Texture.IsDisposed)
		{
			return false;
		}
		if (!IsUnicode && SaveHitMap && hue == 0)
		{
			hue = Hue;
		}
		if (hue > 0)
		{
			hue--;
		}
		Vector3 color = new Vector3((int)hue, 0f, alpha);
		if (hue != 0)
		{
			if (IsUnicode)
			{
				color.Y = 3f;
			}
			else if (Font != 5 && Font != 8)
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
		batcher.Draw(Texture, new Rectangle(x, y, Width, Height), color);
		return true;
	}

	public void CreateTexture()
	{
		if (Texture != null && !Texture.IsDisposed)
		{
			Texture.Dispose();
			Texture = null;
		}
		if (IsHTML)
		{
			FontsLoader.Instance.SetUseHTML(value: true, HTMLColor, HasBackgroundColor);
		}
		FontsLoader.Instance.RecalculateWidthByInfo = RecalculateWidthByInfo;
		if (IsUnicode)
		{
			FontsLoader.Instance.GenerateUnicode(this, Font, Text, Hue, Cell, MaxWidth, Align, (ushort)FontStyle, SaveHitMap, MaxHeight, _picker);
		}
		else
		{
			FontsLoader.Instance.GenerateASCII(this, Font, Text, Hue, MaxWidth, Align, (ushort)FontStyle, SaveHitMap, MaxHeight, _picker);
		}
		if (Texture != null)
		{
			Width = Texture.Width;
			Height = Texture.Height;
		}
		if (IsHTML)
		{
			FontsLoader.Instance.SetUseHTML(value: false);
		}
		FontsLoader.Instance.RecalculateWidthByInfo = false;
	}

	public void Destroy()
	{
		if (!IsDestroyed)
		{
			IsDestroyed = true;
			if (Texture != null && !Texture.IsDisposed)
			{
				Texture.Dispose();
			}
			_pool.ReturnOne(this);
		}
	}
}
