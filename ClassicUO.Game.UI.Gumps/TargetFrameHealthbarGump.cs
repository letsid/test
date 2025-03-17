using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Input;

namespace ClassicUO.Game.UI.Gumps;

internal class TargetFrameHealthbarGump : HealthBarGump
{
	public TargetFrameHealthbarGump(Entity entity)
		: base(entity)
	{
	}

	public TargetFrameHealthbarGump(uint serial)
		: base(serial)
	{
	}

	public TargetFrameHealthbarGump()
	{
	}

	protected override void OnDragEnd(int x, int y)
	{
		if (TargetManager.IsTargeting)
		{
			Mouse.LastLeftButtonClickTime = 0u;
			Mouse.CancelDoubleClick = true;
		}
		if (ProfileManager.CurrentProfile != null)
		{
			ProfileManager.CurrentProfile.TargetFrameHealthBarPositionX = base.X;
			ProfileManager.CurrentProfile.TargetFrameHealthBarPositionY = base.Y;
		}
		base.OnDragEnd(x, y);
	}
}
