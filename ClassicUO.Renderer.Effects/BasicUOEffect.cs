using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer.Effects;

internal class BasicUOEffect : Effect
{
	public EffectParameter MatrixTransform { get; }

	public EffectParameter WorldMatrix { get; }

	public EffectParameter Viewport { get; }

	public EffectParameter Brighlight { get; }

	public EffectPass Pass { get; }

	public BasicUOEffect(GraphicsDevice graphicsDevice)
		: base(graphicsDevice, Resources.IsometricEffect)
	{
		MatrixTransform = base.Parameters["MatrixTransform"];
		WorldMatrix = base.Parameters["WorldMatrix"];
		Viewport = base.Parameters["Viewport"];
		Brighlight = base.Parameters["Brightlight"];
		base.CurrentTechnique = base.Techniques["HueTechnique"];
		Pass = base.CurrentTechnique.Passes[0];
	}
}
