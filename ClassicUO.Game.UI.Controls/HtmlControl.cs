using System.Collections.Generic;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class HtmlControl : Control
{
	private RenderedText _gameText;

	private ScrollBarBase _scrollBar;

	public bool HasScrollbar { get; }

	public bool HasBackground { get; }

	public bool UseFlagScrollbar { get; }

	public int ScrollX { get; set; }

	public int ScrollY { get; set; }

	public string Text
	{
		get
		{
			return _gameText.Text;
		}
		set
		{
			_gameText.Text = value;
		}
	}

	public HtmlControl(List<string> parts, string[] lines)
		: this()
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = int.Parse(parts[3]);
		base.Height = int.Parse(parts[4]);
		int num = int.Parse(parts[5]);
		HasBackground = parts[6] == "1";
		HasScrollbar = parts[7] != "0";
		UseFlagScrollbar = HasScrollbar && parts[7] == "2";
		_gameText.IsHTML = true;
		_gameText.MaxWidth = base.Width - (HasScrollbar ? 16 : 0) - (HasBackground ? 8 : 0);
		base.IsFromServer = true;
		if (num >= 0 && num < lines.Length)
		{
			InternalBuild(lines[num], 0);
		}
	}

	public HtmlControl(int x, int y, int w, int h, bool hasbackground, bool hasscrollbar, bool useflagscrollbar = false, string text = "", int hue = 0, bool ishtml = false, byte font = 1, bool isunicode = true, FontStyle style = FontStyle.None, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT)
		: this()
	{
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		HasBackground = hasbackground;
		HasScrollbar = hasscrollbar;
		UseFlagScrollbar = useflagscrollbar;
		if (!string.IsNullOrEmpty(text))
		{
			_gameText.IsHTML = ishtml;
			_gameText.FontStyle = style;
			_gameText.Align = align;
			_gameText.Font = font;
			_gameText.IsUnicode = isunicode;
			_gameText.MaxWidth = w - (HasScrollbar ? 16 : 0) - (HasBackground ? 8 : 0);
		}
		InternalBuild(text, hue);
	}

	public HtmlControl()
	{
		_gameText = RenderedText.Create(string.Empty, ushort.MaxValue, 1, isunicode: true, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
		CanMove = true;
	}

	private void InternalBuild(string text, int hue)
	{
		if (!string.IsNullOrEmpty(text))
		{
			if (_gameText.IsHTML)
			{
				uint hTMLColor = uint.MaxValue;
				ushort hue2 = 0;
				if (hue > 0)
				{
					hTMLColor = ((hue != 16777215 && hue != 65535 && hue != 255) ? ((HuesHelper.Color16To32((ushort)hue) << 8) | 0xFF) : 4294967294u);
				}
				else if (!HasBackground)
				{
					hue2 = ushort.MaxValue;
					if (!HasScrollbar)
					{
						hTMLColor = 16843263u;
					}
				}
				else
				{
					_gameText.MaxWidth -= 9;
					hTMLColor = 16843263u;
				}
				_gameText.HTMLColor = hTMLColor;
				_gameText.Hue = hue2;
			}
			else
			{
				_gameText.Hue = (ushort)hue;
			}
			_gameText.HasBackgroundColor = !HasBackground;
			_gameText.Text = text;
		}
		if (HasBackground)
		{
			ResizePic resizePic = new ResizePic(9350);
			resizePic.Width = base.Width - (HasScrollbar ? 16 : 0);
			resizePic.Height = base.Height;
			resizePic.AcceptMouseInput = false;
			Add(resizePic);
		}
		if (HasScrollbar)
		{
			if (UseFlagScrollbar)
			{
				_scrollBar = new ScrollFlag
				{
					Location = new Point(base.Width - 14, 0)
				};
			}
			else
			{
				_scrollBar = new ScrollBar(base.Width - 14, 0, base.Height);
			}
			_scrollBar.Height = base.Height;
			_scrollBar.MinValue = 0;
			_scrollBar.MaxValue = _gameText.Height - base.Height + (HasBackground ? 8 : 0);
			ScrollY = _scrollBar.Value;
			Add(_scrollBar);
		}
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		if (HasScrollbar)
		{
			switch (delta)
			{
			case MouseEventType.WheelScrollUp:
				_scrollBar.Value -= _scrollBar.ScrollStep;
				break;
			case MouseEventType.WheelScrollDown:
				_scrollBar.Value += _scrollBar.ScrollStep;
				break;
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (HasScrollbar)
		{
			if (base.WantUpdateSize)
			{
				_scrollBar.Height = base.Height;
				_scrollBar.MinValue = 0;
				_scrollBar.MaxValue = _gameText.Height - base.Height + (HasBackground ? 8 : 0);
				base.WantUpdateSize = false;
			}
			ScrollY = _scrollBar.Value;
		}
		base.Update(totalTime, frameTime);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		if (batcher.ClipBegin(x, y, base.Width, base.Height))
		{
			base.Draw(batcher, x, y);
			int num = (HasBackground ? 4 : 0);
			_gameText.Draw(batcher, x + num, y + num, ScrollX, ScrollY, base.Width + ScrollX, base.Height + ScrollY);
			batcher.ClipEnd();
		}
		return true;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && _gameText != null)
		{
			for (int i = 0; i < _gameText.Links.Count; i++)
			{
				WebLinkRect webLinkRect = _gameText.Links[i];
				if (webLinkRect.Bounds.Contains(x, ((_scrollBar != null) ? _scrollBar.Value : 0) + y) && FontsLoader.Instance.GetWebLink(webLinkRect.LinkID, out var result))
				{
					Log.Info("LINK CLICKED: " + result.Link);
					PlatformHelper.LaunchBrowser(result.Link);
					_gameText.CreateTexture();
					break;
				}
			}
		}
		base.OnMouseUp(x, y, button);
	}

	public override void Dispose()
	{
		_gameText?.Destroy();
		_gameText = null;
		base.Dispose();
	}
}
