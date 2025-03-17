using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClassicUO.Game.UI.Gumps.CharCreation;
using ClassicUO.Utility;

namespace ClassicUO.IO.Resources;

internal class ProfessionLoader : UOFileLoader
{
	internal enum PROF_TYPE
	{
		NO_PROF,
		CATEGORY,
		PROFESSION
	}

	private enum PM_CODE
	{
		BEGIN = 1,
		NAME,
		TRUENAME,
		DESC,
		TOPLEVEL,
		GUMP,
		TYPE,
		CHILDREN,
		SKILL,
		STAT,
		STR,
		INT,
		DEX,
		END,
		TRUE,
		CATEGORY,
		NAME_CLILOC_ID,
		DESCRIPTION_CLILOC_ID
	}

	private static ProfessionLoader _instance;

	private readonly string[] _Keys = new string[18]
	{
		"begin", "name", "truename", "desc", "toplevel", "gump", "type", "children", "skill", "stat",
		"str", "int", "dex", "end", "true", "category", "nameid", "descid"
	};

	public static ProfessionLoader Instance => _instance ?? (_instance = new ProfessionLoader());

	public Dictionary<ProfessionInfo, List<ProfessionInfo>> Professions { get; } = new Dictionary<ProfessionInfo, List<ProfessionInfo>>();

	private ProfessionLoader()
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			FileInfo fileInfo = new FileInfo(UOFileManager.GetUOFilePath("Prof.txt"));
			if (fileInfo.Exists)
			{
				if (fileInfo.Length > 1048576)
				{
					throw new InternalBufferOverflowException($"{fileInfo.FullName} exceeds the maximum 1Megabyte allowed size for a string text file, please, check that the file is correct and not corrupted -> {fileInfo.Length} file size");
				}
				TextFileParser textFileParser = new TextFileParser(File.ReadAllText(fileInfo.FullName), new char[3] { ' ', '\t', ',' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
				while (!textFileParser.IsEOF())
				{
					List<string> list = textFileParser.ReadTokens();
					if (list.Count > 0 && list[0].ToLower() == "begin" && !ParseFilePart(textFileParser))
					{
						break;
					}
				}
			}
			Professions[new ProfessionInfo
			{
				Name = "Advanced",
				Localization = 1061176,
				Description = 1061226,
				Graphic = 5545,
				TopLevel = true,
				Type = PROF_TYPE.PROFESSION,
				DescriptionIndex = -1,
				TrueName = "advanced"
			}] = null;
			foreach (KeyValuePair<ProfessionInfo, List<ProfessionInfo>> profession in Professions)
			{
				profession.Key.Childrens = null;
				if (profession.Value != null)
				{
					foreach (ProfessionInfo item in profession.Value)
					{
						item.Childrens = null;
					}
				}
			}
		});
	}

	private int GetKeyCode(string key)
	{
		key = key.ToLowerInvariant();
		int num = 0;
		for (int i = 0; i < _Keys.Length; i++)
		{
			if (num > 0)
			{
				break;
			}
			if (key == _Keys[i])
			{
				num = i + 1;
			}
		}
		return num;
	}

	private bool ParseFilePart(TextFileParser file)
	{
		List<string> list = new List<string>();
		PROF_TYPE pROF_TYPE = PROF_TYPE.NO_PROF;
		string text = string.Empty;
		string text2 = string.Empty;
		int result = 0;
		int result2 = 0;
		int result3 = 0;
		ushort result4 = 0;
		bool flag = false;
		int[,] array = new int[4, 2]
		{
			{ 255, 0 },
			{ 255, 0 },
			{ 255, 0 },
			{ 255, 0 }
		};
		int[] array2 = new int[3];
		bool flag2 = false;
		while (!file.IsEOF() && !flag2)
		{
			List<string> list2 = file.ReadTokens();
			if (list2.Count < 1)
			{
				continue;
			}
			switch ((PM_CODE)GetKeyCode(list2[0]))
			{
			case PM_CODE.BEGIN:
			case PM_CODE.END:
				flag2 = true;
				break;
			case PM_CODE.NAME:
				text = list2[1];
				break;
			case PM_CODE.TRUENAME:
				text2 = list2[1];
				break;
			case PM_CODE.DESC:
				int.TryParse(list2[1], out result3);
				break;
			case PM_CODE.TOPLEVEL:
				flag = GetKeyCode(list2[1]) == 15;
				break;
			case PM_CODE.GUMP:
				ushort.TryParse(list2[1], out result4);
				break;
			case PM_CODE.TYPE:
				pROF_TYPE = ((GetKeyCode(list2[1]) == 16) ? PROF_TYPE.CATEGORY : PROF_TYPE.PROFESSION);
				break;
			case PM_CODE.CHILDREN:
			{
				for (int k = 1; k < list2.Count; k++)
				{
					list.Add(list2[k]);
				}
				break;
			}
			case PM_CODE.SKILL:
			{
				if (list2.Count <= 2)
				{
					break;
				}
				int num = 0;
				int i = 0;
				for (int length = array.GetLength(0); i < length; i++)
				{
					if (array[i, 0] == 255)
					{
						num = i;
						break;
					}
				}
				for (int j = 0; j < SkillsLoader.Instance.SkillsCount; j++)
				{
					SkillEntry skillEntry = SkillsLoader.Instance.Skills[j];
					if (list2[1] == skillEntry.Name || ((SkillEntry.HardCodedName)skillEntry.Index/*cast due to .constrained prefix*/).ToString().ToLower() == list2[1].ToLower())
					{
						array[num, 0] = j;
						int.TryParse(list2[2], out array[num, 1]);
						break;
					}
				}
				break;
			}
			case PM_CODE.STAT:
				if (list2.Count > 2)
				{
					int keyCode = GetKeyCode(list2[1]);
					int.TryParse(list2[2], out var result5);
					switch (keyCode)
					{
					case 11:
						array2[0] = result5;
						break;
					case 12:
						array2[1] = result5;
						break;
					case 13:
						array2[2] = result5;
						break;
					}
				}
				break;
			case PM_CODE.NAME_CLILOC_ID:
				int.TryParse(list2[1], out result);
				text = ClilocLoader.Instance.GetString(result, camelcase: true, text);
				break;
			case PM_CODE.DESCRIPTION_CLILOC_ID:
				int.TryParse(list2[1], out result2);
				break;
			}
		}
		ProfessionInfo professionInfo = null;
		List<ProfessionInfo> value = null;
		switch (pROF_TYPE)
		{
		case PROF_TYPE.CATEGORY:
			professionInfo = new ProfessionInfo
			{
				Childrens = list
			};
			value = new List<ProfessionInfo>();
			break;
		case PROF_TYPE.PROFESSION:
			professionInfo = new ProfessionInfo
			{
				StatsVal = array2,
				SkillDefVal = array
			};
			break;
		}
		bool result6 = pROF_TYPE != PROF_TYPE.NO_PROF;
		if (professionInfo != null)
		{
			professionInfo.Localization = result;
			professionInfo.Description = result2;
			professionInfo.Name = text;
			professionInfo.TrueName = text2;
			professionInfo.DescriptionIndex = result3;
			professionInfo.TopLevel = flag;
			professionInfo.Graphic = result4;
			professionInfo.Type = pROF_TYPE;
			if (flag)
			{
				Professions[professionInfo] = value;
			}
			else
			{
				foreach (KeyValuePair<ProfessionInfo, List<ProfessionInfo>> profession in Professions)
				{
					if (profession.Key.Childrens != null && profession.Value != null && profession.Key.Childrens.Contains(text2))
					{
						Professions[profession.Key].Add(professionInfo);
						result6 = true;
						break;
					}
				}
			}
		}
		return result6;
	}
}
