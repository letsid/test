using System;
using System.IO;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Collections;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal class JournalManager
{
	private StreamWriter _fileWriter;

	private bool _writerHasException;

	public static Deque<JournalEntry> Entries { get; } = new Deque<JournalEntry>(100);

	public event EventHandler<JournalEntry> EntryAdded;

	public void Add(string text, ushort hue, string name, TextType type, bool isunicode = true)
	{
		JournalEntry journalEntry = ((Entries.Count >= 100) ? Entries.RemoveFromFront() : new JournalEntry());
		byte font = (byte)((!isunicode) ? 9u : 0u);
		journalEntry.ForcedUnicode = false;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.OverrideAllFonts)
		{
			font = ProfileManager.CurrentProfile.ChatFont;
			if (!isunicode)
			{
				journalEntry.ForcedUnicode = true;
			}
			isunicode = true;
		}
		DateTime now = DateTime.Now;
		journalEntry.Text = text;
		journalEntry.Font = font;
		journalEntry.Hue = hue;
		journalEntry.Name = name;
		journalEntry.IsUnicode = isunicode;
		journalEntry.Time = now;
		journalEntry.TextType = type;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ForceUnicodeJournal)
		{
			journalEntry.Font = 0;
			if (!isunicode)
			{
				journalEntry.ForcedUnicode = true;
			}
			journalEntry.IsUnicode = true;
		}
		Entries.AddToBack(journalEntry);
		this.EntryAdded.Raise(journalEntry);
		if (_fileWriter == null && !_writerHasException)
		{
			CreateWriter();
		}
		if (string.IsNullOrEmpty(journalEntry.Name) && journalEntry.TextType == TextType.SYSTEM)
		{
			journalEntry.Name = "System: ";
		}
		string text2 = ((journalEntry.Name != string.Empty) ? (journalEntry.Name ?? "") : string.Empty);
		if (string.Equals(text2, ResGeneral.YouSee))
		{
			text2 = (string.Equals(text2, ResGeneral.YouSee) ? "&rsaquo;&rsaquo; " : text2);
			text = text2 + journalEntry.Text;
			uint unicodeFontColor = HuesLoader.Instance.GetUnicodeFontColor(30720, hue, journalEntry.IsUnicode);
			_fileWriter?.WriteLine($"<div class=\"see\"><font color=\"#{unicodeFontColor:X6}\">[{now:HH:mm:ss}] {text} </font></div>");
		}
		else
		{
			text = text2 + journalEntry.Text;
			uint unicodeFontColor2 = HuesLoader.Instance.GetUnicodeFontColor(30720, hue, journalEntry.IsUnicode);
			_fileWriter?.WriteLine($"<div><font color=\"#{unicodeFontColor2:X6}\">[{now:HH:mm:ss}] {text} </font></div>");
		}
	}

	private void CreateWriter()
	{
		if (_fileWriter != null || ProfileManager.CurrentProfile == null)
		{
			return;
		}
		try
		{
			string path = Path.Combine(FileSystemHelper.CreateFolderIfNotExists(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Alathair", "Logs", ProfileManager.CurrentProfile.CharacterName ?? ""), $"{ProfileManager.CurrentProfile.CharacterName}_{DateTime.Now:yyyy_MM_dd}.html");
			bool num = File.Exists(path);
			_fileWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = true
			};
			if (!num)
			{
				_fileWriter.WriteLine("<!DOCTYPE html><html><head><style type=\"text/css\">body {background-color:black;}.see {font-style: normal; font-size: 1em;}.hidden {font-style: italic; font-size: 1em;}.normal {font-style: normal; font-size: 1em;}</style><title>Alathair-Log</title></head><body>\r\n");
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			_writerHasException = true;
		}
	}

	public void CloseWriter()
	{
		_fileWriter?.Flush();
		_fileWriter?.Dispose();
		_fileWriter = null;
	}

	public void Clear()
	{
		CloseWriter();
	}
}
