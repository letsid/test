using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class InfoBarControl : Control
{
	private readonly Label _data;

	private readonly Label _label;

	private ushort _warningLinesHue;

	protected long _refreshTime;

	public string Text => _label.Text;

	public InfoBarVars Var { get; }

	public ushort Hue => _label.Hue;

	public InfoBarControl(string label, InfoBarVars var, ushort hue)
	{
		AcceptMouseInput = false;
		base.WantUpdateSize = true;
		CanMove = false;
		Label label2 = new Label(label, isunicode: true, 999);
		label2.Height = 20;
		label2.Hue = hue;
		_label = label2;
		Var = var;
		Label label3 = new Label("", isunicode: true, 999);
		label3.Height = 20;
		label3.X = _label.Width;
		label3.Hue = 1153;
		_data = label3;
		Add(_label);
		Add(_data);
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if ((double)_refreshTime < totalTime)
		{
			_refreshTime = (long)totalTime + 125;
			_data.Text = GetVarData(Var);
			if (ProfileManager.CurrentProfile.InfoBarHighlightType == 0 || Var == InfoBarVars.NameNotoriety)
			{
				_data.Hue = GetVarHue(Var);
			}
			else
			{
				_data.Hue = 1153;
				_warningLinesHue = GetVarHue(Var);
			}
			_data.WantUpdateSize = true;
		}
		base.WantUpdateSize = true;
		base.Update(totalTime, frameTime);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		if (Var != InfoBarVars.NameNotoriety && ProfileManager.CurrentProfile.InfoBarHighlightType == 1 && _warningLinesHue != 1153)
		{
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(_warningLinesHue);
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(_data.ScreenCoordinateX, _data.ScreenCoordinateY, _data.Width, 2), hueVector);
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), new Rectangle(_data.ScreenCoordinateX, _data.ScreenCoordinateY + base.Parent.Height - 2, _data.Width, 2), hueVector);
		}
		return true;
	}

	private string GetVarData(InfoBarVars var)
	{
		return var switch
		{
			InfoBarVars.HP => $"{World.Player.Hits}/{World.Player.HitsMax}", 
			InfoBarVars.Mana => $"{World.Player.Mana}/{World.Player.ManaMax}", 
			InfoBarVars.Stamina => $"{World.Player.Stamina}/{World.Player.StaminaMax}", 
			InfoBarVars.Gewicht => $"{World.Player.Weight}/{World.Player.WeightMax}", 
			InfoBarVars.Begleiter => $"{World.Player.Followers}/{World.Player.FollowersMax}", 
			InfoBarVars.Gold => World.Player.Gold.ToString(), 
			InfoBarVars.Schaden => $"{World.Player.DamageMin}-{World.Player.DamageMax}", 
			InfoBarVars.Armor => World.Player.PhysicalResistance.ToString(), 
			InfoBarVars.Durst => World.Player.Luck.ToString(), 
			InfoBarVars.Hunger => World.Player.StatsCap.ToString(), 
			InfoBarVars.KlingenResi => World.Player.KlingenResistance.ToString(), 
			InfoBarVars.StumpfResi => World.Player.StumpfResistance.ToString(), 
			InfoBarVars.SpitzResi => World.Player.SpitzResistance.ToString(), 
			InfoBarVars.PhysResiDurchschn => World.Player.PhysResiOverall.ToString(), 
			InfoBarVars.KrankheitsResi => World.Player.KrankheitsResistance.ToString(), 
			InfoBarVars.FeuerResi => World.Player.FireResistance.ToString(), 
			InfoBarVars.EnergieResi => World.Player.EnergyResistance.ToString(), 
			InfoBarVars.GiftResi => World.Player.PoisonResistance.ToString(), 
			InfoBarVars.MagieResi => World.Player.ColdResistance.ToString(), 
			InfoBarVars.NameNotoriety => World.Player.Name, 
			_ => "", 
		};
	}

	private ushort GetVarHue(InfoBarVars var)
	{
		switch (var)
		{
		case InfoBarVars.HP:
		{
			float num = (float)(int)World.Player.Hits / (float)(int)World.Player.HitsMax;
			if ((double)num <= 0.25)
			{
				return 33;
			}
			if ((double)num <= 0.5)
			{
				return 48;
			}
			if ((double)num <= 0.75)
			{
				return 53;
			}
			return 1153;
		}
		case InfoBarVars.Mana:
		{
			float num = (float)(int)World.Player.Mana / (float)(int)World.Player.ManaMax;
			if ((double)num <= 0.25)
			{
				return 33;
			}
			if ((double)num <= 0.5)
			{
				return 48;
			}
			if ((double)num <= 0.75)
			{
				return 53;
			}
			return 1153;
		}
		case InfoBarVars.Stamina:
		{
			float num = (float)(int)World.Player.Stamina / (float)(int)World.Player.StaminaMax;
			if ((double)num <= 0.25)
			{
				return 33;
			}
			if ((double)num <= 0.5)
			{
				return 48;
			}
			if ((double)num <= 0.75)
			{
				return 53;
			}
			return 1153;
		}
		case InfoBarVars.Gewicht:
		{
			float num = (float)(int)World.Player.Weight / (float)(int)World.Player.WeightMax;
			if (num >= 1f)
			{
				return 33;
			}
			if ((double)num >= 0.75)
			{
				return 48;
			}
			if ((double)num >= 0.5)
			{
				return 53;
			}
			return 1153;
		}
		case InfoBarVars.NameNotoriety:
			return Notoriety.GetHue(World.Player.NotorietyFlag);
		default:
			return 1153;
		}
	}
}
