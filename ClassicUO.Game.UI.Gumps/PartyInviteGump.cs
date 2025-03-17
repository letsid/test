using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Network;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class PartyInviteGump : Gump
{
	public PartyInviteGump(uint inviter)
		: base(0u, 0u)
	{
		base.CanCloseWithRightClick = true;
		Mobile mobile = World.Mobiles.Get(inviter);
		int num = ((!(mobile == null) && mobile.Name.Length >= 10) ? (mobile.Name.Length * 5) : 0);
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl();
		alphaBlendControl.Width = 270 + num;
		alphaBlendControl.Height = 80;
		alphaBlendControl.X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125;
		alphaBlendControl.Y = 150;
		alphaBlendControl.Alpha = 0.8f;
		AlphaBlendControl c = alphaBlendControl;
		Label label = new Label(string.Format(ResGumps.P0HasInvitedYouToParty, (mobile == null || string.IsNullOrEmpty(mobile.Name)) ? ResGumps.NoName : mobile.Name), isunicode: true, 15);
		label.X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 115;
		label.Y = 165;
		Label c2 = label;
		NiceButton niceButton = new NiceButton(ProfileManager.CurrentProfile.GameWindowSize.X / 2 + 99 + num, 205, 45, 25, ButtonAction.Activate, ResGumps.Accept);
		NiceButton niceButton2 = new NiceButton(ProfileManager.CurrentProfile.GameWindowSize.X / 2 + 39 + num, 205, 45, 25, ButtonAction.Activate, ResGumps.Decline);
		Add(c);
		Add(c2);
		Add(niceButton);
		Add(niceButton2);
		niceButton.MouseUp += delegate
		{
			if (World.Party.Inviter != 0 && World.Party.Leader == 0)
			{
				GameActions.RequestPartyAccept(World.Party.Inviter);
				World.Party.Leader = World.Party.Inviter;
				World.Party.Inviter = 0u;
			}
			Dispose();
		};
		niceButton2.MouseUp += delegate
		{
			if (World.Party.Inviter != 0 && World.Party.Leader == 0)
			{
				NetClient.Socket.Send_PartyDecline(World.Party.Inviter);
				World.Party.Inviter = 0u;
			}
			Dispose();
		};
	}
}
