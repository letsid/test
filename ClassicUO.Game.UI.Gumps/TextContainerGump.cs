using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Gumps;

internal abstract class TextContainerGump : Gump
{
	public TextRenderer TextRenderer { get; } = new TextRenderer();

	protected TextContainerGump(uint local, uint server)
		: base(local, server)
	{
	}

	public void AddText(TextObject msg)
	{
		if (msg != null)
		{
			msg.Time = Time.Ticks + 4000;
			TextRenderer.AddMessage(msg);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		TextRenderer.Update(totalTime, frameTime);
	}

	public override void Dispose()
	{
		TextRenderer.UnlinkD();
		base.Dispose();
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		TextRenderer.ProcessWorldText(doit: true);
		TextRenderer.Draw(batcher, x, y, -1, isGump: true);
		return true;
	}
}
