using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsChivalry
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsChivalry()
	{
		SpellBookName = SpellBookType.Chivalry.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Cleanse by Fire", 201, 20736, 20736, "Expor Flamus", 10, 5, 10, TargetType.Beneficial, Reagents.None)
			},
			{
				2,
				new SpellDefinition("Close Wounds", 202, 20737, 20737, "Obsu Vulni", 10, 0, 10, TargetType.Beneficial, Reagents.None)
			},
			{
				3,
				new SpellDefinition("Consecrate Weapon", 203, 20738, 20738, "Consecrus Arma", 10, 15, 10, TargetType.Neutral, Reagents.None)
			},
			{
				4,
				new SpellDefinition("Dispel Evil", 204, 20739, 20739, "Dispiro Malas", 10, 35, 10, TargetType.Neutral, Reagents.None)
			},
			{
				5,
				new SpellDefinition("Divine Fury", 205, 20740, 20740, "Divinum Furis", 10, 25, 10, TargetType.Neutral, Reagents.None)
			},
			{
				6,
				new SpellDefinition("Enemy of One", 206, 20741, 20741, "Forul Solum", 20, 45, 10, TargetType.Neutral, Reagents.None)
			},
			{
				7,
				new SpellDefinition("Holy Light", 207, 20742, 20742, "Augus Luminos", 20, 55, 10, TargetType.Harmful, Reagents.None)
			},
			{
				8,
				new SpellDefinition("Noble Sacrifice", 208, 20743, 20743, "Dium Prostra", 20, 65, 30, TargetType.Beneficial, Reagents.None)
			},
			{
				9,
				new SpellDefinition("Remove Curse", 209, 20744, 20744, "Extermo Vomica", 20, 5, 10, TargetType.Beneficial, Reagents.None)
			},
			{
				10,
				new SpellDefinition("Sacred Journey", 210, 20745, 20745, "Sanctum Viatas", 20, 5, 10, TargetType.Neutral, Reagents.None)
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
