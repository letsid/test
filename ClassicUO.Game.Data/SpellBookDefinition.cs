namespace ClassicUO.Game.Data;

internal static class SpellBookDefinition
{
	private const int MAGERY_SPELLS_OFFSET = 61;

	private const int NECRO_SPELLS_OFFSET = 125;

	private const int CHIVAL_SPELLS_OFFSETS = 142;

	private const int BUSHIDO_SPELLS_OFFSETS = 152;

	private const int NINJITSU_SPELLS_OFFSETS = 158;

	private const int SPELLWEAVING_SPELLS_OFFSETS = 166;

	private const int MYSTICISM_SPELLS_OFFSETS = 182;

	private const int MASTERY_SPELLS_OFFSETS = 198;

	public static int GetSpellsGroup(int spellID)
	{
		switch (spellID / 100)
		{
		case 0:
			return 61;
		case 1:
			return 125;
		case 2:
			return 142;
		case 4:
			return 152;
		case 5:
			return 158;
		case 6:
			if (spellID > 620)
			{
				return 182;
			}
			return 166;
		case 7:
			return 198;
		default:
			return -1;
		}
	}
}
