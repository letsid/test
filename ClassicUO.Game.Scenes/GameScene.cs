using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Map;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.Game.Scenes;

internal class GameScene : Scene
{
	private struct LightData
	{
		public byte ID;

		public ushort Color;

		public int DrawX;

		public int DrawY;

		public ushort Graphic;

		public ushort Hue;
	}

	private struct TreeUnion
	{
		public readonly ushort Start;

		public readonly ushort End;

		public TreeUnion(ushort start, ushort end)
		{
			Start = start;
			End = end;
		}
	}

	private static readonly Lazy<BlendState> _darknessBlend = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Blend.Zero,
		ColorDestinationBlend = Blend.SourceColor,
		ColorBlendFunction = BlendFunction.Add
	});

	private static readonly Lazy<BlendState> _altLightsBlend = new Lazy<BlendState>(() => new BlendState
	{
		ColorSourceBlend = Blend.DestinationColor,
		ColorDestinationBlend = Blend.One,
		ColorBlendFunction = BlendFunction.Add
	});

	private static XBREffect _xbr;

	private bool _alphaChanged;

	private long _alphaTimer;

	private bool _forceStopScene;

	private HealthLinesManager _healthLinesManager;

	private bool _isListReady;

	private Point _lastSelectedMultiPositionInHouseCustomization;

	private int _lightCount;

	private readonly LightData[] _lights = new LightData[500];

	private Item _multi;

	private Rectangle _rectangleObj = Rectangle.Empty;

	private Rectangle _rectanglePlayer;

	private long _timePing;

	private uint _timeToPlaceMultiInHouseCustomization;

	private readonly bool _use_render_target;

	private UseItemQueue _useItemQueue = new UseItemQueue();

	private bool _useObjectHandles;

	public RenderTarget2D _world_render_target;

	public RenderTarget2D _lightRenderTarget;

	private static readonly RenderedText _youAreDeadText = RenderedText.Create(ResGeneral.YouAreDead, ushort.MaxValue, 3, isunicode: false, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);

	private static GameObject[] _foliages = new GameObject[100];

	private static readonly TreeUnion[] _treeInfos = new TreeUnion[9]
	{
		new TreeUnion(3397, 3404),
		new TreeUnion(3420, 3426),
		new TreeUnion(3443, 3449),
		new TreeUnion(3463, 3467),
		new TreeUnion(4798, 4807),
		new TreeUnion(3405, 3411),
		new TreeUnion(3427, 3433),
		new TreeUnion(3450, 3455),
		new TreeUnion(3468, 3472)
	};

	private sbyte _maxGroundZ;

	private int _maxZ;

	private Vector2 _minPixel;

	private Vector2 _maxPixel;

	private bool _noDrawRoofs;

	private Point _offset;

	private Point _maxTile;

	private Point _minTile;

	private Point _last_scaled_offset;

	private int _oldPlayerX;

	private int _oldPlayerY;

	private int _oldPlayerZ;

	private int _renderIndex = 1;

	private int _foliageCount;

	private GameObject _renderListStaticsHead;

	private GameObject _renderList;

	private int _renderListStaticsCount;

	private GameObject _renderListTransparentObjectsHead;

	private GameObject _renderListTransparentObjects;

	private int _renderListTransparentObjectsCount;

	private GameObject _renderListAnimationsHead;

	private GameObject _renderListAnimations;

	private int _renderListAnimationCount;

	private bool _boatRun;

	private bool _boatIsMoving;

	private readonly bool[] _flags = new bool[5];

	private bool _followingMode;

	private uint _followingTarget;

	private uint _holdMouse2secOverItemTime;

	private bool _isMouseLeftDown;

	private bool _isSelectionActive;

	private Direction _lastBoatDirection;

	private bool _requestedWarMode;

	private bool _rightMousePressed;

	private bool _continueRunning;

	private Point _selectionStart;

	private Point _selectionEnd;

	public bool UpdateDrawPosition { get; set; }

	public HotkeysManager Hotkeys { get; private set; }

	public MacroManager Macros { get; private set; }

	public InfoBarManager InfoBars { get; private set; }

	public Weather Weather { get; private set; }

	public bool DisconnectionRequested { get; set; }

	public bool UseLights
	{
		get
		{
			if (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.UseCustomLightLevel)
			{
				return World.Light.RealPersonal < World.Light.RealOverall;
			}
			return World.Light.Personal < World.Light.Overall;
		}
	}

	public bool UseAltLights
	{
		get
		{
			if (ProfileManager.CurrentProfile != null)
			{
				return ProfileManager.CurrentProfile.UseAlternativeLights;
			}
			return false;
		}
	}

	public Point MinTile => _minTile;

	public Point ScreenOffset => _offset;

	public sbyte FoliageIndex { get; private set; }

	private int AnchorOffset
	{
		get
		{
			if (!ProfileManager.CurrentProfile.DragSelectAsAnchor)
			{
				return 2;
			}
			return 0;
		}
	}

	public GameScene()
		: base(2, canresize: true, maximized: true, loadaudio: false)
	{
	}

	public void DoubleClickDelayed(uint serial)
	{
		_useItemQueue.Add(serial);
	}

	public override void Load()
	{
		base.Load();
		ItemHold.Clear();
		Hotkeys = new HotkeysManager();
		Macros = new MacroManager();
		if (ProfileManager.CurrentProfile.Macros != null)
		{
			for (int i = 0; i < ProfileManager.CurrentProfile.Macros.Length; i++)
			{
				Macros.PushToBack(ProfileManager.CurrentProfile.Macros[i]);
			}
			Macros.Save();
			ProfileManager.CurrentProfile.Macros = null;
		}
		Macros.Load();
		InfoBars = new InfoBarManager();
		InfoBars.Load();
		_healthLinesManager = new HealthLinesManager();
		Weather = new Weather();
		UIManager.Add(new WorldViewportGump(this), front: false);
		if (!ProfileManager.CurrentProfile.TopbarGumpIsDisabled)
		{
			TopBarGump.Create();
		}
		CommandManager.Initialize();
		NetClient.Socket.Disconnected += SocketOnDisconnected;
		MessageManager.MessageReceived += ChatOnMessageReceived;
		UIManager.ContainerScale = (float)(int)ProfileManager.CurrentProfile.ContainersScale / 100f;
		SDL.SDL_SetWindowMinimumSize(Client.Game.Window.Handle, 640, 480);
		if (ProfileManager.CurrentProfile.WindowBorderless)
		{
			Client.Game.SetWindowBorderless(borderless: true);
		}
		else if (Settings.GlobalSettings.IsWindowMaximized)
		{
			Client.Game.MaximizeWindow();
		}
		else if (Settings.GlobalSettings.WindowSize.HasValue)
		{
			int x = Settings.GlobalSettings.WindowSize.Value.X;
			int y = Settings.GlobalSettings.WindowSize.Value.Y;
			x = Math.Max(640, x);
			y = Math.Max(480, y);
			Client.Game.SetWindowSize(x, y);
		}
		CircleOfTransparency.Create(ProfileManager.CurrentProfile.CircleOfTransparencyRadius);
		Plugin.OnConnected();
		base.Camera.SetZoomValues(new float[20]
		{
			0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1f, 1.1f, 1.2f, 1.3f, 1.5f,
			1.6f, 1.7f, 1.8f, 1.9f, 2f, 2.1f, 2.2f, 2.3f, 2.4f, 2.5f
		});
		base.Camera.Zoom = ProfileManager.CurrentProfile.DefaultScale;
	}

	private void ChatOnMessageReceived(object sender, MessageEventArgs e)
	{
		if (e.Type == MessageType.Command)
		{
			return;
		}
		string text = e.Name + ": ";
		Profile currentProfile = ProfileManager.CurrentProfile;
		ushort hue = e.Hue;
		string text3;
		string text2;
		switch (e.Type)
		{
		case MessageType.Regular:
		case MessageType.Limit3Spell:
			text3 = ((!(e.Parent == null) && SerialHelper.IsValid(e.Parent.Serial)) ? text : string.Empty);
			text2 = e.Text;
			break;
		case MessageType.System:
			text3 = ((string.IsNullOrEmpty(e.Name) || string.Equals(e.Name, "system", StringComparison.InvariantCultureIgnoreCase)) ? string.Empty : text);
			text2 = e.Text;
			break;
		case MessageType.Emote:
			text3 = text;
			text2 = e.Text ?? "";
			if (e.Hue == 0)
			{
				hue = ProfileManager.CurrentProfile.EmoteHue;
			}
			break;
		case MessageType.Label:
			text3 = ((!(e.Parent == null) && SerialHelper.IsValid(e.Parent.Serial)) ? ResGeneral.YouSee : string.Empty);
			text2 = e.Text;
			break;
		case MessageType.Spell:
			text3 = text;
			text2 = e.Text;
			break;
		case MessageType.Party:
			text2 = e.Text;
			text = text.Remove(text.Length - 2, 2);
			text3 = string.Format(ResGeneral.Party0, text);
			hue = ProfileManager.CurrentProfile.PartyMessageHue;
			break;
		case MessageType.Alliance:
			text2 = e.Text;
			text3 = string.Format(ResGeneral.Alliance0, text);
			hue = ProfileManager.CurrentProfile.AllyMessageHue;
			break;
		case MessageType.Guild:
			text2 = e.Text;
			text3 = string.Format(ResGeneral.Guild0, text);
			hue = ProfileManager.CurrentProfile.GuildMessageHue;
			break;
		default:
			text2 = e.Text;
			text3 = text;
			hue = e.Hue;
			Log.Warn($"Unhandled text type {e.Type}  -  text: '{e.Text}'");
			break;
		}
		if (string.IsNullOrEmpty(text2))
		{
			return;
		}
		if (text3 == ResGeneral.YouSee)
		{
			if (e.Parent is Mobile)
			{
				Mobile mobile = (Mobile)e.Parent;
				if ((currentProfile.ShowYouSeeEntriesHostile || (mobile.NotorietyFlag != NotorietyFlag.Murderer && mobile.NotorietyFlag != NotorietyFlag.Boss && mobile.NotorietyFlag != NotorietyFlag.Criminal)) && (currentProfile.ShowYouSeeEntriesNeutral || (mobile.NotorietyFlag != NotorietyFlag.Gray && mobile.NotorietyFlag != NotorietyFlag.Invulnerable && mobile.NotorietyFlag != 0)))
				{
					WorldJournalAdd(e, text3, text2, hue);
				}
			}
			else if (e.Parent is Item && currentProfile.ShowYouSeeEntriesItems)
			{
				WorldJournalAdd(e, text3, text2, hue);
			}
		}
		else
		{
			WorldJournalAdd(e, text3, text2, hue);
			bool flag = currentProfile.AdditionalJournalShowSystemMessages && e.TextType == TextType.SYSTEM && string.IsNullOrEmpty(text3);
			if ((e.Parent != null && World.AdditionalJournalFilterDict.Count != 0 && World.AdditionalJournalFilterDict.ContainsKey(e.Parent.Serial)) || (currentProfile.AdditionalJournalShowSelf && World.Player == e.Parent) || flag)
			{
				AdditionalJournalAdd(e, text3, text2, hue);
			}
		}
	}

	private static void WorldJournalAdd(MessageEventArgs e, string name, string text, ushort hue)
	{
		World.Journal.Add(text, hue, name, e.TextType, e.IsUnicode);
	}

	private static void AdditionalJournalAdd(MessageEventArgs e, string name, string text, ushort hue)
	{
		World.AdditionalJournal.Add(text, hue, name, e.TextType, e.IsUnicode);
	}

	public override void Unload()
	{
		Client.Game.SetWindowTitle(string.Empty);
		ItemHold.Clear();
		try
		{
			Plugin.OnDisconnected();
		}
		catch
		{
		}
		TargetManager.Reset();
		UIManager.GetGump<WorldMapGump>(null)?.SaveSettings();
		ProfileManager.CurrentProfile?.Save(ProfileManager.ProfilePath);
		Macros.Save();
		InfoBars.Save();
		ProfileManager.UnLoadProfile();
		StaticFilters.CleanCaveTextures();
		StaticFilters.CleanTreeTextures();
		NetClient.Socket.Disconnected -= SocketOnDisconnected;
		NetClient.Socket.Disconnect();
		_lightRenderTarget?.Dispose();
		_world_render_target?.Dispose();
		CommandManager.UnRegisterAll();
		Weather.Reset();
		UIManager.Clear();
		World.Clear();
		ChatManager.Clear();
		DelayedObjectClickManager.Clear();
		_useItemQueue?.Clear();
		_useItemQueue = null;
		Hotkeys = null;
		Macros = null;
		MessageManager.MessageReceived -= ChatOnMessageReceived;
		Settings.GlobalSettings.WindowSize = new Point(Client.Game.Window.ClientBounds.Width, Client.Game.Window.ClientBounds.Height);
		Settings.GlobalSettings.IsWindowMaximized = Client.Game.IsWindowMaximized();
		Client.Game.SetWindowBorderless(borderless: false);
		base.Unload();
	}

	private void SocketOnDisconnected(object sender, SocketError e)
	{
		if (Settings.GlobalSettings.Reconnect)
		{
			_forceStopScene = true;
			return;
		}
		UIManager.Add(new MessageBoxGump(200, 200, string.Format(ResGeneral.ConnectionLost0, StringHelper.AddSpaceBeforeCapital(e.ToString())), delegate(bool s)
		{
			if (s)
			{
				Client.Game.SetScene(new LoginScene());
			}
		}, hasBackground: false, MessageButtonType.OK, 32));
	}

	public void RequestQuitGame()
	{
		UIManager.Add(new QuestionGump(ResGeneral.QuitPrompt, delegate(bool s)
		{
			if (s)
			{
				if ((World.ClientFeatures.Flags & CharacterListFlags.CLF_OWERWRITE_CONFIGURATION_BUTTON) != 0)
				{
					DisconnectionRequested = true;
					NetClient.Socket.Send_LogoutNotification();
				}
				else
				{
					NetClient.Socket.Disconnect();
					Client.Game.SetScene(new LoginScene());
				}
			}
		}));
	}

	public void AddLight(GameObject obj, GameObject lightObject, int x, int y)
	{
		if (_lightCount >= 500 || (!UseLights && !UseAltLights) || obj == null)
		{
			return;
		}
		bool flag = true;
		int x2 = obj.X + 1;
		int y2 = obj.Y + 1;
		GameObject tile = World.Map.GetTile(x2, y2);
		if (tile != null)
		{
			sbyte b = (sbyte)(obj.Z + 5);
			for (GameObject gameObject = tile; gameObject != null; gameObject = gameObject.TNext)
			{
				if (((gameObject is Static @static && !@static.ItemData.IsTransparent) || (gameObject is Multi multi && !multi.ItemData.IsTransparent)) && gameObject.AllowedToDraw && gameObject.Z < _maxZ && gameObject.Z >= b)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		ref LightData reference = ref _lights[_lightCount];
		ushort num = (reference.Graphic = lightObject.Graphic);
		reference.Hue = lightObject.Hue;
		if ((num >= 15874 && num <= 15883) || (num >= 14612 && num <= 14633) || num == 2845)
		{
			reference.ID = 2;
		}
		else if (obj == lightObject && obj is Item item)
		{
			reference.ID = item.LightID;
		}
		else if (lightObject is Item item2)
		{
			reference.ID = (byte)item2.ItemData.LightIndex;
			if (obj is Mobile mobile)
			{
				switch (mobile.Direction)
				{
				case Direction.Right:
					y += 33;
					x += 22;
					break;
				case Direction.Left:
					y += 33;
					x -= 22;
					break;
				case Direction.East:
					x += 22;
					y += 55;
					break;
				case Direction.Down:
					y += 55;
					break;
				case Direction.South:
					x -= 22;
					y += 55;
					break;
				}
			}
		}
		else if (obj is Mobile)
		{
			reference.ID = 1;
		}
		else
		{
			reference.ID = TileDataLoader.Instance.StaticData[obj.Graphic].Layer;
		}
		if (reference.ID < 500)
		{
			reference.Color = LightColors.GetHue(num);
			if (reference.Color != 0)
			{
				reference.Color++;
			}
			reference.DrawX = x;
			reference.DrawY = y;
			_lightCount++;
		}
	}

	private void FillGameObjectList()
	{
		_renderListStaticsHead = null;
		_renderList = null;
		_renderListStaticsCount = 0;
		_renderListTransparentObjectsHead = null;
		_renderListTransparentObjects = null;
		_renderListTransparentObjectsCount = 0;
		_renderListAnimationsHead = null;
		_renderListAnimations = null;
		_renderListAnimationCount = 0;
		_foliageCount = 0;
		if (!World.InGame)
		{
			return;
		}
		_isListReady = false;
		_alphaChanged = _alphaTimer < Time.Ticks;
		if (_alphaChanged)
		{
			_alphaTimer = Time.Ticks + 20;
		}
		FoliageIndex++;
		if (FoliageIndex >= 100)
		{
			FoliageIndex = 1;
		}
		GetViewPort();
		bool flag = NameOverHeadManager.IsToggled || (Keyboard.Ctrl && Keyboard.Shift);
		if (flag != _useObjectHandles)
		{
			_useObjectHandles = flag;
			if (_useObjectHandles)
			{
				NameOverHeadManager.Open();
			}
			else
			{
				NameOverHeadManager.Close();
			}
		}
		_rectanglePlayer.X = (int)((float)(World.Player.RealScreenPosition.X - World.Player.FrameInfo.X + 22) + World.Player.CumulativeOffset.X);
		_rectanglePlayer.Y = (int)((float)(World.Player.RealScreenPosition.Y - World.Player.FrameInfo.Y + 22) + (World.Player.CumulativeOffset.Y - World.Player.CumulativeOffset.Z));
		_rectanglePlayer.Width = World.Player.FrameInfo.Width;
		_rectanglePlayer.Height = World.Player.FrameInfo.Height;
		int x = _minTile.X;
		int y = _minTile.Y;
		int x2 = _maxTile.X;
		int y2 = _maxTile.Y;
		ClassicUO.Game.Map.Map map = World.Map;
		bool useObjectHandles = _useObjectHandles;
		int cotZ = World.Player.Z + 5;
		Vector2 playerScreePos = World.Player.GetScreenPosition();
		for (int i = 0; i < 2; i++)
		{
			int num = y;
			int num2 = y2;
			if (i != 0)
			{
				num = x;
				num2 = x2;
			}
			for (int j = num; j < num2; j++)
			{
				int num3 = x;
				int num4 = j;
				if (i != 0)
				{
					num3 = j;
					num4 = y2;
				}
				while (num3 >= x && num3 <= x2 && num4 >= y && num4 <= y2)
				{
					AddTileToRenderList(map.GetTile(num3, num4), num3, num4, useObjectHandles, 300, cotZ, ref playerScreePos);
					num3++;
					num4--;
				}
			}
		}
		if (_alphaChanged)
		{
			for (int k = 0; k < _foliageCount; k++)
			{
				GameObject gameObject = _foliages[k];
				if (gameObject.FoliageIndex == FoliageIndex)
				{
					CalculateAlpha(ref gameObject.AlphaHue, 76);
				}
				else
				{
					CalculateAlpha(ref gameObject.AlphaHue, 255);
				}
			}
		}
		UpdateTextServerEntities(World.Mobiles.Values, force: true);
		UpdateTextServerEntities(World.Items.Values, force: false);
		_renderIndex++;
		if (_renderIndex >= 100)
		{
			_renderIndex = 1;
		}
		UpdateDrawPosition = false;
		_isListReady = true;
	}

	private void UpdateTextServerEntities<T>(IEnumerable<T> entities, bool force) where T : Entity
	{
		foreach (T entity in entities)
		{
			if (entity.UseInRender != _renderIndex && entity.TextContainer != null && !entity.TextContainer.IsEmpty && (force || entity.Graphic == 8198))
			{
				entity.UpdateRealScreenPosition(_offset.X, _offset.Y);
				entity.UseInRender = (byte)_renderIndex;
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		Profile currentProfile = ProfileManager.CurrentProfile;
		base.Camera.SetGameWindowBounds(currentProfile.GameWindowPosition.X + 5, currentProfile.GameWindowPosition.Y + 5, currentProfile.GameWindowSize.X, currentProfile.GameWindowSize.Y);
		SelectedObject.TranslatedMousePositionByViewport = base.Camera.MouseToWorldPosition();
		base.Update(totalTime, frameTime);
		PacketHandlers.SendMegaClilocRequests();
		if (_forceStopScene)
		{
			LoginScene loginScene = new LoginScene();
			Client.Game.SetScene(loginScene);
			loginScene.Reconnect = true;
		}
		else
		{
			if (!World.InGame)
			{
				return;
			}
			World.Update(totalTime, frameTime);
			AnimatedStaticsManager.Process();
			BoatMovingManager.Update();
			Pathfinder.ProcessAutoWalk();
			DelayedObjectClickManager.Update();
			if (!MoveCharacterByMouseInput() && !currentProfile.DisableArrowBtn)
			{
				Direction direction = DirectionHelper.DirectionFromKeyboardArrows(_flags[0], _flags[2], _flags[1], _flags[3]);
				if (World.InGame && !Pathfinder.AutoWalking && direction != Direction.NONE)
				{
					World.Player.Walk(direction, currentProfile.AlwaysRun);
				}
			}
			if (_followingMode && SerialHelper.IsMobile(_followingTarget) && !Pathfinder.AutoWalking)
			{
				Mobile mobile = World.Mobiles.Get(_followingTarget);
				if (mobile != null)
				{
					int distance = mobile.Distance;
					if (distance > World.ClientViewRange)
					{
						StopFollowing();
					}
					else if (distance > 3)
					{
						Pathfinder.WalkTo(mobile.X, mobile.Y, mobile.Z, 1);
					}
				}
				else
				{
					StopFollowing();
				}
			}
			if (totalTime > (double)_timePing)
			{
				NetClient.Socket.Statistics.SendPing();
				_timePing = (long)totalTime + 1000;
			}
			Macros.Update();
			if (((currentProfile.CorpseOpenOptions == 1 || currentProfile.CorpseOpenOptions == 3) && TargetManager.IsTargeting) || ((currentProfile.CorpseOpenOptions == 2 || currentProfile.CorpseOpenOptions == 3) && World.Player.IsHidden))
			{
				_useItemQueue.ClearCorpses();
			}
			_useItemQueue.Update(totalTime, frameTime);
			if (!UIManager.IsMouseOverWorld)
			{
				SelectedObject.Object = (SelectedObject.LastObject = null);
			}
			if (TargetManager.IsTargeting && TargetManager.TargetingState == CursorTarget.MultiPlacement && World.CustomHouseManager == null && TargetManager.MultiTargetInfo != null)
			{
				if (_multi == null)
				{
					_multi = Item.Create(0u);
					_multi.Graphic = TargetManager.MultiTargetInfo.Model;
					_multi.Hue = TargetManager.MultiTargetInfo.Hue;
					_multi.IsMulti = true;
				}
				if (SelectedObject.Object is GameObject gameObject)
				{
					int num = gameObject.X % 8;
					int num2 = gameObject.Y % 8;
					Chunk chunk = World.Map.GetChunk(gameObject.X, gameObject.Y);
					GameObject gameObject2 = ((chunk != null) ? chunk.Tiles[num, num2] : null);
					ushort x;
					ushort y;
					sbyte z;
					if (gameObject2 != null)
					{
						x = gameObject2.X;
						y = gameObject2.Y;
						z = gameObject2.Z;
					}
					else
					{
						x = gameObject.X;
						y = gameObject.Y;
						z = gameObject.Z;
					}
					World.Map.GetMapZ(x, y, out var groundZ, out var _);
					if (gameObject is Static @static && @static.ItemData.IsWet)
					{
						groundZ = gameObject.Z;
					}
					x -= TargetManager.MultiTargetInfo.XOff;
					y -= TargetManager.MultiTargetInfo.YOff;
					z = (sbyte)(groundZ - TargetManager.MultiTargetInfo.ZOff);
					_multi.X = x;
					_multi.Y = y;
					_multi.Z = z;
					_multi.UpdateScreenPosition();
					_multi.CheckGraphicChange(0);
					_multi.AddToTile();
					World.HouseManager.TryGetHouse(_multi.Serial, out var house);
					foreach (Multi component in house.Components)
					{
						component.IsHousePreview = true;
						component.X = (ushort)(_multi.X + component.MultiOffsetX);
						component.Y = (ushort)(_multi.Y + component.MultiOffsetY);
						component.Z = (sbyte)(_multi.Z + component.MultiOffsetZ);
						component.UpdateScreenPosition();
						component.AddToTile();
					}
				}
			}
			else if (_multi != null)
			{
				World.HouseManager.RemoveMultiTargetHouse();
				_multi.Destroy();
				_multi = null;
			}
			if (_isMouseLeftDown && !ItemHold.Enabled)
			{
				if (World.CustomHouseManager != null && World.CustomHouseManager.SelectedGraphic != 0 && !World.CustomHouseManager.SeekTile && !World.CustomHouseManager.Erasing && Time.Ticks > _timeToPlaceMultiInHouseCustomization)
				{
					if (SelectedObject.LastObject is GameObject gameObject3 && (gameObject3.X != _lastSelectedMultiPositionInHouseCustomization.X || gameObject3.Y != _lastSelectedMultiPositionInHouseCustomization.Y))
					{
						World.CustomHouseManager.OnTargetWorld(gameObject3);
						_timeToPlaceMultiInHouseCustomization = Time.Ticks + 50;
						_lastSelectedMultiPositionInHouseCustomization.X = gameObject3.X;
						_lastSelectedMultiPositionInHouseCustomization.Y = gameObject3.Y;
					}
				}
				else if (Time.Ticks - _holdMouse2secOverItemTime >= 1000 && SelectedObject.LastObject is Item item && GameActions.PickUp(item.Serial, 0, 0, -1, null))
				{
					_isMouseLeftDown = false;
					_holdMouse2secOverItemTime = 0u;
				}
			}
			if (PacketHandlers.ResendGameWindow && PacketHandlers.ResendGameWindowTimeout < DateTime.Now)
			{
				PacketHandlers.ResendGameWindowSize();
			}
			CheckSumCalculator.HandlePending();
		}
	}

	public override void FixedUpdate(double totalTime, double frameTime)
	{
	}

	public override bool Draw(UltimaBatcher2D batcher)
	{
		if (!World.InGame)
		{
			return false;
		}
		int x = ProfileManager.CurrentProfile.GameWindowPosition.X + 5;
		int y = ProfileManager.CurrentProfile.GameWindowPosition.Y + 5;
		int x2 = ProfileManager.CurrentProfile.GameWindowSize.X;
		int y2 = ProfileManager.CurrentProfile.GameWindowSize.Y;
		if (CheckDeathScreen(batcher, x, y, x2, y2))
		{
			return true;
		}
		Viewport viewport = batcher.GraphicsDevice.Viewport;
		Viewport viewport2 = base.Camera.GetViewport();
		Matrix matrix = (_use_render_target ? Matrix.Identity : base.Camera.ViewTransformMatrix);
		bool flag = false;
		if (!_use_render_target)
		{
			flag = PrepareLightsRendering(batcher, ref matrix);
			batcher.GraphicsDevice.Viewport = viewport2;
		}
		DrawWorld(batcher, ref matrix, _use_render_target);
		if (_use_render_target)
		{
			flag = PrepareLightsRendering(batcher, ref matrix);
			batcher.GraphicsDevice.Viewport = viewport2;
		}
		Vector3 zero = Vector3.Zero;
		zero.Z = 1f;
		if (_use_render_target)
		{
			if (_xbr == null)
			{
				_xbr = new XBREffect(batcher.GraphicsDevice);
			}
			_xbr.TextureSize.SetValue(new Vector2(x2, y2));
			batcher.Begin(null, base.Camera.ViewTransformMatrix);
			batcher.Draw(_world_render_target, new Rectangle(0, 0, x2, y2), zero);
			batcher.End();
		}
		if (flag)
		{
			batcher.Begin();
			if (UseAltLights)
			{
				zero.Z = 0.5f;
				batcher.SetBlendState(_altLightsBlend.Value);
			}
			else
			{
				batcher.SetBlendState(_darknessBlend.Value);
			}
			batcher.Draw(_lightRenderTarget, new Rectangle(0, 0, x2, y2), zero);
			batcher.SetBlendState(null);
			batcher.End();
			zero.Z = 1f;
		}
		batcher.Begin();
		DrawOverheads(batcher, x, y);
		DrawSelection(batcher);
		batcher.End();
		batcher.GraphicsDevice.Viewport = viewport;
		return base.Draw(batcher);
	}

	private void DrawWorld(UltimaBatcher2D batcher, ref Matrix matrix, bool use_render_target)
	{
		SelectedObject.Object = null;
		FillGameObjectList();
		if (use_render_target)
		{
			batcher.GraphicsDevice.SetRenderTarget(_world_render_target);
			batcher.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);
		}
		else
		{
			switch (ProfileManager.CurrentProfile.FilterType)
			{
			default:
				batcher.SetSampler(SamplerState.PointClamp);
				break;
			case 1:
				batcher.SetSampler(SamplerState.AnisotropicClamp);
				break;
			case 2:
				batcher.SetSampler(SamplerState.LinearClamp);
				break;
			}
		}
		batcher.Begin(null, matrix);
		batcher.SetBrightlight((float)ProfileManager.CurrentProfile.TerrainShadowsLevel * 0.1f);
		batcher.SetStencil(DepthStencilState.Default);
		base.RenderedObjectsCount = 0;
		base.RenderedObjectsCount += DrawRenderList(batcher, _renderListStaticsHead, _renderListStaticsCount);
		base.RenderedObjectsCount += DrawRenderList(batcher, _renderListAnimationsHead, _renderListAnimationCount);
		if (_renderListTransparentObjectsCount > 0)
		{
			batcher.SetStencil(DepthStencilState.DepthRead);
			base.RenderedObjectsCount += DrawRenderList(batcher, _renderListTransparentObjectsHead, _renderListTransparentObjectsCount);
		}
		batcher.SetStencil(null);
		if (_multi != null && TargetManager.IsTargeting && TargetManager.TargetingState == CursorTarget.MultiPlacement)
		{
			_multi.Draw(batcher, _multi.RealScreenPosition.X, _multi.RealScreenPosition.Y, _multi.CalculateDepthZ());
		}
		Weather.Draw(batcher, 0, 0);
		batcher.End();
		batcher.SetSampler(null);
		batcher.SetStencil(null);
		_ = batcher.FlushesDone;
		_ = batcher.TextureSwitches;
		if (use_render_target)
		{
			batcher.GraphicsDevice.SetRenderTarget(null);
		}
	}

	private int DrawRenderList(UltimaBatcher2D batcher, GameObject obj, int count)
	{
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (obj.Z <= _maxGroundZ)
			{
				float depth = obj.CalculateDepthZ();
				if (obj.Draw(batcher, obj.RealScreenPosition.X, obj.RealScreenPosition.Y, depth))
				{
					num++;
				}
			}
			obj = obj.RenderListNext;
		}
		return num;
	}

	private bool PrepareLightsRendering(UltimaBatcher2D batcher, ref Matrix matrix)
	{
		if ((!UseLights && !UseAltLights) || (World.Player.IsDead && ProfileManager.CurrentProfile.EnableBlackWhiteEffect) || _lightRenderTarget == null)
		{
			return false;
		}
		batcher.GraphicsDevice.SetRenderTarget(_lightRenderTarget);
		batcher.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);
		if (!UseAltLights)
		{
			float num = World.Light.IsometricLevel;
			if (ProfileManager.CurrentProfile.UseDarkNights)
			{
				num -= 0.04f;
			}
			batcher.GraphicsDevice.Clear(ClearOptions.Target, new Vector4(num, num, num, 1f), 0f, 0);
		}
		batcher.Begin(null, matrix);
		batcher.SetBlendState(BlendState.Additive);
		Vector3 zero = Vector3.Zero;
		zero.Y = 9f;
		zero.Z = 1f;
		for (int i = 0; i < _lightCount; i++)
		{
			ref LightData reference = ref _lights[i];
			float num2 = 1f;
			if (LightsLoader.Instance.LightDefDict.TryGetValue(reference.Graphic, out var value))
			{
				if (reference.Hue != 0 && value.AutoCustomRenderer == 1)
				{
					zero.X = (int)reference.Hue;
				}
				else
				{
					switch (value.Renderer)
					{
					case 1:
						zero.X = 167f;
						num2 = 0.5f;
						break;
					case 20:
						zero.X = 167f;
						break;
					case 2:
						zero.X = 90f;
						break;
					case 6:
						num2 = 0.5f;
						zero.X = 4f;
						break;
					case 10:
						zero.X = 4f;
						break;
					case 30:
						zero.X = 44f;
						break;
					case 31:
						num2 = 0.5f;
						zero.X = 44f;
						break;
					case 32:
						zero.X = 25f;
						break;
					case 40:
						zero.X = 38f;
						break;
					case 50:
						zero.X = 55f;
						break;
					case 60:
						num2 = 0.5f;
						zero.X = 55f;
						break;
					case 61:
						num2 = 0.75f;
						zero.X = 55f;
						break;
					case 62:
						num2 = 0.75f;
						break;
					default:
						zero.X = 0f;
						break;
					}
				}
			}
			else
			{
				zero.X = (int)reference.Color;
			}
			Rectangle bounds;
			Texture2D lightTexture = LightsLoader.Instance.GetLightTexture(reference.ID, out bounds);
			if (lightTexture != null)
			{
				batcher.Draw(lightTexture, new Vector2((float)reference.DrawX - (float)bounds.Width * num2 * 0.5f, (float)reference.DrawY - (float)bounds.Height * num2 * 0.5f), bounds, zero, 0f, new Vector2(0f, 0f), num2, SpriteEffects.None, 0f);
			}
		}
		_lightCount = 0;
		batcher.SetBlendState(null);
		batcher.End();
		batcher.GraphicsDevice.SetRenderTarget(null);
		return true;
	}

	public void DrawOverheads(UltimaBatcher2D batcher, int x, int y)
	{
		_healthLinesManager.Draw(batcher);
		int num = _renderIndex - 1;
		if (num < 1)
		{
			num = 99;
		}
		if (!UIManager.IsMouseOverWorld)
		{
			SelectedObject.Object = null;
		}
		World.WorldTextManager.ProcessWorldText(doit: true);
		World.WorldTextManager.Draw(batcher, x, y, num);
		SelectedObject.LastObject = SelectedObject.Object;
	}

	public void DrawSelection(UltimaBatcher2D batcher)
	{
		if (_isSelectionActive)
		{
			Vector3 vector = default(Vector3);
			vector.Z = 0.7f;
			int num = Math.Min(_selectionStart.X, Mouse.Position.X);
			int num2 = Math.Max(_selectionStart.X, Mouse.Position.X);
			int num3 = Math.Min(_selectionStart.Y, Mouse.Position.Y);
			int num4 = Math.Max(_selectionStart.Y, Mouse.Position.Y);
			Rectangle destinationRectangle = new Rectangle(num - base.Camera.Bounds.X, num3 - base.Camera.Bounds.Y, num2 - num, num4 - num3);
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), destinationRectangle, vector);
			vector.Z = 0.3f;
			batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.DeepSkyBlue), destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, vector);
		}
	}

	private bool CheckDeathScreen(UltimaBatcher2D batcher, int x, int y, int width, int height)
	{
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableDeathScreen && World.InGame && World.Player.IsDead && World.Player.DeathScreenTimer > Time.Ticks)
		{
			batcher.Begin();
			_youAreDeadText.Draw(batcher, x + (width / 2 - _youAreDeadText.Width / 2), y + height / 2, 1f, 0);
			batcher.End();
			return true;
		}
		return false;
	}

	private void StopFollowing()
	{
		if (_followingMode)
		{
			_followingMode = false;
			_followingTarget = 0u;
			Pathfinder.StopAutoWalk();
			MessageManager.HandleMessage(World.Player, ResGeneral.StoppedFollowing, string.Empty, 0, MessageType.Regular, 3, TextType.CLIENT);
		}
	}

	public void UpdateMaxDrawZ(bool force = false)
	{
		int x = World.Player.X;
		int y = World.Player.Y;
		int z = World.Player.Z;
		if (x == _oldPlayerX && y == _oldPlayerY && z == _oldPlayerZ && !force)
		{
			return;
		}
		_oldPlayerX = x;
		_oldPlayerY = y;
		_oldPlayerZ = z;
		sbyte maxGroundZ = sbyte.MaxValue;
		_maxGroundZ = sbyte.MaxValue;
		_maxZ = 127;
		_noDrawRoofs = !ProfileManager.CurrentProfile.DrawRoofs;
		int x2 = x;
		int y2 = y;
		Chunk chunk = World.Map.GetChunk(x2, y2, load: false);
		if (chunk == null)
		{
			return;
		}
		int x3 = x % 8;
		int y3 = y % 8;
		int num = z + 14;
		int num2 = z + 16;
		for (GameObject gameObject = chunk.GetHeadObject(x3, y3); gameObject != null; gameObject = gameObject.TNext)
		{
			sbyte b = gameObject.Z;
			if (gameObject is Land land)
			{
				if (land.IsStretched)
				{
					b = land.AverageZ;
				}
				if (num2 <= b)
				{
					maxGroundZ = (sbyte)num2;
					_maxGroundZ = (sbyte)num2;
					_maxZ = _maxGroundZ;
					break;
				}
			}
			else if (!(gameObject is Mobile) && b > num && _maxZ > b)
			{
				ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[gameObject.Graphic];
				if ((reference.Flags & (TileFlag.Transparent | TileFlag.Foliage)) == TileFlag.None && (!reference.IsRoof || reference.IsSurface))
				{
					_maxZ = b;
					_noDrawRoofs = true;
				}
			}
		}
		int num3 = _maxZ;
		_maxGroundZ = (sbyte)_maxZ;
		x++;
		y++;
		x2 = x;
		y2 = y;
		chunk = World.Map.GetChunk(x2, y2, load: false);
		if (chunk != null)
		{
			x3 = x % 8;
			y3 = y % 8;
			for (GameObject gameObject2 = chunk.GetHeadObject(x3, y3); gameObject2 != null; gameObject2 = gameObject2.TNext)
			{
				if (!(gameObject2 is Mobile))
				{
					sbyte z2 = gameObject2.Z;
					if (z2 > num && _maxZ > z2 && !(gameObject2 is Land))
					{
						ref StaticTiles reference2 = ref TileDataLoader.Instance.StaticData[gameObject2.Graphic];
						if ((reference2.Flags & (TileFlag.Transparent | TileFlag.Surface)) == TileFlag.None && reference2.IsRoof)
						{
							_maxZ = z2;
							World.Map.ClearBockAccess();
							_maxGroundZ = World.Map.CalculateNearZ(z2, x, y, z2);
							_noDrawRoofs = true;
						}
					}
				}
			}
			num3 = _maxGroundZ;
		}
		_maxZ = _maxGroundZ;
		if (num3 < num2)
		{
			_maxZ = num2;
			_maxGroundZ = (sbyte)num2;
		}
		_maxGroundZ = maxGroundZ;
	}

	private void IsFoliageUnion(ushort graphic, int x, int y, int z)
	{
		for (int i = 0; i < _treeInfos.Length; i++)
		{
			ref TreeUnion reference = ref _treeInfos[i];
			if (reference.Start <= graphic && graphic <= reference.End)
			{
				while (graphic > reference.Start)
				{
					graphic--;
					x--;
					y++;
				}
				graphic = reference.Start;
				while (graphic <= reference.End)
				{
					ApplyFoliageTransparency(graphic, x, y, z);
					graphic++;
					x++;
					y--;
				}
				break;
			}
		}
	}

	private void ApplyFoliageTransparency(ushort graphic, int x, int y, int z)
	{
		GameObject tile = World.Map.GetTile(x, y);
		if (tile == null)
		{
			return;
		}
		for (GameObject gameObject = tile; gameObject != null; gameObject = gameObject.TNext)
		{
			if (gameObject.Graphic == graphic && gameObject.Z == z)
			{
				gameObject.FoliageIndex = FoliageIndex;
			}
		}
	}

	private void UpdateObjectHandles(Entity obj, bool useObjectHandles)
	{
		if (useObjectHandles && NameOverHeadManager.IsAllowed(obj))
		{
			if (obj.ObjectHandlesStatus != ObjectHandlesStatus.CLOSED)
			{
				if (obj.ObjectHandlesStatus == ObjectHandlesStatus.NONE)
				{
					obj.ObjectHandlesStatus = ObjectHandlesStatus.OPEN;
				}
				obj.UpdateTextCoordsV();
			}
		}
		else if (obj.ObjectHandlesStatus != 0)
		{
			obj.ObjectHandlesStatus = ObjectHandlesStatus.NONE;
			obj.UpdateTextCoordsV();
		}
	}

	private void CheckIfBehindATree(GameObject obj, int worldX, int worldY, ref StaticTiles itemData)
	{
		if (!itemData.IsFoliage)
		{
			return;
		}
		if (obj.FoliageIndex != FoliageIndex)
		{
			sbyte foliageIndex = 0;
			bool flag = World.Player.X <= worldX && World.Player.Y <= worldY;
			if (!flag)
			{
				flag = World.Player.Y <= worldY && World.Player.X <= worldX + 1;
				if (!flag)
				{
					flag = World.Player.X <= worldX && World.Player.Y <= worldY + 1;
				}
			}
			if (flag)
			{
				Rectangle rect = ArtLoader.Instance.GetRealArtBounds(obj.Graphic);
				rect.X = obj.RealScreenPosition.X - (rect.Width >> 1) + rect.X;
				rect.Y = obj.RealScreenPosition.Y - rect.Height + rect.Y;
				if (Exstentions.InRect(ref rect, ref _rectanglePlayer))
				{
					foliageIndex = FoliageIndex;
					IsFoliageUnion(obj.Graphic, obj.X, obj.Y, obj.Z);
				}
			}
			obj.FoliageIndex = foliageIndex;
		}
		if (_foliageCount >= _foliages.Length)
		{
			int newSize = _foliages.Length + 50;
			Array.Resize(ref _foliages, newSize);
		}
		_foliages[_foliageCount++] = obj;
	}

	private bool ProcessAlpha(GameObject obj, ref StaticTiles itemData, bool useCoT, ref Vector2 playerPos, int cotZ, out bool allowSelection)
	{
		allowSelection = true;
		if (obj.Z >= _maxZ)
		{
			if (((!_alphaChanged) ? ((int)obj.AlphaHue) : (CalculateAlpha(ref obj.AlphaHue, 0) ? 1 : 0)) == 0)
			{
				obj.UseInRender = (byte)_renderIndex;
				return false;
			}
		}
		else
		{
			if (_noDrawRoofs && itemData.IsRoof && !itemData.IsWall)
			{
				if (_alphaChanged && !CalculateAlpha(ref obj.AlphaHue, 0))
				{
					return false;
				}
				return obj.AlphaHue != 0;
			}
			if (itemData.IsTranslucent)
			{
				if (_alphaChanged)
				{
					CalculateAlpha(ref obj.AlphaHue, 170);
				}
			}
			else if (!itemData.IsFoliage && (!useCoT || !CheckCircleOfTransparencyRadius(obj, cotZ, ref playerPos, ref allowSelection)) && _alphaChanged && obj.AlphaHue != byte.MaxValue)
			{
				CalculateAlpha(ref obj.AlphaHue, 255);
			}
		}
		return true;
	}

	private bool CheckCircleOfTransparencyRadius(GameObject obj, int maxZ, ref Vector2 playerPos, ref bool allowSelection)
	{
		if (ProfileManager.CurrentProfile.UseCircleOfTransparency && obj.TransparentTest(maxZ))
		{
			int circleOfTransparencyRadius = ProfileManager.CurrentProfile.CircleOfTransparencyRadius;
			Vector2 value = new Vector2(obj.RealScreenPosition.X, obj.RealScreenPosition.Y - 44);
			Vector2.Distance(ref playerPos, ref value, out var result);
			if (result <= (float)circleOfTransparencyRadius)
			{
				float num = (float)(circleOfTransparencyRadius - 44) * 0.5f;
				float num2 = (result - num) / ((float)circleOfTransparencyRadius - num);
				obj.AlphaHue = (byte)Microsoft.Xna.Framework.MathHelper.Clamp(num2 * 255f, 0f, 255f);
				allowSelection = obj.AlphaHue >= 127;
				return true;
			}
		}
		return false;
	}

	private static bool CalculateAlpha(ref byte alphaHue, int maxAlpha)
	{
		if (ProfileManager.CurrentProfile != null && !ProfileManager.CurrentProfile.UseObjectsFading)
		{
			alphaHue = (byte)maxAlpha;
			return maxAlpha != 0;
		}
		bool result = false;
		int num = alphaHue;
		if (num > maxAlpha)
		{
			num -= 25;
			if (num < maxAlpha)
			{
				num = maxAlpha;
			}
			result = true;
		}
		else if (num < maxAlpha)
		{
			num += 25;
			if (num > maxAlpha)
			{
				num = maxAlpha;
			}
			result = true;
		}
		alphaHue = (byte)num;
		return result;
	}

	private static byte CalculateObjectHeight(ref int maxObjectZ, ref StaticTiles itemData)
	{
		if (itemData.Height != byte.MaxValue)
		{
			byte b = itemData.Height;
			if (itemData.Height == 0 && !itemData.IsBackground && !itemData.IsSurface)
			{
				b = 10;
			}
			if ((itemData.Flags & TileFlag.Bridge) != TileFlag.None)
			{
				b /= 2;
			}
			maxObjectZ += b;
			return b;
		}
		return byte.MaxValue;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool IsFoliageVisibleAtSeason(ref StaticTiles itemData, Season season)
	{
		if (itemData.IsFoliage && !itemData.IsMultiMovable)
		{
			return season < Season.Winter;
		}
		return true;
	}

	private bool HasSurfaceOverhead(Entity obj)
	{
		if (obj.Serial == World.Player.Serial)
		{
			return false;
		}
		bool flag = false;
		for (int i = -1; i <= 2; i++)
		{
			for (int j = -1; j <= 2; j++)
			{
				GameObject gameObject = World.Map.GetTile(obj.X + j, obj.Y + i);
				flag = false;
				while (gameObject != null)
				{
					GameObject tNext = gameObject.TNext;
					if (gameObject.Z > obj.Z && (gameObject is Static || gameObject is Multi) && TileDataLoader.Instance.StaticData[gameObject.Graphic].IsWindow && _maxZ - gameObject.Z + 5 >= gameObject.Z - obj.Z)
					{
						flag = true;
						break;
					}
					gameObject = tNext;
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		return flag;
	}

	private void PushToRenderList(GameObject obj, ref GameObject renderList, ref GameObject first, ref int renderListCount, bool allowSelection)
	{
		if (obj.AlphaHue == 0)
		{
			return;
		}
		if (allowSelection && obj.Z <= _maxGroundZ && obj.AllowedToDraw && obj.CheckMouseSelection())
		{
			SelectedObject.Object = obj;
		}
		if (obj.AlphaHue != byte.MaxValue)
		{
			if (_renderListTransparentObjectsHead == null)
			{
				_renderListTransparentObjectsHead = (_renderListTransparentObjects = obj);
			}
			else
			{
				_renderListTransparentObjects.RenderListNext = obj;
				_renderListTransparentObjects = obj;
			}
			obj.RenderListNext = null;
			_renderListTransparentObjectsCount++;
		}
		else
		{
			if (first == null)
			{
				first = (renderList = obj);
			}
			else
			{
				renderList.RenderListNext = obj;
				renderList = obj;
			}
			obj.RenderListNext = null;
			renderListCount++;
		}
		obj.UseInRender = (byte)_renderIndex;
	}

	private bool AddTileToRenderList(GameObject obj, int worldX, int worldY, bool useObjectHandles, int maxZ, int cotZ, ref Vector2 playerScreePos)
	{
		int num = -127;
		int num2 = 0;
		while (obj != null)
		{
			if ((UpdateDrawPosition && obj.CurrentRenderIndex != _renderIndex) || obj.IsPositionChanged)
			{
				obj.UpdateRealScreenPosition(_offset.X, _offset.Y);
			}
			obj.UseInRender = byte.MaxValue;
			int x = obj.RealScreenPosition.X;
			if ((float)x < _minPixel.X || (float)x > _maxPixel.X)
			{
				break;
			}
			int num3 = obj.RealScreenPosition.Y;
			int maxObjectZ = obj.PriorityZ;
			int index;
			bool allowSelection5;
			if (obj is Land land)
			{
				if (maxObjectZ > maxZ)
				{
					return false;
				}
				obj.CurrentRenderIndex = _renderIndex;
				if (!((float)num3 > _maxPixel.Y))
				{
					if (land.IsStretched)
					{
						num3 += land.Z << 2;
						num3 -= land.MinZ << 2;
					}
					if (!((float)num3 < _minPixel.Y))
					{
						PushToRenderList(obj, ref _renderList, ref _renderListStaticsHead, ref _renderListStaticsCount, allowSelection: true);
					}
				}
			}
			else if (obj is Static { ItemData: var itemData } @static)
			{
				if (!itemData.IsInternal && World.Season < (Season)254 && ProcessAlpha(obj, ref itemData, useCoT: true, ref playerScreePos, cotZ, out var allowSelection) && (!itemData.IsFoliage || !ProfileManager.CurrentProfile.TreeToStumps) && (itemData.IsMultiMovable || !@static.IsVegetation || !ProfileManager.CurrentProfile.HideVegetation))
				{
					byte b = 0;
					if (obj.AllowedToDraw)
					{
						b = CalculateObjectHeight(ref maxObjectZ, ref itemData);
					}
					if (maxObjectZ > maxZ)
					{
						if (itemData.Height != 0)
						{
							return maxObjectZ - maxZ < b;
						}
						return false;
					}
					obj.CurrentRenderIndex = _renderIndex;
					if (!((float)num3 < _minPixel.Y) && !((float)num3 > _maxPixel.Y))
					{
						CheckIfBehindATree(obj, worldX, worldY, ref itemData);
						if (num != @static.PriorityZ)
						{
							num = @static.PriorityZ;
							num2 = 0;
						}
						@static.InnerTileIndex = num2++;
						if (ProfileManager.CurrentProfile.ShadowsEnabled && ProfileManager.CurrentProfile.ShadowsStatics && (StaticFilters.IsTree(obj.Graphic, out index) || itemData.IsFoliage || StaticFilters.IsRock(obj.Graphic) || itemData.IsTranslucent || itemData.IsTransparent))
						{
							PushToRenderList(obj, ref _renderListTransparentObjects, ref _renderListTransparentObjectsHead, ref _renderListTransparentObjectsCount, allowSelection);
						}
						else
						{
							byte alphaHue = obj.AlphaHue;
							if (itemData.IsTranslucent || itemData.IsTransparent)
							{
								obj.AlphaHue = byte.MaxValue;
							}
							PushToRenderList(obj, ref _renderList, ref _renderListStaticsHead, ref _renderListStaticsCount, allowSelection);
							obj.AlphaHue = alphaHue;
						}
					}
				}
			}
			else if (obj is Multi { ItemData: var itemData2 } multi)
			{
				if (!itemData2.IsInternal && ProcessAlpha(obj, ref itemData2, useCoT: true, ref playerScreePos, cotZ, out var allowSelection2) && (itemData2.IsMultiMovable || ((!itemData2.IsFoliage || !ProfileManager.CurrentProfile.TreeToStumps) && (!multi.IsVegetation || !ProfileManager.CurrentProfile.HideVegetation))))
				{
					byte b2 = 0;
					if (obj.AllowedToDraw)
					{
						b2 = CalculateObjectHeight(ref maxObjectZ, ref itemData2);
					}
					if (maxObjectZ > maxZ)
					{
						if (itemData2.Height != 0)
						{
							return maxObjectZ - maxZ < b2;
						}
						return false;
					}
					obj.CurrentRenderIndex = _renderIndex;
					if (!((float)num3 < _minPixel.Y) && !((float)num3 > _maxPixel.Y))
					{
						CheckIfBehindATree(obj, worldX, worldY, ref itemData2);
						if (ProfileManager.CurrentProfile.ShadowsEnabled && ProfileManager.CurrentProfile.ShadowsStatics && (StaticFilters.IsTree(obj.Graphic, out index) || itemData2.IsFoliage || StaticFilters.IsRock(obj.Graphic)))
						{
							PushToRenderList(obj, ref _renderListTransparentObjects, ref _renderListTransparentObjectsHead, ref _renderListTransparentObjectsCount, allowSelection2);
						}
						else
						{
							byte alphaHue2 = obj.AlphaHue;
							if (itemData2.IsTranslucent || itemData2.IsTransparent)
							{
								obj.AlphaHue = byte.MaxValue;
							}
							PushToRenderList(obj, ref _renderList, ref _renderListStaticsHead, ref _renderListStaticsCount, allowSelection2);
							obj.AlphaHue = alphaHue2;
						}
					}
				}
			}
			else if (obj is Mobile mobile)
			{
				UpdateObjectHandles(mobile, useObjectHandles);
				maxObjectZ += 16;
				if (maxObjectZ > maxZ)
				{
					return false;
				}
				StaticTiles itemData3 = default(StaticTiles);
				if (ProcessAlpha(obj, ref itemData3, useCoT: false, ref playerScreePos, cotZ, out var allowSelection3))
				{
					obj.CurrentRenderIndex = _renderIndex;
					if (!((float)num3 < _minPixel.Y) && !((float)num3 > _maxPixel.Y))
					{
						obj.AllowedToDraw = !HasSurfaceOverhead(mobile);
						if (!mobile.IsDead)
						{
							PushToRenderList(obj, ref _renderListTransparentObjects, ref _renderListTransparentObjectsHead, ref _renderListTransparentObjectsCount, allowSelection3);
						}
						else
						{
							PushToRenderList(obj, ref _renderListAnimations, ref _renderListAnimationsHead, ref _renderListAnimationCount, allowSelection3);
						}
					}
				}
			}
			else if (obj is Item item)
			{
				ref StaticTiles reference = ref item.IsMulti ? ref TileDataLoader.Instance.StaticData[item.MultiGraphic] : ref item.ItemData;
				if (item.IsCorpse || !reference.IsInternal)
				{
					if (item.IsCorpse || (!item.IsMulti && (!item.IsLocked || (item.IsLocked && reference.IsContainer))))
					{
						UpdateObjectHandles(item, useObjectHandles);
					}
					if (item.IsMulti && World.Player != null && World.Player.NotorietyFlag == NotorietyFlag.Staff)
					{
						UpdateObjectHandles(item, useObjectHandles);
					}
					if (ProcessAlpha(obj, ref reference, useCoT: false, ref playerScreePos, cotZ, out var allowSelection4) && (reference.IsMultiMovable || !reference.IsFoliage || !ProfileManager.CurrentProfile.TreeToStumps))
					{
						byte b3 = 0;
						if (obj.AllowedToDraw)
						{
							b3 = CalculateObjectHeight(ref maxObjectZ, ref reference);
						}
						if (maxObjectZ > maxZ)
						{
							if (reference.Height != 0)
							{
								return maxObjectZ - maxZ < b3;
							}
							return false;
						}
						obj.CurrentRenderIndex = _renderIndex;
						if (!((float)num3 < _minPixel.Y) && !((float)num3 > _maxPixel.Y))
						{
							if (!item.IsCorpse)
							{
								_ = reference.IsMultiMovable;
							}
							if (!item.IsCorpse)
							{
								CheckIfBehindATree(obj, worldX, worldY, ref reference);
							}
							if (num != item.PriorityZ)
							{
								num = item.PriorityZ;
								num2 = 0;
							}
							item.InnerTileIndex = num2++;
							if (item.IsCorpse)
							{
								PushToRenderList(obj, ref _renderListAnimations, ref _renderListAnimationsHead, ref _renderListAnimationCount, allowSelection4);
							}
							else
							{
								PushToRenderList(obj, ref _renderList, ref _renderListStaticsHead, ref _renderListStaticsCount, allowSelection: true);
							}
						}
					}
				}
			}
			else if (obj is GameEffect gameEffect && ProcessAlpha(obj, ref TileDataLoader.Instance.StaticData[gameEffect.Graphic], useCoT: false, ref playerScreePos, cotZ, out allowSelection5))
			{
				obj.CurrentRenderIndex = _renderIndex;
				if (!((float)num3 < _minPixel.Y) && !((float)num3 > _maxPixel.Y))
				{
					_ = gameEffect.IsMoving;
					Mobile mobile2 = gameEffect.Source as Mobile;
					if (mobile2 != null && !mobile2.IsDead)
					{
						PushToRenderList(obj, ref _renderListTransparentObjects, ref _renderListTransparentObjectsHead, ref _renderListTransparentObjectsCount, allowSelection: false);
					}
					else if (gameEffect is FixedEffect)
					{
						PushToRenderList(obj, ref _renderListAnimations, ref _renderListAnimationsHead, ref _renderListAnimationCount, allowSelection: false);
					}
					else
					{
						PushToRenderList(obj, ref _renderList, ref _renderListStaticsHead, ref _renderListStaticsCount, allowSelection: false);
					}
				}
			}
			obj = obj.TNext;
		}
		return false;
	}

	private void GetViewPort()
	{
		int x = _offset.X;
		int y = _offset.Y;
		Point last_scaled_offset = _last_scaled_offset;
		float zoom = base.Camera.Zoom;
		int num = 0;
		int num2 = 0;
		int x2 = ProfileManager.CurrentProfile.GameWindowSize.X;
		int y2 = ProfileManager.CurrentProfile.GameWindowSize.Y;
		int num3 = num + (x2 >> 1);
		int num4 = num2 + (y2 >> 1) + (World.Player.Z << 2);
		num3 -= (int)World.Player.CumulativeOffset.X;
		num4 -= (int)(World.Player.CumulativeOffset.Y - World.Player.CumulativeOffset.Z);
		ushort x3 = World.Player.X;
		int y3 = World.Player.Y;
		int num5 = (x3 - y3) * 22 - num3;
		int num6 = (x3 + y3) * 22 - num4;
		int num13;
		int num14;
		if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.EnableMousewheelScaleZoom)
		{
			float num7 = num;
			float num8 = (float)x2 + num7;
			float num9 = num2;
			float num10 = (float)y2 + num9;
			float num11 = num8 * zoom;
			float num12 = num10 * zoom;
			num13 = (int)(num7 * zoom - (num11 - num8));
			num14 = (int)(num9 * zoom - (num12 - num10));
		}
		else
		{
			num13 = 0;
			num14 = 0;
		}
		int num15 = (int)((float)(x2 / 44 + 1) * zoom);
		int num16 = (int)((float)(y2 / 44 + 1) * zoom);
		if (num15 < num16)
		{
			num15 = num16;
		}
		else
		{
			num16 = num15;
		}
		int num17 = x3 - num15;
		if (num17 < 0)
		{
			num17 = 0;
		}
		int num18 = x3 + num15;
		int num19 = y3 - num16;
		if (num19 < 0)
		{
			num19 = 0;
		}
		int num20 = y3 + num16;
		_ = (num17 >> 3) - 1;
		int num21 = (num19 >> 3) - 1;
		int num22 = (num18 >> 3) + 1;
		int num23 = (num20 >> 3) + 1;
		_ = 0;
		if (num21 < 0)
		{
			num21 = 0;
		}
		if (num22 >= MapLoader.Instance.MapsDefaultSize[World.Map.Index, 0])
		{
			num22 = MapLoader.Instance.MapsDefaultSize[World.Map.Index, 0] - 1;
		}
		if (num23 >= MapLoader.Instance.MapsDefaultSize[World.Map.Index, 1])
		{
			num23 = MapLoader.Instance.MapsDefaultSize[World.Map.Index, 1] - 1;
		}
		int num24 = (int)(44f / zoom);
		Point zero = Point.Zero;
		zero.X -= num24;
		zero.Y -= num24;
		zero = base.Camera.ScreenToWorld(zero);
		int x4 = zero.X;
		int y4 = zero.Y;
		zero.X = base.Camera.Bounds.Width + num24;
		zero.Y = base.Camera.Bounds.Height + num24;
		zero = base.Camera.ScreenToWorld(zero);
		int x5 = zero.X;
		int y5 = zero.Y;
		if (UpdateDrawPosition || x != num5 || y != num6 || last_scaled_offset.X != num13 || last_scaled_offset.Y != num14)
		{
			UpdateDrawPosition = true;
			if (_use_render_target && (_world_render_target == null || _world_render_target.Width != (int)((float)x2 * zoom) || _world_render_target.Height != (int)((float)y2 * zoom)))
			{
				_world_render_target?.Dispose();
				PresentationParameters presentationParameters = Client.Game.GraphicsDevice.PresentationParameters;
				_world_render_target = new RenderTarget2D(Client.Game.GraphicsDevice, x2, y2, mipMap: false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, presentationParameters.MultiSampleCount, presentationParameters.RenderTargetUsage);
			}
			if (_lightRenderTarget == null || _lightRenderTarget.Width != x2 || _lightRenderTarget.Height != y2)
			{
				_lightRenderTarget?.Dispose();
				PresentationParameters presentationParameters2 = Client.Game.GraphicsDevice.PresentationParameters;
				_lightRenderTarget = new RenderTarget2D(Client.Game.GraphicsDevice, x2, y2, mipMap: false, presentationParameters2.BackBufferFormat, presentationParameters2.DepthStencilFormat, presentationParameters2.MultiSampleCount, presentationParameters2.RenderTargetUsage);
			}
		}
		_minTile.X = num17;
		_minTile.Y = num19;
		_maxTile.X = num18;
		_maxTile.Y = num20;
		_minPixel.X = x4;
		_minPixel.Y = y4;
		_maxPixel.X = x5;
		_maxPixel.Y = y5;
		_offset.X = num5;
		_offset.Y = num6;
		_last_scaled_offset.X = num13;
		_last_scaled_offset.Y = num14;
		UpdateMaxDrawZ();
	}

	private bool MoveCharacterByMouseInput()
	{
		if ((_rightMousePressed || _continueRunning) && World.InGame)
		{
			if (Pathfinder.AutoWalking)
			{
				Pathfinder.StopAutoWalk();
			}
			int num = ProfileManager.CurrentProfile.GameWindowPosition.X + (ProfileManager.CurrentProfile.GameWindowSize.X >> 1);
			int num2 = ProfileManager.CurrentProfile.GameWindowPosition.Y + (ProfileManager.CurrentProfile.GameWindowSize.Y >> 1);
			Direction direction = (Direction)GameCursor.GetMouseDirection(num, num2, Mouse.Position.X, Mouse.Position.Y, 1);
			double num3 = ClassicUO.Utility.MathHelper.Hypotenuse(num - Mouse.Position.X, num2 - Mouse.Position.Y);
			Direction direction2 = direction;
			if (direction2 == Direction.North)
			{
				direction2 = (Direction)8;
			}
			bool flag = num3 >= 190.0;
			if (World.Player.IsDrivingBoat)
			{
				if (!_boatIsMoving || _boatRun != flag || _lastBoatDirection != direction2 - 1)
				{
					_boatRun = flag;
					_lastBoatDirection = direction2 - 1;
					_boatIsMoving = true;
					BoatMovingManager.MoveRequest(direction2 - 1, (byte)((!flag) ? 1u : 2u));
				}
			}
			else
			{
				World.Player.Walk(direction2 - 1, flag);
			}
			return true;
		}
		return false;
	}

	private bool CanDragSelectOnObject(GameObject obj)
	{
		if (obj != null && !(obj is Static) && !(obj is Land) && !(obj is Multi))
		{
			if (obj is Item item)
			{
				return item.IsLocked;
			}
			return false;
		}
		return true;
	}

	private bool DragSelectModifierActive()
	{
		if (Keyboard.Ctrl && Keyboard.Shift)
		{
			return false;
		}
		if (ProfileManager.CurrentProfile.DragSelectModifierKey == 0)
		{
			return true;
		}
		if (ProfileManager.CurrentProfile.DragSelectModifierKey == 1 && Keyboard.Ctrl)
		{
			return true;
		}
		if (ProfileManager.CurrentProfile.DragSelectModifierKey == 2 && Keyboard.Shift)
		{
			return true;
		}
		return false;
	}

	private void DoDragSelect()
	{
		if (_selectionStart.X > Mouse.Position.X)
		{
			_selectionEnd.X = _selectionStart.X;
			_selectionStart.X = Mouse.Position.X;
		}
		else
		{
			_selectionEnd.X = Mouse.Position.X;
		}
		if (_selectionStart.Y > Mouse.Position.Y)
		{
			_selectionEnd.Y = _selectionStart.Y;
			_selectionStart.Y = Mouse.Position.Y;
		}
		else
		{
			_selectionEnd.Y = Mouse.Position.Y;
		}
		_rectangleObj.X = _selectionStart.X - base.Camera.Bounds.X;
		_rectangleObj.Y = _selectionStart.Y - base.Camera.Bounds.Y;
		_rectangleObj.Width = _selectionEnd.X - base.Camera.Bounds.X - _rectangleObj.X;
		_rectangleObj.Height = _selectionEnd.Y - base.Camera.Bounds.Y - _rectangleObj.Y;
		int num = ProfileManager.CurrentProfile.DragSelectStartX;
		int num2 = ProfileManager.CurrentProfile.DragSelectStartY;
		bool customBarsToggled = ProfileManager.CurrentProfile.CustomBarsToggled;
		Rectangle bounds;
		if (customBarsToggled)
		{
			bounds = new Rectangle(0, 0, 100, 60);
		}
		else
		{
			GumpsLoader.Instance.GetGumpTexture(2052u, out bounds);
		}
		foreach (Mobile value in World.Mobiles.Values)
		{
			if ((ProfileManager.CurrentProfile.DragSelectHumanoidsOnly && !value.IsHuman) || (value.IsInvisibleAnimation() && value.IsYellowHits) || value.IgnoreCharacters)
			{
				continue;
			}
			Point realScreenPosition = value.RealScreenPosition;
			realScreenPosition.X += (int)value.CumulativeOffset.X + 22 + 5;
			realScreenPosition.Y += (int)(value.CumulativeOffset.Y - value.CumulativeOffset.Z) + 12 * AnchorOffset;
			realScreenPosition.X -= value.FrameInfo.X;
			realScreenPosition.Y -= value.FrameInfo.Y;
			Point point = new Point(realScreenPosition.X + value.FrameInfo.Width, realScreenPosition.Y + value.FrameInfo.Height);
			realScreenPosition = base.Camera.WorldToScreen(realScreenPosition);
			_rectanglePlayer.X = realScreenPosition.X;
			_rectanglePlayer.Y = realScreenPosition.Y;
			point = base.Camera.WorldToScreen(point);
			_rectanglePlayer.Width = point.X - realScreenPosition.X;
			_rectanglePlayer.Height = point.Y - realScreenPosition.Y;
			if (!_rectangleObj.Intersects(_rectanglePlayer) || !(value != World.Player) || UIManager.GetGump<BaseHealthBarGump>(value) != null)
			{
				continue;
			}
			BaseHealthBarGump baseHealthBarGump = ((!customBarsToggled) ? ((BaseHealthBarGump)new HealthBarGump(value)) : ((BaseHealthBarGump)new HealthBarGumpCustom(value)));
			if (num2 >= ProfileManager.CurrentProfile.GameWindowPosition.Y + ProfileManager.CurrentProfile.GameWindowSize.Y - 20)
			{
				num2 = ProfileManager.CurrentProfile.DragSelectStartY;
				num += bounds.Width + 2;
			}
			if (num >= ProfileManager.CurrentProfile.GameWindowPosition.X + ProfileManager.CurrentProfile.GameWindowSize.X - 20)
			{
				num = ProfileManager.CurrentProfile.DragSelectStartX;
			}
			baseHealthBarGump.X = num;
			baseHealthBarGump.Y = num2;
			foreach (BaseHealthBarGump item in from s in UIManager.Gumps.OfType<BaseHealthBarGump>()
				orderby s.ScreenCoordinateX, s.ScreenCoordinateY
				select s)
			{
				if (item.Bounds.Intersects(baseHealthBarGump.Bounds))
				{
					num2 = item.Bounds.Bottom + AnchorOffset;
					if (num2 >= ProfileManager.CurrentProfile.GameWindowPosition.Y + ProfileManager.CurrentProfile.GameWindowSize.Y - 100)
					{
						num2 = ProfileManager.CurrentProfile.DragSelectStartY;
						num = item.Bounds.Right + AnchorOffset;
					}
					if (num >= ProfileManager.CurrentProfile.GameWindowPosition.X + ProfileManager.CurrentProfile.GameWindowSize.X - 100)
					{
						num = ProfileManager.CurrentProfile.DragSelectStartX;
					}
					baseHealthBarGump.X = num;
					baseHealthBarGump.Y = num2;
					if (ProfileManager.CurrentProfile.DragSelectAsAnchor)
					{
						baseHealthBarGump.TryAttacheToExist();
					}
				}
			}
			if (!ProfileManager.CurrentProfile.DragSelectAsAnchor)
			{
				num2 += bounds.Height + 2;
			}
			UIManager.Add(baseHealthBarGump);
			baseHealthBarGump.SetInScreen();
		}
		_isSelectionActive = false;
	}

	internal override bool OnMouseDown(MouseButtonType button)
	{
		return button switch
		{
			MouseButtonType.Left => OnLeftMouseDown(), 
			MouseButtonType.Right => OnRightMouseDown(), 
			MouseButtonType.Middle => OnMiddleMouseDown(), 
			MouseButtonType.XButton1 => OnMouse5Down(), 
			MouseButtonType.XButton2 => OnMouse4Down(), 
			_ => false, 
		};
	}

	private bool OnMiddleMouseDown()
	{
		SDL.SDL_Keymod sDL_Keymod = SDL.SDL_Keymod.KMOD_NONE;
		if (Keyboard.Ctrl)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_CTRL;
		}
		if (Keyboard.Alt)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_ALT;
		}
		if (Keyboard.Shift)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_SHIFT;
		}
		UIManager.KeyboardFocusControl?.InvokeKeyDown(SDL.SDL_Keycode.SDLK_F20, sDL_Keymod);
		SDL.SDL_KeyboardEvent e = default(SDL.SDL_KeyboardEvent);
		e.keysym.sym = SDL.SDL_Keycode.SDLK_F20;
		if (UIManager.IsMouseOverWorld)
		{
			OnKeyDown(e);
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		return true;
	}

	private bool OnMouse4Down()
	{
		SDL.SDL_Keymod sDL_Keymod = SDL.SDL_Keymod.KMOD_NONE;
		if (Keyboard.Ctrl)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_CTRL;
		}
		if (Keyboard.Alt)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_ALT;
		}
		if (Keyboard.Shift)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_SHIFT;
		}
		UIManager.KeyboardFocusControl?.InvokeKeyDown(SDL.SDL_Keycode.SDLK_F21, sDL_Keymod);
		SDL.SDL_KeyboardEvent e = default(SDL.SDL_KeyboardEvent);
		e.keysym.sym = SDL.SDL_Keycode.SDLK_F21;
		if (UIManager.IsMouseOverWorld)
		{
			OnKeyDown(e);
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		return true;
	}

	private bool OnMouse5Down()
	{
		SDL.SDL_Keymod sDL_Keymod = SDL.SDL_Keymod.KMOD_NONE;
		if (Keyboard.Ctrl)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_CTRL;
		}
		if (Keyboard.Alt)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_ALT;
		}
		if (Keyboard.Shift)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_SHIFT;
		}
		UIManager.KeyboardFocusControl?.InvokeKeyDown(SDL.SDL_Keycode.SDLK_F22, sDL_Keymod);
		SDL.SDL_KeyboardEvent e = default(SDL.SDL_KeyboardEvent);
		e.keysym.sym = SDL.SDL_Keycode.SDLK_F22;
		if (UIManager.IsMouseOverWorld)
		{
			OnKeyDown(e);
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		return true;
	}

	internal override bool OnMouseUp(MouseButtonType button)
	{
		return button switch
		{
			MouseButtonType.Left => OnLeftMouseUp(), 
			MouseButtonType.Right => OnRightMouseUp(), 
			_ => false, 
		};
	}

	internal override bool OnMouseDoubleClick(MouseButtonType button)
	{
		return button switch
		{
			MouseButtonType.Left => OnLeftMouseDoubleClick(), 
			MouseButtonType.Right => OnRightMouseDoubleClick(), 
			_ => false, 
		};
	}

	private bool OnLeftMouseDown()
	{
		if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
		{
			UIManager.ShowGamePopup(null);
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		if (World.CustomHouseManager != null)
		{
			_isMouseLeftDown = true;
			if (TargetManager.IsTargeting && TargetManager.TargetingState == CursorTarget.MultiPlacement && (World.CustomHouseManager.SelectedGraphic != 0 || World.CustomHouseManager.Erasing || World.CustomHouseManager.SeekTile) && SelectedObject.LastObject is GameObject gameObject)
			{
				World.CustomHouseManager.OnTargetWorld(gameObject);
				_lastSelectedMultiPositionInHouseCustomization.X = gameObject.X;
				_lastSelectedMultiPositionInHouseCustomization.Y = gameObject.Y;
			}
		}
		else
		{
			SelectedObject.LastLeftDownObject = SelectedObject.Object;
			if (ProfileManager.CurrentProfile.EnableDragSelect && DragSelectModifierActive())
			{
				if (CanDragSelectOnObject(SelectedObject.Object as GameObject))
				{
					_selectionStart = Mouse.Position;
					_isSelectionActive = true;
				}
			}
			else
			{
				_isMouseLeftDown = true;
				_holdMouse2secOverItemTime = Time.Ticks;
			}
		}
		return true;
	}

	private bool OnLeftMouseUp()
	{
		if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
		{
			UIManager.ShowGamePopup(null);
		}
		if (_isMouseLeftDown)
		{
			_isMouseLeftDown = false;
			_holdMouse2secOverItemTime = 0u;
		}
		if (_selectionStart.X == Mouse.Position.X && _selectionStart.Y == Mouse.Position.Y)
		{
			_isSelectionActive = false;
		}
		if (_isSelectionActive)
		{
			DoDragSelect();
			return true;
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		if (UIManager.SystemChat != null && !UIManager.SystemChat.IsFocused)
		{
			UIManager.KeyboardFocusControl = null;
			UIManager.SystemChat.SetFocus();
		}
		if (!ProfileManager.CurrentProfile.DisableAutoMove && _rightMousePressed)
		{
			_continueRunning = true;
		}
		BaseGameObject lastObject = SelectedObject.LastObject;
		SelectedObject.LastLeftDownObject = null;
		if (UIManager.IsDragging)
		{
			return false;
		}
		if (ItemHold.Enabled && !ItemHold.IsFixedPosition)
		{
			uint num = uint.MaxValue;
			bool flag = false;
			ushort num2 = 0;
			ushort num3 = 0;
			sbyte b = 0;
			GameObject gameObject = SelectedObject.LastObject as GameObject;
			if (gameObject is Entity entity)
			{
				flag = entity.Distance <= 3 || World.Player.NotorietyFlag == NotorietyFlag.Staff;
				if (flag)
				{
					if ((entity is Item item && item.ItemData.IsContainer) || entity is Mobile)
					{
						num2 = ushort.MaxValue;
						num3 = ushort.MaxValue;
						b = 0;
						num = entity.Serial;
					}
					else if (entity is Item item2 && (item2.ItemData.IsSurface || (item2.ItemData.IsStackable && item2.Graphic == ItemHold.Graphic)))
					{
						num2 = entity.X;
						num3 = entity.Y;
						b = entity.Z;
						if (item2.ItemData.IsSurface)
						{
							b += (sbyte)((item2.ItemData.Height != byte.MaxValue) ? item2.ItemData.Height : 0);
						}
						else
						{
							num = entity.Serial;
						}
					}
				}
				else
				{
					Client.Game.Scene.Audio.PlaySound(81);
				}
			}
			else if (gameObject is Land || gameObject is Static || gameObject is Multi)
			{
				flag = gameObject.Distance <= 3 || World.Player.NotorietyFlag == NotorietyFlag.Staff;
				if (flag)
				{
					num2 = gameObject.X;
					num3 = gameObject.Y;
					b = gameObject.Z;
					if (!(gameObject is Land))
					{
						ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[gameObject.Graphic];
						if (reference.IsSurface)
						{
							b += (sbyte)((reference.Height != byte.MaxValue) ? reference.Height : 0);
						}
					}
				}
				else
				{
					Client.Game.Scene.Audio.PlaySound(81);
				}
			}
			if (flag)
			{
				if (num == uint.MaxValue && num2 == 0 && num3 == 0)
				{
					flag = false;
				}
				if (flag)
				{
					GameActions.DropItem(ItemHold.Serial, num2, num3, b, num);
				}
			}
		}
		else if (TargetManager.IsTargeting)
		{
			switch (TargetManager.TargetingState)
			{
			case CursorTarget.MultiPlacement:
				if (World.CustomHouseManager != null)
				{
					break;
				}
				goto case CursorTarget.Object;
			case CursorTarget.Object:
			case CursorTarget.Position:
			case CursorTarget.Grab:
			case CursorTarget.SetGrabBag:
			{
				BaseGameObject baseGameObject = lastObject;
				if (baseGameObject is TextObject textObject)
				{
					baseGameObject = textObject.Owner;
				}
				if (!(baseGameObject is Entity entity3))
				{
					if (!(baseGameObject is Land land))
					{
						if (!(baseGameObject is Multi multi))
						{
							if (baseGameObject is GameObject gameObject2)
							{
								TargetManager.Target(gameObject2.Graphic, gameObject2.X, gameObject2.Y, gameObject2.Z);
							}
						}
						else
						{
							TargetManager.SendMultiTarget(multi.Graphic, multi.X, multi.Y, multi.Z, multi.Hue);
						}
					}
					else
					{
						TargetManager.Target(0, land.X, land.Y, land.Z, land.TileData.IsWet);
					}
				}
				else
				{
					TargetManager.Target(entity3.Serial);
				}
				Mouse.LastLeftButtonClickTime = 0u;
				break;
			}
			case CursorTarget.SetTargetClientSide:
			{
				BaseGameObject baseGameObject2 = lastObject;
				if (baseGameObject2 is TextObject textObject2)
				{
					baseGameObject2 = textObject2.Owner;
				}
				else if (baseGameObject2 is GameEffect { Source: not null } gameEffect)
				{
					baseGameObject2 = gameEffect.Source;
				}
				if (!(baseGameObject2 is Entity entity6))
				{
					if (!(baseGameObject2 is Land land2))
					{
						if (baseGameObject2 is GameObject gameObject3)
						{
							TargetManager.Target(gameObject3.Graphic, gameObject3.X, gameObject3.Y, gameObject3.Z);
							UIManager.Add(new InspectorGump(gameObject3));
						}
					}
					else
					{
						TargetManager.Target(0, land2.X, land2.Y, land2.Z);
						UIManager.Add(new InspectorGump(land2));
					}
				}
				else
				{
					TargetManager.Target(entity6.Serial);
					UIManager.Add(new InspectorGump(entity6));
				}
				Mouse.LastLeftButtonClickTime = 0u;
				break;
			}
			case CursorTarget.HueCommandTarget:
				if (SelectedObject.Object is Entity entity4)
				{
					CommandManager.OnHueTarget(entity4);
				}
				break;
			case CursorTarget.IgnorePlayerTarget:
				if (SelectedObject.Object is Entity entity5)
				{
					IgnoreManager.AddIgnoredTarget(entity5);
				}
				TargetManager.CancelTarget();
				break;
			case CursorTarget.AddAdditionalJournalTarget:
				if (SelectedObject.Object is Entity entity2)
				{
					AdditionalJournalListGump.AddAdditionalJournalDictEntry(entity2);
				}
				TargetManager.CancelTarget();
				break;
			}
		}
		else
		{
			GameObject gameObject4 = lastObject as GameObject;
			if (!(gameObject4 is Static @static))
			{
				if (!(gameObject4 is Multi { Name: var text } multi2))
				{
					if (gameObject4 is Entity entity7)
					{
						if (Keyboard.Alt && entity7 is Mobile)
						{
							MessageManager.HandleMessage(World.Player, ResGeneral.NowFollowing, string.Empty, 0, MessageType.Regular, 3, TextType.CLIENT);
							_followingMode = true;
							_followingTarget = entity7;
						}
						else if (!DelayedObjectClickManager.IsEnabled)
						{
							DelayedObjectClickManager.Set(entity7.Serial, Mouse.Position.X, Mouse.Position.Y, Time.Ticks + 350);
						}
					}
				}
				else
				{
					if (string.IsNullOrEmpty(text))
					{
						text = ClilocLoader.Instance.GetString(1020000 + multi2.Graphic, multi2.ItemData.Name);
					}
					MessageManager.HandleMessage(null, text, string.Empty, 946, MessageType.Label, 3, TextType.CLIENT);
					gameObject4.AddMessage(MessageType.Label, text, 3, 946, isunicode: false, TextType.CLIENT);
					if (gameObject4.TextContainer != null && gameObject4.TextContainer.MaxSize == 5)
					{
						gameObject4.TextContainer.MaxSize = 1;
					}
				}
			}
			else
			{
				string text2 = StringHelper.GetPluralAdjustedString(@static.Name, @static.ItemData.Count > 1);
				if (string.IsNullOrEmpty(text2))
				{
					text2 = ClilocLoader.Instance.GetString(1020000 + @static.Graphic, @static.ItemData.Name);
				}
				MessageManager.HandleMessage(null, text2, string.Empty, 946, MessageType.Label, 3, TextType.CLIENT);
				gameObject4.AddMessage(MessageType.Label, text2, 3, 946, isunicode: false, TextType.CLIENT);
				if (gameObject4.TextContainer != null && gameObject4.TextContainer.MaxSize != 1)
				{
					gameObject4.TextContainer.MaxSize = 1;
				}
			}
		}
		return true;
	}

	private bool OnLeftMouseDoubleClick()
	{
		bool flag = false;
		if (!UIManager.IsMouseOverWorld)
		{
			if (DelayedObjectClickManager.IsEnabled)
			{
				DelayedObjectClickManager.Clear();
				return false;
			}
			return false;
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
		return flag;
	}

	private bool OnRightMouseDown()
	{
		if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
		{
			UIManager.ShowGamePopup(null);
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		_rightMousePressed = true;
		_continueRunning = false;
		StopFollowing();
		return true;
	}

	private bool OnRightMouseUp()
	{
		if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
		{
			UIManager.ShowGamePopup(null);
		}
		_rightMousePressed = false;
		if (_boatIsMoving)
		{
			_boatIsMoving = false;
			BoatMovingManager.MoveRequest(World.Player.Direction, 0);
		}
		return UIManager.IsMouseOverWorld;
	}

	private bool OnRightMouseDoubleClick()
	{
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		if (ProfileManager.CurrentProfile.EnablePathfind && !Pathfinder.AutoWalking)
		{
			if (ProfileManager.CurrentProfile.UseShiftToPathfind && !Keyboard.Shift)
			{
				return false;
			}
			if (SelectedObject.Object is GameObject gameObject)
			{
				if (gameObject is Static || gameObject is Multi || gameObject is Item)
				{
					if (TileDataLoader.Instance.StaticData[gameObject.Graphic].IsSurface && Pathfinder.WalkTo(gameObject.X, gameObject.Y, gameObject.Z, 0))
					{
						World.Player.AddMessage(MessageType.Label, ResGeneral.Pathfinding, 3, 0, isunicode: false, TextType.CLIENT);
						return true;
					}
				}
				else if (gameObject is Land && Pathfinder.WalkTo(gameObject.X, gameObject.Y, gameObject.Z, 0))
				{
					World.Player.AddMessage(MessageType.Label, ResGeneral.Pathfinding, 3, 0, isunicode: false, TextType.CLIENT);
					return true;
				}
			}
		}
		return false;
	}

	internal override bool OnMouseWheel(bool up)
	{
		if (Keyboard.Ctrl && ItemHold.Enabled)
		{
			if (!up && !ItemHold.IsFixedPosition)
			{
				ItemHold.IsFixedPosition = true;
				ItemHold.IgnoreFixedPosition = true;
				ItemHold.FixedX = Mouse.Position.X;
				ItemHold.FixedY = Mouse.Position.Y;
			}
			if (ItemHold.IgnoreFixedPosition)
			{
				return true;
			}
		}
		SDL.SDL_Keymod sDL_Keymod = SDL.SDL_Keymod.KMOD_NONE;
		if (Keyboard.Ctrl)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_CTRL;
		}
		if (Keyboard.Alt)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_ALT;
		}
		if (Keyboard.Shift)
		{
			sDL_Keymod |= SDL.SDL_Keymod.KMOD_SHIFT;
		}
		if (up)
		{
			UIManager.KeyboardFocusControl?.InvokeKeyDown(SDL.SDL_Keycode.SDLK_F23, sDL_Keymod);
			SDL.SDL_KeyboardEvent e = default(SDL.SDL_KeyboardEvent);
			e.keysym.sym = SDL.SDL_Keycode.SDLK_F23;
			if (UIManager.IsMouseOverWorld)
			{
				OnKeyDown(e);
			}
		}
		else
		{
			UIManager.KeyboardFocusControl?.InvokeKeyDown(SDL.SDL_Keycode.SDLK_F24, sDL_Keymod);
			SDL.SDL_KeyboardEvent e2 = default(SDL.SDL_KeyboardEvent);
			e2.keysym.sym = SDL.SDL_Keycode.SDLK_F24;
			if (UIManager.IsMouseOverWorld)
			{
				OnKeyDown(e2);
			}
		}
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		if (Keyboard.Ctrl && ProfileManager.CurrentProfile.EnableMousewheelScaleZoom)
		{
			base.Camera.ZoomIndex += ((!up) ? 1 : (-1));
			return true;
		}
		return false;
	}

	internal override bool OnMouseDragging()
	{
		if (!UIManager.IsMouseOverWorld)
		{
			return false;
		}
		bool result = true;
		if (Mouse.LButtonPressed && !ItemHold.Enabled)
		{
			Point lDragOffset = Mouse.LDragOffset;
			if (!UIManager.GameCursor.IsDraggingCursorForced && !_isSelectionActive && (Math.Abs(lDragOffset.X) > 5 || Math.Abs(lDragOffset.Y) > 5))
			{
				Entity entity = SelectedObject.LastLeftDownObject as Entity;
				if (entity != null)
				{
					if (SerialHelper.IsMobile(entity.Serial) || entity is Item { IsDamageable: not false })
					{
						UIManager.GetGump<BaseHealthBarGump>(entity)?.Dispose();
						BaseHealthBarGump control;
						if (ProfileManager.CurrentProfile.CustomBarsToggled)
						{
							Rectangle rectangle = new Rectangle(0, 0, 120, 36);
							HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(entity);
							healthBarGumpCustom.X = Mouse.LClickPosition.X - (rectangle.Width >> 1);
							healthBarGumpCustom.Y = Mouse.LClickPosition.Y - (rectangle.Height >> 1);
							control = healthBarGumpCustom;
							UIManager.Add(healthBarGumpCustom);
						}
						else
						{
							GumpsLoader.Instance.GetGumpTexture(2052u, out var bounds);
							HealthBarGump healthBarGump = new HealthBarGump(entity);
							healthBarGump.X = Mouse.LClickPosition.X - (bounds.Width >> 1);
							healthBarGump.Y = Mouse.LClickPosition.Y - (bounds.Height >> 1);
							control = healthBarGump;
							UIManager.Add(healthBarGump);
						}
						UIManager.AttemptDragControl(control, attemptAlwaysSuccessful: true);
						result = false;
					}
					else if (entity is Item item2)
					{
						GameActions.PickUp(item2, Mouse.Position.X, Mouse.Position.Y, -1, null);
					}
				}
				SelectedObject.LastLeftDownObject = null;
			}
		}
		return result;
	}

	internal override void OnKeyDown(SDL.SDL_KeyboardEvent e)
	{
		if (e.keysym.sym == SDL.SDL_Keycode.SDLK_TAB && e.repeat != 0)
		{
			return;
		}
		if (e.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE && TargetManager.IsTargeting)
		{
			TargetManager.CancelTarget();
		}
		else if (e.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)
		{
			TargetManager.LoseTargetAndCloseHealthBar();
		}
		if (UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
		{
			return;
		}
		switch (e.keysym.sym)
		{
		case SDL.SDL_Keycode.SDLK_ESCAPE:
			if (Pathfinder.AutoWalking && Pathfinder.PathindingCanBeCancelled)
			{
				Pathfinder.StopAutoWalk();
			}
			break;
		case SDL.SDL_Keycode.SDLK_TAB:
			if (!ProfileManager.CurrentProfile.DisableTabBtn && ProfileManager.CurrentProfile.HoldDownKeyTab && !_requestedWarMode)
			{
				_requestedWarMode = true;
				if (!World.Player.InWarMode)
				{
					NetClient.Socket.Send_ChangeWarMode(state: true);
				}
			}
			break;
		case SDL.SDL_Keycode.SDLK_1:
			if (!Keyboard.Shift)
			{
				break;
			}
			goto IL_01ac;
		case SDL.SDL_Keycode.SDLK_BACKSLASH:
			if (Keyboard.Shift)
			{
				goto IL_01ac;
			}
			goto case SDL.SDL_Keycode.SDLK_EXCLAIM;
		case SDL.SDL_Keycode.SDLK_7:
			if (!Keyboard.Shift)
			{
				break;
			}
			goto IL_01ac;
		case SDL.SDL_Keycode.SDLK_COMMA:
			if (Keyboard.Shift)
			{
				goto IL_01ac;
			}
			goto case SDL.SDL_Keycode.SDLK_EXCLAIM;
		case SDL.SDL_Keycode.SDLK_PERIOD:
			if (Keyboard.Shift)
			{
				goto IL_01ac;
			}
			goto case SDL.SDL_Keycode.SDLK_EXCLAIM;
		case SDL.SDL_Keycode.SDLK_EXCLAIM:
		case SDL.SDL_Keycode.SDLK_MINUS:
		case SDL.SDL_Keycode.SDLK_SLASH:
		case SDL.SDL_Keycode.SDLK_COLON:
		case SDL.SDL_Keycode.SDLK_SEMICOLON:
		case SDL.SDL_Keycode.SDLK_LEFTBRACKET:
		case SDL.SDL_Keycode.SDLK_KP_MINUS:
		case SDL.SDL_Keycode.SDLK_KP_PERIOD:
			if (ProfileManager.CurrentProfile.ActivateChatAfterEnter && ProfileManager.CurrentProfile.ActivateChatAdditionalButtons && !UIManager.SystemChat.IsActive && !Keyboard.Shift && !Keyboard.Alt && !Keyboard.Ctrl)
			{
				UIManager.SystemChat.IsActive = true;
			}
			break;
		case SDL.SDL_Keycode.SDLK_RETURN:
		case SDL.SDL_Keycode.SDLK_KP_ENTER:
			{
				if (UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
				{
					break;
				}
				if (ProfileManager.CurrentProfile.ActivateChatAfterEnter)
				{
					UIManager.SystemChat.Mode = ChatMode.Default;
					if (!Keyboard.Shift || !ProfileManager.CurrentProfile.ActivateChatShiftEnterSupport)
					{
						UIManager.SystemChat.ToggleChatVisibility();
					}
				}
				return;
			}
			IL_01ac:
			if (ProfileManager.CurrentProfile.ActivateChatAfterEnter && ProfileManager.CurrentProfile.ActivateChatAdditionalButtons && !UIManager.SystemChat.IsActive)
			{
				UIManager.SystemChat.IsActive = true;
			}
			break;
		}
		if ((UIManager.KeyboardFocusControl == UIManager.SystemChat.TextBoxControl && UIManager.SystemChat.IsActive && ProfileManager.CurrentProfile.ActivateChatAfterEnter) || UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl || UIManager.SystemChat.Mode < ChatMode.Default)
		{
			return;
		}
		Macro macro = Macros.FindMacro(e.keysym.sym, Keyboard.Alt, Keyboard.Ctrl, Keyboard.Shift);
		if (macro != null && e.keysym.sym != 0)
		{
			if (macro.Items == null || !(macro.Items is MacroObject macroObject))
			{
				return;
			}
			if (macroObject.Code == MacroType.Walk)
			{
				_flags[4] = true;
				switch (macroObject.SubCode)
				{
				case MacroSubType.NW:
					_flags[0] = true;
					break;
				case MacroSubType.SW:
					_flags[1] = true;
					break;
				case MacroSubType.SE:
					_flags[2] = true;
					break;
				case MacroSubType.NE:
					_flags[3] = true;
					break;
				case MacroSubType.N:
					_flags[0] = true;
					_flags[3] = true;
					break;
				case MacroSubType.S:
					_flags[1] = true;
					_flags[2] = true;
					break;
				case MacroSubType.E:
					_flags[3] = true;
					_flags[2] = true;
					break;
				case MacroSubType.W:
					_flags[0] = true;
					_flags[1] = true;
					break;
				}
			}
			else
			{
				Macros.SetMacroToExecute(macroObject);
				Macros.WaitingBandageTarget = false;
				Macros.WaitForTargetTimer = 0L;
				Macros.Update();
			}
		}
		else if (string.IsNullOrEmpty(UIManager.SystemChat.TextBoxControl.Text))
		{
			switch (e.keysym.sym)
			{
			case SDL.SDL_Keycode.SDLK_UP:
				_flags[0] = true;
				break;
			case SDL.SDL_Keycode.SDLK_LEFT:
				_flags[1] = true;
				break;
			case SDL.SDL_Keycode.SDLK_DOWN:
				_flags[2] = true;
				break;
			case SDL.SDL_Keycode.SDLK_RIGHT:
				_flags[3] = true;
				break;
			case SDL.SDL_Keycode.SDLK_PAGEUP:
				World.Player.Walk(Direction.North, run: false);
				break;
			case SDL.SDL_Keycode.SDLK_PAGEDOWN:
				World.Player.Walk(Direction.East, run: false);
				break;
			case SDL.SDL_Keycode.SDLK_HOME:
				World.Player.Walk(Direction.West, run: false);
				break;
			case SDL.SDL_Keycode.SDLK_END:
				World.Player.Walk(Direction.South, run: false);
				break;
			case (SDL.SDL_Keycode)1073741900:
				break;
			}
		}
	}

	internal override void OnKeyUp(SDL.SDL_KeyboardEvent e)
	{
		if (!World.InGame)
		{
			return;
		}
		if (ProfileManager.CurrentProfile.EnableMousewheelScaleZoom && ProfileManager.CurrentProfile.RestoreScaleAfterUnpressCtrl && !Keyboard.Ctrl)
		{
			base.Camera.Zoom = ProfileManager.CurrentProfile.DefaultScale;
		}
		if (_flags[4])
		{
			Macro macro = Macros.FindMacro(e.keysym.sym, Keyboard.Alt, Keyboard.Ctrl, Keyboard.Shift);
			if (macro != null && e.keysym.sym != 0 && macro.Items != null && macro.Items is MacroObject { Code: MacroType.Walk } macroObject)
			{
				_flags[4] = false;
				switch (macroObject.SubCode)
				{
				case MacroSubType.NW:
					_flags[0] = false;
					break;
				case MacroSubType.SW:
					_flags[1] = false;
					break;
				case MacroSubType.SE:
					_flags[2] = false;
					break;
				case MacroSubType.NE:
					_flags[3] = false;
					break;
				case MacroSubType.N:
					_flags[0] = false;
					_flags[3] = false;
					break;
				case MacroSubType.S:
					_flags[1] = false;
					_flags[2] = false;
					break;
				case MacroSubType.E:
					_flags[3] = false;
					_flags[2] = false;
					break;
				case MacroSubType.W:
					_flags[0] = false;
					_flags[1] = false;
					break;
				}
				Macros.SetMacroToExecute(macroObject);
				Macros.WaitForTargetTimer = 0L;
				Macros.Update();
				for (int i = 0; i < 4; i++)
				{
					if (_flags[i])
					{
						_flags[4] = true;
						break;
					}
				}
			}
		}
		switch (e.keysym.sym)
		{
		case SDL.SDL_Keycode.SDLK_UP:
			_flags[0] = false;
			break;
		case SDL.SDL_Keycode.SDLK_LEFT:
			_flags[1] = false;
			break;
		case SDL.SDL_Keycode.SDLK_DOWN:
			_flags[2] = false;
			break;
		case SDL.SDL_Keycode.SDLK_RIGHT:
			_flags[3] = false;
			break;
		}
		if (e.keysym.sym != SDL.SDL_Keycode.SDLK_TAB || ProfileManager.CurrentProfile.DisableTabBtn)
		{
			return;
		}
		if (ProfileManager.CurrentProfile.HoldDownKeyTab)
		{
			if (_requestedWarMode)
			{
				NetClient.Socket.Send_ChangeWarMode(state: false);
				_requestedWarMode = false;
			}
		}
		else
		{
			GameActions.ToggleWarMode();
		}
	}
}
