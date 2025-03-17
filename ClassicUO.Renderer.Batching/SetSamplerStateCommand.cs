using System;

namespace ClassicUO.Renderer.Batching;

internal struct SetSamplerStateCommand
{
	public int type;

	public IntPtr id;

	public int index;
}
