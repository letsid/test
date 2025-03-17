namespace ClassicUO.IO.Resources;

public class CustomBook
{
	public ushort GumpID { get; set; }

	public ushort TextCol { get; set; }

	public ushort Renderer { get; set; }

	public CustomBook()
	{
	}

	public CustomBook(ushort GumpID, ushort TextCol, ushort Renderer)
	{
		this.GumpID = GumpID;
		this.TextCol = TextCol;
		this.Renderer = Renderer;
	}
}
