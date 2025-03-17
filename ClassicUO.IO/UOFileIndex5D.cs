namespace ClassicUO.IO;

internal struct UOFileIndex5D
{
	public uint FileID;

	public uint BlockID;

	public uint Position;

	public uint Length;

	public uint GumpData;

	public UOFileIndex5D(uint file, uint index, uint offset, uint length, uint extra = 0u)
	{
		FileID = file;
		BlockID = index;
		Position = offset;
		Length = length;
		GumpData = extra;
	}
}
