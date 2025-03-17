namespace ClassicUO.Game.Managers;

internal class MultiTargetInfo
{
	public readonly ushort XOff;

	public readonly ushort YOff;

	public readonly ushort ZOff;

	public readonly ushort Model;

	public readonly ushort Hue;

	public MultiTargetInfo(ushort model, ushort x, ushort y, ushort z, ushort hue)
	{
		Model = model;
		XOff = x;
		YOff = y;
		ZOff = z;
		Hue = hue;
	}
}
