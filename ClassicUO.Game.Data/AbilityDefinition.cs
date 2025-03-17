namespace ClassicUO.Game.Data;

internal readonly struct AbilityDefinition
{
	public readonly int Index;

	public readonly string Name;

	public readonly ushort Icon;

	public AbilityDefinition(int index, string name, ushort icon)
	{
		Index = index;
		Name = name;
		Icon = icon;
	}
}
