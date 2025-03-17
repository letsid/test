using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsNinjitsu
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsNinjitsu()
	{
		SpellBookName = SpellBookType.Ninjitsu.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Focus Attack", 501, 21280, string.Empty, 20, 60, TargetType.Harmful, Reagents.None)
			},
			{
				2,
				new SpellDefinition("Death Strike", 502, 21281, string.Empty, 30, 85, TargetType.Harmful, Reagents.None)
			},
			{
				3,
				new SpellDefinition("Animal Form", 503, 21282, string.Empty, 0, 10, TargetType.Beneficial, Reagents.None)
			},
			{
				4,
				new SpellDefinition("Ki Attack", 504, 21283, string.Empty, 25, 80, TargetType.Harmful, Reagents.None)
			},
			{
				5,
				new SpellDefinition("Surprise Attack", 505, 21284, string.Empty, 20, 30, TargetType.Harmful, Reagents.None)
			},
			{
				6,
				new SpellDefinition("Backstab", 506, 21285, string.Empty, 30, 20, TargetType.Harmful, Reagents.None)
			},
			{
				7,
				new SpellDefinition("Shadowjump", 507, 21286, string.Empty, 15, 50, TargetType.Neutral, Reagents.None)
			},
			{
				8,
				new SpellDefinition("Mirror Image", 508, 21287, string.Empty, 10, 40, TargetType.Neutral, Reagents.None)
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
