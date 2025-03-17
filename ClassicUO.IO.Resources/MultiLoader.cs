using System.IO;
using System.Threading.Tasks;
using ClassicUO.Data;

namespace ClassicUO.IO.Resources;

internal class MultiLoader : UOFileLoader
{
	private static MultiLoader _instance;

	public static MultiLoader Instance => _instance ?? (_instance = new MultiLoader());

	public int Count { get; private set; }

	public UOFile File { get; private set; }

	public bool IsUOP { get; private set; }

	public int Offset { get; private set; }

	private MultiLoader()
	{
	}

	public unsafe override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("MultiCollection.uop");
			if (Client.IsUOPInstallation && System.IO.File.Exists(uOFilePath))
			{
				Count = 8704;
				File = new UOFileUop(uOFilePath, "build/multicollection/{0:D6}.bin");
				Entries = new UOFileIndex[Count];
				IsUOP = true;
			}
			else
			{
				string uOFilePath2 = UOFileManager.GetUOFilePath("multi.mul");
				string uOFilePath3 = UOFileManager.GetUOFilePath("multi.idx");
				if (System.IO.File.Exists(uOFilePath2) && System.IO.File.Exists(uOFilePath3))
				{
					File = new UOFileMul(uOFilePath2, uOFilePath3, 8704, 14);
					int count = (Offset = ((Client.Version >= ClientVersion.CV_7090) ? (sizeof(MultiBlockNew) + 2) : sizeof(MultiBlock)));
					Count = count;
				}
			}
			File.FillEntries(ref Entries);
		});
	}
}
