using System;

namespace ClassicUO.Input;

internal sealed class MouseWheelEventArgs : EventArgs
{
	public MouseEventType Direction { get; }

	public MouseWheelEventArgs(MouseEventType direction)
	{
		if (direction != MouseEventType.WheelScroll && direction != MouseEventType.WheelScrollDown && direction != MouseEventType.WheelScrollUp)
		{
			throw new Exception("Wrong scroll direction: " + direction);
		}
		Direction = direction;
	}
}
