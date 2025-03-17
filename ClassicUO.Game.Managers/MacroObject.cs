namespace ClassicUO.Game.Managers;

internal class MacroObject : LinkedObject
{
	public MacroType Code { get; set; }

	public MacroSubType SubCode { get; set; }

	public sbyte SubMenuType { get; set; }

	public MacroObject(MacroType code, MacroSubType sub)
	{
		Code = code;
		SubCode = sub;
		switch (code)
		{
		case MacroType.Walk:
		case MacroType.Open:
		case MacroType.Close:
		case MacroType.Minimize:
		case MacroType.Maximize:
		case MacroType.UseSkill:
		case MacroType.CastSpell:
		case MacroType.ArmDisarm:
		case MacroType.InvokeVirtue:
		case MacroType.SelectNext:
		case MacroType.SelectPrevious:
		case MacroType.SelectNearest:
		case MacroType.Zoom:
		case MacroType.UsePotion:
		case MacroType.SelectNearestToCursor:
		case MacroType.Useitem:
			if (sub == MacroSubType.MSC_NONE)
			{
				int count = 0;
				int offset = 0;
				Macro.GetBoundByCode(code, ref count, ref offset);
				SubCode = (MacroSubType)offset;
			}
			SubMenuType = 1;
			break;
		case MacroType.Say:
		case MacroType.Emote:
		case MacroType.Whisper:
		case MacroType.Yell:
		case MacroType.Delay:
		case MacroType.SetUpdateRange:
		case MacroType.ModifyUpdateRange:
		case MacroType.PartySay:
		case MacroType.BandageSelfKonditionell:
		case MacroType.Usetype:
		case MacroType.Usename:
			SubMenuType = 2;
			break;
		default:
			SubMenuType = 0;
			break;
		}
	}

	public virtual bool HasString()
	{
		return false;
	}
}
