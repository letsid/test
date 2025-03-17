using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum MessageType : byte
{
	Regular = 0,
	System = 1,
	Emote = 2,
	Limit3Spell = 3,
	Object = 4,
	Nothing = 5,
	Label = 6,
	Focus = 7,
	Whisper = 8,
	Yell = 9,
	Spell = 0xA,
	Guild = 0xD,
	Alliance = 0xE,
	Command = 0xF,
	Encoded = 0xC0,
	Party = byte.MaxValue
}
