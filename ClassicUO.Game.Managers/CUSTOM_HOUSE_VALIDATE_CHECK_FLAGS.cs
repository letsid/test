using System;

namespace ClassicUO.Game.Managers;

[Flags]
internal enum CUSTOM_HOUSE_VALIDATE_CHECK_FLAGS
{
	CHVCF_TOP = 1,
	CHVCF_BOTTOM = 2,
	CHVCF_N = 4,
	CHVCF_E = 8,
	CHVCF_S = 0x10,
	CHVCF_W = 0x20,
	CHVCF_DIRECT_SUPPORT = 0x40,
	CHVCF_CANGO_W = 0x80,
	CHVCF_CANGO_N = 0x100
}
