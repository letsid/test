using System;
using System.Collections.Generic;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class CheckerTrans : Control
{
	private static readonly Lazy<DepthStencilState> _checkerStencil = new Lazy<DepthStencilState>(() => new DepthStencilState
	{
		DepthBufferEnable = false,
		StencilEnable = true,
		StencilFunction = CompareFunction.Always,
		ReferenceStencil = 1,
		StencilMask = 1,
		StencilFail = StencilOperation.Keep,
		StencilDepthBufferFail = StencilOperation.Keep,
		StencilPass = StencilOperation.Replace
	});

	private static readonly Lazy<BlendState> _checkerBlend = new Lazy<BlendState>(() => new BlendState
	{
		ColorWriteChannels = ColorWriteChannels.None
	});

	public CheckerTrans(List<string> parts)
	{
		base.X = int.Parse(parts[1]);
		base.Y = int.Parse(parts[2]);
		base.Width = int.Parse(parts[3]);
		base.Height = int.Parse(parts[4]);
		AcceptMouseInput = false;
		base.IsFromServer = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, 0.5f);
		batcher.Draw(SolidColorTextureCache.GetTexture(Color.Black), new Rectangle(x, y, base.Width, base.Height), hueVector);
		return true;
	}
}
