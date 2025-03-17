using System;
using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Input;

namespace ClassicUO.Game.UI.Controls;

internal class ContextMenuControl
{
	private readonly List<ContextMenuItemEntry> _items;

	public ContextMenuControl()
	{
		_items = new List<ContextMenuItemEntry>();
	}

	public void Add(string text, Action action, bool canBeSelected = false, bool defaultValue = false)
	{
		_items.Add(new ContextMenuItemEntry(text, action, canBeSelected, defaultValue));
	}

	public void Add(ContextMenuItemEntry entry)
	{
		_items.Add(entry);
	}

	public void Add(string text, List<ContextMenuItemEntry> entries)
	{
		_items.Add(new ContextMenuItemEntry(text)
		{
			Items = entries
		});
	}

	public void Show()
	{
		UIManager.ShowContextMenu(null);
		if (_items.Count != 0)
		{
			ContextMenuShowMenu contextMenuShowMenu = new ContextMenuShowMenu(_items);
			contextMenuShowMenu.X = Mouse.Position.X + 5;
			contextMenuShowMenu.Y = Mouse.Position.Y - 20;
			UIManager.ShowContextMenu(contextMenuShowMenu);
		}
	}

	public void Dispose()
	{
		UIManager.ShowContextMenu(null);
		_items.Clear();
	}
}
