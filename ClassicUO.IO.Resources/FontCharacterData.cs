namespace ClassicUO.IO.Resources;

internal struct FontCharacterData
{
	public byte Width;

	public byte Height;

	public unsafe ushort* Data;

	public unsafe FontCharacterData(byte w, byte h, ushort* data)
	{
		Width = w;
		Height = h;
		Data = data;
	}
}
