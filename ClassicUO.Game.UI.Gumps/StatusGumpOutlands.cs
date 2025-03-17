using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class StatusGumpOutlands : StatusGumpBase
{
	private enum MobileStats
	{
		Name,
		Strength,
		Dexterity,
		Intelligence,
		HealthCurrent,
		HealthMax,
		StaminaCurrent,
		StaminaMax,
		ManaCurrent,
		ManaMax,
		WeightCurrent,
		WeightMax,
		Followers,
		FollowersMax,
		Gold,
		AR,
		PhysicalOverallResistance,
		KlingenResistance,
		StumpfResistance,
		SpitzResistance,
		KrankheitsResistance,
		Damage,
		HungerSatisfactionMinutes,
		MurderCount,
		MurderCountDecayHours,
		CriminalTimerSeconds,
		PvpCooldownSeconds,
		BandageTimerSeconds,
		Max
	}

	private enum FillStats
	{
		Hits,
		Mana,
		Stam
	}

	private readonly GumpPicWithWidth[] _fillBars = new GumpPicWithWidth[3];

	public StatusGumpOutlands()
	{
		_ = Point.Zero;
		_labels = new Label[28];
		Add(new GumpPic(0, 0, 10860, 0));
		Add(new GumpPic(34, 12, 2053, 0));
		Add(new GumpPic(34, 25, 2053, 0));
		Add(new GumpPic(34, 38, 2053, 0));
		if (Client.Version >= ClientVersion.CV_5020)
		{
			Button button = new Button(0, 2103, 2104, 2104, "", 0);
			button.X = 159;
			button.Y = 40;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
			Label label = new Label(ResGumps.Buffs, isunicode: false, 902, 60, 1);
			label.X = 174;
			label.Y = 40;
			Add(label);
		}
		ushort graphic = 2054;
		if (World.Player.IsPoisoned)
		{
			graphic = 2056;
		}
		else if (World.Player.IsYellowHits)
		{
			graphic = 2057;
		}
		_fillBars[0] = new GumpPicWithWidth(34, 12, graphic, 0, 0);
		_fillBars[1] = new GumpPicWithWidth(34, 25, 2054, 0, 0);
		_fillBars[2] = new GumpPicWithWidth(34, 38, 2054, 0, 0);
		Add(_fillBars[0]);
		Add(_fillBars[1]);
		Add(_fillBars[2]);
		UpdateStatusFillBar(FillStats.Hits, World.Player.Hits, World.Player.HitsMax);
		UpdateStatusFillBar(FillStats.Mana, World.Player.Mana, World.Player.ManaMax);
		UpdateStatusFillBar(FillStats.Stam, World.Player.Stamina, World.Player.StaminaMax);
		Label label2 = new Label((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty, isunicode: false, 902, 320, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label2.X = 108;
		label2.Y = 12;
		Label label3 = label2;
		_labels[0] = label3;
		Add(label3);
		int x = 60;
		AddStatTextLabel(World.Player.Strength.ToString(), MobileStats.Strength, x, 73, 0, 902);
		AddStatTextLabel(World.Player.Dexterity.ToString(), MobileStats.Dexterity, x, 102, 0, 902);
		AddStatTextLabel(World.Player.Intelligence.ToString(), MobileStats.Intelligence, x, 130, 0, 902);
		AddStatTextLabel(World.Player.Hits.ToString(), MobileStats.HealthCurrent, 117, 66, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.HitsMax.ToString(), MobileStats.HealthMax, 117, 79, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.Stamina.ToString(), MobileStats.StaminaCurrent, 117, 95, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.StaminaMax.ToString(), MobileStats.StaminaMax, 117, 108, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.Mana.ToString(), MobileStats.ManaCurrent, 117, 124, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.ManaMax.ToString(), MobileStats.ManaMax, 117, 137, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		Add(new Line(118, 79, 30, 1, 4281874488u));
		Add(new Line(118, 108, 30, 1, 4281874488u));
		Add(new Line(118, 137, 30, 1, 4281874488u));
		AddStatTextLabel($"{World.Player.Followers}/{World.Player.FollowersMax}", MobileStats.Followers, 192, 73, 0, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.PhysicalResistance.ToString(), MobileStats.AR, 196, 102, 0, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.Weight.ToString(), MobileStats.WeightCurrent, 185, 124, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		AddStatTextLabel(World.Player.WeightMax.ToString(), MobileStats.WeightMax, 185, 137, 40, 902, TEXT_ALIGN_TYPE.TS_CENTER);
		Add(new Line(186, 137, 30, 1, 4281874488u));
		AddStatTextLabel(World.Player.Luck.ToString(), MobileStats.HungerSatisfactionMinutes, 282, 44, 0, 902);
		AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.MurderCount, 260, 73, 0, 902);
		AddStatTextLabel($"{World.Player.DamageMin}-{World.Player.DamageMax}", MobileStats.Damage, 260, 102, 0, 902);
		AddStatTextLabel(World.Player.Gold.ToString(), MobileStats.Gold, 254, 132, 0, 902);
		AddStatTextLabel(World.Player.ColdResistance.ToString(), MobileStats.CriminalTimerSeconds, 354, 44, 0, 902);
		AddStatTextLabel(World.Player.FireResistance.ToString(), MobileStats.MurderCountDecayHours, 354, 73, 0, 902);
		AddStatTextLabel(World.Player.PoisonResistance.ToString(), MobileStats.PvpCooldownSeconds, 354, 102, 0, 902);
		AddStatTextLabel(World.Player.EnergyResistance.ToString(), MobileStats.BandageTimerSeconds, 354, 131, 0, 902);
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if ((double)_refreshTime < totalTime)
		{
			_refreshTime = (long)totalTime + 250;
			UpdateStatusFillBar(FillStats.Hits, World.Player.Hits, World.Player.HitsMax);
			UpdateStatusFillBar(FillStats.Mana, World.Player.Mana, World.Player.ManaMax);
			UpdateStatusFillBar(FillStats.Stam, World.Player.Stamina, World.Player.StaminaMax);
			_labels[0].Text = ((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty);
			_labels[1].Text = World.Player.Strength.ToString();
			_labels[2].Text = World.Player.Dexterity.ToString();
			_labels[3].Text = World.Player.Intelligence.ToString();
			_labels[4].Text = World.Player.Hits.ToString();
			_labels[5].Text = World.Player.HitsMax.ToString();
			_labels[6].Text = World.Player.Stamina.ToString();
			_labels[7].Text = World.Player.StaminaMax.ToString();
			_labels[8].Text = World.Player.Mana.ToString();
			_labels[9].Text = World.Player.ManaMax.ToString();
			_labels[12].Text = $"{World.Player.Followers}/{World.Player.FollowersMax}";
			_labels[15].Text = World.Player.PhysicalResistance.ToString();
			_labels[16].Text = World.Player.PhysResiOverall.ToString();
			if (!string.IsNullOrEmpty(World.Player.KlingenResistance.ToString()))
			{
				_labels[17].Text = World.Player.KlingenResistance.ToString();
			}
			else
			{
				_labels[17].Text = "0";
			}
			if (!string.IsNullOrEmpty(World.Player.StumpfResistance.ToString()))
			{
				_labels[18].Text = World.Player.StumpfResistance.ToString();
			}
			else
			{
				_labels[18].Text = "0";
			}
			if (!string.IsNullOrEmpty(World.Player.SpitzResistance.ToString()))
			{
				_labels[19].Text = World.Player.SpitzResistance.ToString();
			}
			else
			{
				_labels[19].Text = "0";
			}
			if (!string.IsNullOrEmpty(World.Player.KrankheitsResistance.ToString()))
			{
				_labels[20].Text = World.Player.KrankheitsResistance.ToString();
			}
			else
			{
				_labels[20].Text = "0";
			}
			_labels[10].Text = World.Player.Weight.ToString();
			_labels[11].Text = World.Player.WeightMax.ToString();
			_labels[21].Text = $"{World.Player.DamageMin}-{World.Player.DamageMax}";
			_labels[14].Text = World.Player.Gold.ToString();
			_labels[22].Text = World.Player.Luck.ToString();
			_labels[23].Text = World.Player.StatsCap.ToString();
			_labels[24].Text = World.Player.FireResistance.ToString();
			_labels[25].Text = World.Player.ColdResistance.ToString();
			_labels[26].Text = World.Player.PoisonResistance.ToString();
			_labels[27].Text = World.Player.EnergyResistance.ToString();
		}
		base.Update(totalTime, frameTime);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button != MouseButtonType.Left)
		{
			return;
		}
		if (TargetManager.IsTargeting)
		{
			TargetManager.Target(World.Player);
			Mouse.LastLeftButtonClickTime = 0u;
			return;
		}
		Point value = new Point(x, y);
		if (new Rectangle(base.Bounds.Width - 42, base.Bounds.Height - 25, base.Bounds.Width, base.Bounds.Height).Contains(value))
		{
			UIManager.GetGump<BaseHealthBarGump>(World.Player)?.Dispose();
			if (ProfileManager.CurrentProfile.CustomBarsToggled)
			{
				HealthBarGumpCustom healthBarGumpCustom = new HealthBarGumpCustom(World.Player);
				healthBarGumpCustom.X = base.ScreenCoordinateX;
				healthBarGumpCustom.Y = base.ScreenCoordinateY;
				UIManager.Add(healthBarGumpCustom);
			}
			else
			{
				HealthBarGump healthBarGump = new HealthBarGump(World.Player);
				healthBarGump.X = base.ScreenCoordinateX;
				healthBarGump.Y = base.ScreenCoordinateY;
				UIManager.Add(healthBarGump);
			}
			Dispose();
		}
	}

	private static int CalculatePercents(int max, int current, int maxValue)
	{
		if (max > 0)
		{
			max = current * 100 / max;
			if (max > 100)
			{
				max = 100;
			}
			if (max > 1)
			{
				max = maxValue * max / 100;
			}
		}
		return max;
	}

	private void UpdateStatusFillBar(FillStats id, int current, int max)
	{
		ushort graphic = 2054;
		if (id == FillStats.Hits)
		{
			if (World.Player.IsPoisoned)
			{
				graphic = 2056;
			}
			else if (World.Player.IsYellowHits)
			{
				graphic = 2057;
			}
		}
		if (max > 0)
		{
			_fillBars[(int)id].Graphic = graphic;
			_fillBars[(int)id].Percent = CalculatePercents(max, current, 109);
		}
	}

	private void AddStatTextLabel(string text, MobileStats stat, int x, int y, int maxWidth = 0, ushort hue = 902, TEXT_ALIGN_TYPE alignment = TEXT_ALIGN_TYPE.TS_LEFT)
	{
		Label label = new Label(text, isunicode: false, hue, maxWidth, 1, FontStyle.None, alignment);
		label.X = x - 5;
		label.Y = y;
		Label label2 = label;
		_labels[(int)stat] = label2;
		Add(label2);
	}
}
