using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct IndexedPrimitiveDataCommand
{
	public int type;

	public IntPtr texture_id;

	public PrimitiveType PrimitiveType;

	public int BaseVertex;

	public int MinVertexIndex;

	public int NumVertices;

	public int StartIndex;

	public int PrimitiveCount;
}
