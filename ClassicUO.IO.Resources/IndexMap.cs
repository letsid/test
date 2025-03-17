namespace ClassicUO.IO.Resources;

internal struct IndexMap
{
	public ulong MapAddress;

	public ulong OriginalMapAddress;

	public ulong OriginalStaticAddress;

	public uint OriginalStaticCount;

	public ulong StaticAddress;

	public uint StaticCount;

	public static IndexMap Invalid;
}
