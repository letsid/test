namespace StbTextEditSharp;

public struct FindState
{
	public float x;

	public float y;

	public float height;

	public int first_char;

	public int length;

	public int prev_first;

	public void FindCharPosition(TextEdit str, int n, bool single_line)
	{
		TextEditRow textEditRow = default(TextEditRow);
		int num = 0;
		int num2 = str.Length;
		int i = 0;
		int num3 = 0;
		if (n == num2)
		{
			if (single_line)
			{
				textEditRow = str.Handler.LayoutRow(0);
				y = 0f;
				first_char = 0;
				length = num2;
				height = textEditRow.ymax - textEditRow.ymin;
				x = textEditRow.x1;
				return;
			}
			y = 0f;
			x = 0f;
			height = 1f;
			for (; i < num2; i += textEditRow.num_chars)
			{
				textEditRow = str.Handler.LayoutRow(i);
				num = i;
			}
			first_char = i;
			length = 0;
			prev_first = num;
			return;
		}
		y = 0f;
		while (true)
		{
			textEditRow = str.Handler.LayoutRow(i);
			if (n < i + textEditRow.num_chars)
			{
				break;
			}
			num = i;
			i += textEditRow.num_chars;
			y += textEditRow.baseline_y_delta;
		}
		num3 = (first_char = i);
		length = textEditRow.num_chars;
		height = textEditRow.ymax - textEditRow.ymin;
		prev_first = num;
		x = textEditRow.x0;
		for (i = 0; num3 + i < n; i++)
		{
			x += 1f;
		}
	}
}
