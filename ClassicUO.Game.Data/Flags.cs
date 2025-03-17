using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum Flags : byte
{
	None = 0,
	Frozen = 1,
	Female = 2,
	Poisoned = 4,
	Flying = 4,
	YellowBar = 8,
	IgnoreMobiles = 0x10,
	Movable = 0x20,
	WarMode = 0x40,
	Hidden = 0x80
}
