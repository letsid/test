using ClassicUO.Resources;

namespace ClassicUO.Game.Data;

internal sealed class Skill
{
	public Lock Lock { get; internal set; }

	public ushort ValueFixed { get; internal set; }

	public ushort BaseFixed { get; internal set; }

	public ushort CapFixed { get; internal set; }

	public float Value => (float)(int)ValueFixed / 10f;

	public float Base => (float)(int)BaseFixed / 10f;

	public float Cap => (float)(int)CapFixed / 10f;

	public bool IsClickable { get; }

	public string Name { get; }

	public int Index { get; }

	public Skill(string name, int index, bool click)
	{
		Name = name;
		Index = index;
		IsClickable = click;
	}

	public override string ToString()
	{
		return string.Format(ResGeneral.Name0Val1, Name, Value);
	}
}
