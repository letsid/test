using System;
using ClassicUO.Input;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal abstract class ScrollBarBase : Control
{
	private const int TIME_BETWEEN_CLICKS = 2;

	private float _timeUntilNextClick;

	protected bool _btUpClicked;

	protected bool _btDownClicked;

	protected bool _btnSliderClicked;

	protected bool _btSliderClicked;

	protected Point _clickPosition;

	protected Rectangle _rectUpButton;

	protected Rectangle _rectDownButton;

	protected int _sliderPosition;

	protected int _value;

	protected int _minValue;

	protected int _maxValue;

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				if (_value < MinValue)
				{
					_value = MinValue;
				}
				else if (_value > MaxValue)
				{
					_value = MaxValue;
				}
				this.ValueChanged.Raise();
			}
		}
	}

	public int MinValue
	{
		get
		{
			return _minValue;
		}
		set
		{
			if (_minValue != value)
			{
				_minValue = value;
				if (_value < _minValue)
				{
					_value = _minValue;
				}
			}
		}
	}

	public int MaxValue
	{
		get
		{
			return _maxValue;
		}
		set
		{
			if (_maxValue != value)
			{
				if (value < 0)
				{
					_maxValue = 0;
				}
				else
				{
					_maxValue = value;
				}
				if (_value > _maxValue)
				{
					_value = _maxValue;
				}
			}
		}
	}

	public int ScrollStep { get; set; } = 50;

	public event EventHandler ValueChanged;

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (MaxValue <= MinValue)
		{
			int value = (MaxValue = MinValue);
			Value = value;
		}
		_sliderPosition = GetSliderYPosition();
		if ((_btUpClicked || _btDownClicked) && _timeUntilNextClick < (float)Time.Ticks)
		{
			_timeUntilNextClick = Time.Ticks + 2;
			if (_btUpClicked)
			{
				Value -= 1 + Control._StepChanger;
			}
			else if (_btDownClicked)
			{
				Value += 1 + Control._StepChanger;
			}
			Control._StepsDone++;
			if (Control._StepsDone % 8 == 0)
			{
				Control._StepChanger++;
			}
		}
	}

	protected override void OnMouseWheel(MouseEventType delta)
	{
		switch (delta)
		{
		case MouseEventType.WheelScrollUp:
			Value -= ScrollStep;
			break;
		case MouseEventType.WheelScrollDown:
			Value += ScrollStep;
			break;
		}
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			_timeUntilNextClick = 0f;
			_btnSliderClicked = false;
			if (_rectDownButton.Contains(x, y))
			{
				_btDownClicked = true;
			}
			else if (_rectUpButton.Contains(x, y))
			{
				_btUpClicked = true;
			}
			else if (Contains(x, y))
			{
				_btnSliderClicked = true;
				CalculateByPosition(x, y);
			}
		}
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			_btDownClicked = false;
			_btUpClicked = false;
			_btnSliderClicked = false;
			Control._StepChanger = (Control._StepsDone = 1);
		}
	}

	protected override void OnMouseOver(int x, int y)
	{
		if (_btnSliderClicked)
		{
			CalculateByPosition(x, y);
		}
	}

	protected int GetSliderYPosition()
	{
		if (MaxValue == MinValue)
		{
			return 0;
		}
		return (int)Math.Round((float)GetScrollableArea() * ((float)(Value - MinValue) / (float)(MaxValue - MinValue)));
	}

	protected abstract int GetScrollableArea();

	protected abstract void CalculateByPosition(int x, int y);
}
