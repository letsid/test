using System;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Batching;

internal struct CreateSamplerStateCommand
{
	public int type;

	public IntPtr id;

	public int index;

	public TextureFilter TextureFilter;

	public TextureAddressMode AddressU;

	public TextureAddressMode AddressV;

	public TextureAddressMode AddressW;

	public int MaxAnisotropy;

	public int MaxMipLevel;

	public float MipMapLevelOfDetailBias;
}
