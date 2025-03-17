using System;

namespace ClassicUO.Renderer;

[Flags]
internal enum FontStyle : ushort
{
	None = 0,
	Solid = 1,
	Italic = 2,
	Indention = 4,
	BlackBorder = 8,
	Underline = 0x10,
	Fixed = 0x20,
	Cropped = 0x40,
	BQ = 0x80,
	ExtraHeight = 0x100,
	CropTexture = 0x200,
	ForcedUnicode = 0x400,
	NoPartialHue = 0x800
}
