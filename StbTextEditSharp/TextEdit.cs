using System;
using ClassicUO.Utility;

namespace StbTextEditSharp;

public class TextEdit
{
	public bool CursorAtEndOfLine;

	public int CursorIndex;

	public bool HasPreferredX;

	public bool InsertMode;

	public float PreferredX;

	public int SelectEnd;

	public int SelectStart;

	public bool SingleLine;

	public UndoState UndoState;

	internal readonly ITextEditHandler Handler;

	public int Length => Handler.Length;

	public string text
	{
		get
		{
			return Handler.Text;
		}
		set
		{
			Handler.Text = value;
		}
	}

	public TextEdit(ITextEditHandler handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		Handler = handler;
		UndoState = new UndoState();
		ClearState(is_single_line: false);
	}

	public void SortSelection()
	{
		if (SelectEnd < SelectStart)
		{
			int selectEnd = SelectEnd;
			SelectEnd = SelectStart;
			SelectStart = selectEnd;
		}
	}

	public void MoveToFirst()
	{
		if (SelectStart != SelectEnd)
		{
			SortSelection();
			CursorIndex = SelectStart;
			SelectEnd = SelectStart;
			HasPreferredX = false;
		}
	}

	public void PrepareSelectionAtCursor()
	{
		if (SelectStart == SelectEnd)
		{
			SelectStart = (SelectEnd = CursorIndex);
		}
		else
		{
			CursorIndex = SelectEnd;
		}
	}

	public void MakeUndoInsert(int where, int length)
	{
		UndoState.CreateUndo(where, 0, length);
	}

	public void ClearState(bool is_single_line)
	{
		UndoState.undo_point = 0;
		UndoState.undo_char_point = 0;
		UndoState.redo_point = 99;
		UndoState.redo_char_point = 999;
		SelectEnd = (SelectStart = 0);
		CursorIndex = 0;
		HasPreferredX = false;
		PreferredX = 0f;
		CursorAtEndOfLine = false;
		SingleLine = is_single_line;
		InsertMode = false;
	}

	public void DeleteChars(int pos, int l)
	{
		if (l != 0)
		{
			text = text.Substring(0, pos) + text.Substring(pos + l);
		}
	}

	public int InsertChars(int pos, int[] codepoints, int start, int length)
	{
		int num = start + length;
		ValueStringBuilder valueStringBuilder = new ValueStringBuilder(num);
		for (int i = start; i < num; i++)
		{
			valueStringBuilder.Append(char.ConvertFromUtf32(codepoints[i]));
		}
		InsertChars(pos, valueStringBuilder.ToString());
		valueStringBuilder.Dispose();
		return length;
	}

	public int InsertChars(int pos, string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0;
		}
		if (text == null)
		{
			text = s;
		}
		else
		{
			text = text.Substring(0, pos) + s + text.Substring(pos);
		}
		return s.Length;
	}

	public int InsertChar(int pos, int codepoint)
	{
		string s = char.ConvertFromUtf32(codepoint);
		return InsertChars(pos, s);
	}

	public int LocateCoord(float x, float y)
	{
		TextEditRow textEditRow = default(TextEditRow);
		int length = Length;
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		textEditRow.x0 = (textEditRow.x1 = 0f);
		textEditRow.ymin = (textEditRow.ymax = 0f);
		textEditRow.num_chars = 0;
		while (num2 < length)
		{
			textEditRow = Handler.LayoutRow(num2);
			if (textEditRow.num_chars <= 0)
			{
				return length;
			}
			if (num2 == 0 && y < num + textEditRow.ymin)
			{
				return 0;
			}
			if (y < num + textEditRow.ymax)
			{
				break;
			}
			num2 += textEditRow.num_chars;
			num += textEditRow.baseline_y_delta;
		}
		if (num2 >= length)
		{
			return length;
		}
		if (x < textEditRow.x0)
		{
			return num2;
		}
		if (x < textEditRow.x1)
		{
			float num4 = textEditRow.x0;
			for (num3 = 0; num3 < textEditRow.num_chars; num3++)
			{
				float width = Handler.GetWidth(num2 + num3);
				if (x < num4 + width)
				{
					if (x < num4 + width / 2f)
					{
						return num3 + num2;
					}
					return num3 + num2 + 1;
				}
				num4 += width;
			}
		}
		if (text[num2 + textEditRow.num_chars - 1] == '\n')
		{
			return num2 + textEditRow.num_chars - 1;
		}
		return num2 + textEditRow.num_chars;
	}

	public void Click(float x, float y)
	{
		CursorIndex = LocateCoord(x, y);
		SelectStart = CursorIndex;
		SelectEnd = CursorIndex;
		HasPreferredX = false;
	}

	public void Drag(float x, float y)
	{
		int num = 0;
		if (SelectStart == SelectEnd)
		{
			SelectStart = CursorIndex;
		}
		num = LocateCoord(x, y);
		CursorIndex = (SelectEnd = num);
	}

	public void Clamp()
	{
		int length = Length;
		if (SelectStart != SelectEnd)
		{
			if (SelectStart > length)
			{
				SelectStart = length;
			}
			if (SelectEnd > length)
			{
				SelectEnd = length;
			}
			if (SelectStart == SelectEnd)
			{
				CursorIndex = SelectStart;
			}
		}
		if (CursorIndex > length)
		{
			CursorIndex = length;
		}
	}

	public void Delete(int where, int len)
	{
		MakeUndoDelete(where, len);
		DeleteChars(where, len);
		HasPreferredX = false;
	}

	public void DeleteSelection()
	{
		Clamp();
		if (SelectStart != SelectEnd)
		{
			if (SelectStart < SelectEnd)
			{
				Delete(SelectStart, SelectEnd - SelectStart);
				SelectEnd = (CursorIndex = SelectStart);
			}
			else
			{
				Delete(SelectEnd, SelectStart - SelectEnd);
				SelectStart = (CursorIndex = SelectEnd);
			}
			HasPreferredX = false;
		}
	}

	public void MoveToLast()
	{
		if (SelectStart != SelectEnd)
		{
			SortSelection();
			Clamp();
			CursorIndex = SelectEnd;
			SelectStart = SelectEnd;
			HasPreferredX = false;
		}
	}

	private static bool IsSpace(int codepoint)
	{
		return char.IsWhiteSpace((char)codepoint);
	}

	public bool IsWordBoundary(int idx)
	{
		if (idx <= 0)
		{
			return true;
		}
		if (IsSpace(text[idx - 1]))
		{
			return !IsSpace(text[idx]);
		}
		return false;
	}

	public int MoveToPreviousWord(int c)
	{
		c--;
		while (c >= 0 && !IsWordBoundary(c))
		{
			c--;
		}
		if (c < 0)
		{
			c = 0;
		}
		return c;
	}

	public int MoveToNextWord(int c)
	{
		int length = Length;
		c++;
		while (c < length && !IsWordBoundary(c))
		{
			c++;
		}
		if (c > length)
		{
			c = length;
		}
		return c;
	}

	public int Cut()
	{
		if (SelectStart != SelectEnd)
		{
			DeleteSelection();
			HasPreferredX = false;
			return 1;
		}
		return 0;
	}

	private int PasteInternal(string text)
	{
		Clamp();
		DeleteSelection();
		if (InsertChars(CursorIndex, text) != 0)
		{
			MakeUndoInsert(CursorIndex, text.Length);
			CursorIndex += text.Length;
			HasPreferredX = false;
			return 1;
		}
		if (UndoState.undo_point != 0)
		{
			UndoState.undo_point--;
		}
		return 0;
	}

	public void InputChar(char ch)
	{
		if (ch == '\n' && SingleLine)
		{
			return;
		}
		if (InsertMode && SelectStart == SelectEnd && CursorIndex < Length)
		{
			MakeUndoReplace(CursorIndex, 1, 1);
			DeleteChars(CursorIndex, 1);
			if (InsertChar(CursorIndex, ch) != 0)
			{
				CursorIndex++;
				HasPreferredX = false;
			}
		}
		else
		{
			DeleteSelection();
			if (InsertChar(CursorIndex, ch) != 0)
			{
				MakeUndoInsert(CursorIndex, 1);
				CursorIndex++;
				HasPreferredX = false;
			}
		}
	}

	public void Key(ControlKeys key)
	{
		while (true)
		{
			switch (key)
			{
			case ControlKeys.InsertMode:
				InsertMode = !InsertMode;
				return;
			case ControlKeys.Undo:
				Undo();
				HasPreferredX = false;
				return;
			case ControlKeys.Redo:
				Redo();
				HasPreferredX = false;
				return;
			case ControlKeys.Left:
				if (SelectStart != SelectEnd)
				{
					MoveToFirst();
				}
				else if (CursorIndex > 0)
				{
					CursorIndex--;
				}
				HasPreferredX = false;
				return;
			case ControlKeys.Right:
				if (SelectStart != SelectEnd)
				{
					MoveToLast();
				}
				else
				{
					CursorIndex++;
				}
				Clamp();
				HasPreferredX = false;
				return;
			case ControlKeys.Shift | ControlKeys.Left:
				Clamp();
				PrepareSelectionAtCursor();
				if (SelectEnd > 0)
				{
					SelectEnd--;
				}
				CursorIndex = SelectEnd;
				HasPreferredX = false;
				return;
			case ControlKeys.WordLeft:
				if (SelectStart != SelectEnd)
				{
					MoveToFirst();
					return;
				}
				CursorIndex = MoveToPreviousWord(CursorIndex);
				Clamp();
				return;
			case ControlKeys.WordLeft | ControlKeys.Shift:
				if (SelectStart == SelectEnd)
				{
					PrepareSelectionAtCursor();
				}
				CursorIndex = MoveToPreviousWord(CursorIndex);
				SelectEnd = CursorIndex;
				Clamp();
				return;
			case ControlKeys.WordRight:
				if (SelectStart != SelectEnd)
				{
					MoveToLast();
					return;
				}
				CursorIndex = MoveToNextWord(CursorIndex);
				Clamp();
				return;
			case ControlKeys.WordRight | ControlKeys.Shift:
				if (SelectStart == SelectEnd)
				{
					PrepareSelectionAtCursor();
				}
				CursorIndex = MoveToNextWord(CursorIndex);
				SelectEnd = CursorIndex;
				Clamp();
				return;
			case ControlKeys.Right | ControlKeys.Shift:
				PrepareSelectionAtCursor();
				SelectEnd++;
				Clamp();
				CursorIndex = SelectEnd;
				HasPreferredX = false;
				return;
			case ControlKeys.Down:
			case ControlKeys.Down | ControlKeys.Shift:
			{
				FindState findState2 = default(FindState);
				TextEditRow textEditRow2 = default(TextEditRow);
				bool flag2 = (key & ControlKeys.Shift) != 0;
				if (SingleLine)
				{
					key = ControlKeys.Right | (key & ControlKeys.Shift);
					break;
				}
				if (flag2)
				{
					PrepareSelectionAtCursor();
				}
				else if (SelectStart != SelectEnd)
				{
					MoveToLast();
				}
				Clamp();
				findState2.FindCharPosition(this, CursorIndex, SingleLine);
				if (findState2.length == 0)
				{
					return;
				}
				float num5 = (HasPreferredX ? PreferredX : findState2.x);
				float num6 = 0f;
				int cursorIndex = findState2.first_char + findState2.length;
				CursorIndex = cursorIndex;
				textEditRow2 = Handler.LayoutRow(CursorIndex);
				num6 = textEditRow2.x0;
				for (int i = 0; i < textEditRow2.num_chars; i++)
				{
					float num7 = 1f;
					num6 += num7;
					if (num6 > num5)
					{
						break;
					}
					CursorIndex++;
				}
				Clamp();
				HasPreferredX = true;
				PreferredX = num5;
				if (flag2)
				{
					SelectEnd = CursorIndex;
				}
				return;
			}
			case ControlKeys.Up:
			case ControlKeys.Up | ControlKeys.Shift:
			{
				FindState findState = default(FindState);
				TextEditRow textEditRow = default(TextEditRow);
				int num = 0;
				bool flag = (key & ControlKeys.Shift) != 0;
				if (SingleLine)
				{
					key = ControlKeys.Left | (key & ControlKeys.Shift);
					break;
				}
				if (flag)
				{
					PrepareSelectionAtCursor();
				}
				else if (SelectStart != SelectEnd)
				{
					MoveToFirst();
				}
				Clamp();
				findState.FindCharPosition(this, CursorIndex, SingleLine);
				if (findState.prev_first == findState.first_char)
				{
					return;
				}
				float num2 = (HasPreferredX ? PreferredX : findState.x);
				float num3 = 0f;
				CursorIndex = findState.prev_first;
				textEditRow = Handler.LayoutRow(CursorIndex);
				num3 = textEditRow.x0;
				for (num = 0; num < textEditRow.num_chars; num++)
				{
					float num4 = 1f;
					num3 += num4;
					if (num3 > num2)
					{
						break;
					}
					CursorIndex++;
				}
				Clamp();
				HasPreferredX = true;
				PreferredX = num2;
				if (flag)
				{
					SelectEnd = CursorIndex;
				}
				return;
			}
			case ControlKeys.Delete:
			case ControlKeys.Delete | ControlKeys.Shift:
				if (SelectStart != SelectEnd)
				{
					DeleteSelection();
				}
				else
				{
					int length2 = Length;
					if (CursorIndex < length2)
					{
						Delete(CursorIndex, 1);
					}
				}
				HasPreferredX = false;
				return;
			case ControlKeys.BackSpace:
			case ControlKeys.BackSpace | ControlKeys.Shift:
				if (SelectStart != SelectEnd)
				{
					DeleteSelection();
				}
				else
				{
					Clamp();
					if (CursorIndex > 0)
					{
						Delete(CursorIndex - 1, 1);
						CursorIndex--;
					}
				}
				HasPreferredX = false;
				return;
			case ControlKeys.TextStart:
				CursorIndex = (SelectStart = (SelectEnd = 0));
				HasPreferredX = false;
				return;
			case ControlKeys.TextEnd:
				CursorIndex = Length;
				SelectStart = (SelectEnd = 0);
				HasPreferredX = false;
				return;
			case ControlKeys.TextStart | ControlKeys.Shift:
				PrepareSelectionAtCursor();
				CursorIndex = (SelectEnd = 0);
				HasPreferredX = false;
				return;
			case ControlKeys.TextEnd | ControlKeys.Shift:
				PrepareSelectionAtCursor();
				CursorIndex = (SelectEnd = Length);
				HasPreferredX = false;
				return;
			case ControlKeys.LineStart:
				Clamp();
				MoveToFirst();
				if (SingleLine)
				{
					CursorIndex = 0;
				}
				else
				{
					while (CursorIndex > 0 && text[CursorIndex - 1] != '\n')
					{
						CursorIndex--;
					}
				}
				HasPreferredX = false;
				return;
			case ControlKeys.LineEnd:
			{
				int length3 = Length;
				Clamp();
				MoveToFirst();
				if (SingleLine)
				{
					CursorIndex = length3;
				}
				else
				{
					while (CursorIndex < length3 && text[CursorIndex] != '\n')
					{
						CursorIndex++;
					}
				}
				HasPreferredX = false;
				return;
			}
			case ControlKeys.LineStart | ControlKeys.Shift:
				Clamp();
				PrepareSelectionAtCursor();
				if (SingleLine)
				{
					CursorIndex = 0;
				}
				else
				{
					while (CursorIndex > 0 && text[CursorIndex - 1] != '\n')
					{
						CursorIndex--;
					}
				}
				SelectEnd = CursorIndex;
				HasPreferredX = false;
				return;
			case ControlKeys.LineEnd | ControlKeys.Shift:
			{
				int length = Length;
				Clamp();
				PrepareSelectionAtCursor();
				if (SingleLine)
				{
					CursorIndex = length;
				}
				else
				{
					while (CursorIndex < length && text[CursorIndex] != '\n')
					{
						CursorIndex++;
					}
				}
				SelectEnd = CursorIndex;
				HasPreferredX = false;
				return;
			}
			default:
				return;
			}
		}
	}

	public void Undo()
	{
		UndoState undoState = UndoState;
		if (undoState.undo_point == 0)
		{
			return;
		}
		UndoRecord undoRecord = default(UndoRecord);
		undoRecord = undoState.undo_rec[undoState.undo_point - 1];
		int num = undoState.redo_point - 1;
		undoState.undo_rec[num].char_storage = -1;
		undoState.undo_rec[num].insert_length = undoRecord.delete_length;
		undoState.undo_rec[num].delete_length = undoRecord.insert_length;
		undoState.undo_rec[num].where = undoRecord.where;
		if (undoRecord.delete_length != 0)
		{
			if (undoState.undo_char_point + undoRecord.delete_length >= 999)
			{
				undoState.undo_rec[num].insert_length = 0;
			}
			else
			{
				int num2 = 0;
				while (undoState.undo_char_point + undoRecord.delete_length > undoState.redo_char_point)
				{
					if (undoState.redo_point == 99)
					{
						return;
					}
					undoState.DiscardRedo();
				}
				num = undoState.redo_point - 1;
				undoState.undo_rec[num].char_storage = undoState.redo_char_point - undoRecord.delete_length;
				undoState.redo_char_point -= undoRecord.delete_length;
				for (num2 = 0; num2 < undoRecord.delete_length; num2++)
				{
					undoState.undo_char[undoState.undo_rec[num].char_storage + num2] = (sbyte)text[undoRecord.where + num2];
				}
			}
			DeleteChars(undoRecord.where, undoRecord.delete_length);
		}
		if (undoRecord.insert_length != 0)
		{
			InsertChars(undoRecord.where, undoState.undo_char, undoRecord.char_storage, undoRecord.insert_length);
			undoState.undo_char_point -= undoRecord.insert_length;
		}
		CursorIndex = undoRecord.where + undoRecord.insert_length;
		undoState.undo_point--;
		undoState.redo_point--;
	}

	public void Redo()
	{
		UndoState undoState = UndoState;
		UndoRecord undoRecord = default(UndoRecord);
		if (undoState.redo_point == 99)
		{
			return;
		}
		int undo_point = undoState.undo_point;
		undoRecord = undoState.undo_rec[undoState.redo_point];
		undoState.undo_rec[undo_point].delete_length = undoRecord.insert_length;
		undoState.undo_rec[undo_point].insert_length = undoRecord.delete_length;
		undoState.undo_rec[undo_point].where = undoRecord.where;
		undoState.undo_rec[undo_point].char_storage = -1;
		UndoRecord undoRecord2 = undoState.undo_rec[undo_point];
		if (undoRecord.delete_length != 0)
		{
			if (undoState.undo_char_point + undoRecord2.insert_length > undoState.redo_char_point)
			{
				undoState.undo_rec[undo_point].insert_length = 0;
				undoState.undo_rec[undo_point].delete_length = 0;
			}
			else
			{
				int num = 0;
				undoState.undo_rec[undo_point].char_storage = undoState.undo_char_point;
				undoState.undo_char_point += undoRecord2.insert_length;
				undoRecord2 = undoState.undo_rec[undo_point];
				for (num = 0; num < undoRecord2.insert_length; num++)
				{
					undoState.undo_char[undoRecord2.char_storage + num] = text[undoRecord2.where + num];
				}
			}
			DeleteChars(undoRecord.where, undoRecord.delete_length);
		}
		if (undoRecord.insert_length != 0)
		{
			InsertChars(undoRecord.where, undoState.undo_char, undoRecord.char_storage, undoRecord.insert_length);
			undoState.redo_char_point += undoRecord.insert_length;
		}
		CursorIndex = undoRecord.where + undoRecord.insert_length;
		undoState.undo_point++;
		undoState.redo_point++;
	}

	public void MakeUndoDelete(int where, int length)
	{
		int? num = UndoState.CreateUndo(where, length, 0);
		if (num.HasValue)
		{
			for (int i = 0; i < length; i++)
			{
				UndoState.undo_char[num.Value + i] = text[where + i];
			}
		}
	}

	public void MakeUndoReplace(int where, int old_length, int new_length)
	{
		int? num = UndoState.CreateUndo(where, old_length, new_length);
		if (num.HasValue)
		{
			for (int i = 0; i < old_length; i++)
			{
				UndoState.undo_char[num.Value + i] = text[where + i];
			}
		}
	}

	public int Paste(string ctext)
	{
		return PasteInternal(ctext);
	}
}
