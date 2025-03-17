using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct CreateVertexBufferCommand
{
	public int type;

	public IntPtr id;

	public int VertexElementsCount;

	public int Size;

	public int DeclarationCount;

	public unsafe VertexDeclarationCommand* Declarations;

	public BufferUsage BufferUsage;

	public bool IsDynamic;
}
