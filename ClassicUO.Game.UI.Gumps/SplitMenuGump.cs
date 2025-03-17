using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps;

internal class SplitMenuGump : Gump
{
	private bool _firstChange;

	private int _lastValue;

	private readonly Point _offset;

	private readonly Button _okButton;

	private readonly HSliderBar _slider;

	private readonly StbTextBox _textBox;

	private bool _updating;

	public SplitMenuGump(uint serial, Point offset)
		: base(serial, 0u)
	{
		Item item = World.Items.Get(serial);
		if (item == null || item.IsDestroyed)
		{
			Dispose();
			return;
		}
		_offset = offset;
		CanMove = true;
		AcceptMouseInput = false;
		base.CanCloseWithRightClick = true;
		GumpPic c = new GumpPic(0, 0, 2140, 0)
		{
			ContainsByBounds = true
		};
		Add(c);
		Add(_slider = new HSliderBar(29, 16, 105, 1, item.Amount, item.Amount, HSliderBarStyle.BlueWidgetNoBar, hasText: false, 0, 0));
		_lastValue = _slider.Value;
		Button obj = new Button(0, 2141, 2142, 2143, "", 0)
		{
			ButtonAction = ButtonAction.Default
		};
		obj.X = 102;
		obj.Y = 37;
		Button c2 = obj;
		_okButton = obj;
		Add(c2);
		_okButton.MouseUp += OkButtonOnMouseClick;
		StbTextBox stbTextBox = new StbTextBox(1, -1, 60, isunicode: false, FontStyle.None, 902);
		stbTextBox.X = 29;
		stbTextBox.Y = 42;
		stbTextBox.Width = 60;
		stbTextBox.Height = 20;
		stbTextBox.NumbersOnly = true;
		StbTextBox c3 = stbTextBox;
		_textBox = stbTextBox;
		Add(c3);
		_textBox.SetText(item.Amount.ToString());
		_textBox.TextChanged += delegate
		{
			UpdateText();
		};
		_textBox.SetKeyboardFocus();
		_slider.ValueChanged += delegate
		{
			UpdateText();
		};
	}

	private void UpdateText()
	{
		if (_updating)
		{
			return;
		}
		_updating = true;
		int result;
		if (_slider.Value != _lastValue)
		{
			_textBox.SetText(_slider.Value.ToString());
		}
		else if (_textBox.Text.Length == 0)
		{
			_slider.Value = _slider.MinValue;
		}
		else if (!int.TryParse(_textBox.Text, out result))
		{
			_textBox.SetText(_slider.Value.ToString());
		}
		else if (result != _slider.Value)
		{
			if (result <= _slider.MaxValue)
			{
				_slider.Value = result;
			}
			else
			{
				if (!_firstChange)
				{
					string s = _textBox.Text[_textBox.Text.Length - 1].ToString();
					_slider.Value = int.Parse(s);
					_firstChange = true;
				}
				else
				{
					_slider.Value = _slider.MaxValue;
				}
				_textBox.SetText(_slider.Value.ToString());
			}
		}
		_lastValue = _slider.Value;
		_updating = false;
	}

	private void OkButtonOnMouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtonType.Left)
		{
			PickUp();
		}
	}

	public override void OnKeyboardReturn(int textID, string text)
	{
		PickUp();
	}

	private void PickUp()
	{
		if (_slider.Value > 0)
		{
			GameActions.PickUp(base.LocalSerial, _offset.X, _offset.Y, _slider.Value, null);
		}
		Dispose();
	}

	public override void Update(double totalTime, double frameTime)
	{
		Item item = World.Items.Get(base.LocalSerial);
		if (item == null || item.IsDestroyed)
		{
			Dispose();
		}
		if (!base.IsDisposed)
		{
			base.Update(totalTime, frameTime);
		}
	}

	public override void Dispose()
	{
		if (_okButton != null)
		{
			_okButton.MouseUp -= OkButtonOnMouseClick;
		}
		base.Dispose();
	}
}
