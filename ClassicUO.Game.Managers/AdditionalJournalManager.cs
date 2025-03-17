using System;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Utility;
using ClassicUO.Utility.Collections;

namespace ClassicUO.Game.Managers;

internal class AdditionalJournalManager
{
	public static Deque<AdditionalJournalEntry> Entries { get; } = new Deque<AdditionalJournalEntry>(100);

	public event EventHandler<AdditionalJournalEntry> EntryAdded;

	public void Add(string text, ushort hue, string name, TextType type, bool isunicode = true)
	{
		AdditionalJournalEntry additionalJournalEntry = ((Entries.Count >= 100) ? Entries.RemoveFromFront() : new AdditionalJournalEntry());
		byte font = (byte)((!isunicode) ? 9u : 0u);
		additionalJournalEntry.ForcedUnicode = false;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.OverrideAllFonts)
		{
			font = ProfileManager.CurrentProfile.ChatFont;
			if (!isunicode)
			{
				additionalJournalEntry.ForcedUnicode = true;
			}
			isunicode = true;
		}
		DateTime now = DateTime.Now;
		additionalJournalEntry.Text = text;
		additionalJournalEntry.Font = font;
		additionalJournalEntry.Hue = hue;
		additionalJournalEntry.Name = name;
		additionalJournalEntry.IsUnicode = isunicode;
		additionalJournalEntry.Time = now;
		additionalJournalEntry.TextType = type;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ForceUnicodeJournal)
		{
			additionalJournalEntry.Font = 0;
			if (!isunicode)
			{
				additionalJournalEntry.ForcedUnicode = true;
			}
			additionalJournalEntry.IsUnicode = true;
		}
		Entries.AddToBack(additionalJournalEntry);
		this.EntryAdded.Raise(additionalJournalEntry);
	}
}
