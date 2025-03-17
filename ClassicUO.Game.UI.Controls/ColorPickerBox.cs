using System;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Controls;

internal class ColorPickerBox : Gump
{
	private readonly int _cellHeight;

	private readonly int _cellWidth;

	private readonly int _columns;

	private readonly ushort[] _customPallete;

	private int _graduation;

	private int _selectedIndex;

	private ushort[] _hues;

	private bool _needToFileeBoxes = true;

	private readonly int _rows;

	public bool ShowLivePreview { get; set; }

	public ushort[] Hues
	{
		get
		{
			CreateTexture();
			return _hues;
		}
	}

	public int Graduation
	{
		get
		{
			return _graduation;
		}
		set
		{
			if (_graduation != value)
			{
				_graduation = value;
				_needToFileeBoxes = true;
				CreateTexture();
				this.ColorSelectedIndex.Raise();
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return _selectedIndex;
		}
		set
		{
			if (value >= 0 && value < _hues.Length && _selectedIndex != value)
			{
				_selectedIndex = value;
				this.ColorSelectedIndex.Raise();
			}
		}
	}

	public ushort SelectedHue
	{
		get
		{
			if (SelectedIndex >= 0 && SelectedIndex < _hues.Length)
			{
				return _hues[SelectedIndex];
			}
			return 0;
		}
	}

	public event EventHandler ColorSelectedIndex;

	public ColorPickerBox(int x, int y, int rows = 10, int columns = 20, int cellW = 8, int cellH = 8, ushort[] customPallete = null)
		: base(0u, 0u)
	{
		base.X = x;
		base.Y = y;
		base.Width = columns * cellW;
		base.Height = rows * cellH;
		_rows = rows;
		_columns = columns;
		_cellWidth = cellW;
		_cellHeight = cellH;
		_customPallete = customPallete;
		AcceptMouseInput = true;
		Graduation = 1;
		SelectedIndex = 0;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (_needToFileeBoxes)
		{
			CreateTexture();
		}
		if (ShowLivePreview)
		{
			int x = Mouse.Position.X - base.X - base.ParentX;
			int y = Mouse.Position.Y - base.Y - base.ParentY;
			if (base.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
			{
				SetSelectedIndex(x, y);
			}
		}
		base.Update(totalTime, frameTime);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Texture2D texture = SolidColorTextureCache.GetTexture(Color.White);
		Rectangle destinationRectangle = new Rectangle(0, 0, _cellWidth, _cellHeight);
		Vector3 hueVector;
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				hueVector = ShaderHueTranslator.GetHueVector(_hues[i * _columns + j]);
				destinationRectangle.X = x + j * _cellWidth;
				destinationRectangle.Y = y + i * _cellHeight;
				batcher.Draw(texture, destinationRectangle, hueVector);
			}
		}
		hueVector = ShaderHueTranslator.GetHueVector(0);
		if (_hues.Length > 1)
		{
			destinationRectangle.X = (int)((float)x + (float)(base.Width / _columns) * ((float)(SelectedIndex % _columns) + 0.5f) - 1f);
			destinationRectangle.Y = (int)((float)y + (float)(base.Height / _rows) * ((float)(SelectedIndex / _columns) + 0.5f) - 1f);
			destinationRectangle.Width = 2;
			destinationRectangle.Height = 2;
			batcher.Draw(SolidColorTextureCache.GetTexture(Color.White), destinationRectangle, hueVector);
		}
		return base.Draw(batcher, x, y);
	}

	protected override void OnMouseUp(int x, int y, MouseButtonType button)
	{
		if (button == MouseButtonType.Left)
		{
			SetSelectedIndex(x, y);
		}
	}

	private void SetSelectedIndex(int x, int y)
	{
		int num = x / (base.Width / _columns);
		int num2 = y / (base.Height / _rows);
		SelectedIndex = num + num2 * _columns;
	}

	private void CreateTexture()
	{
		if (!_needToFileeBoxes || base.IsDisposed)
		{
			return;
		}
		_needToFileeBoxes = false;
		int num = _rows * _columns;
		ushort num2 = (ushort)(Graduation + 1);
		if (_hues == null || num != _hues.Length)
		{
			_hues = new ushort[num];
		}
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0; j < _columns; j++)
			{
				ushort[] customPallete = _customPallete;
				ushort num3 = (ushort)(((customPallete != null) ? customPallete[i * _columns + j] : num2) + 1);
				_hues[i * _columns + j] = num3;
				num2 += 5;
			}
		}
	}
}
