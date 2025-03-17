namespace ClassicUO.Game.Managers;

internal class WMapEntity
{
	public bool IsGuild;

	public uint LastUpdate;

	public string Name;

	public readonly uint Serial;

	public int X;

	public int Y;

	public int HP;

	public int Map;

	public WMapEntity(uint serial)
	{
		Serial = serial;
	}
}
