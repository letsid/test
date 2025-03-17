using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal static class IgnoreManager
{
	public static HashSet<string> IgnoredCharsList = new HashSet<string>();

	public static void Initialize()
	{
		ReadIgnoreList();
	}

	public static void AddIgnoredTarget(Entity entity)
	{
		if (entity is Mobile { IsHuman: not false, IsYellowHits: false } mobile && mobile.Serial != World.Player.Serial)
		{
			string name = mobile.Name;
			if (IgnoredCharsList.Contains(name))
			{
				GameActions.Print(string.Format(ResGumps.AddToIgnoreListExist, name), 946, MessageType.Regular, 3);
				return;
			}
			IgnoredCharsList.Add(name);
			UIManager.GetGump<IgnoreManagerGump>(null)?.Redraw();
			GameActions.Print(string.Format(ResGumps.AddToIgnoreListSuccess, name), 946, MessageType.Regular, 3);
		}
		else
		{
			GameActions.Print(string.Format(ResGumps.AddToIgnoreListNotMobile), 946, MessageType.Regular, 3);
		}
	}

	public static void RemoveIgnoredTarget(string charName)
	{
		if (IgnoredCharsList.Contains(charName))
		{
			IgnoredCharsList.Remove(charName);
		}
	}

	private static void ReadIgnoreList()
	{
		HashSet<string> hashSet = new HashSet<string>();
		string text = Path.Combine(ProfileManager.ProfilePath, "ignore_list.xml");
		if (!File.Exists(text))
		{
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
		}
		XmlElement xmlElement = xmlDocument["ignore"];
		if (xmlElement != null)
		{
			foreach (XmlElement childNode in xmlElement.ChildNodes)
			{
				if (!(childNode.Name != "info"))
				{
					string attribute = childNode.GetAttribute("charname");
					hashSet.Add(attribute);
				}
			}
		}
		IgnoredCharsList = hashSet;
	}

	public static void SaveIgnoreList()
	{
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(ProfileManager.ProfilePath, "ignore_list.xml"), Encoding.UTF8)
		{
			Formatting = Formatting.Indented,
			IndentChar = '\t',
			Indentation = 1
		};
		xmlTextWriter.WriteStartDocument(standalone: true);
		xmlTextWriter.WriteStartElement("ignore");
		foreach (string ignoredChars in IgnoredCharsList)
		{
			xmlTextWriter.WriteStartElement("info");
			xmlTextWriter.WriteAttributeString("charname", ignoredChars);
			xmlTextWriter.WriteEndElement();
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
	}
}
