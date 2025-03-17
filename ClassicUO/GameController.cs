using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
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

namespace ClassicUO;

internal class GameController : Microsoft.Xna.Framework.Game
{
	public struct SDL_SysWMEventMsg
	{
		public SDL.SDL_version version;

		public SDL.SDL_SYSWM_TYPE subsystem;

		public IntPtr hwnd;

		public uint msg;

		public ulong wparam;

		public ulong lparam;
	}

	public enum MyWindowMessages
	{
		WM_LBUTTONDOWN = 513,
		WM_LBUTTONUP = 514,
		WM_USER = 1024,
		WM_ALAMODLEFTCLICK = 1066,
		WM_ALAMODRIGHTCLICK = 1067,
		WM_ALAMODDOUBLECLICK = 1068
	}

	private bool _dragStarted;

	private SDL.SDL_EventFilter _filter;

	private readonly Texture2D[] _hueSamplers = new Texture2D[3];

	private bool _ignoreNextTextInput;

	private readonly float[] _intervalFixedUpdate = new float[2];

	private double _statisticsTimer;

	private double _totalElapsed;

	private double _currentFpsTime;

	private uint _totalFrames;

	private UltimaBatcher2D _uoSpriteBatch;

	private bool _suppressedDraw;

	private UOFontRenderer _fontRenderer;

	private SyncLoop _synchronizationContext;

	public readonly uint[] FrameDelay = new uint[2];

	public Scene Scene { get; private set; }

	public GraphicsDeviceManager GraphicManager { get; }

	public GameController()
	{
		GraphicManager = new GraphicsDeviceManager(this);
		GraphicManager.PreparingDeviceSettings += delegate(object? sender, PreparingDeviceSettingsEventArgs e)
		{
			e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
		};
		GraphicManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
		SetVSync(value: false);
		base.Window.ClientSizeChanged += WindowOnClientSizeChanged;
		base.Window.AllowUserResizing = true;
		base.Window.Title = $"Ultima Online - Alathair - {CUOEnviroment.Version}";
		base.IsMouseVisible = Settings.GlobalSettings.RunMouseInASeparateThread;
		base.IsFixedTimeStep = false;
		base.TargetElapsedTime = TimeSpan.FromMilliseconds(4.0);
		base.InactiveSleepTime = TimeSpan.Zero;
		_synchronizationContext = new SyncLoop();
		SynchronizationContext.SetSynchronizationContext(_synchronizationContext);
	}

	protected override void Initialize()
	{
		if (GraphicManager.GraphicsDevice.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
		{
			GraphicManager.GraphicsProfile = GraphicsProfile.HiDef;
		}
		GraphicManager.ApplyChanges();
		SetRefreshRate(Settings.GlobalSettings.FPS);
		_uoSpriteBatch = new UltimaBatcher2D(base.GraphicsDevice);
		SDL.SDL_EventState(SDL.SDL_EventType.SDL_SYSWMEVENT, 1);
		_filter = HandleSdlEvent;
		SDL.SDL_AddEventWatch(_filter, IntPtr.Zero);
		base.Initialize();
	}

	protected override void LoadContent()
	{
		base.LoadContent();
		uint[] array = ArrayPool<uint>.Shared.Rent(131072);
		try
		{
			HuesLoader.Instance.CreateShaderColors(array);
			_hueSamplers[0] = new Texture2D(base.GraphicsDevice, 32, 2048);
			_hueSamplers[0].SetData(array, 0, 65536);
			_hueSamplers[1] = new Texture2D(base.GraphicsDevice, 32, 2048);
			_hueSamplers[1].SetData(array, 65536, 65536);
		}
		finally
		{
			ArrayPool<uint>.Shared.Return(array, clearArray: true);
		}
		array = ArrayPool<uint>.Shared.Rent(2016);
		try
		{
			LightColors.CreateLookupTables(array);
			_hueSamplers[2] = new Texture2D(base.GraphicsDevice, 32, 63);
			_hueSamplers[2].SetData(array, 0, 2016);
		}
		finally
		{
			ArrayPool<uint>.Shared.Return(array, clearArray: true);
		}
		base.GraphicsDevice.Textures[1] = _hueSamplers[0];
		base.GraphicsDevice.Textures[2] = _hueSamplers[1];
		base.GraphicsDevice.Textures[3] = _hueSamplers[2];
		GumpsLoader.Instance.CreateAtlas(base.GraphicsDevice);
		LightsLoader.Instance.CreateAtlas(base.GraphicsDevice);
		AnimationsLoader.Instance.CreateAtlas(base.GraphicsDevice);
		_fontRenderer = new UOFontRenderer(base.GraphicsDevice);
		UIManager.InitializeGameCursor();
		AnimatedStaticsManager.Initialize();
		SetScene(new LoginScene());
		SetWindowPositionBySettings();
	}

	protected override void UnloadContent()
	{
		SDL.SDL_GetWindowBordersSize(base.Window.Handle, out var top, out var left, out var _, out var _);
		Settings.GlobalSettings.WindowPosition = new Point(Math.Max(0, base.Window.ClientBounds.X - left), Math.Max(0, base.Window.ClientBounds.Y - top));
		Scene?.Unload();
		Settings.GlobalSettings.Save();
		Plugin.OnClosing();
		ArtLoader.Instance.Dispose();
		GumpsLoader.Instance.Dispose();
		TexmapsLoader.Instance.Dispose();
		AnimationsLoader.Instance.Dispose();
		LightsLoader.Instance.Dispose();
		TileDataLoader.Instance.Dispose();
		AnimDataLoader.Instance.Dispose();
		ClilocLoader.Instance.Dispose();
		FontsLoader.Instance.Dispose();
		HuesLoader.Instance.Dispose();
		MapLoader.Instance.Dispose();
		MultiLoader.Instance.Dispose();
		MultiMapLoader.Instance.Dispose();
		ProfessionLoader.Instance.Dispose();
		SkillsLoader.Instance.Dispose();
		SoundsLoader.Instance.Dispose();
		SpeechesLoader.Instance.Dispose();
		Verdata.File?.Dispose();
		World.Map?.Destroy();
		base.UnloadContent();
	}

	public void SetWindowTitle(string title)
	{
		if (string.IsNullOrEmpty(title))
		{
			base.Window.Title = $"Ultima Online - Alathair - {CUOEnviroment.Version}";
		}
		else
		{
			base.Window.Title = $"Ultima Online - Alathair - {title} - {CUOEnviroment.Version}";
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetScene<T>() where T : Scene
	{
		return Scene as T;
	}

	public void SetScene(Scene scene)
	{
		Scene?.Dispose();
		Scene = scene;
		if (scene != null)
		{
			base.Window.AllowUserResizing = scene.CanResize;
			scene.Load();
		}
	}

	public void SetVSync(bool value)
	{
		GraphicManager.SynchronizeWithVerticalRetrace = value;
	}

	public void SetRefreshRate(int rate)
	{
		if (rate < 12)
		{
			rate = 12;
		}
		else if (rate > 250)
		{
			rate = 250;
		}
		float num = ((rate != 12) ? (1000f / (float)rate) : 80f);
		FrameDelay[0] = (FrameDelay[1] = (uint)num);
		FrameDelay[1] = FrameDelay[1] >> 1;
		Settings.GlobalSettings.FPS = rate;
		_intervalFixedUpdate[0] = num;
		_intervalFixedUpdate[1] = 217f;
	}

	private void SetWindowPosition(int x, int y)
	{
		SDL.SDL_SetWindowPosition(base.Window.Handle, x, y);
	}

	public void SetWindowSize(int width, int height)
	{
		GraphicManager.PreferredBackBufferWidth = width;
		GraphicManager.PreferredBackBufferHeight = height;
		GraphicManager.ApplyChanges();
	}

	public void SetWindowBorderless(bool borderless)
	{
		SDL.SDL_WindowFlags sDL_WindowFlags = (SDL.SDL_WindowFlags)SDL.SDL_GetWindowFlags(base.Window.Handle);
		if (!((sDL_WindowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0 && borderless) && ((sDL_WindowFlags & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != 0 || borderless))
		{
			SDL.SDL_SetWindowBordered(base.Window.Handle, (!borderless) ? SDL.SDL_bool.SDL_TRUE : SDL.SDL_bool.SDL_FALSE);
			SDL.SDL_GetCurrentDisplayMode(0, out var mode);
			int w = mode.w;
			int h = mode.h;
			if (borderless)
			{
				SetWindowSize(w, h);
				SDL.SDL_SetWindowPosition(base.Window.Handle, 0, 0);
			}
			else
			{
				SDL.SDL_GetWindowBordersSize(base.Window.Handle, out var top, out var _, out var bottom, out var _);
				SetWindowSize(w, h - (top - bottom));
				SetWindowPositionBySettings();
			}
			WorldViewportGump gump = UIManager.GetGump<WorldViewportGump>(null);
			if (gump != null && ProfileManager.CurrentProfile.GameWindowFullSize)
			{
				gump.ResizeGameWindow(new Point(w, h));
				gump.X = -5;
				gump.Y = -5;
			}
		}
	}

	public void MaximizeWindow()
	{
		SDL.SDL_MaximizeWindow(base.Window.Handle);
	}

	public bool IsWindowMaximized()
	{
		return (SDL.SDL_GetWindowFlags(base.Window.Handle) & 0x80) != 0;
	}

	public void RestoreWindow()
	{
		SDL.SDL_RestoreWindow(base.Window.Handle);
	}

	public void SetWindowPositionBySettings()
	{
		SDL.SDL_GetWindowBordersSize(base.Window.Handle, out var top, out var left, out var _, out var _);
		if (Settings.GlobalSettings.WindowPosition.HasValue)
		{
			int val = left + Settings.GlobalSettings.WindowPosition.Value.X;
			int val2 = top + Settings.GlobalSettings.WindowPosition.Value.Y;
			val = Math.Max(0, val);
			val2 = Math.Max(0, val2);
			SetWindowPosition(val, val2);
		}
	}

	protected override void Update(GameTime gameTime)
	{
		if (Profiler.InContext("OutOfContext"))
		{
			Profiler.ExitContext("OutOfContext");
		}
		Time.Ticks = (uint)gameTime.TotalGameTime.TotalMilliseconds;
		Mouse.Update();
		OnNetworkUpdate(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
		Plugin.Tick();
		if (Scene != null && Scene.IsLoaded && !Scene.IsDestroyed)
		{
			Profiler.EnterContext("Update");
			Scene.Update(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
			Profiler.ExitContext("Update");
		}
		UIManager.Update(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
		_totalElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
		_currentFpsTime += gameTime.ElapsedGameTime.TotalMilliseconds;
		if (_currentFpsTime >= 1000.0)
		{
			CUOEnviroment.CurrentRefreshRate = _totalFrames;
			_totalFrames = 0u;
			_currentFpsTime = 0.0;
		}
		double num = _intervalFixedUpdate[(!base.IsActive && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ReduceFPSWhenInactive) ? 1 : 0];
		_suppressedDraw = false;
		if (_totalElapsed > num)
		{
			if (Scene != null && Scene.IsLoaded && !Scene.IsDestroyed)
			{
				Profiler.EnterContext("FixedUpdate");
				Scene.FixedUpdate(gameTime.TotalGameTime.TotalMilliseconds, gameTime.ElapsedGameTime.TotalMilliseconds);
				Profiler.ExitContext("FixedUpdate");
			}
			_totalElapsed %= num;
		}
		else if (!Mouse.SimulateClick)
		{
			_suppressedDraw = true;
			SuppressDraw();
			if (!gameTime.IsRunningSlowly)
			{
				Thread.Sleep(1);
			}
		}
		_synchronizationContext.ExecuteTask();
		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		Profiler.EndFrame();
		Profiler.BeginFrame();
		if (Profiler.InContext("OutOfContext"))
		{
			Profiler.ExitContext("OutOfContext");
		}
		Profiler.EnterContext("RenderFrame");
		_totalFrames++;
		base.GraphicsDevice.Clear(Color.Black);
		if (Scene != null && Scene.IsLoaded && !Scene.IsDestroyed)
		{
			Scene.Draw(_uoSpriteBatch);
		}
		ScreenEffectManager.Process(_uoSpriteBatch);
		UIManager.Draw(_uoSpriteBatch);
		if (World.InGame && SelectedObject.LastObject is TextObject textObject)
		{
			if (textObject.IsTextGump)
			{
				textObject.ToTopD();
			}
			else
			{
				World.WorldTextManager?.MoveToTop(textObject);
			}
		}
		SelectedObject.HealthbarObject = null;
		SelectedObject.SelectedContainer = null;
		base.Draw(gameTime);
		Profiler.ExitContext("RenderFrame");
		Profiler.EnterContext("OutOfContext");
		Plugin.ProcessDrawCmdList(base.GraphicsDevice);
	}

	private void OnNetworkUpdate(double totalTime, double frameTime)
	{
		if (NetClient.LoginSocket.IsDisposed && NetClient.LoginSocket.IsConnected)
		{
			NetClient.LoginSocket.Disconnect();
		}
		else if (!NetClient.Socket.IsConnected)
		{
			NetClient.LoginSocket.Update();
			UpdateSocketStats(NetClient.LoginSocket, totalTime);
		}
		else if (!NetClient.Socket.IsDisposed)
		{
			NetClient.Socket.Update();
			UpdateSocketStats(NetClient.Socket, totalTime);
		}
	}

	protected override bool BeginDraw()
	{
		if (!_suppressedDraw)
		{
			return base.BeginDraw();
		}
		return false;
	}

	private void UpdateSocketStats(NetClient socket, double totalTime)
	{
		if (_statisticsTimer < totalTime)
		{
			socket.Statistics.Update();
			_statisticsTimer = totalTime + 500.0;
		}
	}

	private void WindowOnClientSizeChanged(object sender, EventArgs e)
	{
		int width = base.Window.ClientBounds.Width;
		int height = base.Window.ClientBounds.Height;
		if (!IsWindowMaximized())
		{
			ProfileManager.CurrentProfile.WindowClientBounds = new Point(width, height);
		}
		SetWindowSize(width, height);
		WorldViewportGump gump = UIManager.GetGump<WorldViewportGump>(null);
		if (gump != null && ProfileManager.CurrentProfile.GameWindowFullSize)
		{
			gump.ResizeGameWindow(new Point(width, height));
			gump.X = -5;
			gump.Y = -5;
		}
	}

	private void SimulateClick(int x, int y, MouseButtonType buttonType)
	{
		Mouse.SimulateClick = true;
		Mouse.Position.X = x;
		Mouse.Position.Y = y;
		Client.Game.Tick();
		if (!Scene.OnMouseDown(buttonType))
		{
			UIManager.OnMouseButtonDown(buttonType);
		}
		if (!Scene.OnMouseUp(buttonType) || UIManager.LastControlMouseDown(buttonType) != null)
		{
			UIManager.OnMouseButtonUp(buttonType);
		}
		Mouse.SimulateClick = false;
	}

	private void SimulateDoubleClick(int x, int y, MouseButtonType buttonType)
	{
		Mouse.SimulateClick = true;
		Mouse.Position.X = x;
		Mouse.Position.Y = y;
		Client.Game.Tick();
		if (Scene.OnMouseDoubleClick(buttonType))
		{
			_ = 1;
		}
		else
			UIManager.OnMouseDoubleClick(buttonType);
		Mouse.SimulateClick = false;
	}

	private unsafe int HandleSdlEvent(IntPtr userData, IntPtr ptr)
	{
		SDL.SDL_Event* ptr2 = (SDL.SDL_Event*)(void*)ptr;
		if (Plugin.ProcessWndProc(ptr2) != 0)
		{
			if (ptr2->type == SDL.SDL_EventType.SDL_MOUSEMOTION && UIManager.GameCursor != null)
			{
				UIManager.GameCursor.AllowDrawSDLCursor = false;
			}
			return 0;
		}
		switch (ptr2->type)
		{
		case SDL.SDL_EventType.SDL_SYSWMEVENT:
		{
			SDL_SysWMEventMsg* ptr3 = (SDL_SysWMEventMsg*)(void*)ptr2->syswm.msg;
			if (ptr3->msg == 1066)
			{
				ulong num3 = ptr3->lparam & 0xFFFF;
				ulong num4 = (ptr3->lparam & 0xFFFF0000u) >> 16;
				SimulateClick((int)num3, (int)num4, MouseButtonType.Left);
			}
			else if (ptr3->msg == 1067)
			{
				ulong num5 = ptr3->lparam & 0xFFFF;
				ulong num6 = (ptr3->lparam & 0xFFFF0000u) >> 16;
				SimulateClick((int)num5, (int)num6, MouseButtonType.Right);
			}
			else if (ptr3->msg == 1068)
			{
				ulong num7 = ptr3->lparam & 0xFFFF;
				ulong num8 = (ptr3->lparam & 0xFFFF0000u) >> 16;
				SimulateDoubleClick((int)num7, (int)num8, MouseButtonType.Left);
			}
			break;
		}
		case SDL.SDL_EventType.SDL_AUDIODEVICEADDED:
			Console.WriteLine("AUDIO ADDED: {0}", ptr2->adevice.which);
			break;
		case SDL.SDL_EventType.SDL_AUDIODEVICEREMOVED:
			Console.WriteLine("AUDIO REMOVED: {0}", ptr2->adevice.which);
			break;
		case SDL.SDL_EventType.SDL_WINDOWEVENT:
			switch (ptr2->window.windowEvent)
			{
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
				Mouse.MouseInWindow = true;
				break;
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
				Mouse.MouseInWindow = false;
				break;
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
				Plugin.OnFocusGained();
				break;
			case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
				Plugin.OnFocusLost();
				break;
			}
			break;
		case SDL.SDL_EventType.SDL_KEYDOWN:
			Keyboard.OnKeyDown(ptr2->key);
			if (Plugin.ProcessHotkeys((int)ptr2->key.keysym.sym, (int)ptr2->key.keysym.mod, ispressed: true))
			{
				_ignoreNextTextInput = false;
				UIManager.KeyboardFocusControl?.InvokeKeyDown(ptr2->key.keysym.sym, ptr2->key.keysym.mod);
				Scene.OnKeyDown(ptr2->key);
			}
			else
			{
				_ignoreNextTextInput = true;
			}
			break;
		case SDL.SDL_EventType.SDL_KEYUP:
			Keyboard.OnKeyUp(ptr2->key);
			UIManager.KeyboardFocusControl?.InvokeKeyUp(ptr2->key.keysym.sym, ptr2->key.keysym.mod);
			Scene.OnKeyUp(ptr2->key);
			Plugin.ProcessHotkeys(0, 0, ispressed: false);
			if (ptr2->key.keysym.sym == SDL.SDL_Keycode.SDLK_PRINTSCREEN)
			{
				TakeScreenshot();
			}
			break;
		case SDL.SDL_EventType.SDL_TEXTINPUT:
			if (!_ignoreNextTextInput)
			{
				string text = SDL.UTF8_ToManaged((IntPtr)ptr2->text.text);
				if (!string.IsNullOrEmpty(text))
				{
					UIManager.KeyboardFocusControl?.InvokeTextInput(text);
					Scene.OnTextInput(text);
				}
			}
			break;
		case SDL.SDL_EventType.SDL_MOUSEMOTION:
			if (UIManager.GameCursor != null && !UIManager.GameCursor.AllowDrawSDLCursor)
			{
				UIManager.GameCursor.AllowDrawSDLCursor = true;
				UIManager.GameCursor.Graphic = ushort.MaxValue;
			}
			Mouse.Update();
			if (Mouse.IsDragging && !Scene.OnMouseDragging())
			{
				UIManager.OnMouseDragging();
			}
			if (Mouse.IsDragging && !_dragStarted)
			{
				_dragStarted = true;
			}
			break;
		case SDL.SDL_EventType.SDL_MOUSEWHEEL:
		{
			Mouse.Update();
			bool flag = ptr2->wheel.y > 0;
			Plugin.ProcessMouse(0, ptr2->wheel.y);
			if (!Scene.OnMouseWheel(flag))
			{
				UIManager.OnMouseWheel(flag);
			}
			break;
		}
		case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
		{
			SDL.SDL_MouseButtonEvent button3 = ptr2->button;
			MouseButtonType button4 = (MouseButtonType)button3.button;
			uint num2 = 0u;
			switch (button4)
			{
			case MouseButtonType.Left:
				num2 = Mouse.LastLeftButtonClickTime;
				break;
			case MouseButtonType.Middle:
				num2 = Mouse.LastMidButtonClickTime;
				break;
			case MouseButtonType.Right:
				num2 = Mouse.LastRightButtonClickTime;
				break;
			default:
				Log.Warn($"No mouse button handled: {button3.button}");
				break;
			}
			Mouse.ButtonPress(button4);
			Mouse.Update();
			uint ticks = Time.Ticks;
			if (num2 + 350 >= ticks)
			{
				num2 = 0u;
				if (!Scene.OnMouseDoubleClick(button4) && !UIManager.OnMouseDoubleClick(button4))
				{
					if (!Scene.OnMouseDown(button4))
					{
						UIManager.OnMouseButtonDown(button4);
					}
				}
				else
				{
					num2 = uint.MaxValue;
				}
			}
			else
			{
				if (button4 != MouseButtonType.Left && button4 != MouseButtonType.Right)
				{
					Plugin.ProcessMouse(ptr2->button.button, 0);
				}
				if (!Scene.OnMouseDown(button4))
				{
					UIManager.OnMouseButtonDown(button4);
				}
				num2 = ((!Mouse.CancelDoubleClick) ? ticks : 0u);
			}
			switch (button4)
			{
			case MouseButtonType.Left:
				Mouse.LastLeftButtonClickTime = num2;
				break;
			case MouseButtonType.Middle:
				Mouse.LastMidButtonClickTime = num2;
				break;
			case MouseButtonType.Right:
				Mouse.LastRightButtonClickTime = num2;
				break;
			}
			break;
		}
		case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
		{
			if (_dragStarted)
			{
				_dragStarted = false;
			}
			SDL.SDL_MouseButtonEvent button = ptr2->button;
			MouseButtonType button2 = (MouseButtonType)button.button;
			uint num = 0u;
			switch (button2)
			{
			case MouseButtonType.Left:
				num = Mouse.LastLeftButtonClickTime;
				break;
			case MouseButtonType.Middle:
				num = Mouse.LastMidButtonClickTime;
				break;
			case MouseButtonType.Right:
				num = Mouse.LastRightButtonClickTime;
				break;
			default:
				Log.Warn($"No mouse button handled: {button.button}");
				break;
			}
			if (num != uint.MaxValue && (!Scene.OnMouseUp(button2) || UIManager.LastControlMouseDown(button2) != null))
			{
				UIManager.OnMouseButtonUp(button2);
			}
			Mouse.ButtonRelease(button2);
			Mouse.Update();
			break;
		}
		}
		return 0;
	}

	private void TakeScreenshot()
	{
		string text = Path.Combine(FileSystemHelper.CreateFolderIfNotExists(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Alathair", "Screenshots"), $"screenshot_{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.png");
		Color[] data = new Color[GraphicManager.PreferredBackBufferWidth * GraphicManager.PreferredBackBufferHeight];
		base.GraphicsDevice.GetBackBufferData(data);
		using Texture2D texture2D = new Texture2D(base.GraphicsDevice, GraphicManager.PreferredBackBufferWidth, GraphicManager.PreferredBackBufferHeight, mipMap: false, SurfaceFormat.Color);
		using FileStream stream = File.Create(text);
		texture2D.SetData(data);
		texture2D.SaveAsPng(stream, texture2D.Width, texture2D.Height);
		string text2 = string.Format(ResGeneral.ScreenshotStoredIn0, text);
		if (ProfileManager.CurrentProfile == null || ProfileManager.CurrentProfile.HideScreenshotStoredInMessage)
		{
			Log.Info(text2);
		}
		else
		{
			GameActions.Print(text2, 68, MessageType.System, 3);
		}
	}
}
