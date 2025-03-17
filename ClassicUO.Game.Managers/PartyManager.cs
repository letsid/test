using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.Network;

namespace ClassicUO.Game.Managers;

internal class PartyManager
{
	public const int PARTY_SIZE = 30;

	public uint Leader { get; set; }

	public uint Inviter { get; set; }

	public bool CanLoot { get; set; }

	public PartyMember[] Members { get; } = new PartyMember[30];

	public long PartyHealTimer { get; set; }

	public uint PartyHealTarget { get; set; }

	public void ParsePacket(ref StackDataReader p)
	{
		byte b = p.ReadUInt8();
		bool flag = false;
		switch (b)
		{
		case 1:
			flag = true;
			goto case 2;
		case 2:
		{
			byte b2 = p.ReadUInt8();
			if (b2 <= 1)
			{
				Leader = 0u;
				Inviter = 0u;
				for (int j = 0; j < 30 && Members[j] != null && Members[j].Serial != 0; j++)
				{
					BaseHealthBarGump gump = UIManager.GetGump<BaseHealthBarGump>(Members[j].Serial);
					if (gump != null)
					{
						if (b == 2)
						{
							Members[j].Serial = 0u;
						}
						gump.RequestUpdateContents();
					}
				}
				Clear();
				UIManager.GetGump<PartyGump>(null)?.RequestUpdateContents();
				break;
			}
			Clear();
			uint num2 = uint.MaxValue;
			if (!flag)
			{
				num2 = p.ReadUInt32BE();
				UIManager.GetGump<BaseHealthBarGump>(num2)?.RequestUpdateContents();
			}
			bool flag2 = !flag && num2 == (uint)World.Player;
			int num3 = 0;
			for (int k = 0; k < b2; k++)
			{
				uint num4 = p.ReadUInt32BE();
				bool flag3 = !flag && num4 == num2;
				if (flag3 && num4 == num2 && k == 0)
				{
					flag2 = true;
				}
				if (!flag3 && !flag2)
				{
					if (!Contains(num4))
					{
						Members[k] = new PartyMember(num4);
						if (string.IsNullOrEmpty(Members[k].Name))
						{
							NetClient.Socket.Send_NameRequest(num4);
						}
					}
					num3++;
				}
				if (k == 0 && !flag3 && !flag2)
				{
					Leader = num4;
				}
				BaseHealthBarGump gump2 = UIManager.GetGump<BaseHealthBarGump>(num4);
				if (gump2 != null)
				{
					gump2.RequestUpdateContents();
				}
				else
				{
					_ = (uint)World.Player;
				}
			}
			if (num3 <= 1 && !flag)
			{
				for (int l = 0; l < 30; l++)
				{
					if (Members[l] != null && SerialHelper.IsValid(Members[l].Serial))
					{
						uint serial = Members[l].Serial;
						Members[l] = null;
						UIManager.GetGump<BaseHealthBarGump>(serial)?.RequestUpdateContents();
					}
				}
				Clear();
			}
			UIManager.GetGump<PartyGump>(null)?.RequestUpdateContents();
			break;
		}
		case 3:
		case 4:
		{
			uint num = p.ReadUInt32BE();
			string text = p.ReadUnicodeBE();
			for (int i = 0; i < 30; i++)
			{
				if (Members[i] != null && Members[i].Serial == num)
				{
					MessageManager.HandleMessage(null, text, Members[i].Name, ProfileManager.CurrentProfile.PartyMessageHue, MessageType.Party, 3, TextType.GUILD_ALLY, unicode: true, "DEU");
					break;
				}
			}
			break;
		}
		case 7:
			Inviter = p.ReadUInt32BE();
			if (ProfileManager.CurrentProfile.AutoAcceptInvitesOfGuildMembers)
			{
				Mobile mobile = World.Get(Inviter) as Mobile;
				if (mobile != null && mobile.NotorietyFlag == NotorietyFlag.Ally)
				{
					GameActions.RequestPartyAccept(mobile.Serial);
					World.Party.Leader = mobile.Serial;
					World.Party.Inviter = 0u;
					break;
				}
			}
			if (ProfileManager.CurrentProfile.PartyInviteGump)
			{
				UIManager.Add(new PartyInviteGump(Inviter));
			}
			break;
		case 5:
		case 6:
			break;
		}
	}

	public void SetName(uint serial, string name)
	{
		for (int i = 0; i < 30; i++)
		{
			PartyMember partyMember = Members[i];
			if (partyMember != null && partyMember.Serial == serial)
			{
				partyMember.Name = name;
				break;
			}
		}
	}

	public bool Contains(uint serial)
	{
		for (int i = 0; i < 30; i++)
		{
			PartyMember partyMember = Members[i];
			if (partyMember != null && partyMember.Serial == serial)
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		Leader = 0u;
		Inviter = 0u;
		for (int i = 0; i < 30; i++)
		{
			Members[i] = null;
		}
	}
}
