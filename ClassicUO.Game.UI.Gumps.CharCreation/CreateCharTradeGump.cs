using System;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class CreateCharTradeGump : Gump
{
	private enum Buttons
	{
		Prev,
		Next
	}

	private readonly HSliderBar[] _attributeSliders;

	private readonly PlayerMobile _character;

	private readonly Combobox[] _skills;

	private readonly HSliderBar[] _skillSliders;

	public CreateCharTradeGump(PlayerMobile character, ProfessionInfo profession)
		: base(0u, 0u)
	{
		_character = character;
		Skill[] skills = _character.Skills;
		foreach (Skill obj in skills)
		{
			obj.ValueFixed = 0;
			obj.BaseFixed = 0;
			obj.CapFixed = 0;
			obj.Lock = Lock.Locked;
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
		bool isunicode;
		bool num = (isunicode = string.Compare(Settings.GlobalSettings.Language, "CHT", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "KOR", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(Settings.GlobalSettings.Language, "JPN", StringComparison.InvariantCultureIgnoreCase) == 0);
		byte font = (byte)(num ? 1u : 2u);
		ushort hue = (ushort)(num ? 65535u : 902u);
		Label label = new Label(ClilocLoader.Instance.GetString(3000326), isunicode, hue, 0, font);
		label.X = 148;
		label.Y = 132;
		Add(label);
		Label label2 = new Label(ClilocLoader.Instance.GetString(3000111), isunicode, 1, 0, 1);
		label2.X = 158;
		label2.Y = 170;
		Add(label2);
		Label label3 = new Label(ClilocLoader.Instance.GetString(3000112), isunicode, 1, 0, 1);
		label3.X = 158;
		label3.Y = 250;
		Add(label3);
		Label label4 = new Label(ClilocLoader.Instance.GetString(3000113), isunicode, 1, 0, 1);
		label4.X = 158;
		label4.Y = 330;
		Add(label4);
		_attributeSliders = new HSliderBar[3];
		Add(_attributeSliders[0] = new HSliderBar(164, 196, 93, 10, 60, ProfessionInfo._VoidStats[0], HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, 0));
		Add(_attributeSliders[1] = new HSliderBar(164, 276, 93, 10, 60, ProfessionInfo._VoidStats[1], HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, 0));
		Add(_attributeSliders[2] = new HSliderBar(164, 356, 93, 10, 60, ProfessionInfo._VoidStats[2], HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, 0));
		string[] array = new string[SkillsLoader.Instance.SortedSkills.Count];
		for (int j = 0; j < array.Length; j++)
		{
			SkillEntry skillEntry = SkillsLoader.Instance.SortedSkills[j];
			if (((World.ClientFeatures.Flags & CharacterListFlags.CLF_SAMURAI_NINJA) == 0 && (skillEntry.Index == 52 || skillEntry.Index == 47 || skillEntry.Index == 53)) || skillEntry.Index == 54)
			{
				array[j] = string.Empty;
			}
			else
			{
				array[j] = skillEntry.Name;
			}
		}
		int num2 = 172;
		_skillSliders = new HSliderBar[CharCreationGump._skillsCount];
		_skills = new Combobox[CharCreationGump._skillsCount];
		for (int k = 0; k < CharCreationGump._skillsCount; k++)
		{
			Add(_skills[k] = new Combobox(344, num2, 182, array, -1, 200, showArrow: false, "Click here", 9));
			Add(_skillSliders[k] = new HSliderBar(344, num2 + 32, 93, 0, 50, ProfessionInfo._VoidSkills[k, 1], HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, 0));
			num2 += 70;
		}
		Button button = new Button(0, 5537, 5539, 5538, "", 0);
		button.X = 586;
		button.Y = 445;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(1, 5540, 5542, 5541, "", 0);
		button2.X = 610;
		button2.Y = 445;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		for (int l = 0; l < _attributeSliders.Length; l++)
		{
			for (int m = 0; m < _attributeSliders.Length; m++)
			{
				if (l != m)
				{
					_attributeSliders[l].AddParisSlider(_attributeSliders[m]);
				}
			}
		}
		for (int n = 0; n < _skillSliders.Length; n++)
		{
			for (int num3 = 0; num3 < _skillSliders.Length; num3++)
			{
				if (n != num3)
				{
					_skillSliders[n].AddParisSlider(_skillSliders[num3]);
				}
			}
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		CharCreationGump gump = UIManager.GetGump<CharCreationGump>(null);
		switch ((Buttons)buttonID)
		{
		case Buttons.Prev:
			gump.StepBack();
			break;
		case Buttons.Next:
		{
			if (!ValidateValues())
			{
				break;
			}
			for (int i = 0; i < _skills.Length; i++)
			{
				if (_skills[i].SelectedIndex != -1)
				{
					Skill obj = _character.Skills[SkillsLoader.Instance.SortedSkills[_skills[i].SelectedIndex].Index];
					obj.ValueFixed = (ushort)_skillSliders[i].Value;
					obj.BaseFixed = 0;
					obj.CapFixed = 0;
					obj.Lock = Lock.Locked;
				}
			}
			_character.Strength = (ushort)_attributeSliders[0].Value;
			_character.Intelligence = (ushort)_attributeSliders[1].Value;
			_character.Dexterity = (ushort)_attributeSliders[2].Value;
			gump.SetAttributes(force: true);
			break;
		}
		}
		base.OnButtonClick(buttonID);
	}

	private bool ValidateValues()
	{
		if (_skills.All((Combobox s) => s.SelectedIndex >= 0))
		{
			if ((from o in _skills
				group o by o.SelectedIndex).Count((IGrouping<int, Combobox> o) => o.Count() > 1) > 0)
			{
				UIManager.GetGump<CharCreationGump>(null)?.ShowMessage(ClilocLoader.Instance.GetString(1080032));
				return false;
			}
			return true;
		}
		UIManager.GetGump<CharCreationGump>(null)?.ShowMessage((Client.Version <= ClientVersion.CV_5090) ? ResGumps.YouMustHaveThreeUniqueSkillsChosen : ClilocLoader.Instance.GetString(1080032));
		return false;
	}
}
