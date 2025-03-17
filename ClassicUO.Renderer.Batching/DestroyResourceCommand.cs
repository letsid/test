using System;

namespace ClassicUO.Renderer.Batching;

internal struct DestroyResourceCommand
{
	public int type;

	public IntPtr id;
}
