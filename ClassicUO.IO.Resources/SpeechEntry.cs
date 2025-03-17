using System;

namespace ClassicUO.IO.Resources;

internal readonly struct SpeechEntry : IComparable<SpeechEntry>
{
	public string[] Keywords { get; }

	public short KeywordID { get; }

	public bool CheckStart { get; }

	public bool CheckEnd { get; }

	public SpeechEntry(int id, string keyword)
	{
		KeywordID = (short)id;
		Keywords = keyword.Split(new char[1] { '*' }, StringSplitOptions.RemoveEmptyEntries);
		CheckStart = keyword.Length > 0 && keyword[0] == '*';
		CheckEnd = keyword.Length > 0 && keyword[keyword.Length - 1] == '*';
	}

	public int CompareTo(SpeechEntry obj)
	{
		if (KeywordID < obj.KeywordID)
		{
			return -1;
		}
		return (KeywordID > obj.KeywordID) ? 1 : 0;
	}
}
