using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum Ability : ushort
{
	Invalid = 0xFF,
	None = 0,
	ArmorIgnore = 1,
	BleedAttack = 2,
	ConcussionBlow = 3,
	CrushingBlow = 4,
	Disarm = 5,
	Dismount = 6,
	DoubleStrike = 7,
	InfectiousStrike = 8,
	MortalStrike = 9,
	MovingShot = 0xA,
	ParalyzingBlow = 0xB,
	ShadowStrike = 0xC,
	WhirlwindAttack = 0xD,
	RidingSwipe = 0xE,
	FrenziedWhirlwind = 0xF,
	Block = 0x10,
	DefenseMastery = 0x11,
	NerveStrike = 0x12,
	TalonStrike = 0x13,
	Feint = 0x14,
	DualWield = 0x15,
	DoubleShot = 0x16,
	ArmorPierce = 0x17,
	Bladeweave = 0x18,
	ForceArrow = 0x19,
	LightningArrow = 0x1A,
	PsychicAttack = 0x1B,
	SerpentArrow = 0x1C,
	ForceOfNature = 0x1D,
	InfusedThrow = 0x1E,
	MysticArc = 0x1F
}
