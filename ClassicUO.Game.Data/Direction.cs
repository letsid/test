using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum Direction : byte
{
	North = 0,
	Right = 1,
	East = 2,
	Down = 3,
	South = 4,
	Left = 5,
	West = 6,
	Up = 7,
	Mask = 7,
	Running = 0x80,
	NONE = 0xED
}
