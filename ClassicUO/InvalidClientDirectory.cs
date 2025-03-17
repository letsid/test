using System;

namespace ClassicUO;

internal class InvalidClientDirectory : Exception
{
	public InvalidClientDirectory(string msg)
		: base(msg)
	{
	}
}
