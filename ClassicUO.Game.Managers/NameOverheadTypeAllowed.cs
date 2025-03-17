using System;

namespace ClassicUO.Game.Managers;

[Flags]
internal enum NameOverheadTypeAllowed
{
	All = 0,
	Mobiles = 1,
	Items = 2,
	Corpses = 3,
	MobilesCorpses = 3
}
