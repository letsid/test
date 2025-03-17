namespace ClassicUO.Game.Scenes;

internal class CityInfo
{
	public readonly string Building;

	public readonly string City;

	public readonly string Description;

	public readonly int Index;

	public readonly bool IsNewCity;

	public readonly uint Map;

	public readonly ushort X;

	public readonly ushort Y;

	public readonly sbyte Z;

	public CityInfo(int index, string city, string building, string description, ushort x, ushort y, sbyte z, uint map, bool isNew)
	{
		Index = index;
		City = city;
		Building = building;
		Description = description;
		X = x;
		Y = y;
		Z = z;
		Map = map;
		IsNewCity = isNew;
	}
}
