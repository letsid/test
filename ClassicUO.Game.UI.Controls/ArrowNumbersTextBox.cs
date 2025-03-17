using System;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls;

internal class ArrowNumbersTextBox : Control
{
	private const int TIME_BETWEEN_CLICKS = 250;

	private readonly int _Min;

	private readonly int _Max;

	private readonly StbTextBox _textBox;

	private float _timeUntilNextClick;

	private readonly Button _up;

	private readonly Button _down;

	internal string Text
	{
		get
		{
			return _textBox?.Text ?? string.Empty;
		}
		set
		{
			_textBox?.SetText(value);
		}
	}

	public ArrowNumbersTextBox(int x, int y, int width, int raiseamount, int minvalue, int maxvalue, byte font = 0, int maxcharlength = -1, bool isunicode = true, FontStyle style = FontStyle.None, ushort hue = 0)
	{
		int num = 20;
		base.X = x;
		base.Y = y;
		base.Width = width;
		base.Height = num;
		_Min = minvalue;
		_Max = maxvalue;
		ResizePic resizePic = new ResizePic(3000);
		resizePic.Width = width;
		resizePic.Height = num + 4;
		Add(resizePic);
		Button button = new Button(raiseamount, 2435, 2436, 0, "", 0);
		button.X = width - 12;
		button.ButtonAction = ButtonAction.Activate;
		_up = button;
		_up.MouseDown += delegate
		{
			if (_up.IsClicked)
			{
				UpdateValue();
				_timeUntilNextClick = 500f;
			}
		};
		Add(_up);
		Button button2 = new Button(-raiseamount, 2437, 2438, 0, "", 0);
		button2.X = width - 12;
		button2.Y = num - 7;
		button2.ButtonAction = ButtonAction.Activate;
		_down = button2;
		_down.MouseDown += delegate
		{
			if (_down.IsClicked)
			{
				UpdateValue();
				_timeUntilNextClick = 500f;
			}
		};
		Add(_down);
		StbTextBox stbTextBox = new StbTextBox(font, maxcharlength, width, isunicode, style, hue);
		stbTextBox.X = 2;
		stbTextBox.Y = 2;
		stbTextBox.Height = num;
		stbTextBox.Width = width - 17;
		stbTextBox.NumbersOnly = true;
		StbTextBox c = stbTextBox;
		_textBox = stbTextBox;
		Add(c);
	}

	private void UpdateValue()
	{
		int.TryParse(_textBox.Text, out var result);
		result = ((!_up.IsClicked) ? (result + _down.ButtonID) : (result + _up.ButtonID));
		ValidateValue(result);
	}

	internal override void OnFocusLost()
	{
		if (!base.IsDisposed)
		{
			int.TryParse(_textBox.Text, out var result);
			ValidateValue(result);
		}
	}

	private void ValidateValue(int val)
	{
		base.Tag = (val = Math.Max(_Min, Math.Min(_Max, val)));
		_textBox.SetText(val.ToString());
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (_up.IsClicked || _down.IsClicked)
		{
			if (_timeUntilNextClick <= 0f)
			{
				_timeUntilNextClick += 250f;
				UpdateValue();
			}
			_timeUntilNextClick -= (float)frameTime;
		}
		base.Update(totalTime, frameTime);
	}
}
