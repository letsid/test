using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum STATIC_TILES_FILTER_FLAGS : byte
{
	STFF_CAVE = 1,
	STFF_STUMP = 2,
	STFF_STUMP_HATCHED = 4,
	STFF_VEGETATION = 8,
	STFF_WATER = 0x10
}
