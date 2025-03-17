using System.IO;

namespace ClassicUO.Utility;

public struct CheckSumJob
{
	public string filename;

	public Stream inputstream;

	public CheckSumJob(string f, Stream i = null)
	{
		filename = f;
		inputstream = i;
	}
}
