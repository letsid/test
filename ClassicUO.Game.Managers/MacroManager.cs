using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using SDL2;

namespace ClassicUO.Game.Managers;

internal class MacroManager : LinkedObject
{
	public static readonly string[] MacroNames = Enum.GetNames(typeof(MacroType));

	private readonly uint[] _itemsInHand = new uint[2];

	private MacroObject _lastMacro;

	private long _nextTimer;

	private readonly byte[] _skillTable = new byte[28]
	{
		1, 14, 45, 48, 42, 31, 36, 17, 44, 13,
		4, 46, 16, 29, 22, 5, 47, 24, 40, 38,
		2, 19, 43, 30, 21, 10, 41, 35
	};

	private readonly int[] _spellsCountTable = new int[7] { 64, 17, 10, 6, 8, 16, 30 };

	public long WaitForTargetTimer { get; set; }

	public bool WaitingBandageTarget { get; set; }

	public void Load()
	{
		string text = Path.Combine(ProfileManager.ProfilePath, "macros.xml");
		if (!File.Exists(text))
		{
			Log.Trace("No macros.xml file. Creating a default file.");
			Clear();
			CreateDefaultMacros();
			Save();
			return;
		}
		string text2 = text + ".notfallmacrodatei";
		if (!File.Exists(text2))
		{
			File.Copy(text, text2);
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(text);
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			return;
		}
		Clear();
		XmlElement xmlElement = xmlDocument["macros"];
		if (xmlElement == null)
		{
			return;
		}
		foreach (XmlElement item in xmlElement.GetElementsByTagName("macro"))
		{
			Macro macro = new Macro(item.GetAttribute("name"));
			macro.Load(item);
			PushToBack(macro);
		}
	}

	public void Save()
	{
		List<Macro> allMacros = GetAllMacros();
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(ProfileManager.ProfilePath, "macros.xml"), Encoding.UTF8)
		{
			Formatting = Formatting.Indented,
			IndentChar = '\t',
			Indentation = 1
		};
		xmlTextWriter.WriteStartDocument(standalone: true);
		xmlTextWriter.WriteStartElement("macros");
		foreach (Macro item in allMacros)
		{
			item.Save(xmlTextWriter);
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
	}

	private void CreateDefaultMacros()
	{
		PushToBack(new Macro(ResGeneral.Paperdoll, SDL.SDL_Keycode.SDLK_p, alt: true, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.Open, MacroSubType.Paperdoll)
			{
				SubMenuType = 1
			}
		});
		PushToBack(new Macro(ResGeneral.Options, SDL.SDL_Keycode.SDLK_o, alt: true, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.Open, MacroSubType.Configuration)
			{
				SubMenuType = 1
			}
		});
		PushToBack(new Macro(ResGeneral.Journal, SDL.SDL_Keycode.SDLK_j, alt: true, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.Open, MacroSubType.Journal)
			{
				SubMenuType = 1
			}
		});
		PushToBack(new Macro(ResGeneral.Backpack, SDL.SDL_Keycode.SDLK_i, alt: true, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.Open, MacroSubType.Backpack)
			{
				SubMenuType = 1
			}
		});
		PushToBack(new Macro(ResGeneral.Radar, SDL.SDL_Keycode.SDLK_r, alt: true, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.Open, MacroSubType.Overview)
			{
				SubMenuType = 1
			}
		});
		PushToBack(new Macro(ResGeneral.Bow, SDL.SDL_Keycode.SDLK_b, alt: false, ctrl: true, shift: false)
		{
			Items = new MacroObject(MacroType.Bow, MacroSubType.MSC_NONE)
			{
				SubMenuType = 0
			}
		});
		PushToBack(new Macro(ResGeneral.Salute, SDL.SDL_Keycode.SDLK_s, alt: false, ctrl: true, shift: false)
		{
			Items = new MacroObject(MacroType.Salute, MacroSubType.MSC_NONE)
			{
				SubMenuType = 0
			}
		});
		PushToBack(new Macro("LastObject", SDL.SDL_Keycode.SDLK_F1, alt: false, ctrl: false, shift: false)
		{
			Items = new MacroObject(MacroType.LastObject, MacroSubType.MSC_NONE)
			{
				SubMenuType = 0
			}
		});
	}

	public List<Macro> GetAllMacros()
	{
		Macro macro = (Macro)Items;
		while (macro?.Previous != null)
		{
			macro = (Macro)macro.Previous;
		}
		List<Macro> list = new List<Macro>();
		while (macro != null)
		{
			list.Add(macro);
			macro = (Macro)macro.Next;
		}
		return list;
	}

	public Macro FindMacro(SDL.SDL_Keycode key, bool alt, bool ctrl, bool shift)
	{
		Macro macro = (Macro)Items;
		while (macro != null && (macro.Key != key || macro.Alt != alt || macro.Ctrl != ctrl || macro.Shift != shift))
		{
			macro = (Macro)macro.Next;
		}
		return macro;
	}

	public Macro FindMacro(string name)
	{
		Macro macro = (Macro)Items;
		while (macro != null && !(macro.Name == name))
		{
			macro = (Macro)macro.Next;
		}
		return macro;
	}

	public void SetMacroToExecute(MacroObject macro)
	{
		_lastMacro = macro;
	}

	public void Update()
	{
		while (_lastMacro != null)
		{
			switch (Process())
			{
			case 2:
				_lastMacro = null;
				break;
			case 1:
				return;
			case 0:
				_lastMacro = (MacroObject)(_lastMacro?.Next);
				break;
			}
		}
	}

	private int Process()
	{
		if (_lastMacro == null)
		{
			return 2;
		}
		if (_nextTimer <= Time.Ticks)
		{
			return Process(_lastMacro);
		}
		return 1;
	}

	private int Process(MacroObject macro)
	{
		if (macro == null)
		{
			return 0;
		}
		int result = 0;
		switch (macro.Code)
		{
		case MacroType.Say:
		case MacroType.Emote:
		case MacroType.Whisper:
		case MacroType.Yell:
		{
			string text3 = ((MacroObjectString)macro).Text;
			if (!string.IsNullOrEmpty(text3))
			{
				MessageType type = MessageType.Regular;
				ushort hue = ProfileManager.CurrentProfile.SpeechHue;
				switch (macro.Code)
				{
				case MacroType.Say:
					type = MessageType.Guild;
					break;
				case MacroType.Emote:
					type = MessageType.Alliance;
					hue = ProfileManager.CurrentProfile.EmoteHue;
					break;
				case MacroType.Whisper:
					type = MessageType.Whisper;
					hue = ProfileManager.CurrentProfile.WhisperHue;
					break;
				case MacroType.Yell:
					type = MessageType.Yell;
					break;
				}
				GameActions.Say(text3, hue, type, 3);
			}
			break;
		}
		case MacroType.Usetype:
		{
			string text = ((MacroObjectString)macro).Text;
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			string[] array = text.Split(' ');
			if (array.Length != 2)
			{
				break;
			}
			ushort num = Convert.ToUInt16(array[0], 16);
			ushort num2 = Convert.ToUInt16(array[1], 16);
			if (num == 0)
			{
				break;
			}
			Item item4 = World.Player.FindItemByLayer(Layer.Backpack);
			Item item5 = ((num2 == 0) ? item4.FindItem(num) : item4.FindItem(num, num2));
			if (item5 != null)
			{
				Layer layer = (Layer)item5.ItemData.Layer;
				if (layer == Layer.OneHanded && item5.ItemData.IsWeapon)
				{
					TargetManager.CancelTarget();
					NetClient.Socket.Send_PickUpRequest(item5, item5.Amount);
					NetClient.Socket.Send_EquipRequest(item5.Serial, layer, item4.Container);
				}
				else
				{
					GameActions.DoubleClick(item5.Serial);
				}
			}
			break;
		}
		case MacroType.Usename:
		{
			string text8 = ((MacroObjectString)macro).Text;
			if (!string.IsNullOrEmpty(text8))
			{
				Item item10 = World.Player.FindItemByLayer(Layer.Backpack);
				Item item11 = item10.FindItem(text8);
				if (item11 != null)
				{
					EquipItem(item10, item11);
				}
			}
			break;
		}
		case MacroType.Useitem:
		{
			string text4 = ((MacroObjectString)macro).Text;
			if (string.IsNullOrEmpty(text4))
			{
				break;
			}
			Item item7 = World.Player.FindItemByLayer(Layer.Backpack);
			switch (macro.SubCode)
			{
			case MacroSubType.ID:
			{
				string[] array2 = text4.Split(' ');
				if (array2.Length == 2 && ushort.TryParse(array2[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result4) && ushort.TryParse(array2[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result5) && result4 != 0)
				{
					Item item8 = ((result5 == 0) ? item7.FindItem(result4) : item7.FindItem(result4, result5));
					if (item8 != null)
					{
						EquipItem(item7, item8);
					}
				}
				break;
			}
			case MacroSubType.Name:
			{
				Item item8 = item7.FindItem(text4);
				if (item8 != null)
				{
					EquipItem(item7, item8);
				}
				break;
			}
			case MacroSubType.NameExakt:
			{
				Item item8 = item7.FindItemExactName(text4);
				if (item8 != null)
				{
					EquipItem(item7, item8);
				}
				break;
			}
			}
			break;
		}
		case MacroType.PartySay:
			if (Client.Game.Scene is GameScene)
			{
				bool isActive = UIManager.SystemChat.IsActive;
				ChatMode mode = UIManager.SystemChat.Mode;
				string text6 = UIManager.SystemChat.TextBoxControl.Text;
				UIManager.SystemChat.Mode = ChatMode.Party;
				UIManager.SystemChat.IsActive = true;
				string text7 = ((MacroObjectString)macro).Text;
				if (!string.IsNullOrEmpty(text7))
				{
					UIManager.SystemChat.OnKeyboardReturn(0, text7);
				}
				UIManager.SystemChat.IsActive = isActive;
				UIManager.SystemChat.Mode = mode;
				UIManager.SystemChat.TextBoxControl.Text = text6;
			}
			break;
		case MacroType.Walk:
		{
			byte b = 7;
			if (macro.SubCode != MacroSubType.NW)
			{
				b = (byte)(macro.SubCode - 2);
				if (b > 7)
				{
					b = 0;
				}
			}
			if (!Pathfinder.AutoWalking)
			{
				World.Player.Walk((Direction)b, run: false);
			}
			break;
		}
		case MacroType.WarPeace:
			GameActions.ToggleWarMode();
			break;
		case MacroType.Paste:
		{
			string clipboardText = StringHelper.GetClipboardText(multiline: true);
			if (clipboardText != null)
			{
				UIManager.SystemChat.TextBoxControl.AppendText(clipboardText);
			}
			break;
		}
		case MacroType.ZusatzjournalAnAus:
			GameActions.ToggleAdditionalJournal();
			break;
		case MacroType.Open:
		case MacroType.Close:
		case MacroType.Minimize:
		case MacroType.Maximize:
			switch (macro.Code)
			{
			case MacroType.Open:
				switch (macro.SubCode)
				{
				case MacroSubType.Configuration:
					GameActions.OpenSettings();
					break;
				case MacroSubType.Paperdoll:
					GameActions.OpenPaperdoll(World.Player);
					break;
				case MacroSubType.Status:
					GameActions.OpenStatusBar();
					break;
				case MacroSubType.Journal:
					GameActions.OpenJournal();
					break;
				case MacroSubType.Skills:
					GameActions.OpenSkills();
					break;
				case MacroSubType.MageSpellbook:
				case MacroSubType.NecroSpellbook:
				case MacroSubType.PaladinSpellbook:
				case MacroSubType.BushidoSpellbook:
				case MacroSubType.NinjitsuSpellbook:
				case MacroSubType.SpellWeavingSpellbook:
				case MacroSubType.MysticismSpellbook:
				{
					SpellBookType spellBookType = SpellBookType.Magery;
					switch (macro.SubCode)
					{
					case MacroSubType.NecroSpellbook:
						spellBookType = SpellBookType.Necromancy;
						break;
					case MacroSubType.PaladinSpellbook:
						spellBookType = SpellBookType.Chivalry;
						break;
					case MacroSubType.BushidoSpellbook:
						spellBookType = SpellBookType.Bushido;
						break;
					case MacroSubType.NinjitsuSpellbook:
						spellBookType = SpellBookType.Ninjitsu;
						break;
					case MacroSubType.SpellWeavingSpellbook:
						spellBookType = SpellBookType.Spellweaving;
						break;
					case MacroSubType.MysticismSpellbook:
						spellBookType = SpellBookType.Mysticism;
						break;
					case MacroSubType.BardSpellbook:
						spellBookType = SpellBookType.Mastery;
						break;
					}
					NetClient.Socket.Send_OpenSpellBook((byte)spellBookType);
					break;
				}
				case MacroSubType.Chat:
					GameActions.OpenChat();
					break;
				case MacroSubType.Backpack:
					GameActions.OpenBackpack();
					break;
				case MacroSubType.Overview:
					GameActions.OpenMiniMap();
					break;
				case MacroSubType.WorldMap:
					GameActions.OpenWorldMap();
					break;
				case MacroSubType.Mail:
				case MacroSubType.PartyManifest:
				{
					PartyGump gump8 = UIManager.GetGump<PartyGump>(null);
					if (gump8 == null)
					{
						int x = Client.Game.Window.ClientBounds.Width / 2 - 272;
						int y = Client.Game.Window.ClientBounds.Height / 2 - 240;
						UIManager.Add(new PartyGump(x, y, World.Party.CanLoot));
					}
					else
					{
						gump8.BringOnTop();
					}
					break;
				}
				case MacroSubType.Guild:
					GameActions.OpenGuildGump();
					break;
				case MacroSubType.QuestLog:
					GameActions.RequestQuestMenu();
					break;
				case MacroSubType.PartyChat:
				case MacroSubType.CombatBook:
				case MacroSubType.RacialAbilitiesBook:
				case MacroSubType.BardSpellbook:
					Log.Warn($"Macro '{macro.SubCode}' not implemented");
					break;
				}
				break;
			case MacroType.Close:
			case MacroType.Minimize:
			case MacroType.Maximize:
				switch (macro.SubCode)
				{
				case MacroSubType.WorldMap:
					if (macro.Code == MacroType.Close)
					{
						UIManager.GetGump<MiniMapGump>(null)?.Dispose();
					}
					break;
				case MacroSubType.Configuration:
					if (macro.Code == MacroType.Close)
					{
						UIManager.GetGump<OptionsGump>(null)?.Dispose();
					}
					break;
				case MacroSubType.Paperdoll:
				{
					PaperDollGump gump6 = UIManager.GetGump<PaperDollGump>(World.Player.Serial);
					if (gump6 != null)
					{
						if (macro.Code == MacroType.Close)
						{
							gump6.Dispose();
						}
						else if (macro.Code == MacroType.Minimize)
						{
							gump6.IsMinimized = true;
						}
						else if (macro.Code == MacroType.Maximize)
						{
							gump6.IsMinimized = false;
						}
					}
					break;
				}
				case MacroSubType.Status:
				{
					StatusGumpBase statusGump = StatusGumpBase.GetStatusGump();
					if (macro.Code == MacroType.Close)
					{
						if (statusGump != null)
						{
							statusGump.Dispose();
						}
						else
						{
							UIManager.GetGump<BaseHealthBarGump>(World.Player)?.Dispose();
						}
					}
					else if (macro.Code == MacroType.Minimize)
					{
						if (statusGump != null)
						{
							statusGump.Dispose();
							if (ProfileManager.CurrentProfile.CustomBarsToggled)
							{
								HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(World.Player);
								healthBarGumpCustom.X = statusGump.ScreenCoordinateX;
								healthBarGumpCustom.Y = statusGump.ScreenCoordinateY;
								UIManager.Add(healthBarGumpCustom);
							}
							else
							{
								HealthBarGump healthBarGump = new HealthBarGump(World.Player);
								healthBarGump.X = statusGump.ScreenCoordinateX;
								healthBarGump.Y = statusGump.ScreenCoordinateY;
								UIManager.Add(healthBarGump);
							}
						}
						else
						{
							UIManager.GetGump<BaseHealthBarGump>(World.Player)?.BringOnTop();
						}
					}
					else
					{
						if (macro.Code != MacroType.Maximize)
						{
							break;
						}
						if (statusGump != null)
						{
							statusGump.BringOnTop();
							break;
						}
						BaseHealthBarGump gump7 = UIManager.GetGump<BaseHealthBarGump>(World.Player);
						if (gump7 != null)
						{
							UIManager.Add(StatusGumpBase.AddStatusGump(gump7.ScreenCoordinateX, gump7.ScreenCoordinateY));
						}
					}
					break;
				}
				case MacroSubType.Journal:
				{
					JournalGump gump3 = UIManager.GetGump<JournalGump>(null);
					if (gump3 != null)
					{
						if (macro.Code == MacroType.Close)
						{
							gump3.Dispose();
						}
						else if (macro.Code == MacroType.Minimize)
						{
							gump3.IsMinimized = true;
						}
						else if (macro.Code == MacroType.Maximize)
						{
							gump3.IsMinimized = false;
						}
					}
					break;
				}
				case MacroSubType.Skills:
					if (ProfileManager.CurrentProfile.StandardSkillsGump)
					{
						StandardSkillsGump gump4 = UIManager.GetGump<StandardSkillsGump>(null);
						if (gump4 != null)
						{
							if (macro.Code == MacroType.Close)
							{
								gump4.Dispose();
							}
							else if (macro.Code == MacroType.Minimize)
							{
								gump4.IsMinimized = true;
							}
							else if (macro.Code == MacroType.Maximize)
							{
								gump4.IsMinimized = false;
							}
						}
					}
					else if (macro.Code == MacroType.Close)
					{
						UIManager.GetGump<SkillGumpAdvanced>(null)?.Dispose();
					}
					break;
				case MacroSubType.MageSpellbook:
				case MacroSubType.NecroSpellbook:
				case MacroSubType.PaladinSpellbook:
				case MacroSubType.BushidoSpellbook:
				case MacroSubType.NinjitsuSpellbook:
				case MacroSubType.SpellWeavingSpellbook:
				case MacroSubType.MysticismSpellbook:
				{
					SpellbookGump gump5 = UIManager.GetGump<SpellbookGump>(null);
					if (gump5 != null)
					{
						if (macro.Code == MacroType.Close)
						{
							gump5.Dispose();
						}
						else if (macro.Code == MacroType.Minimize)
						{
							gump5.IsMinimized = true;
						}
						else if (macro.Code == MacroType.Maximize)
						{
							gump5.IsMinimized = false;
						}
					}
					break;
				}
				case MacroSubType.Overview:
					if (macro.Code == MacroType.Close)
					{
						UIManager.GetGump<MiniMapGump>(null)?.Dispose();
					}
					else if (macro.Code == MacroType.Minimize)
					{
						UIManager.GetGump<MiniMapGump>(null)?.ToggleSize(false);
					}
					else if (macro.Code == MacroType.Maximize)
					{
						UIManager.GetGump<MiniMapGump>(null)?.ToggleSize(true);
					}
					break;
				case MacroSubType.Backpack:
				{
					Item item9 = World.Player.FindItemByLayer(Layer.Backpack);
					if (!(item9 != null))
					{
						break;
					}
					ContainerGump gump2 = UIManager.GetGump<ContainerGump>(item9.Serial);
					if (gump2 != null)
					{
						if (macro.Code == MacroType.Close)
						{
							gump2.Dispose();
						}
						else if (macro.Code == MacroType.Minimize)
						{
							gump2.IsMinimized = true;
						}
						else if (macro.Code == MacroType.Maximize)
						{
							gump2.IsMinimized = false;
						}
					}
					break;
				}
				case MacroSubType.Mail:
					Log.Warn($"Macro '{macro.SubCode}' not implemented");
					break;
				case MacroSubType.PartyManifest:
					if (macro.Code == MacroType.Close)
					{
						UIManager.GetGump<PartyGump>(null)?.Dispose();
					}
					break;
				case MacroSubType.PartyChat:
				case MacroSubType.CombatBook:
				case MacroSubType.RacialAbilitiesBook:
				case MacroSubType.BardSpellbook:
					Log.Warn($"Macro '{macro.SubCode}' not implemented");
					break;
				}
				break;
			}
			break;
		case MacroType.OpenDoor:
			GameActions.OpenDoor();
			break;
		case MacroType.UseSkill:
		{
			int num3 = (int)(macro.SubCode - 33);
			if (num3 >= 0 && num3 < 29)
			{
				num3 = _skillTable[num3];
				if (num3 != 255)
				{
					GameActions.UseSkill(num3);
				}
			}
			break;
		}
		case MacroType.LastSkill:
			GameActions.UseSkill(GameActions.LastSkillIndex);
			break;
		case MacroType.CastSpell:
		{
			int num4 = (int)(macro.SubCode - 66 + 1);
			if (num4 <= 0 || num4 > 151)
			{
				break;
			}
			int num5 = 0;
			int i;
			for (i = 0; i < 7; i++)
			{
				num5 += _spellsCountTable[i];
				if (num4 <= num5)
				{
					break;
				}
			}
			if (i >= 7)
			{
				break;
			}
			num4 -= num5 - _spellsCountTable[i];
			num4 += i * 100;
			if (i > 2)
			{
				num4 += 100;
				if (i == 6)
				{
					num4 -= 23;
				}
			}
			GameActions.CastSpell(num4);
			break;
		}
		case MacroType.LastSpell:
			GameActions.Say(".lzauber", ushort.MaxValue, MessageType.Regular, 3);
			break;
		case MacroType.Bow:
		case MacroType.Salute:
			GameActions.EmoteAction((macro.Code - 18 == MacroType.None) ? "bow" : "salute");
			break;
		case MacroType.QuitGame:
			Client.Game.GetScene<GameScene>()?.RequestQuitGame();
			break;
		case MacroType.AllNames:
			GameActions.AllNames();
			break;
		case MacroType.LastObject:
			if (World.Get(World.LastObject) != null)
			{
				GameActions.DoubleClick(World.LastObject);
			}
			break;
		case MacroType.UseItemInHand:
		{
			Item item2 = World.Player.FindItemByLayer(Layer.OneHanded);
			if (item2 != null)
			{
				GameActions.DoubleClick(item2.Serial);
				break;
			}
			Item item3 = World.Player.FindItemByLayer(Layer.TwoHanded);
			if (item3 != null)
			{
				GameActions.DoubleClick(item3.Serial);
			}
			break;
		}
		case MacroType.LastTarget:
			if (TargetManager.IsTargeting)
			{
				if (TargetManager.TargetingState != 0)
				{
					TargetManager.TargetLast();
				}
				else if (TargetManager.LastTargetInfo.IsEntity)
				{
					TargetManager.Target(TargetManager.LastTargetInfo.Serial);
				}
				else
				{
					TargetManager.Target(TargetManager.LastTargetInfo.Graphic, TargetManager.LastTargetInfo.X, TargetManager.LastTargetInfo.Y, TargetManager.LastTargetInfo.Z);
				}
				WaitForTargetTimer = 0L;
			}
			else if (WaitForTargetTimer < Time.Ticks)
			{
				WaitForTargetTimer = 0L;
			}
			else
			{
				result = 1;
			}
			break;
		case MacroType.TargetSelf:
			if (TargetManager.IsTargeting)
			{
				TargetManager.Target(World.Player);
				WaitForTargetTimer = 0L;
			}
			else if (WaitForTargetTimer < Time.Ticks)
			{
				WaitForTargetTimer = 0L;
			}
			else
			{
				result = 1;
			}
			break;
		case MacroType.ArmDisarm:
		{
			int num7 = (int)(1 - (macro.SubCode - 61));
			Client.Game.GetScene<GameScene>();
			if (num7 < 0 || num7 > 1 || ItemHold.Enabled)
			{
				break;
			}
			if (_itemsInHand[num7] != 0)
			{
				GameActions.PickUp(_itemsInHand[num7], 0, 0, 1, null);
				GameActions.Equip();
				_itemsInHand[num7] = 0u;
				_nextTimer = Time.Ticks + 1000;
				break;
			}
			Item item12 = World.Player.FindItemByLayer(Layer.Backpack);
			if (!(item12 == null))
			{
				Item item13 = World.Player.FindItemByLayer((Layer)(1 + (byte)num7));
				if (item13 != null)
				{
					_itemsInHand[num7] = item13.Serial;
					GameActions.PickUp(item13, 0, 0, 1, null);
					GameActions.DropItem(ItemHold.Serial, 65535, 65535, 0, item12.Serial);
					_nextTimer = Time.Ticks + 1000;
				}
			}
			break;
		}
		case MacroType.WaitForTarget:
			if (WaitForTargetTimer == 0L)
			{
				WaitForTargetTimer = Time.Ticks + 5000;
			}
			if (TargetManager.IsTargeting || WaitForTargetTimer < Time.Ticks)
			{
				WaitForTargetTimer = 0L;
			}
			else
			{
				result = 1;
			}
			break;
		case MacroType.TargetNext:
		{
			uint num6 = World.FindNext(ScanTypeObject.Mobiles, TargetManager.LastTargetInfo.Serial, reverse: false);
			if (SerialHelper.IsValid(num6))
			{
				TargetManager.LastTargetInfo.SetEntity(num6);
				TargetManager.LastAttack = num6;
			}
			break;
		}
		case MacroType.AttackLast:
			if (TargetManager.LastTargetInfo.IsEntity)
			{
				GameActions.Attack(TargetManager.LastTargetInfo.Serial);
			}
			break;
		case MacroType.Delay:
		{
			string text5 = ((MacroObjectString)macro).Text;
			if (!string.IsNullOrEmpty(text5) && int.TryParse(text5, out var result6))
			{
				_nextTimer = Time.Ticks + result6;
			}
			break;
		}
		case MacroType.CircleTrans:
			ProfileManager.CurrentProfile.UseCircleOfTransparency = !ProfileManager.CurrentProfile.UseCircleOfTransparency;
			break;
		case MacroType.CloseGump:
			UIManager.Gumps.Where((Gump s) => !(s is TopBarGump) && !(s is BuffGump) && !(s is WorldViewportGump)).ToList().ForEach(delegate(Gump s)
			{
				s.Dispose();
			});
			break;
		case MacroType.AlwaysRun:
			ProfileManager.CurrentProfile.AlwaysRun = !ProfileManager.CurrentProfile.AlwaysRun;
			GameActions.Print(ProfileManager.CurrentProfile.AlwaysRun ? ResGeneral.AlwaysRunIsNowOn : ResGeneral.AlwaysRunIsNowOff, 946, MessageType.Regular, 3);
			break;
		case MacroType.AlwaysWalk:
			ProfileManager.CurrentProfile.AlwaysWalk = !ProfileManager.CurrentProfile.AlwaysWalk;
			GameActions.Print(ProfileManager.CurrentProfile.AlwaysWalk ? ResGeneral.AlwaysWalkIsNowOn : ResGeneral.AlwaysWalkIsNowOff, 946, MessageType.Regular, 3);
			break;
		case MacroType.SaveDesktop:
			ProfileManager.CurrentProfile?.Save(ProfileManager.ProfilePath);
			break;
		case MacroType.EnableRangeColor:
			ProfileManager.CurrentProfile.NoColorObjectsOutOfRange = true;
			break;
		case MacroType.DisableRangeColor:
			ProfileManager.CurrentProfile.NoColorObjectsOutOfRange = false;
			break;
		case MacroType.ToggleRangeColor:
			ProfileManager.CurrentProfile.NoColorObjectsOutOfRange = !ProfileManager.CurrentProfile.NoColorObjectsOutOfRange;
			break;
		case MacroType.AttackSelectedTarget:
			if (SerialHelper.IsMobile(TargetManager.SelectedTarget))
			{
				GameActions.Attack(TargetManager.SelectedTarget);
			}
			break;
		case MacroType.UseSelectedTarget:
			if (SerialHelper.IsValid(TargetManager.SelectedTarget))
			{
				GameActions.DoubleClick(TargetManager.SelectedTarget);
			}
			break;
		case MacroType.CurrentTarget:
			if (TargetManager.SelectedTarget != 0)
			{
				if (WaitForTargetTimer == 0L)
				{
					WaitForTargetTimer = Time.Ticks + 5000;
				}
				if (TargetManager.IsTargeting)
				{
					TargetManager.Target(TargetManager.SelectedTarget);
					WaitForTargetTimer = 0L;
				}
				else if (WaitForTargetTimer < Time.Ticks)
				{
					WaitForTargetTimer = 0L;
				}
				else
				{
					result = 1;
				}
			}
			break;
		case MacroType.TargetSystemOnOff:
			GameActions.Print(ResGeneral.TargetSystemNotImplemented, 946, MessageType.Regular, 3);
			break;
		case MacroType.BandageSelf:
		case MacroType.BandageTarget:
		case MacroType.BandageSelfKonditionell:
		{
			Item item6 = World.Player.FindBandage();
			if (!(item6 != null))
			{
				break;
			}
			if (macro.Code == MacroType.BandageSelf)
			{
				NetClient.Socket.Send_TargetSelectedObject(item6.Serial, World.Player.Serial);
			}
			else if (macro.Code == MacroType.BandageSelfKonditionell)
			{
				string text2 = ((MacroObjectString)macro).Text;
				if (!string.IsNullOrEmpty(text2) && int.TryParse(text2, out var result3) && result3 <= 100 && World.Player != null && World.Player.HitsPercentage < result3)
				{
					NetClient.Socket.Send_TargetSelectedObject(item6.Serial, World.Player.Serial);
				}
			}
			else if (SerialHelper.IsMobile(TargetManager.SelectedTarget))
			{
				NetClient.Socket.Send_TargetSelectedObject(item6.Serial, TargetManager.SelectedTarget);
			}
			break;
		}
		case MacroType.SetUpdateRange:
		case MacroType.ModifyUpdateRange:
		{
			if (macro is MacroObjectString macroObjectString && !string.IsNullOrEmpty(macroObjectString.Text) && byte.TryParse(macroObjectString.Text, out var result2))
			{
				if (result2 < 5)
				{
					result2 = 5;
				}
				else if (result2 > 31)
				{
					result2 = 31;
				}
				World.ClientViewRange = result2;
				GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, result2), 946, MessageType.Regular, 3);
			}
			break;
		}
		case MacroType.IncreaseUpdateRange:
			World.ClientViewRange++;
			if (World.ClientViewRange > 31)
			{
				World.ClientViewRange = 31;
			}
			GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, World.ClientViewRange), 946, MessageType.Regular, 3);
			break;
		case MacroType.DecreaseUpdateRange:
			World.ClientViewRange--;
			if (World.ClientViewRange < 5)
			{
				World.ClientViewRange = 5;
			}
			GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, World.ClientViewRange), 946, MessageType.Regular, 3);
			break;
		case MacroType.MaxUpdateRange:
			World.ClientViewRange = 31;
			GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, World.ClientViewRange), 946, MessageType.Regular, 3);
			break;
		case MacroType.MinUpdateRange:
			World.ClientViewRange = 5;
			GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, World.ClientViewRange), 946, MessageType.Regular, 3);
			break;
		case MacroType.DefaultUpdateRange:
			World.ClientViewRange = 31;
			GameActions.Print(string.Format(ResGeneral.ClientViewRangeIsNow0, World.ClientViewRange), 946, MessageType.Regular, 3);
			break;
		case MacroType.SelectNext:
		case MacroType.SelectPrevious:
		case MacroType.SelectNearest:
		{
			ScanModeObject scanModeObject = (ScanModeObject)(macro.Code - 48);
			ScanTypeObject scanTypeObject = (ScanTypeObject)(macro.SubCode - 209);
			if (scanModeObject == ScanModeObject.Nearest)
			{
				SetLastTarget(World.FindNearest(scanTypeObject));
			}
			else
			{
				SetLastTarget(World.FindNext(scanTypeObject, TargetManager.SelectedTarget, scanModeObject == ScanModeObject.Previous));
			}
			break;
		}
		case MacroType.SelectNearestToCursor:
			SetLastTarget(World.FindNearestToMouseCursor((ScanTypeObject)(macro.SubCode - 209)), overheadMessage: false);
			break;
		case MacroType.ToggleBuffIconGump:
		{
			BuffGump gump = UIManager.GetGump<BuffGump>(null);
			if (gump != null)
			{
				gump.Dispose();
			}
			else
			{
				UIManager.Add(new BuffGump(100, 100));
			}
			break;
		}
		case MacroType.InvokeVirtue:
		{
			byte id = (byte)(macro.SubCode - 63 + 1);
			NetClient.Socket.Send_InvokeVirtueRequest(id);
			break;
		}
		case MacroType.PrimaryAbility:
			GameActions.UsePrimaryAbility();
			break;
		case MacroType.SecondaryAbility:
			GameActions.UseSecondaryAbility();
			break;
		case MacroType.ToggleGargoyleFly:
			if (World.Player.Race == RaceType.GARGOYLE)
			{
				NetClient.Socket.Send_ToggleGargoyleFlying();
			}
			break;
		case MacroType.EquipLastWeapon:
			NetClient.Socket.Send_EquipLastWeapon();
			break;
		case MacroType.Zoom:
			switch (macro.SubCode)
			{
			case MacroSubType.MSC_NONE:
			case MacroSubType.DefaultZoom:
				Client.Game.Scene.Camera.Zoom = ProfileManager.CurrentProfile.DefaultScale;
				break;
			case MacroSubType.ZoomIn:
			{
				Camera camera2 = Client.Game.Scene.Camera;
				int zoomIndex = camera2.ZoomIndex - 1;
				camera2.ZoomIndex = zoomIndex;
				break;
			}
			case MacroSubType.ZoomOut:
			{
				Camera camera = Client.Game.Scene.Camera;
				int zoomIndex = camera.ZoomIndex + 1;
				camera.ZoomIndex = zoomIndex;
				break;
			}
			}
			break;
		case MacroType.ToggleChatVisibility:
			UIManager.SystemChat?.ToggleChatVisibility();
			break;
		case MacroType.AuraOnOff:
			AuraManager.ToggleVisibility();
			break;
		case MacroType.Grab:
			GameActions.Print(ResGeneral.TargetAnItemToGrabIt, 946, MessageType.Regular, 3);
			TargetManager.SetTargeting(CursorTarget.Grab, 0u, TargetType.Neutral);
			break;
		case MacroType.SetGrabBag:
			GameActions.Print(ResGumps.TargetContainerToGrabItemsInto, 946, MessageType.Regular, 3);
			TargetManager.SetTargeting(CursorTarget.SetGrabBag, 0u, TargetType.Neutral);
			break;
		case MacroType.NamesOnOff:
			NameOverHeadManager.ToggleOverheads();
			break;
		case MacroType.UsePotion:
		{
			ScanTypeObject scanTypeObject = (ScanTypeObject)(macro.SubCode - 226);
			ushort graphic = (ushort)(3846 + scanTypeObject);
			Item item = World.Player.FindItemByGraphic(graphic);
			if (item != null)
			{
				GameActions.DoubleClick(item);
			}
			break;
		}
		case MacroType.CloseAllHealthBars:
			foreach (BaseHealthBarGump item14 in UIManager.Gumps.OfType<BaseHealthBarGump>())
			{
				if (UIManager.AnchorManager[item14] == null && item14.LocalSerial != (uint)World.Player)
				{
					item14.Dispose();
				}
			}
			break;
		case MacroType.ToggleDrawRoofs:
			ProfileManager.CurrentProfile.DrawRoofs = !ProfileManager.CurrentProfile.DrawRoofs;
			break;
		case MacroType.ToggleTreeStumps:
			StaticFilters.CleanTreeTextures();
			ProfileManager.CurrentProfile.TreeToStumps = !ProfileManager.CurrentProfile.TreeToStumps;
			break;
		case MacroType.ToggleVegetation:
			ProfileManager.CurrentProfile.HideVegetation = !ProfileManager.CurrentProfile.HideVegetation;
			break;
		case MacroType.ToggleCaveTiles:
			StaticFilters.CleanCaveTextures();
			ProfileManager.CurrentProfile.EnableCaveBorder = !ProfileManager.CurrentProfile.EnableCaveBorder;
			break;
		case MacroType.Doppelklick:
			MacroDoubleClick();
			break;
		}
		return result;
	}

	private static void EquipItem(Item backpack, Item searchItem)
	{
		Layer layer = (Layer)searchItem.ItemData.Layer;
		if (layer == Layer.OneHanded && searchItem.ItemData.IsWeapon)
		{
			TargetManager.CancelTarget();
			NetClient.Socket.Send_PickUpRequest(searchItem, searchItem.Amount);
			NetClient.Socket.Send_EquipRequest(searchItem.Serial, layer, backpack.Container);
		}
		else
		{
			GameActions.DoubleClick(searchItem.Serial);
		}
	}

	private static void MacroDoubleClick()
	{
		bool flag = false;
		if (!UIManager.IsMouseOverWorld)
		{
			UIManager.OnMouseDoubleClick(MouseButtonType.Left);
			return;
		}
		BaseGameObject lastObject = SelectedObject.LastObject;
		if (!(lastObject is Item item))
		{
			if (!(lastObject is Mobile mobile))
			{
				if (lastObject is TextObject { Owner: Entity owner })
				{
					flag = true;
					GameActions.DoubleClick(owner);
				}
				else
				{
					World.LastObject = 0u;
				}
			}
			else
			{
				flag = true;
				if (World.Player.InWarMode && World.Player != mobile)
				{
					GameActions.Attack(mobile);
				}
				else
				{
					GameActions.DoubleClick(mobile);
				}
			}
		}
		else
		{
			flag = true;
			if (!GameActions.OpenCorpse(item))
			{
				GameActions.DoubleClick(item);
			}
		}
		if (flag)
		{
			DelayedObjectClickManager.Clear();
		}
	}

	private static void SetLastTarget(uint serial)
	{
		if (SerialHelper.IsValid(serial))
		{
			Entity entity = World.Get(serial);
			if (SerialHelper.IsMobile(serial))
			{
				if (entity != null)
				{
					GameActions.MessageOverhead(string.Format(ResGeneral.Target0, entity.Name), Notoriety.GetHue(((Mobile)entity).NotorietyFlag), World.Player);
					TargetManager.SelectedTarget = serial;
					TargetManager.LastTargetInfo.SetEntity(serial);
					return;
				}
			}
			else if (entity != null)
			{
				GameActions.MessageOverhead(string.Format(ResGeneral.Target0, entity.Name), 992, World.Player);
				TargetManager.SelectedTarget = serial;
				TargetManager.LastTargetInfo.SetEntity(serial);
				return;
			}
		}
		GameActions.Print(ResGeneral.EntityNotFound, 946, MessageType.Regular, 3);
	}

	public static void SetLastTarget(uint serial, bool overheadMessage)
	{
		if (SerialHelper.IsValid(serial))
		{
			Entity entity = World.Get(serial);
			if (SerialHelper.IsMobile(serial))
			{
				if (entity != null)
				{
					if (overheadMessage)
					{
						GameActions.MessageOverhead(string.Format(ResGeneral.Target0, entity.Name), Notoriety.GetHue(((Mobile)entity).NotorietyFlag), World.Player);
					}
					TargetManager.SelectedTarget = serial;
					TargetManager.LastTargetInfo.SetEntity(serial);
					return;
				}
			}
			else if (entity != null)
			{
				if (overheadMessage)
				{
					GameActions.MessageOverhead(string.Format(ResGeneral.Target0, entity.Name), 992, World.Player);
				}
				TargetManager.SelectedTarget = serial;
				TargetManager.LastTargetInfo.SetEntity(serial);
				return;
			}
		}
		GameActions.Print(ResGeneral.EntityNotFound, 946, MessageType.Regular, 3);
	}
}
