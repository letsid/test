using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using SDL2;

namespace ClassicUO.Input;

internal static class Mouse
{
	public const int MOUSE_DELAY_DOUBLE_CLICK = 350;

	public static Point Position;

	public static Point LClickPosition;

	public static Point RClickPosition;

	public static Point MClickPosition;

	public static bool SimulateClick;

	public static uint LastLeftButtonClickTime { get; set; }

	public static uint LastMidButtonClickTime { get; set; }

	public static uint LastRightButtonClickTime { get; set; }

	public static bool CancelDoubleClick { get; set; }

	public static bool LButtonPressed { get; set; }

	public static bool RButtonPressed { get; set; }

	public static bool MButtonPressed { get; set; }

	public static bool XButtonPressed { get; set; }

	public static bool IsDragging { get; set; }

	public static Point LDragOffset
	{
		get
		{
			if (!LButtonPressed)
			{
				return Point.Zero;
			}
			return Position - LClickPosition;
		}
	}

	public static Point RDragOffset
	{
		get
		{
			if (!RButtonPressed)
			{
				return Point.Zero;
			}
			return Position - RClickPosition;
		}
	}

	public static Point MDragOffset
	{
		get
		{
			if (!MButtonPressed)
			{
				return Point.Zero;
			}
			return Position - MClickPosition;
		}
	}

	public static bool MouseInWindow { get; set; }

	public static void ButtonPress(MouseButtonType type)
	{
		CancelDoubleClick = false;
		AwayStateHelper.NotifyPresent();
		switch (type)
		{
		case MouseButtonType.Left:
			LButtonPressed = true;
			LClickPosition = Position;
			break;
		case MouseButtonType.Middle:
			MButtonPressed = true;
			MClickPosition = Position;
			break;
		case MouseButtonType.Right:
			RButtonPressed = true;
			RClickPosition = Position;
			break;
		case MouseButtonType.XButton1:
		case MouseButtonType.XButton2:
			XButtonPressed = true;
			break;
		}
		SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_TRUE);
	}

	public static void ButtonRelease(MouseButtonType type)
	{
		switch (type)
		{
		case MouseButtonType.Left:
			LButtonPressed = false;
			break;
		case MouseButtonType.Middle:
			MButtonPressed = false;
			break;
		case MouseButtonType.Right:
			RButtonPressed = false;
			break;
		case MouseButtonType.XButton1:
		case MouseButtonType.XButton2:
			XButtonPressed = false;
			break;
		}
		if (!LButtonPressed && !RButtonPressed && !MButtonPressed)
		{
			SDL.SDL_CaptureMouse(SDL.SDL_bool.SDL_FALSE);
		}
	}

	public static void Update()
	{
		if (!SimulateClick)
		{
			if (!MouseInWindow)
			{
				SDL.SDL_GetGlobalMouseState(out var x, out var y);
				SDL.SDL_GetWindowPosition(Client.Game.Window.Handle, out var x2, out var y2);
				Position.X = x - x2;
				Position.Y = y - y2;
			}
			else
			{
				SDL.SDL_GetMouseState(out Position.X, out Position.Y);
			}
			Position.X = (int)((double)Position.X * (double)Client.Game.GraphicManager.PreferredBackBufferWidth / (double)Client.Game.Window.ClientBounds.Width);
			Position.Y = (int)((double)Position.Y * (double)Client.Game.GraphicManager.PreferredBackBufferHeight / (double)Client.Game.Window.ClientBounds.Height);
			IsDragging = LButtonPressed || RButtonPressed || MButtonPressed;
		}
	}
}
