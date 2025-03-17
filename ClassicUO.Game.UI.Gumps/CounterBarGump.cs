using System.Collections.Generic;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class CounterBarGump : Gump
{
	private class CounterItem : Control
	{
		private class ImageWithText : Control
		{
			private readonly Label _label;

			private ushort _graphic;

			private ushort _hue;

			private bool _partial;

			public ImageWithText()
			{
				CanMove = true;
				base.WantUpdateSize = true;
				AcceptMouseInput = false;
				Label label = new Label("", isunicode: true, 53, 0, 1, FontStyle.BlackBorder);
				label.X = 2;
				label.Y = base.Height - 15;
				_label = label;
				Add(_label);
			}

			public void ChangeGraphic(ushort graphic, ushort hue)
			{
				if (graphic != 0)
				{
					_graphic = graphic;
					_hue = hue;
					_partial = TileDataLoader.Instance.StaticData[graphic].IsPartialHue;
					_label.Y = base.Parent.Height - 15;
				}
				else
				{
					_graphic = 0;
				}
			}

			public override void Update(double totalTime, double frameTime)
			{
				base.Update(totalTime, frameTime);
				if (base.Parent != null)
				{
					base.Width = base.Parent.Width;
					base.Height = base.Parent.Height;
				}
			}

			public override bool Draw(UltimaBatcher2D batcher, int x, int y)
			{
				if (_graphic != 0)
				{
					Rectangle bounds;
					Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(_graphic, out bounds);
					if (staticTexture != null)
					{
						Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(_graphic);
						Vector3 hueVector = ShaderHueTranslator.GetHueVector(_hue, _partial, 1f);
						Point point = new Point(base.Width, base.Height);
						Point point2 = default(Point);
						if (realArtBounds.Width < base.Width)
						{
							point.X = realArtBounds.Width;
							point2.X = (base.Width >> 1) - (point.X >> 1);
						}
						if (realArtBounds.Height < base.Height)
						{
							point.Y = realArtBounds.Height;
							point2.Y = (base.Height >> 1) - (point.Y >> 1);
						}
						batcher.Draw(staticTexture, new Rectangle(x + point2.X, y + point2.Y, point.X, point.Y), new Rectangle(bounds.X + realArtBounds.X, bounds.Y + realArtBounds.Y, realArtBounds.Width, realArtBounds.Height), hueVector);
					}
				}
				return base.Draw(batcher, x, y);
			}

			public void SetAmount(string amount)
			{
				_label.Text = amount;
			}
		}

		private int _amount;

		private readonly ImageWithText _image;

		private uint _time;

		public ushort Graphic { get; private set; }

		public ushort Hue { get; private set; }

		public string Name { get; private set; }

		public CounterItem(int x, int y, int w, int h)
		{
			AcceptMouseInput = true;
			base.WantUpdateSize = false;
			CanMove = true;
			base.CanCloseWithRightClick = false;
			base.X = x;
			base.Y = y;
			base.Width = w;
			base.Height = h;
			_image = new ImageWithText();
			Add(_image);
			base.ContextMenu = new ContextMenuControl();
			base.ContextMenu.Add(ResGumps.UseObject, Use);
			base.ContextMenu.Add(ResGumps.Remove, RemoveItem);
		}

		public void SetGraphic(ushort graphic, ushort hue, string name)
		{
			_image.ChangeGraphic(graphic, hue);
			if (graphic != 0)
			{
				Name = name;
				Graphic = graphic;
				Hue = hue;
			}
		}

		public void RemoveItem()
		{
			_image?.ChangeGraphic(0, 0);
			_amount = 0;
			Graphic = 0;
		}

		public void Use()
		{
			if (Graphic == 0)
			{
				return;
			}
			Item item = World.Player.FindItemByLayer(Layer.Backpack);
			if (!(item == null))
			{
				Item item2 = item.FindItem(Graphic, Name, Hue);
				if (item2 != null)
				{
					GameActions.DoubleClick(item2);
				}
			}
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			switch (button)
			{
			case MouseButtonType.Left:
				if (ItemHold.Enabled)
				{
					SetGraphic(ItemHold.Graphic, ItemHold.Hue, ItemHold.Name);
					GameActions.DropItem(ItemHold.Serial, ItemHold.X, ItemHold.Y, 0, ItemHold.Container);
				}
				return;
			case MouseButtonType.Right:
				if (Keyboard.Alt && Graphic != 0)
				{
					RemoveItem();
					return;
				}
				break;
			}
			if (Graphic != 0)
			{
				base.OnMouseUp(x, y, button);
			}
		}

		protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				Use();
			}
			return true;
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (_time >= Time.Ticks)
			{
				return;
			}
			_time = Time.Ticks + 100;
			if (Graphic == 0)
			{
				_image.SetAmount(string.Empty);
				return;
			}
			_amount = 0;
			Item item = (Item)World.Player.Items;
			while (item != null)
			{
				if (item.ItemData.IsContainer && !item.IsEmpty && (int)item.Layer >= 1 && (int)item.Layer <= 24)
				{
					GetAmount(item, Graphic, Hue, Name, ref _amount);
				}
				item = (Item)item.Next;
			}
			if (ProfileManager.CurrentProfile.CounterBarDisplayAbbreviatedAmount && _amount >= ProfileManager.CurrentProfile.CounterBarAbbreviatedAmount)
			{
				_image.SetAmount(StringHelper.IntToAbbreviatedString(_amount));
			}
			else
			{
				_image.SetAmount(_amount.ToString());
			}
		}

		private static void GetAmount(Item parent, ushort graphic, ushort hue, string name, ref int amount)
		{
			if (parent == null)
			{
				return;
			}
			for (LinkedObject linkedObject = parent.Items; linkedObject != null; linkedObject = linkedObject.Next)
			{
				Item item = (Item)linkedObject;
				GetAmount(item, graphic, hue, name, ref amount);
				if (item.Graphic == graphic && item.Hue == hue && item.Name == name && item.Exists)
				{
					amount += item.Amount;
				}
			}
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			base.Draw(batcher, x, y);
			Texture2D texture = SolidColorTextureCache.GetTexture(base.MouseIsOver ? Color.Yellow : ((ProfileManager.CurrentProfile.CounterBarHighlightOnAmount && _amount < ProfileManager.CurrentProfile.CounterBarHighlightAmount && Graphic != 0) ? Color.Red : Color.Gray));
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.DrawRectangle(texture, x, y, base.Width, base.Height, hueVector);
			return true;
		}
	}

	private AlphaBlendControl _background;

	private int _rows;

	private int _columns;

	private int _rectSize;

	public override GumpType GumpType => GumpType.CounterBar;

	public CounterBarGump()
		: base(0u, 0u)
	{
	}

	public CounterBarGump(int x, int y, int rectSize = 30, int rows = 1, int columns = 1)
		: base(0u, 0u)
	{
		base.X = x;
		base.Y = y;
		if (rectSize < 30)
		{
			rectSize = 30;
		}
		else if (rectSize > 80)
		{
			rectSize = 80;
		}
		if (rows < 1)
		{
			rows = 1;
		}
		if (columns < 1)
		{
			columns = 1;
		}
		_rows = rows;
		_columns = columns;
		_rectSize = rectSize;
		BuildGump();
	}

	private void BuildGump()
	{
		CanMove = true;
		AcceptMouseInput = true;
		AcceptKeyboardInput = false;
		base.CanCloseWithRightClick = false;
		base.WantUpdateSize = false;
		base.Width = _rectSize * _columns + 1;
		base.Height = _rectSize * _rows + 1;
		AlphaBlendControl alphaBlendControl = new AlphaBlendControl(0.7f);
		alphaBlendControl.Width = base.Width;
		alphaBlendControl.Height = base.Height;
		AlphaBlendControl c = alphaBlendControl;
		_background = alphaBlendControl;
		Add(c);
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				Add(new CounterItem(j * _rectSize + 2, i * _rectSize + 2, _rectSize - 4, _rectSize - 4));
			}
		}
	}

	public void SetLayout(int size, int rows, int columns)
	{
		bool flag = false;
		if (rows > 30)
		{
			rows = 30;
		}
		if (columns > 30)
		{
			columns = 30;
		}
		if (size < 30)
		{
			size = 30;
		}
		else if (size > 80)
		{
			size = 80;
		}
		if (_rectSize != size)
		{
			flag = true;
			_rectSize = size;
		}
		if (rows < 1)
		{
			rows = 1;
		}
		if (_rows != rows)
		{
			flag = true;
			_rows = rows;
		}
		if (columns < 1)
		{
			columns = 1;
		}
		if (_columns != columns)
		{
			flag = true;
			_columns = columns;
		}
		if (flag)
		{
			ApplyLayout();
		}
	}

	private void ApplyLayout()
	{
		base.Width = _rectSize * _columns + 1;
		base.Height = _rectSize * _rows + 1;
		_background.Width = base.Width;
		_background.Height = base.Height;
		CounterItem[] controls = GetControls<CounterItem>();
		int[] array = new int[controls.Length];
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				int num = i * _columns + j;
				if (num < controls.Length)
				{
					CounterItem counterItem = controls[num];
					counterItem.X = j * _rectSize + 2;
					counterItem.Y = i * _rectSize + 2;
					counterItem.Width = _rectSize - 4;
					counterItem.Height = _rectSize - 4;
					counterItem.SetGraphic(counterItem.Graphic, counterItem.Hue, counterItem.Name);
					array[num] = -1;
				}
				else
				{
					Add(new CounterItem(j * _rectSize + 2, i * _rectSize + 2, _rectSize - 4, _rectSize - 4));
				}
			}
		}
		for (int k = 0; k < array.Length; k++)
		{
			int num2 = array[k];
			if (num2 >= 0 && num2 < controls.Length)
			{
				controls[k].Parent = null;
				controls[k].Dispose();
			}
		}
		SetInScreen();
	}

	public override void Save(XmlTextWriter writer)
	{
		base.Save(writer);
		writer.WriteAttributeString("rows", _rows.ToString());
		writer.WriteAttributeString("columns", _columns.ToString());
		writer.WriteAttributeString("rectsize", _rectSize.ToString());
		IEnumerable<CounterItem> enumerable = FindControls<CounterItem>();
		writer.WriteStartElement("controls");
		foreach (CounterItem item in enumerable)
		{
			writer.WriteStartElement("control");
			writer.WriteAttributeString("graphic", item.Graphic.ToString());
			writer.WriteAttributeString("hue", item.Hue.ToString());
			writer.WriteAttributeString("name", item.Name);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	public override void Restore(XmlElement xml)
	{
		base.Restore(xml);
		_rows = int.Parse(xml.GetAttribute("rows"));
		_columns = int.Parse(xml.GetAttribute("columns"));
		_rectSize = int.Parse(xml.GetAttribute("rectsize"));
		BuildGump();
		XmlElement xmlElement = xml["controls"];
		if (xmlElement != null)
		{
			CounterItem[] controls = GetControls<CounterItem>();
			int num = 0;
			foreach (XmlElement item in xmlElement.GetElementsByTagName("control"))
			{
				if (num < controls.Length)
				{
					controls[num++]?.SetGraphic(ushort.Parse(item.GetAttribute("graphic")), ushort.Parse(item.GetAttribute("hue")), item.GetAttribute("name"));
				}
				else
				{
					Log.Error(ResGumps.IndexOutOfbounds);
				}
			}
		}
		bool isEnabled = (base.IsVisible = ProfileManager.CurrentProfile.CounterBarEnabled);
		base.IsEnabled = isEnabled;
	}
}
