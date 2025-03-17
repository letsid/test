using System.Collections.Generic;
using System.Linq;

namespace ClassicUO.Game.UI.Controls;

internal class RadioButton : Checkbox
{
	public int GroupIndex { get; set; }

	public RadioButton(int group, List<string> parts, string[] lines)
		: base(parts, lines)
	{
		GroupIndex = group;
		base.IsFromServer = true;
	}

	public RadioButton(int group, ushort inactive, ushort active, string text = "", byte font = 0, ushort color = 0, bool isunicode = true, int maxWidth = 0)
		: base(inactive, active, text, font, color, isunicode, maxWidth)
	{
		GroupIndex = group;
	}

	protected override void OnCheckedChanged()
	{
		if (base.IsChecked && HandleClick())
		{
			base.OnCheckedChanged();
		}
	}

	private bool HandleClick()
	{
		IEnumerable<RadioButton> enumerable = (from s in base.Parent?.FindControls<RadioButton>()
			where s.GroupIndex == GroupIndex && s != this
			select s);
		if (enumerable == null)
		{
			return false;
		}
		foreach (RadioButton item in enumerable)
		{
			item.IsChecked = false;
		}
		return true;
	}
}
