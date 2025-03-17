using System;
using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using SDL2;
using StbTextEditSharp;

namespace ClassicUO.Game.UI.Controls;

internal class StbTextBox : Control, ITextEditHandler
{
	protected static readonly Color SELECTION_COLOR = new Color
	{
		PackedValue = 2157994016u
	};

	private readonly FontStyle _fontStyle;

	private readonly int _maxCharCount = -1;

	protected Point _caretScreenPosition;

	protected bool _is_writing;

	protected bool _leftWasDown;

	protected bool _fromServer;

	protected RenderedText _rendererText;

	protected RenderedText _rendererCaret;

	protected TextEdit Stb { get; }

	public override bool AcceptKeyboardInput
	{
		get
		{
			if (base.AcceptKeyboardInput)
			{
				return base.IsEditable;
			}
			return false;
		}
	}

	public byte Font
	{
		get
		{
			return _rendererText.Font;
		}
		set
		{
			if (_rendererText.Font != value)
			{
				_rendererText.Font = value;
				_rendererText.CreateTexture();
				_rendererCaret.Font = value;
				_rendererCaret.CreateTexture();
				UpdateCaretScreenPosition();
			}
		}
	}

	public bool AllowTAB { get; set; }

	public bool NoSelection { get; set; }

	public int CaretIndex
	{
		get
		{
			return Stb.CursorIndex;
		}
		set
		{
			Stb.CursorIndex = value;
			UpdateCaretScreenPosition();
		}
	}

	public bool Multiline
	{
		get
		{
			return !Stb.SingleLine;
		}
		set
		{
			Stb.SingleLine = !value;
		}
	}

	public bool NumbersOnly { get; set; }

	public int SelectionStart
	{
		get
		{
			return Stb.SelectStart;
		}
		set
		{
			if (AllowSelection)
			{
				Stb.SelectStart = value;
			}
		}
	}

	public int SelectionEnd
	{
		get
		{
			return Stb.SelectEnd;
		}
		set
		{
			if (AllowSelection)
			{
				Stb.SelectEnd = value;
			}
		}
	}

	public bool AllowSelection { get; set; } = true;

	public bool IsUnicode => _rendererText.IsUnicode;

	public ushort Hue
	{
		get
		{
			return _rendererText.Hue;
		}
		set
		{
			if (_rendererText.Hue != value)
			{
				_rendererText.Hue = value;
				_rendererCaret.Hue = value;
				_rendererText.CreateTexture();
				_rendererCaret.CreateTexture();
			}
		}
	}

	internal int TotalHeight
	{
		get
		{
			int num = 20;
			for (MultilinesFontInfo multilinesFontInfo = GetInfo(); multilinesFontInfo != null; multilinesFontInfo = multilinesFontInfo.Next)
			{
				num += multilinesFontInfo.MaxHeight;
			}
			return num;
		}
	}

	public string Text
	{
		get
		{
			return _rendererText.Text;
		}
		set
		{
			if (_maxCharCount > 0)
			{
				_ = NumbersOnly;
				if (value != null && value.Length > _maxCharCount)
				{
					value = value.Substring(0, _maxCharCount);
				}
			}
			_rendererText.Text = value;
			if (!_is_writing)
			{
				OnTextChanged();
			}
		}
	}

	public int Length => Text?.Length ?? 0;

	public event EventHandler TextChanged;

	public StbTextBox(byte font, int max_char_count = -1, int maxWidth = 0, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0, TEXT_ALIGN_TYPE align = TEXT_ALIGN_TYPE.TS_LEFT)
	{
		AcceptKeyboardInput = true;
		AcceptMouseInput = true;
		CanMove = false;
		base.IsEditable = true;
		_maxCharCount = max_char_count;
		Stb = new TextEdit(this);
		Stb.SingleLine = true;
		if (maxWidth > 0)
		{
			style |= FontStyle.CropTexture;
		}
		_fontStyle = style;
		if ((style & (FontStyle.Fixed | FontStyle.Cropped)) != 0)
		{
			_ = 0;
		}
		style &= ~(FontStyle.Cropped | FontStyle.CropTexture);
		_rendererText = RenderedText.Create(string.Empty, hue, font, isunicode, style, align, maxWidth, 30);
		_rendererCaret = RenderedText.Create("_", hue, font, isunicode, ((style & FontStyle.BlackBorder) != 0) ? FontStyle.BlackBorder : FontStyle.None, align, 0, 30);
		base.Height = _rendererCaret.Height;
	}

	public StbTextBox(List<string> parts, string[] lines)
		: this(1, (parts[0] == "textentrylimited") ? int.Parse(parts[8]) : 255, int.Parse(parts[3]), isunicode: true, FontStyle.BlackBorder | FontStyle.CropTexture, (ushort)(UInt16Converter.Parse(parts[5]) + 1))
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = _rendererText.MaxWidth;
		base.Height = (_rendererText.MaxHeight = int.Parse(parts[4]));
		Multiline = false;
		_fromServer = true;
		base.LocalSerial = SerialHelper.Parse(parts[6]);
		base.IsFromServer = true;
		int num2 = int.Parse(parts[7]);
		if (num2 >= 0 && num2 < lines.Length)
		{
			SetText(lines[num2]);
		}
	}

	public float GetWidth(int index)
	{
		return _rendererText.GetCharWidthAtIndex(index);
	}

	public TextEditRow LayoutRow(int startIndex)
	{
		TextEditRow layoutRow = _rendererText.GetLayoutRow(startIndex);
		int screenCoordinateX = base.ScreenCoordinateX;
		int screenCoordinateY = base.ScreenCoordinateY;
		layoutRow.x0 += screenCoordinateX;
		layoutRow.x1 += screenCoordinateX;
		layoutRow.ymin += screenCoordinateY;
		layoutRow.ymax += screenCoordinateY;
		return layoutRow;
	}

	public MultilinesFontInfo CalculateFontInfo(string text, bool countret = true)
	{
		if (IsUnicode)
		{
			return FontsLoader.Instance.GetInfoUnicode(_rendererText.Font, text, text.Length, _rendererText.Align, (ushort)_rendererText.FontStyle, _rendererText.MaxWidth, countret);
		}
		return FontsLoader.Instance.GetInfoASCII(_rendererText.Font, text, text.Length, _rendererText.Align, (ushort)_rendererText.FontStyle, _rendererText.MaxWidth, countret);
	}

	public void SelectAll()
	{
		if (AllowSelection)
		{
			Stb.SelectStart = 0;
			Stb.SelectEnd = Length;
		}
	}

	protected void UpdateCaretScreenPosition()
	{
		_caretScreenPosition = _rendererText.GetCaretPosition(Stb.CursorIndex);
	}

	private ControlKeys ApplyShiftIfNecessary(ControlKeys k)
	{
		if (Keyboard.Shift && !NoSelection)
		{
			k |= ControlKeys.Shift;
		}
		return k;
	}

	private bool IsMaxCharReached(int count)
	{
		if (_maxCharCount >= 0)
		{
			return Length + count >= _maxCharCount;
		}
		return false;
	}

	private void Sanitize(ref string text)
	{
		if ((_fontStyle & FontStyle.Fixed) == 0 && (_fontStyle & FontStyle.Cropped) == 0 && (_fontStyle & FontStyle.CropTexture) == 0)
		{
			return;
		}
		if (_rendererText.MaxWidth == 0)
		{
			Log.Warn("maxwidth must be setted.");
		}
		else if (!string.IsNullOrEmpty(text) && (_rendererText.IsUnicode ? FontsLoader.Instance.GetWidthUnicode(_rendererText.Font, text) : FontsLoader.Instance.GetWidthASCII(_rendererText.Font, text)) > _rendererText.MaxWidth)
		{
			if ((_fontStyle & FontStyle.Fixed) != 0)
			{
				text = Text;
				Stb.CursorIndex = Math.Max(0, text.Length - 1);
			}
			else
			{
				_ = _fontStyle & FontStyle.CropTexture;
				_ = _fontStyle & FontStyle.Cropped;
			}
		}
	}

	protected virtual void OnTextChanged()
	{
		this.TextChanged?.Raise(this);
		UpdateCaretScreenPosition();
	}

	protected MultilinesFontInfo GetInfo()
	{
		return _rendererText.GetInfo();
	}

	internal override void OnFocusEnter()
	{
		base.OnFocusEnter();
		CaretIndex = Text?.Length ?? 0;
	}

	internal override void OnFocusLost()
	{
		if (Stb != null)
		{
			Stb.SelectStart = (Stb.SelectEnd = 0);
		}
		base.OnFocusLost();
	}

	protected override void OnKeyDown(SDL.SDL_Keycode key, SDL.SDL_Keymod mod)
	{
		ControlKeys? controlKeys = null;
		bool flag = false;
		if (Client.Game.Scene is GameScene gameScene && gameScene.Macros.FindMacro(key, Keyboard.Alt, Keyboard.Ctrl, Keyboard.Shift) != null)
		{
			return;
		}
		switch (key)
		{
		case SDL.SDL_Keycode.SDLK_TAB:
			if (AllowTAB)
			{
				OnTextInput("   ");
			}
			else
			{
				base.Parent?.KeyboardTabToNextFocus(this);
			}
			break;
		case SDL.SDL_Keycode.SDLK_a:
			if (Keyboard.Ctrl && !NoSelection)
			{
				SelectAll();
			}
			break;
		case SDL.SDL_Keycode.SDLK_ESCAPE:
			SelectionStart = 0;
			SelectionEnd = 0;
			break;
		case SDL.SDL_Keycode.SDLK_INSERT:
			if (base.IsEditable)
			{
				controlKeys = ControlKeys.InsertMode;
			}
			break;
		case SDL.SDL_Keycode.SDLK_c:
			if (Keyboard.Ctrl && !NoSelection)
			{
				int num = Math.Min(Stb.SelectStart, Stb.SelectEnd);
				int num2 = Math.Max(Stb.SelectStart, Stb.SelectEnd);
				if (num < num2 && num >= 0 && num2 - num <= Text.Length)
				{
					SDL.SDL_SetClipboardText(Text.Substring(num, num2 - num));
				}
			}
			break;
		case SDL.SDL_Keycode.SDLK_x:
		{
			if (!Keyboard.Ctrl || NoSelection)
			{
				break;
			}
			int num = Math.Min(Stb.SelectStart, Stb.SelectEnd);
			int num2 = Math.Max(Stb.SelectStart, Stb.SelectEnd);
			if (num < num2 && num >= 0 && num2 - num <= Text.Length)
			{
				SDL.SDL_SetClipboardText(Text.Substring(num, num2 - num));
				if (base.IsEditable)
				{
					Stb.Cut();
				}
			}
			break;
		}
		case SDL.SDL_Keycode.SDLK_v:
			if (Keyboard.Ctrl && base.IsEditable)
			{
				OnTextInput(StringHelper.GetClipboardText(Multiline));
			}
			break;
		case SDL.SDL_Keycode.SDLK_z:
			if (Keyboard.Ctrl && base.IsEditable)
			{
				controlKeys = ControlKeys.Undo;
			}
			break;
		case SDL.SDL_Keycode.SDLK_y:
			if (Keyboard.Ctrl && base.IsEditable)
			{
				controlKeys = ControlKeys.Redo;
			}
			break;
		case SDL.SDL_Keycode.SDLK_LEFT:
			if (Keyboard.Ctrl && Keyboard.Shift)
			{
				if (!NoSelection)
				{
					controlKeys = ControlKeys.WordLeft | ControlKeys.Shift;
				}
			}
			else if (!Keyboard.Shift)
			{
				controlKeys = ((!Keyboard.Ctrl) ? new ControlKeys?(ControlKeys.Left) : new ControlKeys?(ControlKeys.WordLeft));
			}
			else if (!NoSelection)
			{
				controlKeys = ControlKeys.Shift | ControlKeys.Left;
			}
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_RIGHT:
			if (Keyboard.Ctrl && Keyboard.Shift)
			{
				if (!NoSelection)
				{
					controlKeys = ControlKeys.WordRight | ControlKeys.Shift;
				}
			}
			else if (!Keyboard.Shift)
			{
				controlKeys = ((!Keyboard.Ctrl) ? new ControlKeys?(ControlKeys.Right) : new ControlKeys?(ControlKeys.WordRight));
			}
			else if (!NoSelection)
			{
				controlKeys = ControlKeys.Right | ControlKeys.Shift;
			}
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_UP:
			controlKeys = ApplyShiftIfNecessary(ControlKeys.Up);
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_DOWN:
			controlKeys = ApplyShiftIfNecessary(ControlKeys.Down);
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_BACKSPACE:
			if (base.IsEditable)
			{
				controlKeys = ApplyShiftIfNecessary(ControlKeys.BackSpace);
				flag = true;
			}
			break;
		case SDL.SDL_Keycode.SDLK_DELETE:
			if (base.IsEditable)
			{
				controlKeys = ApplyShiftIfNecessary(ControlKeys.Delete);
				flag = true;
			}
			break;
		case SDL.SDL_Keycode.SDLK_HOME:
			if (Keyboard.Ctrl && Keyboard.Shift)
			{
				if (!NoSelection)
				{
					controlKeys = ControlKeys.TextStart | ControlKeys.Shift;
				}
			}
			else if (!Keyboard.Shift)
			{
				controlKeys = ((!Keyboard.Ctrl) ? new ControlKeys?(ControlKeys.LineStart) : new ControlKeys?(ControlKeys.TextStart));
			}
			else if (!NoSelection)
			{
				controlKeys = ControlKeys.LineStart | ControlKeys.Shift;
			}
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_END:
			if (Keyboard.Ctrl && Keyboard.Shift)
			{
				if (!NoSelection)
				{
					controlKeys = ControlKeys.TextEnd | ControlKeys.Shift;
				}
			}
			else if (!Keyboard.Shift)
			{
				controlKeys = ((!Keyboard.Ctrl) ? new ControlKeys?(ControlKeys.LineEnd) : new ControlKeys?(ControlKeys.TextEnd));
			}
			else if (!NoSelection)
			{
				controlKeys = ControlKeys.LineEnd | ControlKeys.Shift;
			}
			flag = true;
			break;
		case SDL.SDL_Keycode.SDLK_RETURN:
		case SDL.SDL_Keycode.SDLK_KP_ENTER:
			if (!base.IsEditable)
			{
				break;
			}
			if (Multiline)
			{
				if (!_fromServer && !IsMaxCharReached(0))
				{
					OnTextInput("\n");
				}
				break;
			}
			base.Parent?.OnKeyboardReturn(0, Text);
			if (UIManager.SystemChat != null && UIManager.SystemChat.TextBoxControl != null && base.IsFocused)
			{
				if (!base.IsFromServer || !UIManager.SystemChat.TextBoxControl.IsVisible)
				{
					OnFocusLost();
					OnFocusEnter();
				}
				else if (UIManager.KeyboardFocusControl == null || UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
				{
					UIManager.SystemChat.TextBoxControl.SetKeyboardFocus();
				}
			}
			break;
		}
		if (controlKeys.HasValue)
		{
			Stb.Key(controlKeys.Value);
		}
		if (flag)
		{
			UpdateCaretScreenPosition();
		}
		base.OnKeyDown(key, mod);
	}

	public void SetText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			ClearText();
			return;
		}
		if (_maxCharCount > 0 && !NumbersOnly && text.Length > _maxCharCount)
		{
			text = text.Substring(0, _maxCharCount);
		}
		Stb.ClearState(!Multiline);
		Text = text;
		Stb.CursorIndex = Length;
		if (!_is_writing)
		{
			OnTextChanged();
		}
	}

	public void ClearText()
	{
		if (Length != 0)
		{
			SelectionStart = 0;
			SelectionEnd = 0;
			Stb.Delete(0, Length);
			if (!_is_writing)
			{
				OnTextChanged();
			}
		}
	}

	public void AppendText(string text)
	{
		Stb.Paste(text);
	}

	protected override void OnTextInput(string c)
	{
		if (c == null || !base.IsEditable)
		{
			return;
		}
		_is_writing = true;
		if (SelectionStart != SelectionEnd)
		{
			Stb.DeleteSelection();
		}
		int num2;
		if (_maxCharCount > 0)
		{
			int num = _maxCharCount - Length;
			if (num <= 0)
			{
				_is_writing = false;
				return;
			}
			num2 = Math.Min(num, c.Length);
			if (num < c.Length && num2 > 0)
			{
				c = c.Substring(0, num2);
			}
		}
		else
		{
			num2 = c.Length;
		}
		if (num2 > 0)
		{
			if (NumbersOnly)
			{
				for (int i = 0; i < num2; i++)
				{
					if (!char.IsNumber(c[i]))
					{
						_is_writing = false;
						return;
					}
				}
				if (_maxCharCount > 0 && int.TryParse(Stb.text + c, out var result) && result > _maxCharCount)
				{
					_is_writing = false;
					SetText(_maxCharCount.ToString());
					return;
				}
			}
			if (num2 > 1)
			{
				Stb.Paste(c);
				OnTextChanged();
			}
			else if (_rendererText.GetCharWidth(c[0]) > 0 || c[0] == '\n')
			{
				Stb.InputChar(c[0]);
				if (base.Parent is SystemChatControl)
				{
					((SystemChatControl)base.Parent).UpdateMode();
				}
				OnTextChanged();
			}
		}
		_is_writing = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (batcher.ClipBegin(x, y, base.Width, base.Height))
		{
			base.Draw(batcher, x, y);
			DrawSelection(batcher, x, y);
			_rendererText.Draw(batcher, x, y, 1f, 0);
			DrawCaret(batcher, x, y);
			batcher.ClipEnd();
		}
		return true;
	}

	private protected void DrawSelection(UltimaBatcher2D batcher, int x, int y)
	{
		if (!AllowSelection)
		{
			return;
		}
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 0.5f);
		int num = Math.Min(Stb.SelectStart, Stb.SelectEnd);
		int num2 = Math.Max(Stb.SelectStart, Stb.SelectEnd);
		if (num >= num2)
		{
			return;
		}
		MultilinesFontInfo multilinesFontInfo = _rendererText.GetInfo();
		int num3 = 1;
		int num4 = 0;
		int num5 = ((_rendererText.Align != 0) ? (_rendererText.GetCaretPosition(0).X - 1) : 0);
		while (multilinesFontInfo != null && num < num2)
		{
			if (num >= num4 && num < num4 + multilinesFontInfo.CharCount)
			{
				int num6 = num - num4;
				int num7 = 0;
				for (int i = 0; i < num6; i++)
				{
					num7 += _rendererText.GetCharWidth(multilinesFontInfo.Data[i].Item);
				}
				if (num2 >= num4 && num2 < num4 + multilinesFontInfo.CharCount)
				{
					int num8 = num2 - num;
					int num9 = 0;
					for (int j = 0; j < num8; j++)
					{
						num9 += _rendererText.GetCharWidth(multilinesFontInfo.Data[num6 + j].Item);
					}
					batcher.Draw(SolidColorTextureCache.GetTexture(SELECTION_COLOR), new Rectangle(x + num7 + num5, y + num3, num9, multilinesFontInfo.MaxHeight + 1), hueVector);
					break;
				}
				batcher.Draw(SolidColorTextureCache.GetTexture(SELECTION_COLOR), new Rectangle(x + num7 + num5, y + num3, multilinesFontInfo.Width - num7, multilinesFontInfo.MaxHeight + 1), hueVector);
				num = num4 + multilinesFontInfo.CharCount;
			}
			num4 += multilinesFontInfo.CharCount;
			num3 += multilinesFontInfo.MaxHeight;
			multilinesFontInfo = multilinesFontInfo.Next;
		}
	}

	protected virtual void DrawCaret(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.HasKeyboardFocus)
		{
			_rendererCaret.Draw(batcher, x + _caretScreenPosition.X, y + _caretScreenPosition.Y, 1f, 0);
		}
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left && base.IsEditable)
		{
			if (!NoSelection)
			{
				_leftWasDown = true;
			}
			Stb.Click(Mouse.Position.X, Mouse.Position.Y);
			UpdateCaretScreenPosition();
		}
		base.OnMouseDown(x, y, button);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			_leftWasDown = false;
		}
		base.OnMouseUp(x, y, button);
	}

	protected override void OnMouseOver(int x, int y)
	{
		base.OnMouseOver(x, y);
		if (_leftWasDown)
		{
			Stb.Drag(Mouse.Position.X, Mouse.Position.Y);
		}
	}

	public override void Dispose()
	{
		_rendererText?.Destroy();
		_rendererCaret?.Destroy();
		base.Dispose();
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (!NoSelection && CaretIndex < Text.Length && CaretIndex >= 0 && !char.IsWhiteSpace(Text[CaretIndex]))
		{
			int num = CaretIndex;
			if (num - 1 >= 0 && char.IsWhiteSpace(Text[num - 1]))
			{
				num++;
			}
			SelectionStart = Stb.MoveToPreviousWord(num);
			SelectionEnd = Stb.MoveToNextWord(num);
			if (SelectionEnd < Text.Length)
			{
				int selectionEnd = SelectionEnd - 1;
				SelectionEnd = selectionEnd;
			}
			return true;
		}
		return base.OnMouseDoubleClick(x, y, button);
	}
}
