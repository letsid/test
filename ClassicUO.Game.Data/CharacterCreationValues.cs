using System.Linq;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.Data;

internal class CharacterCreationValues
{
	internal class ComboContent
	{
		private readonly int[] _ids;

		public string[] Labels { get; }

		public ComboContent(int[] labels, int[] ids)
		{
			_ids = ids;
			Labels = labels.Select((int o) => ClilocLoader.Instance.GetString(o)).ToArray();
		}

		public int GetGraphic(int index)
		{
			return _ids[index];
		}
	}

	private static readonly ushort[] HumanSkinTone = new ushort[64]
	{
		1001, 1009, 1017, 1025, 1033, 1041, 1049, 1057, 1002, 1010,
		1018, 1026, 1034, 1042, 1050, 1057, 1003, 1011, 1019, 1027,
		1035, 1043, 1051, 1057, 1004, 1012, 1020, 1028, 1036, 1044,
		1052, 1057, 1005, 1013, 1021, 1029, 1037, 1045, 1053, 1057,
		1006, 1014, 1022, 1030, 1038, 1046, 1054, 1057, 1007, 1015,
		1023, 1031, 1039, 1047, 1055, 1057, 1008, 1016, 1024, 1032,
		1040, 1048, 1056, 1057
	};

	private static readonly ushort[] ElfSkinTone = new ushort[32]
	{
		1245, 1899, 2100, 1071, 588, 589, 590, 190, 1190, 864,
		884, 870, 999, 989, 850, 2306, 1900, 899, 1400, 1000,
		883, 904, 900, 885, 1342, 896, 897, 898, 1898, 996,
		1308, 997
	};

	private static readonly ushort[] GargoyleSkinTone = new ushort[28]
	{
		1754, 1755, 1756, 1757, 1758, 1759, 1760, 1761, 1762, 1763,
		1764, 1765, 1766, 1767, 1768, 1769, 1770, 1771, 1772, 1773,
		1774, 1775, 1776, 1777, 1778, 1754, 1755, 1756
	};

	private static readonly ushort[] HumanHairColor = new ushort[48]
	{
		1101, 1109, 1117, 1125, 1133, 1141, 1102, 1110, 1118, 1126,
		1134, 1142, 1103, 1111, 1119, 1127, 1135, 1143, 1104, 1112,
		1120, 1128, 1136, 1144, 1105, 1113, 1121, 1129, 1137, 1145,
		1106, 1114, 1122, 1130, 1138, 1146, 1107, 1115, 1123, 1131,
		1139, 1147, 1108, 1116, 1124, 1132, 1140, 1148
	};

	private static readonly ushort[] ElfHairColor = new ushort[54]
	{
		51, 52, 53, 54, 55, 56, 256, 1719, 518, 528,
		619, 706, 712, 483, 568, 872, 1436, 2130, 141, 142,
		143, 144, 145, 344, 345, 346, 347, 348, 349, 444,
		1828, 87, 295, 302, 498, 592, 796, 797, 798, 799,
		800, 801, 802, 803, 804, 805, 901, 902, 903, 904,
		905, 901, 902, 903
	};

	private static readonly ushort[] GargoyleHairColor = new ushort[18]
	{
		1800, 1802, 1804, 1806, 1808, 1890, 1892, 1895, 1898, 1778,
		1776, 1774, 1763, 1761, 1759, 1800, 1802, 1804
	};

	private static readonly int[] HumanHairLabels = new int[10] { 3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000348, 3000349 };

	private static readonly int[] HumanHairGraphics = new int[10] { 0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8264, 8265 };

	private static readonly int[] HumanFacialLabels = new int[8] { 3000340, 3000351, 3000352, 3000353, 3000354, 1011060, 1011061, 3000357 };

	private static readonly int[] HumanFacialGraphics = new int[8] { 0, 8256, 8254, 8255, 8257, 8267, 8268, 8269 };

	private static readonly int[] HumanFemaleHairLabels = new int[10] { 3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000349, 3000350 };

	private static readonly int[] HumanFemaleHairGraphics = new int[10] { 0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8265, 8262 };

	private static readonly int[] ElfHairLabels = new int[9] { 3000340, 1074385, 1074386, 1074387, 1074388, 1074390, 1074391, 1074392, 1074394 };

	private static readonly int[] ElfHairGraphics = new int[9] { 0, 12223, 12224, 12225, 12226, 12237, 12238, 12239, 12241 };

	private static readonly int[] ElfFemaleHairLabels = new int[9] { 3000340, 1074386, 1074387, 1074388, 1074389, 1074391, 1074392, 1074393, 1074394 };

	private static readonly int[] ElfFemaleHairGraphics = new int[9] { 0, 12224, 12225, 12226, 12236, 12238, 12239, 12240, 12241 };

	private static readonly int[] GargoyleHairLabels = new int[9] { 3000340, 1112310, 1112311, 1112312, 1112313, 1112314, 1112315, 1112316, 1112317 };

	private static readonly int[] GargoyleHairGraphics = new int[9] { 0, 16984, 16985, 16986, 16987, 16988, 16989, 16990, 16991 };

	private static readonly int[] GargoyleFacialLabels = new int[5] { 3000340, 1112310, 1112311, 1112312, 1112313 };

	private static readonly int[] GargoyleFacialGraphics = new int[5] { 0, 17069, 17070, 17071, 17072 };

	private static readonly int[] GargoyleFemaleHairLabels = new int[9] { 3000340, 1112310, 1112311, 1112312, 1112313, 1112314, 1112315, 1112316, 1112317 };

	private static readonly int[] GargoyleFemaleHairGraphics = new int[9] { 0, 16993, 16994, 17011, 17012, 17013, 17066, 17067, 17073 };

	public static ushort[] GetSkinPallet(RaceType race)
	{
		return race switch
		{
			RaceType.HUMAN => HumanSkinTone, 
			RaceType.ELF => ElfSkinTone, 
			RaceType.GARGOYLE => GargoyleSkinTone, 
			_ => new ushort[0], 
		};
	}

	public static ushort[] GetHairPallet(RaceType race)
	{
		return race switch
		{
			RaceType.HUMAN => HumanHairColor, 
			RaceType.ELF => ElfHairColor, 
			RaceType.GARGOYLE => GargoyleHairColor, 
			_ => new ushort[0], 
		};
	}

	public static ComboContent GetHairComboContent(bool isFemale, RaceType race)
	{
		switch (race)
		{
		case RaceType.HUMAN:
			if (isFemale)
			{
				return new ComboContent(HumanFemaleHairLabels, HumanFemaleHairGraphics);
			}
			return new ComboContent(HumanHairLabels, HumanHairGraphics);
		case RaceType.ELF:
			if (isFemale)
			{
				return new ComboContent(ElfFemaleHairLabels, ElfFemaleHairGraphics);
			}
			return new ComboContent(ElfHairLabels, ElfHairGraphics);
		case RaceType.GARGOYLE:
			if (isFemale)
			{
				return new ComboContent(GargoyleFemaleHairLabels, GargoyleFemaleHairGraphics);
			}
			return new ComboContent(GargoyleHairLabels, GargoyleHairGraphics);
		default:
			return new ComboContent(new int[0], new int[0]);
		}
	}

	public static ComboContent GetFacialHairComboContent(RaceType race)
	{
		return race switch
		{
			RaceType.HUMAN => new ComboContent(HumanFacialLabels, HumanFacialGraphics), 
			RaceType.GARGOYLE => new ComboContent(GargoyleFacialLabels, GargoyleFacialGraphics), 
			_ => new ComboContent(new int[0], new int[0]), 
		};
	}
}
