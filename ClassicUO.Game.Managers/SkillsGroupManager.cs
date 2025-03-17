using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.IO;
using ClassicUO.IO.Resources;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers;

internal static class SkillsGroupManager
{
	public static readonly List<SkillsGroup> Groups = new List<SkillsGroup>();

	public static void Add(SkillsGroup g)
	{
		Groups.Add(g);
	}

	public static bool Remove(SkillsGroup g)
	{
		if (Groups[0] == g)
		{
			MessageBoxGump messageBoxGump = new MessageBoxGump(200, 125, ResGeneral.CannotDeleteThisGroup, null, hasBackground: false, MessageButtonType.OK, 902);
			messageBoxGump.X = ProfileManager.CurrentProfile.GameWindowPosition.X + ProfileManager.CurrentProfile.GameWindowSize.X / 2 - 100;
			messageBoxGump.Y = ProfileManager.CurrentProfile.GameWindowPosition.Y + ProfileManager.CurrentProfile.GameWindowSize.Y / 2 - 62;
			UIManager.Add(messageBoxGump);
			return false;
		}
		Groups.Remove(g);
		g.TransferTo(Groups[0]);
		return true;
	}

	public static void Load()
	{
		Groups.Clear();
		string text = Path.Combine(ProfileManager.ProfilePath, "skillsgroups.xml");
		if (!File.Exists(text))
		{
			Log.Trace("No skillsgroups.xml file. Creating a default file.");
			MakeDefault();
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(text);
		}
		catch (Exception ex)
		{
			MakeDefault();
			Log.Error(ex.ToString());
			return;
		}
		XmlElement xmlElement = xmlDocument["skillsgroups"];
		if (xmlElement == null)
		{
			return;
		}
		foreach (XmlElement item in xmlElement.GetElementsByTagName("group"))
		{
			SkillsGroup skillsGroup = new SkillsGroup();
			skillsGroup.Name = item.GetAttribute("name");
			XmlElement xmlElement3 = item["skillids"];
			if (xmlElement3 != null)
			{
				foreach (XmlElement item2 in xmlElement3.GetElementsByTagName("skill"))
				{
					skillsGroup.Add(byte.Parse(item2.GetAttribute("id")));
				}
			}
			skillsGroup.Sort();
			Add(skillsGroup);
		}
	}

	public static void Save()
	{
		using XmlTextWriter xmlTextWriter = new XmlTextWriter(Path.Combine(ProfileManager.ProfilePath, "skillsgroups.xml"), Encoding.UTF8)
		{
			Formatting = Formatting.Indented,
			IndentChar = '\t',
			Indentation = 1
		};
		xmlTextWriter.WriteStartDocument(standalone: true);
		xmlTextWriter.WriteStartElement("skillsgroups");
		foreach (SkillsGroup group in Groups)
		{
			group.Save(xmlTextWriter);
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.WriteEndDocument();
	}

	public static void MakeDefault()
	{
		Groups.Clear();
		if (!LoadMULFile(UOFileManager.GetUOFilePath("skillgrp.mul")))
		{
			MakeDefaultMiscellaneous();
			MakeDefaultCombat();
			MakeDefaultTradeSkills();
			MakeDefaultMagic();
			MakeDefaultWilderness();
			MakeDefaultThieving();
			MakeDefaultBard();
		}
		foreach (SkillsGroup group in Groups)
		{
			group.Sort();
		}
		Save();
	}

	private static void MakeDefaultMiscellaneous()
	{
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Miscellaneous;
		skillsGroup.Add(4);
		skillsGroup.Add(6);
		skillsGroup.Add(10);
		skillsGroup.Add(12);
		skillsGroup.Add(19);
		skillsGroup.Add(3);
		skillsGroup.Add(36);
		Add(skillsGroup);
	}

	private static void MakeDefaultCombat()
	{
		int skillsCount = SkillsLoader.Instance.SkillsCount;
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Combat;
		skillsGroup.Add(1);
		skillsGroup.Add(31);
		skillsGroup.Add(42);
		skillsGroup.Add(17);
		skillsGroup.Add(41);
		skillsGroup.Add(5);
		skillsGroup.Add(40);
		skillsGroup.Add(27);
		if (skillsCount > 57)
		{
			skillsGroup.Add(57);
		}
		skillsGroup.Add(43);
		if (skillsCount > 50)
		{
			skillsGroup.Add(50);
		}
		if (skillsCount > 51)
		{
			skillsGroup.Add(51);
		}
		if (skillsCount > 52)
		{
			skillsGroup.Add(52);
		}
		if (skillsCount > 53)
		{
			skillsGroup.Add(53);
		}
		Add(skillsGroup);
	}

	private static void MakeDefaultTradeSkills()
	{
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.TradeSkills;
		skillsGroup.Add(0);
		skillsGroup.Add(7);
		skillsGroup.Add(8);
		skillsGroup.Add(11);
		skillsGroup.Add(13);
		skillsGroup.Add(23);
		skillsGroup.Add(44);
		skillsGroup.Add(45);
		skillsGroup.Add(34);
		skillsGroup.Add(37);
		Add(skillsGroup);
	}

	private static void MakeDefaultMagic()
	{
		int skillsCount = SkillsLoader.Instance.SkillsCount;
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Magic;
		skillsGroup.Add(16);
		if (skillsCount > 56)
		{
			skillsGroup.Add(56);
		}
		skillsGroup.Add(25);
		skillsGroup.Add(46);
		if (skillsCount > 55)
		{
			skillsGroup.Add(55);
		}
		skillsGroup.Add(26);
		if (skillsCount > 54)
		{
			skillsGroup.Add(54);
		}
		skillsGroup.Add(32);
		if (skillsCount > 49)
		{
			skillsGroup.Add(49);
		}
		Add(skillsGroup);
	}

	private static void MakeDefaultWilderness()
	{
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Wilderness;
		skillsGroup.Add(2);
		skillsGroup.Add(35);
		skillsGroup.Add(18);
		skillsGroup.Add(20);
		skillsGroup.Add(38);
		skillsGroup.Add(39);
		Add(skillsGroup);
	}

	private static void MakeDefaultThieving()
	{
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Thieving;
		skillsGroup.Add(14);
		skillsGroup.Add(21);
		skillsGroup.Add(24);
		skillsGroup.Add(30);
		skillsGroup.Add(48);
		skillsGroup.Add(28);
		skillsGroup.Add(33);
		skillsGroup.Add(47);
		Add(skillsGroup);
	}

	private static void MakeDefaultBard()
	{
		SkillsGroup skillsGroup = new SkillsGroup();
		skillsGroup.Name = ResGeneral.Bard;
		skillsGroup.Add(15);
		skillsGroup.Add(29);
		skillsGroup.Add(9);
		skillsGroup.Add(22);
		Add(skillsGroup);
	}

	private static bool LoadMULFile(string path)
	{
		FileInfo fileInfo = new FileInfo(path);
		if (!fileInfo.Exists)
		{
			return false;
		}
		try
		{
			byte b = 0;
			bool flag = false;
			using BinaryReader binaryReader = new BinaryReader(File.OpenRead(fileInfo.FullName));
			int num = 4;
			int num2 = 17;
			int num3 = binaryReader.ReadInt32();
			if (num3 == -1)
			{
				flag = true;
				num3 = binaryReader.ReadInt32();
				num *= 2;
				num2 *= 2;
			}
			StringBuilder stringBuilder = new StringBuilder(17);
			SkillsGroup skillsGroup = new SkillsGroup();
			skillsGroup.Name = ResGeneral.Miscellaneous;
			SkillsGroup[] array = new SkillsGroup[num3];
			array[0] = skillsGroup;
			for (int i = 0; i < num3 - 1; i++)
			{
				binaryReader.BaseStream.Seek(num + i * num2, SeekOrigin.Begin);
				if (flag)
				{
					short num4;
					while ((num4 = binaryReader.ReadInt16()) != 0)
					{
						stringBuilder.Append((char)num4);
					}
				}
				else
				{
					while (true)
					{
						byte num5 = binaryReader.ReadByte();
						short num4 = num5;
						if (num5 == 0)
						{
							break;
						}
						stringBuilder.Append((char)num4);
					}
				}
				array[i + 1] = new SkillsGroup
				{
					Name = stringBuilder.ToString()
				};
				stringBuilder.Clear();
			}
			binaryReader.BaseStream.Seek(num + (num3 - 1) * num2, SeekOrigin.Begin);
			while (binaryReader.BaseStream.Length != binaryReader.BaseStream.Position)
			{
				int num6 = binaryReader.ReadInt32();
				if (num6 < array.Length && b < SkillsLoader.Instance.SkillsCount)
				{
					array[num6].Add(b++);
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				Add(array[j]);
			}
		}
		catch (Exception arg)
		{
			Log.Error($"Error while reading skillgrp.mul, using CUO defaults! exception given is: {arg}");
			return false;
		}
		return Groups.Count != 0;
	}
}
