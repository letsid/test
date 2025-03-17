using System.Collections.Generic;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Controls;

internal class ContextMenuShowMenu : Gump
{
	private class ContextMenuItem : Control
	{
		private static readonly RenderedText _moreMenuLabel = RenderedText.Create(">", ushort.MaxValue, byte.MaxValue, isunicode: true, FontStyle.BlackBorder, TEXT_ALIGN_TYPE.TS_LEFT, 0, 30);

		private readonly ContextMenuItemEntry _entry;

		private readonly Label _label;

		private readonly GumpPic _selectedPic;

		private readonly ContextMenuShowMenu _subMenu;

		public ContextMenuItem(ContextMenuShowMenu parent, ContextMenuItemEntry entry)
		{
			base.CanCloseWithRightClick = false;
			_entry = entry;
			Label label = new Label(entry.Text, isunicode: true, ushort.MaxValue, 0, byte.MaxValue, FontStyle.BlackBorder);
			label.X = 25;
			_label = label;
			Add(_label);
			if (entry.CanBeSelected)
			{
				_selectedPic = new GumpPic(3, 0, 2104, 0)
				{
					IsVisible = entry.IsSelected,
					IsEnabled = false
				};
				Add(_selectedPic);
			}
			base.Height = 25;
			_label.Y = (base.Height >> 1) - (_label.Height >> 1);
			if (_selectedPic != null)
			{
				_selectedPic.Y = (base.Height >> 1) - (_selectedPic.Height >> 1);
			}
			base.Width = _label.X + _label.Width + 20;
			if (base.Width < 100)
			{
				base.Width = 100;
			}
			if (_entry.Items != null && _entry.Items.Count != 0)
			{
				_subMenu = new ContextMenuShowMenu(_entry.Items);
				parent.Add(_subMenu);
				if (parent._subMenus == null)
				{
					parent._subMenus = new List<ContextMenuShowMenu>();
				}
				parent._subMenus.Add(_subMenu);
			}
			base.WantUpdateSize = false;
		}

		public override void Update(double totalTime, double frameTime)
		{
			base.Update(totalTime, frameTime);
			if (base.Width > _label.Width)
			{
				_label.Width = base.Width;
			}
			if (_subMenu == null)
			{
				return;
			}
			_subMenu.X = base.Width;
			_subMenu.Y = base.Y;
			if (base.MouseIsOver)
			{
				_subMenu.IsVisible = true;
				return;
			}
			Control control = UIManager.MouseOverControl?.Parent;
			while (control != null && control != _subMenu)
			{
				control = control.Parent;
			}
			_subMenu.IsVisible = control != null;
		}

		protected override void OnMouseUp(int x, int y, MouseButtonType button)
		{
			if (button == MouseButtonType.Left)
			{
				_entry.Action?.Invoke();
				base.RootParent?.Dispose();
				if (_entry.CanBeSelected)
				{
					_entry.IsSelected = !_entry.IsSelected;
					_selectedPic.IsVisible = _entry.IsSelected;
				}
				Mouse.CancelDoubleClick = true;
				Mouse.LastLeftButtonClickTime = 0u;
				base.OnMouseUp(x, y, button);
			}
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (!string.IsNullOrWhiteSpace(_label.Text) && base.MouseIsOver)
			{
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Gray), new Rectangle(x + 2, y + 5, base.Width - 4, base.Height - 10), hueVector);
			}
			base.Draw(batcher, x, y);
			if (_entry.Items != null && _entry.Items.Count != 0)
			{
				_moreMenuLabel.Draw(batcher, x + base.Width - _moreMenuLabel.Width, y + (base.Height >> 1) - (_moreMenuLabel.Height >> 1) - 1, 1f, 0);
			}
			return true;
		}
	}

	private readonly AlphaBlendControl _background;

	private List<ContextMenuShowMenu> _subMenus;

	public ContextMenuShowMenu(List<ContextMenuItemEntry> list)
		: base(0u, 0u)
	{
		base.WantUpdateSize = true;
		base.ModalClickOutsideAreaClosesThisControl = true;
		base.IsModal = true;
		base.LayerOrder = UILayer.Over;
		CanMove = false;
		AcceptMouseInput = true;
		_background = new AlphaBlendControl(0.7f);
		Add(_background);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			ContextMenuItem contextMenuItem = new ContextMenuItem(this, list[i]);
			if (i > 0)
			{
				contextMenuItem.Y = num;
			}
			if (_background.Width < contextMenuItem.Width)
			{
				_background.Width = contextMenuItem.Width;
			}
			_background.Height += contextMenuItem.Height;
			Add(contextMenuItem);
			num += contextMenuItem.Height;
		}
		foreach (ContextMenuItem item in FindControls<ContextMenuItem>())
		{
			if (item.Width < _background.Width)
			{
				item.Width = _background.Width;
			}
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		base.Update(totalTime, frameTime);
		base.WantUpdateSize = true;
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x - 1, y - 1, _background.Width + 1, _background.Height + 1, hueVector);
		return base.Draw(batcher, x, y);
	}

	public override bool Contains(int x, int y)
	{
		if (_background.Bounds.Contains(x, y))
		{
			return true;
		}
		if (_subMenus != null)
		{
			foreach (ContextMenuShowMenu subMenu in _subMenus)
			{
				if (subMenu.Contains(x - subMenu.X, y - subMenu.Y))
				{
					return true;
				}
			}
		}
		return false;
	}
}
