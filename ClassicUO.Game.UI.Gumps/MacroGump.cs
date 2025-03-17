using ClassicUO.Configuration;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps;

internal sealed class MacroGump : Gump
{
	public MacroGump(string name)
		: base(0u, 0u)
	{
		CanMove = true;
		base.CanCloseWithRightClick = true;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl();
		alphaBlendControl.Width = 260;
		alphaBlendControl.Height = 200;
		alphaBlendControl.X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 125;
		alphaBlendControl.Y = 150;
		alphaBlendControl.Alpha = 0.8f;
		AlphaBlendControl alphaBlendControl2 = alphaBlendControl;
		Label label = new Label("Edit macro: " + name, isunicode: true, 15);
		label.X = ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 105;
		label.Y = alphaBlendControl2.Y + 2;
		Label c = label;
		Add(alphaBlendControl2);
		Add(c);
		MacroControl macroControl = new MacroControl(name, isFastAssign: true);
		macroControl.X = alphaBlendControl2.X + 20;
		macroControl.Y = alphaBlendControl2.Y + 20;
		Add(macroControl);
		SetInScreen();
	}
}
