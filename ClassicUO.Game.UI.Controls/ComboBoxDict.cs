using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls;

internal class ComboBoxDict : Control
{
	private class ComboboxDictGump : Gump
	{
		private const int ELEMENT_HEIGHT = 15;

		private readonly ComboBoxDict _combobox;

		public ComboboxDictGump(int x, int y, int width, int maxHeight, Dictionary<int, string> items, byte font, ComboBoxDict combobox)
			: base(0u, 0u)
		{
			CanMove = false;
			AcceptMouseInput = true;
			base.X = x;
			base.Y = y;
			base.IsModal = true;
			base.LayerOrder = UILayer.Over;
			base.ModalClickOutsideAreaClosesThisControl = true;
			_combobox = combobox;
			ResizePic resizePic;
			Add(resizePic = new ResizePic(3000));
			resizePic.AcceptMouseInput = false;
			HoveredLabel[] array = new HoveredLabel[items.Count];
			bool isunicode = true;
			byte font2 = 1;
			int num = 0;
			items = items.OrderBy((KeyValuePair<int, string> value) => value.Value).ToDictionary((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value);
			foreach (KeyValuePair<int, string> item in items)
			{
				string text = item.Value;
				if (text == null)
				{
					text = string.Empty;
				}
				HoveredLabel hoveredLabel = new HoveredLabel(text, isunicode, 1, 1153, 1153, 0, font2);
				hoveredLabel.X = 2;
				hoveredLabel.Y = num * 15;
				hoveredLabel.DrawBackgroundCurrentIndex = true;
				hoveredLabel.IsVisible = text.Length != 0;
				hoveredLabel.Tag = item.Key;
				HoveredLabel hoveredLabel2 = hoveredLabel;
				hoveredLabel2.MouseUp += LabelOnMouseUp;
				array[num] = hoveredLabel2;
				num++;
			}
			int num2 = Math.Min(maxHeight, array.Max((HoveredLabel o) => o.Y + o.Height));
			int num3 = Math.Max(width, array.Max((HoveredLabel o) => o.X + o.Width));
			ScrollArea scrollArea = new ScrollArea(0, 0, num3 + 15, num2, normalScrollbar: true);
			HoveredLabel[] array2 = array;
			foreach (HoveredLabel hoveredLabel3 in array2)
			{
				hoveredLabel3.Width = num3;
				scrollArea.Add(hoveredLabel3);
			}
			Add(scrollArea);
			resizePic.Width = num3;
			resizePic.Height = num2;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (batcher.ClipBegin(x, y, base.Width, base.Height))
			{
				base.Draw(batcher, x, y);
				batcher.ClipEnd();
			}
			return true;
		}

		private void LabelOnMouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtonType.Left)
			{
				_combobox.SelectedIndex = (int)((Label)sender).Tag;
				Dispose();
			}
		}
	}

	private readonly byte _font;

	private readonly Dictionary<int, string> _items;

	private readonly Label _label;

	private readonly int _maxHeight;

	private int _selectedIndex;

	public int SelectedIndex
	{
		get
		{
			return _selectedIndex;
		}
		set
		{
			_selectedIndex = value;
			if (_items != null)
			{
				_label.Text = _items[value];
				this.OnOptionSelected?.Invoke(this, value);
			}
		}
	}

	public event EventHandler<int> OnOptionSelected;

	public ComboBoxDict(int x, int y, int width, Dictionary<int, string> items, int selected = -1, int maxHeight = 200, bool showArrow = true, string emptyString = "", byte font = 9)
	{
		base.X = x;
		base.Y = y;
		base.Width = width;
		base.Height = 25;
		SelectedIndex = selected;
		_font = font;
		_items = items;
		_maxHeight = maxHeight;
		ResizePic resizePic = new ResizePic(3000);
		resizePic.Width = width;
		resizePic.Height = base.Height;
		Add(resizePic);
		items.TryGetValue(selected, out var value);
		Label c;
		if (string.IsNullOrEmpty(value))
		{
			Label label = new Label("Makro ohne Funktion", isunicode: true, 1, 0, 1);
			label.X = 2;
			label.Y = 5;
			c = label;
			_label = label;
			Add(c);
			if (showArrow)
			{
				Add(new GumpPic(width - 18, 2, 252, 0));
			}
			return;
		}
		string text = ((selected > -1) ? items[selected] : emptyString);
		bool isunicode = true;
		byte font2 = 1;
		Label label2 = new Label(text, isunicode, 1, 0, font2);
		label2.X = 2;
		label2.Y = 5;
		c = label2;
		_label = label2;
		Add(c);
		if (showArrow)
		{
			Add(new GumpPic(width - 18, 2, 252, 0));
		}
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (batcher.ClipBegin(x, y, base.Width, base.Height))
		{
			base.Draw(batcher, x, y);
			batcher.ClipEnd();
		}
		return true;
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			int num = base.ScreenCoordinateY + base.Offset.Y;
			if (num < 0)
			{
				num = 0;
			}
			else if (num + _maxHeight > Client.Game.Window.ClientBounds.Height)
			{
				num = Client.Game.Window.ClientBounds.Height - _maxHeight;
			}
			UIManager.Add(new ComboboxDictGump(base.ScreenCoordinateX, num, base.Width, _maxHeight, _items, _font, this));
			base.OnMouseUp(x, y, button);
		}
	}
}
