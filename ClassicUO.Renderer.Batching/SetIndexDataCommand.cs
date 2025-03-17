using System;

namespace ClassicUO.Renderer.Batching;

internal struct SetIndexDataCommand
{
	public int type;

	public IntPtr id;

	public IntPtr indices_buffer_ptr;

	public int indices_buffer_length;
}
