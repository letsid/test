using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class SkillButtonGump : AnchorableGump
{
	private Skill _skill;

	public override GumpType GumpType => GumpType.SkillButton;

	public int SkillID => _skill.Index;

	public SkillButtonGump(Skill skill, int x, int y)
		: this()
	{
		base.X = x;
		base.Y = y;
		_skill = skill;
		BuildGump();
	}

	public SkillButtonGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		base.WantUpdateSize = false;
		base.WidthMultiplier = 2;
		base.HeightMultiplier = 1;
		GroupMatrixWidth = 44;
		GroupMatrixHeight = 44;
		base.AnchorType = ANCHOR_TYPE.SPELL;
	}

	private void BuildGump()
	{
		base.Width = 88;
		base.Height = 44;
		ResizePic resizePic = new ResizePic(9400);
		resizePic.Width = base.Width;
		resizePic.Height = base.Height;
		resizePic.AcceptMouseInput = true;
		resizePic.CanMove = true;
		Add(resizePic);
		Label label = new Label(_skill.Name, isunicode: true, 0, base.Width - 8, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label.X = 4;
		label.Y = 0;
		label.Width = base.Width - 8;
		label.AcceptMouseInput = true;
		label.CanMove = true;
		Label label2 = label;
		Add(label);
		label2.Y = (base.Height >> 1) - (label2.Height >> 1);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		base.OnMouseUp(x, y, button);
		if (ProfileManager.CurrentProfile.CastSpellsByOneClick && button == MouseButtonType.Left && !Keyboard.Alt)
		{
			GameActions.UseSkill(_skill.Index);
		}
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (!ProfileManager.CurrentProfile.CastSpellsByOneClick && button == MouseButtonType.Left && !Keyboard.Alt)
		{
			GameActions.UseSkill(_skill.Index);
			return true;
		}
		return false;
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("id", _skill.Index.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		int num = int.Parse(xml.GetAttribute("id"));
		if (num >= 0 && num < World.Player.Skills.Length)
		{
			_skill = World.Player.Skills[num];
			BuildGump();
		}
		else
		{
			Dispose();
		}
	}
}
