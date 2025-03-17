using System.Collections.Generic;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls;

internal class Label : Control
{
	private readonly RenderedText _gText;

	public string Text
	{
		get
		{
			return _gText.Text;
		}
		set
		{
			_gText.Text = value;
			base.Width = _gText.Width;
			base.Height = _gText.Height;
		}
	}

	public ushort Hue
	{
		get
		{
			return _gText.Hue;
		}
		set
		{
			if (_gText.Hue != value)
			{
				_gText.Hue = value;
				_gText.CreateTexture();
			}
		}
	}

	public byte Font => _gText.Font;

	public bool Unicode => _gText.IsUnicode;

	public Label(string text, bool isunicode, ushort hue, int maxwidth = 0, byte font = byte.MaxValue, FontStyle style = FontStyle.None, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT, bool ishtml = false)
	{
		_gText = RenderedText.Create(text, hue, font, isunicode, style, align, maxwidth, 30, ishtml);
		AcceptMouseInput = false;
		base.Width = _gText.Width;
		base.Height = _gText.Height;
	}

	public Label(List<string> parts, string[] lines)
		: this((int.TryParse(parts[4], out var result) && result >= 0 && result < lines.Length) ? lines[result] : string.Empty, isunicode: true, (ushort)(UInt16Converter.Parse(parts[3]) + 1), 0, byte.MaxValue, FontStyle.BlackBorder)
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.IsFromServer = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		_gText.Draw(batcher, x, y, base.Alpha, 0);
		return base.Draw(batcher, x, y);
	}

	public override void Dispose()
	{
		base.Dispose();
		_gText.Destroy();
	}
}
