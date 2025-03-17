using System.Collections.Generic;
using ClassicUO.Game.Managers;

namespace ClassicUO.Game.Data;

internal static class SpellsMysticism
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	static SpellsMysticism()
	{
		SpellBookName = SpellBookType.Mysticism.ToString();
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Nether Bolt", 678, 24000, "In Corp Ylem", 4, 0, TargetType.Harmful, Reagents.BlackPearl, Reagents.SulfurousAsh)
			},
			{
				2,
				new SpellDefinition("Healing Stone", 679, 24001, "Kal In Mani", 4, 0, TargetType.Neutral, Reagents.Bone, Reagents.Garlic, Reagents.Ginseng, Reagents.SpidersSilk)
			},
			{
				3,
				new SpellDefinition("Purge Magic", 680, 24002, "An Ort Sanct", 6, 8, TargetType.Beneficial, Reagents.FertileDirt, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				4,
				new SpellDefinition("Enchant", 681, 24003, "In Ort Ylem", 6, 8, TargetType.Neutral, Reagents.SpidersSilk, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				5,
				new SpellDefinition("Sleep", 682, 24004, "In Zu", 8, 20, TargetType.Harmful, Reagents.Nightshade, Reagents.SpidersSilk, Reagents.BlackPearl)
			},
			{
				6,
				new SpellDefinition("Eagle Strike", 683, 24005, "Kal Por Xen", 9, 20, TargetType.Harmful, Reagents.Bloodmoss, Reagents.Bone, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				7,
				new SpellDefinition("Animated Weapon", 684, 24006, "In Jux Por Ylem", 11, 33, TargetType.Neutral, Reagents.Bone, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.Nightshade)
			},
			{
				8,
				new SpellDefinition("Stone Form", 685, 24007, "In Rel Ylem", 11, 33, TargetType.Neutral, Reagents.Bloodmoss, Reagents.FertileDirt, Reagents.Garlic)
			},
			{
				9,
				new SpellDefinition("Spell Trigger", 686, 24008, "In Vas Ort Ex", 14, 45, TargetType.Neutral, Reagents.DragonsBlood, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				10,
				new SpellDefinition("Mass Sleep", 687, 24009, "Vas Zu", 14, 45, TargetType.Harmful, Reagents.Ginseng, Reagents.Nightshade, Reagents.SpidersSilk)
			},
			{
				11,
				new SpellDefinition("Cleansing Winds", 688, 24010, "In Vas Mani Hur", 20, 58, TargetType.Beneficial, Reagents.DragonsBlood, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot)
			},
			{
				12,
				new SpellDefinition("Bombard", 689, 24011, "Corp Por Ylem", 20, 58, TargetType.Harmful, Reagents.Bloodmoss, Reagents.DragonsBlood, Reagents.Garlic, Reagents.SulfurousAsh)
			},
			{
				13,
				new SpellDefinition("Spell Plague", 690, 24012, "Vas Rel Jux Ort", 40, 70, TargetType.Harmful, Reagents.DemonBone, Reagents.DragonsBlood, Reagents.Nightshade, Reagents.SulfurousAsh)
			},
			{
				14,
				new SpellDefinition("Hail Storm", 691, 24013, "Kal Des Ylem", 50, 70, TargetType.Harmful, Reagents.DragonsBlood, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				15,
				new SpellDefinition("Nether Cyclone", 692, 24014, "Grav Hur", 50, 83, TargetType.Harmful, Reagents.MandrakeRoot, Reagents.Nightshade, Reagents.SulfurousAsh, Reagents.Bloodmoss)
			},
			{
				16,
				new SpellDefinition("Rising Colossus", 693, 24015, "Kal Vas Xen Corp Ylem", 50, 83, TargetType.Neutral, Reagents.DemonBone, Reagents.DragonsBlood, Reagents.FertileDirt, Reagents.Nightshade)
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
