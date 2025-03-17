using System.IO;

namespace ClassicUO.IO.Resources;

internal static class Verdata
{
	public static UOFileIndex5D[] Patches { get; }

	public static UOFileMul File { get; }

	static Verdata()
	{
		string uOFilePath = UOFileManager.GetUOFilePath("verdata.mul");
		if (!System.IO.File.Exists(uOFilePath))
		{
			Patches = new UOFileIndex5D[0];
			File = null;
			return;
		}
		File = new UOFileMul(uOFilePath);
		try
		{
			Patches = File.ReadArray<UOFileIndex5D>(File.ReadInt());
		}
		catch
		{
			Patches = new UOFileIndex5D[0];
		}
	}
}
