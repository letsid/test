using System.Collections.Generic;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class SkillListEntry : Control
{
	private enum Buttons
	{
		ActiveSkillUse = 1
	}

	private readonly Button _activeUse;

	private readonly Skill _skill;

	public SkillListEntry(Label skillname, Label skillvaluebase, Label skillvalue, Label skillcap, Skill skill)
	{
		SkillListEntry skillListEntry = this;
		base.Height = 20;
		_skill = skill;
		skillname.X = 20;
		if (skill.IsClickable)
		{
			Button button = new Button(1, 2103, 2104, 0, "", 0);
			button.X = 0;
			button.Y = 4;
			button.ButtonAction = ButtonAction.Activate;
			Button c = button;
			_activeUse = button;
			Add(c);
		}
		Add(skillname);
		skillvaluebase.X = 200;
		Add(skillvaluebase);
		skillvalue.X = 280;
		Add(skillvalue);
		skillcap.X = 360;
		Add(skillcap);
		GumpPic loc = new GumpPic(425, 4, (ushort)((skill.Lock == Lock.Up) ? 2435u : ((skill.Lock == Lock.Down) ? 2437u : 2092u)), 0);
		Add(loc);
		loc.MouseUp += delegate
		{
			switch (skillListEntry._skill.Lock)
			{
			case Lock.Up:
				skillListEntry._skill.Lock = Lock.Down;
				GameActions.ChangeSkillLockStatus((ushort)skillListEntry._skill.Index, 1);
				loc.Graphic = 2437;
				break;
			case Lock.Down:
				skillListEntry._skill.Lock = Lock.Locked;
				GameActions.ChangeSkillLockStatus((ushort)skillListEntry._skill.Index, 2);
				loc.Graphic = 2092;
				break;
			case Lock.Locked:
				skillListEntry._skill.Lock = Lock.Up;
				GameActions.ChangeSkillLockStatus((ushort)skillListEntry._skill.Index, 0);
				loc.Graphic = 2435;
				break;
			}
		};
	}

	protected override void OnDragBegin(int x, int y)
	{
		if (_skill.IsClickable && Mouse.LButtonPressed)
		{
			GetSpellFloatingButton(_skill.Index)?.Dispose();
			GumpsLoader.Instance.GetGumpTexture(9400u, out var bounds);
			SkillButtonGump skillButtonGump = new SkillButtonGump(_skill, Mouse.LClickPosition.X + (bounds.Width >> 1), Mouse.LClickPosition.Y + (bounds.Height >> 1));
			UIManager.Add(skillButtonGump);
			UIManager.AttemptDragControl(skillButtonGump, attemptAlwaysSuccessful: true);
		}
	}

	private static SkillButtonGump GetSpellFloatingButton(int id)
	{
		for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
		{
			if (linkedListNode.Value is SkillButtonGump skillButtonGump && skillButtonGump.SkillID == id)
			{
				return skillButtonGump;
			}
		}
		return null;
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 1)
		{
			GameActions.UseSkill(_skill.Index);
		}
	}
}
