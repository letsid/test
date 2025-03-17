using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Utility;

namespace ClassicUO.Game.Data;

internal static class SpellsMagery
{
	private static readonly Dictionary<int, SpellDefinition> _spellsDict;

	private static string[] _spRegsChars;

	public static string SpellBookName { get; set; }

	public static IReadOnlyDictionary<int, SpellDefinition> GetAllSpells => _spellsDict;

	internal static int MaxSpellCount => _spellsDict.Count;

	public static string[] CircleNames { get; }

	public static string[] SpecialReagentsChars
	{
		get
		{
			if (_spRegsChars == null)
			{
				_spRegsChars = new string[_spellsDict.Max((KeyValuePair<int, SpellDefinition> o) => o.Key)];
				for (int num = _spRegsChars.Length; num > 0; num--)
				{
					if (_spellsDict.TryGetValue(num, out var value))
					{
						_spRegsChars[num - 1] = StringHelper.RemoveUpperLowerChars(value.PowerWords);
					}
					else
					{
						_spRegsChars[num - 1] = string.Empty;
					}
				}
			}
			return _spRegsChars;
		}
	}

	static SpellsMagery()
	{
		SpellBookName = SpellBookType.Magery.ToString();
		CircleNames = new string[8] { "First Circle", "Second Circle", "Third Circle", "Fourth Circle", "Fifth Circle", "Sixth Circle", "Seventh Circle", "Eighth Circle" };
		_spellsDict = new Dictionary<int, SpellDefinition>
		{
			{
				1,
				new SpellDefinition("Clumsy", 1, 7000, "Uus Jux", TargetType.Harmful, Reagents.Bloodmoss, Reagents.Nightshade)
			},
			{
				2,
				new SpellDefinition("Create Food", 2, 7001, "In Mani Ylem", TargetType.Neutral, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot)
			},
			{
				3,
				new SpellDefinition("Feeblemind", 3, 7002, "Rel Wis", TargetType.Harmful, Reagents.Nightshade, Reagents.Ginseng)
			},
			{
				4,
				new SpellDefinition("Heal", 4, 7003, "In Mani", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng, Reagents.SpidersSilk)
			},
			{
				5,
				new SpellDefinition("Magic Arrow", 5, 7004, "In Por Ylem", TargetType.Harmful, Reagents.SulfurousAsh)
			},
			{
				6,
				new SpellDefinition("Night Sight", 6, 7005, "In Lor", TargetType.Beneficial, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				7,
				new SpellDefinition("Reactive Armor", 7, 7006, "Flam Sanct", TargetType.Beneficial, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				8,
				new SpellDefinition("Weaken", 8, 7007, "Des Mani", TargetType.Harmful, Reagents.Garlic, Reagents.Nightshade)
			},
			{
				9,
				new SpellDefinition("Agility", 9, 7008, "Ex Uus", TargetType.Beneficial, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				10,
				new SpellDefinition("Cunning", 10, 7009, "Uus Wis", TargetType.Beneficial, Reagents.Nightshade, Reagents.MandrakeRoot)
			},
			{
				11,
				new SpellDefinition("Cure", 11, 7010, "An Nox", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng)
			},
			{
				12,
				new SpellDefinition("Harm", 12, 7011, "An Mani", TargetType.Harmful, Reagents.Nightshade, Reagents.SpidersSilk)
			},
			{
				13,
				new SpellDefinition("Magic Trap", 13, 7012, "In Jux", TargetType.Neutral, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				14,
				new SpellDefinition("Magic Untrap", 14, 7013, "An Jux", TargetType.Neutral, Reagents.Bloodmoss, Reagents.SulfurousAsh)
			},
			{
				15,
				new SpellDefinition("Protection", 15, 7014, "Uus Sanct", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng, Reagents.SulfurousAsh)
			},
			{
				16,
				new SpellDefinition("Strength", 16, 7015, "Uus Mani", TargetType.Beneficial, Reagents.MandrakeRoot, Reagents.Nightshade)
			},
			{
				17,
				new SpellDefinition("Bless", 17, 7016, "Rel Sanct", TargetType.Beneficial, Reagents.Garlic, Reagents.MandrakeRoot)
			},
			{
				18,
				new SpellDefinition("Fireball", 18, 7017, "Vas Flam", TargetType.Harmful, default(Reagents))
			},
			{
				19,
				new SpellDefinition("Magic Lock", 19, 7018, "An Por", TargetType.Neutral, Reagents.Bloodmoss, Reagents.Garlic, Reagents.SulfurousAsh)
			},
			{
				20,
				new SpellDefinition("Poison", 20, 7019, "In Nox", TargetType.Harmful, Reagents.Nightshade)
			},
			{
				21,
				new SpellDefinition("Telekinesis", 21, 7020, "Ort Por Ylem", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				22,
				new SpellDefinition("Teleport", 22, 7021, "Rel Por", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				23,
				new SpellDefinition("Unlock", 23, 7022, "Ex Por", TargetType.Neutral, Reagents.Bloodmoss, Reagents.SulfurousAsh)
			},
			{
				24,
				new SpellDefinition("Wall of Stone", 24, 7023, "In Sanct Ylem", TargetType.Neutral, Reagents.Bloodmoss, Reagents.Garlic)
			},
			{
				25,
				new SpellDefinition("Arch Cure", 25, 7024, "Vas An Nox", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot)
			},
			{
				26,
				new SpellDefinition("Arch Protection", 26, 7025, "Vas Uus Sanct", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				27,
				new SpellDefinition("Curse", 27, 7026, "Des Sanct", TargetType.Harmful, Reagents.Garlic, Reagents.Nightshade, Reagents.SulfurousAsh)
			},
			{
				28,
				new SpellDefinition("Fire Field", 28, 7027, "In Flam Grav", TargetType.Neutral, Reagents.BlackPearl, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				29,
				new SpellDefinition("Greater Heal", 29, 7028, "In Vas Mani", TargetType.Beneficial, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				30,
				new SpellDefinition("Lightning", 30, 7029, "Por Ort Grav", TargetType.Harmful, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				31,
				new SpellDefinition("Mana Drain", 31, 7030, "Ort Rel", TargetType.Harmful, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				32,
				new SpellDefinition("Recall", 32, 7031, "Kal Ort Por", TargetType.Neutral, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				33,
				new SpellDefinition("Blade Spirits", 33, 7032, "In Jux Hur Ylem", TargetType.Neutral, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.Nightshade)
			},
			{
				34,
				new SpellDefinition("Dispel Field", 34, 7033, "An Grav", TargetType.Neutral, Reagents.BlackPearl, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				35,
				new SpellDefinition("Incognito", 35, 7034, "Kal In Ex", TargetType.Neutral, Reagents.Bloodmoss, Reagents.Garlic, Reagents.Nightshade)
			},
			{
				36,
				new SpellDefinition("Magic Reflection", 36, 7035, "In Jux Sanct", TargetType.Beneficial, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				37,
				new SpellDefinition("Mind Blast", 37, 7036, "Por Corp Wis", TargetType.Harmful, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.Nightshade, Reagents.SulfurousAsh)
			},
			{
				38,
				new SpellDefinition("Paralyze", 38, 7037, "An Ex Por", TargetType.Harmful, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				39,
				new SpellDefinition("Poison Field", 39, 7038, "In Nox Grav", TargetType.Neutral, Reagents.BlackPearl, Reagents.Nightshade, Reagents.SpidersSilk)
			},
			{
				40,
				new SpellDefinition("Summon Creature", 40, 7039, "Kal Xen", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				41,
				new SpellDefinition("Dispel", 41, 7040, "An Ort", TargetType.Neutral, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				42,
				new SpellDefinition("Energy Bolt", 42, 7041, "Corp Por", TargetType.Harmful, Reagents.BlackPearl, Reagents.Nightshade)
			},
			{
				43,
				new SpellDefinition("Explosion", 43, 7042, "Vas Ort Flam", TargetType.Harmful, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				44,
				new SpellDefinition("Invisibility", 44, 7043, "An Lor Xen", TargetType.Beneficial, Reagents.Bloodmoss, Reagents.Nightshade)
			},
			{
				45,
				new SpellDefinition("Mark", 45, 7044, "Kal Por Ylem", TargetType.Neutral, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot)
			},
			{
				46,
				new SpellDefinition("Mass Curse", 46, 7045, "Vas Des Sanct", TargetType.Harmful, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.Nightshade, Reagents.SulfurousAsh)
			},
			{
				47,
				new SpellDefinition("Paralyze Field", 47, 7046, "In Ex Grav", TargetType.Neutral, Reagents.BlackPearl, Reagents.Ginseng, Reagents.SpidersSilk)
			},
			{
				48,
				new SpellDefinition("Reveal", 48, 7047, "Wis Quas", TargetType.Neutral, Reagents.Bloodmoss, Reagents.SulfurousAsh)
			},
			{
				49,
				new SpellDefinition("Chain Lightning", 49, 7048, "Vas Ort Grav", TargetType.Harmful, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				50,
				new SpellDefinition("Energy Field", 50, 7049, "In Sanct Grav", TargetType.Neutral, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				51,
				new SpellDefinition("Flamestrike", 51, 7050, "Kal Vas Flam", TargetType.Harmful, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				52,
				new SpellDefinition("Gate Travel", 52, 7051, "Vas Rel Por", TargetType.Neutral, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				53,
				new SpellDefinition("Mana Vampire", 53, 7052, "Ort Sanct", TargetType.Harmful, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				54,
				new SpellDefinition("Mass Dispel", 54, 7053, "Vas An Ort", TargetType.Neutral, Reagents.BlackPearl, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				55,
				new SpellDefinition("Meteor Swarm", 55, 7054, "Flam Kal Des Ylem", TargetType.Harmful, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				56,
				new SpellDefinition("Polymorph", 56, 7055, "Vas Ylem Rel", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				57,
				new SpellDefinition("Earthquake", 57, 7056, "In Vas Por", TargetType.Harmful, Reagents.Bloodmoss, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SulfurousAsh)
			},
			{
				58,
				new SpellDefinition("Energy Vortex", 58, 7057, "Vas Corp Por", TargetType.Neutral, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.Nightshade)
			},
			{
				59,
				new SpellDefinition("Resurrection", 59, 7058, "An Corp", TargetType.Beneficial, Reagents.Bloodmoss, Reagents.Ginseng, Reagents.Garlic)
			},
			{
				60,
				new SpellDefinition("Air Elemental", 60, 7059, "Kal Vas Xen Hur", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				61,
				new SpellDefinition("Summon Daemon", 61, 7060, "Kal Vas Xen Corp", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				62,
				new SpellDefinition("Earth Elemental", 62, 7061, "Kal Vas Xen Ylem", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			},
			{
				63,
				new SpellDefinition("Fire Elemental", 63, 7062, "Kal Vas Xen Flam", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh)
			},
			{
				64,
				new SpellDefinition("Water Elemental", 64, 7063, "Kal Vas Xen An Flam", TargetType.Neutral, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk)
			}
		};
	}

	public static SpellDefinition GetSpell(int index)
	{
		if (!_spellsDict.TryGetValue(index, out var value))
		{
			return SpellDefinition.EmptySpell;
		}
		return value;
	}

	public static void SetSpell(int id, in SpellDefinition newspell)
	{
		_spRegsChars = null;
		_spellsDict[id] = newspell;
	}

	internal static void Clear()
	{
		_spellsDict.Clear();
	}
}
