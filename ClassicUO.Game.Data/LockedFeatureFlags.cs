using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum LockedFeatureFlags : uint
{
	TheSecondAge = 1u,
	Renaissance = 2u,
	ThirdDawn = 4u,
	LordBlackthornsRevenge = 8u,
	AgeOfShadows = 0x10u,
	CharacterSlot6 = 0x20u,
	SamuraiEmpire = 0x40u,
	MondainsLegacy = 0x80u,
	Splash8 = 0x100u,
	Splash9 = 0x200u,
	TenthAge = 0x400u,
	MoreStorage = 0x800u,
	CharacterSlot7 = 0x1000u,
	TenthAgeFaces = 0x2000u,
	TrialAccount = 0x4000u,
	EleventhAge = 0x8000u,
	StygianAbyss = 0x10000u,
	HighSeas = 0x20000u,
	GothicHousing = 0x40000u,
	RusticHousing = 0x80000u
}
