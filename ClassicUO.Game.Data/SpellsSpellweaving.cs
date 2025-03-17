using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsSpellweaving
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsSpellweaving()
	{
		SpellBookName = SpellBookType.Spellweaving.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Arcane Circle", 601, 23000, "Myrshalee", 20, 0, TargetType.Neutral, Reagents.None)
			},
			{
				2,
				new SpellDefinition("Gift of Renewal", 602, 23001, "Olorisstra", 24, 0, TargetType.Beneficial, Reagents.None)
			},
			{
				3,
				new SpellDefinition("Immolating Weapon", 603, 23002, "Thalshara", 32, 10, TargetType.Neutral, Reagents.None)
			},
			{
				4,
				new SpellDefinition("Attune Weapon", 604, 23003, "Haeldril", 24, 0, TargetType.Harmful, Reagents.None)
			},
			{
				5,
				new SpellDefinition("Thunderstorm", 605, 23004, "Erelonia", 32, 10, TargetType.Harmful, Reagents.None)
			},
			{
				6,
				new SpellDefinition("Nature's Fury", 606, 23005, "Rauvvrae", 24, 0, TargetType.Neutral, Reagents.None)
			},
			{
				7,
				new SpellDefinition("Summon Fey", 607, 23006, "Alalithra", 10, 38, TargetType.Neutral, Reagents.None)
			},
			{
				8,
				new SpellDefinition("Summon Fiend", 608, 23007, "Nylisstra", 10, 38, TargetType.Neutral, Reagents.None)
			},
			{
				9,
				new SpellDefinition("Reaper Form", 609, 23008, "Tarisstree", 34, 24, TargetType.Neutral, Reagents.None)
			},
			{
				10,
				new SpellDefinition("Wildfire", 610, 23009, "Haelyn", 50, 66, TargetType.Harmful, Reagents.None)
			},
			{
				11,
				new SpellDefinition("Essence of Wind", 611, 23010, "Anathrae", 40, 52, TargetType.Harmful, Reagents.None)
			},
			{
				12,
				new SpellDefinition("Dryad Allure", 612, 23011, "Rathril", 40, 52, TargetType.Neutral, Reagents.None)
			},
			{
				13,
				new SpellDefinition("Ethereal Voyage", 613, 23012, "Orlavdra", 32, 24, TargetType.Neutral, Reagents.None)
			},
			{
				14,
				new SpellDefinition("Word of Death", 614, 23013, "Nyraxle", 50, 23, TargetType.Harmful, Reagents.None)
			},
			{
				15,
				new SpellDefinition("Gift of Life", 615, 23014, "Illorae", 70, 38, TargetType.Beneficial, Reagents.None)
			},
			{
				16,
				new SpellDefinition("Arcane Empowerment", 616, 23015, "Aslavdra", 50, 24, TargetType.Beneficial, Reagents.None)
			}
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

	public static void SetSpell(int id, in SpellDefinition newspell)
	{
		_spellsDict[id] = newspell;
	}

	internal static void Clear()
	{
		_spellsDict.Clear();
	}
}
