namespace ClassicUO.IO.Resources;

internal class SkillEntry
{
	internal enum HardCodedName
	{
		Alchemy,
		Anatomy,
		AnimalLore,
		ItemID,
		ArmsLore,
		Parrying,
		Begging,
		Blacksmith,
		Bowcraft,
		Peacemaking,
		Camping,
		Carpentry,
		Cartography,
		Cooking,
		DetectHidden,
		Enticement,
		EvaluateIntelligence,
		Healing,
		Fishing,
		ForensicEvaluation,
		Herding,
		Hiding,
		Provocation,
		Inscription,
		Lockpicking,
		Magery,
		ResistingSpells,
		Tactics,
		Snooping,
		Musicanship,
		Poisoning,
		Archery,
		SpiritSpeak,
		Stealing,
		Tailoring,
		AnimalTaming,
		TasteIdentification,
		Tinkering,
		Tracking,
		Veterinary,
		Swordsmanship,
		MaceFighting,
		Fencing,
		Wrestling,
		Lumberjacking,
		Mining,
		Meditation,
		Stealth,
		Disarm,
		Necromancy,
		Focus,
		Chivalry,
		Bushido,
		Ninjitsu,
		Spellweaving
	}

	public bool HasAction;

	public readonly int Index;

	public string Name;

	public SkillEntry(int index, string name, bool hasAction)
	{
		Index = index;
		Name = name;
		HasAction = hasAction;
	}

	public override string ToString()
	{
		return Name;
	}
}
