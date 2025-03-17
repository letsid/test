using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal class InfoBarManager
{
	private readonly List<InfoBarItem> infoBarItems;

	public InfoBarManager()
	{
		infoBarItems = new List<InfoBarItem>();
		if (ProfileManager.CurrentProfile.InfoBarItems != null)
		{
			infoBarItems.AddRange(ProfileManager.CurrentProfile.InfoBarItems);
			ProfileManager.CurrentProfile.InfoBarItems = null;
			Save();
		}
	}

	public List<InfoBarItem> GetInfoBars()
	{
		return infoBarItems;
	}

	public static string[] GetVars()
	{
		if (!CUOEnviroment.IsOutlands)
		{
			return Enum.GetNames(typeof(InfoBarVars));
		}
		return Enum.GetNames(typeof(InfoBarVarsOutlands));
	}

	public void AddItem(InfoBarItem ibi)
	{
		infoBarItems.Add(ibi);
	}

	public void RemoveItem(InfoBarItem item)
	{
		infoBarItems.Remove(item);
	}

	public void Clear()
	{
		infoBarItems.Clear();
	}

	public void Save()
	{
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(ProfileManager.ProfilePath, "infobar.xml"), Encoding.UTF8)
		{
			Formatting = Formatting.Indented,
			IndentChar = '\t',
			Indentation = 1
		};
		xmlTextWriter.WriteStartDocument(standalone: true);
		xmlTextWriter.WriteStartElement("infos");
		foreach (InfoBarItem infoBarItem in infoBarItems)
		{
			infoBarItem.Save(xmlTextWriter);
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
	}

	public void Load()
	{
		string text = Path.Combine(ProfileManager.ProfilePath, "infobar.xml");
		if (!File.Exists(text))
		{
			CreateDefault();
			Save();
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(text);
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
			return;
		}
		infoBarItems.Clear();
		XmlElement xmlElement = xmlDocument["infos"];
		int num = GetVars().Length;
		if (xmlElement == null)
		{
			return;
		}
		foreach (XmlElement item in xmlElement.GetElementsByTagName("info"))
		{
			InfoBarItem infoBarItem = new InfoBarItem(item);
			if ((int)infoBarItem.var < num)
			{
				infoBarItems.Add(infoBarItem);
			}
		}
	}

	public void CreateDefault()
	{
		infoBarItems.Clear();
		infoBarItems.Add(new InfoBarItem("", InfoBarVars.NameNotoriety, 978));
		infoBarItems.Add(new InfoBarItem(ResGeneral.Hits, InfoBarVars.HP, 438));
		infoBarItems.Add(new InfoBarItem(ResGeneral.Mana, InfoBarVars.Mana, 493));
		infoBarItems.Add(new InfoBarItem(ResGeneral.Stam, InfoBarVars.Stamina, 558));
		infoBarItems.Add(new InfoBarItem(ResGeneral.Weight, InfoBarVars.Gewicht, 978));
	}
}
