using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ClassicUO.Configuration;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer.Batching;
using ClassicUO.Utility.Logging;
using ClassicUO.Utility.Platforms;
using CUO_API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;

namespace ClassicUO.Network;

internal class Plugin
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void OnInstall(void* header);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool OnPacketSendRecv_new(byte[] data, ref int length);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool OnPacketSendRecv_new_intptr(IntPtr data, ref int length);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int OnDrawCmdList(out IntPtr cmdlist, ref int size);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate int OnWndProc(SDL.SDL_Event* ev);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool OnGetStaticData(int index, ref ulong flags, ref byte weight, ref byte layer, ref int count, ref ushort animid, ref ushort lightidx, ref byte height, ref string name);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool OnGetTileData(int index, ref ulong flags, ref ushort textid, ref string name);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool OnGetCliloc(int cliloc, [MarshalAs(UnmanagedType.LPStr)] string args, bool capitalize, [MarshalAs(UnmanagedType.LPStr)] out string buffer);

	private struct PluginHeader
	{
		public int ClientVersion;

		public IntPtr HWND;

		public IntPtr OnRecv;

		public IntPtr OnSend;

		public IntPtr OnHotkeyPressed;

		public IntPtr OnMouse;

		public IntPtr OnPlayerPositionChanged;

		public IntPtr OnClientClosing;

		public IntPtr OnInitialize;

		public IntPtr OnConnected;

		public IntPtr OnDisconnected;

		public IntPtr OnFocusGained;

		public IntPtr OnFocusLost;

		public IntPtr GetUOFilePath;

		public IntPtr Recv;

		public IntPtr Send;

		public IntPtr GetPacketLength;

		public IntPtr GetPlayerPosition;

		public IntPtr CastSpell;

		public IntPtr GetStaticImage;

		public IntPtr Tick;

		public IntPtr RequestMove;

		public IntPtr SetTitle;

		public IntPtr OnRecv_new;

		public IntPtr OnSend_new;

		public IntPtr Recv_new;

		public IntPtr Send_new;

		public IntPtr OnDrawCmdList;

		public IntPtr SDL_Window;

		public IntPtr OnWndProc;

		public IntPtr GetStaticData;

		public IntPtr GetTileData;

		public IntPtr GetCliloc;
	}

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnCastSpell _castSpell;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnDrawCmdList _draw_cmd_list;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetCliloc _get_cliloc;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetStaticData _get_static_data;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetTileData _get_tile_data;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetPacketLength _getPacketLength;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetPlayerPosition _getPlayerPosition;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetStaticImage _getStaticImage;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnGetUOFilePath _getUoFilePath;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnWndProc _on_wnd_proc;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnClientClose _onClientClose;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnConnected _onConnected;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnDisconnected _onDisconnected;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnFocusGained _onFocusGained;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnFocusLost _onFocusLost;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnHotkey _onHotkeyPressed;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnInitialize _onInitialize;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnMouse _onMouse;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv_new _onRecv_new;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv_new _onSend_new;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnUpdatePlayerPosition _onUpdatePlayerPosition;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv _recv;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv _send;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv _onRecv;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv _onSend;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv_new_intptr _recv_new;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnPacketSendRecv_new_intptr _send_new;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private RequestMove _requestMove;

	private readonly Dictionary<IntPtr, GraphicsResource> _resources = new Dictionary<IntPtr, GraphicsResource>();

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnSetTitle _setTitle;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	private OnTick _tick;

	public static List<Plugin> Plugins { get; } = new List<Plugin>();

	public string PluginPath { get; }

	public bool IsValid { get; private set; }

	private Plugin(string path)
	{
		PluginPath = path;
	}

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteFile(string name);

	public static Plugin Create(string path)
	{
		path = Path.GetFullPath(Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Plugins", path));
		if (!File.Exists(path))
		{
			Log.Error("Plugin '" + path + "' not found.");
			return null;
		}
		Log.Trace("Loading plugin: " + path);
		Plugin plugin = new Plugin(path);
		plugin.Load();
		if (!plugin.IsValid)
		{
			Log.Warn("Invalid plugin: " + path);
			return null;
		}
		Log.Trace("Plugin: " + path + " loaded.");
		Plugins.Add(plugin);
		return plugin;
	}

	public unsafe void Load()
	{
		_recv = OnPluginRecv;
		_send = OnPluginSend;
		_recv_new = OnPluginRecv_new;
		_send_new = OnPluginSend_new;
		_getPacketLength = PacketsTable.GetPacketLength;
		_getPlayerPosition = GetPlayerPosition;
		_castSpell = GameActions.CastSpell;
		_getStaticImage = GetStaticImage;
		_getUoFilePath = GetUOFilePath;
		_requestMove = RequestMove;
		_setTitle = SetWindowTitle;
		_get_static_data = GetStaticData;
		_get_tile_data = GetTileData;
		_get_cliloc = GetCliloc;
		SDL.SDL_SysWMinfo info = default(SDL.SDL_SysWMinfo);
		SDL.SDL_VERSION(out info.version);
		SDL.SDL_GetWindowWMInfo(Client.Game.Window.Handle, ref info);
		IntPtr hWND = IntPtr.Zero;
		if (info.subsystem == SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS)
		{
			hWND = info.info.win.window;
		}
		PluginHeader pluginHeader = default(PluginHeader);
		pluginHeader.ClientVersion = (int)Client.Version;
		pluginHeader.Recv = Marshal.GetFunctionPointerForDelegate(_recv);
		pluginHeader.Send = Marshal.GetFunctionPointerForDelegate(_send);
		pluginHeader.GetPacketLength = Marshal.GetFunctionPointerForDelegate(_getPacketLength);
		pluginHeader.GetPlayerPosition = Marshal.GetFunctionPointerForDelegate(_getPlayerPosition);
		pluginHeader.CastSpell = Marshal.GetFunctionPointerForDelegate(_castSpell);
		pluginHeader.GetStaticImage = Marshal.GetFunctionPointerForDelegate(_getStaticImage);
		pluginHeader.HWND = hWND;
		pluginHeader.GetUOFilePath = Marshal.GetFunctionPointerForDelegate(_getUoFilePath);
		pluginHeader.RequestMove = Marshal.GetFunctionPointerForDelegate(_requestMove);
		pluginHeader.SetTitle = Marshal.GetFunctionPointerForDelegate(_setTitle);
		pluginHeader.Recv_new = Marshal.GetFunctionPointerForDelegate(_recv_new);
		pluginHeader.Send_new = Marshal.GetFunctionPointerForDelegate(_send_new);
		pluginHeader.SDL_Window = Client.Game.Window.Handle;
		pluginHeader.GetStaticData = Marshal.GetFunctionPointerForDelegate(_get_static_data);
		pluginHeader.GetTileData = Marshal.GetFunctionPointerForDelegate(_get_tile_data);
		pluginHeader.GetCliloc = Marshal.GetFunctionPointerForDelegate(_get_cliloc);
		PluginHeader pluginHeader2 = pluginHeader;
		void* ptr = &pluginHeader2;
		if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
		{
			UnblockPath(Path.GetDirectoryName(PluginPath));
		}
		try
		{
			IntPtr intPtr = Native.LoadLibrary(PluginPath);
			Log.Trace($"assembly: {intPtr}");
			if (intPtr == IntPtr.Zero)
			{
				throw new Exception("Invalid Assembly, Attempting managed load.");
			}
			Log.Trace($"Searching for 'Install' entry point  -  {intPtr}");
			IntPtr processAddress = Native.GetProcessAddress(intPtr, "Install");
			Log.Trace($"Entry point: {processAddress}");
			if (processAddress == IntPtr.Zero)
			{
				throw new Exception("Invalid Entry Point, Attempting managed load.");
			}
			Marshal.GetDelegateForFunctionPointer<OnInstall>(processAddress)(ptr);
			Console.WriteLine(">>> ADDRESS {0}", pluginHeader2.OnInitialize);
		}
		catch
		{
			try
			{
				Type type = Assembly.LoadFile(PluginPath).GetType("Assistant.Engine");
				if (type == null)
				{
					Log.Error("Unable to find Plugin Type, API requires the public class Engine in namespace Assistant.");
					return;
				}
				MethodInfo method = type.GetMethod("Install", BindingFlags.Static | BindingFlags.Public);
				if (method == null)
				{
					Log.Error("Engine class missing public static Install method Needs 'public static unsafe void Install(PluginHeader *plugin)' ");
					return;
				}
				method.Invoke(null, new object[1] { (IntPtr)ptr });
			}
			catch (Exception ex)
			{
				Log.Error("Plugin threw an error during Initialization. " + ex.Message + " " + ex.StackTrace + " " + ex.InnerException?.Message + " " + ex.InnerException?.StackTrace);
				return;
			}
		}
		if (pluginHeader2.OnRecv != IntPtr.Zero)
		{
			_onRecv = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(pluginHeader2.OnRecv);
		}
		if (pluginHeader2.OnSend != IntPtr.Zero)
		{
			_onSend = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(pluginHeader2.OnSend);
		}
		if (pluginHeader2.OnHotkeyPressed != IntPtr.Zero)
		{
			_onHotkeyPressed = Marshal.GetDelegateForFunctionPointer<OnHotkey>(pluginHeader2.OnHotkeyPressed);
		}
		if (pluginHeader2.OnMouse != IntPtr.Zero)
		{
			_onMouse = Marshal.GetDelegateForFunctionPointer<OnMouse>(pluginHeader2.OnMouse);
		}
		if (pluginHeader2.OnPlayerPositionChanged != IntPtr.Zero)
		{
			_onUpdatePlayerPosition = Marshal.GetDelegateForFunctionPointer<OnUpdatePlayerPosition>(pluginHeader2.OnPlayerPositionChanged);
		}
		if (pluginHeader2.OnClientClosing != IntPtr.Zero)
		{
			_onClientClose = Marshal.GetDelegateForFunctionPointer<OnClientClose>(pluginHeader2.OnClientClosing);
		}
		if (pluginHeader2.OnInitialize != IntPtr.Zero)
		{
			_onInitialize = Marshal.GetDelegateForFunctionPointer<OnInitialize>(pluginHeader2.OnInitialize);
		}
		if (pluginHeader2.OnConnected != IntPtr.Zero)
		{
			_onConnected = Marshal.GetDelegateForFunctionPointer<OnConnected>(pluginHeader2.OnConnected);
		}
		if (pluginHeader2.OnDisconnected != IntPtr.Zero)
		{
			_onDisconnected = Marshal.GetDelegateForFunctionPointer<OnDisconnected>(pluginHeader2.OnDisconnected);
		}
		if (pluginHeader2.OnFocusGained != IntPtr.Zero)
		{
			_onFocusGained = Marshal.GetDelegateForFunctionPointer<OnFocusGained>(pluginHeader2.OnFocusGained);
		}
		if (pluginHeader2.OnFocusLost != IntPtr.Zero)
		{
			_onFocusLost = Marshal.GetDelegateForFunctionPointer<OnFocusLost>(pluginHeader2.OnFocusLost);
		}
		if (pluginHeader2.Tick != IntPtr.Zero)
		{
			_tick = Marshal.GetDelegateForFunctionPointer<OnTick>(pluginHeader2.Tick);
		}
		if (pluginHeader2.OnRecv_new != IntPtr.Zero)
		{
			_onRecv_new = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv_new>(pluginHeader2.OnRecv_new);
		}
		if (pluginHeader2.OnSend_new != IntPtr.Zero)
		{
			_onSend_new = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv_new>(pluginHeader2.OnSend_new);
		}
		if (pluginHeader2.OnDrawCmdList != IntPtr.Zero)
		{
			_draw_cmd_list = Marshal.GetDelegateForFunctionPointer<OnDrawCmdList>(pluginHeader2.OnDrawCmdList);
		}
		if (pluginHeader2.OnWndProc != IntPtr.Zero)
		{
			_on_wnd_proc = Marshal.GetDelegateForFunctionPointer<OnWndProc>(pluginHeader2.OnWndProc);
		}
		IsValid = true;
		if (_onInitialize != null)
		{
			_onInitialize();
		}
	}

	private static string GetUOFilePath()
	{
		return Settings.GlobalSettings.UltimaOnlineDirectory;
	}

	private static void SetWindowTitle(string str)
	{
		Client.Game.SetWindowTitle(str);
	}

	private static bool GetStaticData(int index, ref ulong flags, ref byte weight, ref byte layer, ref int count, ref ushort animid, ref ushort lightidx, ref byte height, ref string name)
	{
		if (index >= 0 && index < 81920)
		{
			ref StaticTiles reference = ref TileDataLoader.Instance.StaticData[index];
			flags = (ulong)reference.Flags;
			weight = reference.Weight;
			layer = reference.Layer;
			count = reference.Count;
			animid = reference.AnimID;
			lightidx = reference.LightIndex;
			height = reference.Height;
			name = reference.Name;
			return true;
		}
		return false;
	}

	private static bool GetTileData(int index, ref ulong flags, ref ushort textid, ref string name)
	{
		if (index >= 0 && index < 81920)
		{
			ref LandTiles reference = ref TileDataLoader.Instance.LandData[index];
			flags = (ulong)reference.Flags;
			textid = reference.TexID;
			name = reference.Name;
			return true;
		}
		return false;
	}

	private static bool GetCliloc(int cliloc, string args, bool capitalize, out string buffer)
	{
		buffer = ClilocLoader.Instance.Translate(cliloc, args, capitalize);
		return buffer != null;
	}

	private static void GetStaticImage(ushort g, ref ArtInfo info)
	{
	}

	private static bool RequestMove(int dir, bool run)
	{
		return World.Player.Walk((Direction)dir, run);
	}

	private static bool GetPlayerPosition(out int x, out int y, out int z)
	{
		if (World.Player != null)
		{
			x = World.Player.X;
			y = World.Player.Y;
			z = World.Player.Z;
			return true;
		}
		x = (y = (z = 0));
		return false;
	}

	internal static void Tick()
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._tick != null)
			{
				plugin._tick();
			}
		}
	}

	internal static bool ProcessRecvPacket(byte[] data, ref int length)
	{
		bool result = true;
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onRecv_new != null)
			{
				if (!plugin._onRecv_new(data, ref length))
				{
					result = false;
				}
			}
			else if (plugin._onRecv != null)
			{
				byte[] data2 = new byte[length];
				Array.Copy(data, data2, length);
				if (!plugin._onRecv(ref data2, ref length))
				{
					result = false;
				}
				Array.Copy(data2, data, length);
			}
		}
		return result;
	}

	internal static bool ProcessSendPacket(byte[] data, ref int length)
	{
		bool result = true;
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onSend_new != null)
			{
				if (!plugin._onSend_new(data, ref length))
				{
					result = false;
				}
			}
			else if (plugin._onSend != null)
			{
				byte[] data2 = new byte[length];
				Array.Copy(data, data2, length);
				if (!plugin._onSend(ref data2, ref length))
				{
					result = false;
				}
				Array.Copy(data2, data, length);
			}
		}
		return result;
	}

	internal static void OnClosing()
	{
		for (int i = 0; i < Plugins.Count; i++)
		{
			if (Plugins[i]._onClientClose != null)
			{
				Plugins[i]._onClientClose();
			}
		}
		Plugins.Clear();
	}

	internal static void OnFocusGained()
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onFocusGained != null)
			{
				plugin._onFocusGained();
			}
		}
	}

	internal static void OnFocusLost()
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onFocusLost != null)
			{
				plugin._onFocusLost();
			}
		}
	}

	internal static void OnConnected()
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onConnected != null)
			{
				plugin._onConnected();
			}
		}
	}

	internal static void OnDisconnected()
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onDisconnected != null)
			{
				plugin._onDisconnected();
			}
		}
	}

	internal static bool ProcessHotkeys(int key, int mod, bool ispressed)
	{
		if (!World.InGame || (UIManager.SystemChat != null && ((ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.ActivateChatAfterEnter && UIManager.SystemChat.IsActive) || UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)))
		{
			return true;
		}
		bool result = true;
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._onHotkeyPressed != null && !plugin._onHotkeyPressed(key, mod, ispressed))
			{
				result = false;
			}
		}
		return result;
	}

	internal static void ProcessMouse(int button, int wheel)
	{
		foreach (Plugin plugin in Plugins)
		{
			plugin._onMouse?.Invoke(button, wheel);
		}
	}

	internal static void ProcessDrawCmdList(GraphicsDevice device)
	{
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._draw_cmd_list != null)
			{
				int size = 0;
				plugin._draw_cmd_list(out var cmdlist, ref size);
				if (size != 0 && cmdlist != IntPtr.Zero)
				{
					plugin.HandleCmdList(device, cmdlist, size, plugin._resources);
				}
			}
		}
	}

	internal unsafe static int ProcessWndProc(SDL.SDL_Event* e)
	{
		int num = 0;
		foreach (Plugin plugin in Plugins)
		{
			if (plugin._on_wnd_proc != null)
			{
				num |= plugin._on_wnd_proc(e);
			}
		}
		return num;
	}

	internal static void UpdatePlayerPosition(int x, int y, int z)
	{
		foreach (Plugin plugin in Plugins)
		{
			try
			{
				if (plugin._onUpdatePlayerPosition != null)
				{
					plugin._onUpdatePlayerPosition(x, y, z);
				}
			}
			catch
			{
				Log.Error("Plugin initialization failed, please re login");
			}
		}
	}

	private static bool OnPluginRecv(ref byte[] data, ref int length)
	{
		NetClient.EnqueuePacketFromPlugin(data, length);
		return true;
	}

	private static bool OnPluginSend(ref byte[] data, ref int length)
	{
		if (NetClient.LoginSocket.IsDisposed && NetClient.Socket.IsConnected)
		{
			NetClient.Socket.Send(data, length, ignorePlugin: true);
		}
		else if (NetClient.Socket.IsDisposed && NetClient.LoginSocket.IsConnected)
		{
			NetClient.LoginSocket.Send(data, length, ignorePlugin: true);
		}
		return true;
	}

	private static bool OnPluginRecv_new(IntPtr buffer, ref int length)
	{
		if (buffer != IntPtr.Zero && length > 0)
		{
			byte[] array = new byte[length];
			Marshal.Copy(buffer, array, 0, length);
			NetClient.EnqueuePacketFromPlugin(array, length);
		}
		return true;
	}

	private unsafe static bool OnPluginSend_new(IntPtr buffer, ref int length)
	{
		if (buffer != IntPtr.Zero && length > 0)
		{
			StackDataWriter stackDataWriter = new StackDataWriter(new Span<byte>((void*)buffer, length));
			if (NetClient.LoginSocket.IsDisposed && NetClient.Socket.IsConnected)
			{
				NetClient.Socket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten, ignorePlugin: true);
			}
			else if (NetClient.Socket.IsDisposed && NetClient.LoginSocket.IsConnected)
			{
				NetClient.LoginSocket.Send(stackDataWriter.AllocatedBuffer, stackDataWriter.BytesWritten, ignorePlugin: true);
			}
			stackDataWriter.Dispose();
		}
		return true;
	}

	private static void UnblockPath(string path)
	{
		string[] files = Directory.GetFiles(path);
		string[] directories = Directory.GetDirectories(path);
		string[] array = files;
		foreach (string text in array)
		{
			if (text.EndsWith("dll") || text.EndsWith("exe"))
			{
				UnblockFile(text);
			}
		}
		array = directories;
		for (int i = 0; i < array.Length; i++)
		{
			UnblockPath(array[i]);
		}
	}

	private static bool UnblockFile(string fileName)
	{
		return DeleteFile(fileName + ":Zone.Identifier");
	}

	private unsafe void HandleCmdList(GraphicsDevice device, IntPtr ptr, int length, IDictionary<IntPtr, GraphicsResource> resources)
	{
		if (ptr == IntPtr.Zero || length <= 0)
		{
			return;
		}
		Effect effect = null;
		Viewport viewport = device.Viewport;
		Rectangle scissorRectangle = device.ScissorRectangle;
		Color blendFactor = device.BlendFactor;
		BlendState blendState = device.BlendState;
		RasterizerState rasterizerState = device.RasterizerState;
		DepthStencilState depthStencilState = device.DepthStencilState;
		SamplerState value = device.SamplerStates[0];
		for (int i = 0; i < length; i++)
		{
			BatchCommand batchCommand = *(BatchCommand*)((byte*)(void*)ptr + (nint)i * (nint)sizeof(BatchCommand));
			switch (batchCommand.type)
			{
			case 0:
			{
				ref ViewportCommand viewportCommand = ref batchCommand.ViewportCommand;
				device.Viewport = new Viewport(viewportCommand.X, viewportCommand.y, viewportCommand.w, viewportCommand.h);
				break;
			}
			case 1:
			{
				ref ScissorCommand scissorCommand = ref batchCommand.ScissorCommand;
				device.ScissorRectangle = new Rectangle(scissorCommand.x, scissorCommand.y, scissorCommand.w, scissorCommand.h);
				break;
			}
			case 18:
				device.BlendFactor = batchCommand.NewBlendFactorCommand.color;
				break;
			case 19:
			{
				ref CreateBlendStateCommand newCreateBlendStateCommand = ref batchCommand.NewCreateBlendStateCommand;
				resources[newCreateBlendStateCommand.id] = new BlendState
				{
					AlphaBlendFunction = newCreateBlendStateCommand.AlphaBlendFunc,
					AlphaDestinationBlend = newCreateBlendStateCommand.AlphaDestBlend,
					AlphaSourceBlend = newCreateBlendStateCommand.AlphaSrcBlend,
					ColorBlendFunction = newCreateBlendStateCommand.ColorBlendFunc,
					ColorDestinationBlend = newCreateBlendStateCommand.ColorDestBlend,
					ColorSourceBlend = newCreateBlendStateCommand.ColorSrcBlend,
					ColorWriteChannels = newCreateBlendStateCommand.ColorWriteChannels0,
					ColorWriteChannels1 = newCreateBlendStateCommand.ColorWriteChannels1,
					ColorWriteChannels2 = newCreateBlendStateCommand.ColorWriteChannels2,
					ColorWriteChannels3 = newCreateBlendStateCommand.ColorWriteChannels3,
					BlendFactor = newCreateBlendStateCommand.BlendFactor,
					MultiSampleMask = newCreateBlendStateCommand.MultipleSampleMask
				};
				break;
			}
			case 20:
			{
				ref CreateRasterizerStateCommand newRasterizeStateCommand = ref batchCommand.NewRasterizeStateCommand;
				resources[newRasterizeStateCommand.id] = new RasterizerState
				{
					CullMode = newRasterizeStateCommand.CullMode,
					DepthBias = newRasterizeStateCommand.DepthBias,
					FillMode = newRasterizeStateCommand.FillMode,
					MultiSampleAntiAlias = newRasterizeStateCommand.MultiSample,
					ScissorTestEnable = newRasterizeStateCommand.ScissorTestEnabled,
					SlopeScaleDepthBias = newRasterizeStateCommand.SlopeScaleDepthBias
				};
				break;
			}
			case 21:
			{
				ref CreateStencilStateCommand newCreateStencilStateCommand = ref batchCommand.NewCreateStencilStateCommand;
				resources[newCreateStencilStateCommand.id] = new DepthStencilState
				{
					DepthBufferEnable = newCreateStencilStateCommand.DepthBufferEnabled,
					DepthBufferWriteEnable = newCreateStencilStateCommand.DepthBufferWriteEnabled,
					DepthBufferFunction = newCreateStencilStateCommand.DepthBufferFunc,
					StencilEnable = newCreateStencilStateCommand.StencilEnabled,
					StencilFunction = newCreateStencilStateCommand.StencilFunc,
					StencilPass = newCreateStencilStateCommand.StencilPass,
					StencilFail = newCreateStencilStateCommand.StencilFail,
					StencilDepthBufferFail = newCreateStencilStateCommand.StencilDepthBufferFail,
					TwoSidedStencilMode = newCreateStencilStateCommand.TwoSidedStencilMode,
					CounterClockwiseStencilFunction = newCreateStencilStateCommand.CounterClockwiseStencilFunc,
					CounterClockwiseStencilFail = newCreateStencilStateCommand.CounterClockwiseStencilFail,
					CounterClockwiseStencilPass = newCreateStencilStateCommand.CounterClockwiseStencilPass,
					CounterClockwiseStencilDepthBufferFail = newCreateStencilStateCommand.CounterClockwiseStencilDepthBufferFail,
					StencilMask = newCreateStencilStateCommand.StencilMask,
					StencilWriteMask = newCreateStencilStateCommand.StencilWriteMask,
					ReferenceStencil = newCreateStencilStateCommand.ReferenceStencil
				};
				break;
			}
			case 22:
			{
				ref CreateSamplerStateCommand newCreateSamplerStateCommand = ref batchCommand.NewCreateSamplerStateCommand;
				resources[newCreateSamplerStateCommand.id] = new SamplerState
				{
					AddressU = newCreateSamplerStateCommand.AddressU,
					AddressV = newCreateSamplerStateCommand.AddressV,
					AddressW = newCreateSamplerStateCommand.AddressW,
					Filter = newCreateSamplerStateCommand.TextureFilter,
					MaxAnisotropy = newCreateSamplerStateCommand.MaxAnisotropy,
					MaxMipLevel = newCreateSamplerStateCommand.MaxMipLevel,
					MipMapLevelOfDetailBias = newCreateSamplerStateCommand.MipMapLevelOfDetailBias
				};
				break;
			}
			case 2:
				device.BlendState = resources[batchCommand.SetBlendStateCommand.id] as BlendState;
				break;
			case 3:
				device.RasterizerState = resources[batchCommand.SetRasterizerStateCommand.id] as RasterizerState;
				break;
			case 4:
				device.DepthStencilState = resources[batchCommand.SetStencilStateCommand.id] as DepthStencilState;
				break;
			case 5:
				device.SamplerStates[batchCommand.SetSamplerStateCommand.index] = resources[batchCommand.SetSamplerStateCommand.id] as SamplerState;
				break;
			case 15:
			{
				ref SetVertexDataCommand setVertexDataCommand = ref batchCommand.SetVertexDataCommand;
				(resources[setVertexDataCommand.id] as VertexBuffer)?.SetDataPointerEXT(0, setVertexDataCommand.vertex_buffer_ptr, setVertexDataCommand.vertex_buffer_length, SetDataOptions.None);
				break;
			}
			case 16:
			{
				ref SetIndexDataCommand setIndexDataCommand = ref batchCommand.SetIndexDataCommand;
				(resources[setIndexDataCommand.id] as IndexBuffer)?.SetDataPointerEXT(0, setIndexDataCommand.indices_buffer_ptr, setIndexDataCommand.indices_buffer_length, SetDataOptions.None);
				break;
			}
			case 8:
			{
				ref CreateVertexBufferCommand createVertexBufferCommand = ref batchCommand.CreateVertexBufferCommand;
				VertexElement[] array = new VertexElement[createVertexBufferCommand.DeclarationCount];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = *(VertexElement*)((byte*)createVertexBufferCommand.Declarations + (nint)j * (nint)sizeof(VertexElement));
				}
				VertexBuffer vertexBuffer = (createVertexBufferCommand.IsDynamic ? new DynamicVertexBuffer(device, new VertexDeclaration(createVertexBufferCommand.Size, array), createVertexBufferCommand.VertexElementsCount, createVertexBufferCommand.BufferUsage) : new VertexBuffer(device, new VertexDeclaration(createVertexBufferCommand.Size, array), createVertexBufferCommand.VertexElementsCount, createVertexBufferCommand.BufferUsage));
				resources[createVertexBufferCommand.id] = vertexBuffer;
				break;
			}
			case 9:
			{
				ref CreateIndexBufferCommand createIndexBufferCommand = ref batchCommand.CreateIndexBufferCommand;
				IndexBuffer indices = (createIndexBufferCommand.IsDynamic ? new DynamicIndexBuffer(device, createIndexBufferCommand.IndexElementSize, createIndexBufferCommand.IndexCount, createIndexBufferCommand.BufferUsage) : new IndexBuffer(device, createIndexBufferCommand.IndexElementSize, createIndexBufferCommand.IndexCount, createIndexBufferCommand.BufferUsage));
				resources[createIndexBufferCommand.id] = indices;
				break;
			}
			case 6:
			{
				VertexBuffer vertexBuffer = resources[batchCommand.SetVertexBufferCommand.id] as VertexBuffer;
				device.SetVertexBuffer(vertexBuffer);
				break;
			}
			case 7:
			{
				IndexBuffer indices = resources[batchCommand.SetIndexBufferCommand.id] as IndexBuffer;
				device.Indices = indices;
				break;
			}
			case 14:
			{
				ref CreateBasicEffectCommand createBasicEffectCommand = ref batchCommand.CreateBasicEffectCommand;
				if (!resources.TryGetValue(createBasicEffectCommand.id, out var value3))
				{
					value3 = new BasicEffect(device);
					resources[createBasicEffectCommand.id] = value3;
					break;
				}
				BasicEffect obj = value3 as BasicEffect;
				obj.World = createBasicEffectCommand.world;
				obj.View = createBasicEffectCommand.view;
				obj.Projection = createBasicEffectCommand.projection;
				obj.TextureEnabled = createBasicEffectCommand.texture_enabled;
				obj.Texture = resources[createBasicEffectCommand.texture_id] as Texture2D;
				obj.VertexColorEnabled = createBasicEffectCommand.vertex_color_enabled;
				effect = obj;
				break;
			}
			case 11:
			{
				ref CreateTexture2DCommand createTexture2DCommand = ref batchCommand.CreateTexture2DCommand;
				Texture2D value2 = ((!createTexture2DCommand.IsRenderTarget) ? new Texture2D(device, createTexture2DCommand.Width, createTexture2DCommand.Height, mipMap: false, createTexture2DCommand.Format) : new RenderTarget2D(device, createTexture2DCommand.Width, createTexture2DCommand.Height, mipMap: false, createTexture2DCommand.Format, DepthFormat.Depth24Stencil8));
				resources[createTexture2DCommand.id] = value2;
				break;
			}
			case 12:
			{
				ref SetTexture2DDataCommand setTexture2DDataCommand = ref batchCommand.SetTexture2DDataCommand;
				if (resources[setTexture2DDataCommand.id] is Texture2D texture2D)
				{
					texture2D.SetDataPointerEXT(setTexture2DDataCommand.level, new Rectangle(setTexture2DDataCommand.x, setTexture2DDataCommand.y, setTexture2DDataCommand.width, setTexture2DDataCommand.height), setTexture2DDataCommand.data, setTexture2DDataCommand.data_length);
				}
				break;
			}
			case 13:
			{
				ref IndexedPrimitiveDataCommand indexedPrimitiveDataCommand = ref batchCommand.IndexedPrimitiveDataCommand;
				if (effect != null)
				{
					foreach (EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.DrawIndexedPrimitives(indexedPrimitiveDataCommand.PrimitiveType, indexedPrimitiveDataCommand.BaseVertex, indexedPrimitiveDataCommand.MinVertexIndex, indexedPrimitiveDataCommand.NumVertices, indexedPrimitiveDataCommand.StartIndex, indexedPrimitiveDataCommand.PrimitiveCount);
					}
				}
				else
				{
					device.DrawIndexedPrimitives(indexedPrimitiveDataCommand.PrimitiveType, indexedPrimitiveDataCommand.BaseVertex, indexedPrimitiveDataCommand.MinVertexIndex, indexedPrimitiveDataCommand.NumVertices, indexedPrimitiveDataCommand.StartIndex, indexedPrimitiveDataCommand.PrimitiveCount);
				}
				break;
			}
			case 17:
			{
				ref DestroyResourceCommand destroyResourceCommand = ref batchCommand.DestroyResourceCommand;
				resources[destroyResourceCommand.id]?.Dispose();
				resources.Remove(destroyResourceCommand.id);
				break;
			}
			}
		}
		device.Viewport = viewport;
		device.ScissorRectangle = scissorRectangle;
		device.BlendFactor = blendFactor;
		device.BlendState = blendState;
		device.RasterizerState = rasterizerState;
		device.DepthStencilState = depthStencilState;
		device.SamplerStates[0] = value;
	}
}
