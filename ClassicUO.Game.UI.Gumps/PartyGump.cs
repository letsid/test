using System.Linq;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;

namespace ClassicUO.Game.UI.Gumps;

internal class PartyGump : Gump
{
	private enum Buttons
	{
		OK = 0,
		Cancel = 1,
		SendMessage = 2,
		LootType = 3,
		Leave = 4,
		Add = 5,
		TellMember = 6,
		KickMember = 36
	}

	public bool CanLoot;

	public PartyGump(int x, int y, bool canloot)
		: base(0u, 0u)
	{
		base.X = x;
		base.Y = y;
		CanLoot = canloot;
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		BuildGump();
	}

	protected override void UpdateContents()
	{
		Clear();
		BuildGump();
	}

	private void BuildGump()
	{
		ResizePic resizePic = new ResizePic(2600);
		resizePic.Width = 450;
		resizePic.Height = 480;
		Add(resizePic);
		Label label = new Label(ResGumps.Tell, isunicode: false, 902, 0, 1);
		label.X = 40;
		label.Y = 30;
		Add(label);
		Label label2 = new Label(ResGumps.Kick, isunicode: false, 902, 0, 1);
		label2.X = 80;
		label2.Y = 30;
		Add(label2);
		if (World.Party == null || World.Party.Leader == 0)
		{
			Label label3 = new Label(ResGumps.PartyManifest, isunicode: false, 902, 0, 2);
			label3.X = 153;
			label3.Y = 20;
			Add(label3);
		}
		else
		{
			int num = World.Party.Members.Count((PartyMember x) => x != null);
			Label label4 = new Label($"{ResGumps.PartyManifest} {num} / {30} Mitgliedern", isunicode: false, 902, 0, 2);
			label4.X = 140;
			label4.Y = 20;
			Add(label4);
		}
		bool flag = World.Party.Leader == 0 || World.Party.Leader == (uint)World.Player;
		bool flag2 = World.Party.Leader != 0 && World.Party.Leader != (uint)World.Player;
		int num2 = 0;
		ScrollArea scrollArea = new ScrollArea(40, 48, 380, 250, normalScrollbar: true);
		Add(scrollArea);
		for (int i = 0; i < 30; i++)
		{
			Button button = new Button(6 + i, 4011, 4013, 4012, "", 0);
			button.X = 0;
			button.Y = num2 + 2;
			button.ButtonAction = ButtonAction.Activate;
			scrollArea.Add(button);
			if (flag)
			{
				Button button2 = new Button(36 + i, 4017, 4019, 4018, "", 0);
				button2.X = 40;
				button2.Y = num2 + 2;
				button2.ButtonAction = ButtonAction.Activate;
				scrollArea.Add(button2);
			}
			scrollArea.Add(new GumpPic(90, num2, 1141, 0));
			string text = "";
			if (World.Party.Members[i] != null && World.Party.Members[i].Name != null)
			{
				text = $"[{i + 1}] {World.Party.Members[i].Name}";
			}
			Label label5 = new Label(text, isunicode: false, 902, 250, 2, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
			label5.X = 100;
			label5.Y = num2 + 1;
			scrollArea.Add(label5);
			num2 += 25;
		}
		Button button3 = new Button(2, 4011, 4013, 4012, "", 0);
		button3.X = 70;
		button3.Y = 307;
		button3.ButtonAction = ButtonAction.Activate;
		Add(button3);
		Label label6 = new Label(ResGumps.SendThePartyAMessage, isunicode: false, 902, 0, 2);
		label6.X = 110;
		label6.Y = 307;
		Add(label6);
		if (CanLoot)
		{
			Button button4 = new Button(3, 4002, 4002, 4002, "", 0);
			button4.X = 70;
			button4.Y = 334;
			button4.ButtonAction = ButtonAction.Activate;
			Add(button4);
			Label label7 = new Label(ResGumps.PartyCanLootMe, isunicode: false, 902, 0, 2);
			label7.X = 110;
			label7.Y = 334;
			Add(label7);
		}
		else
		{
			Button button5 = new Button(3, 4009, 4009, 4009, "", 0);
			button5.X = 70;
			button5.Y = 334;
			button5.ButtonAction = ButtonAction.Activate;
			Add(button5);
			Label label8 = new Label(ResGumps.PartyCannotLootMe, isunicode: false, 902, 0, 2);
			label8.X = 110;
			label8.Y = 334;
			Add(label8);
		}
		Button button6 = new Button(4, 4014, 4016, 4015, "", 0);
		button6.X = 70;
		button6.Y = 360;
		button6.ButtonAction = ButtonAction.Activate;
		Add(button6);
		if (flag2)
		{
			Label label9 = new Label(ResGumps.LeaveTheParty, isunicode: false, 902, 0, 2);
			label9.X = 110;
			label9.Y = 360;
			Add(label9);
		}
		else
		{
			Label label10 = new Label(ResGumps.DisbandTheParty, isunicode: false, 902, 0, 2);
			label10.X = 110;
			label10.Y = 360;
			Add(label10);
		}
		Button button7 = new Button(5, 4008, 4010, 4009, "", 0);
		button7.X = 70;
		button7.Y = 385;
		button7.ButtonAction = ButtonAction.Activate;
		Add(button7);
		Label label11 = new Label(ResGumps.AddNewMember, isunicode: false, 902, 0, 2);
		label11.X = 110;
		label11.Y = 385;
		Add(label11);
		Button button8 = new Button(0, 249, 248, 247, "", 0);
		button8.X = 130;
		button8.Y = 430;
		button8.ButtonAction = ButtonAction.Activate;
		Add(button8);
		Button button9 = new Button(1, 243, 241, 242, "", 0);
		button9.X = 236;
		button9.Y = 430;
		button9.ButtonAction = ButtonAction.Activate;
		Add(button9);
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			if (World.Party.Leader != 0 && World.Party.CanLoot != CanLoot)
			{
				World.Party.CanLoot = CanLoot;
				NetClient.Socket.Send_PartyChangeLootTypeRequest(CanLoot);
			}
			Dispose();
			return;
		case 1:
			Dispose();
			return;
		case 2:
			if (World.Party.Leader == 0)
			{
				GameActions.Print(ResGumps.YouAreNotInAParty, 0, MessageType.System, 3, unicode: false);
			}
			else
			{
				UIManager.SystemChat.TextBoxControl.SetText("/");
			}
			return;
		case 3:
			CanLoot = !CanLoot;
			RequestUpdateContents();
			return;
		case 4:
			if (World.Party.Leader == 0)
			{
				GameActions.Print(ResGumps.YouAreNotInAParty, 0, MessageType.System, 3, unicode: false);
			}
			else
			{
				GameActions.RequestPartyQuit();
			}
			return;
		case 5:
			if (World.Party.Leader == 0 || World.Party.Leader == (uint)World.Player)
			{
				NetClient.Socket.Send_PartyInviteRequest();
			}
			return;
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		{
			int num = buttonID - 6;
			if (World.Party.Members[num] == null || World.Party.Members[num].Serial == 0)
			{
				GameActions.Print(ResGumps.ThereIsNoOneInThatPartySlot, 0, MessageType.System, 3, unicode: false);
			}
			else
			{
				UIManager.SystemChat.TextBoxControl.SetText($"/{num + 1} ");
			}
			return;
		}
		}
		if (buttonID >= 36)
		{
			int num2 = buttonID - 36;
			if (World.Party.Members[num2] == null || World.Party.Members[num2].Serial == 0)
			{
				GameActions.Print(ResGumps.ThereIsNoOneInThatPartySlot, 0, MessageType.System, 3, unicode: false);
			}
			else
			{
				NetClient.Socket.Send_PartyRemoveRequest(World.Party.Members[num2].Serial);
			}
		}
	}
}
