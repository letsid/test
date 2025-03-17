using ClassicUO.Game.Data;

namespace ClassicUO.Game.Managers;

internal struct CorpseInfo
{
	public uint CorpseSerial;

	public uint ObjectSerial;

	public Direction Direction;

	public bool IsRunning;

	public CorpseInfo(uint corpseSerial, uint objectSerial, Direction direction, bool isRunning)
	{
		CorpseSerial = corpseSerial;
		ObjectSerial = objectSerial;
		Direction = direction;
		IsRunning = isRunning;
	}
}
