using System.Collections.Generic;
using ClassicUO.Data;
using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Gumps.CharCreation;

internal class ProfessionInfo
{
	internal static readonly int[,] _VoidSkills = new int[4, 2]
	{
		{ 0, InitialSkillValue },
		{ 0, InitialSkillValue },
		{
			0,
			(Client.Version >= ClientVersion.CV_70160) ? InitialSkillValue : 0
		},
		{ 0, InitialSkillValue }
	};

	internal static readonly int[] _VoidStats = new int[3] { 60, RemainStatValue, RemainStatValue };

	public static int InitialSkillValue
	{
		get
		{
			if (Client.Version < ClientVersion.CV_70160)
			{
				return 50;
			}
			return 30;
		}
	}

	public static int RemainStatValue
	{
		get
		{
			if (Client.Version < ClientVersion.CV_70160)
			{
				return 10;
			}
			return 15;
		}
	}

	public string Name { get; set; }

	public string TrueName { get; set; }

	public int Localization { get; set; }

	public int Description { get; set; }

	public int DescriptionIndex { get; set; }

	public ProfessionLoader.PROF_TYPE Type { get; set; }

	public ushort Graphic { get; set; }

	public bool TopLevel { get; set; }

	public int[,] SkillDefVal { get; set; } = _VoidSkills;

	public int[] StatsVal { get; set; } = _VoidStats;

	public List<string> Childrens { get; set; }
}
