using System;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class UseSpellButtonGump : AnchorableGump
{
	private const ushort LOCK_GRAPHIC = 4230;

	private GumpPic _background;

	private SpellDefinition _spell;

	private readonly MacroManager _mm;

	public bool ShowEdit
	{
		get
		{
			if (Keyboard.Ctrl && Keyboard.Alt)
			{
				return ProfileManager.CurrentProfile.FastSpellsAssign;
			}
			return false;
		}
	}

	public override GumpType GumpType => GumpType.SpellButton;

	public int SpellID => _spell?.ID ?? 0;

	public ushort Hue
	{
		set
		{
			_background.Hue = value;
		}
	}

	public UseSpellButtonGump()
		: base(0u, 0u)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		_mm = Client.Game.GetScene<GameScene>().Macros;
	}

	public UseSpellButtonGump(SpellDefinition spell)
		: this()
	{
		_spell = spell;
		BuildGump();
	}

	private void BuildGump()
	{
		GumpPic obj = new GumpPic(0, 0, (ushort)_spell.GumpIconSmallID, 0)
		{
			AcceptMouseInput = false
		};
		GumpPic c = obj;
		_background = obj;
		Add(c);
		int spellTooltip = GetSpellTooltip(_spell.ID);
		if (spellTooltip != 0)
		{
			SetTooltip(ClilocLoader.Instance.GetString(spellTooltip), 80);
		}
		base.WantUpdateSize = true;
		AcceptMouseInput = true;
		GroupMatrixWidth = 44;
		GroupMatrixHeight = 44;
		base.AnchorType = ANCHOR_TYPE.SPELL;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		if (ShowEdit)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(4230u, out bounds);
			if (gumpTexture != null)
			{
				if (UIManager.MouseOverControl != null && (UIManager.MouseOverControl == this || UIManager.MouseOverControl.RootParent == this))
				{
					hueVector.X = 34f;
					hueVector.Y = 1f;
				}
				batcher.Draw(gumpTexture, new Vector2(x + (base.Width - bounds.Width), y), bounds, hueVector);
			}
		}
		return true;
	}

	private int GetSpellsId()
	{
		int num = _spell.ID % 100;
		if (num > 78)
		{
			return num - 78;
		}
		return num;
	}

	private static int GetSpellTooltip(int id)
	{
		if (id >= 1 && id <= 64)
		{
			return 3002011 + (id - 1);
		}
		if (id >= 101 && id <= 117)
		{
			return 1060509 + (id - 101);
		}
		if (id >= 201 && id <= 210)
		{
			return 1060585 + (id - 201);
		}
		if (id >= 401 && id <= 406)
		{
			return 1060595 + (id - 401);
		}
		if (id >= 501 && id <= 508)
		{
			return 1060610 + (id - 501);
		}
		if (id >= 601 && id <= 616)
		{
			return 1071026 + (id - 601);
		}
		if (id >= 678 && id <= 693)
		{
			return 1031678 + (id - 678);
		}
		switch (id)
		{
		case 701:
		case 702:
		case 703:
		case 704:
		case 705:
		case 706:
			return 1115612 + (id - 701);
		case 707:
		case 708:
		case 709:
		case 710:
		case 711:
		case 712:
		case 713:
		case 714:
		case 715:
		case 716:
		case 717:
		case 718:
		case 719:
		case 720:
		case 721:
		case 722:
		case 723:
		case 724:
		case 725:
		case 726:
		case 727:
		case 728:
		case 729:
		case 730:
		case 731:
		case 732:
		case 733:
		case 734:
		case 735:
		case 736:
		case 737:
		case 738:
		case 739:
		case 740:
		case 741:
		case 742:
		case 743:
		case 744:
		case 745:
			if (id <= 745)
			{
				return 1155896 + (id - 707);
			}
			break;
		}
		return 0;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		base.OnMouseUp(x, y, button);
		Point lDragOffset = Mouse.LDragOffset;
		if (button == MouseButtonType.Left && ShowEdit)
		{
			Macro item = Macro.CreateFastMacro(_spell.Name, MacroType.CastSpell, (MacroSubType)(GetSpellsId() + SpellBookDefinition.GetSpellsGroup(_spell.ID)));
			if (_mm.FindMacro(_spell.Name) == null)
			{
				_mm.MoveToBack(item);
			}
			GameActions.OpenMacroGump(_spell.Name);
		}
		if (ProfileManager.CurrentProfile.CastSpellsByOneClick && button == MouseButtonType.Left && Math.Abs(lDragOffset.X) < 5 && Math.Abs(lDragOffset.Y) < 5)
		{
			GameActions.CastSpell(_spell.ID);
		}
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (!ProfileManager.CurrentProfile.CastSpellsByOneClick && button == MouseButtonType.Left)
		{
			GameActions.CastSpell(_spell.ID);
			return true;
		}
		return false;
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("id", _spell.ID.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		_spell = SpellDefinition.FullIndexGetSpell(int.Parse(xml.GetAttribute("id")));
		BuildGump();
	}
}
