using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsBushido
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsBushido()
	{
		SpellBookName = SpellBookType.Bushido.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Honorable Execution", 401, 21536, string.Empty, 0, 25, TargetType.Harmful, Reagents.None)
			},
			{
				2,
				new SpellDefinition("Confidence", 402, 21537, string.Empty, 10, 25, TargetType.Beneficial, Reagents.None)
			},
			{
				3,
				new SpellDefinition("Evasion", 403, 21538, string.Empty, 10, 60, TargetType.Beneficial, Reagents.None)
			},
			{
				4,
				new SpellDefinition("Counter Attack", 404, 21539, string.Empty, 5, 40, TargetType.Harmful, Reagents.None)
			},
			{
				5,
				new SpellDefinition("Lightning Strike", 405, 21540, string.Empty, 10, 50, TargetType.Harmful, Reagents.None)
			},
			{
				6,
				new SpellDefinition("Momentum Strike", 406, 21541, string.Empty, 10, 70, TargetType.Harmful, Reagents.None)
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
