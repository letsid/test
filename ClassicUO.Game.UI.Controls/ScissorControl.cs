using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls;

internal class ScissorControl : Control
{
	public bool DoScissor;

	public ScissorControl(bool enabled, int x, int y, int width, int height)
		: this(enabled)
	{
		base.X = x;
		base.Y = y;
		base.Width = width;
		base.Height = height;
	}

	public ScissorControl(bool enabled)
	{
		CanMove = false;
		AcceptMouseInput = false;
		AcceptKeyboardInput = false;
		base.Alpha = 1f;
		base.WantUpdateSize = false;
		DoScissor = enabled;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (DoScissor)
		{
			batcher.ClipBegin(x, y, base.Width, base.Height);
		}
		else
		{
			batcher.ClipEnd();
		}
		return true;
	}
}
