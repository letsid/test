using System.Collections.Generic;
using System.IO;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data;

internal static class BuffTable
{
	private static ushort[] _table;

	private static ushort[] _defaultTable = new ushort[189]
	{
		30028, 30026, 0, 0, 30046, 30025, 30033, 30038, 30010, 30029,
		30030, 30053, 30011, 30019, 30020, 30022, 30044, 30047, 30054, 30036,
		30016, 30056, 30031, 30032, 30035, 30014, 30045, 30051, 30050, 30015,
		30041, 30039, 30027, 30013, 30049, 30040, 30043, 30048, 30017, 30021,
		30034, 30057, 30024, 30042, 30012, 30023, 30055, 30018, 30090, 30091,
		30092, 30093, 0, 30094, 2379, 2380, 2381, 2382, 2383, 2384,
		30014, 20497, 30096, 30097, 30098, 30099, 30100, 30101, 30102, 30104,
		30105, 30107, 30108, 30110, 30111, 30112, 30113, 30115, 30116, 30117,
		30118, 30119, 30144, 30145, 30146, 30147, 30148, 30194, 30195, 30196,
		30197, 30198, 30199, 30200, 30201, 30202, 30203, 30204, 30205, 30206,
		30207, 30208, 30209, 30210, 30211, 30212, 30213, 30214, 30215, 30216,
		30217, 30218, 30219, 30220, 30221, 30222, 30223, 30224, 30225, 30226,
		30227, 30228, 30229, 30149, 30198, 30235, 39881, 39861, 39901, 39878,
		39884, 39870, 39869, 39883, 39880, 39871, 39885, 39872, 39886, 39873,
		39879, 39874, 39863, 39882, 39862, 39864, 39865, 39866, 39867, 39868,
		39875, 39876, 39877, 39890, 39891, 39892, 39893, 39889, 39894, 39895,
		39887, 39896, 39897, 39899, 39900, 39898, 39888, 39902, 39903, 49993,
		49997, 49998, 49996, 49995, 49994, 49987, 49989, 49990, 49991, 49992,
		40158, 24033, 24031, 24035, 24037, 24036, 24038, 23889, 2385
	};

	public static ushort[] Table => _table;

	public static void Load()
	{
		string text = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		string path = Path.Combine(text, "buff.txt");
		if (File.Exists(path))
		{
			List<ushort> list = new List<ushort>();
			TextFileParser textFileParser = new TextFileParser(File.ReadAllText(path), new char[3] { ' ', '\t', ',' }, new char[2] { '#', ';' }, new char[2] { '"', '"' });
			while (!textFileParser.IsEOF())
			{
				List<string> list2 = textFileParser.ReadTokens();
				if (list2 != null && list2.Count != 0 && ushort.TryParse(list2[0], out var result))
				{
					list.Add(result);
				}
			}
			_table = list.ToArray();
		}
		else
		{
			_table = _defaultTable;
		}
	}
}
