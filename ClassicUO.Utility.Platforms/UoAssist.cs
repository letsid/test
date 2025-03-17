using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Utility.Logging;
using SDL2;

namespace ClassicUO.Utility.Platforms;

internal static class UoAssist
{
	private class CustomWindow : IDisposable
	{
		private enum UOAMessage : uint
		{
			First = 1224u,
			REGISTER = 1224u,
			COUNT_RESOURCES = 1225u,
			GET_COORDS = 1226u,
			GET_SKILL = 1227u,
			GET_STAT = 1228u,
			SET_MACRO = 1229u,
			PLAY_MACRO = 1230u,
			DISPLAY_TEXT = 1231u,
			REQUEST_MULTIS = 1232u,
			ADD_CMD = 1233u,
			GET_UID = 1234u,
			GET_SHARDNAME = 1235u,
			ADD_USER_2_PARTY = 1236u,
			GET_UO_HWND = 1237u,
			GET_POISON = 1238u,
			SET_SKILL_LOCK = 1239u,
			GET_ACCT_ID = 1240u,
			RES_COUNT_DONE = 1325u,
			CAST_SPELL = 1326u,
			LOGIN = 1327u,
			MAGERY_LEVEL = 1328u,
			INT_STATUS = 1329u,
			SKILL_LEVEL = 1330u,
			MACRO_DONE = 1331u,
			LOGOUT = 1332u,
			STR_STATUS = 1333u,
			DEX_STATUS = 1334u,
			ADD_MULTI = 1335u,
			REM_MULTI = 1336u,
			MAP_INFO = 1337u,
			POWERHOUR = 1338u,
			Last = 1338u
		}

		private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WNDCLASS
		{
			public readonly uint style;

			public IntPtr lpfnWndProc;

			public readonly int cbClsExtra;

			public readonly int cbWndExtra;

			public IntPtr hInstance;

			public readonly IntPtr hIcon;

			public readonly IntPtr hCursor;

			public readonly IntPtr hbrBackground;

			[MarshalAs(UnmanagedType.LPWStr)]
			public readonly string lpszMenuName;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpszClassName;
		}

		private class WndRegEnt
		{
			public int Handle { get; }

			public int Type { get; }

			public WndRegEnt(int hWnd, int type)
			{
				Handle = hWnd;
				Type = type;
			}
		}

		private class WndCmd
		{
			private readonly IntPtr hWnd;

			private readonly uint Msg;

			public WndCmd(uint msg, IntPtr handle, string cmd)
			{
				Msg = msg;
				hWnd = handle;
				CommandManager.Register(cmd, MyCallback);
			}

			private void MyCallback(string[] args)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < args.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append(args[i]);
				}
				string text = stringBuilder.ToString();
				ushort num = 0;
				if (text != null && text.Length > 0)
				{
					num = GlobalAddAtom(text);
				}
				PostMessage(hWnd, Msg, (IntPtr)num, IntPtr.Zero);
			}
		}

		private const int ERROR_CLASS_ALREADY_EXISTS = 1410;

		public const uint WM_USER = 1024u;

		private uint _cmdID = 1425u;

		private readonly Dictionary<int, WndRegEnt> _wndRegs = new Dictionary<int, WndRegEnt>();

		private bool m_disposed;

		private IntPtr m_hwnd;

		private readonly WndProc m_wnd_proc_delegate;

		public CustomWindow(string class_name)
		{
			SDL.SDL_SysWMinfo info = default(SDL.SDL_SysWMinfo);
			SDL.SDL_VERSION(out info.version);
			SDL.SDL_GetWindowWMInfo(Client.Game.Window.Handle, ref info);
			IntPtr hInstance = IntPtr.Zero;
			if (info.subsystem == SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS)
			{
				hInstance = info.info.win.window;
			}
			if (class_name == null)
			{
				throw new Exception("class_name is null");
			}
			if (class_name == string.Empty)
			{
				throw new Exception("class_name is empty");
			}
			m_wnd_proc_delegate = CustomWndProc;
			WNDCLASS lpWndClass = new WNDCLASS
			{
				hInstance = hInstance,
				lpszClassName = class_name,
				lpfnWndProc = Marshal.GetFunctionPointerForDelegate(m_wnd_proc_delegate)
			};
			ushort num = RegisterClassW(ref lpWndClass);
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (num == 0 && lastWin32Error != 1410)
			{
				throw new Exception("Could not register window class");
			}
			m_hwnd = CreateWindowExW(0u, class_name, class_name, 0u, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);
			if (m_hwnd != IntPtr.Zero)
			{
				ShowWindow(m_hwnd, 0);
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		[DllImport("user32.dll")]
		internal static extern uint PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll")]
		internal static extern ushort GlobalAddAtom(string str);

		[DllImport("kernel32.dll")]
		internal static extern ushort GlobalDeleteAtom(ushort atom);

		[DllImport("kernel32.dll")]
		internal static extern uint GlobalGetAtomName(ushort atom, StringBuilder buff, int bufLen);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern ushort RegisterClassW([In] ref WNDCLASS lpWndClass);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr CreateWindowExW(uint dwExStyle, [MarshalAs(UnmanagedType.LPWStr)] string lpClassName, [MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyWindow(IntPtr hWnd);

		private void Dispose(bool disposing)
		{
			if (!m_disposed && m_hwnd != IntPtr.Zero)
			{
				DestroyWindow(m_hwnd);
				m_hwnd = IntPtr.Zero;
			}
		}

		private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			if (msg >= 1224 && msg < 1339)
			{
				return (IntPtr)OnUOAssistMessage(msg, wParam.ToInt32(), lParam.ToInt32());
			}
			return DefWindowProcW(hWnd, msg, wParam, lParam);
		}

		private int OnUOAssistMessage(uint msg, int wParam, int lParam)
		{
			switch ((UOAMessage)msg)
			{
			case UOAMessage.First:
				if (_wndRegs.ContainsKey(wParam))
				{
					_wndRegs.Remove(wParam);
					return 2;
				}
				_wndRegs.Add(wParam, new WndRegEnt(wParam, (lParam == 1) ? 1 : 0));
				if (lParam == 1 && World.InGame)
				{
					foreach (Item value in World.Items.Values)
					{
						if (value.IsMulti)
						{
							PostMessage((IntPtr)wParam, 1335u, (IntPtr)((value.X & 0xFFFF) | ((value.Y & 0xFFFF) << 16)), (IntPtr)value.Graphic);
						}
					}
				}
				return 1;
			case UOAMessage.GET_COORDS:
				if (World.Player != null)
				{
					return (World.Player.X & 0xFFFF) | ((World.Player.Y & 0xFFFF) << 16);
				}
				break;
			case UOAMessage.GET_STAT:
				if (World.Player == null || wParam < 0 || wParam > 5)
				{
					return 0;
				}
				return wParam switch
				{
					0 => World.Player.Strength, 
					1 => World.Player.Intelligence, 
					2 => World.Player.Dexterity, 
					3 => World.Player.Weight, 
					4 => World.Player.HitsMax, 
					5 => (int)World.Player.TithingPoints, 
					_ => 0, 
				};
			case UOAMessage.DISPLAY_TEXT:
				if (World.Player != null)
				{
					ushort hue = (ushort)(wParam & 0xFFFF);
					StringBuilder stringBuilder2 = new StringBuilder(256);
					if (GlobalGetAtomName((ushort)lParam, stringBuilder2, 256) == 0)
					{
						return 0;
					}
					if ((wParam & 0x10000) != 0)
					{
						MessageManager.HandleMessage(null, stringBuilder2.ToString(), "System", hue, MessageType.Regular, 3, TextType.SYSTEM, unicode: true);
					}
					else
					{
						World.Player.AddMessage(MessageType.Regular, stringBuilder2.ToString(), 3, hue, isunicode: true, TextType.OBJECT);
					}
					return 1;
				}
				break;
			case UOAMessage.REQUEST_MULTIS:
				return (World.Player != null) ? 1 : 0;
			case UOAMessage.ADD_CMD:
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				if (GlobalGetAtomName((ushort)lParam, stringBuilder, 256) == 0)
				{
					return 0;
				}
				if (wParam == 0)
				{
					CommandManager.UnRegister(stringBuilder.ToString());
					return 0;
				}
				new WndCmd(_cmdID, (IntPtr)wParam, stringBuilder.ToString());
				return (int)_cmdID++;
			}
			case UOAMessage.GET_UID:
				if (!(World.Player != null))
				{
					return 0;
				}
				return (int)World.Player.Serial;
			case UOAMessage.GET_UO_HWND:
			{
				SDL.SDL_SysWMinfo info = default(SDL.SDL_SysWMinfo);
				SDL.SDL_VERSION(out info.version);
				SDL.SDL_GetWindowWMInfo(SDL.SDL_GL_GetCurrentWindow(), ref info);
				IntPtr intPtr = IntPtr.Zero;
				if (info.subsystem == SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS)
				{
					intPtr = info.info.win.window;
				}
				return (int)intPtr;
			}
			case UOAMessage.GET_POISON:
				if (!(World.Player != null) || !World.Player.IsPoisoned)
				{
					return 0;
				}
				return 1;
			}
			return 0;
		}

		public void SignalMapChanged(int map)
		{
			PostMessage(1337u, (IntPtr)map, IntPtr.Zero);
		}

		public void SignalMessage(string str)
		{
			PostMessage(1425u, (IntPtr)GlobalAddAtom(str), IntPtr.Zero);
		}

		public void SignalHitsUpdate()
		{
			if (World.Player != null)
			{
				PostMessage(1333u, (IntPtr)World.Player.HitsMax, (IntPtr)World.Player.Hits);
			}
		}

		public void SignalStaminaUpdate()
		{
			if (World.Player != null)
			{
				PostMessage(1334u, (IntPtr)World.Player.HitsMax, (IntPtr)World.Player.Hits);
			}
		}

		public void SignalManaUpdate()
		{
			if (World.Player != null)
			{
				PostMessage(1329u, (IntPtr)World.Player.HitsMax, (IntPtr)World.Player.Hits);
			}
		}

		public void SignalAddMulti(ushort graphic, ushort x, ushort y)
		{
			IntPtr intPtr = (IntPtr)((x & 0xFFFF) | ((y & 0xFFFF) << 16));
			if (intPtr == IntPtr.Zero)
			{
				return;
			}
			foreach (KeyValuePair<int, WndRegEnt> wndReg in _wndRegs)
			{
				if (wndReg.Value.Type == 1)
				{
					PostMessage((IntPtr)wndReg.Value.Handle, 1335u, intPtr, (IntPtr)graphic);
				}
			}
		}

		private void PostMessage(uint msg, IntPtr wParam, IntPtr lParam)
		{
			List<int> list = null;
			foreach (KeyValuePair<int, WndRegEnt> wndReg in _wndRegs)
			{
				if (PostMessage((IntPtr)wndReg.Key, msg, wParam, lParam) == 0)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(wndReg.Key);
				}
			}
			if (list == null)
			{
				return;
			}
			foreach (int item in list)
			{
				_wndRegs.Remove(item);
			}
		}
	}

	private static CustomWindow _customWindow;

	public static void Start()
	{
		if (Environment.OSVersion.Platform != PlatformID.Win32NT)
		{
			Log.Warn("This OS does not support the UOAssist API");
		}
		else
		{
			_customWindow = new CustomWindow("UOASSIST-TP-MSG-WND");
		}
	}

	public static void SignalMapChanged(int newMap)
	{
		_customWindow?.SignalMapChanged(newMap);
	}

	public static void SignalMessage(string msg)
	{
		_customWindow?.SignalMessage(msg);
	}

	public static void SignalHits()
	{
		_customWindow?.SignalHitsUpdate();
	}

	public static void SignalStamina()
	{
		_customWindow?.SignalStaminaUpdate();
	}

	public static void SignalMana()
	{
		_customWindow?.SignalManaUpdate();
	}

	public static void SignalAddMulti(ushort graphic, ushort x, ushort y)
	{
		_customWindow?.SignalAddMulti(graphic, x, y);
	}
}
