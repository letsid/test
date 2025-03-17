using ClassicUO.Utility.Collections;

namespace ClassicUO.IO.Resources;

internal sealed class MultilinesFontInfo
{
	public TEXT_ALIGN_TYPE Align;

	public int CharCount;

	public int CharStart;

	public RawList<MultilinesFontData> Data = new RawList<MultilinesFontData>();

	public int IndentionOffset;

	public int MaxHeight;

	public MultilinesFontInfo Next;

	public int Width;

	public void Reset()
	{
		Width = 0;
		IndentionOffset = 0;
		MaxHeight = 0;
		CharStart = 0;
		CharCount = 0;
		Align = TEXT_ALIGN_TYPE.TS_LEFT;
		Next = null;
	}
}
