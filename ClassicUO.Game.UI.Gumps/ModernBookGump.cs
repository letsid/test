using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class ModernBookGump : Gump
{
	private class StbPageTextBox : StbTextBox
	{
		private static readonly StringBuilder _sb = new StringBuilder();

		private static string[] _handler;

		private readonly ModernBookGump _gump;

		internal int _caretPage;

		internal int _focusPage;

		internal readonly int[,] _pageCoords;

		internal readonly string[] _pageLines;

		internal readonly bool[] _pagesChanged;

		internal bool _ServerUpdate;

		internal Point _caretPos => _caretScreenPosition;

		internal RenderedText renderedText => _rendererText;

		internal RenderedText renderedCaret => _rendererCaret;

		public StbPageTextBox(byte font, int bookpages, ModernBookGump gump, int max_char_count = -1, int maxWidth = 0, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0)
			: base(font, max_char_count, maxWidth, isunicode, style, hue)
		{
			_pageCoords = new int[bookpages, 2];
			_pageLines = new string[bookpages * 8];
			_pagesChanged = new bool[bookpages + 1];
			Priority = ClickPriority.High;
			_gump = gump;
		}

		internal int GetCaretPage()
		{
			Point caretPosition = _rendererText.GetCaretPosition(base.CaretIndex);
			int i = 0;
			for (int length = _pageCoords.GetLength(0); i < length; i++)
			{
				if (caretPosition.Y >= _pageCoords[i, 0] && caretPosition.Y < _pageCoords[i, 0] + _pageCoords[i, 1])
				{
					return i;
				}
			}
			return 0;
		}

		protected override void OnMouseDown(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				if (base.IsEditable)
				{
					SetKeyboardFocus();
				}
				if (!base.NoSelection)
				{
					_leftWasDown = true;
				}
				if (_focusPage >= 0 && _focusPage < _pageCoords.GetLength(0))
				{
					x = ((_focusPage % 2 != 0) ? (x - (38 + _gump.X)) : (x - (223 + _gump.X)));
					y += _pageCoords[_focusPage, 0] - (34 + _gump.Y);
				}
				base.Stb.Click(x, y);
				UpdateCaretScreenPosition();
				_caretPage = GetCaretPage();
			}
		}

		protected override void OnMouseOver(int x, int y)
		{
			if (_leftWasDown)
			{
				if (_focusPage >= 0 && _focusPage < _pageCoords.GetLength(0))
				{
					x = ((_focusPage % 2 != 0) ? (x - (38 + _gump.X)) : (x - (223 + _gump.X)));
					y += _pageCoords[_focusPage, 0] - (34 + _gump.Y);
				}
				base.Stb.Drag(x, y);
			}
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (_focusPage >= 0 && _focusPage < _pageCoords.GetLength(0))
			{
				x = ((_focusPage % 2 != 0) ? (x - (38 + _gump.X)) : (x - (223 + _gump.X)));
				y += _pageCoords[_focusPage, 0] - (34 + _gump.Y);
			}
			base.OnMouseUp(x, y, button);
		}

		internal void UpdatePageCoords()
		{
			MultilinesFontInfo multilinesFontInfo = _rendererText.GetInfo();
			int i = 0;
			int num = 0;
			for (; i < _pageCoords.GetLength(0); i++)
			{
				_pageCoords[i, 0] = num;
				_pageCoords[i, 1] = 0;
				for (int j = 0; j < 8; j++)
				{
					if (multilinesFontInfo == null)
					{
						break;
					}
					_pageCoords[i, 1] += multilinesFontInfo.MaxHeight;
					multilinesFontInfo = multilinesFontInfo.Next;
				}
				num += _pageCoords[i, 1];
			}
		}

		internal void DrawSelection(UltimaBatcher2D batcher, int x, int y, int starty, int endy)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 0.5f);
			int num = Math.Min(base.Stb.SelectStart, base.Stb.SelectEnd);
			int num2 = Math.Max(base.Stb.SelectStart, base.Stb.SelectEnd);
			if (num >= num2)
			{
				return;
			}
			MultilinesFontInfo multilinesFontInfo = _rendererText.GetInfo();
			int num3 = 1;
			int num4 = 0;
			while (multilinesFontInfo != null && num < num2)
			{
				if (num >= num4 && num < num4 + multilinesFontInfo.CharCount)
				{
					int num5 = num - num4;
					int num6 = 0;
					for (int i = 0; i < num5; i++)
					{
						num6 += _rendererText.GetCharWidth(multilinesFontInfo.Data[i].Item);
					}
					if (num2 >= num4 && num2 < num4 + multilinesFontInfo.CharCount)
					{
						int num7 = num2 - num;
						int num8 = 0;
						for (int j = 0; j < num7; j++)
						{
							num8 += _rendererText.GetCharWidth(multilinesFontInfo.Data[num5 + j].Item);
						}
						if (num3 >= starty && num3 <= endy)
						{
							batcher.Draw(SolidColorTextureCache.GetTexture(StbTextBox.SELECTION_COLOR), new Rectangle(x + num6, y + num3 - starty, num8, multilinesFontInfo.MaxHeight + 1), hueVector);
						}
						break;
					}
					if (num3 >= starty && num3 <= endy)
					{
						batcher.Draw(SolidColorTextureCache.GetTexture(StbTextBox.SELECTION_COLOR), new Rectangle(x + num6, y + num3 - starty, multilinesFontInfo.Width - num6, multilinesFontInfo.MaxHeight + 1), hueVector);
					}
					num = num4 + multilinesFontInfo.CharCount;
				}
				num4 += multilinesFontInfo.CharCount;
				num3 += multilinesFontInfo.MaxHeight;
				multilinesFontInfo = multilinesFontInfo.Next;
			}
		}

		protected override void OnTextChanged()
		{
			_is_writing = true;
			if (!_ServerUpdate)
			{
				if (_handler == null || _handler.Length < _pageLines.Length)
				{
					_handler = new string[_pageLines.Length];
				}
				string[] array = base.Text.Split('\n');
				int i = 0;
				int num = 0;
				for (; i < array.Length; i++)
				{
					if (num >= _pageLines.Length)
					{
						break;
					}
					if (array[i].Length > 0)
					{
						int num2 = 0;
						int num3 = 0;
						int charWidth = _rendererText.GetCharWidth(array[i][num2]);
						while (true)
						{
							if (num3 + charWidth > _rendererText.MaxWidth)
							{
								_handler[num] = _sb.ToString();
								_sb.Clear();
								num++;
								num3 = 0;
								if (num >= _pageLines.Length)
								{
									break;
								}
							}
							num3 += charWidth;
							_sb.Append(array[i][num2]);
							num2++;
							if (num2 >= array[i].Length)
							{
								_sb.Append('\n');
								_handler[num] = _sb.ToString();
								_sb.Clear();
								num++;
								break;
							}
							charWidth = _rendererText.GetCharWidth(array[i][num2]);
						}
					}
					else
					{
						_handler[num] = "\n";
						num++;
					}
				}
				_sb.Clear();
				for (int j = 0; j < _pageLines.Length; j++)
				{
					if (!_pagesChanged[(j >> 3) + 1] && _handler[j] != _pageLines[j])
					{
						_pagesChanged[(j >> 3) + 1] = true;
					}
					_sb.Append(_pageLines[j] = _handler[j]);
				}
				_rendererText.Text = _sb.ToString();
				_sb.Clear();
				UpdatePageCoords();
			}
			base.OnTextChanged();
			_is_writing = false;
		}

		protected override void CloseWithRightClick()
		{
			if (_gump != null && !_gump.IsDisposed)
			{
				_gump.CloseWithRightClick();
			}
			else
			{
				base.CloseWithRightClick();
			}
		}
	}

	internal const int MAX_BOOK_LINES = 8;

	private const int MAX_BOOK_CHARS_PER_LINE = 52;

	private const int LEFT_X = 38;

	private const int RIGHT_X = 223;

	private const int UPPER_MARGIN = 34;

	private const int PAGE_HEIGHT = 166;

	private StbPageTextBox _bookPage;

	private GumpPic _forwardGumpPic;

	private GumpPic _backwardGumpPic;

	private StbTextBox _titleTextBox;

	private StbTextBox _authorTextBox;

	internal string[] BookLines => _bookPage._pageLines;

	internal bool[] _pagesChanged => _bookPage._pagesChanged;

	public ushort BookPageCount { get; internal set; }

	public HashSet<int> KnownPages { get; internal set; } = new HashSet<int>();

	public static bool IsNewBook => Client.Version > ClientVersion.CV_200;

	public bool UseNewHeader { get; set; } = true;

	public static byte DefaultFont => (byte)(IsNewBook ? 1u : 4u);

	public bool IntroChanges => _pagesChanged[0];

	internal int MaxPage => (BookPageCount >> 1) + 1;

	public ModernBookGump(uint serial, ushort page_count, string title, string author, bool is_editable, bool old_packet)
		: base(serial, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		BookPageCount = page_count;
		base.IsEditable = is_editable;
		UseNewHeader = !old_packet;
		BuildGump(title, author, serial);
	}

	internal void ServerSetBookText()
	{
		if (BookLines == null || BookLines.Length == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int charWidth = _bookPage.renderedText.GetCharWidth(' ');
		int i = 0;
		for (int num = BookLines.Length; i < num; i++)
		{
			if (BookLines[i] != null && BookLines[i].Contains("\n"))
			{
				BookLines[i] = BookLines[i].Replace("\n", "");
			}
		}
		int j = 0;
		for (int num2 = BookLines.Length; j < num2; j++)
		{
			int num3 = (IsNewBook ? FontsLoader.Instance.GetWidthUnicode(_bookPage.renderedText.Font, BookLines[j]) : FontsLoader.Instance.GetWidthASCII(_bookPage.renderedText.Font, BookLines[j]));
			if (BookLines[j] == null)
			{
				stringBuilder.Append('\n');
				continue;
			}
			stringBuilder.Append(BookLines[j]);
			if (j + 1 < num2 && ((string.IsNullOrWhiteSpace(BookLines[j]) && !BookLines[j].Contains("\n")) || num3 + charWidth <= _bookPage.renderedText.MaxWidth))
			{
				stringBuilder.Append('\n');
				BookLines[j] += "\n";
			}
		}
		_bookPage._ServerUpdate = true;
		_bookPage.SetText(stringBuilder.ToString());
		_bookPage.CaretIndex = 0;
		_bookPage.UpdatePageCoords();
		_bookPage._ServerUpdate = false;
	}

	private void BuildGump(string title, string author, uint serial)
	{
		base.CanCloseWithRightClick = true;
		ushort graphicID = World.Get(serial).Graphic;
		ushort hue = World.Get(serial).Hue;
		CustomBook customBook = new CustomBook();
		if (graphicID == 0)
		{
			return;
		}
		Tuple<ushort, CustomBook> tuple = GumpsLoader.Instance.BooksDefList.FirstOrDefault((Tuple<ushort, CustomBook> bd) => bd.Item1 == graphicID);
		if (tuple == null)
		{
			customBook.GumpID = 510;
			customBook.TextCol = 0;
		}
		else
		{
			customBook = tuple.Item2;
		}
		Add(new GumpPic(0, 0, customBook.GumpID, 0)
		{
			CanMove = true,
			IsPartialHue = true,
			Hue = hue
		});
		Add(_backwardGumpPic = new GumpPic(0, 0, (ushort)(customBook.GumpID + 1), 0)
		{
			IsPartialHue = true,
			Hue = hue
		});
		Add(_forwardGumpPic = new GumpPic(356, 0, (ushort)(customBook.GumpID + 2), 0)
		{
			IsPartialHue = true,
			Hue = hue
		});
		_forwardGumpPic.MouseUp += delegate(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtonType.Left && sender is Control)
			{
				SetActivePage(base.ActivePage + 1);
			}
		};
		_forwardGumpPic.MouseDoubleClick += delegate(object? sender, MouseDoubleClickEventArgs e)
		{
			if (e.Button == MouseButtonType.Left && sender is Control)
			{
				SetActivePage(MaxPage);
			}
		};
		_backwardGumpPic.MouseUp += delegate(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtonType.Left && sender is Control)
			{
				SetActivePage(base.ActivePage - 1);
			}
		};
		_backwardGumpPic.MouseDoubleClick += delegate(object? sender, MouseDoubleClickEventArgs e)
		{
			if (e.Button == MouseButtonType.Left && sender is Control)
			{
				SetActivePage(1);
			}
		};
		StbPageTextBox stbPageTextBox = new StbPageTextBox(DefaultFont, BookPageCount, this, 416 * BookPageCount, 165, IsNewBook, FontStyle.ExtraHeight, 1);
		stbPageTextBox.X = 0;
		stbPageTextBox.Y = 0;
		stbPageTextBox.Height = 166 * BookPageCount;
		stbPageTextBox.Width = 165;
		stbPageTextBox.IsEditable = base.IsEditable;
		stbPageTextBox.Multiline = true;
		_bookPage = stbPageTextBox;
		StbTextBox stbTextBox = new StbTextBox(DefaultFont, 47, 150, IsNewBook, FontStyle.None, 0);
		stbTextBox.X = 40;
		stbTextBox.Y = 60;
		stbTextBox.Height = 25;
		stbTextBox.Width = 155;
		stbTextBox.IsEditable = base.IsEditable;
		StbTextBox c = stbTextBox;
		_titleTextBox = stbTextBox;
		Add(c, 1);
		_titleTextBox.SetText(title);
		_titleTextBox.TextChanged += PageZero_TextChanged;
		Label label = new Label(ResGumps.By, isunicode: true, 1);
		label.X = 40;
		label.Y = 130;
		Add(label, 1);
		StbTextBox stbTextBox2 = new StbTextBox(DefaultFont, 29, 150, IsNewBook, FontStyle.None, 0);
		stbTextBox2.X = 40;
		stbTextBox2.Y = 160;
		stbTextBox2.Height = 25;
		stbTextBox2.Width = 155;
		stbTextBox2.IsEditable = base.IsEditable;
		c = stbTextBox2;
		_authorTextBox = stbTextBox2;
		Add(c, 1);
		_authorTextBox.SetText(author);
		_authorTextBox.TextChanged += PageZero_TextChanged;
		int i = 1;
		int num = 38;
		for (; i <= BookPageCount; i++)
		{
			num = ((i % 2 != 1) ? 38 : 223);
			int num2 = i + 1;
			if (num2 % 2 == 1)
			{
				num2++;
			}
			num2 >>= 1;
			Label label2 = new Label(i.ToString(), isunicode: true, 1);
			label2.X = num + 80;
			label2.Y = 200;
			Add(label2, num2);
		}
		base.ActivePage = 1;
		UpdatePageButtonVisibility();
		Client.Game.Scene.Audio.PlaySound(85);
	}

	private void PageZero_TextChanged(object sender, EventArgs e)
	{
		_pagesChanged[0] = true;
	}

	private void UpdatePageButtonVisibility()
	{
		if (base.ActivePage == 1)
		{
			_backwardGumpPic.IsVisible = false;
			_forwardGumpPic.IsVisible = true;
		}
		else if (base.ActivePage == MaxPage)
		{
			_forwardGumpPic.IsVisible = false;
			_backwardGumpPic.IsVisible = true;
		}
		else
		{
			_backwardGumpPic.IsVisible = true;
			_forwardGumpPic.IsVisible = true;
		}
	}

	public void SetTile(string title, bool editable)
	{
		_titleTextBox.SetText(title);
		_titleTextBox.IsEditable = editable;
	}

	public void SetAuthor(string author, bool editable)
	{
		_authorTextBox.SetText(author);
		_authorTextBox.IsEditable = editable;
	}

	private void SetActivePage(int page)
	{
		page = Math.Min(Math.Max(page, 1), MaxPage);
		if (page != base.ActivePage)
		{
			Client.Game.Scene.Audio.PlaySound(85);
		}
		if (!base.IsEditable)
		{
			int num = page - 1 << 1;
			int num2 = num + 1;
			if (num > 0 && !KnownPages.Contains(num))
			{
				NetClient.Socket.Send_BookPageDataRequest(base.LocalSerial, (ushort)num);
			}
			if (num2 < MaxPage * 2 && !KnownPages.Contains(num2))
			{
				NetClient.Socket.Send_BookPageDataRequest(base.LocalSerial, (ushort)num2);
			}
		}
		else
		{
			for (int i = 0; i < _pagesChanged.Length; i++)
			{
				if (!_pagesChanged[i])
				{
					continue;
				}
				_pagesChanged[i] = false;
				if (i < 1)
				{
					if (UseNewHeader)
					{
						NetClient.Socket.Send_BookHeaderChanged(base.LocalSerial, _titleTextBox.Text, _authorTextBox.Text);
					}
					else
					{
						NetClient.Socket.Send_BookHeaderChanged_Old(base.LocalSerial, _titleTextBox.Text, _authorTextBox.Text);
					}
					continue;
				}
				string[] array = new string[8];
				int num3 = (i - 1) * 8;
				int num4 = 0;
				while (num3 < (i - 1) * 8 + 8)
				{
					array[num4] = BookLines[num3];
					num3++;
					num4++;
				}
				NetClient.Socket.Send_BookPageData(base.LocalSerial, array, i);
			}
		}
		base.ActivePage = page;
		UpdatePageButtonVisibility();
		if (UIManager.KeyboardFocusControl == null || (UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl && UIManager.KeyboardFocusControl != _bookPage && page != _bookPage._focusPage / 2 + 1))
		{
			UIManager.SystemChat.TextBoxControl.SetKeyboardFocus();
		}
	}

	public override void OnButtonClick(int buttonID)
	{
	}

	protected override void CloseWithRightClick()
	{
		SetActivePage(0);
		base.CloseWithRightClick();
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		if (batcher.ClipBegin(x, y, base.Width, base.Height))
		{
			RenderedText renderedText = _bookPage.renderedText;
			int num = (base.ActivePage - 1) * 2;
			if (num < BookPageCount)
			{
				int num2 = _bookPage._pageCoords[num, 0];
				int num3 = _bookPage._pageCoords[num, 1];
				_bookPage.DrawSelection(batcher, x + 223, y + 34, num2, num2 + num3);
				renderedText.Draw(batcher, x + 223, y + 34, 0, num2, renderedText.Width, num3);
				if (num == _bookPage._caretPage)
				{
					if (_bookPage._caretPos.Y < num2 + num3)
					{
						if (_bookPage._caretPos.Y >= num2)
						{
							if (_bookPage.HasKeyboardFocus)
							{
								_bookPage.renderedCaret.Draw(batcher, _bookPage._caretPos.X + x + 223, _bookPage._caretPos.Y + y + 34 - num2, 0, 0, _bookPage.renderedCaret.Width, _bookPage.renderedCaret.Height);
							}
						}
						else
						{
							_bookPage._caretPage = _bookPage.GetCaretPage();
						}
					}
					else if (_bookPage._caretPos.Y <= _bookPage.Height && base.IsEditable && _bookPage._caretPage + 2 < _bookPage._pagesChanged.Length)
					{
						_bookPage._focusPage = _bookPage._caretPage++;
						SetActivePage(_bookPage._caretPage / 2 + 2);
					}
				}
			}
			num--;
			if (num > 0)
			{
				int num4 = _bookPage._pageCoords[num, 0];
				int num5 = _bookPage._pageCoords[num, 1];
				_bookPage.DrawSelection(batcher, x + 38, y + 34, num4, num4 + num5);
				renderedText.Draw(batcher, x + 38, y + 34, 0, num4, renderedText.Width, num5);
				if (num == _bookPage._caretPage)
				{
					if (_bookPage._caretPos.Y < num4 + num5)
					{
						if (_bookPage._caretPos.Y >= num4)
						{
							if (_bookPage.HasKeyboardFocus)
							{
								_bookPage.renderedCaret.Draw(batcher, _bookPage._caretPos.X + x + 38, _bookPage._caretPos.Y + y + 34 - num4, 0, 0, _bookPage.renderedCaret.Width, _bookPage.renderedCaret.Height);
							}
						}
						else if (_bookPage._caretPage > 0)
						{
							_bookPage._focusPage = _bookPage._caretPage--;
							SetActivePage(_bookPage._caretPage / 2 + 1);
						}
					}
					else if (_bookPage._caretPos.Y <= _bookPage.Height && _bookPage._caretPage + 2 < _bookPage._pagesChanged.Length)
					{
						_bookPage._caretPage++;
					}
				}
			}
			batcher.ClipEnd();
		}
		return true;
	}

	public override void Dispose()
	{
		base.Dispose();
		_bookPage?.Dispose();
	}

	public override void OnHitTestSuccess(int x, int y, ref Control res)
	{
		if (!base.IsDisposed)
		{
			int num = -1;
			if (base.ActivePage > 1 && x >= 38 + base.X && x <= 38 + base.X + _bookPage.Width)
			{
				num = (base.ActivePage - 1) * 2 - 1;
			}
			else if (base.ActivePage - 1 < BookPageCount >> 1 && x >= 223 + base.X && x <= 223 + _bookPage.Width + base.X)
			{
				num = (base.ActivePage - 1) * 2;
			}
			if (num >= 0 && num < BookPageCount && y >= 34 + base.Y && y <= 200 + base.Y)
			{
				_bookPage._focusPage = num;
				res = _bookPage;
			}
		}
	}
}
