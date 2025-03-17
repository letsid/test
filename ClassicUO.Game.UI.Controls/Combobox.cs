using System;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;

namespace ClassicUO.Game.UI.Controls;

internal class Combobox : Control
{
	private class ComboboxGump : Gump
	{
		private const int ELEMENT_HEIGHT = 15;

		private readonly Combobox _combobox;

		public ComboboxGump(int x, int y, int width, int maxHeight, string[] items, byte font, Combobox combobox)
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
			HoveredLabel[] array = new HoveredLabel[items.Length];
			bool isunicode = true;
			byte font2 = 1;
			for (int i = 0; i < items.Length; i++)
			{
				string text = items[i];
				if (text == null)
				{
					text = string.Empty;
				}
				HoveredLabel hoveredLabel = new HoveredLabel(text, isunicode, 1, 1153, 1153, 0, font2);
				hoveredLabel.X = 2;
				hoveredLabel.Y = i * 15;
				hoveredLabel.DrawBackgroundCurrentIndex = true;
				hoveredLabel.IsVisible = text.Length != 0;
				hoveredLabel.Tag = i;
				HoveredLabel hoveredLabel2 = hoveredLabel;
				hoveredLabel2.MouseUp += LabelOnMouseUp;
				array[i] = hoveredLabel2;
			}
			int num = Math.Min(maxHeight, array.Max((HoveredLabel o) => o.Y + o.Height));
			int num2 = Math.Max(width, array.Max((HoveredLabel o) => o.X + o.Width));
			ScrollArea scrollArea = new ScrollArea(0, 0, num2 + 15, num, normalScrollbar: true);
			HoveredLabel[] array2 = array;
			foreach (HoveredLabel hoveredLabel3 in array2)
			{
				hoveredLabel3.Width = num2;
				scrollArea.Add(hoveredLabel3);
			}
			Add(scrollArea);
			resizePic.Width = num2;
			resizePic.Height = num;
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

	private readonly string[] _items;

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

	public Combobox(int x, int y, int width, string[] items, int selected = -1, int maxHeight = 200, bool showArrow = true, string emptyString = "", byte font = 9)
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
		string text = ((selected > -1) ? items[selected] : emptyString);
		bool isunicode = true;
		byte font2 = 1;
		Label label = new Label(text, isunicode, 1, 0, font2);
		label.X = 2;
		label.Y = 5;
		Label c = label;
		_label = label;
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
			UIManager.Add(new ComboboxGump(base.ScreenCoordinateX, num, base.Width, _maxHeight, _items, _font, this));
			base.OnMouseUp(x, y, button);
		}
	}
}
