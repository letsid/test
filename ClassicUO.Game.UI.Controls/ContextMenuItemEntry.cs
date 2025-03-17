using System;
using System.Collections.Generic;

namespace ClassicUO.Game.UI.Controls;

internal sealed class ContextMenuItemEntry
{
	public readonly Action Action;

	public readonly bool CanBeSelected;

	public bool IsSelected;

	public List<ContextMenuItemEntry> Items = new List<ContextMenuItemEntry>();

	public readonly string Text;

	public ContextMenuItemEntry(string text, Action action = null, bool canBeSelected = false, bool defaultValue = false)
	{
		Text = text;
		Action = action;
		CanBeSelected = canBeSelected;
		IsSelected = defaultValue;
	}

	public void Add(ContextMenuItemEntry subEntry)
	{
		Items.Add(subEntry);
	}
}
