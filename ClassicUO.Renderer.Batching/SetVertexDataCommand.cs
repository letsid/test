using System;

namespace ClassicUO.Renderer.Batching;

internal struct SetVertexDataCommand
{
	public int type;

	public IntPtr id;

	public IntPtr vertex_buffer_ptr;

	public int vertex_buffer_length;
}
