using System;
using System.Collections.Generic;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Utility.Collections;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class JournalGump : ResizableBackgroundGump
{
	private class RenderedTextList : Control
	{
		private readonly Deque<RenderedText> _entries;

		private readonly Deque<RenderedText> _hours;

		private readonly ScrollBarBase _scrollBar;

		private readonly Deque<TextType> _text_types;

		public RenderedTextList(int x, int y, int width, int height, ScrollBarBase scrollBarControl)
		{
			_scrollBar = scrollBarControl;
			_scrollBar.IsVisible = false;
			AcceptMouseInput = true;
			CanMove = true;
			base.X = x;
			base.Y = y;
			base.Width = width;
			base.Height = height;
			_entries = new Deque<RenderedText>();
			_hours = new Deque<RenderedText>();
			_text_types = new Deque<TextType>();
			base.WantUpdateSize = false;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			base.Draw(batcher, x, y);
			int num = y;
			int num2 = 0;
			int num3 = _scrollBar.Value + _scrollBar.Height;
			for (int i = 0; i < _entries.Count; i++)
			{
				RenderedText renderedText = _entries[i];
				RenderedText renderedText2 = _hours[i];
				int num4 = _entries.Count - _hours.Count;
				for (int j = 1; j <= num4; j++)
				{
					_hours.AddToBack(renderedText2);
				}
				_ = _text_types[i];
				if (num2 + renderedText.Height <= _scrollBar.Value)
				{
					num2 += renderedText.Height;
					continue;
				}
				if (num2 + renderedText.Height <= num3)
				{
					int num5 = num2 - _scrollBar.Value;
					if (num5 < 0)
					{
						if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowJournalEntryTime)
						{
							renderedText2.Draw(batcher, renderedText2.Width, renderedText2.Height, x, y, renderedText.Width, renderedText.Height + num5, 0, -num5, 0);
							renderedText.Draw(batcher, renderedText.Width, renderedText.Height, x + renderedText2.Width, y, renderedText.Width, renderedText.Height + num5, 0, -num5, 0);
						}
						else
						{
							renderedText.Draw(batcher, renderedText.Width, renderedText.Height, x, y, renderedText.Width, renderedText.Height + num5, 0, -num5, 0);
						}
						num += renderedText.Height + num5;
					}
					else
					{
						if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowJournalEntryTime)
						{
							renderedText2.Draw(batcher, x, num, 1f, 0);
							renderedText.Draw(batcher, x + renderedText2.Width, num, 1f, 0);
						}
						else
						{
							renderedText.Draw(batcher, x, num, 1f, 0);
						}
						num += renderedText.Height;
					}
					num2 += renderedText.Height;
					continue;
				}
				int num6 = num3 - num2;
				if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowJournalEntryTime)
				{
					renderedText2.Draw(batcher, renderedText2.Width, renderedText2.Height, x, y + _scrollBar.Height - num6, renderedText.Width, num6, 0, 0, 0);
					renderedText.Draw(batcher, renderedText.Width, renderedText.Height, x + renderedText2.Width, y + _scrollBar.Height - num6, renderedText.Width, num6, 0, 0, 0);
				}
				else
				{
					renderedText.Draw(batcher, renderedText.Width, renderedText.Height, x, y + _scrollBar.Height - num6, renderedText.Width, num6, 0, 0, 0);
				}
				break;
			}
			return true;
		}

		protected override void OnDragEnd(int x, int y)
		{
			base.OnDragEnd(x, y);
			ProfileManager.CurrentProfile.JournalPositionX = base.ScreenCoordinateX;
			ProfileManager.CurrentProfile.JournalPositionY = base.ScreenCoordinateY;
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (base.IsVisible)
			{
				_scrollBar.X = base.X + base.Width - (_scrollBar.Width >> 1) + 5;
				_scrollBar.Height = base.Height;
				CalculateScrollBarMaxValue();
				_scrollBar.IsVisible = _scrollBar.MaxValue > _scrollBar.MinValue;
			}
		}

		private void CalculateScrollBarMaxValue()
		{
			bool flag = _scrollBar.Value == _scrollBar.MaxValue;
			int num = 0;
			for (int i = 0; i < _entries.Count; i++)
			{
				if (i < _text_types.Count)
				{
					num += _entries[i].Height;
				}
			}
			num -= _scrollBar.Height;
			if (num > 0)
			{
				_scrollBar.MaxValue = num;
				if (flag)
				{
					_scrollBar.Value = _scrollBar.MaxValue;
				}
			}
			else
			{
				_scrollBar.MaxValue = 0;
				_scrollBar.Value = 0;
			}
		}

		public void AddEntry(string text, int font, ushort hue, bool isUnicode, DateTime time, TextType text_type, bool ForcedUnicode = false)
		{
			bool flag = _scrollBar.Value == _scrollBar.MaxValue;
			while (_entries.Count > 199)
			{
				_entries.RemoveFromFront().Destroy();
				_hours.RemoveFromFront().Destroy();
				_text_types.RemoveFromFront();
			}
			RenderedText renderedText = RenderedText.Create($"{time:t} ", 946, 1, isunicode: true, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
			_hours.AddToBack(renderedText);
			int num = 0;
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowJournalEntryTime)
			{
				num = renderedText.Width;
			}
			RenderedText renderedText2 = RenderedText.Create(text, hue, (byte)font, isUnicode, (FontStyle)(0xC | (ForcedUnicode ? 1024 : 0)), TEXT_ALIGN_TYPE.TS_LEFT, base.Width - (18 + num), 30);
			_entries.AddToBack(renderedText2);
			_text_types.AddToBack(text_type);
			_scrollBar.MaxValue += renderedText2.Height;
			if (flag)
			{
				_scrollBar.Value = _scrollBar.MaxValue;
			}
		}

		private static bool CanBeDrawn(TextType type)
		{
			if (type == TextType.CLIENT && !ProfileManager.CurrentProfile.ShowJournalClient)
			{
				return false;
			}
			if (type == TextType.SYSTEM && !ProfileManager.CurrentProfile.ShowJournalSystem)
			{
				return false;
			}
			if (type == TextType.OBJECT && !ProfileManager.CurrentProfile.ShowJournalObjects)
			{
				return false;
			}
			if (type == TextType.GUILD_ALLY && !ProfileManager.CurrentProfile.ShowJournalGuildAlly)
			{
				return false;
			}
			return true;
		}

		public override void Dispose()
		{
			for (int i = 0; i < _entries.Count; i++)
			{
				_entries[i].Destroy();
				_hours[i].Destroy();
			}
			_entries.Clear();
			_hours.Clear();
			_text_types.Clear();
			base.Dispose();
		}
	}

	private const ushort BORDER_HUE = 0;

	private bool _isMinimized;

	private RenderedTextList _journalEntries;

	private ScrollFlag _scrollBar;

	private int _padding = 8;

	public static Dictionary<int, Tuple<ushort, ushort>> mSkins = new Dictionary<int, Tuple<ushort, ushort>>
	{
		{
			0,
			new Tuple<ushort, ushort>(3000, 1)
		},
		{
			1,
			new Tuple<ushort, ushort>(5120, 1109)
		},
		{
			2,
			new Tuple<ushort, ushort>(3000, 0)
		},
		{
			3,
			new Tuple<ushort, ushort>(5054, 0)
		},
		{
			4,
			new Tuple<ushort, ushort>(3000, 2683)
		}
	};

	public ushort Hue { get; set; }

	public override GumpType GumpType => GumpType.Journal;

	public bool IsMinimized
	{
		get
		{
			return _isMinimized;
		}
		set
		{
			if (_isMinimized == value)
			{
				return;
			}
			_isMinimized = value;
			foreach (Control child in base.Children)
			{
				child.IsVisible = !value;
			}
			base.WantUpdateSize = true;
		}
	}

	public JournalGump(int journalSkin)
		: base(400, 400, 250, 300, 0u, 0u, 0, mSkins[journalSkin].Item1, mSkins[journalSkin].Item2, (journalSkin == 0) ? 4 : 0)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		LoadSettings();
		OnResize();
		AddTextList();
		InitializeJournalEntries();
		World.Journal.EntryAdded -= AddJournalEntry;
		World.Journal.EntryAdded += AddJournalEntry;
	}

	protected void LoadSettings()
	{
		if (ProfileManager.CurrentProfile.JournalWidth != 0 && ProfileManager.CurrentProfile.JournalHeight != 0)
		{
			base.Width = ProfileManager.CurrentProfile.JournalWidth;
			base.Height = ProfileManager.CurrentProfile.JournalHeight;
		}
		base.X = ProfileManager.CurrentProfile.JournalPositionX;
		base.Y = ProfileManager.CurrentProfile.JournalPositionY;
		ResizeWindow(new Point(base.Width, base.Height));
	}

	public override void OnResize()
	{
		base.OnResize();
		if (_journalEntries != null && _journalEntries.Width != base.Width - (_scrollBar.Width >> 1))
		{
			Remove(_journalEntries);
			Add(_journalEntries = new RenderedTextList(_padding, _padding, base.Width - (_scrollBar.Width >> 1), base.Height - 2 * _padding, _scrollBar));
			InitializeJournalEntries();
			ProfileManager.CurrentProfile.JournalWidth = base.Width;
			ProfileManager.CurrentProfile.JournalHeight = base.Height;
		}
	}

	private void AddTextList()
	{
		_scrollBar = new ScrollFlag(-25, 5, base.Height, showbuttons: true);
		Add(_journalEntries = new RenderedTextList(_padding, _padding, base.Width - (_scrollBar.Width >> 1) - 5, base.Height - 2 * _padding, _scrollBar));
		Add(_scrollBar);
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		_scrollBar.InvokeMouseWheel(delta);
	}

	public override void Dispose()
	{
		World.Journal.EntryAdded -= AddJournalEntry;
		base.Dispose();
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_journalEntries != null && _journalEntries.Width != base.Width - (_scrollBar.Width >> 1) - 5)
		{
			Remove(_journalEntries);
			Add(_journalEntries = new RenderedTextList(_padding, _padding, base.Width - (_scrollBar.Width >> 1) - 5, base.Height - 2 * _padding, _scrollBar));
			InitializeJournalEntries();
		}
	}

	protected override void OnMove(int x, int y)
	{
		base.OnMove(x, y);
		ProfileManager.CurrentProfile.JournalPositionX = base.ScreenCoordinateX;
		ProfileManager.CurrentProfile.JournalPositionY = base.ScreenCoordinateY;
	}

	protected void AddJournalEntry(object sender, JournalEntry entry)
	{
		string text = ((entry.Name != string.Empty) ? (entry.Name ?? "") : string.Empty) + entry.Text;
		_journalEntries.AddEntry(text, entry.Font, entry.Hue, entry.IsUnicode, entry.Time, entry.TextType, entry.ForcedUnicode);
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("height", base.Height.ToString());
		writer.WriteAttributeString("width", base.Width.ToString());
		writer.WriteAttributeString("isminimized", IsMinimized.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		base.Height = int.Parse(xml.GetAttribute("height"));
		base.Height = int.Parse(xml.GetAttribute("width"));
		IsMinimized = bool.Parse(xml.GetAttribute("isminimized"));
	}

	private void InitializeJournalEntries()
	{
		foreach (JournalEntry entry in JournalManager.Entries)
		{
			AddJournalEntry(null, entry);
		}
		_scrollBar.MinValue = 0;
	}

	private void _gumpPic_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && IsMinimized)
		{
			IsMinimized = false;
			e.Result = true;
		}
	}

	private void _hitBox_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left && !IsMinimized)
		{
			IsMinimized = true;
		}
	}
}
