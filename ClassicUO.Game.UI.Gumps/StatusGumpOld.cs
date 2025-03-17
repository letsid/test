using ClassicUO.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class StatusGumpOld : StatusGumpBase
{
	private enum MobileStats
	{
		Name,
		Strength,
		Dexterity,
		Intelligence,
		HealthCurrent,
		StaminaCurrent,
		ManaCurrent,
		WeightCurrent,
		Gold,
		AR,
		Sex,
		NumStats
	}

	public StatusGumpOld()
	{
		Point zero = Point.Zero;
		_labels = new Label[11];
		Add(new GumpPic(0, 0, 2050, 0));
		zero.X = 244;
		zero.Y = 112;
		Label label = new Label((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty, isunicode: false, 902, 0, 1);
		label.X = 86;
		label.Y = 42;
		Label label2 = label;
		_labels[0] = label2;
		Add(label2);
		if (Client.Version >= ClientVersion.CV_5020)
		{
			Button button = new Button(0, 30008, 30009, 30009, "", 0);
			button.X = 20;
			button.Y = 42;
			button.ButtonAction = ButtonAction.Activate;
			Add(button);
		}
		Label label3 = new Label(World.Player.Strength.ToString(), isunicode: false, 902, 0, 1);
		label3.X = 86;
		label3.Y = 62;
		label2 = label3;
		Label label4 = new Label(World.Player.Dexterity.ToString(), isunicode: false, 902, 0, 1);
		label4.X = 86;
		label4.Y = 74;
		label2 = label4;
		_labels[2] = label2;
		Add(label2);
		Label label5 = new Label(World.Player.Intelligence.ToString(), isunicode: false, 902, 0, 1);
		label5.X = 86;
		label5.Y = 86;
		label2 = label5;
		_labels[3] = label2;
		Add(label2);
		Label label6 = new Label(World.Player.IsFemale ? ResGumps.Female : ResGumps.Male, isunicode: false, 902, 0, 1);
		label6.X = 86;
		label6.Y = 98;
		label2 = label6;
		_labels[10] = label2;
		Add(label2);
		Label label7 = new Label(World.Player.PhysicalResistance.ToString(), isunicode: false, 902, 0, 1);
		label7.X = 86;
		label7.Y = 110;
		label2 = label7;
		_labels[9] = label2;
		Add(label2);
		Label label8 = new Label($"{World.Player.Hits}/{World.Player.HitsMax}", isunicode: false, 902, 0, 1);
		label8.X = 171;
		label8.Y = 62;
		label2 = label8;
		_labels[4] = label2;
		Add(label2);
		Label label9 = new Label($"{World.Player.Mana}/{World.Player.ManaMax}", isunicode: false, 902, 0, 1);
		label9.X = 171;
		label9.Y = 74;
		label2 = label9;
		_labels[6] = label2;
		Add(label2);
		Label label10 = new Label($"{World.Player.Stamina}/{World.Player.StaminaMax}", isunicode: false, 902, 0, 1);
		label10.X = 171;
		label10.Y = 86;
		label2 = label10;
		_labels[5] = label2;
		Add(label2);
		Label label11 = new Label(World.Player.Gold.ToString(), isunicode: false, 902, 0, 1);
		label11.X = 171;
		label11.Y = 98;
		label2 = label11;
		_labels[8] = label2;
		Add(label2);
		Label label12 = new Label($"{World.Player.Weight}/{World.Player.WeightMax}", isunicode: false, 902, 0, 1);
		label12.X = 171;
		label12.Y = 110;
		label2 = label12;
		_labels[7] = label2;
		Add(label2);
		Add(new HitBox(86, 61, 34, 12, ResGumps.Strength, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(86, 73, 34, 12, ResGumps.Dex, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(86, 85, 34, 12, ResGumps.Intelligence, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(86, 97, 34, 12, ResGumps.Sex, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(86, 109, 34, 12, ResGumps.Armor, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(171, 61, 66, 12, ResGeneral.Hits, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(171, 73, 66, 12, ResGeneral.Mana, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(171, 85, 66, 12, ResGumps.Stamina, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(171, 97, 66, 12, ResGumps.Gold, 0f)
		{
			CanMove = true
		});
		Add(new HitBox(171, 109, 66, 12, ResGeneral.Weight, 0f)
		{
			CanMove = true
		});
		_point = zero;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (!base.IsDisposed)
		{
			if ((double)_refreshTime < totalTime)
			{
				_refreshTime = (long)totalTime + 250;
				_labels[0].Text = ((!string.IsNullOrEmpty(World.Player.Name)) ? World.Player.Name : string.Empty);
				_labels[1].Text = World.Player.Strength.ToString();
				_labels[2].Text = World.Player.Dexterity.ToString();
				_labels[3].Text = World.Player.Intelligence.ToString();
				_labels[10].Text = (World.Player.IsFemale ? ResGumps.Female : ResGumps.Male);
				_labels[9].Text = World.Player.PhysicalResistance.ToString();
				_labels[4].Text = $"{World.Player.Hits}/{World.Player.HitsMax}";
				_labels[6].Text = $"{World.Player.Mana}/{World.Player.ManaMax}";
				_labels[5].Text = $"{World.Player.Stamina}/{World.Player.StaminaMax}";
				_labels[8].Text = World.Player.Gold.ToString();
				_labels[7].Text = $"{World.Player.Weight}/{World.Player.WeightMax}";
			}
			base.Update(totalTime, frameTime);
		}
	}
}
