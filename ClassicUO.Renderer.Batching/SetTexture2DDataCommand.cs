using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct SetTexture2DDataCommand
{
	public int type;

	public IntPtr id;

	public SurfaceFormat format;

	public int x;

	public int y;

	public int width;

	public int height;

	public int level;

	public IntPtr data;

	public int data_length;
}
