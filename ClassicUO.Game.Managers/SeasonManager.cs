using System;
using System.IO;

namespace ClassicUO.Game.Managers;

internal static class SeasonManager
{
	private static ushort[] _springLandTile;

	private static ushort[] _summerLandTile;

	private static ushort[] _fallLandTile;

	private static ushort[] _winterLandTile;

	private static ushort[] _desolationLandTile;

	private static ushort[] _springGraphic;

	private static ushort[] _summerGraphic;

	private static ushort[] _fallGraphic;

	private static ushort[] _winterGraphic;

	private static ushort[] _desolationGraphic;

	private static readonly string _seasonsFilePath;

	private static readonly string _seasonsFile;

	static SeasonManager()
	{
		_seasonsFilePath = Path.Combine(CUOEnviroment.ExecutablePath, "Data", "Client");
		_seasonsFile = Path.Combine(_seasonsFilePath, "seasons.txt");
		LoadSeasonFile();
	}

	public static void LoadSeasonFile()
	{
		_springLandTile = new ushort[16384];
		_summerLandTile = new ushort[16384];
		_fallLandTile = new ushort[16384];
		_winterLandTile = new ushort[16384];
		_desolationLandTile = new ushort[16384];
		_springGraphic = new ushort[81920];
		_summerGraphic = new ushort[81920];
		_fallGraphic = new ushort[81920];
		_winterGraphic = new ushort[81920];
		_desolationGraphic = new ushort[81920];
		if (!File.Exists(_seasonsFile))
		{
			CreateDefaultSeasonsFile();
		}
		using StreamReader streamReader = new StreamReader(_seasonsFile);
		while (!streamReader.EndOfStream)
		{
			string text = streamReader.ReadLine();
			if (string.IsNullOrEmpty(text) || text.StartsWith("#") || text.StartsWith("//"))
			{
				continue;
			}
			string[] array = text.Split(',');
			if (array.Length < 4)
			{
				continue;
			}
			ushort num = (array[2].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ? Convert.ToUInt16(array[2], 16) : Convert.ToUInt16(array[2]));
			ushort num2 = (array[3].StartsWith("0x", StringComparison.InvariantCultureIgnoreCase) ? Convert.ToUInt16(array[3], 16) : Convert.ToUInt16(array[3]));
			bool flag = array[1].StartsWith("static", StringComparison.InvariantCultureIgnoreCase);
			switch (array[0].ToLower())
			{
			case "spring":
				if (flag)
				{
					_springGraphic[num] = num2;
				}
				else
				{
					_springLandTile[num] = num2;
				}
				break;
			case "summer":
				if (flag)
				{
					_summerGraphic[num] = num2;
				}
				else
				{
					_summerLandTile[num] = num2;
				}
				break;
			case "fall":
				if (flag)
				{
					_fallGraphic[num] = num2;
				}
				else
				{
					_fallLandTile[num] = num2;
				}
				break;
			case "winter":
				if (flag)
				{
					_winterGraphic[num] = num2;
				}
				else
				{
					_winterLandTile[num] = num2;
				}
				break;
			case "desolation":
				if (flag)
				{
					_desolationGraphic[num] = num2;
				}
				else
				{
					_desolationLandTile[num] = num2;
				}
				break;
			}
		}
	}

	public static ushort GetSeasonGraphic(Season season, ushort graphic)
	{
		switch (season)
		{
		case Season.Spring:
			if (_springGraphic[graphic] != 0)
			{
				return _springGraphic[graphic];
			}
			return graphic;
		case Season.Summer:
			if (_summerGraphic[graphic] != 0)
			{
				return _summerGraphic[graphic];
			}
			return graphic;
		case Season.Fall:
			if (_fallGraphic[graphic] != 0)
			{
				return _fallGraphic[graphic];
			}
			return graphic;
		case Season.Winter:
			if (_winterGraphic[graphic] != 0)
			{
				return _winterGraphic[graphic];
			}
			return graphic;
		case Season.Desolation:
			if (_desolationGraphic[graphic] != 0)
			{
				return _desolationGraphic[graphic];
			}
			return graphic;
		default:
			return graphic;
		}
	}

	public static ushort GetLandSeasonGraphic(Season season, ushort graphic)
	{
		switch (season)
		{
		case Season.Spring:
			if (_springLandTile[graphic] != 0)
			{
				return _springLandTile[graphic];
			}
			return graphic;
		case Season.Summer:
			if (_summerLandTile[graphic] != 0)
			{
				return _summerLandTile[graphic];
			}
			return graphic;
		case Season.Fall:
			if (_fallLandTile[graphic] != 0)
			{
				return _fallLandTile[graphic];
			}
			return graphic;
		case Season.Winter:
			if (_winterLandTile[graphic] != 0)
			{
				return _winterLandTile[graphic];
			}
			return graphic;
		case Season.Desolation:
			if (_desolationLandTile[graphic] != 0)
			{
				return _desolationLandTile[graphic];
			}
			return graphic;
		default:
			return graphic;
		}
	}

	private static void CreateDefaultSeasonsFile()
	{
		if (File.Exists(_seasonsFile))
		{
			return;
		}
		using StreamWriter streamWriter = new StreamWriter(_seasonsFile);
		streamWriter.WriteLine("spring,static,0x0CA7,0x0C84");
		streamWriter.WriteLine("spring,static,0x0CAC,0x0C46");
		streamWriter.WriteLine("spring,static,0x0CAD,0x0C48");
		streamWriter.WriteLine("spring,static,0x0CAE,0x0CB5");
		streamWriter.WriteLine("spring,static,0x0C4A,0x0CB5");
		streamWriter.WriteLine("spring,static,0x0CAF,0x0C4E");
		streamWriter.WriteLine("spring,static,0x0CB0,0x0C4D");
		streamWriter.WriteLine("spring,static,0x0CB6,0x0D2B");
		streamWriter.WriteLine("spring,static,0x0D0D,0x0D2B");
		streamWriter.WriteLine("spring,static,0x0D14,0x0D2B");
		streamWriter.WriteLine("spring,static,0x0D0C,0x0D29");
		streamWriter.WriteLine("spring,static,0x0D0E,0x0CBE");
		streamWriter.WriteLine("spring,static,0x0D0F,0x0CBF");
		streamWriter.WriteLine("spring,static,0x0D10,0x0CC0");
		streamWriter.WriteLine("spring,static,0x0D11,0x0C87");
		streamWriter.WriteLine("spring,static,0x0D12,0x0C38");
		streamWriter.WriteLine("spring,static,0x0D13,0x0D2F");
		streamWriter.WriteLine("fall,static,0x0CD1,0x0CD2");
		streamWriter.WriteLine("fall,static,0x0CD4,0x0CD5");
		streamWriter.WriteLine("fall,static,0x0CDB,0x0CDC");
		streamWriter.WriteLine("fall,static,0x0CDE,0x0CDF");
		streamWriter.WriteLine("fall,static,0x0CE1,0x0CE2");
		streamWriter.WriteLine("fall,static,0x0CE4,0x0CE5");
		streamWriter.WriteLine("fall,static,0x0CE7,0x0CE8");
		streamWriter.WriteLine("fall,static,0x0D95,0x0D97");
		streamWriter.WriteLine("fall,static,0x0D99,0x0D9B");
		streamWriter.WriteLine("fall,static,0x0CCE,0x0CCF");
		streamWriter.WriteLine("fall,static,0x0CE9,0x0D3F");
		streamWriter.WriteLine("fall,static,0x0C9E,0x0D3F");
		streamWriter.WriteLine("fall,static,0x0CEA,0x0D40");
		streamWriter.WriteLine("fall,static,0x0C84,0x1B22");
		streamWriter.WriteLine("fall,static,0x0CB0,0x1B22");
		streamWriter.WriteLine("fall,static,0x0C8B,0x0CC6");
		streamWriter.WriteLine("fall,static,0x0C8C,0x0CC6");
		streamWriter.WriteLine("fall,static,0x0C8D,0x0CC6");
		streamWriter.WriteLine("fall,static,0x0C8E,0x0CC6");
		streamWriter.WriteLine("fall,static,0x0CA7,0x0C48");
		streamWriter.WriteLine("fall,static,0x0CAC,0x1B1F");
		streamWriter.WriteLine("fall,static,0x0CAD,0x1B20");
		streamWriter.WriteLine("fall,static,0x0CAE,0x1B21");
		streamWriter.WriteLine("fall,static,0x0CAF,0x0D0D");
		streamWriter.WriteLine("fall,static,0x0CB5,0x0D10");
		streamWriter.WriteLine("fall,static,0x0CB6,0x0D2B");
		streamWriter.WriteLine("fall,static,0x0CC7,0x0C4E");
		streamWriter.WriteLine("winter,static,0x0CA7,0x0CC6");
		streamWriter.WriteLine("winter,static,0x0CAC,0x0D3D");
		streamWriter.WriteLine("winter,static,0x0CAD,0x0D33");
		streamWriter.WriteLine("winter,static,0x0CAE,0x0D33");
		streamWriter.WriteLine("winter,static,0x0CB5,0x0D33");
		streamWriter.WriteLine("winter,static,0x0CAF,0x17CD");
		streamWriter.WriteLine("winter,static,0x0C87,0x17CD");
		streamWriter.WriteLine("winter,static,0x0C89,0x17CD");
		streamWriter.WriteLine("winter,static,0x0D16,0x17CD");
		streamWriter.WriteLine("winter,static,0x0D17,0x17CD");
		streamWriter.WriteLine("winter,static,0x0D32,0x17CD");
		streamWriter.WriteLine("winter,static,0x0D33,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB0,0x17CD");
		streamWriter.WriteLine("winter,static,0x0C8E,0x1B8D");
		streamWriter.WriteLine("winter,static,0x0C99,0x1B8D");
		streamWriter.WriteLine("winter,static,0x0C46,0x1B9D");
		streamWriter.WriteLine("winter,static,0x0C49,0x1B9D");
		streamWriter.WriteLine("winter,static,0x0C45,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0C48,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0CBF,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0C4E,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0D2B,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0C85,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0D15,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0D29,0x1B9C");
		streamWriter.WriteLine("winter,static,0x0CB1,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB2,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB3,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB4,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB7,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CC5,0x17CD");
		streamWriter.WriteLine("winter,static,0x0D0C,0x17CD");
		streamWriter.WriteLine("winter,static,0x0CB6,0x17CD");
		streamWriter.WriteLine("winter,static,0x0C37,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C38,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C47,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C4A,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C4B,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C4D,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C8C,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0D2F,0x1B1F");
		streamWriter.WriteLine("winter,static,0x0C8D,0x1B22");
		streamWriter.WriteLine("winter,static,0x0C93,0x1B22");
		streamWriter.WriteLine("winter,static,0x0C94,0x1B22");
		streamWriter.WriteLine("winter,static,0x0C98,0x1B22");
		streamWriter.WriteLine("winter,static,0x0C9F,0x1B22");
		streamWriter.WriteLine("winter,static,0x0CA0,0x1B22");
		streamWriter.WriteLine("winter,static,0x0CA1,0x1B22");
		streamWriter.WriteLine("winter,static,0x0CA2,0x1B22");
		streamWriter.WriteLine("winter,static,0x0CA3,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0CA4,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D0D,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D0E,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D10,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D12,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D13,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D18,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D19,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0D2D,0x1BAE");
		streamWriter.WriteLine("winter,static,0x0CC7,0x1B20");
		streamWriter.WriteLine("winter,static,0x0C84,0x1B84");
		streamWriter.WriteLine("winter,static,0x0C8B,0x1B84");
		streamWriter.WriteLine("winter,static,0x0CE9,0x0CCA");
		streamWriter.WriteLine("winter,static,0x0C9E,0x0CCA");
		streamWriter.WriteLine("winter,static,0x33A1,0x17CD");
		streamWriter.WriteLine("winter,static,0x33A2,0x17CD");
		streamWriter.WriteLine("winter,static,0x33A3,0x17CD");
		streamWriter.WriteLine("winter,static,0x33A4,0x17CD");
		streamWriter.WriteLine("winter,static,0x33A6,0x17CD");
		streamWriter.WriteLine("winter,static,0x33AB,0x17CD");
		streamWriter.WriteLine("winter,landtile,196,282");
		streamWriter.WriteLine("winter,landtile,197,283");
		streamWriter.WriteLine("winter,landtile,198,284");
		streamWriter.WriteLine("winter,landtile,199,285");
		streamWriter.WriteLine("winter,landtile,248,282");
		streamWriter.WriteLine("winter,landtile,249,283");
		streamWriter.WriteLine("winter,landtile,250,284");
		streamWriter.WriteLine("winter,landtile,251,285");
		streamWriter.WriteLine("winter,landtile,349,937");
		streamWriter.WriteLine("winter,landtile,350,940");
		streamWriter.WriteLine("winter,landtile,351,938");
		streamWriter.WriteLine("winter,landtile,352,939");
		streamWriter.WriteLine("winter,landtile,200,282");
		streamWriter.WriteLine("winter,landtile,201,283");
		streamWriter.WriteLine("winter,landtile,202,284");
		streamWriter.WriteLine("winter,landtile,203,285");
		streamWriter.WriteLine("winter,landtile,204,282");
		streamWriter.WriteLine("winter,landtile,205,283");
		streamWriter.WriteLine("winter,landtile,206,284");
		streamWriter.WriteLine("winter,landtile,207,285");
		streamWriter.WriteLine("winter,landtile,208,282");
		streamWriter.WriteLine("winter,landtile,209,283");
		streamWriter.WriteLine("winter,landtile,210,284");
		streamWriter.WriteLine("winter,landtile,211,285");
		streamWriter.WriteLine("winter,landtile,212,282");
		streamWriter.WriteLine("winter,landtile,213,283");
		streamWriter.WriteLine("winter,landtile,214,284");
		streamWriter.WriteLine("winter,landtile,215,285");
		streamWriter.WriteLine("winter,landtile,216,282");
		streamWriter.WriteLine("winter,landtile,217,283");
		streamWriter.WriteLine("winter,landtile,218,284");
		streamWriter.WriteLine("winter,landtile,219,285");
		streamWriter.WriteLine("winter,landtile,1697,282");
		streamWriter.WriteLine("winter,landtile,1698,283");
		streamWriter.WriteLine("winter,landtile,1699,284");
		streamWriter.WriteLine("winter,landtile,1700,285");
		streamWriter.WriteLine("winter,landtile,1711,282");
		streamWriter.WriteLine("winter,landtile,1712,283");
		streamWriter.WriteLine("winter,landtile,1713,284");
		streamWriter.WriteLine("winter,landtile,1714,285");
		streamWriter.WriteLine("winter,landtile,1715,282");
		streamWriter.WriteLine("winter,landtile,1716,283");
		streamWriter.WriteLine("winter,landtile,1717,284");
		streamWriter.WriteLine("winter,landtile,1718,285");
		streamWriter.WriteLine("winter,landtile,1719,282");
		streamWriter.WriteLine("winter,landtile,1720,283");
		streamWriter.WriteLine("winter,landtile,1721,284");
		streamWriter.WriteLine("winter,landtile,1722,285");
		streamWriter.WriteLine("winter,landtile,1723,282");
		streamWriter.WriteLine("winter,landtile,1724,283");
		streamWriter.WriteLine("winter,landtile,1725,284");
		streamWriter.WriteLine("winter,landtile,1726,285");
		streamWriter.WriteLine("winter,landtile,1727,282");
		streamWriter.WriteLine("winter,landtile,1728,283");
		streamWriter.WriteLine("winter,landtile,1729,284");
		streamWriter.WriteLine("winter,landtile,1730,285");
		streamWriter.WriteLine("winter,landtile,332,932");
		streamWriter.WriteLine("winter,landtile,333,929");
		streamWriter.WriteLine("winter,landtile,334,930");
		streamWriter.WriteLine("winter,landtile,335,931");
		streamWriter.WriteLine("winter,landtile,353,908");
		streamWriter.WriteLine("winter,landtile,354,907");
		streamWriter.WriteLine("winter,landtile,355,905");
		streamWriter.WriteLine("winter,landtile,356,906");
		streamWriter.WriteLine("winter,landtile,357,904");
		streamWriter.WriteLine("winter,landtile,358,903");
		streamWriter.WriteLine("winter,landtile,359,902");
		streamWriter.WriteLine("winter,landtile,360,901");
		streamWriter.WriteLine("winter,landtile,361,912");
		streamWriter.WriteLine("winter,landtile,362,911");
		streamWriter.WriteLine("winter,landtile,363,909");
		streamWriter.WriteLine("winter,landtile,364,910");
		streamWriter.WriteLine("winter,landtile,369,916");
		streamWriter.WriteLine("winter,landtile,370,915");
		streamWriter.WriteLine("winter,landtile,371,914");
		streamWriter.WriteLine("winter,landtile,372,913");
		streamWriter.WriteLine("winter,landtile,1351,917");
		streamWriter.WriteLine("winter,landtile,1352,918");
		streamWriter.WriteLine("winter,landtile,1353,919");
		streamWriter.WriteLine("winter,landtile,1354,920");
		streamWriter.WriteLine("winter,landtile,1355,921");
		streamWriter.WriteLine("winter,landtile,1356,922");
		streamWriter.WriteLine("winter,landtile,1357,923");
		streamWriter.WriteLine("winter,landtile,1358,924");
		streamWriter.WriteLine("winter,landtile,1359,925");
		streamWriter.WriteLine("winter,landtile,1360,927");
		streamWriter.WriteLine("winter,landtile,1361,928");
		streamWriter.WriteLine("winter,landtile,1362,930");
		streamWriter.WriteLine("winter,landtile,1363,933");
		streamWriter.WriteLine("winter,landtile,1364,934");
		streamWriter.WriteLine("winter,landtile,1365,935");
		streamWriter.WriteLine("winter,landtile,1366,936");
		streamWriter.WriteLine("winter,landtile,804,931");
		streamWriter.WriteLine("winter,landtile,805,929");
		streamWriter.WriteLine("winter,landtile,806,926");
		streamWriter.WriteLine("winter,landtile,807,925");
		streamWriter.WriteLine("winter,landtile,808,932");
		streamWriter.WriteLine("winter,landtile,809,930");
		streamWriter.WriteLine("winter,landtile,810,928");
		streamWriter.WriteLine("winter,landtile,811,927");
		streamWriter.WriteLine("winter,landtile,812,919");
		streamWriter.WriteLine("winter,landtile,813,920");
		streamWriter.WriteLine("winter,landtile,814,917");
		streamWriter.WriteLine("winter,landtile,815,921");
		streamWriter.WriteLine("winter,landtile,3,282");
		streamWriter.WriteLine("winter,landtile,4,283");
		streamWriter.WriteLine("winter,landtile,5,284");
		streamWriter.WriteLine("winter,landtile,6,285");
		streamWriter.WriteLine("winter,landtile,121,910");
		streamWriter.WriteLine("winter,landtile,122,909");
		streamWriter.WriteLine("winter,landtile,123,912");
		streamWriter.WriteLine("winter,landtile,124,911");
		streamWriter.WriteLine("winter,landtile,125,906");
		streamWriter.WriteLine("winter,landtile,126,905");
		streamWriter.WriteLine("winter,landtile,130,908");
		streamWriter.WriteLine("winter,landtile,131,907");
		streamWriter.WriteLine("winter,landtile,133,904");
		streamWriter.WriteLine("winter,landtile,134,904");
		streamWriter.WriteLine("winter,landtile,135,903");
		streamWriter.WriteLine("winter,landtile,136,903");
		streamWriter.WriteLine("winter,landtile,137,902");
		streamWriter.WriteLine("winter,landtile,138,902");
		streamWriter.WriteLine("winter,landtile,139,901");
		streamWriter.WriteLine("winter,landtile,140,901");
		streamWriter.WriteLine("winter,landtile,871,917");
		streamWriter.WriteLine("winter,landtile,872,918");
		streamWriter.WriteLine("winter,landtile,873,919");
		streamWriter.WriteLine("winter,landtile,874,920");
		streamWriter.WriteLine("winter,landtile,875,921");
		streamWriter.WriteLine("winter,landtile,876,922");
		streamWriter.WriteLine("winter,landtile,877,923");
		streamWriter.WriteLine("winter,landtile,878,924");
		streamWriter.WriteLine("winter,landtile,879,925");
		streamWriter.WriteLine("winter,landtile,880,926");
		streamWriter.WriteLine("winter,landtile,881,927");
		streamWriter.WriteLine("winter,landtile,882,928");
		streamWriter.WriteLine("winter,landtile,883,929");
		streamWriter.WriteLine("winter,landtile,884,930");
		streamWriter.WriteLine("winter,landtile,885,931");
		streamWriter.WriteLine("winter,landtile,886,932");
		streamWriter.WriteLine("winter,landtile,887,933");
		streamWriter.WriteLine("winter,landtile,888,934");
		streamWriter.WriteLine("winter,landtile,889,935");
		streamWriter.WriteLine("winter,landtile,890,936");
		streamWriter.WriteLine("winter,landtile,891,937");
		streamWriter.WriteLine("winter,landtile,892,938");
		streamWriter.WriteLine("winter,landtile,893,939");
		streamWriter.WriteLine("winter,landtile,894,940");
		streamWriter.WriteLine("winter,landtile,365,916");
		streamWriter.WriteLine("winter,landtile,366,915");
		streamWriter.WriteLine("winter,landtile,367,913");
		streamWriter.WriteLine("winter,landtile,368,914");
		streamWriter.WriteLine("winter,landtile,236,278");
		streamWriter.WriteLine("winter,landtile,237,279");
		streamWriter.WriteLine("winter,landtile,238,276");
		streamWriter.WriteLine("winter,landtile,239,277");
		streamWriter.WriteLine("winter,landtile,240,305");
		streamWriter.WriteLine("winter,landtile,241,302");
		streamWriter.WriteLine("winter,landtile,242,303");
		streamWriter.WriteLine("winter,landtile,243,304");
		streamWriter.WriteLine("winter,landtile,244,272");
		streamWriter.WriteLine("winter,landtile,245,273");
		streamWriter.WriteLine("winter,landtile,246,274");
		streamWriter.WriteLine("winter,landtile,247,275");
		streamWriter.WriteLine("winter,landtile,561,268");
		streamWriter.WriteLine("winter,landtile,562,269");
		streamWriter.WriteLine("winter,landtile,563,270");
		streamWriter.WriteLine("winter,landtile,564,271");
		streamWriter.WriteLine("winter,landtile,565,272");
		streamWriter.WriteLine("winter,landtile,566,273");
		streamWriter.WriteLine("winter,landtile,567,274");
		streamWriter.WriteLine("winter,landtile,568,275");
		streamWriter.WriteLine("winter,landtile,569,276");
		streamWriter.WriteLine("winter,landtile,570,277");
		streamWriter.WriteLine("winter,landtile,571,278");
		streamWriter.WriteLine("winter,landtile,572,279");
		streamWriter.WriteLine("winter,landtile,573,1861");
		streamWriter.WriteLine("winter,landtile,574,1862");
		streamWriter.WriteLine("winter,landtile,575,1863");
		streamWriter.WriteLine("winter,landtile,576,1864");
		streamWriter.WriteLine("winter,landtile,577,1865");
		streamWriter.WriteLine("winter,landtile,578,1866");
		streamWriter.WriteLine("winter,landtile,579,1867");
		streamWriter.WriteLine("winter,landtile,1741,1868");
		streamWriter.WriteLine("winter,landtile,1742,1869");
		streamWriter.WriteLine("winter,landtile,1743,1870");
		streamWriter.WriteLine("winter,landtile,1744,1871");
		streamWriter.WriteLine("winter,landtile,1745,1872");
		streamWriter.WriteLine("winter,landtile,1746,1873");
		streamWriter.WriteLine("winter,landtile,1747,1874");
		streamWriter.WriteLine("winter,landtile,1748,1875");
		streamWriter.WriteLine("winter,landtile,1749,1876");
		streamWriter.WriteLine("winter,landtile,1750,1877");
		streamWriter.WriteLine("winter,landtile,1751,1878");
		streamWriter.WriteLine("winter,landtile,1752,1879");
		streamWriter.WriteLine("winter,landtile,1753,1880");
		streamWriter.WriteLine("winter,landtile,1754,1881");
		streamWriter.WriteLine("winter,landtile,1755,1882");
		streamWriter.WriteLine("winter,landtile,1756,1883");
		streamWriter.WriteLine("winter,landtile,1757,1884");
		streamWriter.WriteLine("winter,landtile,1758,282");
		streamWriter.WriteLine("winter,landtile,1759,283");
		streamWriter.WriteLine("winter,landtile,1760,284");
		streamWriter.WriteLine("winter,landtile,1761,285");
		streamWriter.WriteLine("winter,landtile,26,379");
		streamWriter.WriteLine("winter,landtile,27,378");
		streamWriter.WriteLine("winter,landtile,28,377");
		streamWriter.WriteLine("winter,landtile,29,380");
		streamWriter.WriteLine("winter,landtile,30,381");
		streamWriter.WriteLine("winter,landtile,31,382");
		streamWriter.WriteLine("winter,landtile,32,383");
		streamWriter.WriteLine("winter,landtile,33,384");
		streamWriter.WriteLine("winter,landtile,34,385");
		streamWriter.WriteLine("winter,landtile,35,386");
		streamWriter.WriteLine("winter,landtile,36,387");
		streamWriter.WriteLine("winter,landtile,37,388");
		streamWriter.WriteLine("winter,landtile,38,389");
		streamWriter.WriteLine("winter,landtile,39,390");
		streamWriter.WriteLine("winter,landtile,40,391");
		streamWriter.WriteLine("winter,landtile,41,392");
		streamWriter.WriteLine("winter,landtile,42,393");
		streamWriter.WriteLine("winter,landtile,43,394");
		streamWriter.WriteLine("winter,landtile,44,387");
		streamWriter.WriteLine("winter,landtile,45,388");
		streamWriter.WriteLine("winter,landtile,46,383");
		streamWriter.WriteLine("winter,landtile,47,380");
		streamWriter.WriteLine("winter,landtile,48,383");
		streamWriter.WriteLine("winter,landtile,49,378");
		streamWriter.WriteLine("winter,landtile,50,379");
		streamWriter.WriteLine("winter,landtile,141,379");
		streamWriter.WriteLine("winter,landtile,142,386");
		streamWriter.WriteLine("winter,landtile,143,385");
		streamWriter.WriteLine("winter,landtile,144,393");
		streamWriter.WriteLine("winter,landtile,145,378");
		streamWriter.WriteLine("winter,landtile,146,387");
		streamWriter.WriteLine("winter,landtile,147,391");
		streamWriter.WriteLine("winter,landtile,148,392");
		streamWriter.WriteLine("winter,landtile,149,377");
		streamWriter.WriteLine("winter,landtile,150,379");
		streamWriter.WriteLine("winter,landtile,151,383");
		streamWriter.WriteLine("winter,landtile,152,380");
		streamWriter.WriteLine("winter,landtile,153,387");
		streamWriter.WriteLine("winter,landtile,154,388");
		streamWriter.WriteLine("winter,landtile,155,393");
		streamWriter.WriteLine("winter,landtile,156,391");
		streamWriter.WriteLine("winter,landtile,157,387");
		streamWriter.WriteLine("winter,landtile,158,385");
		streamWriter.WriteLine("winter,landtile,159,385");
		streamWriter.WriteLine("winter,landtile,160,389");
		streamWriter.WriteLine("winter,landtile,161,379");
		streamWriter.WriteLine("winter,landtile,162,384");
		streamWriter.WriteLine("winter,landtile,163,380");
		streamWriter.WriteLine("winter,landtile,164,379");
		streamWriter.WriteLine("winter,landtile,165,378");
		streamWriter.WriteLine("winter,landtile,166,378");
		streamWriter.WriteLine("winter,landtile,167,394");
		streamWriter.WriteLine("winter,landtile,1521,282");
		streamWriter.WriteLine("winter,landtile,1522,283");
		streamWriter.WriteLine("winter,landtile,1523,284");
		streamWriter.WriteLine("winter,landtile,1524,285");
		streamWriter.WriteLine("winter,landtile,1529,282");
		streamWriter.WriteLine("winter,landtile,1530,283");
		streamWriter.WriteLine("winter,landtile,1531,284");
		streamWriter.WriteLine("winter,landtile,1532,285");
		streamWriter.WriteLine("winter,landtile,1533,282");
		streamWriter.WriteLine("winter,landtile,1534,283");
		streamWriter.WriteLine("winter,landtile,1535,284");
		streamWriter.WriteLine("winter,landtile,1536,285");
		streamWriter.WriteLine("winter,landtile,1537,282");
		streamWriter.WriteLine("winter,landtile,1538,283");
		streamWriter.WriteLine("winter,landtile,1539,284");
		streamWriter.WriteLine("winter,landtile,1540,285");
		streamWriter.WriteLine("winter,landtile,741,379");
		streamWriter.WriteLine("winter,landtile,742,385");
		streamWriter.WriteLine("winter,landtile,743,389");
		streamWriter.WriteLine("winter,landtile,744,393");
		streamWriter.WriteLine("winter,landtile,745,378");
		streamWriter.WriteLine("winter,landtile,746,384");
		streamWriter.WriteLine("winter,landtile,747,388");
		streamWriter.WriteLine("winter,landtile,748,392");
		streamWriter.WriteLine("winter,landtile,749,377");
		streamWriter.WriteLine("winter,landtile,750,385");
		streamWriter.WriteLine("winter,landtile,751,383");
		streamWriter.WriteLine("winter,landtile,752,380");
		streamWriter.WriteLine("winter,landtile,753,391");
		streamWriter.WriteLine("winter,landtile,754,388");
		streamWriter.WriteLine("winter,landtile,755,385");
		streamWriter.WriteLine("winter,landtile,756,384");
		streamWriter.WriteLine("winter,landtile,757,391");
		streamWriter.WriteLine("winter,landtile,758,379");
		streamWriter.WriteLine("winter,landtile,759,393");
		streamWriter.WriteLine("winter,landtile,760,383");
		streamWriter.WriteLine("winter,landtile,761,385");
		streamWriter.WriteLine("winter,landtile,762,391");
		streamWriter.WriteLine("winter,landtile,763,391");
		streamWriter.WriteLine("winter,landtile,764,379");
		streamWriter.WriteLine("winter,landtile,765,384");
		streamWriter.WriteLine("winter,landtile,766,384");
		streamWriter.WriteLine("winter,landtile,767,379");
		streamWriter.WriteLine("winter,landtile,9,282");
		streamWriter.WriteLine("winter,landtile,10,283");
		streamWriter.WriteLine("winter,landtile,11,284");
		streamWriter.WriteLine("winter,landtile,12,285");
		streamWriter.WriteLine("winter,landtile,13,282");
		streamWriter.WriteLine("winter,landtile,14,283");
		streamWriter.WriteLine("winter,landtile,15,284");
		streamWriter.WriteLine("winter,landtile,16,285");
		streamWriter.WriteLine("winter,landtile,17,282");
		streamWriter.WriteLine("winter,landtile,18,283");
		streamWriter.WriteLine("winter,landtile,19,284");
		streamWriter.WriteLine("winter,landtile,20,285");
		streamWriter.WriteLine("winter,landtile,21,282");
		streamWriter.WriteLine("desolation,static,0x1B7E,0x1E34");
		streamWriter.WriteLine("desolation,static,0x0D2B,0x1B15");
		streamWriter.WriteLine("desolation,static,0x0D11,0x122B");
		streamWriter.WriteLine("desolation,static,0x0D14,0x122B");
		streamWriter.WriteLine("desolation,static,0x0D17,0x122B");
		streamWriter.WriteLine("desolation,static,0x0D16,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CB9,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CBA,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CBB,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CBC,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CBD,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CBE,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CC7,0x1B0D");
		streamWriter.WriteLine("desolation,static,0x0CE9,0x0ED7");
		streamWriter.WriteLine("desolation,static,0x0CEA,0x0D3F");
		streamWriter.WriteLine("desolation,static,0x0D0F,0x1B1C");
		streamWriter.WriteLine("desolation,static,0x0CB8,0x1CEA");
		streamWriter.WriteLine("desolation,static,0x0C84,0x1B84");
		streamWriter.WriteLine("desolation,static,0x0C8B,0x1B84");
		streamWriter.WriteLine("desolation,static,0x0C9E,0x1182");
		streamWriter.WriteLine("desolation,static,0x0CAD,0x1AE1");
		streamWriter.WriteLine("desolation,static,0x0C4C,0x1B16");
		streamWriter.WriteLine("desolation,static,0x0C8E,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0C99,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0CAC,0x1B8D");
		streamWriter.WriteLine("desolation,static,0x0C46,0x1B9D");
		streamWriter.WriteLine("desolation,static,0x0C49,0x1B9D");
		streamWriter.WriteLine("desolation,static,0x0CB6,0x1B9D");
		streamWriter.WriteLine("desolation,static,0x0C45,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0C48,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0C4E,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0C85,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0CA7,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0CAE,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0CAF,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0CB5,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0D15,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0D29,0x1B9C");
		streamWriter.WriteLine("desolation,static,0x0C37,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C38,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C47,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C4A,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C4B,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C4D,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C8C,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C8D,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C93,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C94,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C98,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0C9F,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CA0,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CA1,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CA2,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CA3,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CA4,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB0,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB1,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB2,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB3,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB4,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CB7,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0CC5,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D0C,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D0D,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D0E,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D10,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D12,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D13,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D18,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D19,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D2D,0x1BAE");
		streamWriter.WriteLine("desolation,static,0x0D2F,0x1BAE");
	}
}
