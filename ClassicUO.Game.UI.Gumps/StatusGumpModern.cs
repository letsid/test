using System;
using System.Collections.Generic;
using ClassicUO.Configuration;
using ClassicUO.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class StatusGumpModern : StatusGumpBase
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
		WeightMax,
		Followers,
		WeightCurrent,
		LowerReagentCost,
		SpellDamageInc,
		FasterCasting,
		FasterCastRecovery,
		StatCap,
		HitChanceInc,
		DefenseChanceInc,
		LowerManaCost,
		DamageChanceInc,
		SwingSpeedInc,
		Luck,
		Gold,
		AR,
		PhyisicalResistanceOverall,
		KlingenResistance,
		StumpfResistance,
		SpitzResistance,
		KrankheitsResistance,
		RF,
		RC,
		RP,
		RE,
		Damage,
		Sex,
		NumStats
	}

	public static Dictionary<int, ushort> statusBarGumps = new Dictionary<int, ushort>
	{
		{ 0, 10862 },
		{ 1, 290 },
		{ 2, 291 },
		{ 3, 292 },
		{ 4, 293 },
		{ 5, 294 },
		{ 6, 295 },
		{ 7, 296 },
		{ 8, 297 },
		{ 9, 298 }
	};

	public List<ushort> PacketResistences { get; set; }

	public StatusGumpModern()
	{
		Point zero = Point.Zero;
		int num = 0;
		int x = 405;
		_labels = new Label[37];
		Add(new GumpPic(0, 0, statusBarGumps[ProfileManager.CurrentProfile.StatusBarGump], 0));
		if (Client.Version >= ClientVersion.CV_308Z)
		{
			zero.X = 389;
			zero.Y = 152;
			AddStatTextLabel((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty, MobileStats.Name, Client.UseUOPGumps ? 90 : 58, 50, 320, 450, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.BlackBorder, 1, isunicode: true);
			if (Client.Version >= ClientVersion.CV_5020)
			{
				Button button = new Button(0, 30008, 30009, 30009, "", 0);
				button.X = 437;
				button.Y = 150;
				button.ButtonAction = ButtonAction.Activate;
				Add(button);
			}
			if (Client.UseUOPGumps)
			{
				num = 80;
				AddStatTextLabel(World.Player.HitChanceIncrease.ToString(), MobileStats.HitChanceInc, num, 161, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(58, 154, 59, 24, ResGumps.HitChanceIncrease, 0f)
				{
					CanMove = true
				});
			}
			else
			{
				num = 88;
			}
			AddStatTextLabel(World.Player.Strength.ToString(), MobileStats.Strength, num, 77, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			AddStatTextLabel(World.Player.Dexterity.ToString(), MobileStats.Dexterity, num, 105, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			AddStatTextLabel(World.Player.Intelligence.ToString(), MobileStats.Intelligence, num, 133, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			Add(new HitBox(58, 70, 59, 24, ResGumps.Strength, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(58, 98, 59, 24, ResGumps.Dexterity, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(58, 126, 59, 24, ResGumps.Intelligence, 0f)
			{
				CanMove = true
			});
			int maxWidth = 40;
			if (Client.UseUOPGumps)
			{
				num = 150;
				AddStatTextLabel($"{World.Player.DefenseChanceIncrease}/{World.Player.MaxDefenseChanceIncrease}", MobileStats.DefenseChanceInc, num, 161, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(124, 154, 59, 24, ResGumps.DefenseChanceIncrease, 0f)
				{
					CanMove = true
				});
			}
			else
			{
				num = 146;
			}
			num -= 5;
			AddStatTextLabel(World.Player.Hits.ToString(), MobileStats.HealthCurrent, num, 70, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			AddStatTextLabel(World.Player.HitsMax.ToString(), MobileStats.HealthMax, num, 83, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			AddStatTextLabel(World.Player.Stamina.ToString(), MobileStats.StaminaCurrent, num, 98, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			AddStatTextLabel(World.Player.StaminaMax.ToString(), MobileStats.StaminaMax, num, 111, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			AddStatTextLabel(World.Player.Mana.ToString(), MobileStats.ManaCurrent, num, 126, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			AddStatTextLabel(World.Player.ManaMax.ToString(), MobileStats.ManaMax, num, 139, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			num += 5;
			Add(new Line(num, 138, Math.Abs(num - 185), 1, 4281874488u));
			Add(new Line(num, 110, Math.Abs(num - 185), 1, 4281874488u));
			Add(new Line(num, 82, Math.Abs(num - 185), 1, 4281874488u));
			Add(new HitBox(124, 70, 59, 24, ResGumps.HitPoints, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(124, 98, 59, 24, ResGumps.Stamina, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(124, 126, 59, 24, ResGeneral.Mana, 0f)
			{
				CanMove = true
			});
			if (Client.UseUOPGumps)
			{
				num = 240;
				AddStatTextLabel(World.Player.LowerManaCost.ToString(), MobileStats.LowerManaCost, num, 162, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(205, 154, 65, 24, ResGumps.LowerManaCost, 0f)
				{
					CanMove = true
				});
			}
			else
			{
				num = 220;
			}
			AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, num - 10, 77, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			AddStatTextLabel(World.Player.Luck.ToString(), MobileStats.Luck, num - 10, 105, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			num -= 10;
			AddStatTextLabel(World.Player.Weight.ToString(), MobileStats.WeightCurrent, num, 126, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			int num2 = (Client.UseUOPGumps ? 236 : 216);
			Add(new Line(num2, 138, Math.Abs(num2 - (Client.UseUOPGumps ? 270 : 250)), 1, 4281874488u));
			AddStatTextLabel(World.Player.WeightMax.ToString(), MobileStats.WeightMax, num, 139, maxWidth, 902, TEXT_ALIGN_TYPE.TS_CENTER, FontStyle.None, 1);
			num = (Client.UseUOPGumps ? 205 : 188);
			Add(new HitBox(num, 70, 65, 24, ResGumps.MaximumStats, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 98, 65, 24, ResGumps.Luck, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 126, 65, 24, ResGeneral.Weight, 0f)
			{
				CanMove = true
			});
			if (Client.UseUOPGumps)
			{
				num = 320;
				AddStatTextLabel(World.Player.DamageIncrease.ToString(), MobileStats.DamageChanceInc, num, 105, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.SwingSpeedIncrease.ToString(), MobileStats.SwingSpeedInc, num, 161, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(285, 98, 69, 24, ResGumps.WeaponDamageIncrease, 0f)
				{
					CanMove = true
				});
				Add(new HitBox(285, 154, 69, 24, ResGumps.SwingSpeedIncrease, 0f)
				{
					CanMove = true
				});
			}
			else
			{
				num = 280;
				AddStatTextLabel(World.Player.Gold.ToString(), MobileStats.Gold, num, 105, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(260, 98, 69, 24, ResGumps.Gold, 0f)
				{
					CanMove = true
				});
			}
			AddStatTextLabel($"{World.Player.DamageMin}-{World.Player.DamageMax}", MobileStats.Damage, num, 77, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			AddStatTextLabel($"{World.Player.Followers}-{World.Player.FollowersMax}", MobileStats.Followers, num, 133, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			num = (Client.UseUOPGumps ? 285 : 260);
			Add(new HitBox(num, 70, 69, 24, ResGumps.Damage, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 126, 69, 24, ResGumps.Followers, 0f)
			{
				CanMove = true
			});
			if (Client.UseUOPGumps)
			{
				num = 400;
				AddStatTextLabel(World.Player.LowerReagentCost.ToString(), MobileStats.LowerReagentCost, num, 77, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.SpellDamageIncrease.ToString(), MobileStats.SpellDamageInc, num, 105, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.FasterCasting.ToString(), MobileStats.FasterCasting, num, 133, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.FasterCastRecovery.ToString(), MobileStats.FasterCastRecovery, num, 161, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				num = 365;
				Add(new HitBox(num, 70, 55, 24, ResGumps.LowerReagentCost, 0f)
				{
					CanMove = true
				});
				Add(new HitBox(num, 98, 55, 24, ResGumps.SpellDamageIncrease, 0f)
				{
					CanMove = true
				});
				Add(new HitBox(num, 126, 55, 24, ResGumps.FasterCasting, 0f)
				{
					CanMove = true
				});
				Add(new HitBox(num, 154, 55, 24, ResGumps.FasterCastRecovery, 0f)
				{
					CanMove = true
				});
				num = 480;
				AddStatTextLabel(World.Player.Gold.ToString(), MobileStats.Gold, num, 161, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				Add(new HitBox(445, 154, 55, 24, ResGumps.Gold, 0f)
				{
					CanMove = true
				});
				num = 475;
				AddStatTextLabel($"{World.Player.PhysicalResistance}/{World.Player.MaxPhysicResistence}", MobileStats.AR, num, 74, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel($"{World.Player.FireResistance}/{World.Player.MaxFireResistence}", MobileStats.RF, num, 92, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel($"{World.Player.ColdResistance}/{World.Player.MaxColdResistence}", MobileStats.RC, num, 106, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel($"{World.Player.PoisonResistance}/{World.Player.MaxPoisonResistence}", MobileStats.RP, num, 120, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel($"{World.Player.EnergyResistance}/{World.Player.MaxEnergyResistence}", MobileStats.RE, num, 134, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			}
			else
			{
				num = 354;
				AddStatTextLabel(World.Player.PhysicalResistance.ToString(), MobileStats.AR, num, 76, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.FireResistance.ToString(), MobileStats.RF, num, 92, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.ColdResistance.ToString(), MobileStats.RC, num, 106, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.PoisonResistance.ToString(), MobileStats.RP, num, 120, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.EnergyResistance.ToString(), MobileStats.RE, num, 134, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.PhysResiOverall.ToString(), MobileStats.PhyisicalResistanceOverall, x, 76, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.KlingenResistance.ToString(), MobileStats.KlingenResistance, x, 92, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.StumpfResistance.ToString(), MobileStats.StumpfResistance, x, 106, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.SpitzResistance.ToString(), MobileStats.SpitzResistance, x, 120, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
				AddStatTextLabel(World.Player.KrankheitsResistance.ToString(), MobileStats.KrankheitsResistance, x, 134, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			}
			num = (Client.UseUOPGumps ? 445 : 334);
			Add(new HitBox(x, 76, 40, 14, "Durchschnittliche physische Resistenzen", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(x, 92, 40, 14, "Klingenresistenz", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(x, 106, 40, 14, "Stumpfresistenz", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(x, 120, 40, 14, "Spitzresistenz", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(x, 134, 40, 14, "Krankheitsresistenz", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 76, 40, 14, "Rüstwert(" + ResGumps.PhysicalResistance + ")", 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 92, 40, 14, ResGumps.FireResistance, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 106, 40, 14, ResGumps.ColdResistance, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 120, 40, 14, ResGumps.PoisonResistance, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(num, 134, 40, 14, ResGumps.EnergyResistance, 0f)
			{
				CanMove = true
			});
		}
		else if (Client.Version == ClientVersion.CV_308D)
		{
			AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, 171, 124, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			Add(new HitBox(171, 124, 34, 12, ResGumps.MaxStats, 0f)
			{
				CanMove = true
			});
		}
		else if (Client.Version == ClientVersion.CV_308J)
		{
			AddStatTextLabel(World.Player.StatsCap.ToString(), MobileStats.StatCap, 180, 131, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			AddStatTextLabel($"{World.Player.Followers}/{World.Player.FollowersMax}", MobileStats.Followers, 180, 144, 0, 902, TEXT_ALIGN_TYPE.TS_LEFT, FontStyle.None, 1);
			Add(new HitBox(180, 131, 34, 12, ResGumps.MaxStats, 0f)
			{
				CanMove = true
			});
			Add(new HitBox(171, 144, 34, 12, ResGumps.Followers, 0f)
			{
				CanMove = true
			});
		}
		_point = zero;
	}

	private void AddStatTextLabel(string text, MobileStats stat, int x, int y, int maxWidth = 0, ushort hue = 902, TEXT_ALIGN_TYPE alignment = TEXT_ALIGN_TYPE.TS_LEFT, FontStyle style = FontStyle.None, byte font = 1, bool isunicode = false)
	{
		FontStyle style2 = style;
		Label label = new Label(text, isunicode, hue, maxWidth, font, style2, alignment);
		label.X = x;
		label.Y = y;
		Label label2 = label;
		_labels[(int)stat] = label2;
		Add(label2);
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
			_labels[0].Text = ((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty);
			if (Client.UseUOPGumps)
			{
				_labels[18].Text = World.Player.HitChanceIncrease.ToString();
			}
			_labels[1].Text = World.Player.Strength.ToString();
			_labels[2].Text = World.Player.Dexterity.ToString();
			_labels[3].Text = World.Player.Intelligence.ToString();
			if (Client.UseUOPGumps)
			{
				_labels[19].Text = $"{World.Player.DefenseChanceIncrease}/{World.Player.MaxDefenseChanceIncrease}";
			}
			_labels[4].Text = World.Player.Hits.ToString();
			if (World.Player.IsPoisoned)
			{
				_labels[5].Hue = 63;
				_labels[4].Hue = 63;
			}
			else if (World.Player.Hits != 0 && World.Player.Hits * 100 / World.Player.HitsMax < 50)
			{
				_labels[5].Hue = 33;
				_labels[4].Hue = 33;
			}
			else
			{
				_labels[5].Hue = 902;
				_labels[4].Hue = 902;
			}
			if (World.Player.Mana != 0 && World.Player.Mana * 100 / World.Player.ManaMax < 50)
			{
				_labels[9].Hue = 33;
				_labels[8].Hue = 33;
			}
			else
			{
				_labels[9].Hue = 902;
				_labels[8].Hue = 902;
			}
			if (World.Player.Stamina != 0 && World.Player.Stamina * 100 / World.Player.StaminaMax < 50)
			{
				_labels[7].Hue = 33;
				_labels[6].Hue = 33;
			}
			else
			{
				_labels[7].Hue = 902;
				_labels[6].Hue = 902;
			}
			_labels[5].Text = World.Player.HitsMax.ToString();
			_labels[6].Text = World.Player.Stamina.ToString();
			_labels[7].Text = World.Player.StaminaMax.ToString();
			_labels[8].Text = World.Player.Mana.ToString();
			_labels[9].Text = World.Player.ManaMax.ToString();
			if (Client.UseUOPGumps)
			{
				_labels[20].Text = World.Player.LowerManaCost.ToString();
			}
			_labels[17].Text = World.Player.StatsCap + "%";
			_labels[23].Text = World.Player.Luck + "%";
			_labels[12].Text = World.Player.Weight.ToString();
			_labels[10].Text = World.Player.WeightMax.ToString();
			if (Client.UseUOPGumps)
			{
				_labels[21].Text = World.Player.DamageIncrease.ToString();
				_labels[22].Text = World.Player.SwingSpeedIncrease.ToString();
			}
			if (World.Player.Gold.ToString().Length > 9)
			{
				_labels[24].Text = World.Player.Gold.ToString().Substring(0, 4) + "M";
			}
			else if (World.Player.Gold.ToString().Length > 8)
			{
				_labels[24].Text = World.Player.Gold.ToString().Substring(0, 3) + "M";
			}
			else if (World.Player.Gold.ToString().Length > 7)
			{
				_labels[24].Text = World.Player.Gold.ToString().Substring(0, 2) + "M";
			}
			else if (World.Player.Gold.ToString().Length > 6)
			{
				_labels[24].Text = World.Player.Gold.ToString().Substring(0, 4) + "k";
			}
			else if (World.Player.Gold.ToString().Length > 5)
			{
				_labels[24].Text = World.Player.Gold.ToString().Substring(0, 3) + "k";
			}
			else
			{
				_labels[24].Text = World.Player.Gold.ToString();
			}
			_labels[35].Text = $"{World.Player.DamageMin}-{World.Player.DamageMax}";
			_labels[11].Text = $"{World.Player.Followers}/{World.Player.FollowersMax}";
			if (Client.UseUOPGumps)
			{
				_labels[13].Text = World.Player.LowerReagentCost.ToString();
				_labels[14].Text = World.Player.SpellDamageIncrease.ToString();
				_labels[15].Text = World.Player.FasterCasting.ToString();
				_labels[16].Text = World.Player.FasterCastRecovery.ToString();
				_labels[26].Text = $"{World.Player.PhysicalResistance}/{World.Player.MaxPhysicResistence}";
				_labels[31].Text = $"{World.Player.FireResistance}/{World.Player.MaxFireResistence}";
				_labels[32].Text = $"{World.Player.ColdResistance}/{World.Player.MaxColdResistence}";
				_labels[33].Text = $"{World.Player.PoisonResistance}/{World.Player.MaxPoisonResistence}";
				_labels[34].Text = $"{World.Player.EnergyResistance}/{World.Player.MaxEnergyResistence}";
			}
			else
			{
				_labels[25].Text = World.Player.PhysicalResistance.ToString();
				_labels[26].Text = World.Player.PhysResiOverall.ToString();
				_labels[27].Text = World.Player.KlingenResistance.ToString();
				_labels[28].Text = World.Player.StumpfResistance.ToString();
				_labels[29].Text = World.Player.SpitzResistance.ToString();
				_labels[30].Text = World.Player.KrankheitsResistance.ToString();
				_labels[31].Text = World.Player.FireResistance.ToString();
				_labels[32].Text = World.Player.ColdResistance.ToString();
				_labels[33].Text = World.Player.PoisonResistance.ToString();
				_labels[34].Text = World.Player.EnergyResistance.ToString();
			}
		}
		base.Update(totalTime, frameTime);
	}
}
