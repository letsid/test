namespace ClassicUO.Game.UI.Controls;

internal class DataBox : Control
{
	public bool ContainsByBounds { get; set; }

	public DataBox(int x, int y, int w, int h)
	{
		CanMove = false;
		AcceptMouseInput = true;
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		base.WantUpdateSize = false;
	}

	public void ReArrangeChildren()
	{
		int i = 0;
		int num = 0;
		for (; i < base.Children.Count; i++)
		{
			Control control = base.Children[i];
			if (control.IsVisible && !control.IsDisposed)
			{
				control.Y = num;
				num += control.Height;
			}
		}
		base.WantUpdateSize = true;
	}

	public override bool Contains(int x, int y)
	{
		if (ContainsByBounds)
		{
			return true;
		}
		Control res = null;
		x += base.ScreenCoordinateX;
		y += base.ScreenCoordinateY;
		foreach (Control child in base.Children)
		{
			child.HitTest(x, y, ref res);
			if (res != null)
			{
				return true;
			}
		}
		return false;
	}
}
