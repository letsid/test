using System.Collections.Generic;

namespace ClassicUO.Game.Data;

internal abstract class CustomHouseObjectCategory<T> where T : CustomHouseObject
{
	public int Index;

	public List<T> Items = new List<T>();
}
