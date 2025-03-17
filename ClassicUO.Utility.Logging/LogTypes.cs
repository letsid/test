using System;

namespace ClassicUO.Utility.Logging;

[Flags]
internal enum LogTypes : byte
{
	None = 0,
	Info = 1,
	Debug = 2,
	Trace = 4,
	Warning = 8,
	Error = 0x10,
	Panic = 0x20,
	Table = 0x30,
	All = byte.MaxValue
}
