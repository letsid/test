using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class SkillGumpAdvanced : Gump
{
	private enum Buttons
	{
		SortName = 1,
		SortReal,
		SortBase,
		SortCap
	}

	private const int WIDTH = 500;

	private const int HEIGHT = 360;

	private readonly Dictionary<Buttons, string> _buttonsToSkillsValues = new Dictionary<Buttons, string>
	{
		{
			Buttons.SortName,
			"Name"
		},
		{
			Buttons.SortReal,
			"Base"
		},
		{
			Buttons.SortBase,
			"Value"
		},
		{
			Buttons.SortCap,
			"Cap"
		}
	};

	private readonly DataBox _databox;

	private readonly List<SkillListEntry> _skillListEntries = new List<SkillListEntry>();

	private bool _sortAsc;

	private string _sortField;

	private readonly GumpPic _sortOrderIndicator;

	private double _totalReal;

	private double _totalValue;

	private bool _updateSkillsNeeded;

	public override GumpType GumpType => GumpType.SkillMenu;

	public SkillGumpAdvanced()
		: base(0u, 0u)
	{
		_totalReal = 0.0;
		_totalValue = 0.0;
		CanMove = true;
		AcceptMouseInput = true;
		base.WantUpdateSize = false;
		base.Width = 500;
		base.Height = 360;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.95f);
		alphaBlendControl.X = 1;
		alphaBlendControl.Y = 1;
		alphaBlendControl.Width = 498;
		alphaBlendControl.Height = 358;
		Add(alphaBlendControl);
		ScrollArea scrollArea = new ScrollArea(20, 60, 460, 250, normalScrollbar: true)
		{
			AcceptMouseInput = true
		};
		Add(scrollArea);
		_databox = new DataBox(0, 0, 1, 1);
		_databox.WantUpdateSize = true;
		scrollArea.Add(_databox);
		NiceButton obj = new NiceButton(10, 10, 180, 25, ButtonAction.Activate, ResGumps.Name)
		{
			ButtonParameter = 1,
			IsSelected = true
		};
		obj.X = 40;
		obj.Y = 25;
		Add(obj);
		NiceButton obj2 = new NiceButton(10, 10, 80, 25, ButtonAction.Activate, ResGumps.Real)
		{
			ButtonParameter = 2
		};
		obj2.X = 220;
		obj2.Y = 25;
		Add(obj2);
		NiceButton obj3 = new NiceButton(10, 10, 80, 25, ButtonAction.Activate, ResGumps.Base)
		{
			ButtonParameter = 3
		};
		obj3.X = 300;
		obj3.Y = 25;
		Add(obj3);
		NiceButton obj4 = new NiceButton(10, 10, 80, 25, ButtonAction.Activate, ResGumps.Cap)
		{
			ButtonParameter = 4
		};
		obj4.X = 380;
		obj4.Y = 25;
		Add(obj4);
		Add(new Line(20, 60, 435, 1, uint.MaxValue));
		Add(new Line(20, 310, 435, 1, uint.MaxValue));
		Add(_sortOrderIndicator = new GumpPic(0, 0, 2437, 0));
		OnButtonClick(1);
	}

	public override void OnButtonClick(int buttonID)
	{
		if (_buttonsToSkillsValues.TryGetValue((Buttons)buttonID, out var value))
		{
			if (_sortField == value)
			{
				_sortAsc = !_sortAsc;
			}
			_sortField = value;
		}
		if (FindControls<NiceButton>().Any((NiceButton s) => s.ButtonParameter == buttonID))
		{
			NiceButton niceButton = FindControls<NiceButton>().First((NiceButton s) => s.ButtonParameter == buttonID);
			ushort graphic = (ushort)(_sortAsc ? 2437u : 2435u);
			_sortOrderIndicator.Graphic = graphic;
			_sortOrderIndicator.X = niceButton.X + niceButton.Width - 15;
			_sortOrderIndicator.Y = niceButton.Y + 5;
		}
		_updateSkillsNeeded = true;
	}

	private void BuildGump()
	{
		_totalReal = 0.0;
		_totalValue = 0.0;
		_databox.Clear();
		foreach (SkillListEntry skillListEntry in _skillListEntries)
		{
			skillListEntry.Clear();
			skillListEntry.Dispose();
		}
		_skillListEntries.Clear();
		PropertyInfo pi = typeof(Skill).GetProperty(_sortField);
		List<Skill> list = new List<Skill>(World.Player.Skills.OrderBy((Skill x) => pi.GetValue(x, null)));
		if (_sortAsc)
		{
			list.Reverse();
		}
		foreach (Skill item in list)
		{
			_totalReal += item.Base;
			_totalValue += item.Value;
			Label skillname = new Label(item.Name, isunicode: true, 1153, 0, 3);
			Label skillvaluebase = new Label(item.Base.ToString(), isunicode: true, 1153, 0, 3);
			Label skillvalue = new Label(item.Value.ToString(), isunicode: true, 1153, 0, 3);
			Label skillcap = new Label(item.Cap.ToString(), isunicode: true, 1153, 0, 3);
			_skillListEntries.Add(new SkillListEntry(skillname, skillvaluebase, skillvalue, skillcap, item));
		}
		foreach (SkillListEntry skillListEntry2 in _skillListEntries)
		{
			_databox.Add(skillListEntry2);
		}
		_databox.WantUpdateSize = true;
		_databox.ReArrangeChildren();
		Label label = new Label(ResGumps.Total, isunicode: true, 1153);
		label.X = 40;
		label.Y = 320;
		Add(label);
		Label label2 = new Label(_totalReal.ToString("F1"), isunicode: true, 1153);
		label2.X = 220;
		label2.Y = 320;
		Add(label2);
		Label label3 = new Label(_totalValue.ToString("F1"), isunicode: true, 1153);
		label3.X = 300;
		label3.Y = 320;
		Add(label3);
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (!_updateSkillsNeeded)
		{
			return;
		}
		foreach (Label item in base.Children.OfType<Label>())
		{
			item.Dispose();
		}
		BuildGump();
		_updateSkillsNeeded = false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, base.Width, base.Height, hueVector);
		return base.Draw(batcher, x, y);
	}

	public void ForceUpdate()
	{
		_updateSkillsNeeded = true;
	}
}
