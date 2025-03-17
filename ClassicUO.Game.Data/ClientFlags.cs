using System;

namespace ClassicUO.Game.Data;

[Flags]
internal enum ClientFlags : uint
{
	CF_T2A = 0u,
	CF_RE = 1u,
	CF_TD = 2u,
	CF_LBR = 4u,
	CF_AOS = 8u,
	CF_SE = 0x10u,
	CF_SA = 0x20u,
	CF_UO3D = 0x40u,
	CF_RESERVED = 0x80u,
	CF_3D = 0x100u,
	CF_UNDEFINED = 0xFFFFu
}
