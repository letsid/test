using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct CreateIndexBufferCommand
{
	public int type;

	public IntPtr id;

	public IndexElementSize IndexElementSize;

	public int IndexCount;

	public BufferUsage BufferUsage;

	public bool IsDynamic;
}
