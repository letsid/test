using System;

namespace ClassicUO.Renderer.Batching;

internal struct CreateEffectCommand
{
	public int type;

	public IntPtr id;

	public IntPtr code;

	public int Length;
}
