using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;

namespace ClassicUO.Game.Managers;

internal static class NameOverHeadManager
{
	private static NameOverHeadHandlerGump _gump;

	public static NameOverheadTypeAllowed TypeAllowed
	{
		get
		{
			return ProfileManager.CurrentProfile.NameOverheadTypeAllowed;
		}
		set
		{
			ProfileManager.CurrentProfile.NameOverheadTypeAllowed = value;
		}
	}

	public static bool IsToggled
	{
		get
		{
			return ProfileManager.CurrentProfile.NameOverheadToggled;
		}
		set
		{
			ProfileManager.CurrentProfile.NameOverheadToggled = value;
		}
	}

	public static bool IsAllowed(Entity serial)
	{
		if (serial == null)
		{
			return false;
		}
		if (SerialHelper.IsMobile(serial))
		{
			Mobile mobile = World.Get(serial.Serial) as Mobile;
			if (mobile != null && mobile.IsInvisibleAnimation())
			{
				return false;
			}
		}
		if (TypeAllowed == NameOverheadTypeAllowed.All)
		{
			return true;
		}
		if (SerialHelper.IsItem(serial.Serial) && TypeAllowed == NameOverheadTypeAllowed.Items)
		{
			return true;
		}
		if (SerialHelper.IsMobile(serial.Serial) && TypeAllowed.HasFlag(NameOverheadTypeAllowed.Mobiles))
		{
			return true;
		}
		if (TypeAllowed.HasFlag(NameOverheadTypeAllowed.Corpses) && SerialHelper.IsItem(serial.Serial))
		{
			Item item = World.Items.Get(serial);
			if ((object)item != null && item.IsCorpse)
			{
				return true;
			}
		}
		return false;
	}

	public static void Open()
	{
		if (_gump == null || _gump.IsDisposed)
		{
			_gump = new NameOverHeadHandlerGump();
			UIManager.Add(_gump);
		}
		_gump.IsEnabled = true;
		_gump.IsVisible = true;
	}

	public static void Close()
	{
		if (_gump != null)
		{
			_gump.IsEnabled = false;
			_gump.IsVisible = false;
		}
	}

	public static void ToggleOverheads()
	{
		IsToggled = !IsToggled;
	}
}
