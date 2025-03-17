using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class OptionsGump : Gump
{
	private enum Buttons
	{
		Disabled = 0,
		Cancel = 1,
		Apply = 2,
		Default = 3,
		Ok = 4,
		SpeechColor = 5,
		EmoteColor = 6,
		PartyMessageColor = 7,
		GuildMessageColor = 8,
		AllyMessageColor = 9,
		InnocentColor = 10,
		FriendColor = 11,
		CriminalColor = 12,
		EnemyColor = 13,
		MurdererColor = 14,
		NewMacro = 15,
		DeleteMacro = 16,
		Last = 16
	}

	private class SettingsSection : Control
	{
		private readonly DataBox _databox;

		private int _indent;

		public SettingsSection(string title, int width)
		{
			CanMove = true;
			AcceptMouseInput = true;
			base.WantUpdateSize = true;
			Label label = new Label(title, isunicode: true, ushort.MaxValue);
			label.X = 5;
			base.Add(label);
			base.Add(new Line(0, label.Height, width - 30, 1, 4290428354u));
			base.Width = width;
			base.Height = label.Height + 1;
			_databox = new DataBox(label.X + 10, label.Height + 4, 0, 0);
			base.Add(_databox);
		}

		public void PushIndent()
		{
			_indent += 40;
		}

		public void PopIndent()
		{
			_indent -= 40;
		}

		public void AddRight(Control c, int offset = 15)
		{
			int num = _databox.Children.Count - 1;
			while (num >= 0 && !_databox.Children[num].IsVisible)
			{
				num--;
			}
			c.X = ((num >= 0) ? (_databox.Children[num].Bounds.Right + offset) : _indent);
			c.Y = ((num >= 0) ? _databox.Children[num].Bounds.Top : 0);
			_databox.Add(c);
			_databox.WantUpdateSize = true;
		}

		public override void Add(Control c, int page = 0)
		{
			int num = _databox.Children.Count - 1;
			int num2 = 0;
			while (num >= 0)
			{
				if (_databox.Children[num].IsVisible)
				{
					if (num2 != 0 && num2 >= _databox.Children[num].Bounds.Bottom + 2)
					{
						break;
					}
					num2 = _databox.Children[num].Bounds.Bottom + 2;
				}
				num--;
			}
			c.X = _indent;
			c.Y = num2;
			_databox.Add(c, page);
			_databox.WantUpdateSize = true;
			base.Height += c.Height + 2;
		}
	}

	private class FontSelector : Control
	{
		private readonly RadioButton[] _buttons;

		public FontSelector(int max_font, int current_font_index, string markup)
		{
			CanMove = false;
			base.CanCloseWithRightClick = false;
			int num = 0;
			_buttons = new RadioButton[max_font];
			for (byte b = 0; b < max_font; b++)
			{
				if (FontsLoader.Instance.UnicodeFontExists(b))
				{
					RadioButton[] buttons = _buttons;
					byte num2 = b;
					RadioButton radioButton = new RadioButton(0, 208, 209, markup, b, ushort.MaxValue);
					radioButton.Y = num;
					radioButton.Tag = b;
					radioButton.IsChecked = current_font_index == b;
					RadioButton c = radioButton;
					buttons[num2] = radioButton;
					Add(c);
					num += 25;
				}
			}
		}

		public byte GetSelectedFont()
		{
			for (byte b = 0; b < _buttons.Length; b++)
			{
				RadioButton radioButton = _buttons[b];
				if (radioButton != null && radioButton.IsChecked)
				{
					return b;
				}
			}
			return byte.MaxValue;
		}

		public void SetSelectedFont(int index)
		{
			if (index >= 0 && index < _buttons.Length && _buttons[index] != null)
			{
				_buttons[index].IsChecked = true;
			}
		}
	}

	private class InputField : Control
	{
		private readonly StbTextBox _textbox;

		public string Text => _textbox.Text;

		public override bool AcceptKeyboardInput
		{
			get
			{
				return _textbox.AcceptKeyboardInput;
			}
			set
			{
				_textbox.AcceptKeyboardInput = value;
			}
		}

		public bool NumbersOnly
		{
			get
			{
				return _textbox.NumbersOnly;
			}
			set
			{
				_textbox.NumbersOnly = value;
			}
		}

		public InputField(ushort backgroundGraphic, byte font, ushort hue, bool unicode, int width, int height, int maxWidthText = 0, int maxCharsCount = -1)
		{
			base.WantUpdateSize = false;
			base.Width = width;
			base.Height = height;
			ResizePic resizePic = new ResizePic(backgroundGraphic);
			resizePic.Width = width;
			resizePic.Height = height;
			ResizePic c = resizePic;
			StbTextBox stbTextBox = new StbTextBox(font, maxCharsCount, maxWidthText, unicode, FontStyle.BlackBorder, hue);
			stbTextBox.X = 4;
			stbTextBox.Y = 4;
			stbTextBox.Width = width - 8;
			stbTextBox.Height = height - 8;
			_textbox = stbTextBox;
			Add(c);
			Add(_textbox);
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (batcher.ClipBegin(x, y, base.Width, base.Height))
			{
				base.Draw(batcher, x, y);
				batcher.ClipEnd();
			}
			return true;
		}

		public void SetText(string text)
		{
			_textbox.SetText(text);
		}
	}

	private const byte FONT = byte.MaxValue;

	private const ushort HUE_FONT = ushort.MaxValue;

	private const int WIDTH = 700;

	private const int HEIGHT = 500;

	private const int TEXTBOX_HEIGHT = 25;

	private static Texture2D _logoTexture2D;

	private Combobox _auraType;

	private Combobox _filterType;

	private Combobox _autoOpenCorpseOptions;

	private InputField _autoOpenCorpseRange;

	private Checkbox _autoOpenDoors;

	private Checkbox _autoOpenCorpse;

	private Checkbox _skipEmptyCorpse;

	private Checkbox _disableTabBtn;

	private Checkbox _disableCtrlQWBtn;

	private Checkbox _disableDefaultHotkeys;

	private Checkbox _disableArrowBtn;

	private Checkbox _disableAutoMove;

	private Checkbox _overrideContainerLocation;

	private Checkbox _smoothDoors;

	private Checkbox _showTargetRangeIndicator;

	private Checkbox _customBars;

	private Checkbox _customBarsBBG;

	private Checkbox _saveHealthbars;

	private HSliderBar _cellSize;

	private Checkbox _containerScaleItems;

	private Checkbox _containerDoubleClickToLoot;

	private Checkbox _relativeDragAnDropItems;

	private Checkbox _useLargeContianersGumps;

	private Checkbox _highlightContainersWhenMouseIsOver;

	private HSliderBar _containersScale;

	private Combobox _cotType;

	private DataBox _databox;

	private HSliderBar _delay_before_display_tooltip;

	private HSliderBar _tooltip_zoom;

	private HSliderBar _tooltip_background_opacity;

	private Combobox _dragSelectModifierKey;

	private Combobox _backpackStyle;

	private Checkbox _enableCounters;

	private Checkbox _highlightOnUse;

	private Checkbox _highlightOnAmount;

	private Checkbox _enableAbbreviatedAmount;

	private Checkbox _enableDragSelect;

	private Checkbox _dragSelectHumanoidsOnly;

	private Checkbox _enableSounds;

	private Checkbox _enableMusic;

	private Checkbox _footStepsSound;

	private Checkbox _combatMusic;

	private Checkbox _musicInBackground;

	private Checkbox _loginMusic;

	private Checkbox _enableHeartbeatSound;

	private FontSelector _fontSelectorChat;

	private Checkbox _forceUnicodeJournal;

	private Checkbox _showJournalEntryTime;

	private Checkbox _showYouSeeEntriesHostile;

	private Checkbox _showYouSeeEntriesNeutral;

	private Checkbox _showYouSeeEntriesItems;

	private Checkbox _showAdditionalJournalSelf;

	private Checkbox _showAdditionalJournalSystemMessages;

	private InputField _gameWindowHeight;

	private Combobox _journalSkin;

	private Checkbox _gameWindowLock;

	private Checkbox _gameWindowFullsize;

	private InputField _gameWindowPositionX;

	private InputField _gameWindowPositionY;

	private InputField _gameWindowWidth;

	private Combobox _gridLoot;

	private Checkbox _hideScreenshotStoredInMessage;

	private Checkbox _highlightObjects;

	private Checkbox _enablePathfind;

	private Checkbox _useShiftPathfind;

	private Checkbox _alwaysRun;

	private Checkbox _alwaysWalk;

	private Checkbox _alwaysRunUnlessHidden;

	private Checkbox _showHpMobile;

	private Checkbox _showTargetFrame;

	private Checkbox _targetWithoutWarmode;

	private Checkbox _showTargetOverheadMessage;

	private Checkbox _highlightByPoisoned;

	private Checkbox _highlightByParalyzed;

	private Checkbox _highlightByInvul;

	private Checkbox _drawRoofs;

	private Checkbox _treeToStumps;

	private Checkbox _hideVegetation;

	private Checkbox _enableCaveBorder;

	private Checkbox _noColorOutOfRangeObjects;

	private Checkbox _useCircleOfTransparency;

	private Checkbox _enableTopbar;

	private Checkbox _holdDownKeyTab;

	private Checkbox _holdDownKeyAlt;

	private Checkbox _closeAllAnchoredGumpsWithRClick;

	private Checkbox _chatAfterEnter;

	private Checkbox _chatAdditionalButtonsCheckbox;

	private Checkbox _chatShiftEnterCheckbox;

	private Checkbox _holdShiftForContext;

	private Checkbox _holdShiftToSplitStack;

	private Checkbox _reduceFPSWhenInactive;

	private Checkbox _sallosEasyGrab;

	private Checkbox _partyInviteGump;

	private Checkbox _objectsFading;

	private Checkbox _textFading;

	private Checkbox _holdAltToMoveGumps;

	private Combobox _hpComboBox;

	private Combobox _healtbarType;

	private Combobox _fieldsType;

	private Combobox _hpComboBoxShowWhen;

	private List<InfoBarBuilderControl> _infoBarBuilderControls;

	private Combobox _infoBarHighlightType;

	private ClickableColorBox _innocentColorPickerBox;

	private ClickableColorBox _friendColorPickerBox;

	private ClickableColorBox _crimialColorPickerBox;

	private ClickableColorBox _canAttackColorPickerBox;

	private ClickableColorBox _enemyColorPickerBox;

	private ClickableColorBox _murdererColorPickerBox;

	private ClickableColorBox _neutralColorPickerBox;

	private ClickableColorBox _beneficColorPickerBox;

	private ClickableColorBox _harmfulColorPickerBox;

	private HSliderBar _lightBar;

	private Checkbox _buffBarTime;

	private Checkbox _buffBlinks;

	private Checkbox _castSpellsByOneClick;

	private Checkbox _queryBeforAttackCheckbox;

	private Checkbox _queryBeforeBeneficialCheckbox;

	private Checkbox _spellColoringCheckbox;

	private Checkbox _spellFormatCheckbox;

	private Checkbox _enableFastSpellsAssign;

	private MacroControl _macroControl;

	private Checkbox _overrideAllFonts;

	private Combobox _overrideAllFontsIsUnicodeCheckbox;

	private Combobox _overrideContainerLocationSetting;

	private ClickableColorBox _poisonColorPickerBox;

	private ClickableColorBox _paralyzedColorPickerBox;

	private ClickableColorBox _invulnerableColorPickerBox;

	private NiceButton _randomizeColorsButton;

	private Checkbox _restorezoomCheckbox;

	private Checkbox _zoomCheckbox;

	private InputField _rows;

	private InputField _columns;

	private InputField _highlightAmount;

	private InputField _abbreviatedAmount;

	private Checkbox _scaleSpeechDelay;

	private Checkbox _saveJournalCheckBox;

	private Checkbox _autoAcceptInvitesOfGuildMembers;

	private Checkbox _showHouseContent;

	private Checkbox _showInfoBar;

	private Checkbox _ignoreAllianceMessages;

	private Checkbox _ignoreGuildMessages;

	private Checkbox _showChatActivity;

	private Checkbox _showSystemChatMessagesInGameWindowBox;

	private Combobox _chatActivitySize_combobox;

	private InputField _chatPositionX;

	private InputField _chatPositionY;

	private HSliderBar _sliderFPS;

	private HSliderBar _circleOfTranspRadius;

	private HSliderBar _sliderSpeechDelay;

	private HSliderBar _sliderZoom;

	private HSliderBar _soundsVolume;

	private HSliderBar _animalVolume;

	private HSliderBar _bardVolume;

	private HSliderBar _musicVolume;

	private HSliderBar _loginMusicVolume;

	private HSliderBar _heartbeatThreshhold;

	private HSliderBar _heartbeatVolume;

	private ClickableColorBox _highlightGameObjectsColor;

	private ClickableColorBox _speechColorPickerBox;

	private ClickableColorBox _emoteColorPickerBox;

	private ClickableColorBox _yellColorPickerBox;

	private ClickableColorBox _whisperColorPickerBox;

	private ClickableColorBox _partyMessageColorPickerBox;

	private ClickableColorBox _guildMessageColorPickerBox;

	private ClickableColorBox _allyMessageColorPickerBox;

	private ClickableColorBox _chatMessageColorPickerBox;

	private ClickableColorBox _partyAuraColorPickerBox;

	private InputField _spellFormatBox;

	private ClickableColorBox _tooltip_font_hue;

	private FontSelector _tooltip_font_selector;

	private HSliderBar _dragSelectStartX;

	private HSliderBar _dragSelectStartY;

	private Checkbox _dragSelectAsAnchor;

	private Checkbox _preventAltF4CloseClient;

	private Combobox _paperdollGump;

	private Combobox _statusBarGump;

	private Checkbox _windowBorderless;

	private Checkbox _altLights;

	private Checkbox _enableLight;

	private Checkbox _enableShadows;

	private Checkbox _enableShadowsStatics;

	private Checkbox _auraMouse;

	private Checkbox _activateShakeEffects;

	private Checkbox _runMouseInSeparateThread;

	private Checkbox _useColoredLights;

	private Checkbox _darkNights;

	private Checkbox _partyAura;

	private Checkbox _hideChatGradient;

	private Checkbox _use_smooth_boat_movement;

	private HSliderBar _terrainShadowLevel;

	private Checkbox _use_tooltip;

	private Checkbox _useStandardSkillsGump;

	private Checkbox _showMobileNameIncoming;

	private Checkbox _showCorpseNameIncoming;

	private Checkbox _showStatsMessage;

	private Checkbox _showSkillsMessage;

	private HSliderBar _showSkillsMessageDelta;

	private Profile _currentProfile = ProfileManager.CurrentProfile;

	private static Texture2D LogoTexture
	{
		get
		{
			if (_logoTexture2D == null || _logoTexture2D.IsDisposed)
			{
				Stream manifestResourceStream = typeof(CUOEnviroment).Assembly.GetManifestResourceStream("ClassicUO.cuologo.png");
				_logoTexture2D = Texture2D.FromStream(Client.Game.GraphicsDevice, manifestResourceStream);
			}
			return _logoTexture2D;
		}
	}

	public OptionsGump()
		: base(0u, 0u)
	{
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.95f);
		alphaBlendControl.X = 1;
		alphaBlendControl.Y = 1;
		alphaBlendControl.Width = 698;
		alphaBlendControl.Height = 498;
		alphaBlendControl.Hue = 999;
		Add(alphaBlendControl);
		int num = 0;
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.General)
		{
			IsSelected = true,
			ButtonParameter = 1
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Sound)
		{
			ButtonParameter = 2
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Video)
		{
			ButtonParameter = 3
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Macros)
		{
			ButtonParameter = 4
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Tooltip)
		{
			ButtonParameter = 5
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Fonts)
		{
			ButtonParameter = 6
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Speech)
		{
			ButtonParameter = 7
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.CombatSpells)
		{
			ButtonParameter = 8
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Counters)
		{
			ButtonParameter = 9
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.InfoBar)
		{
			ButtonParameter = 10
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Containers)
		{
			ButtonParameter = 11
		});
		Add(new NiceButton(10, 10 + 30 * num++, 140, 25, ButtonAction.Default, ResGumps.Experimental)
		{
			ButtonParameter = 12
		});
		Add(new Line(160, 5, 1, 490, Color.Gray.PackedValue));
		int num2 = 60;
		int num3 = 60;
		Add(new Line(160, 441, 540, 1, Color.Gray.PackedValue));
		Button button = new Button(1, 243, 241, 242, "", 0);
		button.X = 154 + num2;
		button.Y = 405 + num3;
		button.ButtonAction = ButtonAction.Activate;
		Add(button);
		Button button2 = new Button(2, 239, 240, 238, "", 0);
		button2.X = 248 + num2;
		button2.Y = 405 + num3;
		button2.ButtonAction = ButtonAction.Activate;
		Add(button2);
		Button button3 = new Button(3, 246, 244, 245, "", 0);
		button3.X = 346 + num2;
		button3.Y = 405 + num3;
		button3.ButtonAction = ButtonAction.Activate;
		Add(button3);
		Button button4 = new Button(4, 249, 248, 247, "", 0);
		button4.X = 443 + num2;
		button4.Y = 405 + num3;
		button4.ButtonAction = ButtonAction.Activate;
		Add(button4);
		AcceptMouseInput = true;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		BuildGeneral();
		BuildSounds();
		BuildVideo();
		BuildCommands();
		BuildFonts();
		BuildSpeech();
		BuildCombat();
		BuildTooltip();
		BuildCounters();
		BuildInfoBar();
		BuildContainers();
		BuildExperimental();
		ChangePage(1);
	}

	private void BuildGeneral()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int x = 5;
		int y = 5;
		DataBox dataBox = new DataBox(x, y, scrollArea.Width - 15, 1);
		dataBox.WantUpdateSize = true;
		scrollArea.Add(dataBox);
		SettingsSection settingsSection = AddSettingsSection(dataBox, ResGumps.General);
		settingsSection.Add(_highlightObjects = AddCheckBox(null, ResGumps.HighlightObjects, _currentProfile.HighlightGameObjects, x, y));
		settingsSection.Add(_highlightGameObjectsColor = AddColorBox(null, x, y, _currentProfile.HighlightGameObjectsColor, ResGumps.HighlightObjectsColor));
		settingsSection.AddRight(AddLabel(null, ResGumps.HighlightObjectsColor, 0, 0), 2);
		settingsSection.Add(_enablePathfind = AddCheckBox(null, ResGumps.EnablePathfinding, _currentProfile.EnablePathfind, x, y));
		settingsSection.AddRight(_useShiftPathfind = AddCheckBox(null, ResGumps.ShiftPathfinding, _currentProfile.UseShiftToPathfind, x, y));
		settingsSection.Add(_alwaysRun = AddCheckBox(null, ResGumps.AlwaysRun, _currentProfile.AlwaysRun, x, y));
		settingsSection.AddRight(_alwaysRunUnlessHidden = AddCheckBox(null, ResGumps.AlwaysRunHidden, _currentProfile.AlwaysRunUnlessHidden, x, y));
		settingsSection.AddRight(_smoothDoors = AddCheckBox(null, ResGumps.SmoothDoors, _currentProfile.SmoothDoors, x, y));
		settingsSection.Add(_alwaysWalk = AddCheckBox(null, ResGumps.AlwaysWalk, _currentProfile.AlwaysWalk, x, y));
		settingsSection.Add(_autoOpenCorpse = AddCheckBox(null, ResGumps.AutoOpenCorpses, _currentProfile.AutoOpenCorpses, x, y));
		settingsSection.PushIndent();
		settingsSection.Add(AddLabel(null, ResGumps.CorpseOpenRange, 0, 0));
		settingsSection.AddRight(_autoOpenCorpseRange = AddInputField(null, x, y, 50, 25, ResGumps.CorpseOpenRange, 50, set_down: false, numbersOnly: true, 5));
		_autoOpenCorpseRange.SetText(_currentProfile.AutoOpenCorpseRange.ToString());
		settingsSection.Add(_skipEmptyCorpse = AddCheckBox(null, ResGumps.SkipEmptyCorpses, _currentProfile.SkipEmptyCorpse, x, y));
		settingsSection.Add(AddLabel(null, ResGumps.CorpseOpenOptions, x, y));
		settingsSection.AddRight(_autoOpenCorpseOptions = AddCombobox(null, new string[4]
		{
			ResGumps.CorpseOpt_None,
			ResGumps.CorpseOpt_NotTar,
			ResGumps.CorpseOpt_NotHid,
			ResGumps.CorpseOpt_Both
		}, _currentProfile.CorpseOpenOptions, x, y, 150), 2);
		settingsSection.PopIndent();
		settingsSection.Add(_noColorOutOfRangeObjects = AddCheckBox(scrollArea, ResGumps.OutOfRangeColor, _currentProfile.NoColorObjectsOutOfRange, x, y));
		settingsSection.Add(_showHouseContent = AddCheckBox(null, ResGumps.ShowHousesContent, _currentProfile.ShowHouseContent, x, y));
		_showHouseContent.IsVisible = Client.Version >= ClientVersion.CV_70796;
		settingsSection.Add(_use_smooth_boat_movement = AddCheckBox(null, ResGumps.SmoothBoat, _currentProfile.UseSmoothBoatMovement, x, y));
		_use_smooth_boat_movement.IsVisible = false;
		SettingsSection settingsSection2 = AddSettingsSection(dataBox, ResGumps.Mobiles);
		settingsSection2.Y = settingsSection.Bounds.Bottom + 40;
		settingsSection2.Add(_showHpMobile = AddCheckBox(null, ResGumps.ShowHP, _currentProfile.ShowMobilesHP, x, y));
		int num = _currentProfile.MobileHPType;
		if (num < 0 || num > 2)
		{
			num = 0;
		}
		settingsSection2.AddRight(_hpComboBox = AddCombobox(null, new string[3]
		{
			ResGumps.HP_Percentage,
			ResGumps.HP_Line,
			ResGumps.HP_Both
		}, num, x, y, 100));
		settingsSection2.AddRight(AddLabel(null, ResGumps.HP_Mode, x, y));
		num = _currentProfile.MobileHPShowWhen;
		if (num != 0 && num > 2)
		{
			num = 0;
		}
		settingsSection2.AddRight(_hpComboBoxShowWhen = AddCombobox(null, new string[3]
		{
			ResGumps.HPShow_Always,
			ResGumps.HPShow_Less,
			ResGumps.HPShow_Smart
		}, num, x, y, 100), 2);
		settingsSection2.Add(_showTargetFrame = AddCheckBox(null, ResGumps.ShowTargetFrame, _currentProfile.ShowTargetFrames, x, y));
		settingsSection2.Add(_targetWithoutWarmode = AddCheckBox(null, ResGumps.TargetWithoutWarmode, _currentProfile.EnableTargetWithoutWarmode, x, y));
		settingsSection2.Add(_showTargetOverheadMessage = AddCheckBox(null, ResGumps.TargetOverheadMessage, _currentProfile.ShowTargetingOverheadMessage, x, y));
		settingsSection2.Add(_highlightByPoisoned = AddCheckBox(null, ResGumps.HighlightPoisoned, _currentProfile.HighlightMobilesByPoisoned, x, y));
		settingsSection2.PushIndent();
		settingsSection2.Add(_poisonColorPickerBox = AddColorBox(null, x, y, _currentProfile.PoisonHue, ResGumps.PoisonedColor));
		settingsSection2.AddRight(AddLabel(null, ResGumps.PoisonedColor, 0, 0), 2);
		settingsSection2.PopIndent();
		settingsSection2.Add(_highlightByParalyzed = AddCheckBox(null, ResGumps.HighlightParalyzed, _currentProfile.HighlightMobilesByParalize, x, y));
		settingsSection2.PushIndent();
		settingsSection2.Add(_paralyzedColorPickerBox = AddColorBox(null, x, y, _currentProfile.ParalyzedHue, ResGumps.ParalyzedColor));
		settingsSection2.AddRight(AddLabel(null, ResGumps.ParalyzedColor, 0, 0), 2);
		settingsSection2.PopIndent();
		settingsSection2.Add(_highlightByInvul = AddCheckBox(null, ResGumps.HighlightInvulnerable, _currentProfile.HighlightMobilesByInvul, x, y));
		settingsSection2.PushIndent();
		settingsSection2.Add(_invulnerableColorPickerBox = AddColorBox(null, x, y, _currentProfile.InvulnerableHue, ResGumps.InvulColor));
		settingsSection2.AddRight(AddLabel(null, ResGumps.InvulColor, 0, 0), 2);
		settingsSection2.PopIndent();
		settingsSection2.Add(AddLabel(null, ResGumps.AuraUnderFeet, x, y));
		settingsSection2.AddRight(_auraType = AddCombobox(null, new string[4]
		{
			ResGumps.AuraType_None,
			ResGumps.AuraType_Warmode,
			ResGumps.AuraType_CtrlShift,
			ResGumps.AuraType_Always
		}, _currentProfile.AuraUnderFeetType, x, y, 100), 2);
		settingsSection2.PushIndent();
		settingsSection2.Add(_partyAura = AddCheckBox(null, ResGumps.CustomColorAuraForPartyMembers, _currentProfile.PartyAura, x, y));
		settingsSection2.PushIndent();
		settingsSection2.Add(_partyAuraColorPickerBox = AddColorBox(null, x, y, _currentProfile.PartyAuraHue, ResGumps.PartyAuraColor));
		settingsSection2.AddRight(AddLabel(null, ResGumps.PartyAuraColor, 0, 0));
		settingsSection2.PopIndent();
		settingsSection2.PopIndent();
		SettingsSection settingsSection3 = AddSettingsSection(dataBox, ResGumps.GumpsAndContext);
		settingsSection3.Y = settingsSection2.Bounds.Bottom + 40;
		settingsSection3.Add(_enableTopbar = AddCheckBox(null, ResGumps.DisableMenu, _currentProfile.TopbarGumpIsDisabled, 0, 0));
		settingsSection3.Add(_holdDownKeyAlt = AddCheckBox(null, ResGumps.AltCloseGumps, _currentProfile.HoldDownKeyAltToCloseAnchored, 0, 0));
		settingsSection3.Add(_holdAltToMoveGumps = AddCheckBox(null, ResGumps.AltMoveGumps, _currentProfile.HoldAltToMoveGumps, 0, 0));
		settingsSection3.Add(_closeAllAnchoredGumpsWithRClick = AddCheckBox(null, ResGumps.ClickCloseAllGumps, _currentProfile.CloseAllAnchoredGumpsInGroupWithRightClick, 0, 0));
		settingsSection3.Add(_useStandardSkillsGump = AddCheckBox(null, ResGumps.StandardSkillGump, _currentProfile.StandardSkillsGump, 0, 0));
		settingsSection3.Add(_partyInviteGump = AddCheckBox(null, ResGumps.ShowGumpPartyInv, _currentProfile.PartyInviteGump, 0, 0));
		settingsSection3.Add(_customBars = AddCheckBox(null, ResGumps.UseCustomHPBars, _currentProfile.CustomBarsToggled, 0, 0));
		settingsSection3.Add(_saveHealthbars = AddCheckBox(null, ResGumps.SaveHPBarsOnLogout, _currentProfile.SaveHealthbars, 0, 0));
		settingsSection3.PushIndent();
		settingsSection3.Add(AddLabel(null, ResGumps.CloseHPGumpWhen, 0, 0));
		num = _currentProfile.CloseHealthBarType;
		if (num < 0 || num > 2)
		{
			num = 0;
		}
		_healtbarType = AddCombobox(null, new string[3]
		{
			ResGumps.HPType_None,
			ResGumps.HPType_MobileOOR,
			ResGumps.HPType_MobileDead
		}, num, 0, 0, 150);
		settingsSection3.AddRight(_healtbarType);
		settingsSection3.PopIndent();
		settingsSection3.Add(AddLabel(null, ResGumps.GridLoot, x, y));
		settingsSection3.AddRight(_gridLoot = AddCombobox(null, new string[3]
		{
			ResGumps.GridLoot_None,
			ResGumps.GridLoot_GridOnly,
			ResGumps.GridLoot_Both
		}, _currentProfile.GridLootType, x, y, 120), 2);
		settingsSection3.Add(_holdShiftForContext = AddCheckBox(null, ResGumps.ShiftContext, _currentProfile.HoldShiftForContext, 0, 0));
		settingsSection3.Add(_holdShiftToSplitStack = AddCheckBox(null, ResGumps.ShiftStack, _currentProfile.HoldShiftToSplitStack, 0, 0));
		settingsSection3.Add(AddLabel(null, "Paperdoll: ", x, y));
		settingsSection3.AddRight(_paperdollGump = AddCombobox(null, new string[10] { "Klassisch", "Neutral", "Licht", "Dunkel", "Elf", "Lethar", "Menekaner", "Rashar", "Thyre", "Zwerg" }, _currentProfile.PaperdollGump, x, y, 120), 2);
		settingsSection3.Add(AddLabel(null, "Statusanzeige: ", x, y));
		settingsSection3.AddRight(_statusBarGump = AddCombobox(null, new string[10] { "Klassisch", "Neutral", "Licht", "Dunkel", "Elf", "Lethar", "Menekaner", "Rashar", "Thyre", "Zwerg" }, _currentProfile.StatusBarGump, x, y, 120), 2);
		SettingsSection settingsSection4 = AddSettingsSection(dataBox, ResGumps.Miscellaneous);
		settingsSection4.Y = settingsSection3.Bounds.Bottom + 40;
		settingsSection4.Add(_useCircleOfTransparency = AddCheckBox(null, ResGumps.EnableCircleTrans, _currentProfile.UseCircleOfTransparency, x, y));
		settingsSection4.AddRight(_circleOfTranspRadius = AddHSlider(null, 50, 200, _currentProfile.CircleOfTransparencyRadius, x, y, 200));
		settingsSection4.PushIndent();
		int circleOfTransparencyType = _currentProfile.CircleOfTransparencyType;
		string[] array = new string[2]
		{
			ResGumps.CircleTransType_Full,
			ResGumps.CircleTransType_Gradient
		};
		if (circleOfTransparencyType < 0 || circleOfTransparencyType > array.Length)
		{
			circleOfTransparencyType = 0;
		}
		settingsSection4.PopIndent();
		settingsSection4.Add(_hideScreenshotStoredInMessage = AddCheckBox(null, ResGumps.HideScreenshotStoredInMessage, _currentProfile.HideScreenshotStoredInMessage, 0, 0));
		settingsSection4.Add(_objectsFading = AddCheckBox(null, ResGumps.ObjAlphaFading, _currentProfile.UseObjectsFading, x, y));
		settingsSection4.Add(_textFading = AddCheckBox(null, ResGumps.TextAlphaFading, _currentProfile.TextFading, x, y));
		settingsSection4.Add(_showTargetRangeIndicator = AddCheckBox(null, ResGumps.ShowTarRangeIndic, _currentProfile.ShowTargetRangeIndicator, x, y));
		settingsSection4.Add(_enableDragSelect = AddCheckBox(null, ResGumps.EnableDragHPBars, _currentProfile.EnableDragSelect, x, y));
		settingsSection4.PushIndent();
		settingsSection4.Add(AddLabel(null, ResGumps.DragKey, x, y));
		settingsSection4.AddRight(_dragSelectModifierKey = AddCombobox(null, new string[3]
		{
			ResGumps.KeyMod_None,
			ResGumps.KeyMod_Ctrl,
			ResGumps.KeyMod_Shift
		}, _currentProfile.DragSelectModifierKey, x, y, 100));
		settingsSection4.Add(_dragSelectHumanoidsOnly = AddCheckBox(null, ResGumps.DragHumanoidsOnly, _currentProfile.DragSelectHumanoidsOnly, x, y));
		settingsSection4.Add(new Label(ResGumps.DragSelectStartingPosX, isunicode: true, ushort.MaxValue));
		settingsSection4.Add(_dragSelectStartX = new HSliderBar(x, y, 200, 0, _currentProfile.GameWindowSize.X, _currentProfile.DragSelectStartX, HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, ushort.MaxValue));
		settingsSection4.Add(new Label(ResGumps.DragSelectStartingPosY, isunicode: true, ushort.MaxValue));
		settingsSection4.Add(_dragSelectStartY = new HSliderBar(x, y, 200, 0, _currentProfile.GameWindowSize.Y, _currentProfile.DragSelectStartY, HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, ushort.MaxValue));
		settingsSection4.Add(_dragSelectAsAnchor = AddCheckBox(null, ResGumps.DragSelectAnchoredHB, _currentProfile.DragSelectAsAnchor, x, y));
		settingsSection4.PopIndent();
		settingsSection4.Add(_showStatsMessage = AddCheckBox(null, ResGumps.ShowStatsChangedMessage, _currentProfile.ShowStatsChangedMessage, x, y));
		settingsSection4.Add(_showSkillsMessage = AddCheckBox(null, ResGumps.ShowSkillsChangedMessageBy, _currentProfile.ShowStatsChangedMessage, x, y));
		settingsSection4.PushIndent();
		settingsSection4.AddRight(_showSkillsMessageDelta = AddHSlider(null, 0, 100, _currentProfile.ShowSkillsChangedDeltaValue, x, y, 200));
		settingsSection4.PopIndent();
		settingsSection4.Add(_enableCaveBorder = AddCheckBox(null, ResGumps.MarkCaveTiles, _currentProfile.EnableCaveBorder, x, y));
		settingsSection4.Add(AddLabel(null, ResGumps.HPFields, x, y));
		num = _currentProfile.FieldsType;
		if (num < 0 || num > 2)
		{
			num = 0;
		}
		settingsSection4.AddRight(_fieldsType = AddCombobox(null, new string[3]
		{
			ResGumps.HPFields_Normal,
			ResGumps.HPFields_Static,
			ResGumps.HPFields_Tile
		}, num, x, y, 150));
		Add(scrollArea, 1);
	}

	private void BuildSounds()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int x = 5;
		int num = 5;
		_enableSounds = AddCheckBox(scrollArea, ResGumps.Sounds, _currentProfile.EnableSound, x, num);
		Label label = AddLabel(scrollArea, "Tiergeräusche", x, num + _enableSounds.Height + 2);
		Label label2 = AddLabel(scrollArea, "Bardengeräusche", x, num + _enableSounds.Height + 2 + label.Height + 2);
		_enableMusic = AddCheckBox(scrollArea, ResGumps.Music, _currentProfile.EnableMusic, x, num + label.Height + label2.Height + _enableSounds.Height + 2);
		_loginMusic = AddCheckBox(scrollArea, ResGumps.LoginMusic, Settings.GlobalSettings.LoginMusic, x, num + label.Height + label2.Height + _enableSounds.Height + 2 + _enableMusic.Height + 2);
		_enableHeartbeatSound = AddCheckBox(scrollArea, ResGumps.HeartbeatSound, _currentProfile.HeartbeatSound, x, num + label.Height + label2.Height + _enableSounds.Height + 2 + _enableMusic.Height + 2 + _loginMusic.Height + 2);
		x = 120;
		num += 2;
		_soundsVolume = AddHSlider(scrollArea, 0, 100, _currentProfile.SoundVolume, x, num, 200);
		_animalVolume = AddHSlider(scrollArea, 0, 100, _currentProfile.AnimalSoundsVolume, x, num + _enableSounds.Height + 2, 200);
		_bardVolume = AddHSlider(scrollArea, 0, 100, _currentProfile.BardSoundsVolume, x, num + _enableSounds.Height + label.Height + 2, 200);
		_musicVolume = AddHSlider(scrollArea, 0, 100, _currentProfile.MusicVolume, x, num + _enableSounds.Height + label.Height + label2.Height + 2, 200);
		_loginMusicVolume = AddHSlider(scrollArea, 0, 100, Settings.GlobalSettings.LoginMusicVolume, x, num + _enableSounds.Height + label.Height + label2.Height + 2 + _enableMusic.Height + 2, 200);
		Label label3 = AddLabel(scrollArea, ResGumps.HeartbeatThreshhold, _enableHeartbeatSound.Bounds.Left, _enableHeartbeatSound.Bounds.Bottom + 2);
		Label label4 = AddLabel(scrollArea, "Herzschlaglautstärke", _enableHeartbeatSound.Bounds.Left, num + label3.Bounds.Bottom + 15);
		_heartbeatThreshhold = AddHSlider(scrollArea, 0, 100, _currentProfile.HeartbeatThreshold, x + 5, label3.Bounds.Bottom + 2, 200);
		_heartbeatVolume = AddHSlider(scrollArea, 0, 100, _currentProfile.HeartbeatVolume, x + 5, _heartbeatThreshhold.Bounds.Bottom + 10, 200);
		x = 5;
		num = _heartbeatThreshhold.Bounds.Bottom + label4.Height + 4;
		_footStepsSound = AddCheckBox(scrollArea, ResGumps.PlayFootsteps, _currentProfile.EnableFootstepsSound, x, num);
		num += _footStepsSound.Height + 2;
		_combatMusic = AddCheckBox(scrollArea, ResGumps.CombatMusic, _currentProfile.EnableCombatMusic, x, num);
		num += _combatMusic.Height + 2;
		_musicInBackground = AddCheckBox(scrollArea, ResGumps.ReproduceSoundsAndMusic, _currentProfile.ReproduceSoundsInBackground, x, num);
		num += _musicInBackground.Height + 2;
		Add(scrollArea, 2);
	}

	private void BuildVideo()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		Label label = AddLabel(scrollArea, ResGumps.FPS, num, num2);
		num += label.Bounds.Right + 5;
		_sliderFPS = AddHSlider(scrollArea, 12, 250, Settings.GlobalSettings.FPS, num, num2, 250);
		num2 += label.Bounds.Bottom + 5;
		_reduceFPSWhenInactive = AddCheckBox(scrollArea, ResGumps.FPSInactive, _currentProfile.ReduceFPSWhenInactive, num, num2);
		num2 += _reduceFPSWhenInactive.Height + 2;
		num = 5;
		num2 += 20;
		DataBox dataBox = new DataBox(num, num2, scrollArea.Width - 15, 1);
		dataBox.WantUpdateSize = true;
		scrollArea.Add(dataBox);
		SettingsSection settingsSection = AddSettingsSection(dataBox, ResGumps.GameWindow);
		settingsSection.Add(_gameWindowFullsize = AddCheckBox(null, ResGumps.AlwaysUseFullsizeGameWindow, _currentProfile.GameWindowFullSize, num, num2));
		settingsSection.Add(_windowBorderless = AddCheckBox(null, ResGumps.BorderlessWindow, _currentProfile.WindowBorderless, num, num2));
		settingsSection.Add(_gameWindowLock = AddCheckBox(null, ResGumps.LockGameWindowMovingResizing, _currentProfile.GameWindowLock, num, num2));
		settingsSection.Add(AddLabel(null, ResGumps.GamePlayWindowPosition, num, num2));
		settingsSection.AddRight(_gameWindowPositionX = AddInputField(null, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true), 4);
		_gameWindowPositionX.SetText(_currentProfile.GameWindowPosition.X.ToString());
		settingsSection.AddRight(_gameWindowPositionY = AddInputField(null, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true));
		_gameWindowPositionY.SetText(_currentProfile.GameWindowPosition.Y.ToString());
		settingsSection.Add(AddLabel(null, ResGumps.GamePlayWindowSize, num, num2));
		settingsSection.AddRight(_gameWindowWidth = AddInputField(null, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true));
		_gameWindowWidth.SetText(_currentProfile.GameWindowSize.X.ToString());
		settingsSection.AddRight(_gameWindowHeight = AddInputField(null, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true));
		_gameWindowHeight.SetText(_currentProfile.GameWindowSize.Y.ToString());
		SettingsSection settingsSection2 = AddSettingsSection(dataBox, "Zoom");
		settingsSection2.Y = settingsSection.Bounds.Bottom + 40;
		settingsSection2.Add(AddLabel(null, ResGumps.DefaultZoom, num, num2));
		settingsSection2.AddRight(_sliderZoom = AddHSlider(null, 0, Client.Game.Scene.Camera.ZoomValuesCount, Client.Game.Scene.Camera.ZoomIndex, num, num2, 100));
		settingsSection2.Add(_zoomCheckbox = AddCheckBox(null, ResGumps.EnableMouseWheelForZoom, _currentProfile.EnableMousewheelScaleZoom, num, num2));
		settingsSection2.Add(_restorezoomCheckbox = AddCheckBox(null, ResGumps.ReleasingCtrlRestoresScale, _currentProfile.RestoreScaleAfterUnpressCtrl, num, num2));
		SettingsSection settingsSection3 = AddSettingsSection(dataBox, "Lichter");
		settingsSection3.Y = settingsSection2.Bounds.Bottom + 40;
		settingsSection3.Add(_darkNights = AddCheckBox(null, ResGumps.DarkNights, _currentProfile.UseDarkNights, num, num2));
		SettingsSection settingsSection4 = AddSettingsSection(dataBox, ResGumps.Miscellaneous);
		settingsSection4.Y = settingsSection3.Bounds.Bottom + 40;
		settingsSection4.Add(_runMouseInSeparateThread = AddCheckBox(null, ResGumps.RunMouseInASeparateThread, Settings.GlobalSettings.RunMouseInASeparateThread, num, num2));
		settingsSection4.Add(_auraMouse = AddCheckBox(null, ResGumps.AuraOnMouseTarget, _currentProfile.AuraOnMouse, num, num2));
		settingsSection4.Add(_activateShakeEffects = AddCheckBox(null, ResGumps.ActivateShakeEffects, _currentProfile.ActivateShakeEffects, num, num2));
		SettingsSection settingsSection5 = AddSettingsSection(dataBox, ResGumps.Shadows);
		settingsSection5.Y = settingsSection4.Bounds.Bottom + 40;
		settingsSection5.Add(_enableShadows = AddCheckBox(null, ResGumps.Shadows, _currentProfile.ShadowsEnabled, num, num2));
		settingsSection5.PushIndent();
		settingsSection5.Add(_enableShadowsStatics = AddCheckBox(null, ResGumps.ShadowStatics, _currentProfile.ShadowsStatics, num, num2));
		settingsSection5.PopIndent();
		settingsSection5.Add(AddLabel(null, ResGumps.TerrainShadowsLevel, num, num2));
		settingsSection5.AddRight(_terrainShadowLevel = AddHSlider(null, 5, 25, _currentProfile.TerrainShadowsLevel, num, num2, 200));
		SettingsSection settingsSection6 = AddSettingsSection(dataBox, "Filters");
		settingsSection6.Y = settingsSection5.Bounds.Bottom + 40;
		settingsSection6.Add(AddLabel(null, ResGumps.FilterType, num, num2));
		settingsSection6.AddRight(_filterType = AddCombobox(null, new string[3]
		{
			ResGumps.OFF,
			string.Format(ResGumps.FilterTypeFormatON, ResGumps.ON, ResGumps.AnisotropicClamp),
			string.Format(ResGumps.FilterTypeFormatON, ResGumps.ON, ResGumps.LinearClamp)
		}, _currentProfile.FilterType, num, num2, 200));
		Add(scrollArea, 3);
	}

	private void BuildCommands()
	{
		ScrollArea rightArea = new ScrollArea(190, 81, 150, 360, normalScrollbar: true);
		Add(new Line(190, 79, 150, 1, Color.Gray.PackedValue), 4);
		Add(new Line(341, 21, 1, 418, Color.Gray.PackedValue), 4);
		NiceButton niceButton = new NiceButton(190, 20, 130, 20, ButtonAction.Activate, ResGumps.NewMacro)
		{
			IsSelectable = false,
			ButtonParameter = 15
		};
		Add(niceButton, 4);
		NiceButton niceButton2 = new NiceButton(190, 52, 130, 20, ButtonAction.Activate, ResGumps.DeleteMacro)
		{
			IsSelectable = false,
			ButtonParameter = 16
		};
		Add(niceButton2, 4);
		int x = 5;
		int y = 5;
		DataBox databox = new DataBox(x, y, 1, 1);
		databox.WantUpdateSize = true;
		rightArea.Add(databox);
		niceButton.MouseUp += delegate
		{
			UIManager.Add(new EntryDialog(250, 150, ResGumps.MacroName, delegate(string name)
			{
				if (!string.IsNullOrWhiteSpace(name))
				{
					MacroManager macros = Client.Game.GetScene<GameScene>().Macros;
					if (macros.FindMacro(name) == null)
					{
						DataBox dataBox = databox;
						NiceButton obj = new NiceButton(0, 0, 130, 25, ButtonAction.Activate, name)
						{
							ButtonParameter = 17 + rightArea.Children.Count
						};
						NiceButton c = obj;
						NiceButton nb = obj;
						dataBox.Add(c);
						databox.ReArrangeChildren();
						nb.IsSelected = true;
						_macroControl?.Dispose();
						OptionsGump optionsGump = this;
						MacroControl macroControl = new MacroControl(name);
						macroControl.X = 400;
						macroControl.Y = 20;
						optionsGump._macroControl = macroControl;
						macros.PushToBack(_macroControl.Macro);
						Add(_macroControl, 4);
						nb.DragBegin += delegate
						{
							if (!UIManager.IsDragging && Math.Max(Math.Abs(Mouse.LDragOffset.X), Math.Abs(Mouse.LDragOffset.Y)) >= 5 && nb.ScreenCoordinateX <= Mouse.LClickPosition.X && nb.ScreenCoordinateX >= Mouse.LClickPosition.X - nb.Width && nb.ScreenCoordinateY <= Mouse.LClickPosition.Y && nb.ScreenCoordinateY + nb.Height >= Mouse.LClickPosition.Y)
							{
								MacroControl control = _macroControl.FindControls<MacroControl>().SingleOrDefault();
								if (control != null)
								{
									UIManager.Gumps.OfType<MacroButtonGump>().FirstOrDefault((MacroButtonGump s) => s._macro == control.Macro)?.Dispose();
									MacroButtonGump macroButtonGump = new MacroButtonGump(control.Macro, Mouse.Position.X, Mouse.Position.Y);
									macroButtonGump.X = Mouse.LClickPosition.X + (macroButtonGump.Width >> 1);
									macroButtonGump.Y = Mouse.LClickPosition.Y + (macroButtonGump.Height >> 1);
									UIManager.Add(macroButtonGump);
									UIManager.AttemptDragControl(macroButtonGump, attemptAlwaysSuccessful: true);
								}
							}
						};
						nb.MouseUp += delegate
						{
							_macroControl?.Dispose();
							OptionsGump optionsGump2 = this;
							MacroControl macroControl2 = new MacroControl(name);
							macroControl2.X = 400;
							macroControl2.Y = 20;
							optionsGump2._macroControl = macroControl2;
							Add(_macroControl, 4);
						};
					}
				}
			})
			{
				CanCloseWithRightClick = true
			});
		};
		niceButton2.MouseUp += delegate
		{
			NiceButton nb2 = databox.FindControls<NiceButton>().SingleOrDefault((NiceButton a) => a.IsSelected);
			if (nb2 != null)
			{
				UIManager.Add(new QuestionGump(ResGumps.MacroDeleteConfirmation, delegate(bool b)
				{
					if (b)
					{
						if (_macroControl != null)
						{
							UIManager.Gumps.OfType<MacroButtonGump>().FirstOrDefault((MacroButtonGump s) => s._macro == _macroControl.Macro)?.Dispose();
							Client.Game.GetScene<GameScene>().Macros.Remove(_macroControl.Macro);
							_macroControl.Dispose();
						}
						nb2.Dispose();
						databox.ReArrangeChildren();
					}
				}));
			}
		};
		for (Macro macro = (Macro)Client.Game.GetScene<GameScene>().Macros.Items; macro != null; macro = (Macro)macro.Next)
		{
			NiceButton nb3;
			databox.Add(nb3 = new NiceButton(0, 0, 130, 25, ButtonAction.Activate, macro.Name)
			{
				ButtonParameter = 17 + rightArea.Children.Count,
				Tag = macro
			});
			nb3.IsSelected = true;
			nb3.DragBegin += delegate(object? sss, MouseEventArgs eee)
			{
				NiceButton niceButton3 = (NiceButton)sss;
				Macro m = niceButton3.Tag as Macro;
				if (m != null && !UIManager.IsDragging && Math.Max(Math.Abs(Mouse.LDragOffset.X), Math.Abs(Mouse.LDragOffset.Y)) >= 5 && nb3.ScreenCoordinateX <= Mouse.LClickPosition.X && nb3.ScreenCoordinateX >= Mouse.LClickPosition.X - nb3.Width && nb3.ScreenCoordinateY <= Mouse.LClickPosition.Y && nb3.ScreenCoordinateY + nb3.Height >= Mouse.LClickPosition.Y)
				{
					UIManager.Gumps.OfType<MacroButtonGump>().FirstOrDefault((MacroButtonGump s) => s._macro == m)?.Dispose();
					MacroButtonGump macroButtonGump2 = new MacroButtonGump(m, Mouse.Position.X, Mouse.Position.Y);
					macroButtonGump2.X = Mouse.LClickPosition.X + (macroButtonGump2.Width >> 1);
					macroButtonGump2.Y = Mouse.LClickPosition.Y + (macroButtonGump2.Height >> 1);
					UIManager.Add(macroButtonGump2);
					UIManager.AttemptDragControl(macroButtonGump2, attemptAlwaysSuccessful: true);
				}
			};
			nb3.MouseUp += delegate(object? sss, MouseEventArgs eee)
			{
				if (((NiceButton)sss).Tag is Macro macro2)
				{
					_macroControl?.Dispose();
					OptionsGump optionsGump3 = this;
					MacroControl macroControl3 = new MacroControl(macro2.Name);
					macroControl3.X = 400;
					macroControl3.Y = 20;
					optionsGump3._macroControl = macroControl3;
					Add(_macroControl, 4);
				}
			};
		}
		databox.ReArrangeChildren();
		Add(rightArea, 4);
	}

	private void BuildTooltip()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_use_tooltip = AddCheckBox(scrollArea, ResGumps.UseTooltip, _currentProfile.UseTooltip, num, num2);
		num2 += _use_tooltip.Height + 2;
		num += 40;
		Label label = AddLabel(scrollArea, ResGumps.DelayBeforeDisplay, num, num2);
		num += label.Width + 5;
		_delay_before_display_tooltip = AddHSlider(scrollArea, 0, 1000, _currentProfile.TooltipDelayBeforeDisplay, num, num2, 200);
		num = 45;
		num2 += label.Height + 2;
		label = AddLabel(scrollArea, ResGumps.TooltipZoom, num, num2);
		num += label.Width + 5;
		_tooltip_zoom = AddHSlider(scrollArea, 100, 200, _currentProfile.TooltipDisplayZoom, num, num2, 200);
		num = 45;
		num2 += label.Height + 2;
		label = AddLabel(scrollArea, ResGumps.TooltipBackgroundOpacity, num, num2);
		num += label.Width + 5;
		_tooltip_background_opacity = AddHSlider(scrollArea, 0, 100, _currentProfile.TooltipBackgroundOpacity, num, num2, 200);
		num = 45;
		num2 += label.Height + 2;
		num2 += 15;
		label = AddLabel(scrollArea, ResGumps.TooltipFont, num, num2);
		num2 += label.Height + 2;
		num += 40;
		FontSelector fontSelector = new FontSelector(7, _currentProfile.TooltipFont, ResGumps.TooltipFontSelect);
		fontSelector.X = num;
		fontSelector.Y = num2;
		_tooltip_font_selector = fontSelector;
		scrollArea.Add(_tooltip_font_selector);
		Add(scrollArea, 5);
	}

	private void BuildFonts()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_overrideAllFonts = AddCheckBox(scrollArea, ResGumps.OverrideGameFont, _currentProfile.OverrideAllFonts, num, num2);
		num += _overrideAllFonts.Width + 5;
		num = 5;
		num2 += _overrideAllFonts.Height + 2;
		_forceUnicodeJournal = AddCheckBox(scrollArea, ResGumps.ForceUnicodeInJournal, _currentProfile.ForceUnicodeJournal, num, num2);
		num2 += _forceUnicodeJournal.Height + 2;
		_showYouSeeEntriesHostile = AddCheckBox(scrollArea, ResGumps.ShowYouSeeEntriesHostile, _currentProfile.ShowYouSeeEntriesHostile, num, num2);
		num2 += _showYouSeeEntriesHostile.Height + 2;
		_showYouSeeEntriesNeutral = AddCheckBox(scrollArea, ResGumps.ShowYouSeeEntriesNeutral, _currentProfile.ShowYouSeeEntriesNeutral, num, num2);
		num2 += _showYouSeeEntriesNeutral.Height + 2;
		_showYouSeeEntriesItems = AddCheckBox(scrollArea, ResGumps.ShowYouSeeEntriesItems, _currentProfile.ShowYouSeeEntriesItems, num, num2);
		num2 += _showYouSeeEntriesItems.Height + 2;
		_showJournalEntryTime = AddCheckBox(scrollArea, ResGumps.ShowJournalEntryTime, _currentProfile.ShowJournalEntryTime, num, num2);
		num2 += _showJournalEntryTime.Height + 2;
		_showAdditionalJournalSelf = AddCheckBox(scrollArea, ResGumps.ShowAdditionalJournalSelf, _currentProfile.AdditionalJournalShowSelf, num, num2);
		num2 += _showAdditionalJournalSelf.Height + 2;
		_showAdditionalJournalSystemMessages = AddCheckBox(scrollArea, ResGumps.ShowAdditionalJournalSystemMessages, _currentProfile.AdditionalJournalShowSystemMessages, num, num2);
		num2 += _showAdditionalJournalSystemMessages.Height + 2;
		AddLabel(scrollArea, "Journalskin: ", num, num2);
		num += 80;
		_journalSkin = AddCombobox(scrollArea, new string[5] { "Normal", "Balronmode", "Hell", "Grau", "Damast" }, _currentProfile.JournalSkin, num, num2, 150);
		num2 += _journalSkin.Height + 2;
		num -= 80;
		Label label = AddLabel(scrollArea, ResGumps.SpeechFont, num, num2);
		num += 40;
		num2 += label.Height + 2;
		FontSelector fontSelector = new FontSelector(20, _currentProfile.ChatFont, ResGumps.ThatSClassicUO);
		fontSelector.X = num;
		fontSelector.Y = num2;
		_fontSelectorChat = fontSelector;
		scrollArea.Add(_fontSelectorChat);
		Add(scrollArea, 6);
	}

	private void BuildSpeech()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_scaleSpeechDelay = AddCheckBox(scrollArea, ResGumps.ScaleSpeechDelay, _currentProfile.ScaleSpeechDelay, num, num2);
		num += _scaleSpeechDelay.Width + 5;
		_sliderSpeechDelay = AddHSlider(scrollArea, 0, 1000, _currentProfile.SpeechDelay, num, num2, 180);
		num = 5;
		num2 += _scaleSpeechDelay.Height + 2;
		_autoAcceptInvitesOfGuildMembers = AddCheckBox(scrollArea, ResGumps.AutoAcceptInvitesOfGuildMembers, _currentProfile.AutoAcceptInvitesOfGuildMembers, num, num2);
		num = 5;
		num2 += _autoAcceptInvitesOfGuildMembers.Height + 2;
		_chatAfterEnter = AddCheckBox(scrollArea, ResGumps.ActiveChatWhenPressingEnter, _currentProfile.ActivateChatAfterEnter, num, num2);
		num2 += _chatAfterEnter.Height + 2;
		_showChatActivity = AddCheckBox(scrollArea, ResGumps.ShowChatActivity, _currentProfile.ShowChatActivity, num, num2);
		num2 += _showChatActivity.Height + 2;
		Label label = AddLabel(scrollArea, ResGumps.ChatActivitySize, num, num2);
		num2 += label.Height + 2;
		num += 40;
		_chatActivitySize_combobox = AddCombobox(scrollArea, new string[2] { "normal", "vergroessert" }, _currentProfile.ChatActivitiySize, num, num2, 150);
		num2 += _chatActivitySize_combobox.Height + 2;
		num -= 40;
		_hideChatGradient = AddCheckBox(scrollArea, ResGumps.HideChatGradient, _currentProfile.HideChatGradient, num, num2);
		num2 += _hideChatGradient.Height + 2;
		_chatAdditionalButtonsCheckbox = AddCheckBox(scrollArea, ResGumps.UseAdditionalButtonsToActivateChat, _currentProfile.ActivateChatAdditionalButtons, num, num2);
		num2 += _chatAdditionalButtonsCheckbox.Height + 2;
		_chatShiftEnterCheckbox = AddCheckBox(scrollArea, ResGumps.UseShiftEnterToSendMessage, _currentProfile.ActivateChatShiftEnterSupport, num, num2);
		num = 5;
		num2 += 25;
		_showSystemChatMessagesInGameWindowBox = AddCheckBox(scrollArea, ResGumps.ToggleSystemMessages, _currentProfile.ShowSystemMessagesInGameWindow, num, num2);
		num = 5;
		num2 += 25;
		AddLabel(scrollArea, ResGumps.ChatPosition, num, num2);
		num += 80;
		_chatPositionX = AddInputField(scrollArea, num, num2, 50, 25, null, 50);
		_chatPositionX.SetText(_currentProfile.ChatPosition.X.ToString());
		num += 60;
		_chatPositionY = AddInputField(scrollArea, num, num2, 50, 25, null, 50);
		_chatPositionY.SetText(_currentProfile.ChatPosition.Y.ToString());
		num = 5;
		num2 += 35;
		_partyMessageColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.PartyMessageHue, ResGumps.PartyMessageColor);
		num += 200;
		num = 5;
		num = 5;
		Add(scrollArea, 7);
	}

	private void BuildCombat()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_holdDownKeyTab = AddCheckBox(scrollArea, ResGumps.TabCombat, _currentProfile.HoldDownKeyTab, num, num2);
		num2 += _holdDownKeyTab.Height + 2;
		_queryBeforAttackCheckbox = AddCheckBox(scrollArea, ResGumps.QueryAttack, _currentProfile.EnabledCriminalActionQuery, num, num2);
		num2 += _queryBeforAttackCheckbox.Height + 2;
		_queryBeforeBeneficialCheckbox = AddCheckBox(scrollArea, ResGumps.QueryBeneficialActs, _currentProfile.EnabledBeneficialCriminalActionQuery, num, num2);
		num2 += _queryBeforeBeneficialCheckbox.Height + 2;
		_buffBarTime = AddCheckBox(scrollArea, ResGumps.ShowBuffDuration, _currentProfile.BuffBarTime, num, num2);
		num2 += _buffBarTime.Height + 2;
		_buffBlinks = AddCheckBox(scrollArea, ResGumps.ShowBuffEndWithAlphaBlinks, _currentProfile.BuffEndWithAlphaBlinks, num, num2);
		num2 += _buffBlinks.Height + 2;
		num2 += 30;
		int num3 = num2;
		_innocentColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.InnocentHue, ResGumps.InnocentColor);
		num2 += _innocentColorPickerBox.Height + 2;
		_friendColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.FriendHue, ResGumps.FriendColor);
		num2 += _innocentColorPickerBox.Height + 2;
		_crimialColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.CriminalHue, ResGumps.CriminalColor);
		num2 += _innocentColorPickerBox.Height + 2;
		_canAttackColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.CanAttackHue, ResGumps.CanAttackColor);
		num2 += _innocentColorPickerBox.Height + 2;
		_murdererColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.MurdererHue, ResGumps.MurdererColor);
		num2 += _innocentColorPickerBox.Height + 2;
		_enemyColorPickerBox = AddColorBox(scrollArea, num, num2, _currentProfile.EnemyHue, ResGumps.EnemyColor);
		num2 += _innocentColorPickerBox.Height + 2;
		num2 = num3;
		num += 200;
		num = 5;
		Add(scrollArea, 8);
	}

	private void BuildCounters()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_enableCounters = AddCheckBox(scrollArea, ResGumps.EnableCounters, _currentProfile.CounterBarEnabled, num, num2);
		num += 40;
		num2 += _enableCounters.Height + 2;
		_highlightOnUse = AddCheckBox(scrollArea, ResGumps.HighlightOnUse, _currentProfile.CounterBarHighlightOnUse, num, num2);
		num2 += _highlightOnUse.Height + 2;
		_enableAbbreviatedAmount = AddCheckBox(scrollArea, ResGumps.EnableAbbreviatedAmountCountrs, _currentProfile.CounterBarDisplayAbbreviatedAmount, num, num2);
		num += _enableAbbreviatedAmount.Width + 5;
		_abbreviatedAmount = AddInputField(scrollArea, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true);
		_abbreviatedAmount.SetText(_currentProfile.CounterBarAbbreviatedAmount.ToString());
		num = 5;
		num += 40;
		num2 += _enableAbbreviatedAmount.Height + 2;
		_highlightOnAmount = AddCheckBox(scrollArea, ResGumps.HighlightRedWhenBelow, _currentProfile.CounterBarHighlightOnAmount, num, num2);
		num += _highlightOnAmount.Width + 5;
		_highlightAmount = AddInputField(scrollArea, num, num2, 50, 25, null, 50, set_down: false, numbersOnly: true, 999);
		_highlightAmount.SetText(_currentProfile.CounterBarHighlightAmount.ToString());
		num = 5;
		num += 40;
		num2 += _highlightAmount.Height + 2 + 5;
		num2 += 40;
		Label label = AddLabel(scrollArea, ResGumps.CounterLayout, num, num2);
		num += 40;
		num2 += label.Height + 2;
		label = AddLabel(scrollArea, ResGumps.CellSize, num, num2);
		int num3 = num;
		_cellSize = AddHSlider(x: num + (label.Width + 5), area: scrollArea, min: 30, max: 80, value: _currentProfile.CounterBarCellSize, y: num2, width: 80);
		num = num3;
		num2 += label.Height + 2 + 15;
		_rows = AddInputField(scrollArea, num, num2, 50, 30, ResGumps.Counter_Rows, 50, set_down: false, numbersOnly: true, 30);
		_rows.SetText(_currentProfile.CounterBarRows.ToString());
		num += _rows.Width + 5 + 100;
		_columns = AddInputField(scrollArea, num, num2, 50, 30, ResGumps.Counter_Columns, 50, set_down: false, numbersOnly: true, 30);
		_columns.SetText(_currentProfile.CounterBarColumns.ToString());
		Add(scrollArea, 9);
	}

	private void BuildExperimental()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_disableDefaultHotkeys = AddCheckBox(scrollArea, ResGumps.DisableDefaultUOHotkeys, _currentProfile.DisableDefaultHotkeys, num, num2);
		num += 40;
		num2 += _disableDefaultHotkeys.Height + 2;
		_disableArrowBtn = AddCheckBox(scrollArea, ResGumps.DisableArrowsPlayerMovement, _currentProfile.DisableArrowBtn, num, num2);
		num2 += _disableArrowBtn.Height + 2;
		_disableTabBtn = AddCheckBox(scrollArea, ResGumps.DisableTab, _currentProfile.DisableTabBtn, num, num2);
		num2 += _disableTabBtn.Height + 2;
		_disableCtrlQWBtn = AddCheckBox(scrollArea, ResGumps.DisableMessageHistory, _currentProfile.DisableCtrlQWBtn, num, num2);
		num2 += _disableCtrlQWBtn.Height + 2;
		_disableAutoMove = AddCheckBox(scrollArea, ResGumps.DisableClickAutomove, _currentProfile.DisableAutoMove, num, num2);
		num2 += _disableAutoMove.Height + 2;
		_preventAltF4CloseClient = AddCheckBox(scrollArea, ResGumps.PreventAltF4ToCloseClient, _currentProfile.PreventAltF4ToCloseClient, num, num2);
		Add(scrollArea, 12);
	}

	private void BuildInfoBar()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		_showInfoBar = AddCheckBox(scrollArea, ResGumps.ShowInfoBar, _currentProfile.ShowInfoBar, num, num2);
		num += 40;
		num2 += _showInfoBar.Height + 2;
		Label label = AddLabel(scrollArea, ResGumps.DataHighlightType, num, num2);
		num += label.Width + 5;
		_infoBarHighlightType = AddCombobox(scrollArea, new string[2]
		{
			ResGumps.TextColor,
			ResGumps.ColoredBars
		}, _currentProfile.InfoBarHighlightType, num, num2, 150);
		num = 5;
		num2 += _infoBarHighlightType.Height + 5;
		NiceButton niceButton = new NiceButton(num, num2, 90, 20, ButtonAction.Activate, ResGumps.AddItem, 0, TEXT_ALIGN_TYPE.TS_LEFT)
		{
			ButtonParameter = -1,
			IsSelectable = true,
			IsSelected = true
		};
		niceButton.MouseUp += delegate
		{
			InfoBarBuilderControl infoBarBuilderControl = new InfoBarBuilderControl(new InfoBarItem("", InfoBarVars.HP, 953));
			infoBarBuilderControl.X = 5;
			infoBarBuilderControl.Y = _databox.Children.Count * infoBarBuilderControl.Height;
			_infoBarBuilderControls.Add(infoBarBuilderControl);
			_databox.Add(infoBarBuilderControl);
			_databox.WantUpdateSize = true;
		};
		scrollArea.Add(niceButton);
		num2 += 40;
		label = AddLabel(scrollArea, ResGumps.Label, num, num2);
		num += 150;
		label = AddLabel(scrollArea, ResGumps.Color, num, num2);
		num += 55;
		label = AddLabel(scrollArea, ResGumps.Data, num, num2);
		num = 5;
		num2 += label.Height + 2;
		scrollArea.Add(new Line(num, num2, scrollArea.Width, 1, Color.Gray.PackedValue));
		num2 += 20;
		List<InfoBarItem> infoBars = Client.Game.GetScene<GameScene>().InfoBars.GetInfoBars();
		_infoBarBuilderControls = new List<InfoBarBuilderControl>();
		_databox = new DataBox(num, num2, 10, 10)
		{
			WantUpdateSize = true
		};
		for (int i = 0; i < infoBars.Count; i++)
		{
			InfoBarBuilderControl infoBarBuilderControl2 = new InfoBarBuilderControl(infoBars[i]);
			infoBarBuilderControl2.X = 5;
			infoBarBuilderControl2.Y = i * infoBarBuilderControl2.Height;
			_infoBarBuilderControls.Add(infoBarBuilderControl2);
			_databox.Add(infoBarBuilderControl2);
		}
		scrollArea.Add(_databox);
		Add(scrollArea, 10);
	}

	private void BuildContainers()
	{
		ScrollArea scrollArea = new ScrollArea(190, 20, 490, 420, normalScrollbar: true);
		int num = 5;
		int num2 = 5;
		bool flag = Client.Version >= ClientVersion.CV_705301;
		Label label;
		if (flag)
		{
			label = AddLabel(scrollArea, ResGumps.BackpackStyle, num, num2);
			num += label.Width + 5;
		}
		_backpackStyle = AddCombobox(scrollArea, new string[4]
		{
			ResGumps.BackpackStyle_Default,
			ResGumps.BackpackStyle_Suede,
			ResGumps.BackpackStyle_PolarBear,
			ResGumps.BackpackStyle_GhoulSkin
		}, _currentProfile.BackpackStyle, num, num2, 200);
		_backpackStyle.IsVisible = flag;
		if (flag)
		{
			num = 5;
			num2 += _backpackStyle.Height + 2 + 10;
		}
		label = AddLabel(scrollArea, ResGumps.ContainerScale, num, num2);
		num += label.Width + 5;
		_containersScale = AddHSlider(scrollArea, 50, 200, _currentProfile.ContainersScale, num, num2, 200);
		num = 5;
		num2 += label.Height + 2;
		_containerScaleItems = AddCheckBox(scrollArea, ResGumps.ScaleItemsInsideContainers, _currentProfile.ScaleItemsInsideContainers, num, num2);
		num2 += _containerScaleItems.Height + 2;
		_useLargeContianersGumps = AddCheckBox(scrollArea, ResGumps.UseLargeContainersGump, _currentProfile.UseLargeContainerGumps, num, num2);
		_useLargeContianersGumps.IsVisible = Client.Version >= ClientVersion.CV_706000;
		if (_useLargeContianersGumps.IsVisible)
		{
			num2 += _useLargeContianersGumps.Height + 2;
		}
		_containerDoubleClickToLoot = AddCheckBox(scrollArea, ResGumps.DoubleClickLootContainers, _currentProfile.DoubleClickToLootInsideContainers, num, num2);
		num2 += _containerDoubleClickToLoot.Height + 2;
		_relativeDragAnDropItems = AddCheckBox(scrollArea, ResGumps.RelativeDragAndDropContainers, _currentProfile.RelativeDragAndDropItems, num, num2);
		num2 += _relativeDragAnDropItems.Height + 2;
		_highlightContainersWhenMouseIsOver = AddCheckBox(scrollArea, ResGumps.HighlightContainerWhenSelected, _currentProfile.HighlightContainerWhenSelected, num, num2);
		num2 += _highlightContainersWhenMouseIsOver.Height + 2;
		_overrideContainerLocation = AddCheckBox(scrollArea, ResGumps.OverrideContainerGumpLocation, _currentProfile.OverrideContainerLocation, num, num2);
		num += _overrideContainerLocation.Width + 5;
		_overrideContainerLocationSetting = AddCombobox(scrollArea, new string[4]
		{
			ResGumps.ContLoc_NearContainerPosition,
			ResGumps.ContLoc_TopRight,
			ResGumps.ContLoc_LastDraggedPosition,
			ResGumps.ContLoc_RememberEveryContainer
		}, _currentProfile.OverrideContainerLocationSetting, num, num2, 200);
		num = 5;
		num2 += _overrideContainerLocation.Height + 2 + 10;
		NiceButton niceButton = new NiceButton(num, num2, 130, 30, ButtonAction.Activate, ResGumps.RebuildContainers)
		{
			ButtonParameter = -1,
			IsSelectable = true,
			IsSelected = true
		};
		niceButton.MouseUp += delegate
		{
			ContainerManager.BuildContainerFile(force: true);
		};
		scrollArea.Add(niceButton);
		Add(scrollArea, 11);
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID != 17)
		{
			switch ((Buttons)buttonID)
			{
			default:
				_ = 16;
				break;
			case Buttons.Cancel:
				Dispose();
				break;
			case Buttons.Apply:
				Apply();
				break;
			case Buttons.Default:
				SetDefault();
				break;
			case Buttons.Ok:
				Apply();
				Dispose();
				break;
			case Buttons.Disabled:
			case Buttons.NewMacro:
				break;
			}
		}
	}

	private void SetDefault()
	{
		switch (base.ActivePage)
		{
		case 1:
			_sliderFPS.Value = 60;
			_reduceFPSWhenInactive.IsChecked = true;
			_highlightObjects.IsChecked = true;
			_highlightGameObjectsColor.Hue = 20;
			_enableTopbar.IsChecked = false;
			_holdDownKeyTab.IsChecked = true;
			_holdDownKeyAlt.IsChecked = true;
			_closeAllAnchoredGumpsWithRClick.IsChecked = false;
			_holdShiftForContext.IsChecked = false;
			_holdAltToMoveGumps.IsChecked = false;
			_holdShiftToSplitStack.IsChecked = false;
			_enablePathfind.IsChecked = false;
			_useShiftPathfind.IsChecked = false;
			_alwaysRun.IsChecked = false;
			_alwaysRunUnlessHidden.IsChecked = false;
			_alwaysWalk.IsChecked = false;
			_showHpMobile.IsChecked = false;
			_hpComboBox.SelectedIndex = 0;
			_hpComboBoxShowWhen.SelectedIndex = 0;
			_highlightByPoisoned.IsChecked = true;
			_highlightByParalyzed.IsChecked = true;
			_highlightByInvul.IsChecked = true;
			_poisonColorPickerBox.Hue = 68;
			_paralyzedColorPickerBox.Hue = 332;
			_invulnerableColorPickerBox.Hue = 48;
			_drawRoofs.IsChecked = false;
			_enableCaveBorder.IsChecked = false;
			_treeToStumps.IsChecked = false;
			_hideVegetation.IsChecked = false;
			_noColorOutOfRangeObjects.IsChecked = false;
			_circleOfTranspRadius.Value = 50;
			_cotType.SelectedIndex = 0;
			_useCircleOfTransparency.IsChecked = false;
			_healtbarType.SelectedIndex = 0;
			_fieldsType.SelectedIndex = 0;
			_useStandardSkillsGump.IsChecked = true;
			_showCorpseNameIncoming.IsChecked = true;
			_showMobileNameIncoming.IsChecked = true;
			_gridLoot.SelectedIndex = 0;
			_sallosEasyGrab.IsChecked = false;
			_partyInviteGump.IsChecked = false;
			_showHouseContent.IsChecked = false;
			_objectsFading.IsChecked = true;
			_textFading.IsChecked = true;
			_enableDragSelect.IsChecked = false;
			_dragSelectHumanoidsOnly.IsChecked = false;
			_showTargetRangeIndicator.IsChecked = false;
			_customBars.IsChecked = false;
			_customBarsBBG.IsChecked = false;
			_autoOpenCorpse.IsChecked = false;
			_autoOpenDoors.IsChecked = false;
			_smoothDoors.IsChecked = false;
			_skipEmptyCorpse.IsChecked = false;
			_saveHealthbars.IsChecked = false;
			_use_smooth_boat_movement.IsChecked = false;
			_preventAltF4CloseClient.IsChecked = false;
			_hideScreenshotStoredInMessage.IsChecked = false;
			_auraType.SelectedIndex = 0;
			_fieldsType.SelectedIndex = 0;
			_showSkillsMessage.IsChecked = true;
			_showSkillsMessageDelta.Value = 1;
			_showStatsMessage.IsChecked = true;
			_dragSelectStartX.Value = 100;
			_dragSelectStartY.Value = 100;
			_dragSelectAsAnchor.IsChecked = false;
			break;
		case 2:
			_enableSounds.IsChecked = true;
			_enableMusic.IsChecked = false;
			_combatMusic.IsChecked = false;
			_soundsVolume.Value = 100;
			_animalVolume.Value = 100;
			_bardVolume.Value = 100;
			_heartbeatVolume.Value = 100;
			_musicVolume.Value = 100;
			_musicInBackground.IsChecked = false;
			_footStepsSound.IsChecked = true;
			_loginMusicVolume.Value = 100;
			_loginMusic.IsChecked = true;
			_enableHeartbeatSound.IsChecked = true;
			_heartbeatThreshhold.Value = 50;
			_soundsVolume.IsVisible = _enableSounds.IsChecked;
			_musicVolume.IsVisible = _enableMusic.IsChecked;
			break;
		case 3:
			_windowBorderless.IsChecked = false;
			_zoomCheckbox.IsChecked = false;
			_restorezoomCheckbox.IsChecked = false;
			_gameWindowWidth.SetText("600");
			_gameWindowHeight.SetText("480");
			_gameWindowPositionX.SetText("20");
			_gameWindowPositionY.SetText("20");
			_gameWindowLock.IsChecked = false;
			_gameWindowFullsize.IsChecked = false;
			Client.Game.Scene.Camera.Zoom = 1f;
			_currentProfile.DefaultScale = 1f;
			_lightBar.Value = 0;
			_enableLight.IsChecked = false;
			_useColoredLights.IsChecked = false;
			_darkNights.IsChecked = false;
			_enableShadows.IsChecked = true;
			_enableShadowsStatics.IsChecked = true;
			_terrainShadowLevel.Value = 15;
			_runMouseInSeparateThread.IsChecked = true;
			_auraMouse.IsChecked = true;
			_activateShakeEffects.IsChecked = true;
			_partyAura.IsChecked = true;
			_partyAuraColorPickerBox.Hue = 68;
			break;
		case 5:
			_use_tooltip.IsChecked = true;
			_tooltip_font_hue.Hue = ushort.MaxValue;
			_delay_before_display_tooltip.Value = 200;
			_tooltip_background_opacity.Value = 70;
			_tooltip_zoom.Value = 100;
			_tooltip_font_selector.SetSelectedFont(1);
			break;
		case 6:
			_fontSelectorChat.SetSelectedFont(0);
			_overrideAllFonts.IsChecked = false;
			_showYouSeeEntriesHostile.IsChecked = true;
			_showYouSeeEntriesNeutral.IsChecked = true;
			_showYouSeeEntriesItems.IsChecked = true;
			break;
		case 7:
			_scaleSpeechDelay.IsChecked = true;
			_sliderSpeechDelay.Value = 100;
			_speechColorPickerBox.Hue = 690;
			_emoteColorPickerBox.Hue = 33;
			_yellColorPickerBox.Hue = 33;
			_whisperColorPickerBox.Hue = 51;
			_partyMessageColorPickerBox.Hue = 68;
			_guildMessageColorPickerBox.Hue = 68;
			_allyMessageColorPickerBox.Hue = 87;
			_chatMessageColorPickerBox.Hue = 598;
			_chatAfterEnter.IsChecked = false;
			_showChatActivity.IsChecked = true;
			_chatActivitySize_combobox.SelectedIndex = 1;
			UIManager.SystemChat.IsActive = !_chatAfterEnter.IsChecked;
			_chatAdditionalButtonsCheckbox.IsChecked = true;
			_chatShiftEnterCheckbox.IsChecked = true;
			_saveJournalCheckBox.IsChecked = false;
			_hideChatGradient.IsChecked = false;
			_ignoreGuildMessages.IsChecked = false;
			_ignoreAllianceMessages.IsChecked = false;
			_chatPositionX.SetText("0");
			_chatPositionY.SetText("0");
			break;
		case 8:
			_innocentColorPickerBox.Hue = 90;
			_friendColorPickerBox.Hue = 68;
			_crimialColorPickerBox.Hue = 946;
			_canAttackColorPickerBox.Hue = 946;
			_murdererColorPickerBox.Hue = 35;
			_enemyColorPickerBox.Hue = 49;
			_queryBeforAttackCheckbox.IsChecked = true;
			_queryBeforeBeneficialCheckbox.IsChecked = false;
			_castSpellsByOneClick.IsChecked = false;
			_buffBarTime.IsChecked = false;
			_buffBlinks.IsChecked = false;
			_enableFastSpellsAssign.IsChecked = false;
			_beneficColorPickerBox.Hue = 89;
			_harmfulColorPickerBox.Hue = 32;
			_neutralColorPickerBox.Hue = 946;
			_spellFormatBox.SetText(ResGumps.SpellFormat_Default);
			_spellColoringCheckbox.IsChecked = false;
			_spellFormatCheckbox.IsChecked = false;
			break;
		case 9:
			_enableCounters.IsChecked = false;
			_highlightOnUse.IsChecked = false;
			_enableAbbreviatedAmount.IsChecked = false;
			_columns.SetText("1");
			_rows.SetText("1");
			_cellSize.Value = 40;
			_highlightOnAmount.IsChecked = false;
			_highlightAmount.SetText("5");
			_abbreviatedAmount.SetText("1000");
			break;
		case 11:
			_containersScale.Value = 100;
			_containerScaleItems.IsChecked = false;
			_useLargeContianersGumps.IsChecked = false;
			_containerDoubleClickToLoot.IsChecked = false;
			_relativeDragAnDropItems.IsChecked = false;
			_highlightContainersWhenMouseIsOver.IsChecked = false;
			_overrideContainerLocation.IsChecked = false;
			_overrideContainerLocationSetting.SelectedIndex = 0;
			_backpackStyle.SelectedIndex = 0;
			break;
		case 12:
			_disableDefaultHotkeys.IsChecked = false;
			_disableArrowBtn.IsChecked = false;
			_disableTabBtn.IsChecked = false;
			_disableCtrlQWBtn.IsChecked = false;
			_disableAutoMove.IsChecked = false;
			break;
		case 4:
		case 10:
			break;
		}
	}

	private void Apply()
	{
		WorldViewportGump gump = UIManager.GetGump<WorldViewportGump>(null);
		if (Settings.GlobalSettings.FPS != _sliderFPS.Value)
		{
			Client.Game.SetRefreshRate(_sliderFPS.Value);
		}
		_currentProfile.HighlightGameObjects = _highlightObjects.IsChecked;
		_currentProfile.HighlightGameObjectsColor = _highlightGameObjectsColor.Hue;
		_currentProfile.ReduceFPSWhenInactive = _reduceFPSWhenInactive.IsChecked;
		_currentProfile.EnablePathfind = _enablePathfind.IsChecked;
		_currentProfile.UseShiftToPathfind = _useShiftPathfind.IsChecked;
		_currentProfile.AlwaysRun = _alwaysRun.IsChecked;
		_currentProfile.AlwaysRunUnlessHidden = _alwaysRunUnlessHidden.IsChecked;
		_currentProfile.AlwaysWalk = _alwaysWalk.IsChecked;
		_currentProfile.ShowMobilesHP = _showHpMobile.IsChecked;
		_currentProfile.HighlightMobilesByPoisoned = _highlightByPoisoned.IsChecked;
		_currentProfile.ShowTargetFrames = _showTargetFrame.IsChecked;
		_currentProfile.EnableTargetWithoutWarmode = _targetWithoutWarmode.IsChecked;
		_currentProfile.ShowTargetingOverheadMessage = _showTargetOverheadMessage.IsChecked;
		_currentProfile.HighlightMobilesByParalize = _highlightByParalyzed.IsChecked;
		_currentProfile.HighlightMobilesByInvul = _highlightByInvul.IsChecked;
		_currentProfile.PoisonHue = _poisonColorPickerBox.Hue;
		_currentProfile.ParalyzedHue = _paralyzedColorPickerBox.Hue;
		_currentProfile.InvulnerableHue = _invulnerableColorPickerBox.Hue;
		_currentProfile.MobileHPType = _hpComboBox.SelectedIndex;
		_currentProfile.MobileHPShowWhen = _hpComboBoxShowWhen.SelectedIndex;
		_currentProfile.HoldDownKeyTab = _holdDownKeyTab.IsChecked;
		_currentProfile.HoldDownKeyAltToCloseAnchored = _holdDownKeyAlt.IsChecked;
		_currentProfile.CloseAllAnchoredGumpsInGroupWithRightClick = _closeAllAnchoredGumpsWithRClick.IsChecked;
		_currentProfile.HoldShiftForContext = _holdShiftForContext.IsChecked;
		_currentProfile.HoldAltToMoveGumps = _holdAltToMoveGumps.IsChecked;
		_currentProfile.HoldShiftToSplitStack = _holdShiftToSplitStack.IsChecked;
		_currentProfile.CloseHealthBarType = _healtbarType.SelectedIndex;
		_currentProfile.HideScreenshotStoredInMessage = _hideScreenshotStoredInMessage.IsChecked;
		if (_currentProfile.TopbarGumpIsDisabled != _enableTopbar.IsChecked)
		{
			if (_enableTopbar.IsChecked)
			{
				UIManager.GetGump<TopBarGump>(null)?.Dispose();
			}
			else
			{
				TopBarGump.Create();
			}
			_currentProfile.TopbarGumpIsDisabled = _enableTopbar.IsChecked;
		}
		if (_currentProfile.PaperdollGump != _paperdollGump.SelectedIndex && World.Player != null)
		{
			UIManager.GetGump<PaperDollGump>(World.Player.Serial)?.Dispose();
			_currentProfile.PaperdollGump = _paperdollGump.SelectedIndex;
			NetClient.Socket.Send_CustomPaperdoll(_paperdollGump.SelectedIndex);
			GameActions.OpenPaperdoll(World.Player.Serial);
		}
		if (_currentProfile.StatusBarGump != _statusBarGump.SelectedIndex && World.Player != null)
		{
			_currentProfile.StatusBarGump = _statusBarGump.SelectedIndex;
			StatusGumpBase statusGump = StatusGumpBase.GetStatusGump();
			if (statusGump == null)
			{
				UIManager.Add(StatusGumpBase.AddStatusGump(Mouse.Position.X - 100, Mouse.Position.Y - 25));
			}
			else
			{
				statusGump.Dispose();
				UIManager.Add(StatusGumpBase.AddStatusGump(Mouse.Position.X - 100, Mouse.Position.Y - 25));
			}
		}
		if (_currentProfile.EnableCaveBorder != _enableCaveBorder.IsChecked)
		{
			StaticFilters.CleanCaveTextures();
			_currentProfile.EnableCaveBorder = _enableCaveBorder.IsChecked;
		}
		_currentProfile.FieldsType = _fieldsType.SelectedIndex;
		_currentProfile.NoColorObjectsOutOfRange = _noColorOutOfRangeObjects.IsChecked;
		_currentProfile.UseCircleOfTransparency = _useCircleOfTransparency.IsChecked;
		if (_currentProfile.CircleOfTransparencyRadius != _circleOfTranspRadius.Value)
		{
			_currentProfile.CircleOfTransparencyRadius = _circleOfTranspRadius.Value;
			CircleOfTransparency.Create(_currentProfile.CircleOfTransparencyRadius);
		}
		_currentProfile.StandardSkillsGump = _useStandardSkillsGump.IsChecked;
		if (_useStandardSkillsGump.IsChecked)
		{
			SkillGumpAdvanced gump2 = UIManager.GetGump<SkillGumpAdvanced>(null);
			if (gump2 != null)
			{
				StandardSkillsGump standardSkillsGump = new StandardSkillsGump();
				standardSkillsGump.X = gump2.X;
				standardSkillsGump.Y = gump2.Y;
				UIManager.Add(standardSkillsGump);
				gump2.Dispose();
			}
		}
		else
		{
			StandardSkillsGump gump3 = UIManager.GetGump<StandardSkillsGump>(null);
			if (gump3 != null)
			{
				SkillGumpAdvanced skillGumpAdvanced = new SkillGumpAdvanced();
				skillGumpAdvanced.X = gump3.X;
				skillGumpAdvanced.Y = gump3.Y;
				UIManager.Add(skillGumpAdvanced);
				gump3.Dispose();
			}
		}
		_currentProfile.GridLootType = _gridLoot.SelectedIndex;
		_currentProfile.PartyInviteGump = _partyInviteGump.IsChecked;
		_currentProfile.UseObjectsFading = _objectsFading.IsChecked;
		_currentProfile.TextFading = _textFading.IsChecked;
		_currentProfile.UseSmoothBoatMovement = _use_smooth_boat_movement.IsChecked;
		_currentProfile.PreventAltF4ToCloseClient = _preventAltF4CloseClient.IsChecked;
		SDLHelper.SetNoCloseOnAltF4Hint(_currentProfile.PreventAltF4ToCloseClient);
		if (_currentProfile.ShowHouseContent != _showHouseContent.IsChecked)
		{
			_currentProfile.ShowHouseContent = _showHouseContent.IsChecked;
			NetClient.Socket.Send_ShowPublicHouseContent(_currentProfile.ShowHouseContent);
		}
		_currentProfile.EnableSound = _enableSounds.IsChecked;
		_currentProfile.EnableMusic = _enableMusic.IsChecked;
		_currentProfile.EnableFootstepsSound = _footStepsSound.IsChecked;
		_currentProfile.EnableCombatMusic = _combatMusic.IsChecked;
		_currentProfile.ReproduceSoundsInBackground = _musicInBackground.IsChecked;
		_currentProfile.SoundVolume = _soundsVolume.Value;
		_currentProfile.BardSoundsVolume = _bardVolume.Value;
		_currentProfile.AnimalSoundsVolume = _animalVolume.Value;
		_currentProfile.HeartbeatVolume = _heartbeatVolume.Value;
		_currentProfile.MusicVolume = _musicVolume.Value;
		_currentProfile.HeartbeatThreshold = _heartbeatThreshhold.Value;
		_currentProfile.HeartbeatSound = _enableHeartbeatSound.IsChecked;
		Settings.GlobalSettings.LoginMusicVolume = _loginMusicVolume.Value;
		Settings.GlobalSettings.LoginMusic = _loginMusic.IsChecked;
		Client.Game.Scene.Audio.UpdateCurrentMusicVolume();
		Client.Game.Scene.Audio.UpdateCurrentSoundsVolume();
		if (!_currentProfile.EnableMusic)
		{
			Client.Game.Scene.Audio.StopMusic();
		}
		if (!_currentProfile.EnableSound)
		{
			Client.Game.Scene.Audio.StopSounds();
		}
		_currentProfile.ScaleSpeechDelay = _scaleSpeechDelay.IsChecked;
		_currentProfile.SpeechDelay = _sliderSpeechDelay.Value;
		_currentProfile.PartyMessageHue = _partyMessageColorPickerBox.Hue;
		_currentProfile.ShowSystemMessagesInGameWindow = _showSystemChatMessagesInGameWindowBox.IsChecked;
		int.TryParse(_chatPositionX.Text, out var result);
		int.TryParse(_chatPositionY.Text, out var result2);
		if (result != _currentProfile.ChatPosition.X || result2 != _currentProfile.ChatPosition.Y)
		{
			_currentProfile.ChatPosition = new Point(result, result2);
			UIManager.SystemChat.X = result + 5;
			UIManager.SystemChat.Y = 5 - result2;
		}
		if (_currentProfile.ActivateChatAfterEnter != _chatAfterEnter.IsChecked)
		{
			UIManager.SystemChat.IsActive = !_chatAfterEnter.IsChecked;
			_currentProfile.ActivateChatAfterEnter = _chatAfterEnter.IsChecked;
		}
		_currentProfile.HideChatGradient = _hideChatGradient.IsChecked;
		_currentProfile.AutoAcceptInvitesOfGuildMembers = _autoAcceptInvitesOfGuildMembers.IsChecked;
		_currentProfile.ActivateChatAdditionalButtons = _chatAdditionalButtonsCheckbox.IsChecked;
		_currentProfile.ShowChatActivity = _showChatActivity.IsChecked;
		_currentProfile.ChatActivitiySize = _chatActivitySize_combobox.SelectedIndex;
		_currentProfile.ActivateChatShiftEnterSupport = _chatShiftEnterCheckbox.IsChecked;
		Client.Game.Scene.Camera.ZoomIndex = _sliderZoom.Value;
		_currentProfile.DefaultScale = Client.Game.Scene.Camera.Zoom;
		_currentProfile.EnableMousewheelScaleZoom = _zoomCheckbox.IsChecked;
		_currentProfile.RestoreScaleAfterUnpressCtrl = _restorezoomCheckbox.IsChecked;
		if (!CUOEnviroment.IsOutlands)
		{
			StatusGumpBase statusGump2 = StatusGumpBase.GetStatusGump();
			if (statusGump2 != null)
			{
				statusGump2.Dispose();
				UIManager.Add(StatusGumpBase.AddStatusGump(statusGump2.ScreenCoordinateX, statusGump2.ScreenCoordinateY));
			}
		}
		int.TryParse(_gameWindowWidth.Text, out var result3);
		int.TryParse(_gameWindowHeight.Text, out var result4);
		if ((result3 != _currentProfile.GameWindowSize.X || result4 != _currentProfile.GameWindowSize.Y) && gump != null)
		{
			Point point = gump.ResizeGameWindow(new Point(result3, result4));
			_gameWindowWidth.SetText(point.X.ToString());
			_gameWindowHeight.SetText(point.Y.ToString());
		}
		int.TryParse(_gameWindowPositionX.Text, out var result5);
		int.TryParse(_gameWindowPositionY.Text, out var result6);
		if ((result5 != _currentProfile.GameWindowPosition.X || result6 != _currentProfile.GameWindowPosition.Y) && gump != null)
		{
			Point location = (_currentProfile.GameWindowPosition = new Point(result5, result6));
			gump.Location = location;
		}
		if (_currentProfile.GameWindowLock != _gameWindowLock.IsChecked)
		{
			if (gump != null)
			{
				gump.CanMove = !_gameWindowLock.IsChecked;
			}
			_currentProfile.GameWindowLock = _gameWindowLock.IsChecked;
		}
		if (_gameWindowFullsize.IsChecked && (result5 != -5 || result6 != -5) && _currentProfile.GameWindowFullSize == _gameWindowFullsize.IsChecked)
		{
			_gameWindowFullsize.IsChecked = false;
		}
		if (_currentProfile.GameWindowFullSize != _gameWindowFullsize.IsChecked)
		{
			Point point3 = Point.Zero;
			Point point4 = Point.Zero;
			if (_gameWindowFullsize.IsChecked)
			{
				if (gump != null)
				{
					point3 = gump.ResizeGameWindow(new Point(Client.Game.Window.ClientBounds.Width, Client.Game.Window.ClientBounds.Height));
					Profile currentProfile = _currentProfile;
					Point point6 = (gump.Location = new Point(-5, -5));
					Point location = (currentProfile.GameWindowPosition = point6);
					point4 = location;
				}
			}
			else if (gump != null)
			{
				point3 = gump.ResizeGameWindow(new Point(600, 480));
				Point point6 = (_currentProfile.GameWindowPosition = new Point(20, 20));
				Point location = (gump.Location = point6);
				point4 = location;
			}
			_gameWindowPositionX.SetText(point4.X.ToString());
			_gameWindowPositionY.SetText(point4.Y.ToString());
			_gameWindowWidth.SetText(point3.X.ToString());
			_gameWindowHeight.SetText(point3.Y.ToString());
			_currentProfile.GameWindowFullSize = _gameWindowFullsize.IsChecked;
		}
		if (_currentProfile.WindowBorderless != _windowBorderless.IsChecked)
		{
			_currentProfile.WindowBorderless = _windowBorderless.IsChecked;
			Client.Game.SetWindowBorderless(_windowBorderless.IsChecked);
		}
		World.Light.Overall = World.Light.RealOverall;
		World.Light.Personal = World.Light.RealPersonal;
		_currentProfile.UseDarkNights = _darkNights.IsChecked;
		_currentProfile.ShadowsEnabled = _enableShadows.IsChecked;
		_currentProfile.ShadowsStatics = _enableShadowsStatics.IsChecked;
		_currentProfile.TerrainShadowsLevel = _terrainShadowLevel.Value;
		_currentProfile.AuraUnderFeetType = _auraType.SelectedIndex;
		_currentProfile.FilterType = _filterType.SelectedIndex;
		GameController game = Client.Game;
		bool isMouseVisible = (Settings.GlobalSettings.RunMouseInASeparateThread = _runMouseInSeparateThread.IsChecked);
		game.IsMouseVisible = isMouseVisible;
		_currentProfile.AuraOnMouse = _auraMouse.IsChecked;
		_currentProfile.ActivateShakeEffects = _activateShakeEffects.IsChecked;
		_currentProfile.PartyAura = _partyAura.IsChecked;
		_currentProfile.PartyAuraHue = _partyAuraColorPickerBox.Hue;
		_currentProfile.OverrideAllFonts = _overrideAllFonts.IsChecked;
		_currentProfile.OverrideAllFontsIsUnicode = _overrideAllFonts.IsChecked;
		_currentProfile.ShowYouSeeEntriesHostile = _showYouSeeEntriesHostile.IsChecked;
		_currentProfile.ShowYouSeeEntriesNeutral = _showYouSeeEntriesNeutral.IsChecked;
		_currentProfile.ShowYouSeeEntriesItems = _showYouSeeEntriesItems.IsChecked;
		_currentProfile.ShowJournalEntryTime = _showJournalEntryTime.IsChecked;
		_currentProfile.AdditionalJournalShowSelf = _showAdditionalJournalSelf.IsChecked;
		_currentProfile.AdditionalJournalShowSystemMessages = _showAdditionalJournalSystemMessages.IsChecked;
		_currentProfile.ForceUnicodeJournal = _forceUnicodeJournal.IsChecked;
		if (_currentProfile.JournalSkin != _journalSkin.SelectedIndex)
		{
			UIManager.GetGump<JournalGump>(null)?.Dispose();
			UIManager.GetGump<AdditionalJournalGump>(null)?.Dispose();
			_currentProfile.JournalSkin = _journalSkin.SelectedIndex;
			GameActions.OpenJournal();
			GameActions.OpenAdditionalJournal();
		}
		byte selectedFont = _fontSelectorChat.GetSelectedFont();
		if (_currentProfile.ChatFont != selectedFont)
		{
			_currentProfile.ChatFont = selectedFont;
			UIManager.SystemChat.TextBoxControl.Font = selectedFont;
		}
		_currentProfile.InnocentHue = _innocentColorPickerBox.Hue;
		_currentProfile.FriendHue = _friendColorPickerBox.Hue;
		_currentProfile.CriminalHue = _crimialColorPickerBox.Hue;
		_currentProfile.CanAttackHue = _canAttackColorPickerBox.Hue;
		_currentProfile.EnemyHue = _enemyColorPickerBox.Hue;
		_currentProfile.MurdererHue = _murdererColorPickerBox.Hue;
		_currentProfile.EnabledCriminalActionQuery = _queryBeforAttackCheckbox.IsChecked;
		_currentProfile.EnabledBeneficialCriminalActionQuery = _queryBeforeBeneficialCheckbox.IsChecked;
		_currentProfile.BuffBarTime = _buffBarTime.IsChecked;
		_currentProfile.BuffEndWithAlphaBlinks = _buffBlinks.IsChecked;
		Client.Game.GetScene<GameScene>().Macros.Save();
		bool counterBarEnabled = _currentProfile.CounterBarEnabled;
		_currentProfile.CounterBarEnabled = _enableCounters.IsChecked;
		_currentProfile.CounterBarCellSize = _cellSize.Value;
		if (!int.TryParse(_rows.Text, out var result7))
		{
			result7 = 1;
			_rows.SetText("1");
		}
		_currentProfile.CounterBarRows = result7;
		if (!int.TryParse(_columns.Text, out result7))
		{
			result7 = 1;
			_columns.SetText("1");
		}
		_currentProfile.CounterBarColumns = result7;
		_currentProfile.CounterBarHighlightOnUse = _highlightOnUse.IsChecked;
		if (!int.TryParse(_highlightAmount.Text, out result7))
		{
			result7 = 5;
			_highlightAmount.SetText("5");
		}
		_currentProfile.CounterBarHighlightAmount = result7;
		if (!int.TryParse(_abbreviatedAmount.Text, out result7))
		{
			result7 = 1000;
			_abbreviatedAmount.SetText("1000");
		}
		_currentProfile.CounterBarAbbreviatedAmount = result7;
		_currentProfile.CounterBarHighlightOnAmount = _highlightOnAmount.IsChecked;
		_currentProfile.CounterBarDisplayAbbreviatedAmount = _enableAbbreviatedAmount.IsChecked;
		CounterBarGump gump4 = UIManager.GetGump<CounterBarGump>(null);
		gump4?.SetLayout(_currentProfile.CounterBarCellSize, _currentProfile.CounterBarRows, _currentProfile.CounterBarColumns);
		if (counterBarEnabled != _currentProfile.CounterBarEnabled)
		{
			if (gump4 == null)
			{
				if (_currentProfile.CounterBarEnabled)
				{
					UIManager.Add(new CounterBarGump(200, 200, _currentProfile.CounterBarCellSize, _currentProfile.CounterBarRows, _currentProfile.CounterBarColumns));
				}
			}
			else
			{
				isMouseVisible = (gump4.IsVisible = _currentProfile.CounterBarEnabled);
				gump4.IsEnabled = isMouseVisible;
			}
		}
		if (!_disableDefaultHotkeys.IsChecked)
		{
			_disableArrowBtn.IsChecked = false;
			_disableTabBtn.IsChecked = false;
			_disableCtrlQWBtn.IsChecked = false;
			_disableAutoMove.IsChecked = false;
		}
		_currentProfile.DisableDefaultHotkeys = _disableDefaultHotkeys.IsChecked;
		_currentProfile.DisableArrowBtn = _disableArrowBtn.IsChecked;
		_currentProfile.DisableTabBtn = _disableTabBtn.IsChecked;
		_currentProfile.DisableCtrlQWBtn = _disableCtrlQWBtn.IsChecked;
		_currentProfile.DisableAutoMove = _disableAutoMove.IsChecked;
		_currentProfile.SmoothDoors = _smoothDoors.IsChecked;
		_currentProfile.AutoOpenCorpses = _autoOpenCorpse.IsChecked;
		_currentProfile.AutoOpenCorpseRange = int.Parse(_autoOpenCorpseRange.Text);
		_currentProfile.CorpseOpenOptions = _autoOpenCorpseOptions.SelectedIndex;
		_currentProfile.SkipEmptyCorpse = _skipEmptyCorpse.IsChecked;
		_currentProfile.EnableDragSelect = _enableDragSelect.IsChecked;
		_currentProfile.DragSelectModifierKey = _dragSelectModifierKey.SelectedIndex;
		_currentProfile.DragSelectHumanoidsOnly = _dragSelectHumanoidsOnly.IsChecked;
		_currentProfile.DragSelectStartX = _dragSelectStartX.Value;
		_currentProfile.DragSelectStartY = _dragSelectStartY.Value;
		_currentProfile.DragSelectAsAnchor = _dragSelectAsAnchor.IsChecked;
		_currentProfile.ShowSkillsChangedMessage = _showSkillsMessage.IsChecked;
		_currentProfile.ShowSkillsChangedDeltaValue = _showSkillsMessageDelta.Value;
		_currentProfile.ShowStatsChangedMessage = _showStatsMessage.IsChecked;
		_currentProfile.OverrideContainerLocation = _overrideContainerLocation.IsChecked;
		_currentProfile.OverrideContainerLocationSetting = _overrideContainerLocationSetting.SelectedIndex;
		_currentProfile.ShowTargetRangeIndicator = _showTargetRangeIndicator.IsChecked;
		bool num = _currentProfile.CustomBarsToggled != _customBars.IsChecked;
		_currentProfile.CustomBarsToggled = _customBars.IsChecked;
		if (num)
		{
			if (_currentProfile.CustomBarsToggled)
			{
				foreach (HealthBarGump item in UIManager.Gumps.OfType<HealthBarGump>().ToList())
				{
					HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(item.LocalSerial);
					healthBarGumpCustom.X = item.X;
					healthBarGumpCustom.Y = item.Y;
					UIManager.Add(healthBarGumpCustom);
					item.Dispose();
				}
			}
			else
			{
				foreach (HealthBarGumpCustom item2 in UIManager.Gumps.OfType<HealthBarGumpCustom>().ToList())
				{
					HealthBarGump healthBarGump = new HealthBarGump(item2.LocalSerial);
					healthBarGump.X = item2.X;
					healthBarGump.Y = item2.Y;
					UIManager.Add(healthBarGump);
					item2.Dispose();
				}
			}
		}
		_currentProfile.SaveHealthbars = _saveHealthbars.IsChecked;
		_currentProfile.ShowInfoBar = _showInfoBar.IsChecked;
		_currentProfile.InfoBarHighlightType = _infoBarHighlightType.SelectedIndex;
		InfoBarManager infoBars = Client.Game.GetScene<GameScene>().InfoBars;
		infoBars.Clear();
		for (int i = 0; i < _infoBarBuilderControls.Count; i++)
		{
			if (!_infoBarBuilderControls[i].IsDisposed)
			{
				infoBars.AddItem(new InfoBarItem(_infoBarBuilderControls[i].LabelText, _infoBarBuilderControls[i].Var, _infoBarBuilderControls[i].Hue));
			}
		}
		infoBars.Save();
		InfoBarGump gump5 = UIManager.GetGump<InfoBarGump>(null);
		if (_currentProfile.ShowInfoBar)
		{
			if (gump5 == null)
			{
				InfoBarGump infoBarGump = new InfoBarGump();
				infoBarGump.X = 300;
				infoBarGump.Y = 300;
				UIManager.Add(infoBarGump);
			}
			else
			{
				gump5.ResetItems();
				gump5.SetInScreen();
			}
		}
		else
		{
			gump5?.Dispose();
		}
		int containersScale = _currentProfile.ContainersScale;
		if ((byte)_containersScale.Value != containersScale || _currentProfile.ScaleItemsInsideContainers != _containerScaleItems.IsChecked)
		{
			byte b2 = (_currentProfile.ContainersScale = (byte)_containersScale.Value);
			containersScale = b2;
			UIManager.ContainerScale = (float)containersScale / 100f;
			_currentProfile.ScaleItemsInsideContainers = _containerScaleItems.IsChecked;
			foreach (ContainerGump item3 in UIManager.Gumps.OfType<ContainerGump>())
			{
				item3.RequestUpdateContents();
			}
		}
		_currentProfile.UseLargeContainerGumps = _useLargeContianersGumps.IsChecked;
		_currentProfile.DoubleClickToLootInsideContainers = _containerDoubleClickToLoot.IsChecked;
		_currentProfile.RelativeDragAndDropItems = _relativeDragAnDropItems.IsChecked;
		_currentProfile.HighlightContainerWhenSelected = _highlightContainersWhenMouseIsOver.IsChecked;
		if (_currentProfile.BackpackStyle != _backpackStyle.SelectedIndex)
		{
			_currentProfile.BackpackStyle = _backpackStyle.SelectedIndex;
			UIManager.GetGump<PaperDollGump>(World.Player.Serial)?.RequestUpdateContents();
			GameActions.DoubleClick(World.Player.FindItemByLayer(Layer.Backpack));
		}
		_currentProfile.UseTooltip = _use_tooltip.IsChecked;
		_currentProfile.TooltipDelayBeforeDisplay = _delay_before_display_tooltip.Value;
		_currentProfile.TooltipBackgroundOpacity = _tooltip_background_opacity.Value;
		_currentProfile.TooltipDisplayZoom = _tooltip_zoom.Value;
		_currentProfile.TooltipFont = _tooltip_font_selector.GetSelectedFont();
		_currentProfile?.Save(ProfileManager.ProfilePath);
	}

	internal void UpdateVideo()
	{
		_gameWindowWidth.SetText(_currentProfile.GameWindowSize.X.ToString());
		_gameWindowHeight.SetText(_currentProfile.GameWindowSize.Y.ToString());
		_gameWindowPositionX.SetText(_currentProfile.GameWindowPosition.X.ToString());
		_gameWindowPositionY.SetText(_currentProfile.GameWindowPosition.Y.ToString());
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.Draw(LogoTexture, new Rectangle(x + 190, y + 20, 450, 400), hueVector);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, base.Width, base.Height, hueVector);
		return base.Draw(batcher, x, y);
	}

	private InputField AddInputField(ScrollArea area, int x, int y, int width, int height, string label = null, int maxWidth = 0, bool set_down = false, bool numbersOnly = false, int maxCharCount = -1)
	{
		InputField obj = new InputField(3000, byte.MaxValue, ushort.MaxValue, unicode: true, width, height, maxWidth, maxCharCount)
		{
			NumbersOnly = numbersOnly
		};
		obj.X = x;
		obj.Y = y;
		InputField inputField = obj;
		if (area != null)
		{
			Label label2 = AddLabel(area, label, x, y);
			if (set_down)
			{
				inputField.Y = label2.Bounds.Bottom + 2;
			}
			else
			{
				inputField.X = label2.Bounds.Right + 2;
			}
			area.Add(inputField);
		}
		return inputField;
	}

	private Label AddLabel(ScrollArea area, string text, int x, int y)
	{
		Label label = new Label(text, isunicode: true, ushort.MaxValue);
		label.X = x;
		label.Y = y;
		Label label2 = label;
		area?.Add(label2);
		return label2;
	}

	private Checkbox AddCheckBox(ScrollArea area, string text, bool ischecked, int x, int y)
	{
		Checkbox obj = new Checkbox(210, 211, text, byte.MaxValue, ushort.MaxValue)
		{
			IsChecked = ischecked
		};
		obj.X = x;
		obj.Y = y;
		Checkbox checkbox = obj;
		area?.Add(checkbox);
		return checkbox;
	}

	private Combobox AddCombobox(ScrollArea area, string[] values, int currentIndex, int x, int y, int width)
	{
		Combobox combobox = new Combobox(x, y, width, values, -1, 200, showArrow: true, "", 9)
		{
			SelectedIndex = currentIndex
		};
		area?.Add(combobox);
		return combobox;
	}

	private HSliderBar AddHSlider(ScrollArea area, int min, int max, int value, int x, int y, int width)
	{
		HSliderBar hSliderBar = new HSliderBar(x, y, width, min, max, value, HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, byte.MaxValue, ushort.MaxValue);
		area?.Add(hSliderBar);
		return hSliderBar;
	}

	private ClickableColorBox AddColorBox(ScrollArea area, int x, int y, ushort hue, string text)
	{
		ClickableColorBox clickableColorBox = new ClickableColorBox(x, y, 13, 14, hue);
		area?.Add(clickableColorBox);
		if (area != null)
		{
			Label label = new Label(text, isunicode: true, ushort.MaxValue);
			label.X = x + clickableColorBox.Width + 10;
			label.Y = y;
			area.Add(label);
		}
		return clickableColorBox;
	}

	private SettingsSection AddSettingsSection(DataBox area, string label)
	{
		SettingsSection settingsSection = new SettingsSection(label, area.Width);
		area.Add(settingsSection);
		area.WantUpdateSize = true;
		return settingsSection;
	}
}
