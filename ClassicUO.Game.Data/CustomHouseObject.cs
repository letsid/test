namespace ClassicUO.Game.Data;

internal abstract class CustomHouseObject
{
	public int Category;

	public int FeatureMask;

	public virtual bool Parse(string text)
	{
		return false;
	}

	public abstract int Contains(ushort graphic);
}
