using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal class XBREffect : Effect
{
	public EffectParameter MatrixTransform { get; }

	public EffectParameter TextureSize { get; }

	public XBREffect(GraphicsDevice graphicsDevice)
		: base(graphicsDevice, Resources.xBREffect)
	{
		MatrixTransform = base.Parameters["MatrixTransform"];
		TextureSize = base.Parameters["textureSize"];
	}
}
