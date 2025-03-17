using System;
using Microsoft.Xna.Framework;

namespace StbRectPackSharp;

internal class Packer : IDisposable
{
	private readonly StbRectPack.stbrp_context _context;

	public int Width => _context.width;

	public int Height => _context.height;

	public int PackeRectanglesCount { get; private set; }

	public unsafe Packer(int width = 256, int height = 256)
	{
		if (width <= 0)
		{
			throw new ArgumentOutOfRangeException("width");
		}
		if (height <= 0)
		{
			throw new ArgumentOutOfRangeException("height");
		}
		_context = new StbRectPack.stbrp_context(width);
		fixed (StbRectPack.stbrp_context* context = &_context)
		{
			StbRectPack.stbrp_init_target(context, width, height, _context.all_nodes, width);
		}
	}

	public void Dispose()
	{
		_context.Dispose();
	}

	public unsafe bool PackRect(int width, int height, out Rectangle packRectangle, int offset = 2)
	{
		StbRectPack.stbrp_rect stbrp_rect = default(StbRectPack.stbrp_rect);
		stbrp_rect.id = PackeRectanglesCount;
		stbrp_rect.w = width + offset;
		stbrp_rect.h = height + offset;
		StbRectPack.stbrp_rect stbrp_rect2 = stbrp_rect;
		int num;
		fixed (StbRectPack.stbrp_context* context = &_context)
		{
			num = StbRectPack.stbrp_pack_rects(context, &stbrp_rect2, 1);
		}
		if (num == 0)
		{
			packRectangle = Rectangle.Empty;
			return false;
		}
		packRectangle = new Rectangle(stbrp_rect2.x + (int)((float)offset / 2f), stbrp_rect2.y + (int)((float)offset / 2f), stbrp_rect2.w - offset, stbrp_rect2.h - offset);
		int packeRectanglesCount = PackeRectanglesCount + 1;
		PackeRectanglesCount = packeRectanglesCount;
		return true;
	}
}
