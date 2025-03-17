using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsNecromancy
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsNecromancy()
	{
		SpellBookName = SpellBookType.Necromancy.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Animate Dead", 101, 20480, "Uus Corp", 23, 40, TargetType.Neutral, Reagents.DaemonBlood, Reagents.GraveDust)
			},
			{
				2,
				new SpellDefinition("Blood Oath", 102, 20481, "In Jux Mani Xen", 13, 20, TargetType.Harmful, Reagents.DaemonBlood)
			},
			{
				3,
				new SpellDefinition("Corpse Skin", 103, 20482, "In Agle Corp Ylem", 11, 20, TargetType.Harmful, Reagents.BatWing, Reagents.GraveDust)
			},
			{
				4,
				new SpellDefinition("Curse Weapon", 104, 20483, "An Sanct Gra Char", 7, 0, TargetType.Neutral, Reagents.PigIron)
			},
			{
				5,
				new SpellDefinition("Evil Omen", 105, 20484, "Pas Tym An Sanct", 11, 20, TargetType.Harmful, Reagents.BatWing, Reagents.NoxCrystal)
			},
			{
				6,
				new SpellDefinition("Horrific Beast", 106, 20485, "Rel Xen Vas Bal", 11, 40, TargetType.Neutral, Reagents.BatWing, Reagents.DaemonBlood)
			},
			{
				7,
				new SpellDefinition("Lich Form", 107, 20486, "Rel Xen Corp Ort", 25, 70, TargetType.Neutral, Reagents.DaemonBlood, Reagents.GraveDust, Reagents.NoxCrystal)
			},
			{
				8,
				new SpellDefinition("Mind Rot", 108, 20487, "Wis An Ben", 17, 30, TargetType.Harmful, Reagents.BatWing, Reagents.DaemonBlood, Reagents.PigIron)
			},
			{
				9,
				new SpellDefinition("Pain Spike", 109, 20488, "In Sar", 5, 20, TargetType.Harmful, Reagents.GraveDust, Reagents.PigIron)
			},
			{
				10,
				new SpellDefinition("Poison Strike", 110, 20489, "In Vas Nox", 17, 50, TargetType.Harmful, Reagents.NoxCrystal)
			},
			{
				11,
				new SpellDefinition("Strangle", 111, 20490, "In Bal Nox", 29, 65, TargetType.Harmful, Reagents.DaemonBlood, Reagents.NoxCrystal)
			},
			{
				12,
				new SpellDefinition("Summon Familiar", 112, 20491, "Kal Xen Bal", 17, 30, TargetType.Neutral, Reagents.BatWing, Reagents.DaemonBlood, Reagents.GraveDust)
			},
			{
				13,
				new SpellDefinition("Vampiric Embrace", 113, 20492, "Rel Xen An Sanct", 25, 99, TargetType.Neutral, Reagents.BatWing, Reagents.NoxCrystal, Reagents.PigIron)
			},
			{
				14,
				new SpellDefinition("Vengeful Spirit", 114, 20493, "Kal Xen Bal Beh", 41, 80, TargetType.Harmful, Reagents.BatWing, Reagents.GraveDust, Reagents.PigIron)
			},
			{
				15,
				new SpellDefinition("Wither", 115, 20494, "Kal Vas An Flam", 23, 60, TargetType.Harmful, Reagents.GraveDust, Reagents.NoxCrystal, Reagents.PigIron)
			},
			{
				16,
				new SpellDefinition("Wraith Form", 116, 20495, "Rel Xen Um", 17, 20, TargetType.Neutral, Reagents.NoxCrystal, Reagents.PigIron)
			},
			{
				17,
				new SpellDefinition("Exorcism", 117, 20496, "Ort Corp Grav", 40, 80, TargetType.Neutral, Reagents.NoxCrystal, Reagents.GraveDust)
			}
		};
	}

	public static SpellDefinition GetSpell(int spellIndex)
	{
		if (!_spellsDict.TryGetValue(spellIndex, out var value))
		{
			return SpellDefinition.EmptySpell;
		}
		return value;
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
