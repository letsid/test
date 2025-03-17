using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ClassicUO.Input;

internal sealed class MouseEventArgs : EventArgs
{
	public Point Location { get; }

	public int X => Location.X;

	public int Y => Location.Y;

	public MouseButtonType Button { get; }

	public ButtonState ButtonState { get; }

	public MouseEventArgs(int x, int y, MouseButtonType button = MouseButtonType.None, ButtonState state = ButtonState.Released)
	{
		Location = new Point(x, y);
		Button = button;
		ButtonState = state;
	}
}
