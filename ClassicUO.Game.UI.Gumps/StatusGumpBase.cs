using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class StatusGumpBase : Gump
{
	protected enum ButtonType
	{
		BuffIcon,
		MinimizeMaximize
	}

	protected enum StatType
	{
		Str,
		Dex,
		Int
	}

	protected const ushort LOCK_UP_GRAPHIC = 2436;

	protected const ushort LOCK_DOWN_GRAPHIC = 2438;

	protected const ushort LOCK_LOCKED_GRAPHIC = 2092;

	protected Label[] _labels;

	protected Point _point;

	protected long _refreshTime;

	public override GumpType GumpType => GumpType.StatusGump;

	protected StatusGumpBase()
		: base(0u, 0u)
	{
		UIManager.GetGump<HealthBarGump>(World.Player)?.Dispose();
		base.CanCloseWithRightClick = true;
		CanMove = true;
	}

	public override void OnButtonClick(int buttonID)
	{
		if (buttonID == 0)
		{
			BuffGump gump = UIManager.GetGump<BuffGump>(null);
			if (gump == null)
			{
				UIManager.Add(new BuffGump(100, 100));
				return;
			}
			gump.SetInScreen();
			gump.BringOnTop();
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (TargetManager.IsTargeting)
		{
			TargetManager.Target(World.Player);
			Mouse.LastLeftButtonClickTime = 0u;
		}
	}

	public static StatusGumpBase GetStatusGump()
	{
		StatusGumpBase statusGumpBase = (CUOEnviroment.IsOutlands ? ((StatusGumpBase)UIManager.GetGump<StatusGumpOutlands>(null)) : ((StatusGumpBase)UIManager.GetGump<StatusGumpModern>(null)));
		statusGumpBase?.SetInScreen();
		return statusGumpBase;
	}

	public static StatusGumpBase AddStatusGump(int x, int y)
	{
		StatusGumpBase statusGumpBase = (CUOEnviroment.IsOutlands ? ((StatusGumpBase)new StatusGumpOutlands()) : ((StatusGumpBase)new StatusGumpModern()));
		statusGumpBase.X = x;
		statusGumpBase.Y = y;
		return statusGumpBase;
	}

	protected static ushort GetStatLockGraphic(Lock lockStatus)
	{
		return lockStatus switch
		{
			Lock.Up => 2436, 
			Lock.Down => 2438, 
			Lock.Locked => 2092, 
			_ => ushort.MaxValue, 
		};
	}

	protected override void UpdateContents()
	{
	}
}
