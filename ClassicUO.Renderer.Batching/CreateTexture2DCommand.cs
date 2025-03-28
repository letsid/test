using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct CreateTexture2DCommand
{
	public int type;

	public IntPtr id;

	public SurfaceFormat Format;

	public int Width;

	public int Height;

	public int MipLevels;

	public bool IsRenderTarget;
}
