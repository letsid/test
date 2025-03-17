using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class CreateCharProfessionGump : Gump
{
	private enum Buttons
	{
		Prev
	}

	private readonly ProfessionInfo _Parent;

	public CreateCharProfessionGump(ProfessionInfo parent = null)
		: base(0u, 0u)
	{
		_Parent = parent;
		if (parent == null || !ProfessionLoader.Instance.Professions.TryGetValue(parent, out var value) || value == null)
		{
			value = new List<ProfessionInfo>(ProfessionLoader.Instance.Professions.Keys);
		}
		ResizePic resizePic = new ResizePic(2600);
		resizePic.X = 100;
		resizePic.Y = 80;
		resizePic.Width = 470;
		resizePic.Height = 372;
		Add(resizePic);
		Add(new GumpPic(291, 42, 1417, 0));
		Add(new GumpPic(214, 58, 1419, 0));
		Add(new GumpPic(300, 51, 5545, 0));
		ClilocLoader instance = ClilocLoader.Instance;
		bool isunicode;
		bool num = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
		byte font = (byte)(num ? 1u : 2u);
		ushort hue = (ushort)(num ? 65535u : 902u);
		Label label = new Label(instance.GetString(3000326, "Choose a Trade for Your Character"), isunicode, hue, 0, font);
		label.X = 158;
		label.Y = 132;
		Add(label);
		for (int i = 0; i < value.Count; i++)
		{
			int num2 = i % 2;
			int num3 = i >> 1;
			ProfessionInfoGump professionInfoGump = new ProfessionInfoGump(value[i]);
			professionInfoGump.X = 145 + num2 * 195;
			professionInfoGump.Y = 168 + num3 * 70;
			professionInfoGump.Selected = SelectProfession;
			Add(professionInfoGump);
		}
		Button button = new Button(0, 5537, 5539, 5538, "", 0);
		button.X = 586;
		button.Y = 445;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
	}

	public void SelectProfession(ProfessionInfo info)
	{
		if (info.Type == ProfessionLoader.PROF_TYPE.CATEGORY && ProfessionLoader.Instance.Professions.TryGetValue(info, out var value) && value != null)
		{
			base.Parent.Add(new CreateCharProfessionGump(info));
			base.Parent.Remove(this);
		}
		else
		{
			UIManager.GetGump<CharCreationGump>(null)?.SetProfession(info);
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 0)
		{
			if (_Parent != null && _Parent.TopLevel)
			{
				base.Parent.Add(new CreateCharProfessionGump());
				base.Parent.Remove(this);
			}
			else
			{
				base.Parent.Remove(this);
				UIManager.GetGump<CharCreationGump>(null)?.StepBack();
			}
		}
		base.OnButtonClick(buttonID);
	}
}
