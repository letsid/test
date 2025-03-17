using System;

namespace ClassicUO.IO.Resources;

[Flags]
internal enum TileFlag : ulong
{
	None = 0uL,
	Background = 1uL,
	Weapon = 2uL,
	Transparent = 4uL,
	Translucent = 8uL,
	Wall = 0x10uL,
	Damaging = 0x20uL,
	Impassable = 0x40uL,
	Wet = 0x80uL,
	Unknown1 = 0x100uL,
	Surface = 0x200uL,
	Bridge = 0x400uL,
	Generic = 0x800uL,
	Window = 0x1000uL,
	NoShoot = 0x2000uL,
	ArticleA = 0x4000uL,
	ArticleAn = 0x8000uL,
	Internal = 0x10000uL,
	Foliage = 0x20000uL,
	PartialHue = 0x40000uL,
	NoHouse = 0x80000uL,
	Map = 0x100000uL,
	Container = 0x200000uL,
	Wearable = 0x400000uL,
	LightSource = 0x800000uL,
	Animation = 0x1000000uL,
	NoDiagonal = 0x2000000uL,
	Unknown2 = 0x4000000uL,
	Armor = 0x8000000uL,
	Roof = 0x10000000uL,
	Door = 0x20000000uL,
	StairBack = 0x40000000uL,
	StairRight = 0x80000000uL,
	AlphaBlend = 0x100000000uL,
	UseNewArt = 0x200000000uL,
	ArtUsed = 0x400000000uL,
	NoShadow = 0x1000000000uL,
	PixelBleed = 0x2000000000uL,
	PlayAnimOnce = 0x4000000000uL,
	MultiMovable = 0x10000000000uL
}
