using System.Linq;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class MenuGump : Gump
{
	private class ItemView : Control
	{
		private readonly ushort _graphic;

		private readonly ushort _hue;

		private readonly bool _isPartial;

		public ItemView(ushort graphic, ushort hue)
		{
			AcceptMouseInput = true;
			base.WantUpdateSize = true;
			_graphic = graphic;
			ArtLoader.Instance.GetStaticTexture(_graphic, out var bounds);
			base.Width = bounds.Width;
			base.Height = bounds.Height;
			_hue = hue;
			_isPartial = TileDataLoader.Instance.StaticData[graphic].IsPartialHue;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (_graphic != 0)
			{
				Rectangle bounds;
				Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(_graphic, out bounds);
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(_hue, _isPartial, 1f);
				batcher.Draw(staticTexture, new Vector2(x, y), bounds, hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private class ContainerHorizontal : Control
	{
		private int _value;

		public int Value
		{
			get
			{
				return _value;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				else if (value > MaxValue)
				{
					value = MaxValue;
				}
				_value = value;
			}
		}

		public int MaxValue { get; private set; }

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (batcher.ClipBegin(x, y, base.Width, base.Height))
			{
				int num = 0;
				int num2 = Value + base.Width;
				bool flag = true;
				foreach (Control child in base.Children)
				{
					if (!child.IsVisible)
					{
						continue;
					}
					child.X = num - Value;
					if (num + child.Width > Value)
					{
						if (num + child.Width <= num2)
						{
							child.Draw(batcher, child.X + x, y);
						}
						else if (flag)
						{
							child.Draw(batcher, child.X + x, y);
							flag = false;
						}
					}
					num += child.Width;
				}
				batcher.ClipEnd();
			}
			return true;
		}

		public void CalculateWidth()
		{
			MaxValue = base.Children.Sum((Control s) => s.Width) - base.Width;
			if (MaxValue < 0)
			{
				MaxValue = 0;
			}
		}
	}

	private readonly ContainerHorizontal _container;

	private bool _isDown;

	private bool _isLeft;

	private readonly HSliderBar _slider;

	public MenuGump(uint serial, uint serv, string name)
		: base(serial, serv)
	{
		CanMove = true;
		AcceptMouseInput = true;
		base.CanCloseWithRightClick = true;
		base.IsFromServer = true;
		Add(new GumpPic(0, 0, 2320, 0));
		ColorBox colorBox = new ColorBox(217, 49, 1);
		colorBox.X = 40;
		colorBox.Y = 42;
		Add(colorBox);
		Label label = new Label(name, isunicode: false, 902, 200, 1, FontStyle.Fixed);
		label.X = 39;
		label.Y = 18;
		Label c = label;
		Add(c);
		ContainerHorizontal containerHorizontal = new ContainerHorizontal();
		containerHorizontal.X = 40;
		containerHorizontal.Y = 42;
		containerHorizontal.Width = 217;
		containerHorizontal.Height = 49;
		containerHorizontal.WantUpdateSize = false;
		_container = containerHorizontal;
		Add(_container);
		Add(_slider = new HSliderBar(40, _container.Y + _container.Height + 12, 217, 0, 1, 0, HSliderBarStyle.MetalWidgetRecessedBar, hasText: false, 0, 0));
		_slider.ValueChanged += delegate
		{
			_container.Value = _slider.Value;
		};
		HitBox hitBox = new HitBox(25, 60, 10, 15)
		{
			Alpha = 0f
		};
		hitBox.MouseDown += delegate
		{
			_isDown = true;
			_isLeft = true;
		};
		hitBox.MouseUp += delegate
		{
			_isDown = false;
		};
		Add(hitBox);
		HitBox hitBox2 = new HitBox(260, 60, 10, 15)
		{
			Alpha = 0f
		};
		hitBox2.MouseDown += delegate
		{
			_isDown = true;
			_isLeft = false;
		};
		hitBox2.MouseUp += delegate
		{
			_isDown = false;
		};
		Add(hitBox2);
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		if (_isDown)
		{
			_container.Value += ((!_isLeft) ? 1 : (-1));
		}
	}

	public void AddItem(ushort graphic, ushort hue, string name, int x, int y, int index)
	{
		ItemView itemView = new ItemView(graphic, (ushort)((hue != 0) ? ((uint)(hue + 1)) : 0u));
		itemView.X = x;
		itemView.Y = y;
		ItemView itemView2 = itemView;
		itemView2.MouseDoubleClick += delegate(object? sender, MouseDoubleClickEventArgs e)
		{
			NetClient.Socket.Send_MenuResponse(base.LocalSerial, (ushort)base.ServerSerial, index, graphic, hue);
			Dispose();
			e.Result = true;
		};
		itemView2.SetTooltip(name);
		_container.Add(itemView2);
		_container.CalculateWidth();
		_slider.MaxValue = _container.MaxValue;
	}

	protected override void CloseWithRightClick()
	{
		base.CloseWithRightClick();
		NetClient.Socket.Send_MenuResponse(base.LocalSerial, (ushort)base.ServerSerial, 0, 0, 0);
	}
}
