using System.Xml;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal class UseAbilityButtonGump : AnchorableGump
{
	private GumpPic _button;

	public override GumpType GumpType => GumpType.AbilityButton;

	public int Index { get; private set; }

	public bool IsPrimary { get; private set; }

	public UseAbilityButtonGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
	}

	public UseAbilityButtonGump(bool primary)
		: this()
	{
		IsPrimary = primary;
		BuildGump();
	}

	private void BuildGump()
	{
		Clear();
		Index = (byte)World.Player.Abilities[(!IsPrimary) ? 1u : 0u] & 0x7F;
		_button = new GumpPic(0, 0, AbilityData.Abilities[Index - 1].Icon, 0)
		{
			AcceptMouseInput = false
		};
		Add(_button);
		SetTooltip(ClilocLoader.Instance.GetString(1028838 + (Index - 1)), 80);
		base.WantUpdateSize = true;
		AcceptMouseInput = true;
		GroupMatrixWidth = 44;
		GroupMatrixHeight = 44;
		base.AnchorType = ANCHOR_TYPE.SPELL;
	}

	protected override void UpdateContents()
	{
		BuildGump();
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			if (IsPrimary)
			{
				GameActions.UsePrimaryAbility();
			}
			else
			{
				GameActions.UseSecondaryAbility();
			}
			return true;
		}
		return false;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (base.IsDisposed)
		{
			return false;
		}
		if (((byte)World.Player.Abilities[(!IsPrimary) ? 1u : 0u] & 0x80) != 0)
		{
			_button.Hue = 38;
		}
		else if (_button.Hue != 0)
		{
			_button.Hue = 0;
		}
		return base.Draw(batcher, x, y);
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("isprimary", IsPrimary.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		IsPrimary = bool.Parse(xml.GetAttribute("isprimary"));
		BuildGump();
	}
}
