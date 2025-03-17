using ClassicUO.IO.Resources;

namespace ClassicUO.Game.UI.Controls;

internal abstract class GumpPicBase : Control
{
	private ushort _graphic;

	public ushort Graphic
	{
		get
		{
			return _graphic;
		}
		set
		{
			_graphic = value;
			if (GumpsLoader.Instance.GetGumpTexture(_graphic, out var bounds) == null)
			{
				Dispose();
				return;
			}
			base.Width = bounds.Width;
			base.Height = bounds.Height;
		}
	}

	public ushort Hue { get; set; }

	protected GumpPicBase()
	{
		CanMove = true;
		AcceptMouseInput = true;
	}

	public override bool Contains(int x, int y)
	{
		if (GumpsLoader.Instance.GetGumpTexture(_graphic, out var _) == null)
		{
			return false;
		}
		if (GumpsLoader.Instance.PixelCheck(Graphic, x - base.Offset.X, y - base.Offset.Y))
		{
			return true;
		}
		for (int i = 0; i < base.Children.Count; i++)
		{
			if (base.Children[i].Contains(x, y))
			{
				return true;
			}
		}
		return false;
	}
}
