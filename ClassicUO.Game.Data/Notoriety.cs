using ClassicUO.Configuration;

namespace ClassicUO.Game.Data;

internal static class Notoriety
{
	internal enum AlaNotoColors
	{
		PvPVictimColor = 2713,
		GMColor = 2471,
		BossNotorietyColor = 2714
	}

	public static ushort GetHue(NotorietyFlag flag)
	{
		return flag switch
		{
			NotorietyFlag.Innocent => ProfileManager.CurrentProfile.InnocentHue, 
			NotorietyFlag.Ally => ProfileManager.CurrentProfile.FriendHue, 
			NotorietyFlag.Criminal => ProfileManager.CurrentProfile.CriminalHue, 
			NotorietyFlag.Gray => ProfileManager.CurrentProfile.CanAttackHue, 
			NotorietyFlag.Enemy => ProfileManager.CurrentProfile.EnemyHue, 
			NotorietyFlag.Boss => 2714, 
			NotorietyFlag.PvPVictim => 2713, 
			NotorietyFlag.Staff => 2471, 
			NotorietyFlag.Murderer => ProfileManager.CurrentProfile.MurdererHue, 
			NotorietyFlag.Invulnerable => 52, 
			_ => 0, 
		};
	}

	public static string GetHTMLHue(NotorietyFlag flag)
	{
		switch (flag)
		{
		case NotorietyFlag.Innocent:
			return "<basefont color=\"cyan\">";
		case NotorietyFlag.Ally:
			return "<basefont color=\"lime\">";
		case NotorietyFlag.Gray:
		case NotorietyFlag.Criminal:
			return "<basefont color=\"gray\">";
		case NotorietyFlag.Enemy:
			return "<basefont color=\"orange\">";
		case NotorietyFlag.Murderer:
			return "<basefont color=\"red\">";
		case NotorietyFlag.Invulnerable:
			return "<basefont color=\"yellow\">";
		default:
			return string.Empty;
		}
	}
}
