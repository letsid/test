using ClassicUO.Input;

namespace ClassicUO.Game.Managers;

internal static class DelayedObjectClickManager
{
	public static uint Serial { get; private set; }

	public static bool IsEnabled { get; private set; }

	public static uint Timer { get; private set; }

	public static int X { get; set; }

	public static int Y { get; set; }

	public static int LastMouseX { get; set; }

	public static int LastMouseY { get; set; }

	public static void Update()
	{
		if (!IsEnabled || Timer > Time.Ticks)
		{
			return;
		}
		if (World.Get(Serial) != null)
		{
			GameActions.SingleClick(Serial);
			if (World.ClientFeatures.PopupEnabled)
			{
				GameActions.OpenPopupMenu(Serial);
			}
		}
		Clear();
	}

	public static void Set(uint serial, int x, int y, uint timer)
	{
		Serial = serial;
		LastMouseX = Mouse.Position.X;
		LastMouseY = Mouse.Position.Y;
		X = x;
		Y = y;
		Timer = timer;
		IsEnabled = true;
	}

	public static void Clear()
	{
		IsEnabled = false;
		Serial = uint.MaxValue;
		Timer = 0u;
	}

	public static void Clear(uint serial)
	{
		if (Serial == serial)
		{
			Timer = 0u;
			Serial = 0u;
			IsEnabled = false;
			X = 0;
			Y = 0;
			LastMouseX = 0;
			LastMouseY = 0;
		}
	}
}
