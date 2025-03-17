using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ClassicUO.IO.Resources;

internal class AnimDataLoader : UOFileLoader
{
	private static AnimDataLoader _instance;

	private UOFileMul _file;

	public static AnimDataLoader Instance => _instance ?? (_instance = new AnimDataLoader());

	public UOFile AnimDataFile => _file;

	private AnimDataLoader()
	{
	}

	public override Task Load()
	{
		return Task.Run(delegate
		{
			string uOFilePath = UOFileManager.GetUOFilePath("animdata.mul");
			if (File.Exists(uOFilePath))
			{
				_file = new UOFileMul(uOFilePath);
			}
		});
	}

	public unsafe AnimDataFrame CalculateCurrentGraphic(ushort graphic)
	{
		IntPtr intPtr = _file?.StartAddress ?? IntPtr.Zero;
		if (intPtr != IntPtr.Zero)
		{
			IntPtr intPtr2 = intPtr + (graphic * 68 + 4 * ((graphic >> 3) + 1));
			if (intPtr2.ToInt64() < intPtr.ToInt64() + _file.Length)
			{
				return Unsafe.AsRef<AnimDataFrame>((void*)intPtr2);
			}
		}
		return default(AnimDataFrame);
	}
}
