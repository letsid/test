namespace ClassicUO.IO.Resources;

internal struct StaticTiles
{
	public TileFlag Flags;

	public byte Weight;

	public byte Layer;

	public int Count;

	public ushort AnimID;

	public ushort Hue;

	public ushort LightIndex;

	public byte Height;

	public string Name;

	public bool IsAnimated => (Flags & TileFlag.Animation) != 0;

	public bool IsBridge => (Flags & TileFlag.Bridge) != 0;

	public bool IsImpassable => (Flags & TileFlag.Impassable) != 0;

	public bool IsSurface => (Flags & TileFlag.Surface) != 0;

	public bool IsWearable => (Flags & TileFlag.Wearable) != 0;

	public bool IsInternal => (Flags & TileFlag.Internal) != 0;

	public bool IsBackground => (Flags & TileFlag.Background) != 0;

	public bool IsNoDiagonal => (Flags & TileFlag.NoDiagonal) != 0;

	public bool IsWet => (Flags & TileFlag.Wet) != 0;

	public bool IsFoliage => (Flags & TileFlag.Foliage) != 0;

	public bool IsRoof => (Flags & TileFlag.Roof) != 0;

	public bool IsTranslucent => (Flags & TileFlag.Translucent) != 0;

	public bool IsPartialHue => (Flags & TileFlag.PartialHue) != 0;

	public bool IsStackable => (Flags & TileFlag.Generic) != 0;

	public bool IsTransparent => (Flags & TileFlag.Transparent) != 0;

	public bool IsContainer => (Flags & TileFlag.Container) != 0;

	public bool IsDoor => (Flags & TileFlag.Door) != 0;

	public bool IsWall => (Flags & TileFlag.Wall) != 0;

	public bool IsLight => (Flags & TileFlag.LightSource) != 0;

	public bool IsNoShoot => (Flags & TileFlag.NoShoot) != 0;

	public bool IsWeapon => (Flags & TileFlag.Weapon) != 0;

	public bool IsMultiMovable => (Flags & TileFlag.MultiMovable) != 0;

	public bool IsWindow => (Flags & TileFlag.Window) != 0;

	public StaticTiles(ulong flags, byte weight, byte layer, int count, ushort animId, ushort hue, ushort lightIndex, byte height, string name)
	{
		Flags = (TileFlag)flags;
		Weight = weight;
		Layer = layer;
		Count = count;
		AnimID = animId;
		Hue = hue;
		LightIndex = lightIndex;
		Height = height;
		Name = name;
	}
}
