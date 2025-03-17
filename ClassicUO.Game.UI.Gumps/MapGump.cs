using System;
using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class MapGump : Gump
{
	private enum ButtonType
	{
		PlotCourse,
		StopPlotting,
		ClearCourse
	}

	private class PinControl : Control
	{
		private readonly GumpPic _pic;

		private readonly RenderedText _text;

		public string NumberText
		{
			get
			{
				return _text.Text;
			}
			set
			{
				_text.Text = value;
			}
		}

		public PinControl(int x, int y)
		{
			base.X = x;
			base.Y = y;
			_text = RenderedText.Create(string.Empty, ushort.MaxValue, 0, isunicode: false, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);
			_pic = new GumpPic(0, 0, 5019, 0);
			Add(_pic);
			base.WantUpdateSize = false;
			base.Width = _pic.Width;
			base.Height = _pic.Height;
			AcceptMouseInput = true;
			CanMove = false;
			_pic.AcceptMouseInput = true;
			Priority = ClickPriority.High;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (base.MouseIsOver)
			{
				_pic.Hue = 53;
			}
			else if (_pic.Hue != 0)
			{
				_pic.Hue = 0;
			}
			base.Draw(batcher, x, y);
			_text.Draw(batcher, x - _text.Width - 1, y, 1f, 0);
			return true;
		}

		public override void Dispose()
		{
			_text?.Destroy();
			base.Dispose();
		}
	}

	private readonly Button[] _buttons = new Button[3];

	private readonly List<Control> _container = new List<Control>();

	private PinControl _currentPin;

	private Point _lastPoint;

	private HitBox _hit;

	private Texture2D _mapTexture;

	private uint _pinTimer;

	public int PlotState { get; private set; }

	public MapGump(uint serial, ushort gumpid, int width, int height)
		: base(serial, 0u)
	{
		AcceptMouseInput = false;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		base.Width = width;
		base.Height = height;
		base.WantUpdateSize = false;
		ResizePic resizePic = new ResizePic(5170);
		resizePic.Width = width + 44;
		resizePic.Height = height + 61;
		Add(resizePic);
		Button[] buttons = _buttons;
		Button button = new Button(0, 5016, 5016, 0, "", 0);
		button.X = width - 100 >> 1;
		button.Y = 5;
		button.ButtonAction = ButtonAction.Activate;
		Button c = button;
		buttons[0] = button;
		Add(c);
		Button[] buttons2 = _buttons;
		Button button2 = new Button(1, 5017, 5017, 0, "", 0);
		button2.X = width - 70 >> 1;
		button2.Y = 5;
		button2.ButtonAction = ButtonAction.Activate;
		c = button2;
		buttons2[1] = button2;
		Add(c);
		Button[] buttons3 = _buttons;
		Button button3 = new Button(2, 5018, 5018, 0, "", 0);
		button3.X = width - 66 >> 1;
		button3.Y = height + 37;
		button3.ButtonAction = ButtonAction.Activate;
		c = button3;
		buttons3[2] = button3;
		Add(c);
		_buttons[0].IsVisible = (_buttons[0].IsEnabled = PlotState == 0);
		_buttons[1].IsVisible = (_buttons[1].IsEnabled = PlotState == 1);
		_buttons[2].IsVisible = (_buttons[2].IsEnabled = PlotState == 1);
		_hit = new HitBox(24, 31, width, height, null, 0f);
		Add(_hit);
		_hit.MouseUp += TextureControlOnMouseUp;
		Add(new GumpPic(width - 20, height - 20, 5021, 0));
	}

	public void SetMapTexture(Texture2D texture)
	{
		_mapTexture?.Dispose();
		_mapTexture = texture;
		base.Width = texture.Width;
		base.Height = texture.Height;
		base.WantUpdateSize = true;
	}

	public void AddPin(int x, int y)
	{
		PinControl pinControl = new PinControl(x, y);
		pinControl.X += pinControl.Width + 5;
		pinControl.Y += pinControl.Height;
		pinControl.NumberText = (_container.Count + 1).ToString();
		_container.Add(pinControl);
		Add(pinControl);
	}

	public void ClearContainer()
	{
		foreach (Control item in _container)
		{
			item.Dispose();
		}
		_container.Clear();
	}

	public void SetPlotState(int s)
	{
		PlotState = s;
		Button obj = _buttons[0];
		bool isVisible = (_buttons[0].IsEnabled = PlotState == 0);
		obj.IsVisible = isVisible;
		Button obj2 = _buttons[1];
		isVisible = (_buttons[1].IsEnabled = PlotState == 1);
		obj2.IsVisible = isVisible;
		Button obj3 = _buttons[2];
		isVisible = (_buttons[2].IsEnabled = PlotState == 1);
		obj3.IsVisible = isVisible;
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((ButtonType)buttonID)
		{
		case ButtonType.PlotCourse:
		case ButtonType.StopPlotting:
			NetClient.Socket.Send_MapMessage(base.LocalSerial, 6, (byte)PlotState, 65512, 65505);
			SetPlotState((PlotState == 0) ? 1 : 0);
			break;
		case ButtonType.ClearCourse:
			NetClient.Socket.Send_MapMessage(base.LocalSerial, 5, 0, 65512, 65505);
			ClearContainer();
			break;
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_currentPin != null && Mouse.LDragOffset != Point.Zero && Mouse.LDragOffset != _lastPoint)
		{
			_currentPin.Location += Mouse.LDragOffset - _lastPoint;
			if (_currentPin.X < _hit.X)
			{
				_currentPin.X = _hit.X;
			}
			else if (_currentPin.X >= _hit.Width)
			{
				_currentPin.X = _hit.Width;
			}
			if (_currentPin.Y < _hit.Y)
			{
				_currentPin.Y = _hit.Y;
			}
			else if (_currentPin.Y >= _hit.Height)
			{
				_currentPin.Y = _hit.Height;
			}
			_lastPoint = Mouse.LDragOffset;
		}
	}

	private void TextureControlOnMouseUp(object sender, MouseEventArgs e)
	{
		Point lDragOffset = Mouse.LDragOffset;
		if (Math.Abs(lDragOffset.X) < 5 && Math.Abs(lDragOffset.Y) < 5 && PlotState != 0 && _currentPin == null && _pinTimer > Time.Ticks)
		{
			ushort x = (ushort)(e.X + 5);
			ushort y = (ushort)e.Y;
			NetClient.Socket.Send_MapMessage(base.LocalSerial, 1, 0, x, y);
			AddPin(x, y);
		}
		_currentPin = null;
		_lastPoint = Point.Zero;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		base.Draw(batcher, x, y);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.Draw(_mapTexture, new Rectangle(x + _hit.X, y + _hit.Y, _hit.Width, _hit.Height), hueVector);
		Texture2D texture = SolidColorTextureCache.GetTexture(Color.White);
		for (int i = 0; i < _container.Count; i++)
		{
			_container[i].Draw(batcher, x + _container[i].X, y + _container[i].Y);
			if (i + 1 >= _container.Count)
			{
				break;
			}
			Control control = _container[i];
			Control control2 = _container[i + 1];
			batcher.DrawLine(texture, new Vector2(control.ScreenCoordinateX, control.ScreenCoordinateY), new Vector2(control2.ScreenCoordinateX, control2.ScreenCoordinateY), hueVector, 1f);
		}
		return true;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		base.OnMouseUp(x, y, button);
		_currentPin = null;
		_lastPoint = Point.Zero;
	}

	protected override void OnMouseDown(int x, int y, MouseButtonType button)
	{
		_pinTimer = Time.Ticks + 300;
		if (UIManager.MouseOverControl is PinControl currentPin)
		{
			_currentPin = currentPin;
		}
	}

	private int LineUnderMouse(ref int x1, ref int y1, ref int x2, ref int y2)
	{
		int num = x2 - x1;
		int num2 = y2 - y1;
		float num3 = num;
		if (num3 == 0f)
		{
			num3 = 1f;
		}
		float num4 = (float)Math.PI;
		float num5 = 0f - (float)(Math.Atan((float)num2 / num3) * 180.0 / (double)num4);
		bool flag = false;
		if (x1 >= x2 && y1 <= y2)
		{
			flag = true;
		}
		else if (x1 >= x2 && y1 >= y2)
		{
			flag = true;
		}
		float num6 = (float)Math.Sin(num5 * num4 / 180f);
		float num7 = (float)Math.Sin(num5 * num4 / 180f);
		int num8 = (int)((float)num * num7 - (float)num2 * num6);
		int num9 = (int)((float)num * num6 + (float)num2 * num7);
		int num10 = x1 + num8;
		int num11 = y1 + num9;
		int num12 = Mouse.Position.X - x1;
		num2 = Mouse.Position.Y - y1;
		num8 = (int)((float)num12 * num7 - (float)num2 * num6);
		num9 = (int)((float)num12 * num6 + (float)num2 * num7);
		Point value = new Point(x1 + num8, y1 + num9);
		int result = 0;
		if (!flag)
		{
			Rectangle rectangle = default(Rectangle);
			rectangle.X = x1 - 5;
			rectangle.Y = y1 - 5;
			rectangle.Width = num10 + 5;
			rectangle.Height = num11 + 5;
			Rectangle rectangle2 = rectangle;
			if (rectangle2.Contains(value))
			{
				x1 += (x2 - x1) / 2;
				y1 += (y2 - y1) / 2;
				result = 1;
			}
		}
		else
		{
			Rectangle rectangle = default(Rectangle);
			rectangle.X = num10 - 5;
			rectangle.Y = num11 - 5;
			rectangle.Width = x1 + 5;
			rectangle.Height = x2 + 5;
			Rectangle rectangle3 = rectangle;
			if (rectangle3.Contains(value))
			{
				x1 = x2 + (x1 - x2) / 2;
				y1 = y2 + (y1 - y2) / 2;
				result = 2;
			}
		}
		return result;
	}

	public override void Dispose()
	{
		_hit.MouseUp -= TextureControlOnMouseUp;
		_mapTexture?.Dispose();
		base.Dispose();
	}
}
