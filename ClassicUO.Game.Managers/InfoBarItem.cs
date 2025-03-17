using System.Xml;

namespace ClassicUO.Game.Managers;

internal class InfoBarItem
{
	public ushort hue;

	public string label;

	public InfoBarVars var;

	public InfoBarItem(string label, InfoBarVars var, ushort labelColor)
	{
		this.label = label;
		this.var = var;
		hue = labelColor;
	}

	public InfoBarItem(XmlElement xml)
	{
		if (xml != null)
		{
			label = xml.GetAttribute("text");
			var = (InfoBarVars)int.Parse(xml.GetAttribute("var"));
			hue = ushort.Parse(xml.GetAttribute("hue"));
		}
	}

	public void Save(XmlTextWriter writer)
	{
		writer.WriteStartElement("info");
		writer.WriteAttributeString("text", label);
		int num = (int)var;
		writer.WriteAttributeString("var", num.ToString());
		writer.WriteAttributeString("hue", hue.ToString());
		writer.WriteEndElement();
	}
}
