using System;

namespace StbTextEditSharp;

public class UndoState
{
	public int redo_char_point;

	public short redo_point;

	public int[] undo_char = new int[999];

	public int undo_char_point;

	public short undo_point;

	public UndoRecord[] undo_rec = new UndoRecord[99];

	public void FlushRedo()
	{
		redo_point = 99;
		redo_char_point = 999;
	}

	public void DiscardUndo()
	{
		if (undo_point <= 0)
		{
			return;
		}
		if (undo_rec[0].char_storage >= 0)
		{
			int insert_length = undo_rec[0].insert_length;
			undo_char_point -= insert_length;
			Array.Copy(undo_char, insert_length, undo_char, 0, undo_char_point);
			for (int i = 0; i < undo_point; i++)
			{
				if (undo_rec[i].char_storage >= 0)
				{
					undo_rec[i].char_storage -= insert_length;
				}
			}
		}
		undo_point--;
		Array.Copy(undo_rec, 1, undo_rec, 0, undo_point);
	}

	public void DiscardRedo()
	{
		int num = 98;
		if (redo_point > num)
		{
			return;
		}
		int length;
		if (undo_rec[num].char_storage >= 0)
		{
			int insert_length = undo_rec[num].insert_length;
			redo_char_point += insert_length;
			length = 999 - redo_char_point;
			Array.Copy(undo_char, redo_char_point - insert_length, undo_char, redo_char_point, length);
			for (int i = redo_point; i < num; i++)
			{
				if (undo_rec[i].char_storage >= 0)
				{
					undo_rec[i].char_storage += insert_length;
				}
			}
		}
		redo_point++;
		length = 99 - redo_point;
		if (length != 0)
		{
			Array.Copy(undo_rec, redo_point, undo_rec, redo_point - 1, length);
		}
	}

	public int? CreateUndoRecord(int numchars)
	{
		FlushRedo();
		if (undo_point == 99)
		{
			DiscardUndo();
		}
		if (numchars > 999)
		{
			undo_point = 0;
			undo_char_point = 0;
			return null;
		}
		while (undo_char_point + numchars > 999)
		{
			DiscardUndo();
		}
		return undo_point++;
	}

	public int? CreateUndo(int pos, int insert_len, int delete_len)
	{
		int? num = CreateUndoRecord(insert_len);
		if (!num.HasValue)
		{
			return null;
		}
		int value = num.Value;
		undo_rec[value].where = pos;
		undo_rec[value].insert_length = (short)insert_len;
		undo_rec[value].delete_length = (short)delete_len;
		if (insert_len == 0)
		{
			undo_rec[value].char_storage = -1;
			return null;
		}
		undo_rec[value].char_storage = (short)undo_char_point;
		undo_char_point = (short)(undo_char_point + insert_len);
		return undo_rec[value].char_storage;
	}
}
