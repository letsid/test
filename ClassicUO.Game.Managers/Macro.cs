using System;
using System.Linq;
using System.Xml;
using SDL2;

namespace ClassicUO.Game.Managers;

internal class Macro : LinkedObject, IEquatable<Macro>
{
	public string Name { get; set; }

	public SDL.SDL_Keycode Key { get; set; }

	public bool Alt { get; set; }

	public bool Ctrl { get; set; }

	public bool Shift { get; set; }

	public Macro(string name, SDL.SDL_Keycode key, bool alt, bool ctrl, bool shift)
		: this(name)
	{
		Key = key;
		Alt = alt;
		Ctrl = ctrl;
		Shift = shift;
	}

	public Macro(string name)
	{
		Name = name;
	}

	public bool Equals(Macro other)
	{
		if (other == null)
		{
			return false;
		}
		if (Key == other.Key && Alt == other.Alt && Ctrl == other.Ctrl && Shift == other.Shift)
		{
			return Name == other.Name;
		}
		return false;
	}

	public void Save(XmlTextWriter writer)
	{
		writer.WriteStartElement("macro");
		writer.WriteAttributeString("name", Name);
		writer.WriteAttributeString("key", ((int)Key).ToString());
		writer.WriteAttributeString("alt", Alt.ToString());
		writer.WriteAttributeString("ctrl", Ctrl.ToString());
		writer.WriteAttributeString("shift", Shift.ToString());
		writer.WriteStartElement("actions");
		for (MacroObject macroObject = (MacroObject)Items; macroObject != null; macroObject = (MacroObject)macroObject.Next)
		{
			writer.WriteStartElement("action");
			writer.WriteAttributeString("code", macroObject.Code.ToString());
			writer.WriteAttributeString("subcode", macroObject.SubCode.ToString());
			writer.WriteAttributeString("submenutype", macroObject.SubMenuType.ToString());
			if (macroObject.HasString())
			{
				writer.WriteAttributeString("text", ((MacroObjectString)macroObject).Text);
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		writer.WriteEndElement();
	}

	public void Load(XmlElement xml)
	{
		if (xml == null)
		{
			return;
		}
		Key = (SDL.SDL_Keycode)int.Parse(xml.GetAttribute("key"));
		Alt = bool.Parse(xml.GetAttribute("alt"));
		Ctrl = bool.Parse(xml.GetAttribute("ctrl"));
		Shift = bool.Parse(xml.GetAttribute("shift"));
		XmlElement xmlElement = xml["actions"];
		if (xmlElement == null)
		{
			return;
		}
		foreach (XmlElement item in xmlElement.GetElementsByTagName("action"))
		{
			string codeString;
			if (int.TryParse(item.GetAttribute("code"), out var result))
			{
				MacroTypeOld macroTypeOld = (MacroTypeOld)result;
				codeString = Enum.GetName(typeof(MacroTypeOld), macroTypeOld);
			}
			else
			{
				codeString = item.GetAttribute("code");
			}
			string[] names = Enum.GetNames(typeof(MacroType));
			if (!names.Any((string m) => m.Equals(codeString)))
			{
				break;
			}
			MacroType macroType = (MacroType)Array.FindIndex(names, (string m) => m.Equals(codeString));
			if (int.TryParse(item.GetAttribute("subcode"), out var result2))
			{
				MacroSubtypeOld macroSubtypeOld = (MacroSubtypeOld)result2;
				codeString = Enum.GetName(typeof(MacroSubtypeOld), macroSubtypeOld);
			}
			else
			{
				codeString = item.GetAttribute("subcode");
			}
			string[] names2 = Enum.GetNames(typeof(MacroSubType));
			if (!names2.Any((string m) => m.Equals(codeString)))
			{
				break;
			}
			MacroSubType macroSubType = (MacroSubType)Array.FindIndex(names2, (string m) => m.Equals(codeString));
			if (macroType == MacroType.INVALID)
			{
				macroType = MacroType.Walk;
				switch ((int)macroSubType)
				{
				case 211:
					macroSubType = MacroSubType.NW;
					break;
				case 214:
					macroSubType = MacroSubType.SW;
					break;
				case 213:
					macroSubType = MacroSubType.SE;
					break;
				case 212:
					macroSubType = MacroSubType.NE;
					break;
				}
			}
			sbyte subMenuType = sbyte.Parse(item.GetAttribute("submenutype"));
			MacroObject macroObject = ((!item.HasAttribute("text")) ? new MacroObject(macroType, macroSubType) : new MacroObjectString(macroType, macroSubType, item.GetAttribute("text")));
			macroObject.SubMenuType = subMenuType;
			PushToBack(macroObject);
		}
	}

	public static MacroObject Create(MacroType code)
	{
		switch (code)
		{
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
		case MacroType.Useitem:
			return new MacroObjectString(code, MacroSubType.MSC_NONE);
		default:
			return new MacroObject(code, MacroSubType.MSC_NONE);
		}
	}

	public static Macro CreateEmptyMacro(string name)
	{
		Macro macro = new Macro(name, SDL.SDL_Keycode.SDLK_UNKNOWN, alt: false, ctrl: false, shift: false);
		MacroObject item = new MacroObject(MacroType.None, MacroSubType.MSC_NONE);
		macro.PushToBack(item);
		return macro;
	}

	public static Macro CreateFastMacro(string name, MacroType type, MacroSubType sub)
	{
		Macro macro = new Macro(name, SDL.SDL_Keycode.SDLK_UNKNOWN, alt: false, ctrl: false, shift: false);
		MacroObject item = new MacroObject(type, sub);
		macro.PushToBack(item);
		return macro;
	}

	public static void GetBoundByCode(MacroType code, ref int count, ref int offset)
	{
		switch (code)
		{
		case MacroType.Walk:
			offset = 1;
			count = 8;
			break;
		case MacroType.Open:
		case MacroType.Close:
		case MacroType.Minimize:
		case MacroType.Maximize:
			offset = 9;
			count = 24;
			break;
		case MacroType.UseSkill:
			offset = 33;
			count = 28;
			break;
		case MacroType.ArmDisarm:
			offset = 61;
			count = 2;
			break;
		case MacroType.InvokeVirtue:
			offset = 63;
			count = 3;
			break;
		case MacroType.CastSpell:
			offset = 66;
			count = 143;
			break;
		case MacroType.SelectNext:
		case MacroType.SelectPrevious:
		case MacroType.SelectNearest:
			offset = 209;
			count = 6;
			break;
		case MacroType.SelectNearestToCursor:
			offset = 209;
			count = 6;
			break;
		case MacroType.Zoom:
			offset = 215;
			count = 3;
			break;
		case MacroType.Useitem:
			offset = 218;
			count = 3;
			break;
		case MacroType.UsePotion:
			offset = 226;
			count = 8;
			break;
		}
	}
}
