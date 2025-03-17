namespace ClassicUO.Game.Managers;

internal struct CustomBuildObject
{
	public ushort Graphic;

	public int X;

	public int Y;

	public int Z;

	public CustomBuildObject(ushort graphic)
	{
		Graphic = graphic;
		X = (Y = (Z = 0));
	}
}
