using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsMastery
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static readonly int[][] SpellbookIndices;

	public static readonly string SpellBookName;

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsMastery()
	{
		SpellbookIndices = new int[6][]
		{
			new int[8] { 1, 2, 3, 4, 5, 6, 7, 8 },
			new int[8] { 9, 10, 11, 12, 13, 14, 19, 20 },
			new int[8] { 17, 21, 22, 25, 26, 34, 35, 36 },
			new int[8] { 27, 28, 29, 32, 37, 38, 40, 41 },
			new int[6] { 23, 24, 30, 31, 43, 44 },
			new int[7] { 15, 16, 18, 33, 39, 42, 45 }
		};
		SpellBookName = SpellBookType.Mastery.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Inspire", 701, 2373, 2373, "Uus Por", 16, 90, 4, TargetType.Beneficial, Reagents.None)
			},
			{
				2,
				new SpellDefinition("Invigorate", 702, 2374, 2374, "An Zu", 22, 90, 5, TargetType.Beneficial, Reagents.None)
			},
			{
				3,
				new SpellDefinition("Resilience", 703, 2375, 2375, "Kal Mani Tym", 16, 90, 4, TargetType.Beneficial, Reagents.None)
			},
			{
				4,
				new SpellDefinition("Perseverance", 704, 2376, 2376, "Uus Jux Sanct", 18, 90, 5, TargetType.Beneficial, Reagents.None)
			},
			{
				5,
				new SpellDefinition("Tribulation", 705, 2377, 2377, "In Jux Hur Rel", 24, 90, 10, TargetType.Harmful, Reagents.None)
			},
			{
				6,
				new SpellDefinition("Despair", 706, 2378, 2378, "Kal Des Mani Tym", 26, 90, 12, TargetType.Harmful, Reagents.None)
			},
			{
				7,
				new SpellDefinition("Death Ray", 707, 39819, 39819, "In Grav Corp", 50, 90, 35, TargetType.Harmful, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.SpidersSilk)
			},
			{
				8,
				new SpellDefinition("Ethereal Burst", 708, 39820, 39820, "Uus Ort Grav", 0, 90, 0, TargetType.Beneficial, Reagents.Bloodmoss, Reagents.Ginseng, Reagents.MandrakeRoot)
			},
			{
				9,
				new SpellDefinition("Nether Blast", 709, 39821, 39821, "In Vas Xen Por", 40, 90, 0, TargetType.Harmful, Reagents.DragonsBlood, Reagents.DemonBone)
			},
			{
				10,
				new SpellDefinition("Mystic Weapon", 710, 39822, 39822, "Vas Ylem Wis", 40, 90, 0, TargetType.Neutral, Reagents.FertileDirt, Reagents.Bone)
			},
			{
				11,
				new SpellDefinition("Command Undead", 711, 39823, 39823, "In Corp Xen Por", 40, 90, 0, TargetType.Neutral, Reagents.DaemonBlood, Reagents.PigIron, Reagents.BatWing)
			},
			{
				12,
				new SpellDefinition("Conduit", 712, 39824, 39824, "Uus Corp Grav", 40, 90, 0, TargetType.Harmful, Reagents.NoxCrystal, Reagents.BatWing, Reagents.GraveDust)
			},
			{
				13,
				new SpellDefinition("Mana Shield", 713, 39825, 39825, "Faerkulggen", 40, 90, 0, TargetType.Beneficial)
			},
			{
				14,
				new SpellDefinition("Summon Reaper", 714, 39826, 39826, "Lartarisstree", 50, 90, 0, TargetType.Neutral)
			},
			{
				15,
				new SpellDefinition("Enchanted Summoning", 715, 39827, 39827, "", 0, 90, 0, TargetType.Neutral)
			},
			{
				16,
				new SpellDefinition("Anticipate Hit", 716, 39828, 39828, "", 10, 90, 0, TargetType.Neutral)
			},
			{
				17,
				new SpellDefinition("Warcry", 717, 39829, 39829, "", 40, 90, 0, TargetType.Neutral)
			},
			{
				18,
				new SpellDefinition("Intuition", 718, 39830, 39830, "", 0, 90, 0, TargetType.Neutral)
			},
			{
				19,
				new SpellDefinition("Rejuvenate", 719, 39831, 39831, "", 10, 90, 35, TargetType.Neutral)
			},
			{
				20,
				new SpellDefinition("Holy Fist", 720, 39832, 39832, "", 50, 90, 35, TargetType.Neutral)
			},
			{
				21,
				new SpellDefinition("Shadow", 721, 39833, 39833, "", 10, 90, 4, TargetType.Neutral)
			},
			{
				22,
				new SpellDefinition("White Tiger Form", 722, 39834, 39834, "", 10, 90, 0, TargetType.Neutral)
			},
			{
				23,
				new SpellDefinition("Flaming Shot", 723, 39835, 39835, "", 30, 90, 0, TargetType.Neutral)
			},
			{
				24,
				new SpellDefinition("Playing The Odds", 724, 39836, 39836, "", 25, 90, 0, TargetType.Neutral)
			},
			{
				25,
				new SpellDefinition("Thrust", 725, 39837, 39837, "", 30, 90, 20, TargetType.Neutral)
			},
			{
				26,
				new SpellDefinition("Pierce", 726, 39838, 39838, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				27,
				new SpellDefinition("Stagger", 727, 39839, 39839, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				28,
				new SpellDefinition("Toughness", 728, 39840, 39840, "", 20, 90, 20, TargetType.Neutral)
			},
			{
				29,
				new SpellDefinition("Onslaught", 729, 39841, 39841, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				30,
				new SpellDefinition("Focused Eye", 730, 39842, 39842, "", 20, 90, 20, TargetType.Neutral)
			},
			{
				31,
				new SpellDefinition("Elemental Fury", 731, 39843, 39843, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				32,
				new SpellDefinition("Called Shot", 732, 39844, 39844, "", 40, 90, 0, TargetType.Neutral)
			},
			{
				33,
				new SpellDefinition("Warrior's Gifts", 733, 39845, 39845, "", 50, 90, 0, TargetType.Neutral)
			},
			{
				34,
				new SpellDefinition("Shield Bash", 734, 39846, 39846, "", 50, 90, 0, TargetType.Neutral)
			},
			{
				35,
				new SpellDefinition("Bodyguard", 735, 39847, 39847, "", 40, 90, 0, TargetType.Neutral)
			},
			{
				36,
				new SpellDefinition("Heighten Senses", 736, 39848, 39848, "", 10, 90, 10, TargetType.Neutral)
			},
			{
				37,
				new SpellDefinition("Tolerance", 737, 39849, 39849, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				38,
				new SpellDefinition("Injected Strike", 738, 39850, 39850, "", 30, 90, 0, TargetType.Neutral)
			},
			{
				39,
				new SpellDefinition("Potency", 739, 39851, 39851, "", 0, 90, 0, TargetType.Neutral)
			},
			{
				40,
				new SpellDefinition("Rampage", 740, 39852, 39852, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				41,
				new SpellDefinition("Fists of Fury", 741, 39853, 39853, "", 20, 90, 0, TargetType.Neutral)
			},
			{
				42,
				new SpellDefinition("Knockout", 742, 39854, 39854, "", 0, 90, 0, TargetType.Neutral)
			},
			{
				43,
				new SpellDefinition("Whispering", 743, 39855, 39855, "", 40, 90, 0, TargetType.Neutral)
			},
			{
				44,
				new SpellDefinition("Combat Training", 744, 39856, 39856, "", 40, 90, 0, TargetType.Neutral)
			},
			{
				45,
				new SpellDefinition("Boarding", 745, 39857, 39857, "", 0, 90, 0, TargetType.Neutral)
			}
		};
	}

	internal static string GetUsedSkillName(int spellid)
	{
		int num = MaxSpellCount * 3 >> 3;
		if (num <= 0)
		{
			num = 1;
		}
		int num2 = spellid / num;
		return num2 switch
		{
			0 => "Provocation", 
			1 => "Peacemaking", 
			3 => "Discordance", 
			4 => "Magery", 
			5 => "Mysticism", 
			6 => "Necromancy", 
			7 => "Spellweaving", 
			8 => "Passive", 
			9 => "Bushido", 
			_ => num2 switch
			{
				0 => "Provocation", 
				1 => "Peacemaking", 
				_ => "Discordance", 
			}, 
		};
	}

	public static SpellDefinition GetSpell(int spellIndex)
	{
		if (_spellsDict.TryGetValue(spellIndex, out var value))
		{
			return value;
		}
		return SpellDefinition.EmptySpell;
	}

	public static List<int> GetSpellListByGroupName(string group)
	{
		List<int> list = new List<int>();
		switch (group.ToLower())
		{
		default:
			list.Add(1);
			list.Add(2);
			break;
		case "peacemaking":
			list.Add(3);
			list.Add(4);
			break;
		case "discordance":
			list.Add(5);
			list.Add(6);
			break;
		case "magery":
			list.Add(15);
			list.Add(7);
			list.Add(8);
			break;
		case "mysticism":
			list.Add(15);
			list.Add(9);
			list.Add(10);
			break;
		case "necromancy":
			list.Add(15);
			list.Add(11);
			list.Add(12);
			break;
		case "spellweaving":
			list.Add(15);
			list.Add(13);
			list.Add(14);
			break;
		case "bushido":
			list.Add(18);
			list.Add(16);
			list.Add(17);
			break;
		case "chivalry":
			list.Add(18);
			list.Add(19);
			list.Add(20);
			break;
		case "ninjitsu":
			list.Add(18);
			list.Add(21);
			list.Add(22);
			break;
		case "archery":
			list.Add(33);
			list.Add(23);
			list.Add(24);
			break;
		case "fencing":
			list.Add(33);
			list.Add(25);
			list.Add(26);
			break;
		case "mace fighting":
			list.Add(33);
			list.Add(27);
			list.Add(28);
			break;
		case "swordsmanship":
			list.Add(33);
			list.Add(29);
			list.Add(30);
			break;
		case "throwing":
			list.Add(33);
			list.Add(31);
			list.Add(32);
			break;
		case "parrying":
			list.Add(34);
			list.Add(35);
			list.Add(36);
			break;
		case "poisoning":
			list.Add(39);
			list.Add(37);
			list.Add(38);
			break;
		case "wrestling":
			list.Add(40);
			list.Add(42);
			list.Add(41);
			break;
		case "animal taming":
			list.Add(45);
			list.Add(43);
			list.Add(44);
			break;
		}
		return list;
	}

	public static string GetMasteryGroupByID(int id)
	{
		switch (id)
		{
		default:
			return "Provocation";
		case 3:
		case 4:
			return "Peacemaking";
		case 5:
		case 6:
			return "Discordance";
		case 7:
		case 8:
			return "Magery";
		case 9:
		case 10:
			return "Mysticism";
		case 11:
		case 12:
			return "Necromancy";
		case 13:
		case 14:
			return "Spellweaving";
		case 16:
		case 17:
			return "Bushido";
		case 19:
		case 20:
			return "Chivalry";
		case 21:
		case 22:
			return "Ninjitsu";
		case 23:
		case 24:
			return "Archery";
		case 25:
		case 26:
			return "Fencing";
		case 27:
		case 28:
			return "Mace Fighting";
		case 29:
		case 30:
			return "Swordmanship";
		case 31:
		case 32:
			return "Throwing";
		case 34:
		case 35:
		case 36:
			return "Parrying";
		case 37:
		case 38:
		case 39:
			return "Poisoning";
		case 40:
		case 41:
		case 42:
			return "Wrestling";
		case 43:
		case 44:
		case 45:
			return "Animal Taming";
		case 15:
		case 18:
		case 33:
			return "Passive";
		}
	}

	public static void SetSpell(int id, in SpellDefinition newspell)
	{
		_spellsDict[id] = newspell;
	}

	internal static void Clear()
	{
		_spellsDict.Clear();
	}
}
