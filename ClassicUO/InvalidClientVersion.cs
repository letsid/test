using System;

namespace ClassicUO;

internal class InvalidClientVersion : Exception
{
	public InvalidClientVersion(string msg)
		: base(msg)
	{
	}
}
