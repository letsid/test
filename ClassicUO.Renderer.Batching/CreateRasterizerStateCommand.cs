using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct CreateRasterizerStateCommand
{
	public int type;

	public IntPtr id;

	public CullMode CullMode;

	public FillMode FillMode;

	public float DepthBias;

	public bool MultiSample;

	public bool ScissorTestEnabled;

	public float SlopeScaleDepthBias;
}
