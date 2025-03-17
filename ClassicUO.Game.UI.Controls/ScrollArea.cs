using System;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class ScrollArea : Control
{
	private bool _isNormalScroll;

	private readonly ScrollBarBase _scrollBar;

	public Rectangle ScissorRectangle;

	public int ScrollMaxHeight { get; set; } = -1;

	public ScrollbarBehaviour ScrollbarBehaviour { get; set; }

	public int ScrollValue => _scrollBar.Value;

	public int ScrollMinValue => _scrollBar.MinValue;

	public int ScrollMaxValue => _scrollBar.MaxValue;

	public ScrollArea(int x, int y, int w, int h, bool normalScrollbar, int scroll_max_height = -1)
	{
		base.X = x;
		base.Y = y;
		base.Width = w;
		base.Height = h;
		_isNormalScroll = normalScrollbar;
		if (normalScrollbar)
		{
			_scrollBar = new ScrollBar(base.Width - 14, 0, base.Height);
		}
		else
		{
			ScrollFlag scrollFlag = new ScrollFlag();
			scrollFlag.X = base.Width - 19;
			scrollFlag.Height = h;
			_scrollBar = scrollFlag;
			base.Width += 15;
		}
		ScrollMaxHeight = scroll_max_height;
		_scrollBar.MinValue = 0;
		_scrollBar.MaxValue = ((scroll_max_height >= 0) ? scroll_max_height : base.Height);
		_scrollBar.Parent = this;
		AcceptMouseInput = true;
		base.WantUpdateSize = false;
		CanMove = true;
		ScrollbarBehaviour = ScrollbarBehaviour.ShowWhenDataExceedFromView;
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		CalculateScrollBarMaxValue();
		if (ScrollbarBehaviour == ScrollbarBehaviour.ShowAlways)
		{
			_scrollBar.IsVisible = true;
		}
		else if (ScrollbarBehaviour == ScrollbarBehaviour.ShowWhenDataExceedFromView)
		{
			_scrollBar.IsVisible = _scrollBar.MaxValue > _scrollBar.MinValue;
		}
	}

	public void Scroll(bool isup)
	{
		if (isup)
		{
			_scrollBar.Value -= _scrollBar.ScrollStep;
		}
		else
		{
			_scrollBar.Value += _scrollBar.ScrollStep;
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		ScrollBarBase scrollBarBase = (ScrollBarBase)base.Children[0];
		scrollBarBase.Draw(batcher, x + scrollBarBase.X, y + scrollBarBase.Y);
		if (batcher.ClipBegin(x + ScissorRectangle.X, y + ScissorRectangle.Y, base.Width - 14 + ScissorRectangle.Width, base.Height + ScissorRectangle.Height))
		{
			for (int i = 1; i < base.Children.Count; i++)
			{
				Control control = base.Children[i];
				if (control.IsVisible)
				{
					int y2 = y + control.Y - scrollBarBase.Value + ScissorRectangle.Y;
					control.Draw(batcher, x + control.X, y2);
				}
			}
			batcher.ClipEnd();
		}
		return true;
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		switch (delta)
		{
		case MouseEventType.WheelScrollUp:
			_scrollBar.Value -= _scrollBar.ScrollStep;
			break;
		case MouseEventType.WheelScrollDown:
			_scrollBar.Value += _scrollBar.ScrollStep;
			break;
		}
	}

	public override void Clear()
	{
		for (int i = 1; i < base.Children.Count; i++)
		{
			base.Children[i].Dispose();
		}
	}

	private void CalculateScrollBarMaxValue()
	{
		_scrollBar.Height = ((ScrollMaxHeight >= 0) ? ScrollMaxHeight : base.Height);
		bool flag = _scrollBar.Value == _scrollBar.MaxValue && _scrollBar.MaxValue != 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 1; i < base.Children.Count; i++)
		{
			Control control = base.Children[i];
			if (control.IsVisible && !control.IsDisposed)
			{
				if (control.X < num)
				{
					num = control.X;
				}
				if (control.Y < num2)
				{
					num2 = control.Y;
				}
				if (control.Bounds.Right > num3)
				{
					num3 = control.Bounds.Right;
				}
				if (control.Bounds.Bottom > num4)
				{
					num4 = control.Bounds.Bottom;
				}
			}
		}
		Math.Abs(num);
		Math.Abs(num3);
		int num5 = Math.Abs(num2) + Math.Abs(num4) - _scrollBar.Height;
		num5 = Math.Max(0, num5 - (-ScissorRectangle.Y + ScissorRectangle.Height));
		if (num5 > 0)
		{
			_scrollBar.MaxValue = num5;
			if (flag)
			{
				_scrollBar.Value = _scrollBar.MaxValue;
			}
		}
		else
		{
			ScrollBarBase scrollBar = _scrollBar;
			int value = (_scrollBar.MaxValue = 0);
			scrollBar.Value = value;
		}
		_scrollBar.UpdateOffset(0, base.Offset.Y);
		for (int j = 1; j < base.Children.Count; j++)
		{
			base.Children[j].UpdateOffset(0, -_scrollBar.Value + ScissorRectangle.Y);
		}
	}
}
