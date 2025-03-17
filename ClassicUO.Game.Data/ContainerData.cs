using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Data;

internal struct ContainerData
{
	public ushort Graphic;

	public Rectangle Bounds;

	public ushort OpenSound;

	public ushort ClosedSound;

	public Rectangle MinimizerArea;

	public ushort IconizedGraphic;

	public int UncoloredHue;

	public int Flags;

	public ContainerData(ushort graphic, ushort sound, ushort closed, int x, int y, int w, int h, ushort iconizedgraphic = 0, int minimizerX = 0, int minimizerY = 0, int uncoloredHue = 0, int flags = 0)
	{
		Graphic = graphic;
		Bounds = new Rectangle(x, y, w, h);
		OpenSound = sound;
		ClosedSound = closed;
		MinimizerArea = ((minimizerX == 0 && minimizerY == 0) ? Rectangle.Empty : new Rectangle(minimizerX, minimizerY, 16, 16));
		IconizedGraphic = iconizedgraphic;
		UncoloredHue = uncoloredHue;
		Flags = flags;
	}
}
