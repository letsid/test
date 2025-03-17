using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Input;

namespace ClassicUO.Game.UI.Gumps;

internal class TargetFrameHealthbarGumpCustom : HealthBarGumpCustom
{
	public TargetFrameHealthbarGumpCustom(Entity entity)
		: base(entity)
	{
	}

	public TargetFrameHealthbarGumpCustom(uint serial)
		: base(serial)
	{
	}

	public TargetFrameHealthbarGumpCustom()
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
