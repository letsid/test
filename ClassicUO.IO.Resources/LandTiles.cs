namespace ClassicUO.IO.Resources;

internal struct LandTiles
{
	public TileFlag Flags;

	public ushort TexID;

	public string Name;

	public bool IsWet => (Flags & TileFlag.Wet) != 0;

	public bool IsImpassable => (Flags & TileFlag.Impassable) != 0;

	public bool IsNoDiagonal => (Flags & TileFlag.NoDiagonal) != 0;

	public LandTiles(ulong flags, ushort textId, string name)
	{
		Flags = (TileFlag)flags;
		TexID = textId;
		Name = name;
	}
}
