using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Game;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using TinyJson;

namespace ClassicUO.Configuration;

[MatchSnakeCase]
internal sealed class Profile
{
	[JsonIgnore]
	public string Username { get; set; }

	[JsonIgnore]
	public string ServerName { get; set; }

	[JsonIgnore]
	public string CharacterName { get; set; }

	public bool EnableSound { get; set; } = true;

	public int SoundVolume { get; set; } = 100;

	public int BardSoundsVolume { get; set; } = 60;

	public int AnimalSoundsVolume { get; set; } = 60;

	public int HeartbeatVolume { get; set; } = 60;

	public bool EnableMusic { get; set; } = true;

	public bool HeartbeatSound { get; set; } = true;

	public int HeartbeatThreshold { get; set; } = 60;

	public int MusicVolume { get; set; } = 30;

	public bool EnableFootstepsSound { get; set; } = true;

	public bool EnableCombatMusic { get; set; } = true;

	public bool ReproduceSoundsInBackground { get; set; }

	public byte ChatFont { get; set; }

	public int SpeechDelay { get; set; } = 100;

	public bool ScaleSpeechDelay { get; set; } = true;

	public bool ForceUnicodeJournal { get; set; }

	public bool ShowJournalEntryTime { get; set; } = true;

	public bool ShowYouSeeEntriesHostile { get; set; } = true;

	public bool ShowYouSeeEntriesItems { get; set; } = true;

	public bool ShowYouSeeEntriesNeutral { get; set; } = true;

	public bool IgnoreAllianceMessages { get; set; }

	public bool IgnoreGuildMessages { get; set; }

	public int JournalSkin { get; set; }

	public Point ChatPosition { get; set; } = new Point(0, 0);

	public bool ShowSystemMessagesInGameWindow { get; set; } = true;

	public bool AutoAcceptInvitesOfGuildMembers { get; set; } = true;

	public ushort SpeechHue { get; set; } = 690;

	public ushort WhisperHue { get; set; } = 51;

	public ushort EmoteHue { get; set; } = 33;

	public ushort YellHue { get; set; } = 33;

	public ushort PartyMessageHue { get; set; } = 68;

	public ushort GuildMessageHue { get; set; } = 68;

	public ushort AllyMessageHue { get; set; } = 87;

	public ushort ChatMessageHue { get; set; } = 598;

	public ushort InnocentHue { get; set; } = 90;

	public ushort PartyAuraHue { get; set; } = 68;

	public ushort FriendHue { get; set; } = 68;

	public ushort CriminalHue { get; set; } = 946;

	public ushort CanAttackHue { get; set; } = 946;

	public ushort EnemyHue { get; set; } = 49;

	public ushort MurdererHue { get; set; } = 35;

	public ushort BeneficHue { get; set; } = 89;

	public ushort HarmfulHue { get; set; } = 32;

	public ushort NeutralHue { get; set; } = 945;

	public bool EnabledSpellHue { get; set; }

	public bool EnabledSpellFormat { get; set; }

	public string SpellDisplayFormat { get; set; } = "{spell}]";

	public ushort PoisonHue { get; set; } = 68;

	public ushort ParalyzedHue { get; set; } = 332;

	public ushort InvulnerableHue { get; set; } = 48;

	public bool EnabledCriminalActionQuery { get; set; }

	public bool EnabledBeneficialCriminalActionQuery { get; set; }

	public bool EnableStatReport { get; set; } = true;

	public bool EnableSkillReport { get; set; } = true;

	public int BackpackStyle { get; set; }

	public bool HighlightGameObjects { get; set; }

	public ushort HighlightGameObjectsColor { get; set; } = 20;

	public bool HighlightMobilesByParalize { get; set; } = true;

	public bool HighlightMobilesByPoisoned { get; set; } = true;

	public bool HighlightMobilesByInvul { get; set; } = true;

	public bool ShowMobilesHP { get; set; } = true;

	public int TargetFrameHealthBarPositionX { get; set; } = 500;

	public int TargetFrameHealthBarPositionY { get; set; } = 500;

	public bool ShowTargetFrames { get; set; }

	public bool EnableTargetWithoutWarmode { get; set; }

	public bool ShowTargetingOverheadMessage { get; set; } = true;

	public int MobileHPType { get; set; }

	public int MobileHPShowWhen { get; set; } = 2;

	public bool DrawRoofs
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool TreeToStumps
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool EnableCaveBorder { get; set; }

	public bool HideVegetation
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public int FieldsType { get; set; }

	public bool NoColorObjectsOutOfRange { get; set; }

	public bool UseCircleOfTransparency { get; set; }

	public int CircleOfTransparencyRadius { get; set; } = 100;

	public int CircleOfTransparencyType
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

	public int VendorGumpHeight { get; set; } = 60;

	public float DefaultScale { get; set; } = 1f;

	public bool EnableMousewheelScaleZoom { get; set; }

	public bool SaveScaleAfterClose { get; set; }

	public bool RestoreScaleAfterUnpressCtrl { get; set; }

	public bool BandageSelfOld { get; set; } = true;

	public bool EnableDeathScreen
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool EnableBlackWhiteEffect
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool ActivateShakeEffects { get; set; } = true;

	public bool UseTooltip { get; set; } = true;

	public ushort TooltipTextHue { get; set; } = ushort.MaxValue;

	public int TooltipDelayBeforeDisplay { get; set; } = 250;

	public int TooltipDisplayZoom { get; set; } = 100;

	public int TooltipBackgroundOpacity { get; set; } = 70;

	public byte TooltipFont { get; set; } = 1;

	public bool EnablePathfind { get; set; }

	public bool UseShiftToPathfind { get; set; }

	public bool AlwaysRun { get; set; }

	public bool AlwaysWalk { get; set; }

	public bool AlwaysRunUnlessHidden { get; set; }

	public bool SmoothMovements { get; set; } = true;

	public bool HoldDownKeyTab { get; set; }

	public bool HoldShiftForContext { get; set; }

	public bool HoldShiftToSplitStack { get; set; }

	public Point WindowClientBounds { get; set; } = new Point(600, 480);

	public Point ContainerDefaultPosition { get; set; } = new Point(24, 24);

	public Point GameWindowPosition { get; set; } = new Point(10, 10);

	public bool GameWindowLock { get; set; }

	public bool GameWindowFullSize { get; set; }

	public bool WindowBorderless { get; set; }

	public Point GameWindowSize { get; set; } = new Point(600, 480);

	public Point TopbarGumpPosition { get; set; } = new Point(0, 0);

	public bool TopbarGumpIsMinimized { get; set; }

	public bool TopbarGumpIsDisabled { get; set; }

	public bool UseAlternativeLights
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool UseCustomLightLevel
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public byte LightLevel
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public bool UseDarkNights { get; set; }

	public int CloseHealthBarType { get; set; } = 2;

	public bool ActivateChatAfterEnter { get; set; }

	public bool ShowChatActivity { get; set; } = true;

	public int ChatActivitiySize { get; set; } = 1;

	public bool ActivateChatAdditionalButtons { get; set; } = true;

	public bool ActivateChatShiftEnterSupport { get; set; } = true;

	public bool UseObjectsFading { get; set; } = true;

	public bool HoldDownKeyAltToCloseAnchored { get; set; } = true;

	public bool CloseAllAnchoredGumpsInGroupWithRightClick { get; set; }

	public bool HoldAltToMoveGumps { get; set; }

	public bool PreventAltF4ToCloseClient { get; set; }

	public bool HideScreenshotStoredInMessage { get; set; }

	public int PaperdollGump { get; set; }

	public int StatusBarGump { get; set; }

	public bool CastSpellsByOneClick { get; set; }

	public bool BuffBarTime { get; set; } = true;

	public bool BuffEndWithAlphaBlinks { get; set; } = true;

	public bool FastSpellsAssign { get; set; }

	public bool AutoOpenDoors
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool SmoothDoors { get; set; }

	public bool AutoOpenCorpses { get; set; }

	public int AutoOpenCorpseRange { get; set; } = 2;

	public int CorpseOpenOptions { get; set; } = 3;

	public bool SkipEmptyCorpse { get; set; } = true;

	public bool DisableDefaultHotkeys { get; set; }

	public bool DisableArrowBtn { get; set; }

	public bool DisableTabBtn { get; set; }

	public bool DisableCtrlQWBtn { get; set; }

	public bool DisableAutoMove { get; set; }

	public bool EnableDragSelect { get; set; }

	public int DragSelectModifierKey { get; set; }

	public bool OverrideContainerLocation { get; set; }

	public int OverrideContainerLocationSetting { get; set; }

	public Point OverrideContainerLocationPosition { get; set; } = new Point(200, 200);

	public bool DragSelectHumanoidsOnly { get; set; }

	public int DragSelectStartX { get; set; } = 100;

	public int DragSelectStartY { get; set; } = 100;

	public bool DragSelectAsAnchor { get; set; }

	public NameOverheadTypeAllowed NameOverheadTypeAllowed { get; set; }

	public bool NameOverheadToggled { get; set; }

	public bool ShowTargetRangeIndicator { get; set; }

	public bool PartyInviteGump { get; set; }

	public bool CustomBarsToggled { get; set; }

	public bool CBBlackBGToggled
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool ShowInfoBar { get; set; }

	public int InfoBarHighlightType { get; set; }

	public InfoBarItem[] InfoBarItems { get; set; }

	public Macro[] Macros { get; set; }

	public bool CounterBarEnabled { get; set; }

	public bool CounterBarHighlightOnUse { get; set; }

	public bool CounterBarHighlightOnAmount { get; set; }

	public bool CounterBarDisplayAbbreviatedAmount { get; set; }

	public int CounterBarAbbreviatedAmount { get; set; } = 1000;

	public int CounterBarHighlightAmount { get; set; } = 5;

	public int CounterBarCellSize { get; set; } = 40;

	public int CounterBarRows { get; set; } = 1;

	public int CounterBarColumns { get; set; } = 1;

	public bool ShowSkillsChangedMessage { get; set; } = true;

	public int ShowSkillsChangedDeltaValue { get; set; } = 1;

	public bool ShowStatsChangedMessage { get; set; } = true;

	public int FilterType { get; set; }

	public bool ShadowsEnabled { get; set; } = true;

	public bool ShadowsStatics { get; set; } = true;

	public int TerrainShadowsLevel { get; set; } = 15;

	public int AuraUnderFeetType { get; set; }

	public bool AuraOnMouse { get; set; }

	public bool PartyAura { get; set; }

	public bool UseXBR { get; set; } = true;

	public bool HideChatGradient { get; set; } = true;

	public bool StandardSkillsGump { get; set; } = true;

	public uint GrabBagSerial { get; set; }

	public int GridLootType { get; set; }

	public bool ReduceFPSWhenInactive { get; set; } = true;

	public bool OverrideAllFonts { get; set; } = true;

	public bool OverrideAllFontsIsUnicode
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool JournalDarkMode { get; set; }

	public byte ContainersScale { get; set; } = 100;

	public bool ScaleItemsInsideContainers { get; set; }

	public bool DoubleClickToLootInsideContainers { get; set; }

	public bool UseLargeContainerGumps { get; set; }

	public bool RelativeDragAndDropItems { get; set; }

	public bool HighlightContainerWhenSelected { get; set; }

	public bool ShowHouseContent { get; set; }

	public bool SaveHealthbars { get; set; }

	public bool TextFading { get; set; } = true;

	public bool UseSmoothBoatMovement { get; set; } = true;

	public bool IgnoreStaminaCheck { get; set; }

	public bool ShowJournalClient { get; set; } = true;

	public bool ShowJournalObjects { get; set; } = true;

	public bool ShowJournalSystem { get; set; } = true;

	public bool ShowJournalGuildAlly { get; set; } = true;

	public int JournalWidth { get; set; } = 400;

	public int JournalHeight { get; set; } = 300;

	public int JournalPositionX { get; set; } = 100;

	public int JournalPositionY { get; set; } = 100;

	public int AdditionalJournalWidth { get; set; } = 400;

	public int AdditionalJournalHeight { get; set; } = 300;

	public int AdditionalJournalPositionX { get; set; } = 100;

	public int AdditionalJournalPositionY { get; set; } = 100;

	public bool AdditionalJournalShowSelf { get; set; } = true;

	public bool AdditionalJournalShowSystemMessages { get; set; } = true;

	public int WorldMapWidth { get; set; } = 400;

	public int WorldMapHeight { get; set; } = 400;

	public int WorldMapFont { get; set; } = 3;

	public bool WorldMapFlipMap { get; set; } = true;

	public bool WorldMapTopMost { get; set; }

	public bool WorldMapFreeView
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool WorldMapShowParty { get; set; } = true;

	public int WorldMapZoomIndex { get; set; } = 4;

	public bool WorldMapShowCoordinates { get; set; } = true;

	public bool WorldMapShowMobiles { get; set; } = true;

	public bool WorldMapShowPlayerName { get; set; } = true;

	public bool WorldMapShowPlayerBar { get; set; } = true;

	public bool WorldMapShowGroupName { get; set; } = true;

	public bool WorldMapShowGroupBar { get; set; } = true;

	public bool WorldMapShowMarkers { get; set; } = true;

	public bool WorldMapShowMarkersNames { get; set; } = true;

	public bool WorldMapShowMultis { get; set; } = true;

	public string WorldMapHiddenMarkerFiles { get; set; } = string.Empty;

	public static uint GumpsVersion { get; private set; }

	public void Save(string path)
	{
		Log.Trace("Saving path:\t\t" + path);
		ConfigurationResolver.Save(this, Path.Combine(path, "profile.json"));
		SaveGumps(path);
		Log.Trace("Saving done!");
	}

	private void SaveGumps(string path)
	{
		using (XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(path, "gumps.xml"), Encoding.UTF8)
		{
			Formatting = Formatting.Indented,
			IndentChar = '\t',
			Indentation = 1
		})
		{
			xmlTextWriter.WriteStartDocument(standalone: true);
			xmlTextWriter.WriteStartElement("gumps");
			UIManager.AnchorManager.Save(xmlTextWriter);
			LinkedList<Gump> linkedList = new LinkedList<Gump>();
			foreach (Gump gump in UIManager.Gumps)
			{
				if (!gump.IsDisposed && gump.CanBeSaved && (!(gump is AnchorableGump control) || UIManager.AnchorManager[control] == null))
				{
					linkedList.AddLast(gump);
				}
			}
			LinkedListNode<Gump> first = linkedList.First;
			while (first != null)
			{
				Gump value = first.Value;
				if (value.LocalSerial != 0)
				{
					Item item = World.Items.Get(value.LocalSerial);
					if (item != null && !item.IsDestroyed && item.Opened)
					{
						while (SerialHelper.IsItem(item.Container))
						{
							item = World.Items.Get(item.Container);
						}
						SaveItemsGumpRecursive(item, xmlTextWriter, linkedList);
						if (first.List != null)
						{
							linkedList.Remove(first);
						}
						first = linkedList.First;
						continue;
					}
				}
				xmlTextWriter.WriteStartElement("gump");
				value.Save(xmlTextWriter);
				xmlTextWriter.WriteEndElement();
				if (first.List != null)
				{
					linkedList.Remove(first);
				}
				first = linkedList.First;
			}
			xmlTextWriter.WriteEndElement();
			xmlTextWriter.WriteEndDocument();
		}
		SkillsGroupManager.Save();
	}

	private static void SaveItemsGumpRecursive(Item parent, XmlTextWriter xml, LinkedList<Gump> list)
	{
		if (parent != null && !parent.IsDestroyed && parent.Opened)
		{
			SaveItemsGump(parent, xml, list);
			Item item = (Item)parent.Items;
			while (item != null)
			{
				Item item2 = (Item)item.Next;
				SaveItemsGumpRecursive(item, xml, list);
				item = item2;
			}
		}
	}

	private static void SaveItemsGump(Item item, XmlTextWriter xml, LinkedList<Gump> list)
	{
		if (!(item != null) || item.IsDestroyed || !item.Opened)
		{
			return;
		}
		LinkedListNode<Gump> linkedListNode = list.First;
		while (linkedListNode != null)
		{
			LinkedListNode<Gump> next = linkedListNode.Next;
			if (linkedListNode.Value.LocalSerial == item.Serial && !linkedListNode.Value.IsDisposed)
			{
				xml.WriteStartElement("gump");
				linkedListNode.Value.Save(xml);
				xml.WriteEndElement();
				list.Remove(linkedListNode);
				break;
			}
			linkedListNode = next;
		}
	}

	public List<Gump> ReadGumps(string path)
	{
		List<Gump> list = new List<Gump>();
		SkillsGroupManager.Load();
		string text = Path.Combine(path, "gumps.xml");
		if (File.Exists(text))
		{
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(text);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
				return list;
			}
			XmlElement xmlElement = xmlDocument["gumps"];
			if (xmlElement != null)
			{
				foreach (XmlElement childNode in xmlElement.ChildNodes)
				{
					if (childNode.Name != "gump")
					{
						continue;
					}
					try
					{
						GumpType gumpType = (GumpType)int.Parse(childNode.GetAttribute("type"));
						int num = int.Parse(childNode.GetAttribute("x"));
						int num2 = int.Parse(childNode.GetAttribute("y"));
						uint localSerial = uint.Parse(childNode.GetAttribute("serial"));
						Gump gump = null;
						switch (gumpType)
						{
						case GumpType.Buff:
							gump = new BuffGump();
							break;
						case GumpType.Container:
							gump = new ContainerGump();
							break;
						case GumpType.CounterBar:
							gump = new CounterBarGump();
							break;
						case GumpType.HealthBar:
							gump = ((!CustomBarsToggled) ? ((BaseHealthBarGump)new HealthBarGump()) : ((BaseHealthBarGump)new HealthBarGumpCustom()));
							break;
						case GumpType.InfoBar:
							gump = new InfoBarGump();
							break;
						case GumpType.Journal:
							gump = new JournalGump(JournalSkin);
							break;
						case GumpType.AdditionalJournal:
							gump = new AdditionalJournalGump(JournalSkin);
							break;
						case GumpType.MacroButton:
							gump = new MacroButtonGump();
							break;
						case GumpType.MiniMap:
							gump = new MiniMapGump();
							break;
						case GumpType.PaperDoll:
							gump = new PaperDollGump();
							break;
						case GumpType.SkillMenu:
							gump = ((!StandardSkillsGump) ? ((Gump)new SkillGumpAdvanced()) : ((Gump)new StandardSkillsGump()));
							break;
						case GumpType.SpellBook:
							gump = new SpellbookGump();
							break;
						case GumpType.StatusGump:
							gump = StatusGumpBase.AddStatusGump(0, 0);
							break;
						case GumpType.AbilityButton:
							gump = new UseAbilityButtonGump();
							break;
						case GumpType.SpellButton:
							gump = new UseSpellButtonGump();
							break;
						case GumpType.SkillButton:
							gump = new SkillButtonGump();
							break;
						case GumpType.RacialButton:
							gump = new RacialAbilityButton();
							break;
						case GumpType.WorldMap:
							gump = new WorldMapGump();
							break;
						case GumpType.Debug:
							gump = new DebugGump(100, 100);
							break;
						case GumpType.NetStats:
							gump = new NetworkStatsGump(100, 100);
							break;
						case GumpType.NameOverHeadHandler:
							NameOverHeadHandlerGump.LastPosition = new Point(num, num2);
							break;
						}
						if (gump != null)
						{
							gump.LocalSerial = localSerial;
							gump.Restore(childNode);
							gump.X = num;
							gump.Y = num2;
							if (gump.LocalSerial != 0)
							{
								UIManager.SavePosition(gump.LocalSerial, new Point(num, num2));
							}
							if (!gump.IsDisposed)
							{
								list.Add(gump);
							}
						}
					}
					catch (Exception ex2)
					{
						Log.Error(ex2.ToString());
					}
				}
				foreach (XmlElement item in xmlElement.GetElementsByTagName("anchored_group_gump"))
				{
					int xCount = int.Parse(item.GetAttribute("matrix_w"));
					int yCount = int.Parse(item.GetAttribute("matrix_h"));
					AnchorManager.AnchorGroup anchorGroup = new AnchorManager.AnchorGroup();
					anchorGroup.ResizeMatrix(xCount, yCount, 0, 0);
					foreach (XmlElement item2 in item.GetElementsByTagName("gump"))
					{
						try
						{
							GumpType gumpType2 = (GumpType)int.Parse(item2.GetAttribute("type"));
							int num3 = int.Parse(item2.GetAttribute("x"));
							int num4 = int.Parse(item2.GetAttribute("y"));
							uint localSerial2 = uint.Parse(item2.GetAttribute("serial"));
							int num5 = int.Parse(item2.GetAttribute("matrix_x"));
							int num6 = int.Parse(item2.GetAttribute("matrix_y"));
							AnchorableGump anchorableGump = null;
							switch (gumpType2)
							{
							case GumpType.SpellButton:
								anchorableGump = new UseSpellButtonGump();
								break;
							case GumpType.SkillButton:
								anchorableGump = new SkillButtonGump();
								break;
							case GumpType.HealthBar:
								anchorableGump = ((!CustomBarsToggled) ? ((BaseHealthBarGump)new HealthBarGump()) : ((BaseHealthBarGump)new HealthBarGumpCustom()));
								break;
							case GumpType.AbilityButton:
								anchorableGump = new UseAbilityButtonGump();
								break;
							case GumpType.MacroButton:
								anchorableGump = new MacroButtonGump();
								break;
							}
							if (anchorableGump == null)
							{
								continue;
							}
							anchorableGump.LocalSerial = localSerial2;
							anchorableGump.Restore(item2);
							anchorableGump.X = num3;
							anchorableGump.Y = num4;
							if (!anchorableGump.IsDisposed)
							{
								if (UIManager.AnchorManager[anchorableGump] == null && anchorGroup.IsEmptyDirection(num5, num6))
								{
									list.Add(anchorableGump);
									UIManager.AnchorManager[anchorableGump] = anchorGroup;
									anchorGroup.AddControlToMatrix(num5, num6, anchorableGump);
								}
								else
								{
									anchorableGump.Dispose();
								}
							}
						}
						catch (Exception ex3)
						{
							Log.Error(ex3.ToString());
						}
					}
				}
			}
		}
		return list;
	}
}
