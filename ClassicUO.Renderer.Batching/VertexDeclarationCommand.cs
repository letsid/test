using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct VertexDeclarationCommand
{
	public int Offset;

	public VertexElementFormat Format;

	public VertexElementUsage Usage;

	public int UsageIndex;
}
