using System;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game;

internal static class GameActions
{
	public static int LastSpellIndex { get; set; } = 1;

	public static int LastSkillIndex { get; set; } = 1;

	public static void ToggleWarMode()
	{
		RequestWarMode(!World.Player.InWarMode);
	}

	public static void RequestWarMode(bool war)
	{
		if (!World.Player.IsDead)
		{
			if (war && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableMusic)
			{
				Client.Game.Scene.Audio.PlayMusic(RandomHelper.GetValue(0, 3) % 3 + 38, iswarmode: true);
			}
			else if (!war)
			{
				Client.Game.Scene.Audio.StopWarMusic();
			}
		}
		NetClient.Socket.Send_ChangeWarMode(war);
	}

	public static void OpenMacroGump(string name)
	{
		UIManager.GetGump<MacroGump>(null)?.Dispose();
		UIManager.Add(new MacroGump(name));
	}

	public static void OpenPaperdoll(uint serial)
	{
		PaperDollGump gump = UIManager.GetGump<PaperDollGump>(serial);
		if (gump == null)
		{
			DoubleClick(serial | 0x80000000u);
			return;
		}
		if (gump.IsMinimized)
		{
			gump.IsMinimized = false;
		}
		gump.SetInScreen();
		gump.BringOnTop();
	}

	public static void OpenSettings(int page = 0)
	{
		OptionsGump gump = UIManager.GetGump<OptionsGump>(null);
		if (gump == null)
		{
			OptionsGump optionsGump = new OptionsGump();
			optionsGump.X = (Client.Game.Window.ClientBounds.Width >> 1) - 300;
			optionsGump.Y = (Client.Game.Window.ClientBounds.Height >> 1) - 250;
			UIManager.Add(optionsGump);
			optionsGump.ChangePage(page);
			optionsGump.SetInScreen();
		}
		else
		{
			gump.SetInScreen();
			gump.BringOnTop();
		}
	}

	public static void OpenStatusBar()
	{
		Client.Game.Scene.Audio.StopWarMusic();
		if (StatusGumpBase.GetStatusGump() == null)
		{
			UIManager.Add(StatusGumpBase.AddStatusGump(100, 100));
		}
	}

	public static void OpenJournal()
	{
		JournalGump gump = UIManager.GetGump<JournalGump>(null);
		if (gump == null)
		{
			JournalGump journalGump = new JournalGump(ProfileManager.CurrentProfile.JournalSkin);
			journalGump.X = ProfileManager.CurrentProfile.JournalPositionX;
			journalGump.Y = ProfileManager.CurrentProfile.JournalPositionY;
			UIManager.Add(journalGump);
			return;
		}
		gump.SetInScreen();
		gump.BringOnTop();
		if (gump.IsMinimized)
		{
			gump.IsMinimized = false;
		}
	}

	public static void OpenAdditionalJournal()
	{
		AdditionalJournalGump gump = UIManager.GetGump<AdditionalJournalGump>(null);
		if (gump == null)
		{
			AdditionalJournalGump additionalJournalGump = new AdditionalJournalGump(ProfileManager.CurrentProfile.JournalSkin);
			additionalJournalGump.X = ProfileManager.CurrentProfile.AdditionalJournalPositionX;
			additionalJournalGump.Y = ProfileManager.CurrentProfile.AdditionalJournalPositionY;
			UIManager.Add(additionalJournalGump);
			return;
		}
		gump.SetInScreen();
		gump.BringOnTop();
		if (gump.IsMinimized)
		{
			gump.IsMinimized = false;
		}
	}

	public static void ToggleAdditionalJournal()
	{
		AdditionalJournalGump gump = UIManager.GetGump<AdditionalJournalGump>(null);
		if (gump == null)
		{
			AdditionalJournalGump additionalJournalGump = new AdditionalJournalGump(ProfileManager.CurrentProfile.JournalSkin);
			additionalJournalGump.X = ProfileManager.CurrentProfile.AdditionalJournalPositionX;
			additionalJournalGump.Y = ProfileManager.CurrentProfile.AdditionalJournalPositionY;
			UIManager.Add(additionalJournalGump);
		}
		else
		{
			gump.SetInScreen();
			gump.BringOnTop();
			gump.Dispose();
		}
	}

	public static void OpenSkills()
	{
		StandardSkillsGump gump = UIManager.GetGump<StandardSkillsGump>(null);
		if (gump != null && gump.IsMinimized)
		{
			gump.IsMinimized = false;
			return;
		}
		World.SkillsRequested = true;
		NetClient.Socket.Send_SkillsRequest(World.Player.Serial);
	}

	public static void OpenMiniMap()
	{
		MiniMapGump gump = UIManager.GetGump<MiniMapGump>(null);
		if (gump == null)
		{
			UIManager.Add(new MiniMapGump());
			return;
		}
		gump.ToggleSize(null);
		gump.SetInScreen();
		gump.BringOnTop();
	}

	public static void OpenWorldMap()
	{
		WorldMapGump gump = UIManager.GetGump<WorldMapGump>(null);
		if (gump == null || gump.IsDisposed)
		{
			gump = new WorldMapGump();
			UIManager.Add(gump);
		}
		else
		{
			gump.BringOnTop();
			gump.SetInScreen();
		}
	}

	public static void OpenChat()
	{
		if (ChatManager.ChatIsEnabled == ChatStatus.Enabled)
		{
			ChatGump gump = UIManager.GetGump<ChatGump>(null);
			if (gump == null)
			{
				UIManager.Add(new ChatGump());
				return;
			}
			gump.SetInScreen();
			gump.BringOnTop();
		}
		else if (ChatManager.ChatIsEnabled == ChatStatus.EnabledUserRequest)
		{
			ChatGumpChooseName gump2 = UIManager.GetGump<ChatGumpChooseName>(null);
			if (gump2 == null)
			{
				UIManager.Add(new ChatGumpChooseName());
				return;
			}
			gump2.SetInScreen();
			gump2.BringOnTop();
		}
	}

	public static bool OpenCorpse(uint serial)
	{
		if (!SerialHelper.IsItem(serial))
		{
			return false;
		}
		Item item = World.Items.Get(serial);
		if (item == null || !item.IsCorpse || item.IsDestroyed)
		{
			return false;
		}
		World.Player.ManualOpenedCorpses.Add(serial);
		DoubleClick(serial);
		return true;
	}

	public static bool OpenBackpack()
	{
		Item item = World.Player.FindItemByLayer(Layer.Backpack);
		if (item == null)
		{
			return false;
		}
		ContainerGump gump = UIManager.GetGump<ContainerGump>(item);
		if (gump == null)
		{
			DoubleClick(item);
		}
		else
		{
			if (gump.IsMinimized)
			{
				gump.IsMinimized = false;
			}
			gump.SetInScreen();
			gump.BringOnTop();
		}
		return true;
	}

	public static void OpenHealthbar(uint serial, bool customBars)
	{
		UIManager.GetGump<TargetFrameHealthbarGump>(null)?.Dispose();
		UIManager.GetGump<TargetFrameHealthbarGumpCustom>(null)?.Dispose();
		Mobile mobile = World.Mobiles.Get(serial);
		if (mobile != null)
		{
			BaseHealthBarGump obj = (customBars ? ((BaseHealthBarGump)new TargetFrameHealthbarGumpCustom(mobile)) : ((BaseHealthBarGump)new TargetFrameHealthbarGump(mobile)));
			UIManager.Add(obj);
			Point location = ((ProfileManager.CurrentProfile == null) ? new Point(500, 500) : new Point(ProfileManager.CurrentProfile.TargetFrameHealthBarPositionX, ProfileManager.CurrentProfile.TargetFrameHealthBarPositionY));
			obj.Location = location;
			obj.SetInScreen();
		}
	}

	public static void Attack(uint serial)
	{
		if (ProfileManager.CurrentProfile.EnabledCriminalActionQuery)
		{
			Mobile mobile = World.Mobiles.Get(serial);
			if (mobile != null && (World.Player.NotorietyFlag == NotorietyFlag.Innocent || World.Player.NotorietyFlag == NotorietyFlag.Ally) && mobile.NotorietyFlag == NotorietyFlag.Innocent && mobile != World.Player)
			{
				UIManager.Add(new QuestionGump(ResGeneral.ThisMayFlagYouCriminal, delegate(bool s)
				{
					if (s)
					{
						NetClient.Socket.Send_AttackRequest(serial);
					}
				}));
				return;
			}
		}
		if (World.Player != null && !World.Player.InWarMode)
		{
			World.Player.InWarMode = true;
		}
		MacroManager.SetLastTarget(serial, ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowTargetingOverheadMessage);
		TargetManager.SelectedTarget = serial;
		TargetManager.LastAttack = serial;
		NetClient.Socket.Send_AttackRequest(serial);
	}

	public static void ShowHealthBarAndDispose(uint serial)
	{
		bool flag = ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.CustomBarsToggled;
		if (flag)
		{
			UIManager.GetGump<TargetFrameHealthbarGump>(null)?.Dispose();
		}
		else
		{
			UIManager.GetGump<TargetFrameHealthbarGumpCustom>(null)?.Dispose();
		}
		if (!(World.Mobiles.Get(TargetManager.SelectedTarget) != null) || (UIManager.GetGump<TargetFrameHealthbarGumpCustom>(TargetManager.SelectedTarget) == null && UIManager.GetGump<TargetFrameHealthbarGump>(TargetManager.SelectedTarget) == null))
		{
			OpenHealthbar(serial, flag);
		}
	}

	public static void DoubleClickQueued(uint serial)
	{
		Client.Game.GetScene<GameScene>()?.DoubleClickDelayed(serial);
	}

	public static void DoubleClick(uint serial)
	{
		if (serial != (uint)World.Player && SerialHelper.IsMobile(serial) && World.Player.InWarMode)
		{
			RequestMobileStatus(serial);
			Attack(serial);
		}
		else if (serial != (uint)World.Player && SerialHelper.IsMobile(serial))
		{
			if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableTargetWithoutWarmode)
			{
				TargetManager.SelectedTarget = serial;
				MacroManager.SetLastTarget(serial, overheadMessage: false);
			}
			NetClient.Socket.Send_DoubleClick(serial);
		}
		else
		{
			NetClient.Socket.Send_DoubleClick(serial);
		}
		if (SerialHelper.IsItem(serial))
		{
			World.LastObject = serial;
		}
	}

	public static void SingleClick(uint serial)
	{
		NetClient.Socket.Send_ClickRequest(serial);
		Entity entity = World.Get(serial);
		if (entity != null)
		{
			entity.IsClicked = true;
		}
	}

	public static void Say(string message, ushort hue = ushort.MaxValue, MessageType type = MessageType.Regular, byte font = 3)
	{
		if (hue == ushort.MaxValue)
		{
			hue = ProfileManager.CurrentProfile.SpeechHue;
		}
		if (Client.Version >= ClientVersion.CV_200)
		{
			NetClient.Socket.Send_UnicodeSpeechRequest(message, type, font, hue, Settings.GlobalSettings.Language);
		}
		else
		{
			NetClient.Socket.Send_ASCIISpeechRequest(message, type, font, hue);
		}
	}

	public static void Print(string message, ushort hue = 946, MessageType type = MessageType.Regular, byte font = 3, bool unicode = true)
	{
		Print(null, message, hue, type, font, unicode);
	}

	public static void Print(Entity entity, string message, ushort hue = 946, MessageType type = MessageType.Regular, byte font = 3, bool unicode = true)
	{
		MessageManager.HandleMessage(entity, message, (entity != null) ? entity.Name : "System", hue, type, font, (entity == null) ? TextType.SYSTEM : TextType.OBJECT, unicode, Settings.GlobalSettings.Language);
	}

	public static void SayParty(string message, uint serial = 0u)
	{
		NetClient.Socket.Send_PartyMessage(message, serial);
	}

	public static void RequestPartyAccept(uint serial)
	{
		NetClient.Socket.Send_PartyAccept(serial);
		UIManager.GetGump<PartyInviteGump>(null)?.Dispose();
	}

	public static void RequestPartyRemoveMemberByTarget()
	{
		NetClient.Socket.Send_PartyRemoveRequest(0u);
	}

	public static void RequestPartyRemoveMember(uint serial)
	{
		NetClient.Socket.Send_PartyRemoveRequest(serial);
	}

	public static void RequestPartyQuit()
	{
		NetClient.Socket.Send_PartyRemoveRequest(World.Player.Serial);
	}

	public static void RequestPartyInviteByTarget()
	{
		NetClient.Socket.Send_PartyInviteRequest();
	}

	public static void RequestPartyLootState(bool isLootable)
	{
		NetClient.Socket.Send_PartyChangeLootTypeRequest(isLootable);
	}

	public static bool PickUp(uint serial, int x, int y, int amount = -1, Point? offset = null, bool is_gump = false)
	{
		if (World.Player.IsDead || ItemHold.Enabled)
		{
			return false;
		}
		Item item = World.Items.Get(serial);
		if (item == null || item.IsDestroyed || item.IsMulti || (item.OnGround && (item.IsLocked || (item.Distance > 3 && World.Player.NotorietyFlag != NotorietyFlag.Staff))))
		{
			return false;
		}
		if (amount <= -1 && item.Amount > 1 && item.ItemData.IsStackable && ProfileManager.CurrentProfile.HoldShiftToSplitStack == Keyboard.Shift)
		{
			if (UIManager.GetGump<SplitMenuGump>(item) != null)
			{
				return false;
			}
			SplitMenuGump splitMenuGump = new SplitMenuGump(item, new Point(x, y));
			splitMenuGump.X = Mouse.Position.X - 80;
			splitMenuGump.Y = Mouse.Position.Y - 40;
			UIManager.Add(splitMenuGump);
			UIManager.AttemptDragControl(splitMenuGump, attemptAlwaysSuccessful: true);
			return true;
		}
		if (amount <= 0)
		{
			amount = item.Amount;
		}
		ItemHold.Clear();
		ItemHold.Set(item, (ushort)amount, offset);
		ItemHold.IsGumpTexture = is_gump;
		NetClient.Socket.Send_PickUpRequest(item, (ushort)amount);
		if (item.OnGround)
		{
			item.RemoveFromTile();
		}
		item.TextContainer?.Clear();
		World.ObjectToRemove = item.Serial;
		return true;
	}

	public static void DropItem(uint serial, int x, int y, int z, uint container)
	{
		if (ItemHold.Enabled && !ItemHold.IsFixedPosition && (ItemHold.Serial != container || ItemHold.ItemData.IsStackable))
		{
			if (Client.Version >= ClientVersion.CV_6017)
			{
				NetClient.Socket.Send_DropRequest(serial, (ushort)x, (ushort)y, (sbyte)z, 0, container);
			}
			else
			{
				NetClient.Socket.Send_DropRequest_Old(serial, (ushort)x, (ushort)y, (sbyte)z, container);
			}
			ItemHold.Enabled = false;
			ItemHold.Dropped = true;
		}
	}

	public static void Equip(uint container = 0u)
	{
		if (ItemHold.Enabled && !ItemHold.IsFixedPosition && ItemHold.ItemData.IsWearable)
		{
			if (!SerialHelper.IsValid(container))
			{
				container = World.Player.Serial;
			}
			NetClient.Socket.Send_EquipRequest(ItemHold.Serial, (Layer)ItemHold.ItemData.Layer, container);
			ItemHold.Enabled = false;
			ItemHold.Dropped = true;
		}
	}

	public static void ReplyGump(uint local, uint server, int button, uint[] switches = null, Tuple<ushort, string>[] entries = null)
	{
		NetClient.Socket.Send_GumpResponse(local, server, button, switches, entries);
	}

	public static void RequestHelp()
	{
		NetClient.Socket.Send_HelpRequest();
	}

	public static void RequestQuestMenu()
	{
		NetClient.Socket.Send_QuestMenuRequest();
	}

	public static void RequestProfile(uint serial)
	{
		NetClient.Socket.Send_ProfileRequest(serial);
	}

	public static void ChangeSkillLockStatus(ushort skillindex, byte lockstate)
	{
		NetClient.Socket.Send_SkillStatusChangeRequest(skillindex, lockstate);
	}

	public static void RequestMobileStatus(uint serial, bool force = false)
	{
		if (!World.InGame)
		{
			return;
		}
		Entity entity = World.Get(serial);
		if (entity != null)
		{
			if (force && entity.HitsRequest >= HitsRequestStatus.Pending)
			{
				SendCloseStatus(serial);
			}
			if (entity.HitsRequest < HitsRequestStatus.Received)
			{
				entity.HitsRequest = HitsRequestStatus.Pending;
				force = true;
			}
		}
		if (force && SerialHelper.IsValid(serial))
		{
			NetClient.Socket.Send_StatusRequest(serial);
		}
	}

	public static void SendCloseStatus(uint serial, bool force = false)
	{
		if (Client.Version >= ClientVersion.CV_200 && World.InGame)
		{
			Entity entity = World.Get(serial);
			if (entity != null && entity.HitsRequest >= HitsRequestStatus.Pending)
			{
				entity.HitsRequest = HitsRequestStatus.None;
				force = true;
			}
			if (force && SerialHelper.IsValid(serial))
			{
				NetClient.Socket.Send_CloseStatusBarGump(serial);
			}
		}
	}

	public static void CastSpellFromBook(int index, uint bookSerial)
	{
		if (index >= 0)
		{
			LastSpellIndex = index;
			NetClient.Socket.Send_CastSpellFromBook(index, bookSerial);
		}
	}

	public static void CastSpell(int index)
	{
		if (index >= 0)
		{
			LastSpellIndex = index;
			NetClient.Socket.Send_CastSpell(index);
		}
	}

	public static void OpenGuildGump()
	{
		NetClient.Socket.Send_GuildMenuRequest();
	}

	public static void ChangeStatLock(byte stat, Lock state)
	{
		NetClient.Socket.Send_StatLockStateRequest(stat, state);
	}

	public static void Rename(uint serial, string name)
	{
		NetClient.Socket.Send_RenameRequest(serial, name);
	}

	internal static void OpenActionBook()
	{
		StackDataWriter stackDataWriter = default(StackDataWriter);
		stackDataWriter.WriteInt8(12);
		stackDataWriter.WriteUInt16BE(7);
		stackDataWriter.WriteUInt16BE(9);
		stackDataWriter.WriteUInt16BE(2);
		NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
	}

	public static void UseSkill(int index)
	{
		if (index >= 0)
		{
			LastSkillIndex = index;
			NetClient.Socket.Send_UseSkill(index);
		}
	}

	public static void OpenPopupMenu(uint serial, bool shift = false)
	{
		shift = shift || Keyboard.Shift;
		if (!ProfileManager.CurrentProfile.HoldShiftForContext || shift)
		{
			NetClient.Socket.Send_RequestPopupMenu(serial);
		}
	}

	public static void ResponsePopupMenu(uint serial, ushort index)
	{
		NetClient.Socket.Send_PopupMenuSelection(serial, index);
	}

	public static void MessageOverhead(string message, uint entity)
	{
		Print(World.Get(entity), message, 946, MessageType.Regular, 3);
	}

	public static void MessageOverhead(string message, ushort hue, uint entity)
	{
		Print(World.Get(entity), message, hue, MessageType.Regular, 3);
	}

	public static void AcceptTrade(uint serial, bool accepted)
	{
		NetClient.Socket.Send_TradeResponse(serial, 2, accepted);
	}

	public static void CancelTrade(uint serial)
	{
		NetClient.Socket.Send_TradeResponse(serial, 1, state: false);
	}

	public static void AllNames()
	{
		foreach (Mobile value in World.Mobiles.Values)
		{
			if (value != World.Player)
			{
				NetClient.Socket.Send_ClickRequest(value.Serial);
			}
		}
		foreach (Item value2 in World.Items.Values)
		{
			if (value2.IsCorpse)
			{
				NetClient.Socket.Send_ClickRequest(value2.Serial);
			}
		}
	}

	public static void OpenDoor()
	{
		NetClient.Socket.Send_OpenDoor();
	}

	public static void EmoteAction(string action)
	{
		NetClient.Socket.Send_EmoteAction(action);
	}

	public static void OpenAbilitiesBook()
	{
		if (UIManager.GetGump<CombatBookGump>(null) == null)
		{
			UIManager.Add(new CombatBookGump(100, 100));
		}
	}

	public static void UsePrimaryAbility()
	{
		ref Ability reference = ref World.Player.Abilities[0];
		if (((byte)reference & 0x80) == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				World.Player.Abilities[i] &= (Ability)127;
			}
			NetClient.Socket.Send_UseCombatAbility((byte)reference);
		}
		else
		{
			NetClient.Socket.Send_UseCombatAbility(0);
		}
		reference ^= (Ability)128;
	}

	public static void UseSecondaryAbility()
	{
		ref Ability reference = ref World.Player.Abilities[1];
		if (((byte)reference & 0x80) == 0)
		{
			for (int i = 0; i < 2; i++)
			{
				World.Player.Abilities[i] &= (Ability)127;
			}
			NetClient.Socket.Send_UseCombatAbility((byte)reference);
		}
		else
		{
			NetClient.Socket.Send_UseCombatAbility(0);
		}
		reference ^= (Ability)128;
	}

	public static void QuestArrow(bool rightClick)
	{
		NetClient.Socket.Send_ClickQuestArrow(rightClick);
	}

	public static void GrabItem(uint serial, ushort amount, uint bag = 0u)
	{
		Item item = World.Player.FindItemByLayer(Layer.Backpack);
		if (!(item == null))
		{
			if (bag == 0)
			{
				bag = ((ProfileManager.CurrentProfile.GrabBagSerial == 0) ? item.Serial : ProfileManager.CurrentProfile.GrabBagSerial);
			}
			if (!World.Items.Contains(bag))
			{
				Print(ResGeneral.GrabBagNotFound, 946, MessageType.Regular, 3);
				ProfileManager.CurrentProfile.GrabBagSerial = 0u;
				bag = item.Serial;
			}
			PickUp(serial, 0, 0, amount, null);
			DropItem(serial, 65535, 65535, 0, bag);
		}
	}
}
