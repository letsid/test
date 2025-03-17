using System;
using Microsoft.Xna.Framework;

namespace ClassicUO.Input;

internal sealed class MouseDoubleClickEventArgs : EventArgs
{
	public Point Location { get; }

	public int X => Location.X;

	public int Y => Location.Y;

	public MouseButtonType Button { get; }

	public bool Result { get; set; }

	public MouseDoubleClickEventArgs(int x, int y, MouseButtonType button)
	{
		Location = new Point(x, y);
		Button = button;
	}
}
