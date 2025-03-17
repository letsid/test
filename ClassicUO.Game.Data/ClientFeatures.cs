using ClassicUO.Data;

namespace ClassicUO.Game.Data;

internal class ClientFeatures
{
	public CharacterListFlags Flags { get; private set; }

	public bool TooltipsEnabled { get; private set; } = true;

	public bool PopupEnabled { get; private set; }

	public bool PaperdollBooks { get; private set; }

	public uint MaxChars { get; private set; } = 5u;

	public void SetFlags(CharacterListFlags flags)
	{
		Flags = flags;
		if ((flags & CharacterListFlags.CLF_ONE_CHARACTER_SLOT) != 0)
		{
			MaxChars = 1u;
		}
		else if ((flags & CharacterListFlags.CLF_7_CHARACTER_SLOT) != 0)
		{
			MaxChars = 7u;
		}
		else if ((flags & CharacterListFlags.CLF_6_CHARACTER_SLOT) != 0)
		{
			MaxChars = 6u;
		}
		PopupEnabled = (flags & CharacterListFlags.CLF_CONTEXT_MENU) != 0;
		TooltipsEnabled = (flags & CharacterListFlags.CLF_PALADIN_NECROMANCER_TOOLTIPS) != 0 && Client.Version >= ClientVersion.CV_308Z;
		PaperdollBooks = (flags & CharacterListFlags.CLF_PALADIN_NECROMANCER_TOOLTIPS) != 0;
	}
}
