namespace ClassicUO.Game.Managers;

internal class ItemProperty
{
	public string Data;

	public string Name;

	public uint Revision;

	public uint Serial;

	public bool IsEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(Name))
			{
				return string.IsNullOrEmpty(Data);
			}
			return false;
		}
	}

	public string CreateData(bool extended)
	{
		return string.Empty;
	}
}
