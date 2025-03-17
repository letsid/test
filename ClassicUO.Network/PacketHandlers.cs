using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Map;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Network;

internal class PacketHandlers
{
	public delegate void OnPacketBufferReader(ref StackDataReader p);

	public enum CustomClientPacketType
	{
		CustomClientPacket_Checksum = 1,
		CustomClientPacket_UserId = 2,
		CustomClientPacket_GamePlayArea = 3,
		CustomClientPacket_ClientInfo = 4,
		CustomClientPacket_Away = 5,
		CustomClientPacket_TargetInfo = 6,
		CustomClientPacket_Command = 9,
		CustomClientPacket_OffsetData = 10
	}

	public enum PacketCommand
	{
		Bonus = 1,
		ActionBook
	}

	private enum CustomServerPacketType : ushort
	{
		CustomServerPacket_CheckChecksum,
		CustomServerPacket_ApplySpeedLimit,
		CustomServerPacket_AddShovableAnims,
		CustomServerPacket_SetChatColors,
		CustomServerPacket_SetDynamicClilocEntry,
		CustomServerPacket_SetAFK,
		CustomServerPacket_ClientAuth,
		CustomServerPacket_Season,
		CustomServerPacket_Clipboard,
		CustomServerPacket_ShakeEffect,
		CustomServerPacket_OffsetRequest,
		CustomServerPacket_ShowNoDrawItems,
		CustomServerPacket_ExtendedStatus
	}

	[Flags]
	private enum AffixType
	{
		Append = 0,
		Prepend = 1,
		System = 2
	}

	private static uint _requestedGridLoot;

	private static readonly TextFileParser _parser = new TextFileParser(string.Empty, new char[1] { ' ' }, new char[0], new char[2] { '{', '}' });

	private static readonly TextFileParser _cmdparser = new TextFileParser(string.Empty, new char[2] { ' ', ',' }, new char[0], new char[2] { '@', '@' });

	private List<uint> _clilocRequests = new List<uint>();

	private readonly OnPacketBufferReader[] _handlers = new OnPacketBufferReader[256];

	public static DateTime ResendGameWindowTimeout = DateTime.Now;

	public static bool ResendGameWindow = true;

	public static PacketHandlers Handlers { get; } = new PacketHandlers();

	public static void GetOrSetUserIDAndSend()
	{
		Guid empty = Guid.Empty;
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Alathair");
		string path = Path.Combine(text, "config.dat");
		if (File.Exists(path))
		{
			empty = new Guid(File.ReadAllBytes(path));
		}
		else
		{
			empty = Guid.NewGuid();
			Directory.CreateDirectory(text);
			File.WriteAllBytes(path, empty.ToByteArray());
		}
		Span<byte> span = stackalloc byte[16];
		string text2 = empty.ToString("N");
		for (int i = 0; i < text2.Length; i += 2)
		{
			span[i / 2] = Convert.ToByte(text2.Substring(i, 2), 16);
		}
		StackDataWriter stackDataWriter = new StackDataWriter(21);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteUInt16BE(21);
		stackDataWriter.WriteUInt16BE(2);
		stackDataWriter.Write(span.ToArray());
		NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		stackDataWriter.Dispose();
	}

	public void Add(byte id, OnPacketBufferReader handler)
	{
		_handlers[id] = handler;
	}

	public static void ResendGameWindowSize()
	{
		int x = ProfileManager.CurrentProfile.GameWindowPosition.X;
		int y = ProfileManager.CurrentProfile.GameWindowPosition.Y;
		int x2 = ProfileManager.CurrentProfile.GameWindowSize.X;
		int y2 = ProfileManager.CurrentProfile.GameWindowSize.Y;
		int width = Client.Game.Window.ClientBounds.Width;
		int height = Client.Game.Window.ClientBounds.Height;
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		StackDataWriter stackDataWriter = default(StackDataWriter);
		stackDataWriter.WriteUInt8(12);
		stackDataWriter.WriteInt16BE(17);
		stackDataWriter.WriteInt16BE(3);
		stackDataWriter.WriteInt16BE((short)x);
		stackDataWriter.WriteInt16BE((short)y);
		stackDataWriter.WriteInt16BE((short)x2);
		stackDataWriter.WriteInt16BE((short)y2);
		stackDataWriter.WriteInt16BE((short)width);
		stackDataWriter.WriteInt16BE((short)height);
		NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten);
		ResendGameWindow = false;
	}

	private static void Custompacket(ref StackDataReader p)
	{
		switch (p.ReadUInt8())
		{
		case 1:
			ApplySpeedLimit(p);
			break;
		case 2:
			AddShovableAnims(p);
			break;
		case 3:
			CustomPacket_SetChatColors(p);
			break;
		case 5:
			Custompacket_SetAfk(p);
			break;
		case 6:
			CheckSumCalculator.AddCheckSumJob(new CheckSumJob(Process.GetCurrentProcess().MainModule.FileName));
			NetClient.Socket.Send_CustomClientVersion();
			GetOrSetUserIDAndSend();
			break;
		case 7:
			CustomSeason(ref p);
			break;
		case 8:
			SetClipboard(p);
			break;
		case 9:
			ShakeEffectManager.ApplyShakeEffect(p);
			break;
		case 11:
			CustomPacket_NoDrawTiles(p);
			break;
		case 12:
			SetResistances(p);
			break;
		case 13:
			CustomBuffDebuff(p);
			break;
		case 14:
			HandleTextActivity(p);
			break;
		case 0:
		case 4:
		case 10:
			break;
		}
	}

	private static void HandleTextActivity(StackDataReader p)
	{
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowChatActivity)
		{
			Entity entity = World.Get(p.ReadUInt32BE());
			if (entity is Mobile)
			{
				((Mobile)entity)._lastTextActivity = Time.Ticks;
			}
		}
	}

	private static void CustomBuffDebuff(StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		ushort time = p.ReadUInt16BE();
		ushort graphic = p.ReadUInt16BE();
		ushort num = p.ReadUInt16BE();
		ushort hue = p.ReadUInt16BE();
		bool flag = p.ReadBool();
		uint num2 = p.ReadUInt32BE();
		string text = p.ReadUnicodeBE();
		if (num == 0)
		{
			return;
		}
		BuffGump gump = UIManager.GetGump<BuffGump>(null);
		if (!flag)
		{
			World.Player.RemoveBuff(num);
			gump?.RequestUpdateContents();
			return;
		}
		string empty = string.Empty;
		if (num2 != 0 && empty.Length < 2)
		{
			empty = string.Empty;
		}
		string text2 = "<left>" + text + empty + "</left>";
		bool num3 = World.Player.IsBuffIconExists(graphic);
		World.Player.AddBuff(num, graphic, hue, time, text2);
		if (!num3)
		{
			gump?.RequestUpdateContents();
		}
	}

	private static void InvalidateAllowedToDraw()
	{
		foreach (KeyValuePair<uint, Item> item in World.Items)
		{
			Item value = item.Value;
			if (value != null)
			{
				value.CheckGraphicChange(0);
			}
		}
		foreach (Chunk usedChunk in World.Map.GetUsedChunks())
		{
			GameObject[,] tiles = usedChunk.Tiles;
			int upperBound = tiles.GetUpperBound(0);
			int upperBound2 = tiles.GetUpperBound(1);
			for (int i = tiles.GetLowerBound(0); i <= upperBound; i++)
			{
				for (int j = tiles.GetLowerBound(1); j <= upperBound2; j++)
				{
					for (GameObject gameObject = tiles[i, j]; gameObject != null; gameObject = gameObject.TNext)
					{
						if (gameObject is Static @static)
						{
							@static.UpdateGraphicBySeason();
						}
					}
				}
			}
		}
	}

	public static void Custompacket_SetAfk(StackDataReader p)
	{
		AwayStateHelper.HandlePacket((byte)p.ReadInt8(), (uint)((int)p.Length + 1));
	}

	public static void CustomPacket_NoDrawTiles(StackDataReader p)
	{
		TileDataLoader instance = TileDataLoader.Instance;
		byte b = p.ReadUInt8();
		if (b == byte.MaxValue)
		{
			instance.ShowNoDrawTiles = !instance.ShowNoDrawTiles;
		}
		else
		{
			instance.ShowNoDrawTiles = b != 0;
		}
		InvalidateAllowedToDraw();
	}

	public static void CustomPacket_SetChatColors(StackDataReader p)
	{
		ushort num = (ushort)(p.ReadUInt16BE() + 1);
		ushort emoteHue = (ushort)(p.ReadUInt16BE() + 1);
		if (ProfileManager.CurrentProfile != null)
		{
			Profile currentProfile = ProfileManager.CurrentProfile;
			ushort num3 = (currentProfile.YellHue = num);
			ushort speechHue = (currentProfile.WhisperHue = num3);
			currentProfile.SpeechHue = speechHue;
			currentProfile.EmoteHue = emoteHue;
		}
	}

	public void AnalyzePacket(byte[] data, int offset, int length)
	{
		OnPacketBufferReader onPacketBufferReader = _handlers[data[0]];
		if (onPacketBufferReader != null)
		{
			StackDataReader p = new StackDataReader(data.AsSpan(0, length));
			p.Seek(offset);
			onPacketBufferReader(ref p);
		}
	}

	public static void Load()
	{
		Handlers.Add(27, EnterWorld);
		Handlers.Add(85, LoginComplete);
		Handlers.Add(12, Custompacket);
		Handlers.Add(189, ClientVersion);
		Handlers.Add(3, ClientTalk);
		Handlers.Add(11, Damage);
		Handlers.Add(17, CharacterStatus);
		Handlers.Add(21, FollowR);
		Handlers.Add(22, NewHealthbarUpdate);
		Handlers.Add(23, NewHealthbarUpdate);
		Handlers.Add(26, UpdateItem);
		Handlers.Add(28, Talk);
		Handlers.Add(29, DeleteObject);
		Handlers.Add(32, UpdatePlayer);
		Handlers.Add(33, DenyWalk);
		Handlers.Add(34, ConfirmWalk);
		Handlers.Add(35, DragAnimation);
		Handlers.Add(36, OpenContainer);
		Handlers.Add(37, UpdateContainedItem);
		Handlers.Add(39, DenyMoveItem);
		Handlers.Add(40, EndDraggingItem);
		Handlers.Add(41, DropItemAccepted);
		Handlers.Add(44, DeathScreen);
		Handlers.Add(45, MobileAttributes);
		Handlers.Add(46, EquipItem);
		Handlers.Add(47, Swing);
		Handlers.Add(50, Unknown_0x32);
		Handlers.Add(56, Pathfinding);
		Handlers.Add(58, UpdateSkills);
		Handlers.Add(60, UpdateContainedItems);
		Handlers.Add(78, PersonalLightLevel);
		Handlers.Add(79, LightLevel);
		Handlers.Add(84, PlaySoundEffect);
		Handlers.Add(86, MapData);
		Handlers.Add(91, SetTime);
		Handlers.Add(101, SetWeather);
		Handlers.Add(102, BookData);
		Handlers.Add(108, TargetCursor);
		Handlers.Add(109, PlayMusic);
		Handlers.Add(111, SecureTrading);
		Handlers.Add(110, CharacterAnimation);
		Handlers.Add(112, GraphicEffect);
		Handlers.Add(113, BulletinBoardData);
		Handlers.Add(114, Warmode);
		Handlers.Add(115, Ping);
		Handlers.Add(116, BuyList);
		Handlers.Add(119, UpdateCharacter);
		Handlers.Add(120, UpdateObject);
		Handlers.Add(124, OpenMenu);
		Handlers.Add(136, OpenPaperdoll);
		Handlers.Add(137, CorpseEquipment);
		Handlers.Add(144, DisplayMap);
		Handlers.Add(147, OpenBook);
		Handlers.Add(149, DyeData);
		Handlers.Add(151, MovePlayer);
		Handlers.Add(152, UpdateName);
		Handlers.Add(153, MultiPlacement);
		Handlers.Add(154, ASCIIPrompt);
		Handlers.Add(158, SellList);
		Handlers.Add(161, UpdateHitpoints);
		Handlers.Add(162, UpdateMana);
		Handlers.Add(163, UpdateStamina);
		Handlers.Add(165, OpenUrl);
		Handlers.Add(166, TipWindow);
		Handlers.Add(170, AttackCharacter);
		Handlers.Add(171, TextEntryDialog);
		Handlers.Add(175, DisplayDeath);
		Handlers.Add(174, UnicodeTalk);
		Handlers.Add(176, OpenGump);
		Handlers.Add(183, Help);
		Handlers.Add(184, CharacterProfile);
		Handlers.Add(185, EnableLockedFeatures);
		Handlers.Add(186, DisplayQuestArrow);
		Handlers.Add(187, UltimaMessengerR);
		Handlers.Add(188, Season);
		Handlers.Add(190, AssistVersion);
		Handlers.Add(191, ExtendedCommand);
		Handlers.Add(192, GraphicEffect);
		Handlers.Add(193, DisplayClilocString);
		Handlers.Add(194, UnicodePrompt);
		Handlers.Add(196, Semivisible);
		Handlers.Add(198, InvalidMapEnable);
		Handlers.Add(199, GraphicEffect);
		Handlers.Add(200, ClientViewRange);
		Handlers.Add(202, GetUserServerPingGodClientR);
		Handlers.Add(203, GlobalQueCount);
		Handlers.Add(204, DisplayClilocString);
		Handlers.Add(208, ConfigurationFileR);
		Handlers.Add(209, Logout);
		Handlers.Add(210, UpdateCharacter);
		Handlers.Add(211, UpdateObject);
		Handlers.Add(212, OpenBook);
		Handlers.Add(214, MegaCliloc);
		Handlers.Add(215, GenericAOSCommandsR);
		Handlers.Add(216, CustomHouse);
		Handlers.Add(219, CharacterTransferLog);
		Handlers.Add(220, OPLInfo);
		Handlers.Add(221, OpenCompressedGump);
		Handlers.Add(222, UpdateMobileStatus);
		Handlers.Add(226, NewCharacterAnimation);
		Handlers.Add(227, KREncryptionResponse);
		Handlers.Add(229, DisplayWaypoint);
		Handlers.Add(230, RemoveWaypoint);
		Handlers.Add(240, KrriosClientSpecial);
		Handlers.Add(241, FreeshardListR);
		Handlers.Add(243, UpdateItemSA);
		Handlers.Add(245, DisplayMap);
		Handlers.Add(246, BoatMoving);
		Handlers.Add(247, PacketList);
		Handlers.Add(168, ServerListReceived);
		Handlers.Add(140, ReceiveServerRelay);
		Handlers.Add(134, UpdateCharacterList);
		Handlers.Add(169, ReceiveCharacterList);
		Handlers.Add(130, ReceiveLoginRejection);
		Handlers.Add(133, ReceiveLoginRejection);
		Handlers.Add(83, ReceiveLoginRejection);
	}

	public static void SendMegaClilocRequests()
	{
		if (!World.ClientFeatures.TooltipsEnabled || Handlers._clilocRequests.Count == 0)
		{
			return;
		}
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_5090)
		{
			if (Handlers._clilocRequests.Count != 0)
			{
				NetClient.Socket.Send_MegaClilocRequest(ref Handlers._clilocRequests);
			}
			return;
		}
		foreach (uint clilocRequest in Handlers._clilocRequests)
		{
			NetClient.Socket.Send_MegaClilocRequest_Old(clilocRequest);
		}
		Handlers._clilocRequests.Clear();
	}

	public static void AddMegaClilocRequest(uint serial)
	{
		foreach (uint clilocRequest in Handlers._clilocRequests)
		{
			if (clilocRequest == serial)
			{
				return;
			}
		}
		Handlers._clilocRequests.Add(serial);
	}

	private static void SetResistances(StackDataReader p)
	{
		for (int i = 0; i < 5; i++)
		{
			short num = (short)p.ReadUInt16BE();
			switch (i)
			{
			case 0:
				World.Player.KrankheitsResistance = num;
				break;
			case 1:
				World.Player.KlingenResistance = num;
				break;
			case 2:
				World.Player.SpitzResistance = num;
				break;
			case 3:
				World.Player.StumpfResistance = num;
				break;
			case 4:
				World.Player.RPPBonusPunkte = num;
				break;
			}
		}
		World.Player.PhysResiOverall = (short)((World.Player.KlingenResistance + World.Player.SpitzResistance + World.Player.StumpfResistance) / 3);
	}

	private static void ApplySpeedLimit(StackDataReader p)
	{
		ushort num = p.ReadUInt16BE();
		byte key = p.ReadUInt8();
		if (!(World.Player != null))
		{
			return;
		}
		if (num == 0)
		{
			if (World.Player.CastRunBlockUntil.ContainsKey(key))
			{
				World.Player.CastRunBlockUntil.Remove(key);
			}
			return;
		}
		DateTime dateTime = DateTime.Now.AddMilliseconds((int)num);
		if (World.Player.CastRunBlockUntil.TryGetValue(key, out var value))
		{
			if (value < dateTime)
			{
				World.Player.CastRunBlockUntil[key] = dateTime;
			}
		}
		else
		{
			World.Player.CastRunBlockUntil.Add(key, dateTime);
		}
	}

	private static void SetClipboard(StackDataReader p)
	{
		SDL.SDL_SetClipboardText(p.ReadASCII());
	}

	private static void AddShovableAnims(StackDataReader p)
	{
		if (!(Client.Game.Scene is GameScene))
		{
			return;
		}
		List<ushort> shovableAnims = Pathfinder.ShovableAnims;
		ushort num = (ushort)(p.Length - 4);
		for (int i = 0; i < num / 2; i++)
		{
			ushort item = p.ReadUInt16BE();
			if (!shovableAnims.Contains(item))
			{
				shovableAnims.Add(item);
			}
		}
	}

	private static void TargetCursor(ref StackDataReader p)
	{
		TargetManager.SetTargeting((CursorTarget)p.ReadUInt8(), p.ReadUInt32BE(), (TargetType)p.ReadUInt8());
		if (World.Party.PartyHealTimer < Time.Ticks && World.Party.PartyHealTarget != 0)
		{
			TargetManager.Target(World.Party.PartyHealTarget);
			World.Party.PartyHealTimer = 0L;
			World.Party.PartyHealTarget = 0u;
		}
	}

	private static void SecureTrading(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		byte b = p.ReadUInt8();
		uint num = p.ReadUInt32BE();
		switch (b)
		{
		case 0:
		{
			uint num4 = p.ReadUInt32BE();
			uint num5 = p.ReadUInt32BE();
			if (!(World.Get(num4) == null) && !(World.Get(num5) == null))
			{
				bool num6 = p.ReadBool();
				string name = string.Empty;
				if (num6 && p.Position < p.Length)
				{
					name = p.ReadASCII();
				}
				UIManager.Add(new TradingGump(num, name, num4, num5));
			}
			break;
		}
		case 1:
			UIManager.GetTradingGump(num)?.Dispose();
			break;
		case 2:
		{
			uint num2 = p.ReadUInt32BE();
			uint num3 = p.ReadUInt32BE();
			TradingGump tradingGump2 = UIManager.GetTradingGump(num);
			if (tradingGump2 != null)
			{
				tradingGump2.ImAccepting = num2 != 0;
				tradingGump2.HeIsAccepting = num3 != 0;
				tradingGump2.RequestUpdateContents();
			}
			break;
		}
		case 3:
		case 4:
		{
			TradingGump tradingGump = UIManager.GetTradingGump(num);
			if (tradingGump != null)
			{
				if (b == 4)
				{
					tradingGump.Gold = p.ReadUInt32BE();
					tradingGump.Platinum = p.ReadUInt32BE();
				}
				else
				{
					tradingGump.HisGold = p.ReadUInt32BE();
					tradingGump.HisPlatinum = p.ReadUInt32BE();
				}
			}
			break;
		}
		}
	}

	private static void ClientTalk(ref StackDataReader p)
	{
		byte b = p.ReadUInt8();
		if ((uint)b <= 46u)
		{
			if (b != 37)
			{
				_ = 46;
			}
		}
		else if (b != 60)
		{
			_ = 120;
		}
	}

	private static void Damage(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		Entity entity = World.Get(p.ReadUInt32BE());
		if (entity != null)
		{
			ushort num = p.ReadUInt16BE();
			if (num > 0)
			{
				World.WorldTextManager.AddDamage(entity, num);
			}
		}
	}

	private static void CharacterStatus(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint serial = p.ReadUInt32BE();
		Entity entity = World.Get(serial);
		if (entity == null)
		{
			return;
		}
		string name = entity.Name;
		entity.Name = p.ReadASCII(30);
		entity.Hits = p.ReadUInt16BE();
		entity.HitsMax = p.ReadUInt16BE();
		if (entity.HitsRequest == HitsRequestStatus.Pending)
		{
			entity.HitsRequest = HitsRequestStatus.Received;
		}
		if (!SerialHelper.IsMobile(serial))
		{
			return;
		}
		Mobile mobile = entity as Mobile;
		if (mobile == null)
		{
			return;
		}
		mobile.IsRenamable = p.ReadBool();
		byte b = p.ReadUInt8();
		if (b > 0 && p.Position + 1 <= p.Length)
		{
			ushort graphicForAnimation = mobile.GetGraphicForAnimation();
			if (graphicForAnimation == 400 || graphicForAnimation == 2566 || graphicForAnimation == 2567)
			{
				mobile.IsFemale = false;
			}
			else
			{
				mobile.IsFemale = true;
			}
			if (mobile == World.Player)
			{
				if (!string.IsNullOrEmpty(World.Player.Name) && name != World.Player.Name)
				{
					Client.Game.SetWindowTitle(World.Player.Name);
				}
				p.Skip(1);
				ushort num = p.ReadUInt16BE();
				ushort num2 = p.ReadUInt16BE();
				ushort num3 = p.ReadUInt16BE();
				World.Player.Stamina = p.ReadUInt16BE();
				World.Player.StaminaMax = p.ReadUInt16BE();
				World.Player.Mana = p.ReadUInt16BE();
				World.Player.ManaMax = p.ReadUInt16BE();
				World.Player.Gold = p.ReadUInt32BE();
				World.Player.PhysicalResistance = (short)p.ReadUInt16BE();
				World.Player.Weight = p.ReadUInt16BE();
				if (World.Player.Strength != 0 && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowStatsChangedMessage)
				{
					ushort strength = World.Player.Strength;
					ushort dexterity = World.Player.Dexterity;
					ushort intelligence = World.Player.Intelligence;
					int num4 = num - strength;
					int num5 = num2 - dexterity;
					int num6 = num3 - intelligence;
					string text = "";
					string text2 = "";
					string text3 = "";
					if (num4 > 0)
					{
						text = "+";
					}
					if (num5 > 0)
					{
						text2 = "+";
					}
					if (num6 > 0)
					{
						text3 = "+";
					}
					if (num4 != 0)
					{
						GameActions.Print(string.Format(ResGeneral.Your0HasChangedBy1ItIsNow2, ResGeneral.Strength, text + num4, num), 368, MessageType.System, 3, unicode: false);
					}
					if (num5 != 0)
					{
						GameActions.Print(string.Format(ResGeneral.Your0HasChangedBy1ItIsNow2, ResGeneral.Dexterity, text2 + num5, num2), 368, MessageType.System, 3, unicode: false);
					}
					if (num6 != 0)
					{
						GameActions.Print(string.Format(ResGeneral.Your0HasChangedBy1ItIsNow2, ResGeneral.Intelligence, text3 + num6, num3), 368, MessageType.System, 3, unicode: false);
					}
				}
				World.Player.Strength = num;
				World.Player.Dexterity = num2;
				World.Player.Intelligence = num3;
				if (b >= 5)
				{
					World.Player.WeightMax = p.ReadUInt16BE();
					byte b2 = p.ReadUInt8();
					if (b2 == 0)
					{
						b2 = 1;
					}
					World.Player.Race = (RaceType)b2;
				}
				else if (Client.Version >= ClassicUO.Data.ClientVersion.CV_500A)
				{
					World.Player.WeightMax = (ushort)(7 * (World.Player.Strength >> 1) + 40);
				}
				else
				{
					World.Player.WeightMax = (ushort)(World.Player.Strength * 4 + 25);
				}
				if (b >= 3)
				{
					World.Player.StatsCap = (short)p.ReadUInt16BE();
					World.Player.Followers = p.ReadUInt8();
					World.Player.FollowersMax = p.ReadUInt8();
				}
				if (b >= 4)
				{
					World.Player.FireResistance = (short)p.ReadUInt16BE();
					World.Player.ColdResistance = (short)p.ReadUInt16BE();
					World.Player.PoisonResistance = (short)p.ReadUInt16BE();
					World.Player.EnergyResistance = (short)p.ReadUInt16BE();
					World.Player.Luck = p.ReadUInt16BE();
					World.Player.DamageMin = (short)p.ReadUInt16BE();
					World.Player.DamageMax = (short)p.ReadUInt16BE();
					World.Player.TithingPoints = p.ReadUInt32BE();
				}
				if (b >= 6)
				{
					World.Player.MaxPhysicResistence = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.MaxFireResistence = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.MaxColdResistence = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.MaxPoisonResistence = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.MaxEnergyResistence = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.DefenseChanceIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.MaxDefenseChanceIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.HitChanceIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.SwingSpeedIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.DamageIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.LowerReagentCost = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.SpellDamageIncrease = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.FasterCastRecovery = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.FasterCasting = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
					World.Player.LowerManaCost = (short)((p.Position + 2 <= p.Length) ? ((short)p.ReadUInt16BE()) : 0);
				}
			}
		}
		if (mobile == World.Player)
		{
			UoAssist.SignalHits();
			UoAssist.SignalStamina();
			UoAssist.SignalMana();
		}
	}

	private static void FollowR(ref StackDataReader p)
	{
		p.ReadUInt32BE();
		p.ReadUInt32BE();
	}

	private static void NewHealthbarUpdate(ref StackDataReader p)
	{
		if (World.Player == null || (p[0] == 22 && Client.Version < ClassicUO.Data.ClientVersion.CV_500A))
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (mobile == null)
		{
			return;
		}
		ushort num = p.ReadUInt16BE();
		for (int i = 0; i < num; i++)
		{
			ushort num2 = p.ReadUInt16BE();
			bool flag = p.ReadBool();
			byte b = (byte)mobile.Flags;
			switch (num2)
			{
			case 1:
				if (flag)
				{
					if (Client.Version >= ClassicUO.Data.ClientVersion.CV_7000)
					{
						mobile.SetSAPoison(value: true);
					}
					else
					{
						b |= 4;
					}
				}
				else if (Client.Version >= ClassicUO.Data.ClientVersion.CV_7000)
				{
					mobile.SetSAPoison(value: false);
				}
				else
				{
					b = (byte)(b & -5);
				}
				break;
			case 2:
				b = ((!flag) ? ((byte)(b & (byte)(b & -9))) : ((byte)(b | 8)));
				break;
			default:
				_ = 3;
				break;
			}
			mobile.Flags = (Flags)b;
		}
	}

	private static void UpdateItem(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			uint num = p.ReadUInt32BE();
			ushort num2 = 0;
			byte graphic_inc = 0;
			byte b = 0;
			ushort num3 = 0;
			byte b2 = 0;
			byte type = 0;
			if ((num & 0x80000000u) != 0)
			{
				num &= 0x7FFFFFFF;
				num2 = 1;
			}
			ushort num4 = p.ReadUInt16BE();
			if ((num4 & 0x8000) != 0)
			{
				num4 &= 0x7FFF;
				graphic_inc = p.ReadUInt8();
			}
			num2 = ((num2 <= 0) ? ((ushort)(num2 + 1)) : p.ReadUInt16BE());
			ushort num5 = p.ReadUInt16BE();
			if ((num5 & 0x8000) != 0)
			{
				num5 &= 0x7FFF;
				b = 1;
			}
			ushort num6 = p.ReadUInt16BE();
			if ((num6 & 0x8000) != 0)
			{
				num6 &= 0x7FFF;
				num3 = 1;
			}
			if ((num6 & 0x4000) != 0)
			{
				num6 &= 0x3FFF;
				b2 = 1;
			}
			if (b != 0)
			{
				b = p.ReadUInt8();
			}
			sbyte z = p.ReadInt8();
			if (num3 != 0)
			{
				num3 = p.ReadUInt16BE();
			}
			if (b2 != 0)
			{
				b2 = p.ReadUInt8();
			}
			if (num4 >= 16384)
			{
				type = 2;
			}
			UpdateGameObject(num, num4, graphic_inc, num2, num5, num6, z, (Direction)b, num3, (Flags)b2, num2, type, 1);
		}
	}

	private static void EnterWorld(ref StackDataReader p)
	{
		if (ProfileManager.CurrentProfile == null)
		{
			string lastCharacter = LastCharacterManager.GetLastCharacter(LoginScene.Account, World.ServerName);
			ProfileManager.Load(World.ServerName, LoginScene.Account, lastCharacter);
		}
		if (World.Player != null)
		{
			World.Clear();
		}
		World.Mobiles.Add(World.Player = new PlayerMobile(p.ReadUInt32BE()));
		p.Skip(4);
		World.Player.Graphic = p.ReadUInt16BE();
		World.Player.CheckGraphicChange(0);
		ushort x = p.ReadUInt16BE();
		ushort y = p.ReadUInt16BE();
		sbyte z = (sbyte)p.ReadUInt16BE();
		if (World.Map == null)
		{
			World.MapIndex = 0;
		}
		Direction direction = (Direction)(p.ReadUInt8() & 7);
		World.Player.X = x;
		World.Player.Y = y;
		World.Player.Z = z;
		World.Player.UpdateScreenPosition();
		World.Player.Direction = direction;
		World.Player.AddToTile();
		World.RangeSize.X = x;
		World.RangeSize.Y = y;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.UseCustomLightLevel)
		{
			World.Light.Overall = ProfileManager.CurrentProfile.LightLevel;
		}
		Client.Game.Scene.Audio.UpdateCurrentMusicVolume();
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_200)
		{
			if (ProfileManager.CurrentProfile != null)
			{
				NetClient.Socket.Send_GameWindowSize((uint)ProfileManager.CurrentProfile.GameWindowSize.X, (uint)ProfileManager.CurrentProfile.GameWindowSize.Y);
			}
			NetClient.Socket.Send_Language(Settings.GlobalSettings.Language);
		}
		NetClient.Socket.Send_ClientVersion(Settings.GlobalSettings.ClientVersion);
		GameActions.SingleClick(World.Player);
		NetClient.Socket.Send_SkillsRequest(World.Player.Serial);
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_70796 && ProfileManager.CurrentProfile != null)
		{
			NetClient.Socket.Send_ShowPublicHouseContent(ProfileManager.CurrentProfile.ShowHouseContent);
		}
		NetClient.Socket.Send_ToPlugins_AllSkills();
		NetClient.Socket.Send_ToPlugins_AllSpells();
	}

	private static void Talk(ref StackDataReader p)
	{
		uint num = p.ReadUInt32BE();
		Entity entity = World.Get(num);
		ushort num2 = p.ReadUInt16BE();
		MessageType messageType = (MessageType)p.ReadUInt8();
		ushort num3 = p.ReadUInt16BE();
		ushort num4 = p.ReadUInt16BE();
		string text = p.ReadASCII(30);
		string text2;
		if (p.Length > 44)
		{
			p.Seek(44L);
			text2 = p.ReadASCII();
		}
		else
		{
			text2 = string.Empty;
		}
		if (num == 0 && num2 == 0 && messageType == MessageType.Regular && num4 == ushort.MaxValue && num3 == ushort.MaxValue && text.StartsWith("SYSTEM"))
		{
			NetClient.Socket.Send_ACKTalk();
			return;
		}
		TextType textType = TextType.SYSTEM;
		if (messageType != MessageType.System && num != uint.MaxValue && num != 0 && (!(text.ToLower() == "system") || !(entity == null)) && entity != null)
		{
			textType = TextType.OBJECT;
			if (string.IsNullOrEmpty(entity.Name))
			{
				entity.Name = text;
			}
		}
		MessageManager.HandleMessage(entity, text2, text, num3, messageType, (byte)num4, textType);
	}

	private static void DeleteObject(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		if ((uint)World.Player == num)
		{
			return;
		}
		Entity entity = World.Get(num);
		if (entity == null)
		{
			return;
		}
		bool flag = false;
		if (entity is Item item)
		{
			uint num2 = item.Container & 0x7FFFFFFF;
			if (SerialHelper.IsValid(item.Container))
			{
				Entity entity2 = World.Get(item.RootContainer);
				if (entity2 != null && entity2 == World.Player)
				{
					flag = item.Layer == Layer.OneHanded || item.Layer == Layer.TwoHanded;
					Item secureTradeBox = World.Player.GetSecureTradeBox();
					if (secureTradeBox != null)
					{
						UIManager.GetTradingGump(secureTradeBox)?.RequestUpdateContents();
					}
				}
				if (num2 == (uint)World.Player && item.Layer == Layer.Invalid)
				{
					ItemHold.Enabled = false;
				}
				if (item.Layer != 0)
				{
					UIManager.GetGump<PaperDollGump>(num2)?.RequestUpdateContents();
				}
				UIManager.GetGump<ContainerGump>(num2)?.RequestUpdateContents();
				if (entity2 != null && entity2.Graphic == 8198 && (ProfileManager.CurrentProfile.GridLootType == 1 || ProfileManager.CurrentProfile.GridLootType == 2))
				{
					UIManager.GetGump<GridLootGump>(num2)?.RequestUpdateContents();
				}
				if (item.Graphic == 3760)
				{
					UIManager.GetGump<BulletinBoardItem>(num)?.Dispose();
					UIManager.GetGump<BulletinBoardGump>(null)?.RemoveBulletinObject(num);
				}
			}
		}
		if (World.CorpseManager.Exists(0u, num))
		{
			return;
		}
		if (entity is Mobile)
		{
			World.Party.Contains(num);
			World.RemoveMobile(num, forceRemove: true);
			return;
		}
		Item item2 = (Item)entity;
		if (item2.IsMulti)
		{
			World.HouseManager.Remove(num);
		}
		Entity entity3 = World.Get(item2.Container);
		if (entity3 != null)
		{
			entity3.Remove(item2);
			if (item2.Layer != 0)
			{
				UIManager.GetGump<PaperDollGump>(entity3)?.RequestUpdateContents();
			}
		}
		else if (item2.IsMulti)
		{
			UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
		}
		World.RemoveItem(num, forceRemove: true);
		if (flag)
		{
			World.Player.UpdateAbilities();
		}
	}

	private static void UpdatePlayer(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			uint serial = p.ReadUInt32BE();
			ushort graphic = p.ReadUInt16BE();
			byte graph_inc = p.ReadUInt8();
			ushort hue = p.ReadUInt16BE();
			Flags flags = (Flags)p.ReadUInt8();
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			ushort serverID = p.ReadUInt16BE();
			Direction direction = (Direction)p.ReadUInt8();
			sbyte z = p.ReadInt8();
			UpdatePlayer(serial, graphic, graph_inc, hue, flags, x, y, z, serverID, direction);
		}
	}

	private static void DenyWalk(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			byte sequence = p.ReadUInt8();
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			Direction direction = (Direction)p.ReadUInt8();
			direction &= Direction.Up;
			sbyte z = p.ReadInt8();
			World.Player.Walker.DenyWalk(sequence, x, y, z);
			World.Player.Direction = direction;
			Client.Game.GetScene<GameScene>()?.Weather?.Reset();
		}
	}

	private static void ConfirmWalk(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			byte sequence = p.ReadUInt8();
			NotorietyFlag notorietyFlag = (NotorietyFlag)p.ReadUInt8();
			if (notorietyFlag == NotorietyFlag.Unknown)
			{
				notorietyFlag = NotorietyFlag.Innocent;
			}
			World.Player.NotorietyFlag = notorietyFlag;
			World.Player.Walker.ConfirmWalk(sequence);
			World.Player.AddToTile();
		}
	}

	private static void DragAnimation(ref StackDataReader p)
	{
		ushort num = p.ReadUInt16BE();
		num += p.ReadUInt8();
		ushort hue = p.ReadUInt16BE();
		p.ReadUInt16BE();
		uint num2 = p.ReadUInt32BE();
		ushort srcX = p.ReadUInt16BE();
		ushort srcY = p.ReadUInt16BE();
		sbyte srcZ = p.ReadInt8();
		uint num3 = p.ReadUInt32BE();
		ushort targetX = p.ReadUInt16BE();
		ushort targetY = p.ReadUInt16BE();
		sbyte targetZ = p.ReadInt8();
		switch (num)
		{
		case 3821:
			num = 3823;
			break;
		case 3818:
			num = 3820;
			break;
		case 3824:
			num = 3826;
			break;
		}
		Mobile mobile = World.Mobiles.Get(num2);
		if (mobile == null)
		{
			num2 = 0u;
		}
		else
		{
			srcX = mobile.X;
			srcY = mobile.Y;
			srcZ = mobile.Z;
		}
		Mobile mobile2 = World.Mobiles.Get(num3);
		if (mobile2 == null)
		{
			num3 = 0u;
		}
		else
		{
			targetX = mobile2.X;
			targetY = mobile2.Y;
			targetZ = mobile2.Z;
		}
		World.SpawnEffect((SerialHelper.IsValid(num2) && SerialHelper.IsValid(num3)) ? GraphicEffectType.DragEffect : GraphicEffectType.Moving, num2, num3, num, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, 5, 5000, fixedDir: true, doesExplode: false, hasparticles: false, GraphicEffectBlendMode.Normal);
	}

	private static void OpenContainer(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		ushort num2 = p.ReadUInt16BE();
		switch (num2)
		{
		case ushort.MaxValue:
		{
			Item item2 = World.Items.Get(num);
			if (item2 == null)
			{
				return;
			}
			UIManager.GetGump<SpellbookGump>(num)?.Dispose();
			SpellbookGump spellbookGump = new SpellbookGump(item2);
			if (!UIManager.GetGumpCachePosition(item2, out var pos))
			{
				pos = new Point(64, 64);
			}
			spellbookGump.Location = pos;
			UIManager.Add(spellbookGump);
			Client.Game.Scene.Audio.PlaySound(85);
			break;
		}
		case 48:
		{
			Mobile mobile = World.Mobiles.Get(num);
			if (mobile == null)
			{
				return;
			}
			UIManager.GetGump<ShopGump>(num)?.Dispose();
			ShopGump shopGump = new ShopGump(num, isBuyGump: true, 150, 5);
			UIManager.Add(shopGump);
			Layer layer = Layer.ShopBuyRestock;
			while ((int)layer < 28)
			{
				Item item3 = mobile.FindItemByLayer(layer);
				LinkedObject linkedObject = item3.Items;
				if (linkedObject != null)
				{
					bool flag = item3.Graphic != 11000;
					if (flag)
					{
						while (linkedObject?.Next != null)
						{
							linkedObject = linkedObject.Next;
						}
					}
					while (linkedObject != null)
					{
						Item item4 = (Item)linkedObject;
						shopGump.AddItem(item4.Serial, item4.Graphic, item4.Hue, item4.Amount, item4.Price, item4.Name, fromcliloc: false);
						linkedObject = ((!flag) ? linkedObject.Next : linkedObject.Previous);
					}
				}
				layer++;
			}
			break;
		}
		default:
		{
			Item item = World.Items.Get(num);
			if (item != null)
			{
				if (item.IsCorpse && (ProfileManager.CurrentProfile.GridLootType == 1 || ProfileManager.CurrentProfile.GridLootType == 2))
				{
					_requestedGridLoot = num;
					if (ProfileManager.CurrentProfile.GridLootType == 1)
					{
						return;
					}
				}
				ContainerGump gump = UIManager.GetGump<ContainerGump>(num);
				bool playsound = false;
				if (Client.Version >= ClassicUO.Data.ClientVersion.CV_706000 && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.UseLargeContainerGumps)
				{
					GumpsLoader instance = GumpsLoader.Instance;
					Rectangle bounds;
					switch (num2)
					{
					case 72:
						if (instance.GetGumpTexture(1768u, out bounds) != null)
						{
							num2 = 1768;
						}
						break;
					case 73:
						if (instance.GetGumpTexture(40159u, out bounds) != null)
						{
							num2 = 40159;
						}
						break;
					case 81:
						if (instance.GetGumpTexture(1767u, out bounds) != null)
						{
							num2 = 1767;
						}
						break;
					case 62:
						if (instance.GetGumpTexture(1769u, out bounds) != null)
						{
							num2 = 1769;
						}
						break;
					case 77:
						if (instance.GetGumpTexture(1770u, out bounds) != null)
						{
							num2 = 1770;
						}
						break;
					case 78:
						if (instance.GetGumpTexture(1766u, out bounds) != null)
						{
							num2 = 1766;
						}
						break;
					case 79:
						if (instance.GetGumpTexture(1765u, out bounds) != null)
						{
							num2 = 1765;
						}
						break;
					case 74:
						if (instance.GetGumpTexture(40157u, out bounds) != null)
						{
							num2 = 40157;
						}
						break;
					case 68:
						if (instance.GetGumpTexture(40163u, out bounds) != null)
						{
							num2 = 40163;
						}
						break;
					}
				}
				int num3;
				int num4;
				if (gump != null)
				{
					num3 = gump.ScreenCoordinateX;
					num4 = gump.ScreenCoordinateY;
					gump.Dispose();
				}
				else
				{
					ContainerManager.CalculateContainerPosition(num, num2);
					num3 = ContainerManager.X;
					num4 = ContainerManager.Y;
					playsound = true;
				}
				ContainerGump containerGump = new ContainerGump(item, num2, playsound);
				containerGump.X = num3;
				containerGump.Y = num4;
				containerGump.InvalidateContents = true;
				UIManager.Add(containerGump);
				UIManager.RemovePosition(num);
			}
			else
			{
				Log.Error("[OpenContainer]: item not found");
			}
			break;
		}
		}
		if (num2 == 48)
		{
			return;
		}
		Item item5 = World.Items.Get(num);
		if (item5 != null)
		{
			item5.Opened = true;
			if (!item5.IsCorpse && num2 != ushort.MaxValue)
			{
				ClearContainerAndRemoveItems(item5);
			}
		}
	}

	private static void UpdateContainedItem(ref StackDataReader p)
	{
		if (World.InGame)
		{
			uint serial = p.ReadUInt32BE();
			ushort graphic = (ushort)(p.ReadUInt16BE() + p.ReadUInt8());
			ushort amount = Math.Max((ushort)1, p.ReadUInt16BE());
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			if (Client.Version >= ClassicUO.Data.ClientVersion.CV_6017)
			{
				p.Skip(1);
			}
			uint containerSerial = p.ReadUInt32BE();
			ushort hue = p.ReadUInt16BE();
			AddItemToContainer(serial, graphic, amount, x, y, hue, containerSerial);
		}
	}

	private static void DenyMoveItem(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		Item item = World.Items.Get(ItemHold.Serial);
		if (ItemHold.Enabled || (ItemHold.Dropped && (item == null || !item.AllowedToDraw)))
		{
			if (World.ObjectToRemove == ItemHold.Serial)
			{
				World.ObjectToRemove = 0u;
			}
			if (SerialHelper.IsValid(ItemHold.Serial) && ItemHold.Graphic != ushort.MaxValue)
			{
				if (!ItemHold.UpdatedInWorld)
				{
					if (ItemHold.Layer == Layer.Invalid && SerialHelper.IsValid(ItemHold.Container))
					{
						Console.WriteLine("=== DENY === ADD TO CONTAINER");
						AddItemToContainer(ItemHold.Serial, ItemHold.Graphic, ItemHold.TotalAmount, ItemHold.X, ItemHold.Y, ItemHold.Hue, ItemHold.Container);
						UIManager.GetGump<ContainerGump>(ItemHold.Container)?.RequestUpdateContents();
					}
					else
					{
						Item orCreateItem = World.GetOrCreateItem(ItemHold.Serial);
						orCreateItem.Graphic = ItemHold.Graphic;
						orCreateItem.Hue = ItemHold.Hue;
						orCreateItem.Amount = ItemHold.TotalAmount;
						orCreateItem.Flags = ItemHold.Flags;
						orCreateItem.Layer = ItemHold.Layer;
						orCreateItem.X = ItemHold.X;
						orCreateItem.Y = ItemHold.Y;
						orCreateItem.Z = ItemHold.Z;
						orCreateItem.CheckGraphicChange(0);
						Entity entity = World.Get(ItemHold.Container);
						if (entity != null)
						{
							if (SerialHelper.IsMobile(entity.Serial))
							{
								Console.WriteLine("=== DENY === ADD TO PAPERDOLL");
								World.RemoveItemFromContainer(orCreateItem);
								entity.PushToBack(orCreateItem);
								orCreateItem.Container = entity.Serial;
								UIManager.GetGump<PaperDollGump>(orCreateItem.Container)?.RequestUpdateContents();
							}
							else
							{
								Console.WriteLine("=== DENY === SOMETHING WRONG");
								World.RemoveItem(orCreateItem, forceRemove: true);
							}
						}
						else
						{
							Console.WriteLine("=== DENY === ADD TO TERRAIN");
							World.RemoveItemFromContainer(orCreateItem);
							orCreateItem.AddToTile();
							orCreateItem.UpdateScreenPosition();
						}
					}
				}
			}
			else
			{
				Log.Error($"Wrong data: serial = {ItemHold.Serial:X8}  -  graphic = {ItemHold.Graphic:X4}");
			}
			UIManager.GetGump<SplitMenuGump>(ItemHold.Serial)?.Dispose();
			ItemHold.Clear();
		}
		else
		{
			Log.Warn("There was a problem with ItemHold object. It was cleared before :|");
		}
		byte b = p.ReadUInt8();
		if (b < 5)
		{
			MessageManager.HandleMessage(null, ServerErrorMessages.GetError(p[0], b), string.Empty, 946, MessageType.System, 3, TextType.SYSTEM);
		}
	}

	private static void EndDraggingItem(ref StackDataReader p)
	{
		if (World.InGame)
		{
			ItemHold.Enabled = false;
			ItemHold.Dropped = false;
		}
	}

	private static void DropItemAccepted(ref StackDataReader p)
	{
		if (World.InGame)
		{
			ItemHold.Enabled = false;
			ItemHold.Dropped = false;
			Console.WriteLine("PACKET - ITEM DROP OK!");
		}
	}

	private static void DeathScreen(ref StackDataReader p)
	{
		if (p.ReadUInt8() != 1)
		{
			Client.Game.GetScene<GameScene>()?.Weather?.Reset();
			Client.Game.Scene.Audio.PlayMusic(Client.Game.Scene.Audio.DeathMusicIndex, iswarmode: true);
			if (ProfileManager.CurrentProfile.EnableDeathScreen)
			{
				World.Player.DeathScreenTimer = Time.Ticks + 1500;
			}
			GameActions.RequestWarMode(war: false);
		}
	}

	private static void MobileAttributes(ref StackDataReader p)
	{
		uint serial = p.ReadUInt32BE();
		Entity entity = World.Get(serial);
		if (entity == null)
		{
			return;
		}
		entity.HitsMax = p.ReadUInt16BE();
		entity.Hits = p.ReadUInt16BE();
		if (entity.HitsRequest == HitsRequestStatus.Pending)
		{
			entity.HitsRequest = HitsRequestStatus.Received;
		}
		if (!SerialHelper.IsMobile(serial))
		{
			return;
		}
		Mobile mobile = entity as Mobile;
		if (!(mobile == null))
		{
			mobile.ManaMax = p.ReadUInt16BE();
			mobile.Mana = p.ReadUInt16BE();
			mobile.StaminaMax = p.ReadUInt16BE();
			mobile.Stamina = p.ReadUInt16BE();
			if (mobile == World.Player)
			{
				UoAssist.SignalHits();
				UoAssist.SignalStamina();
				UoAssist.SignalMana();
			}
		}
	}

	private static void EquipItem(ref StackDataReader p)
	{
		if (World.InGame)
		{
			Item orCreateItem = World.GetOrCreateItem(p.ReadUInt32BE());
			if (orCreateItem.Graphic != 0 && orCreateItem.Layer != Layer.Backpack)
			{
				World.RemoveItemFromContainer(orCreateItem);
			}
			if (SerialHelper.IsValid(orCreateItem.Container))
			{
				UIManager.GetGump<ContainerGump>(orCreateItem.Container)?.RequestUpdateContents();
				UIManager.GetGump<PaperDollGump>(orCreateItem.Container)?.RequestUpdateContents();
			}
			orCreateItem.Graphic = (ushort)(p.ReadUInt16BE() + p.ReadInt8());
			orCreateItem.Layer = (Layer)p.ReadUInt8();
			orCreateItem.Container = p.ReadUInt32BE();
			orCreateItem.FixHue(p.ReadUInt16BE());
			orCreateItem.Amount = 1;
			Entity entity = World.Get(orCreateItem.Container);
			entity?.PushToBack(orCreateItem);
			if (((int)orCreateItem.Layer < 26 || (int)orCreateItem.Layer > 28) && SerialHelper.IsValid(orCreateItem.Container) && (int)orCreateItem.Layer < 25)
			{
				UIManager.GetGump<PaperDollGump>(orCreateItem.Container)?.RequestUpdateContents();
			}
			if (entity == World.Player && (orCreateItem.Layer == Layer.OneHanded || orCreateItem.Layer == Layer.TwoHanded))
			{
				World.Player?.UpdateAbilities();
			}
		}
	}

	private static void Swing(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		p.Skip(1);
		if (p.ReadUInt32BE() != (uint)World.Player)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		if (TargetManager.LastAttack != num || !World.Player.InWarMode || World.Player.Walker.LastStepRequestTime + 2000 >= Time.Ticks || World.Player.Steps.Count != 0)
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(num);
		if (mobile != null)
		{
			Direction direction = DirectionHelper.GetDirectionAB(World.Player.X, World.Player.Y, mobile.X, mobile.Y);
			int x = World.Player.X;
			int y = World.Player.Y;
			sbyte z = World.Player.Z;
			if (Pathfinder.CanWalk(ref direction, ref x, ref y, ref z) && World.Player.Direction != direction)
			{
				World.Player.Walk(direction, run: false);
			}
		}
	}

	private static void Unknown_0x32(ref StackDataReader p)
	{
	}

	private static void UpdateSkills(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		byte b2 = p.ReadUInt8();
		bool flag = (b2 != 0 && b2 <= 3) || b2 == 223;
		bool flag2 = b2 == byte.MaxValue || b2 == 223;
		if (b2 == 254)
		{
			int num = p.ReadUInt16BE();
			SkillsLoader.Instance.Skills.Clear();
			SkillsLoader.Instance.SortedSkills.Clear();
			for (int i = 0; i < num; i++)
			{
				bool hasAction = p.ReadBool();
				int length = p.ReadUInt8();
				SkillsLoader.Instance.Skills.Add(new SkillEntry(i, p.ReadASCII(length), hasAction));
			}
			SkillsLoader.Instance.SortedSkills.AddRange(SkillsLoader.Instance.Skills);
			SkillsLoader.Instance.SortedSkills.Sort((SkillEntry a, SkillEntry b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCulture));
			return;
		}
		StandardSkillsGump standardSkillsGump = null;
		SkillGumpAdvanced skillGumpAdvanced = null;
		if (ProfileManager.CurrentProfile.StandardSkillsGump)
		{
			standardSkillsGump = UIManager.GetGump<StandardSkillsGump>(null);
		}
		else
		{
			skillGumpAdvanced = UIManager.GetGump<SkillGumpAdvanced>(null);
		}
		if (!flag2 && (b2 == 1 || b2 == 3 || World.SkillsRequested))
		{
			World.SkillsRequested = false;
			if (ProfileManager.CurrentProfile.StandardSkillsGump)
			{
				if (standardSkillsGump == null)
				{
					StandardSkillsGump standardSkillsGump2 = new StandardSkillsGump();
					standardSkillsGump2.X = 100;
					standardSkillsGump2.Y = 100;
					standardSkillsGump = standardSkillsGump2;
					UIManager.Add(standardSkillsGump2);
				}
			}
			else if (skillGumpAdvanced == null)
			{
				SkillGumpAdvanced skillGumpAdvanced2 = new SkillGumpAdvanced();
				skillGumpAdvanced2.X = 100;
				skillGumpAdvanced2.Y = 100;
				skillGumpAdvanced = skillGumpAdvanced2;
				UIManager.Add(skillGumpAdvanced2);
			}
		}
		while (p.Position < p.Length)
		{
			ushort num2 = p.ReadUInt16BE();
			if (p.Position >= p.Length || (num2 == 0 && b2 == 0))
			{
				break;
			}
			if (b2 == 0 || b2 == 2)
			{
				num2--;
			}
			ushort num3 = p.ReadUInt16BE();
			ushort baseFixed = p.ReadUInt16BE();
			Lock @lock = (Lock)p.ReadUInt8();
			ushort capFixed = 1000;
			if (flag)
			{
				capFixed = p.ReadUInt16BE();
			}
			if (num2 < World.Player.Skills.Length)
			{
				Skill skill = World.Player.Skills[num2];
				if (skill != null)
				{
					if (flag2)
					{
						float num4 = (float)(int)num3 / 10f - skill.Value;
						if (num4 != 0f && !float.IsNaN(num4) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ShowSkillsChangedMessage && Math.Ceiling(Math.Abs(num4)) >= (double)ProfileManager.CurrentProfile.ShowSkillsChangedDeltaValue)
						{
							GameActions.Print(string.Format(ResGeneral.YourSkillIn0Has1By2ItIsNow3, skill.Name, (num4 < 0f) ? ResGeneral.Decreased : ResGeneral.Increased, Math.Abs(num4), skill.Value + num4), 88, MessageType.System, 3, unicode: false);
						}
					}
					skill.BaseFixed = baseFixed;
					skill.ValueFixed = num3;
					skill.CapFixed = capFixed;
					skill.Lock = @lock;
					standardSkillsGump?.Update(num2);
					skillGumpAdvanced?.ForceUpdate();
				}
			}
			if (flag2)
			{
				break;
			}
		}
	}

	private static void Pathfinding(ref StackDataReader p)
	{
		if (World.InGame)
		{
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			ushort z = p.ReadUInt16BE();
			Pathfinder.WalkTo(x, y, z, 0);
		}
	}

	private static void UpdateContainedItems(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		ushort num = p.ReadUInt16BE();
		for (int i = 0; i < num; i++)
		{
			uint serial = p.ReadUInt32BE();
			ushort graphic = (ushort)(p.ReadUInt16BE() + p.ReadUInt8());
			ushort amount = Math.Max(p.ReadUInt16BE(), (ushort)1);
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			if (Client.Version >= ClassicUO.Data.ClientVersion.CV_6017)
			{
				p.Skip(1);
			}
			uint num2 = p.ReadUInt32BE();
			ushort hue = p.ReadUInt16BE();
			if (i == 0)
			{
				Entity entity = World.Get(num2);
				if (entity != null)
				{
					ClearContainerAndRemoveItems(entity, entity.Graphic == 8198);
				}
			}
			AddItemToContainer(serial, graphic, amount, x, y, hue, num2);
		}
	}

	private static void PersonalLightLevel(ref StackDataReader p)
	{
		if (World.InGame && (uint)World.Player == p.ReadUInt32BE())
		{
			byte b = p.ReadUInt8();
			if (b > 30)
			{
				b = 30;
			}
			World.Light.RealPersonal = b;
			if (!ProfileManager.CurrentProfile.UseCustomLightLevel)
			{
				World.Light.Personal = b;
			}
		}
	}

	private static void LightLevel(ref StackDataReader p)
	{
		if (World.InGame)
		{
			byte b = p.ReadUInt8();
			if (b > 30)
			{
				b = 30;
			}
			World.Light.RealOverall = b;
			if (!ProfileManager.CurrentProfile.UseCustomLightLevel)
			{
				World.Light.Overall = b;
			}
		}
	}

	private static void PlaySoundEffect(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			p.Skip(1);
			ushort index = p.ReadUInt16BE();
			p.ReadUInt16BE();
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			p.ReadUInt16BE();
			Client.Game.Scene.Audio.PlaySoundWithDistance(index, x, y);
		}
	}

	private static void PlayMusic(ref StackDataReader p)
	{
		ushort music = p.ReadUInt16BE();
		Client.Game.Scene.Audio.PlayMusic(music);
	}

	private static void LoginComplete(ref StackDataReader p)
	{
		if (!(World.Player != null) || !(Client.Game.Scene is LoginScene))
		{
			return;
		}
		GameScene gameScene = new GameScene();
		gameScene.Audio = Client.Game.Scene.Audio;
		Client.Game.Scene.Audio = null;
		Client.Game.SetScene(gameScene);
		GameActions.RequestMobileStatus(World.Player);
		NetClient.Socket.Send_OpenChat("");
		gameScene.DoubleClickDelayed(World.Player);
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_306E)
		{
			NetClient.Socket.Send_ClientType();
		}
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_305D)
		{
			NetClient.Socket.Send_ClientViewRange(World.ClientViewRange);
		}
		List<Gump> list = ProfileManager.CurrentProfile.ReadGumps(ProfileManager.ProfilePath);
		if (list == null)
		{
			return;
		}
		foreach (Gump item in list)
		{
			UIManager.Add(item);
		}
	}

	private static void MapData(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		MapGump gump = UIManager.GetGump<MapGump>(p.ReadUInt32BE());
		if (gump != null)
		{
			switch ((MapMessageType)p.ReadUInt8())
			{
			case MapMessageType.Add:
			{
				p.Skip(1);
				ushort x = p.ReadUInt16BE();
				ushort y = p.ReadUInt16BE();
				gump.AddPin(x, y);
				break;
			}
			case MapMessageType.Clear:
				gump.ClearContainer();
				break;
			case MapMessageType.EditResponse:
				gump.SetPlotState(p.ReadUInt8());
				break;
			case MapMessageType.Insert:
			case MapMessageType.Move:
			case MapMessageType.Remove:
			case MapMessageType.Edit:
				break;
			}
		}
	}

	private static void SetTime(ref StackDataReader p)
	{
	}

	private static void SetWeather(ref StackDataReader p)
	{
		GameScene scene = Client.Game.GetScene<GameScene>();
		if (scene != null)
		{
			Weather weather = scene.Weather;
			WEATHER_TYPE wEATHER_TYPE = (WEATHER_TYPE)p.ReadUInt8();
			if (weather.CurrentWeather != wEATHER_TYPE)
			{
				byte count = p.ReadUInt8();
				byte temp = p.ReadUInt8();
				weather.Generate(wEATHER_TYPE, count, temp);
			}
		}
	}

	private static void BookData(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint value = p.ReadUInt32BE();
		ushort num = p.ReadUInt16BE();
		ModernBookGump gump = UIManager.GetGump<ModernBookGump>(value);
		if (gump == null || gump.IsDisposed)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			int num2 = p.ReadUInt16BE() - 1;
			gump.KnownPages.Add(num2 + 1);
			if (num2 < gump.BookPageCount && num2 >= 0)
			{
				ushort num3 = p.ReadUInt16BE();
				for (int j = 0; j < num3; j++)
				{
					int num4 = num2 * 8 + j;
					if (num4 < gump.BookLines.Length)
					{
						gump.BookLines[num4] = (ModernBookGump.IsNewBook ? p.ReadUTF8(safe: true) : p.ReadASCII());
					}
					else
					{
						Log.Error("BOOKGUMP: The server is sending a page number GREATER than the allowed number of pages in BOOK!");
					}
				}
				if (num3 < 8)
				{
					for (int k = num3; k < 8; k++)
					{
						gump.BookLines[num2 * 8 + k] = string.Empty;
					}
				}
			}
			else
			{
				Log.Error("BOOKGUMP: The server is sending a page number GREATER than the allowed number of pages in BOOK!");
			}
		}
		gump.ServerSetBookText();
	}

	private static void CharacterAnimation(ref StackDataReader p)
	{
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (!(mobile == null))
		{
			ushort index = p.ReadUInt16BE();
			ushort num = p.ReadUInt16BE();
			ushort num2 = p.ReadUInt16BE();
			bool forward = !p.ReadBool();
			bool repeat = p.ReadBool();
			byte interval = p.ReadUInt8();
			mobile.SetAnimation(Mobile.GetReplacedObjectAnimation(mobile.Graphic, index), interval, (byte)num, (byte)num2, repeat, forward, fromServer: true);
		}
	}

	private static void GraphicEffect(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		GraphicEffectType graphicEffectType = (GraphicEffectType)p.ReadUInt8();
		if (graphicEffectType > GraphicEffectType.FixedFrom)
		{
			if (graphicEffectType == GraphicEffectType.ScreenFade && p[0] == 112)
			{
				p.Skip(8);
				ushort num = p.ReadUInt16BE();
				if (num > 5)
				{
					num = 5;
				}
				switch (num)
				{
				case 0:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.BlackScreenShotEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 2000
					});
					break;
				case 1:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.FadeScreenShotEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 2000
					});
					break;
				case 2:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.FadeScreenEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 200
					});
					break;
				case 3:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.FadeAndBlackEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 4000
					});
					break;
				case 4:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.FadeScreenEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 2000
					});
					break;
				case 5:
					ScreenEffectManager.ActiveScreenEffects.Add(new ScreenEffectManager.BlackScreenEffect
					{
						StartTime = Time.Ticks,
						Expiration = Time.Ticks + 2000
					});
					break;
				}
			}
			return;
		}
		uint source = p.ReadUInt32BE();
		uint target = p.ReadUInt32BE();
		ushort graphic = p.ReadUInt16BE();
		ushort srcX = p.ReadUInt16BE();
		ushort srcY = p.ReadUInt16BE();
		sbyte srcZ = p.ReadInt8();
		ushort targetX = p.ReadUInt16BE();
		ushort targetY = p.ReadUInt16BE();
		sbyte targetZ = p.ReadInt8();
		byte b = p.ReadUInt8();
		ushort duration = p.ReadUInt8();
		p.Skip(2);
		bool fixedDir = p.ReadBool();
		bool doesExplode = p.ReadBool();
		ushort hue = 0;
		GraphicEffectBlendMode blendmode = GraphicEffectBlendMode.Normal;
		if (p[0] == 112)
		{
			if (b > 20)
			{
				b -= 20;
			}
			b = (byte)(20 - b);
			if (graphicEffectType == GraphicEffectType.Moving)
			{
				graphicEffectType = GraphicEffectType.Moving70;
			}
		}
		else
		{
			hue = (ushort)p.ReadUInt32BE();
			blendmode = (GraphicEffectBlendMode)(p.ReadUInt32BE() % 7);
			if (b > 200)
			{
				b = 200;
			}
		}
		World.SpawnEffect(graphicEffectType, source, target, graphic, hue, srcX, srcY, srcZ, targetX, targetY, targetZ, b, duration, fixedDir, doesExplode, hasparticles: false, blendmode);
	}

	private static void ClientViewRange(ref StackDataReader p)
	{
		World.ClientViewRange = p.ReadUInt8();
	}

	private static void BulletinBoardData(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		switch (p.ReadUInt8())
		{
		case 0:
		{
			uint num4 = p.ReadUInt32BE();
			Item item = World.Items.Get(num4);
			if (item != null)
			{
				UIManager.GetGump<BulletinBoardGump>(num4)?.Dispose();
				int x = (Client.Game.Window.ClientBounds.Width >> 1) - 245;
				int y = (Client.Game.Window.ClientBounds.Height >> 1) - 205;
				UIManager.Add(new BulletinBoardGump(item, x, y, p.ReadUTF8(22, safe: true)));
				item.Opened = true;
			}
			break;
		}
		case 1:
		{
			BulletinBoardGump gump = UIManager.GetGump<BulletinBoardGump>(p.ReadUInt32BE());
			if (gump != null)
			{
				uint serial = p.ReadUInt32BE();
				p.ReadUInt32BE();
				int num3 = p.ReadUInt8();
				string text3 = ((num3 <= 0) ? string.Empty : p.ReadUTF8(num3, safe: true)) + " - ";
				num3 = p.ReadUInt8();
				text3 = text3 + ((num3 <= 0) ? string.Empty : p.ReadUTF8(num3, safe: true)) + " - ";
				num3 = p.ReadUInt8();
				text3 += ((num3 <= 0) ? string.Empty : p.ReadUTF8(num3, safe: true));
				gump.AddBulletinObject(serial, text3);
			}
			break;
		}
		case 2:
		{
			uint num = p.ReadUInt32BE();
			if (UIManager.GetGump<BulletinBoardGump>(num) == null)
			{
				break;
			}
			uint msgSerial = p.ReadUInt32BE();
			int num2 = p.ReadUInt8();
			string text = ((num2 > 0) ? p.ReadASCII(num2) : string.Empty);
			num2 = p.ReadUInt8();
			string subject = ((num2 > 0) ? p.ReadUTF8(num2, safe: true) : string.Empty);
			num2 = p.ReadUInt8();
			string datatime = ((num2 > 0) ? p.ReadASCII(num2) : string.Empty);
			p.Skip(4);
			byte b = p.ReadUInt8();
			if (b > 0)
			{
				p.Skip(b * 4);
			}
			byte b2 = p.ReadUInt8();
			Span<char> initialBuffer = stackalloc char[256];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			for (int i = 0; i < b2; i++)
			{
				byte b3 = p.ReadUInt8();
				if (b3 > 0)
				{
					string s = p.ReadUTF8(b3, safe: true);
					valueStringBuilder.Append(s);
					valueStringBuilder.Append('\n');
				}
			}
			string text2 = valueStringBuilder.ToString();
			byte variant = (byte)(1u + ((text == World.Player.Name) ? 1u : 0u));
			BulletinBoardItem bulletinBoardItem = new BulletinBoardItem(num, msgSerial, text, subject, datatime, text2.TrimStart(), variant);
			bulletinBoardItem.X = 40;
			bulletinBoardItem.Y = 40;
			UIManager.Add(bulletinBoardItem);
			valueStringBuilder.Dispose();
			break;
		}
		}
	}

	private static void Warmode(ref StackDataReader p)
	{
		if (World.InGame)
		{
			World.Player.InWarMode = p.ReadBool();
		}
	}

	private static void Ping(ref StackDataReader p)
	{
		if (NetClient.Socket.IsConnected && !NetClient.Socket.IsDisposed)
		{
			NetClient.Socket.Statistics.PingReceived();
		}
		else if (NetClient.LoginSocket.IsConnected && !NetClient.LoginSocket.IsDisposed)
		{
			NetClient.LoginSocket.Statistics.PingReceived();
		}
	}

	private static void BuyList(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		Item item = World.Items.Get(p.ReadUInt32BE());
		if (item == null)
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(item.Container);
		if (mobile == null)
		{
			return;
		}
		ShopGump shopGump = UIManager.GetGump<ShopGump>(null);
		if (shopGump != null && (shopGump.LocalSerial != (uint)mobile || !shopGump.IsBuyGump))
		{
			shopGump.Dispose();
			shopGump = null;
		}
		if (shopGump == null)
		{
			shopGump = new ShopGump(mobile, isBuyGump: true, 150, 5);
			UIManager.Add(shopGump);
		}
		if (item.Layer != Layer.ShopBuyRestock && item.Layer != Layer.ShopBuy)
		{
			return;
		}
		byte b = p.ReadUInt8();
		LinkedObject linkedObject = item.Items;
		if (linkedObject == null)
		{
			return;
		}
		bool flag = false;
		if (item.Graphic == 11000)
		{
			linkedObject = item.SortContents((Item x, Item y) => x.X - y.X);
		}
		else
		{
			flag = true;
			while (linkedObject?.Next != null)
			{
				linkedObject = linkedObject.Next;
			}
		}
		for (int i = 0; i < b; i++)
		{
			if (linkedObject == null)
			{
				break;
			}
			Item item2 = (Item)linkedObject;
			item2.Price = p.ReadUInt32BE();
			byte length = p.ReadUInt8();
			string text = p.ReadASCII(length);
			int result;
			if (World.OPL.TryGetNameAndData(item2.Serial, out var name, out var _))
			{
				item2.Name = name;
			}
			else if (int.TryParse(text, out result))
			{
				item2.Name = ClilocLoader.Instance.Translate(result, $"\t{item2.ItemData.Name}: \t{item2.Amount}", capitalize: true);
			}
			else if (string.IsNullOrEmpty(text))
			{
				item2.Name = item2.ItemData.Name;
			}
			else
			{
				item2.Name = text;
			}
			linkedObject = ((!flag) ? linkedObject.Next : linkedObject.Previous);
		}
	}

	private static void UpdateCharacter(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		Mobile mobile = World.Mobiles.Get(num);
		if (!(mobile == null))
		{
			ushort graphic = p.ReadUInt16BE();
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			sbyte z = p.ReadInt8();
			Direction direction = (Direction)p.ReadUInt8();
			ushort hue = p.ReadUInt16BE();
			Flags flags = (Flags)p.ReadUInt8();
			NotorietyFlag notorietyFlag = (NotorietyFlag)p.ReadUInt8();
			mobile.NotorietyFlag = notorietyFlag;
			if (num == (uint)World.Player)
			{
				mobile.Flags = flags;
				mobile.Graphic = graphic;
				mobile.CheckGraphicChange(0);
				mobile.FixHue(hue);
			}
			else
			{
				UpdateGameObject(num, graphic, 0, 0, x, y, z, direction, hue, flags, 0, 1, 1);
			}
		}
	}

	private static void UpdateObject(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		ushort graphic = p.ReadUInt16BE();
		ushort x = p.ReadUInt16BE();
		ushort y = p.ReadUInt16BE();
		sbyte z = p.ReadInt8();
		Direction direction = (Direction)p.ReadUInt8();
		ushort hue = p.ReadUInt16BE();
		Flags flags = (Flags)p.ReadUInt8();
		NotorietyFlag notorietyFlag = (NotorietyFlag)p.ReadUInt8();
		if (num == (uint)World.Player)
		{
			_ = World.Player.IsDead;
			World.Player.Graphic = graphic;
			World.Player.CheckGraphicChange(0);
			World.Player.FixHue(hue);
			World.Player.Flags = flags;
		}
		else
		{
			UpdateGameObject(num, graphic, 0, 0, x, y, z, direction, hue, flags, 0, 0, 1);
		}
		Entity entity = World.Get(num);
		if (entity == null)
		{
			return;
		}
		if (!entity.IsEmpty)
		{
			LinkedObject linkedObject = entity.Items;
			while (linkedObject != null)
			{
				LinkedObject next = linkedObject.Next;
				Item item = (Item)linkedObject;
				if (!item.Opened && item.Layer != Layer.Backpack)
				{
					World.RemoveItem(item.Serial, forceRemove: true);
				}
				linkedObject = next;
			}
		}
		if (SerialHelper.IsMobile(num) && entity is Mobile mobile)
		{
			mobile.NotorietyFlag = notorietyFlag;
			UIManager.GetGump<PaperDollGump>(num)?.RequestUpdateContents();
		}
		if (p[0] != 120)
		{
			p.Skip(6);
		}
		uint num2 = p.ReadUInt32BE();
		while (num2 != 0 && p.Position < p.Length)
		{
			ushort num3 = p.ReadUInt16BE();
			byte layer = p.ReadUInt8();
			ushort hue2 = 0;
			if (Client.Version >= ClassicUO.Data.ClientVersion.CV_70331)
			{
				hue2 = p.ReadUInt16BE();
			}
			else if ((num3 & 0x8000) != 0)
			{
				num3 &= 0x7FFF;
				hue2 = p.ReadUInt16BE();
			}
			Item orCreateItem = World.GetOrCreateItem(num2);
			orCreateItem.Graphic = num3;
			orCreateItem.FixHue(hue2);
			orCreateItem.Amount = 1;
			World.RemoveItemFromContainer(orCreateItem);
			orCreateItem.Container = num;
			orCreateItem.Layer = (Layer)layer;
			orCreateItem.CheckGraphicChange(0);
			entity.PushToBack(orCreateItem);
			num2 = p.ReadUInt32BE();
		}
		if (num == (uint)World.Player)
		{
			UIManager.GetGump<PaperDollGump>(num)?.RequestUpdateContents();
			World.Player.UpdateAbilities();
		}
	}

	private static void OpenMenu(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		ushort serv = p.ReadUInt16BE();
		string name = p.ReadASCII(p.ReadUInt8());
		int num2 = p.ReadUInt8();
		ushort num3 = p.ReadUInt16BE();
		p.Seek(p.Position - 2);
		if (num3 != 0)
		{
			MenuGump menuGump = new MenuGump(num, serv, name);
			menuGump.X = 100;
			menuGump.Y = 100;
			MenuGump menuGump2 = menuGump;
			int num4 = 0;
			for (int i = 0; i < num2; i++)
			{
				ushort num5 = p.ReadUInt16BE();
				ushort hue = p.ReadUInt16BE();
				name = p.ReadASCII(p.ReadUInt8());
				ArtLoader.Instance.GetStaticTexture(num5, out var bounds);
				if (bounds.Width != 0 && bounds.Height != 0)
				{
					int height = bounds.Height;
					height = ((height < 47) ? (47 - height >> 1) : 0);
					menuGump2.AddItem(num5, hue, name, num4, height, i + 1);
					num4 += bounds.Width;
				}
			}
			UIManager.Add(menuGump2);
			return;
		}
		GrayMenuGump grayMenuGump = new GrayMenuGump(num, serv, name);
		grayMenuGump.X = (Client.Game.Window.ClientBounds.Width >> 1) - 200;
		grayMenuGump.Y = (Client.Game.Window.ClientBounds.Height >> 1) - (121 + num2 * 21 >> 1);
		GrayMenuGump grayMenuGump2 = grayMenuGump;
		int num6 = 35 + grayMenuGump2.Height;
		int num7 = 70 + num6;
		for (int j = 0; j < num2; j++)
		{
			p.Skip(4);
			name = p.ReadASCII(p.ReadUInt8());
			int num8 = grayMenuGump2.AddItem(name, num6);
			if (num8 < 21)
			{
				num8 = 21;
			}
			num6 += num8 - 1;
			num7 += num8;
		}
		num6 += 5;
		Button obj = new Button(0, 5200, 5201, 5200, "", 0)
		{
			ButtonAction = ButtonAction.Activate
		};
		obj.X = 70;
		obj.Y = num6;
		grayMenuGump2.Add(obj);
		Button obj2 = new Button(1, 5042, 5043, 5042, "", 0)
		{
			ButtonAction = ButtonAction.Activate
		};
		obj2.X = 200;
		obj2.Y = num6;
		grayMenuGump2.Add(obj2);
		grayMenuGump2.SetHeight(num7);
		grayMenuGump2.WantUpdateSize = false;
		UIManager.Add(grayMenuGump2);
	}

	private static void OpenPaperdoll(ref StackDataReader p)
	{
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (mobile == null)
		{
			return;
		}
		string text = p.ReadASCII(60);
		byte b = p.ReadUInt8();
		byte paperdollGump = p.ReadUInt8();
		mobile.Title = text;
		PaperDollGump gump = UIManager.GetGump<PaperDollGump>(mobile);
		if (gump == null)
		{
			if (!UIManager.GetGumpCachePosition(mobile, out var pos))
			{
				pos = new Point(100, 100);
			}
			UIManager.Add(new PaperDollGump(mobile, (b & 2) != 0, paperdollGump)
			{
				Location = pos
			});
			return;
		}
		bool canLift = gump.CanLift;
		bool flag2 = (gump.CanLift = (b & 2) != 0);
		gump.UpdateTitle(text);
		if (canLift != flag2)
		{
			gump.RequestUpdateContents();
		}
		gump.SetInScreen();
		gump.BringOnTop();
	}

	private static void CorpseEquipment(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		Entity entity = World.Get(num);
		if (entity == null || entity.Graphic != 8198)
		{
			return;
		}
		Layer layer = (Layer)p.ReadUInt8();
		while (layer != 0 && p.Position < p.Length)
		{
			uint serial = p.ReadUInt32BE();
			if (layer != Layer.Backpack)
			{
				Item orCreateItem = World.GetOrCreateItem(serial);
				World.RemoveItemFromContainer(orCreateItem);
				orCreateItem.Container = num;
				orCreateItem.Layer = layer;
				entity.PushToBack(orCreateItem);
			}
			layer = (Layer)p.ReadUInt8();
		}
	}

	private static void DisplayMap(ref StackDataReader p)
	{
		uint serial = p.ReadUInt32BE();
		ushort gumpid = p.ReadUInt16BE();
		ushort startx = p.ReadUInt16BE();
		ushort starty = p.ReadUInt16BE();
		ushort endx = p.ReadUInt16BE();
		ushort endy = p.ReadUInt16BE();
		ushort width = p.ReadUInt16BE();
		ushort height = p.ReadUInt16BE();
		MapGump mapGump = new MapGump(serial, gumpid, width, height);
		if (p[0] == 245 || Client.Version >= ClassicUO.Data.ClientVersion.CV_308Z)
		{
			ushort num = 0;
			if (p[0] == 245)
			{
				num = p.ReadUInt16BE();
			}
			if (MultiMapLoader.Instance.HasFacet(num))
			{
				mapGump.SetMapTexture(MultiMapLoader.Instance.LoadFacet(num, width, height, startx, starty, endx, endy));
			}
			else
			{
				mapGump.SetMapTexture(MultiMapLoader.Instance.LoadMap(width, height, startx, starty, endx, endy));
			}
		}
		else
		{
			mapGump.SetMapTexture(MultiMapLoader.Instance.LoadMap(width, height, startx, starty, endx, endy));
		}
		UIManager.Add(mapGump);
		Item item = World.Items.Get(serial);
		if (item != null)
		{
			item.Opened = true;
		}
	}

	private static void OpenBook(ref StackDataReader p)
	{
		uint num = p.ReadUInt32BE();
		bool flag = p[0] == 147;
		bool flag2 = p.ReadBool();
		if (!flag)
		{
			flag2 = p.ReadBool();
		}
		else
		{
			p.Skip(1);
		}
		ModernBookGump gump = UIManager.GetGump<ModernBookGump>(num);
		if (gump == null || gump.IsDisposed)
		{
			ushort page_count = p.ReadUInt16BE();
			string title = (flag ? p.ReadUTF8(60, safe: true) : p.ReadUTF8(p.ReadUInt16BE(), safe: true));
			string author = (flag ? p.ReadUTF8(30, safe: true) : p.ReadUTF8(p.ReadUInt16BE(), safe: true));
			ModernBookGump modernBookGump = new ModernBookGump(num, page_count, title, author, flag2, flag);
			modernBookGump.X = 100;
			modernBookGump.Y = 100;
			UIManager.Add(modernBookGump);
			NetClient.Socket.Send_BookPageDataRequest(num, 1);
		}
		else
		{
			p.Skip(2);
			gump.IsEditable = flag2;
			gump.SetTile(flag ? p.ReadUTF8(60, safe: true) : p.ReadUTF8(p.ReadUInt16BE(), safe: true), flag2);
			gump.SetAuthor(flag ? p.ReadUTF8(30, safe: true) : p.ReadUTF8(p.ReadUInt16BE(), safe: true), flag2);
			gump.UseNewHeader = !flag;
			gump.SetInScreen();
			gump.BringOnTop();
		}
	}

	private static void DyeData(ref StackDataReader p)
	{
		uint num = p.ReadUInt32BE();
		p.Skip(2);
		ushort num2 = p.ReadUInt16BE();
		GumpsLoader.Instance.GetGumpTexture(2310u, out var bounds);
		int x = (Client.Game.Window.ClientBounds.Width >> 1) - (bounds.Width >> 1);
		int y = (Client.Game.Window.ClientBounds.Height >> 1) - (bounds.Height >> 1);
		ColorPickerGump gump = UIManager.GetGump<ColorPickerGump>(num);
		if (gump == null || gump.IsDisposed || gump.Graphic != num2)
		{
			gump?.Dispose();
			gump = new ColorPickerGump(num, num2, x, y, null);
			UIManager.Add(gump);
		}
	}

	private static void MovePlayer(ref StackDataReader p)
	{
		if (World.InGame)
		{
			Direction direction = (Direction)p.ReadUInt8();
			World.Player.Walk(direction & Direction.Up, (direction & Direction.Running) != 0);
		}
	}

	private static void UpdateName(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		string text = p.ReadASCII();
		WMapEntity entity = World.WMapManager.GetEntity(num);
		if (entity != null && !string.IsNullOrEmpty(text))
		{
			entity.Name = text;
		}
		Entity entity2 = World.Get(num);
		if (entity2 != null)
		{
			entity2.Name = text;
			if (num == World.Player.Serial && !string.IsNullOrEmpty(text) && text != World.Player.Name)
			{
				Client.Game.SetWindowTitle(text);
			}
			UIManager.GetGump<NameOverheadGump>(num)?.SetName();
		}
		if (World.Party != null && World.Party.Contains(num))
		{
			World.Party.SetName(num, text);
		}
	}

	private static void MultiPlacement(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			p.ReadBool();
			uint deedSerial = p.ReadUInt32BE();
			p.ReadUInt8();
			p.Seek(18L);
			ushort model = p.ReadUInt16BE();
			ushort x = p.ReadUInt16BE();
			ushort y = p.ReadUInt16BE();
			ushort z = p.ReadUInt16BE();
			ushort hue = p.ReadUInt16BE();
			TargetManager.SetTargetingMulti(deedSerial, model, x, y, z, hue);
		}
	}

	private static void ASCIIPrompt(ref StackDataReader p)
	{
		if (World.InGame)
		{
			PromptData promptData = default(PromptData);
			promptData.Prompt = ConsolePrompt.ASCII;
			promptData.Data = p.ReadUInt64BE();
			MessageManager.PromptData = promptData;
		}
	}

	private static void SellList(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (mobile == null)
		{
			return;
		}
		ushort num = p.ReadUInt16BE();
		if (num <= 0)
		{
			return;
		}
		UIManager.GetGump<ShopGump>(mobile)?.Dispose();
		ShopGump shopGump = new ShopGump(mobile, isBuyGump: false, 100, 0);
		for (int i = 0; i < num; i++)
		{
			uint serial = p.ReadUInt32BE();
			ushort num2 = p.ReadUInt16BE();
			ushort hue = p.ReadUInt16BE();
			ushort amount = p.ReadUInt16BE();
			ushort price = p.ReadUInt16BE();
			string name = p.ReadASCII(p.ReadUInt16BE());
			bool fromcliloc = false;
			string data;
			if (int.TryParse(name, out var result))
			{
				name = ClilocLoader.Instance.GetString(result);
				fromcliloc = true;
			}
			else if (string.IsNullOrEmpty(name) && !World.OPL.TryGetNameAndData(serial, out name, out data))
			{
				name = TileDataLoader.Instance.StaticData[num2].Name;
			}
			shopGump.AddItem(serial, num2, hue, amount, price, name, fromcliloc);
		}
		UIManager.Add(shopGump);
	}

	private static void UpdateHitpoints(ref StackDataReader p)
	{
		Entity entity = World.Get(p.ReadUInt32BE());
		if (!(entity == null))
		{
			entity.HitsMax = p.ReadUInt16BE();
			entity.Hits = p.ReadUInt16BE();
			if (entity.HitsRequest == HitsRequestStatus.Pending)
			{
				entity.HitsRequest = HitsRequestStatus.Received;
			}
			if (entity == World.Player)
			{
				UoAssist.SignalHits();
			}
		}
	}

	private static void UpdateMana(ref StackDataReader p)
	{
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (!(mobile == null))
		{
			mobile.ManaMax = p.ReadUInt16BE();
			mobile.Mana = p.ReadUInt16BE();
			if (mobile == World.Player)
			{
				UoAssist.SignalMana();
			}
		}
	}

	private static void UpdateStamina(ref StackDataReader p)
	{
		Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
		if (!(mobile == null))
		{
			mobile.StaminaMax = p.ReadUInt16BE();
			mobile.Stamina = p.ReadUInt16BE();
			if (mobile == World.Player)
			{
				UoAssist.SignalStamina();
			}
		}
	}

	private static void OpenUrl(ref StackDataReader p)
	{
		string text = p.ReadASCII();
		if (!string.IsNullOrEmpty(text))
		{
			PlatformHelper.LaunchBrowser(text);
		}
	}

	private static void TipWindow(ref StackDataReader p)
	{
		byte b = p.ReadUInt8();
		if (b != 1)
		{
			uint serial = p.ReadUInt32BE();
			string text = p.ReadASCII(p.ReadUInt16BE())?.Replace('\r', '\n');
			int num = 20;
			int num2 = 20;
			if (b == 0)
			{
				num = 200;
				num2 = 100;
			}
			TipNoticeGump tipNoticeGump = new TipNoticeGump(serial, b, text);
			tipNoticeGump.X = num;
			tipNoticeGump.Y = num2;
			UIManager.Add(tipNoticeGump);
		}
	}

	private static void AttackCharacter(ref StackDataReader p)
	{
		uint num = p.ReadUInt32BE();
		GameActions.SendCloseStatus(TargetManager.LastAttack);
		TargetManager.LastAttack = num;
		GameActions.RequestMobileStatus(num);
	}

	private static void TextEntryDialog(ref StackDataReader p)
	{
		if (World.InGame)
		{
			uint serial = p.ReadUInt32BE();
			byte parentid = p.ReadUInt8();
			byte buttonid = p.ReadUInt8();
			ushort length = p.ReadUInt16BE();
			string text = p.ReadASCII(length);
			bool canCloseWithRightClick = p.ReadBool();
			byte variant = p.ReadUInt8();
			uint maxlen = p.ReadUInt32BE();
			ushort length2 = p.ReadUInt16BE();
			string description = p.ReadASCII(length2);
			UIManager.Add(new TextEntryDialogGump(serial, 143, 172, variant, (int)maxlen, text, description, buttonid, parentid)
			{
				CanCloseWithRightClick = canCloseWithRightClick
			});
		}
	}

	private static void UnicodeTalk(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			if (Client.Game.GetScene<LoginScene>() != null)
			{
				Log.Warn("UnicodeTalk received during LoginScene");
				if (p.Length > 48)
				{
					p.Seek(48L);
					Log.PushIndent();
					Log.Warn("Handled UnicodeTalk in LoginScene");
					Log.PopIndent();
				}
			}
			return;
		}
		uint num = p.ReadUInt32BE();
		Entity entity = World.Get(num);
		ushort num2 = p.ReadUInt16BE();
		MessageType messageType = (MessageType)p.ReadUInt8();
		ushort num3 = p.ReadUInt16BE();
		ushort num4 = p.ReadUInt16BE();
		string lang = p.ReadUTF8(4);
		string text = p.ReadASCII();
		if (num == 0 && num2 == 0 && messageType == MessageType.Regular && num4 == ushort.MaxValue && num3 == ushort.MaxValue && text.ToLower() == "system")
		{
			byte[] array = new byte[40]
			{
				3, 0, 40, 32, 0, 52, 0, 3, 219, 19,
				20, 63, 69, 44, 88, 15, 93, 68, 46, 80,
				17, 223, 117, 92, 224, 62, 113, 79, 49, 52,
				5, 78, 24, 30, 114, 15, 89, 173, 245, 0
			};
			NetClient.Socket.Send(array, array.Length);
			return;
		}
		string text2 = string.Empty;
		if (p.Length > 48)
		{
			p.Seek(48L);
			text2 = p.ReadUnicodeBE();
		}
		TextType textType = TextType.SYSTEM;
		if (messageType == MessageType.Alliance || messageType == MessageType.Guild)
		{
			textType = TextType.GUILD_ALLY;
		}
		else if (entity == null && messageType != MessageType.System)
		{
			text = "";
		}
		else
		{
			if (messageType == MessageType.System || num == uint.MaxValue || num == 0 || (text.ToLower() == "system" && entity == null))
			{
				return;
			}
			if (entity != null)
			{
				textType = TextType.OBJECT;
				entity.Name = (string.IsNullOrEmpty(text) ? text2 : text);
			}
		}
		if (entity != null && entity is Mobile)
		{
			((Mobile)entity)._lastTextActivity = Time.Ticks - 4000;
		}
		MessageManager.HandleMessage(entity, text2, text, num3, messageType, ProfileManager.CurrentProfile.ChatFont, textType, unicode: true, lang);
	}

	private static void DisplayDeath(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		uint num2 = p.ReadUInt32BE();
		uint num3 = p.ReadUInt32BE();
		Mobile mobile = World.Mobiles.Get(num);
		if (mobile == null || num == (uint)World.Player)
		{
			if (num != (uint)World.Player && SerialHelper.IsValid(num2) && ProfileManager.CurrentProfile.CloseHealthBarType == 2)
			{
				UIManager.GetGump<BaseHealthBarGump>(num)?.Dispose();
			}
			return;
		}
		if (num != (uint)World.Player && SerialHelper.IsValid(num2) && ProfileManager.CurrentProfile.CloseHealthBarType == 2 && (World.Party == null || !World.Party.Contains(num)))
		{
			UIManager.GetGump<BaseHealthBarGump>(num)?.Dispose();
		}
		num |= 0x80000000u;
		if (World.Mobiles.Remove(mobile.Serial))
		{
			for (LinkedObject linkedObject = mobile.Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				((Item)linkedObject).Container = num;
			}
			World.Mobiles[num] = mobile;
			mobile.Serial = num;
		}
		if (SerialHelper.IsValid(num2))
		{
			World.CorpseManager.Add(num2, num, mobile.Direction, num3 != 0);
		}
		byte dieGroupIndex = AnimationsLoader.Instance.GetDieGroupIndex(mobile.Graphic, num3 != 0, isRunning: true);
		mobile.SetAnimation(dieGroupIndex, 0, 5, 1);
		mobile.AnimIndex = 0;
		if (ProfileManager.CurrentProfile.AutoOpenCorpses)
		{
			World.Player.TryOpenCorpses();
		}
	}

	private static void OpenGump(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint sender = p.ReadUInt32BE();
		uint gumpID = p.ReadUInt32BE();
		int x = (int)p.ReadUInt32BE();
		int y = (int)p.ReadUInt32BE();
		ushort length = p.ReadUInt16BE();
		string layout = p.ReadASCII(length);
		ushort num = p.ReadUInt16BE();
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			int num2 = p.ReadUInt16BE();
			if (num2 > 0)
			{
				array[i] = p.ReadUnicodeBE(num2);
			}
			else
			{
				array[i] = string.Empty;
			}
		}
		CreateGump(sender, gumpID, x, y, layout, array);
	}

	private static void ChatMessage(ref StackDataReader p)
	{
		ushort num = p.ReadUInt16BE();
		switch (num)
		{
		case 1000:
		{
			p.Skip(4);
			string arg = p.ReadUnicodeBE();
			bool hasPassword = p.ReadUInt16BE() == 49;
			ChatManager.CurrentChannelName = arg;
			ChatManager.AddChannel(arg, hasPassword);
			UIManager.GetGump<ChatGump>(null)?.RequestUpdateContents();
			break;
		}
		case 1001:
		{
			p.Skip(4);
			string arg = p.ReadUnicodeBE();
			ChatManager.RemoveChannel(arg);
			UIManager.GetGump<ChatGump>(null)?.RequestUpdateContents();
			break;
		}
		case 1003:
			ChatManager.ChatIsEnabled = ChatStatus.EnabledUserRequest;
			break;
		case 1004:
			ChatManager.Clear();
			ChatManager.ChatIsEnabled = ChatStatus.Disabled;
			UIManager.GetGump<ChatGump>(null)?.Dispose();
			break;
		case 1005:
			p.Skip(4);
			p.ReadUnicodeBE();
			ChatManager.ChatIsEnabled = ChatStatus.Enabled;
			NetClient.Socket.Send_ChatJoinCommand("General");
			break;
		case 1006:
			p.Skip(4);
			p.ReadUInt16BE();
			p.ReadUnicodeBE();
			break;
		case 1007:
			p.Skip(4);
			p.ReadUnicodeBE();
			break;
		case 1009:
		{
			p.Skip(4);
			string arg = (ChatManager.CurrentChannelName = p.ReadUnicodeBE());
			UIManager.GetGump<ChatGump>(null)?.UpdateConference();
			GameActions.Print(string.Format(ResGeneral.YouHaveJoinedThe0Channel, arg), ProfileManager.CurrentProfile.ChatMessageHue, MessageType.Regular, 1);
			break;
		}
		case 1012:
		{
			p.Skip(4);
			string arg = p.ReadUnicodeBE();
			GameActions.Print(string.Format(ResGeneral.YouHaveLeftThe0Channel, arg), ProfileManager.CurrentProfile.ChatMessageHue, MessageType.Regular, 1);
			break;
		}
		case 37:
		case 38:
		case 39:
		{
			p.Skip(4);
			p.ReadUInt16BE();
			string text3 = p.ReadUnicodeBE();
			string text4 = p.ReadUnicodeBE();
			if (!string.IsNullOrEmpty(text4))
			{
				int num2 = text4.IndexOf('{');
				int num3 = text4.IndexOf('}') + 1;
				if (num3 > num2 && num2 > -1)
				{
					text4 = text4.Remove(num2, num3 - num2);
				}
			}
			GameActions.Print(text3 + ": " + text4, ProfileManager.CurrentProfile.ChatMessageHue, MessageType.Regular, 1);
			break;
		}
		default:
			if (num < 40 || num > 44)
			{
				break;
			}
			goto case 1;
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
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
		case 36:
		{
			string text = ChatManager.GetMessage(num - 1);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			p.Skip(4);
			string text2 = p.ReadUnicodeBE();
			if (!string.IsNullOrEmpty(text2))
			{
				if (text.IndexOf("%1") >= 0)
				{
					text = text.Replace("%1", text2);
				}
				if ((num - 1 == 10 || num - 1 == 23) && text.IndexOf("%2") >= 0)
				{
					text = text.Replace("%2", text2);
				}
			}
			GameActions.Print(text, ProfileManager.CurrentProfile.ChatMessageHue, MessageType.Regular, 1);
			break;
		}
		case 1008:
			break;
		}
	}

	private static void Help(ref StackDataReader p)
	{
	}

	private static void CharacterProfile(ref StackDataReader p)
	{
		if (World.InGame)
		{
			uint num = p.ReadUInt32BE();
			string header = p.ReadASCII();
			string footer = p.ReadUnicodeBE();
			string body = p.ReadUnicodeBE();
			UIManager.GetGump<ProfileGump>(num)?.Dispose();
			UIManager.Add(new ProfileGump(num, header, footer, body, num == World.Player.Serial));
		}
	}

	private static void EnableLockedFeatures(ref StackDataReader p)
	{
		uint num = 0u;
		num = ((Client.Version < ClassicUO.Data.ClientVersion.CV_60142) ? p.ReadUInt16BE() : p.ReadUInt32BE());
		World.ClientLockedFeatures.SetFlags((LockedFeatureFlags)num);
		ChatManager.ChatIsEnabled = (World.ClientLockedFeatures.T2A ? ChatStatus.Enabled : ChatStatus.Disabled);
		AnimationsLoader.Instance.UpdateAnimationTable(num);
	}

	private static void DisplayQuestArrow(ref StackDataReader p)
	{
		bool num = p.ReadBool();
		ushort num2 = p.ReadUInt16BE();
		ushort num3 = p.ReadUInt16BE();
		uint num4 = 0u;
		if (Client.Version >= ClassicUO.Data.ClientVersion.CV_7090)
		{
			num4 = p.ReadUInt32BE();
		}
		QuestArrowGump gump = UIManager.GetGump<QuestArrowGump>(num4);
		if (num)
		{
			if (gump == null)
			{
				UIManager.Add(new QuestArrowGump(num4, num2, num3));
			}
			else
			{
				gump.SetRelativePosition(num2, num3);
			}
		}
		else
		{
			gump?.Dispose();
		}
	}

	private static void UltimaMessengerR(ref StackDataReader p)
	{
	}

	private static void CustomSeason(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			World.ChangeSeason(World.OldSeason = (Season)p.ReadUInt8(), 0);
		}
	}

	private static void Season(ref StackDataReader p)
	{
	}

	private static void ClientVersion(ref StackDataReader p)
	{
		NetClient.Socket.Send_ClientVersion(Settings.GlobalSettings.ClientVersion);
	}

	private static void AssistVersion(ref StackDataReader p)
	{
	}

	private static void ExtendedCommand(ref StackDataReader p)
	{
		ushort num = p.ReadUInt16BE();
		uint num3;
		Mobile mobile2;
		switch (num)
		{
		case 1:
		{
			for (int m = 0; m < 6; m++)
			{
				World.Player.Walker.FastWalkStack.SetValue(m, p.ReadUInt32BE());
			}
			break;
		}
		case 2:
			World.Player.Walker.FastWalkStack.AddValue(p.ReadUInt32BE());
			break;
		case 4:
		{
			uint num16 = p.ReadUInt32BE();
			int num17 = (int)p.ReadUInt32BE();
			LinkedListNode<Gump> linkedListNode = UIManager.Gumps.First;
			while (linkedListNode != null)
			{
				LinkedListNode<Gump>? next = linkedListNode.Next;
				if (linkedListNode.Value.ServerSerial == num16 && linkedListNode.Value.IsFromServer)
				{
					if (num17 != 0)
					{
						linkedListNode.Value?.OnButtonClick(num17);
					}
					else if (linkedListNode.Value.CanMove)
					{
						UIManager.SavePosition(num16, linkedListNode.Value.Location);
					}
					else
					{
						UIManager.RemovePosition(num16);
					}
					linkedListNode.Value.Dispose();
				}
				linkedListNode = next;
			}
			break;
		}
		case 6:
			World.Party.ParsePacket(ref p);
			break;
		case 8:
			World.MapIndex = p.ReadUInt8();
			break;
		case 12:
			UIManager.GetGump<HealthBarGump>(p.ReadUInt32BE())?.Dispose();
			break;
		case 16:
		{
			Item item2 = World.Items.Get(p.ReadUInt32BE());
			if (item2 == null)
			{
				break;
			}
			uint num7 = p.ReadUInt32BE();
			string empty = string.Empty;
			if (num7 != 0)
			{
				empty = ClilocLoader.Instance.GetString((int)num7, camelcase: true);
				if (!string.IsNullOrEmpty(empty))
				{
					item2.Name = empty;
				}
				MessageManager.HandleMessage(item2, empty, item2.Name, 946, MessageType.Regular, 3, TextType.OBJECT, unicode: true);
			}
			empty = string.Empty;
			ushort num8 = 0;
			uint num9 = p.ReadUInt32BE();
			Span<char> initialBuffer = stackalloc char[256];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			if (num9 == 4294967293u)
			{
				num8 = p.ReadUInt16BE();
				if (num8 > 0)
				{
					valueStringBuilder.Append(ResGeneral.CraftedBy);
					valueStringBuilder.Append(p.ReadASCII(num8));
				}
			}
			if (num8 != 0)
			{
				num9 = p.ReadUInt32BE();
			}
			if (num9 == 4294967292u)
			{
				valueStringBuilder.Append("[Unidentified");
			}
			byte b3 = 0;
			while (p.Position < p.Length - 4)
			{
				if (b3 != 0 || num9 == 4294967293u || num9 == 4294967292u)
				{
					num9 = p.ReadUInt32BE();
				}
				short num10 = (short)p.ReadUInt16BE();
				string @string = ClilocLoader.Instance.GetString((int)num9);
				if (num10 == -1)
				{
					if (b3 > 0)
					{
						valueStringBuilder.Append("/");
						valueStringBuilder.Append(@string);
					}
					else
					{
						valueStringBuilder.Append(" [");
						valueStringBuilder.Append(@string);
					}
				}
				else
				{
					valueStringBuilder.Append("\n[");
					valueStringBuilder.Append(@string);
					valueStringBuilder.Append(" : ");
					valueStringBuilder.Append(num10.ToString());
					valueStringBuilder.Append("]");
					b3 += 20;
				}
				b3++;
			}
			if ((b3 < 20 && b3 > 0) || (num9 == 4294967292u && b3 == 0))
			{
				valueStringBuilder.Append(']');
			}
			if (valueStringBuilder.Length != 0)
			{
				MessageManager.HandleMessage(item2, valueStringBuilder.ToString(), item2.Name, 946, MessageType.Regular, 3, TextType.OBJECT, unicode: true);
			}
			valueStringBuilder.Dispose();
			NetClient.Socket.Send_MegaClilocRequest_Old(item2);
			break;
		}
		case 20:
		{
			PopupMenuGump popupMenuGump = new PopupMenuGump(PopupMenuData.Parse(ref p));
			popupMenuGump.X = DelayedObjectClickManager.LastMouseX;
			popupMenuGump.Y = DelayedObjectClickManager.LastMouseY;
			UIManager.ShowGamePopup(popupMenuGump);
			break;
		}
		case 22:
		{
			uint num15 = p.ReadUInt32BE();
			num3 = p.ReadUInt32BE();
			switch (num15)
			{
			case 1u:
				UIManager.GetGump<PaperDollGump>(num3)?.Dispose();
				break;
			case 2u:
				UIManager.GetGump<HealthBarGump>(num3)?.Dispose();
				if (num3 == World.Player.Serial)
				{
					StatusGumpBase.GetStatusGump()?.Dispose();
				}
				break;
			case 8u:
				UIManager.GetGump<ProfileGump>(null)?.Dispose();
				break;
			case 12u:
				UIManager.GetGump<ContainerGump>(num3)?.Dispose();
				break;
			}
			break;
		}
		case 24:
			if (MapLoader.Instance.ApplyPatches(ref p))
			{
				int mapIndex = World.MapIndex;
				World.MapIndex = -1;
				World.MapIndex = mapIndex;
				Log.Trace("Map Patches applied.");
			}
			break;
		case 25:
		{
			byte b4 = p.ReadUInt8();
			num3 = p.ReadUInt32BE();
			if (b4 != 0)
			{
				if (b4 != 2)
				{
					if (b4 != 5)
					{
						break;
					}
					int position = p.Position;
					p.ReadUInt8();
					if (p.ReadUInt8() == byte.MaxValue)
					{
						byte num12 = p.ReadUInt8();
						ushort num13 = p.ReadUInt16BE();
						ushort num14 = p.ReadUInt16BE();
						if (num12 == 0 && num13 == 0 && num14 == 0)
						{
							p.Seek(position);
							goto IL_051d;
						}
						Mobile mobile = World.Mobiles.Get(num3);
						if (mobile != null)
						{
							mobile.SetAnimation(Mobile.GetReplacedObjectAnimation(mobile.Graphic, num13), 0, 0, 0);
							mobile.ExecuteAnimation = false;
							mobile.AnimIndex = (byte)num14;
						}
						break;
					}
					if (!(World.Player != null) || num3 != (uint)World.Player)
					{
						break;
					}
					p.Seek(position);
				}
				if (num3 == (uint)World.Player)
				{
					p.ReadUInt8();
					byte b5 = p.ReadUInt8();
					World.Player.StrLock = (Lock)((b5 >> 4) & 3);
					World.Player.DexLock = (Lock)((b5 >> 2) & 3);
					World.Player.IntLock = (Lock)(b5 & 3);
					StatusGumpBase.GetStatusGump()?.RequestUpdateContents();
				}
				break;
			}
			goto IL_051d;
		}
		case 27:
		{
			p.Skip(2);
			Item orCreateItem = World.GetOrCreateItem(p.ReadUInt32BE());
			orCreateItem.Graphic = p.ReadUInt16BE();
			orCreateItem.Clear();
			ushort num2 = p.ReadUInt16BE();
			for (int i = 0; i < 2; i++)
			{
				uint num5 = 0u;
				for (int j = 0; j < 4; j++)
				{
					num5 |= (uint)(p.ReadUInt8() << j * 8);
				}
				for (int k = 0; k < 32; k++)
				{
					if ((num5 & (1 << k)) != 0L)
					{
						ushort num6 = (ushort)(i * 32 + k + 1);
						Item item = Item.Create(num6);
						item.Serial = num6;
						item.Graphic = 7982;
						item.Amount = num6;
						item.Container = orCreateItem;
						orCreateItem.PushToBack(item);
					}
				}
			}
			UIManager.GetGump<SpellbookGump>(orCreateItem)?.RequestUpdateContents();
			break;
		}
		case 29:
		{
			num3 = p.ReadUInt32BE();
			uint num11 = p.ReadUInt32BE();
			if (World.Items.Get(num3) == null)
			{
				World.HouseManager.Remove(num3);
			}
			if (!World.HouseManager.TryGetHouse(num3, out var house) || !house.IsCustom || house.Revision != num11)
			{
				NetClient.Socket.Send_CustomHouseDataRequest(num3);
				break;
			}
			house.Generate();
			BoatMovingManager.ClearSteps(num3);
			UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
			if (World.HouseManager.EntityIntoHouse(num3, World.Player))
			{
				Client.Game.GetScene<GameScene>()?.UpdateMaxDrawZ(force: true);
			}
			break;
		}
		case 32:
		{
			num3 = p.ReadUInt32BE();
			ushort num2 = p.ReadUInt8();
			p.ReadUInt16BE();
			p.ReadUInt16BE();
			p.ReadUInt16BE();
			p.ReadInt8();
			switch (num2)
			{
			case 4:
				if (UIManager.GetGump<HouseCustomizationGump>(null) == null)
				{
					UIManager.Add(new HouseCustomizationGump(num3, 50, 50));
				}
				break;
			case 5:
				UIManager.GetGump<HouseCustomizationGump>(num3)?.Dispose();
				break;
			case 1:
			case 2:
			case 3:
				break;
			}
			break;
		}
		case 33:
		{
			for (int l = 0; l < 2; l++)
			{
				World.Player.Abilities[l] &= (Ability)127;
			}
			break;
		}
		case 34:
		{
			p.Skip(1);
			Entity entity = World.Get(p.ReadUInt32BE());
			if (entity != null)
			{
				byte b2 = p.ReadUInt8();
				if (b2 > 0)
				{
					World.WorldTextManager.AddDamage(entity, b2);
				}
			}
			break;
		}
		case 37:
		{
			ushort num4 = p.ReadUInt16BE();
			bool flag = p.ReadBool();
			{
				foreach (Gump gump in UIManager.Gumps)
				{
					if (!gump.IsDisposed && gump.IsVisible && gump is UseSpellButtonGump useSpellButtonGump && useSpellButtonGump.SpellID == num4)
					{
						if (flag)
						{
							useSpellButtonGump.Hue = 38;
							World.ActiveSpellIcons.Add(num4);
						}
						else
						{
							useSpellButtonGump.Hue = 0;
							World.ActiveSpellIcons.Remove(num4);
						}
						break;
					}
				}
				break;
			}
		}
		case 38:
		{
			byte b = p.ReadUInt8();
			if (b > 3)
			{
				b = 0;
			}
			World.Player.SpeedMode = (CharacterSpeedType)b;
			break;
		}
		case 42:
			p.ReadBool();
			p.ReadUInt8();
			GameActions.Print("[DEBUG]: change-race gump is not implemented yet.", 34, MessageType.Regular, 3);
			break;
		case 43:
		{
			num3 = p.ReadUInt16BE();
			byte id = p.ReadUInt8();
			byte animIndex = p.ReadUInt8();
			{
				foreach (Mobile value in World.Mobiles.Values)
				{
					if ((value.Serial & 0xFFFF) == num3)
					{
						value.SetAnimation(id, 0, 0, 0);
						value.AnimIndex = animIndex;
						value.ExecuteAnimation = false;
						break;
					}
				}
				break;
			}
		}
		case 48879:
		{
			ushort num2 = p.ReadUInt16BE();
			break;
		}
		default:
			Log.Warn("Unhandled 0xBF - sub: " + num.ToHex());
			break;
		case 0:
		case 17:
			break;
			IL_051d:
			mobile2 = World.Mobiles.Get(num3);
			if (!(mobile2 == null))
			{
				bool isDead = p.ReadBool();
				mobile2.IsDead = isDead;
			}
			break;
		}
	}

	private static void DisplayClilocString(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		uint num = p.ReadUInt32BE();
		Entity entity = World.Get(num);
		p.ReadUInt16BE();
		MessageType type = (MessageType)p.ReadUInt8();
		ushort hue = p.ReadUInt16BE();
		ushort num2 = p.ReadUInt16BE();
		uint num3 = p.ReadUInt32BE();
		AffixType affixType = (AffixType)((p[0] == 204) ? p.ReadUInt8() : 0);
		string text = p.ReadASCII(30);
		string text2 = ((p[0] == 204) ? p.ReadASCII() : string.Empty);
		string arg = null;
		if (num3 == 1008092 || num3 == 1005445)
		{
			for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
			{
				if (linkedListNode.Value is PartyInviteGump partyInviteGump)
				{
					partyInviteGump.Dispose();
				}
			}
		}
		int remaining = p.Remaining;
		if (remaining > 0)
		{
			arg = ((p[0] != 204) ? p.ReadUnicodeLE(remaining / 2) : p.ReadUnicodeBE(remaining));
		}
		string text3 = ClilocLoader.Instance.Translate((int)num3, arg);
		if (text3 == null)
		{
			return;
		}
		if (!string.IsNullOrWhiteSpace(text2))
		{
			text3 = (((affixType & AffixType.Prepend) == 0) ? (text3 + text2) : (text2 + text3));
		}
		if ((affixType & AffixType.System) != 0)
		{
			type = MessageType.System;
		}
		if (!FontsLoader.Instance.UnicodeFontExists((byte)num2))
		{
			num2 = 0;
		}
		TextType textType = TextType.SYSTEM;
		if (num != uint.MaxValue && num != 0 && (string.IsNullOrEmpty(text) || !string.Equals(text, "system", StringComparison.InvariantCultureIgnoreCase)) && entity != null)
		{
			textType = TextType.OBJECT;
			if (string.IsNullOrEmpty(entity.Name))
			{
				entity.Name = text;
			}
		}
		MessageManager.HandleMessage(entity, text3, text, hue, type, (byte)num2, textType, unicode: true);
	}

	private static void UnicodePrompt(ref StackDataReader p)
	{
		if (World.InGame)
		{
			PromptData promptData = default(PromptData);
			promptData.Prompt = ConsolePrompt.Unicode;
			promptData.Data = p.ReadUInt64BE();
			MessageManager.PromptData = promptData;
		}
	}

	private static void Semivisible(ref StackDataReader p)
	{
	}

	private static void InvalidMapEnable(ref StackDataReader p)
	{
	}

	private static void ParticleEffect3D(ref StackDataReader p)
	{
	}

	private static void GetUserServerPingGodClientR(ref StackDataReader p)
	{
	}

	private static void GlobalQueCount(ref StackDataReader p)
	{
	}

	private static void ConfigurationFileR(ref StackDataReader p)
	{
	}

	private static void Logout(ref StackDataReader p)
	{
		if (Client.Game.GetScene<GameScene>().DisconnectionRequested && (World.ClientFeatures.Flags & CharacterListFlags.CLF_OWERWRITE_CONFIGURATION_BUTTON) != 0)
		{
			if (p.ReadBool())
			{
				NetClient.Socket.Disconnect();
				Client.Game.SetScene(new LoginScene());
			}
			else
			{
				Log.Warn("0x1D - client asked to disconnect but server answered 'NO!'");
			}
		}
	}

	private static void MegaCliloc(ref StackDataReader p)
	{
		if (!World.InGame || p.ReadUInt16BE() > 1)
		{
			return;
		}
		uint serial = p.ReadUInt32BE();
		p.Skip(2);
		uint revision = p.ReadUInt32BE();
		Entity entity = World.Mobiles.Get(serial);
		if (entity == null)
		{
			if (SerialHelper.IsMobile(serial))
			{
				Log.Warn("Searching a mobile into World.Items from MegaCliloc packet");
			}
			entity = World.Items.Get(serial);
		}
		List<(int, string)> list = new List<(int, string)>();
		int num = 0;
		while (p.Position < p.Length)
		{
			int num2 = (int)p.ReadUInt32BE();
			if (num2 == 0)
			{
				break;
			}
			ushort num3 = p.ReadUInt16BE();
			string arg = string.Empty;
			if (num3 != 0)
			{
				arg = p.ReadUnicodeLE(num3 / 2);
			}
			string text = ClilocLoader.Instance.Translate(num2, arg);
			if (text == null)
			{
				continue;
			}
			if (Client.Version >= ClassicUO.Data.ClientVersion.CV_60143 && num2 == 1080418)
			{
				text = text.Insert(0, "<basefont color=#42a5ff>");
				text += "</basefont>";
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Item1 == num2 && string.Equals(list[i].Item2, text, StringComparison.Ordinal))
				{
					list.RemoveAt(i);
					break;
				}
			}
			list.Add((num2, text));
			num += text.Length;
		}
		Item item = null;
		if (entity is Item item2 && SerialHelper.IsValid(item2.Container))
		{
			item = World.Items.Get(item2.Container);
		}
		bool flag = false;
		if (item != null)
		{
			flag = item.Layer == Layer.ShopBuy || item.Layer == Layer.ShopBuyRestock || item.Layer == Layer.ShopSell;
		}
		bool flag2 = true;
		string name = string.Empty;
		string data = string.Empty;
		if (list.Count != 0)
		{
			Span<char> initialBuffer = stackalloc char[num];
			ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
			foreach (var item4 in list)
			{
				string item3 = item4.Item2;
				if (flag2)
				{
					name = item3;
					if (entity != null && !SerialHelper.IsMobile(serial))
					{
						entity.Name = item3;
					}
					flag2 = false;
				}
				else
				{
					if (valueStringBuilder.Length != 0)
					{
						valueStringBuilder.Append('\n');
					}
					valueStringBuilder.Append(item3);
				}
			}
			data = valueStringBuilder.ToString();
			valueStringBuilder.Dispose();
		}
		World.OPL.Add(serial, revision, name, data);
		if (flag && item != null && SerialHelper.IsValid(item.Serial))
		{
			UIManager.GetGump<ShopGump>(item.RootContainer)?.SetNameTo((Item)entity, name);
		}
	}

	private static void GenericAOSCommandsR(ref StackDataReader p)
	{
	}

	private unsafe static void ReadUnsafeCustomHouseData(ReadOnlySpan<byte> source, int sourcePosition, int dlen, int clen, int planeZ, int planeMode, short minX, short minY, short maxY, Item item, House house)
	{
		bool isMultiMovable = item.ItemData.IsMultiMovable;
		byte[] array = null;
		Span<byte> span = ((dlen > 1024) ? ((Span<byte>)(array = ArrayPool<byte>.Shared.Rent(dlen, zero: true))) : stackalloc byte[dlen]);
		Span<byte> span2 = span;
		try
		{
			fixed (byte* ptr2 = span2)
			{
				fixed (byte* ptr = &source[sourcePosition])
				{
					ZLib.Decompress((IntPtr)ptr, clen, 0, (IntPtr)ptr2, dlen);
				}
				StackDataReader stackDataReader = new StackDataReader(span2.Slice(0, dlen));
				ushort num = 0;
				sbyte b = 0;
				sbyte b2 = 0;
				sbyte b3 = 0;
				switch (planeMode)
				{
				case 0:
				{
					int num5 = dlen / 5;
					for (uint num8 = 0u; num8 < num5; num8++)
					{
						num = stackDataReader.ReadUInt16BE();
						b = stackDataReader.ReadInt8();
						b2 = stackDataReader.ReadInt8();
						b3 = stackDataReader.ReadInt8();
						if (num != 0)
						{
							house.Add(num, 0, (ushort)(item.X + b), (ushort)(item.Y + b2), (sbyte)(item.Z + b3), iscustom: true, isMultiMovable);
						}
					}
					break;
				}
				case 1:
				{
					b3 = (sbyte)((planeZ > 0) ? ((sbyte)((planeZ - 1) % 4 * 20 + 7)) : 0);
					int num5 = dlen >> 2;
					for (uint num7 = 0u; num7 < num5; num7++)
					{
						num = stackDataReader.ReadUInt16BE();
						b = stackDataReader.ReadInt8();
						b2 = stackDataReader.ReadInt8();
						if (num != 0)
						{
							house.Add(num, 0, (ushort)(item.X + b), (ushort)(item.Y + b2), (sbyte)(item.Z + b3), iscustom: true, isMultiMovable);
						}
					}
					break;
				}
				case 2:
				{
					short num2 = 0;
					short num3 = 0;
					short num4 = 0;
					b3 = (sbyte)((planeZ > 0) ? ((sbyte)((planeZ - 1) % 4 * 20 + 7)) : 0);
					if (planeZ <= 0)
					{
						num2 = minX;
						num3 = minY;
						num4 = (short)(maxY - minY + 2);
					}
					else if (planeZ <= 4)
					{
						num2 = (short)(minX + 1);
						num3 = (short)(minY + 1);
						num4 = (short)(maxY - minY);
					}
					else
					{
						num2 = minX;
						num3 = minY;
						num4 = (short)(maxY - minY + 1);
					}
					int num5 = dlen >> 1;
					for (uint num6 = 0u; num6 < num5; num6++)
					{
						num = stackDataReader.ReadUInt16BE();
						b = (sbyte)(num6 / num4 + num2);
						b2 = (sbyte)(num6 % num4 + num3);
						if (num != 0)
						{
							house.Add(num, 0, (ushort)(item.X + b), (ushort)(item.Y + b2), (sbyte)(item.Z + b3), iscustom: true, isMultiMovable);
						}
					}
					break;
				}
				}
				stackDataReader.Release();
			}
		}
		finally
		{
			if (array != null)
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}
	}

	private static void CustomHouse(ref StackDataReader p)
	{
		p.ReadUInt8();
		p.ReadBool();
		uint num = p.ReadUInt32BE();
		Item item = World.Items.Get(num);
		uint revision = p.ReadUInt32BE();
		if (item == null)
		{
			return;
		}
		Rectangle? multiInfo = item.MultiInfo;
		if (!item.IsMulti || !multiInfo.HasValue)
		{
			return;
		}
		p.Skip(4);
		if (!World.HouseManager.TryGetHouse(item, out var house))
		{
			house = new House(item, revision, isCustom: true);
			World.HouseManager.Add(item, house);
		}
		else
		{
			house.ClearComponents(removeCustomOnly: true);
			house.Revision = revision;
			house.IsCustom = true;
		}
		short num2 = (short)multiInfo.Value.X;
		short num3 = (short)multiInfo.Value.Y;
		short num4 = (short)multiInfo.Value.Height;
		if (num2 == 0 && num3 == 0 && num4 == 0 && multiInfo.Value.Width == 0)
		{
			Log.Warn("[CustomHouse (0xD8) - Invalid multi dimentions. Maybe missing some installation required files");
			return;
		}
		byte b = p.ReadUInt8();
		house.ClearCustomHouseComponents((CUSTOM_HOUSE_MULTI_OBJECT_FLAGS)0);
		for (int i = 0; i < b; i++)
		{
			uint num5 = p.ReadUInt32BE();
			int dlen = (int)(((num5 & 0xFF0000) >> 16) | ((num5 & 0xF0) << 4));
			int num6 = (int)(((num5 & 0xFF00) >> 8) | ((num5 & 0xF) << 8));
			int planeZ = (int)((num5 & 0xF000000) >> 24);
			int planeMode = (int)((num5 & 0xF0000000u) >> 28);
			if (num6 > 0)
			{
				ReadUnsafeCustomHouseData(p.Buffer, p.Position, dlen, num6, planeZ, planeMode, num2, num3, num4, item, house);
				p.Skip(num6);
			}
		}
		if (World.CustomHouseManager != null)
		{
			World.CustomHouseManager.GenerateFloorPlace();
			UIManager.GetGump<HouseCustomizationGump>(house.Serial)?.Update();
		}
		UIManager.GetGump<MiniMapGump>(null)?.RequestUpdateContents();
		if (World.HouseManager.EntityIntoHouse(num, World.Player))
		{
			Client.Game.GetScene<GameScene>()?.UpdateMaxDrawZ(force: true);
		}
		BoatMovingManager.ClearSteps(num);
	}

	private static void CharacterTransferLog(ref StackDataReader p)
	{
	}

	private static void OPLInfo(ref StackDataReader p)
	{
		if (World.ClientFeatures.TooltipsEnabled)
		{
			uint serial = p.ReadUInt32BE();
			uint revision = p.ReadUInt32BE();
			if (!World.OPL.IsRevisionEquals(serial, revision))
			{
				AddMegaClilocRequest(serial);
			}
		}
	}

	private unsafe static void OpenCompressedGump(ref StackDataReader p)
	{
		uint sender = p.ReadUInt32BE();
		uint gumpID = p.ReadUInt32BE();
		uint x = p.ReadUInt32BE();
		uint y = p.ReadUInt32BE();
		uint num = p.ReadUInt32BE() - 4;
		int num2 = (int)p.ReadUInt32BE();
		byte[] array = ArrayPool<byte>.Shared.Rent(num2, zero: true);
		string @string;
		try
		{
			fixed (byte* ptr = array)
			{
				ZLib.Decompress(p.PositionAddress, (int)num, 0, (IntPtr)ptr, num2);
				@string = Encoding.UTF8.GetString(ptr, num2);
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(array);
		}
		p.Skip((int)num);
		uint num3 = p.ReadUInt32BE();
		string[] array2 = new string[num3];
		if (num3 != 0)
		{
			num = p.ReadUInt32BE() - 4;
			num2 = (int)p.ReadUInt32BE();
			array = ArrayPool<byte>.Shared.Rent(num2, zero: true);
			try
			{
				fixed (byte* ptr2 = array)
				{
					ZLib.Decompress(p.PositionAddress, (int)num, 0, (IntPtr)ptr2, num2);
				}
				p.Skip((int)num);
				StackDataReader stackDataReader = new StackDataReader(array.AsSpan(0, num2));
				for (int i = 0; i < num3; i++)
				{
					if (stackDataReader.Remaining >= 2)
					{
						int num4 = stackDataReader.ReadUInt16BE();
						if (num4 > 0)
						{
							array2[i] = stackDataReader.ReadUnicodeBE(num4);
						}
						else
						{
							array2[i] = string.Empty;
						}
					}
					else
					{
						array2[i] = string.Empty;
					}
				}
				stackDataReader.Release();
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(array);
			}
		}
		CreateGump(sender, gumpID, (int)x, (int)y, @string, array2);
	}

	private static void UpdateMobileStatus(ref StackDataReader p)
	{
		p.ReadUInt32BE();
		if (p.ReadUInt8() == 1)
		{
			p.ReadUInt32BE();
		}
	}

	private static void NewCharacterAnimation(ref StackDataReader p)
	{
		if (!(World.Player == null))
		{
			Mobile mobile = World.Mobiles.Get(p.ReadUInt32BE());
			if (!(mobile == null))
			{
				ushort num = p.ReadUInt16BE();
				ushort action = p.ReadUInt16BE();
				byte mode = p.ReadUInt8();
				byte objectNewAnimation = Mobile.GetObjectNewAnimation(mobile, num, action, mode);
				mobile.SetAnimation(objectNewAnimation, 0, 0, 1, (num == 1 || num == 2) && mobile.Graphic == 21, forward: true, fromServer: true);
			}
		}
	}

	private static void KREncryptionResponse(ref StackDataReader p)
	{
	}

	private static void DisplayWaypoint(ref StackDataReader p)
	{
		p.ReadUInt32BE();
		p.ReadUInt16BE();
		p.ReadUInt16BE();
		p.ReadInt8();
		p.ReadUInt8();
		p.ReadUInt16BE();
		p.ReadUInt16BE();
		p.ReadUInt32BE();
		p.ReadUnicodeLE();
	}

	private static void RemoveWaypoint(ref StackDataReader p)
	{
		p.ReadUInt32BE();
	}

	private static void KrriosClientSpecial(ref StackDataReader p)
	{
		byte b = p.ReadUInt8();
		int num;
		bool flag;
		uint serial;
		switch (b)
		{
		case 0:
			Log.Trace("Krrios special packet accepted");
			World.WMapManager.SetACKReceived();
			World.WMapManager.SetEnable(v: true);
			break;
		case 2:
			num = (p.ReadBool() ? 1 : 0);
			goto IL_0065;
		case 1:
			num = 1;
			goto IL_0065;
		case 254:
			{
				Log.Info("Razor ACK sent");
				NetClient.Socket.Send_RazorACK();
				break;
			}
			IL_0065:
			flag = (byte)num != 0;
			while ((serial = p.ReadUInt32BE()) != 0)
			{
				if (flag)
				{
					ushort x = p.ReadUInt16BE();
					ushort y = p.ReadUInt16BE();
					byte map = p.ReadUInt8();
					int hp = ((b != 1) ? p.ReadUInt8() : 0);
					World.WMapManager.AddOrUpdate(serial, x, y, hp, map, b == 2, null, from_packet: true);
				}
			}
			World.WMapManager.RemoveUnupdatedWEntity();
			break;
		}
	}

	private static void FreeshardListR(ref StackDataReader p)
	{
	}

	private static void UpdateItemSA(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		p.Skip(2);
		byte type = p.ReadUInt8();
		uint num = p.ReadUInt32BE();
		ushort num2 = p.ReadUInt16BE();
		byte b = p.ReadUInt8();
		ushort count = p.ReadUInt16BE();
		ushort uNK = p.ReadUInt16BE();
		ushort x = p.ReadUInt16BE();
		ushort y = p.ReadUInt16BE();
		sbyte z = p.ReadInt8();
		Direction direction = (Direction)p.ReadUInt8();
		ushort hue = p.ReadUInt16BE();
		Flags flags = (Flags)p.ReadUInt8();
		ushort isPlayerItem = p.ReadUInt16BE();
		if (num != (uint)World.Player)
		{
			UpdateGameObject(num, num2, b, count, x, y, z, direction, hue, flags, uNK, type, isPlayerItem);
			if (num2 == 8198 && ProfileManager.CurrentProfile.AutoOpenCorpses)
			{
				World.Player.TryOpenCorpses();
			}
		}
		else if (p[0] == 247)
		{
			UpdatePlayer(num, num2, b, hue, flags, x, y, z, 0, direction);
		}
	}

	private static void BoatMoving(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			return;
		}
		uint serial = p.ReadUInt32BE();
		byte speed = p.ReadUInt8();
		Direction movingDir = (Direction)(p.ReadUInt8() & 7);
		Direction facingDir = (Direction)(p.ReadUInt8() & 7);
		ushort num = p.ReadUInt16BE();
		ushort num2 = p.ReadUInt16BE();
		ushort num3 = p.ReadUInt16BE();
		Item item = World.Items.Get(serial);
		if (item == null)
		{
			return;
		}
		bool flag = ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.UseSmoothBoatMovement;
		flag = true;
		if (flag)
		{
			BoatMovingManager.AddStep(serial, speed, movingDir, facingDir, num, num2, (sbyte)num3);
		}
		else
		{
			item.X = num;
			item.Y = num2;
			item.Z = (sbyte)num3;
			item.AddToTile();
			item.UpdateScreenPosition();
			if (World.HouseManager.TryGetHouse(serial, out var house))
			{
				house.Generate(recalculate: true, pushtotile: true, removePreview: true);
			}
		}
		int num4 = p.ReadUInt16BE();
		for (int i = 0; i < num4; i++)
		{
			uint num5 = p.ReadUInt32BE();
			ushort num6 = p.ReadUInt16BE();
			ushort num7 = p.ReadUInt16BE();
			ushort num8 = p.ReadUInt16BE();
			if (num5 == (uint)World.Player)
			{
				World.RangeSize.X = num6;
				World.RangeSize.Y = num7;
			}
			Entity entity = World.Get(num5);
			if (!(entity == null))
			{
				if (flag)
				{
					BoatMovingManager.PushItemToList(serial, num5, num - num6, num2 - num7, (sbyte)(num3 - num8));
				}
				else if (num5 == (uint)World.Player)
				{
					UpdatePlayer(num5, entity.Graphic, 0, entity.Hue, entity.Flags, num6, num7, (sbyte)num8, 0, World.Player.Direction);
				}
				else
				{
					UpdateGameObject(num5, entity.Graphic, 0, (ushort)((entity.Graphic == 8198) ? ((Item)entity).Amount : 0), num6, num7, (sbyte)num8, SerialHelper.IsMobile(entity) ? entity.Direction : Direction.North, entity.Hue, entity.Flags, 0, 0, 1);
				}
			}
		}
	}

	private static void PacketList(ref StackDataReader p)
	{
		if (World.Player == null)
		{
			return;
		}
		int num = p.ReadUInt16BE();
		for (int i = 0; i < num; i++)
		{
			byte b = p.ReadUInt8();
			if (b == 243)
			{
				UpdateItemSA(ref p);
				continue;
			}
			Log.Warn($"Unknown packet ID: [0x{b:X2}] in 0xF7");
			break;
		}
	}

	private static void ServerListReceived(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			Client.Game.GetScene<LoginScene>()?.ServerListReceived(ref p);
		}
	}

	private static void ReceiveServerRelay(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			Client.Game.GetScene<LoginScene>()?.HandleRelayServerPacket(ref p);
		}
	}

	private static void UpdateCharacterList(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			Client.Game.GetScene<LoginScene>()?.UpdateCharacterList(ref p);
		}
	}

	private static void ReceiveCharacterList(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			Client.Game.GetScene<LoginScene>()?.ReceiveCharacterList(ref p);
		}
	}

	private static void ReceiveLoginRejection(ref StackDataReader p)
	{
		if (!World.InGame)
		{
			Client.Game.GetScene<LoginScene>()?.HandleErrorCode(ref p);
		}
	}

	private static void AddItemToContainer(uint serial, ushort graphic, ushort amount, ushort x, ushort y, ushort hue, uint containerSerial)
	{
		if (ItemHold.Serial == serial && ItemHold.Dropped)
		{
			Console.WriteLine("ADD ITEM TO CONTAINER -- CLEAR HOLD");
			ItemHold.Clear();
		}
		Entity entity = World.Get(containerSerial);
		if (entity == null)
		{
			Log.Warn($"No container ({containerSerial}) found");
			return;
		}
		Item item = World.Items.Get(serial);
		if (SerialHelper.IsMobile(serial))
		{
			World.RemoveMobile(serial, forceRemove: true);
			Log.Warn("AddItemToContainer function adds mobile as Item");
		}
		if (item != null && (entity.Graphic != 8198 || item.Layer == Layer.Invalid))
		{
			World.RemoveItem(item, forceRemove: true);
		}
		item = World.GetOrCreateItem(serial);
		item.Graphic = graphic;
		item.CheckGraphicChange(0);
		item.Amount = amount;
		item.FixHue(hue);
		item.X = x;
		item.Y = y;
		item.Z = 0;
		World.RemoveItemFromContainer(item);
		item.Container = containerSerial;
		entity.PushToBack(item);
		if (SerialHelper.IsMobile(containerSerial))
		{
			Item item2 = World.Mobiles.Get(containerSerial)?.GetSecureTradeBox();
			if (item2 != null)
			{
				UIManager.GetTradingGump(item2)?.RequestUpdateContents();
			}
			else
			{
				UIManager.GetGump<PaperDollGump>(containerSerial)?.RequestUpdateContents();
			}
		}
		else if (SerialHelper.IsItem(containerSerial))
		{
			Gump gump = UIManager.GetGump<BulletinBoardGump>(containerSerial);
			if (gump != null)
			{
				NetClient.Socket.Send_BulletinBoardRequestMessageSummary(containerSerial, serial);
			}
			else
			{
				gump = UIManager.GetGump<SpellbookGump>(containerSerial);
				if (gump == null)
				{
					gump = UIManager.GetGump<ContainerGump>(containerSerial);
					if (gump != null)
					{
						((ContainerGump)gump).CheckItemControlPosition(item);
					}
					if (ProfileManager.CurrentProfile.GridLootType > 0)
					{
						GridLootGump gridLootGump = UIManager.GetGump<GridLootGump>(containerSerial);
						if (gridLootGump == null && SerialHelper.IsValid(_requestedGridLoot) && _requestedGridLoot == containerSerial)
						{
							gridLootGump = new GridLootGump(_requestedGridLoot);
							UIManager.Add(gridLootGump);
							_requestedGridLoot = 0u;
						}
						gridLootGump?.RequestUpdateContents();
					}
				}
				if (gump != null)
				{
					if (SerialHelper.IsItem(containerSerial))
					{
						((Item)entity).Opened = true;
					}
					gump.RequestUpdateContents();
				}
			}
		}
		UIManager.GetTradingGump(containerSerial)?.RequestUpdateContents();
	}

	private static void UpdateGameObject(uint serial, ushort graphic, byte graphic_inc, ushort count, ushort x, ushort y, sbyte z, Direction direction, ushort hue, Flags flagss, int UNK, byte type, ushort isPlayerItem)
	{
		Mobile mobile = null;
		Item item = null;
		Entity entity = World.Get(serial);
		if (ItemHold.Enabled && ItemHold.Serial == serial)
		{
			if (SerialHelper.IsValid(ItemHold.Container))
			{
				if (ItemHold.Layer == Layer.Invalid)
				{
					UIManager.GetGump<ContainerGump>(ItemHold.Container)?.RequestUpdateContents();
				}
				else
				{
					UIManager.GetGump<PaperDollGump>(ItemHold.Container)?.RequestUpdateContents();
				}
			}
			ItemHold.UpdatedInWorld = true;
		}
		bool flag = false;
		if (entity == null || entity.IsDestroyed)
		{
			flag = true;
			if (SerialHelper.IsMobile(serial) && type != 3)
			{
				mobile = World.GetOrCreateMobile(serial);
				if (mobile == null)
				{
					return;
				}
				entity = mobile;
				mobile.Graphic = (ushort)(graphic + graphic_inc);
				mobile.CheckGraphicChange(0);
				mobile.Direction = direction & Direction.Up;
				mobile.FixHue(hue);
				mobile.X = x;
				mobile.Y = y;
				mobile.Z = z;
				mobile.Flags = flagss;
			}
			else
			{
				item = World.GetOrCreateItem(serial);
				if (item == null)
				{
					return;
				}
				entity = item;
			}
		}
		else if (entity is Item item2)
		{
			item = item2;
			if (SerialHelper.IsValid(item.Container))
			{
				World.RemoveItemFromContainer(item);
			}
		}
		else
		{
			mobile = (Mobile)entity;
		}
		if (entity == null)
		{
			return;
		}
		if (item != null)
		{
			if (graphic != 8198)
			{
				graphic += graphic_inc;
			}
			if (type == 2)
			{
				item.IsMulti = true;
				item.WantUpdateMulti = (graphic & 0x3FFF) != item.Graphic || item.X != x || item.Y != y || item.Z != z;
				item.Graphic = (ushort)(graphic & 0x3FFF);
			}
			else
			{
				item.IsDamageable = type == 3;
				item.IsMulti = false;
				item.Graphic = graphic;
			}
			item.X = x;
			item.Y = y;
			item.Z = z;
			item.LightID = (byte)direction;
			if (graphic == 8198)
			{
				item.Layer = (Layer)direction;
			}
			item.FixHue(hue);
			if (count == 0)
			{
				count = 1;
			}
			item.Amount = count;
			item.Flags = flagss;
			item.Direction = direction;
			item.CheckGraphicChange(item.AnimIndex);
			item.IsPlayerItem = isPlayerItem;
		}
		else
		{
			graphic += graphic_inc;
			if (serial != (uint)World.Player)
			{
				Direction direction2 = direction & Direction.Up;
				bool flag2 = (direction & Direction.Running) != 0;
				if (World.Get(mobile) == null || (mobile.X == ushort.MaxValue && mobile.Y == ushort.MaxValue))
				{
					mobile.X = x;
					mobile.Y = y;
					mobile.Z = z;
					mobile.Direction = direction2;
					mobile.IsRunning = flag2;
					mobile.ClearSteps();
				}
				if (!mobile.EnqueueStep(x, y, z, direction2, flag2))
				{
					mobile.X = x;
					mobile.Y = y;
					mobile.Z = z;
					mobile.Direction = direction2;
					mobile.IsRunning = flag2;
					mobile.ClearSteps();
				}
			}
			mobile.Graphic = (ushort)(graphic & 0x3FFF);
			mobile.FixHue(hue);
			mobile.Flags = flagss;
		}
		if (flag && !entity.IsClicked)
		{
			if (mobile != null)
			{
				GameActions.SingleClick(serial);
			}
			else if (graphic == 8198)
			{
				GameActions.SingleClick(serial);
			}
		}
		if (mobile != null)
		{
			mobile.AddToTile();
			mobile.UpdateScreenPosition();
			if (flag)
			{
				GameActions.RequestMobileStatus(serial);
			}
			return;
		}
		if (ItemHold.Serial == serial && ItemHold.Dropped)
		{
			ItemHold.Enabled = false;
			ItemHold.Dropped = false;
		}
		if (item.OnGround)
		{
			item.AddToTile();
			item.UpdateScreenPosition();
			if (graphic == 8198 && ProfileManager.CurrentProfile.AutoOpenCorpses)
			{
				World.Player.TryOpenCorpses();
			}
		}
	}

	private static void UpdatePlayer(uint serial, ushort graphic, byte graph_inc, ushort hue, Flags flags, ushort x, ushort y, sbyte z, ushort serverID, Direction direction)
	{
		if (serial == (uint)World.Player)
		{
			World.Player.CloseBank();
			World.Player.Walker.WalkingFailed = false;
			World.Player.X = x;
			World.Player.Y = y;
			World.Player.Z = z;
			World.RangeSize.X = x;
			World.RangeSize.Y = y;
			_ = World.Player.IsDead;
			ushort graphic2 = World.Player.Graphic;
			World.Player.Graphic = graphic;
			World.Player.Direction = direction & Direction.Up;
			World.Player.FixHue(hue);
			World.Player.Flags = flags;
			World.Player.Walker.DenyWalk(byte.MaxValue, -1, -1, -1);
			GameScene scene = Client.Game.GetScene<GameScene>();
			if (scene != null)
			{
				scene.Weather.Reset();
				scene.UpdateDrawPosition = true;
			}
			if (graphic2 != 0 && graphic2 != World.Player.Graphic && World.Player.IsDead)
			{
				TargetManager.Reset();
			}
			World.Player.Walker.ResendPacketResync = false;
			World.Player.CloseRangedGumps();
			World.Player.UpdateScreenPosition();
			World.Player.AddToTile();
			World.Player.UpdateAbilities();
		}
	}

	private static void ClearContainerAndRemoveItems(Entity container, bool remove_unequipped = false)
	{
		if (container == null || container.IsEmpty)
		{
			return;
		}
		LinkedObject linkedObject = container.Items;
		LinkedObject linkedObject2 = null;
		while (linkedObject != null)
		{
			LinkedObject next = linkedObject.Next;
			Item item = (Item)linkedObject;
			if (remove_unequipped && item.Layer != 0)
			{
				if (linkedObject2 == null)
				{
					linkedObject2 = linkedObject;
				}
			}
			else
			{
				World.RemoveItem(item, forceRemove: true);
			}
			linkedObject = next;
		}
		container.Items = (remove_unequipped ? linkedObject2 : null);
	}

	private static Gump CreateGump(uint sender, uint gumpID, int x, int y, string layout, string[] lines)
	{
		List<string> tokens = _parser.GetTokens(layout);
		int count = tokens.Count;
		if (count <= 0)
		{
			return null;
		}
		Gump gump = null;
		bool flag = true;
		if (UIManager.GetGumpCachePosition(gumpID, out var pos))
		{
			x = pos.X;
			y = pos.Y;
			for (LinkedListNode<Gump> linkedListNode = UIManager.Gumps.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
			{
				Control value = linkedListNode.Value;
				if (!value.IsDisposed && value.LocalSerial == sender && value.ServerSerial == gumpID && gumpID != 1440 && gumpID != 1258)
				{
					value.Clear();
					gump = value as Gump;
					flag = false;
					break;
				}
			}
		}
		else
		{
			UIManager.SavePosition(gumpID, new Point(x, y));
		}
		if (gump == null)
		{
			Gump gump2 = new Gump(sender, gumpID);
			gump2.X = x;
			gump2.Y = y;
			gump2.CanMove = true;
			gump2.CanCloseWithRightClick = true;
			gump2.CanCloseWithEsc = true;
			gump2.InvalidateContents = false;
			gump2.IsFromServer = true;
			gump = gump2;
		}
		int num = 0;
		int num2 = 0;
		bool flag2 = false;
		for (int i = 0; i < count; i++)
		{
			List<string> tokens2 = _cmdparser.GetTokens(tokens[i], trim: false);
			if (tokens2.Count == 0)
			{
				continue;
			}
			string a = tokens2[0];
			if (string.Equals(a, "button", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new Button(tokens2), num2);
			}
			else if (string.Equals(a, "buttontileart", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new ButtonTileArt(tokens2), num2);
			}
			else if (string.Equals(a, "checkertrans", StringComparison.InvariantCultureIgnoreCase))
			{
				CheckerTrans checkerTrans = new CheckerTrans(tokens2);
				gump.Add(checkerTrans, num2);
				ApplyTrans(gump, num2, checkerTrans.X, checkerTrans.Y, checkerTrans.Width, checkerTrans.Height);
			}
			else if (string.Equals(a, "croppedtext", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new CroppedText(tokens2, lines), num2);
			}
			else if (string.Equals(a, "gumppic", StringComparison.InvariantCultureIgnoreCase))
			{
				GumpPic gumpPic = new GumpPic(tokens2);
				if (tokens2.Count >= 6 && tokens2[5].IndexOf("virtuegumpitem", StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					gumpPic.ContainsByBounds = true;
					gumpPic.IsVirtue = true;
					string text;
					switch (gumpPic.Hue)
					{
					case 2403:
						text = "";
						break;
					case 18:
					case 235:
					case 1154:
					case 1348:
					case 1547:
					case 2210:
					case 2213:
						text = "Seeker of ";
						break;
					case 618:
					case 1352:
					case 1552:
					case 2118:
					case 2212:
					case 2216:
					case 2302:
					case 2404:
						text = "Follower of ";
						break;
					case 33:
					case 43:
					case 53:
					case 67:
					case 98:
					case 318:
					case 1153:
						text = "Knight of ";
						break;
					case 2406:
						text = ((gumpPic.Graphic != 111) ? "Knight of " : "Seeker of ");
						break;
					default:
						text = "";
						break;
					}
					string text2 = gumpPic.Graphic switch
					{
						105 => ClilocLoader.Instance.GetString(1051002), 
						106 => ClilocLoader.Instance.GetString(1051007), 
						107 => ClilocLoader.Instance.GetString(1051005), 
						109 => ClilocLoader.Instance.GetString(1051006), 
						110 => ClilocLoader.Instance.GetString(1051001), 
						111 => ClilocLoader.Instance.GetString(1051003), 
						112 => ClilocLoader.Instance.GetString(1051004), 
						_ => ClilocLoader.Instance.GetString(1051000), 
					};
					if (string.IsNullOrEmpty(text2))
					{
						text2 = "Unknown virtue";
					}
					gumpPic.SetTooltip(text + text2, 100);
				}
				gump.Add(gumpPic, num2);
			}
			else if (string.Equals(a, "gumppictiled", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new GumpPicTiled(tokens2), num2);
			}
			else if (string.Equals(a, "htmlgump", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new HtmlControl(tokens2, lines), num2);
			}
			else if (string.Equals(a, "xmfhtmlgump", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new HtmlControl(int.Parse(tokens2[1]), int.Parse(tokens2[2]), int.Parse(tokens2[3]), int.Parse(tokens2[4]), int.Parse(tokens2[6]) == 1, int.Parse(tokens2[7]) != 0, tokens2[6] != "0" && tokens2[7] == "2", ClilocLoader.Instance.GetString(int.Parse(tokens2[5].Replace("#", ""))), 0, ishtml: true, 1)
				{
					IsFromServer = true
				}, num2);
			}
			else if (string.Equals(a, "xmfhtmlgumpcolor", StringComparison.InvariantCultureIgnoreCase))
			{
				int num3 = int.Parse(tokens2[8]);
				if (num3 == 32767)
				{
					num3 = 16777215;
				}
				gump.Add(new HtmlControl(int.Parse(tokens2[1]), int.Parse(tokens2[2]), int.Parse(tokens2[3]), int.Parse(tokens2[4]), int.Parse(tokens2[6]) == 1, int.Parse(tokens2[7]) != 0, tokens2[6] != "0" && tokens2[7] == "2", ClilocLoader.Instance.GetString(int.Parse(tokens2[5].Replace("#", ""))), num3, ishtml: true, 1)
				{
					IsFromServer = true
				}, num2);
			}
			else if (string.Equals(a, "xmfhtmltok", StringComparison.InvariantCultureIgnoreCase))
			{
				int num4 = int.Parse(tokens2[7]);
				if (num4 == 32767)
				{
					num4 = 16777215;
				}
				StringBuilder stringBuilder = null;
				if (tokens2.Count >= 9)
				{
					stringBuilder = new StringBuilder();
					for (int j = 9; j < tokens2.Count; j++)
					{
						stringBuilder.Append('\t');
						stringBuilder.Append(tokens2[j]);
					}
				}
				gump.Add(new HtmlControl(int.Parse(tokens2[1]), int.Parse(tokens2[2]), int.Parse(tokens2[3]), int.Parse(tokens2[4]), int.Parse(tokens2[5]) == 1, int.Parse(tokens2[6]) != 0, tokens2[5] != "0" && tokens2[6] == "2", (stringBuilder == null) ? ClilocLoader.Instance.GetString(int.Parse(tokens2[8].Replace("#", ""))) : ClilocLoader.Instance.Translate(int.Parse(tokens2[8].Replace("#", "")), stringBuilder.ToString().Trim('@').Replace('@', '\t')), num4, ishtml: true, 1)
				{
					IsFromServer = true
				}, num2);
			}
			else if (string.Equals(a, "page", StringComparison.InvariantCultureIgnoreCase))
			{
				if (tokens2.Count >= 2)
				{
					num2 = int.Parse(tokens2[1]);
				}
			}
			else if (string.Equals(a, "resizepic", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new ResizePic(tokens2), num2);
			}
			else if (string.Equals(a, "text", StringComparison.InvariantCultureIgnoreCase))
			{
				if (tokens2.Count >= 5)
				{
					gump.Add(new Label(tokens2, lines), num2);
				}
			}
			else if (string.Equals(a, "textentrylimited", StringComparison.InvariantCultureIgnoreCase) || string.Equals(a, "textentry", StringComparison.InvariantCultureIgnoreCase))
			{
				StbTextBox stbTextBox = new StbTextBox(tokens2, lines);
				if (!flag2)
				{
					stbTextBox.SetKeyboardFocus();
					flag2 = true;
				}
				gump.Add(stbTextBox, num2);
			}
			else if (string.Equals(a, "tilepichue", StringComparison.InvariantCultureIgnoreCase) || string.Equals(a, "tilepic", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new StaticPic(tokens2), num2);
			}
			else if (string.Equals(a, "noclose", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.CanCloseWithRightClick = false;
			}
			else if (string.Equals(a, "nodispose", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.CanCloseWithEsc = false;
			}
			else if (string.Equals(a, "nomove", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.BlockMovement = true;
			}
			else if (string.Equals(a, "group", StringComparison.InvariantCultureIgnoreCase) || string.Equals(a, "endgroup", StringComparison.InvariantCultureIgnoreCase))
			{
				num++;
			}
			else if (string.Equals(a, "radio", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new RadioButton(num, tokens2, lines), num2);
			}
			else if (string.Equals(a, "checkbox", StringComparison.InvariantCultureIgnoreCase))
			{
				gump.Add(new Checkbox(tokens2, lines), num2);
			}
			else if (string.Equals(a, "tooltip", StringComparison.InvariantCultureIgnoreCase))
			{
				string text3 = null;
				if (tokens2.Count > 2 && tokens2[2].Length != 0)
				{
					string text4 = tokens2[2];
					for (int k = 3; k < tokens2.Count; k++)
					{
						text4 = text4 + "\t" + tokens2[k];
					}
					if (text4.Length == 0)
					{
						text3 = ClilocLoader.Instance.GetString(int.Parse(tokens2[1]));
						Log.Error("String '" + text4 + "' too short, something wrong with gump tooltip: " + text3);
					}
					else
					{
						text3 = ClilocLoader.Instance.Translate(int.Parse(tokens2[1]), text4);
					}
				}
				else
				{
					text3 = ClilocLoader.Instance.GetString(int.Parse(tokens2[1]));
				}
				Control control = ((gump.Children.Count != 0) ? gump.Children[gump.Children.Count - 1] : null);
				if (control == null)
				{
					continue;
				}
				if (control.HasTooltip)
				{
					if (control.Tooltip is string text5)
					{
						string text6 = text5 + "\n" + text3;
						control.SetTooltip(text6);
					}
				}
				else
				{
					control.SetTooltip(text3);
				}
				control.Priority = ClickPriority.High;
				control.AcceptMouseInput = true;
			}
			else if (string.Equals(a, "itemproperty", StringComparison.InvariantCultureIgnoreCase))
			{
				if (World.ClientFeatures.TooltipsEnabled && gump.Children.Count != 0)
				{
					gump.Children[gump.Children.Count - 1].SetTooltip(SerialHelper.Parse(tokens2[1]));
					if (uint.TryParse(tokens2[1], out var result) && (!World.OPL.TryGetRevision(result, out var revision) || revision == 0))
					{
						AddMegaClilocRequest(result);
					}
				}
			}
			else if (!string.Equals(a, "noresize", StringComparison.InvariantCultureIgnoreCase))
			{
				if (string.Equals(a, "mastergump", StringComparison.InvariantCultureIgnoreCase))
				{
					gump.MasterGumpSerial = ((tokens2.Count > 0) ? SerialHelper.Parse(tokens2[1]) : 0u);
				}
				else
				{
					Log.Warn(tokens2[0]);
				}
			}
		}
		if (flag)
		{
			UIManager.Add(gump);
		}
		gump.Update(Time.Ticks, 0.0);
		gump.SetInScreen();
		return gump;
	}

	private static void ApplyTrans(Gump gump, int current_page, int x, int y, int width, int height)
	{
		int num = x + width;
		int num2 = y + height;
		for (int i = 0; i < gump.Children.Count; i++)
		{
			Control control = gump.Children[i];
			bool num3 = control.Page == 0 || current_page == control.Page;
			bool flag = x < control.X + control.Width && control.X < num && y < control.Y + control.Height && control.Y < num2;
			if (num3 && control.IsVisible && flag)
			{
				control.Alpha = 0.5f;
			}
		}
	}
}
