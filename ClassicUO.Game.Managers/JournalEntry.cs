using System;
using ClassicUO.Game.Data;

namespace ClassicUO.Game.Managers;

internal class JournalEntry
{
	public byte Font;

	public ushort Hue;

	public bool IsUnicode;

	public string Name;

	public string Text;

	public TextType TextType;

	public DateTime Time;

	public bool ForcedUnicode;
}
