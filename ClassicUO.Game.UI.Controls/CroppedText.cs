using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls;

internal class CroppedText : Control
{
	private readonly RenderedText _gameText;

	public CroppedText(string text, ushort hue, int maxWidth = 0)
	{
		_gameText = RenderedText.Create(text, hue, (Client.Version >= ClientVersion.CV_305D) ? ((byte)1) : ((byte)0), isunicode: true, (maxWidth > 0) ? (FontStyle.BlackBorder | FontStyle.Cropped) : FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, maxWidth, 30);
		AcceptMouseInput = false;
	}

	public CroppedText(List<string> parts, string[] lines)
		: this((int.TryParse(parts[6], out var result) && result >= 0 && result < lines.Length) ? lines[result] : string.Empty, (ushort)(UInt16Converter.Parse(parts[5]) + 1), int.Parse(parts[3]))
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = int.Parse(parts[3]);
		base.Height = int.Parse(parts[4]);
		base.IsFromServer = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		_gameText.Draw(batcher, x, y, 1f, 0);
		return base.Draw(batcher, x, y);
	}

	public override void Dispose()
	{
		base.Dispose();
		_gameText?.Destroy();
	}
}
