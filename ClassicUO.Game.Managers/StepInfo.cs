namespace ClassicUO.Game.Managers;

internal struct StepInfo
{
	public byte Direction;

	public byte OldDirection;

	public byte Sequence;

	public bool Accepted;

	public bool Running;

	public bool NoRotation;

	public long Timer;

	public ushort X;

	public ushort Y;

	public sbyte Z;
}
