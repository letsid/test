using ClassicUO.Configuration;
using ClassicUO.Input;
using ClassicUO.Renderer;

namespace ClassicUO.Game.Managers;

internal static class AuraManager
{
	private static readonly Aura _aura = new Aura(30);

	private static int _saveAuraUnderFeetType;

	public static bool IsEnabled
	{
		get
		{
			if (ProfileManager.CurrentProfile == null)
			{
				return false;
			}
			switch (ProfileManager.CurrentProfile.AuraUnderFeetType)
			{
			default:
				return false;
			case 1:
				if (World.Player != null && World.Player.InWarMode)
				{
					return true;
				}
				goto default;
			case 2:
				if (Keyboard.Ctrl && Keyboard.Shift)
				{
					return true;
				}
				goto default;
			case 3:
				return true;
			}
		}
	}

	public static void ToggleVisibility()
	{
		Profile currentProfile = ProfileManager.CurrentProfile;
		if (!IsEnabled)
		{
			_saveAuraUnderFeetType = currentProfile.AuraUnderFeetType;
			currentProfile.AuraUnderFeetType = 3;
		}
		else
		{
			currentProfile.AuraUnderFeetType = _saveAuraUnderFeetType;
		}
	}

	public static void Draw(UltimaBatcher2D batcher, int x, int y, ushort hue, float depth)
	{
		_aura.Draw(batcher, x, y, hue, depth);
	}
}
