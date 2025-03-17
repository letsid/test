using System.Xml;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;

namespace ClassicUO.Game.UI.Gumps;

internal class RacialAbilityButton : Gump
{
	public ushort Graphic;

	public override GumpType GumpType => GumpType.RacialButton;

	public RacialAbilityButton(ushort graphic)
		: this()
	{
		base.LocalSerial = (uint)(7000 + graphic);
		UIManager.GetGump<RacialAbilityButton>(base.LocalSerial)?.Dispose();
		Graphic = graphic;
		BuildGump();
	}

	public RacialAbilityButton()
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
	}

	private void BuildGump()
	{
		GumpPic gumpPic = new GumpPic(0, 0, Graphic, 0);
		Add(gumpPic);
		gumpPic.SetTooltip(ClilocLoader.Instance.GetString(1112198 + (Graphic - 24016)), 200);
	}

	protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
	{
		if (Graphic == 24026 && World.Player.Race == RaceType.GARGOYLE)
		{
			NetClient.Socket.Send_ToggleGargoyleFlying();
			return true;
		}
		return base.OnMouseDoubleClick(x, y, button);
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("graphic", Graphic.ToString());
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		Graphic = ushort.Parse(xml.GetAttribute("graphic"));
		BuildGump();
	}
}
