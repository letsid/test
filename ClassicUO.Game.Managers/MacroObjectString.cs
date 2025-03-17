namespace ClassicUO.Game.Managers;

internal class MacroObjectString : MacroObject
{
	public string Text { get; set; }

	public MacroObjectString(MacroType code, MacroSubType sub, string str = "")
		: base(code, sub)
	{
		Text = str;
	}

	public override bool HasString()
	{
		return true;
	}
}
