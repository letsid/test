using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class GridLootGump : Gump
{
	private class GridLootItem : Control
	{
		private readonly HitBox _hit;

		public GridLootItem(uint serial, int size)
		{
			base.LocalSerial = serial;
			Item item = World.Items.Get(serial);
			if (item == null)
			{
				Dispose();
				return;
			}
			CanMove = false;
			HSliderBar amount = new HSliderBar(0, 0, size, 1, (!item.ItemData.IsStackable) ? 1 : item.Amount, (!item.ItemData.IsStackable) ? 1 : item.Amount, HSliderBarStyle.MetalWidgetRecessedBar, hasText: true, 0, ushort.MaxValue, unicode: true, drawUp: true);
			Add(amount);
			amount.IsVisible = (amount.IsEnabled = amount.MaxValue > 1);
			AlphaBlendControl alphaBlendControl = new AlphaBlendControl();
			alphaBlendControl.Y = 15;
			alphaBlendControl.Width = size;
			alphaBlendControl.Height = size;
			Add(alphaBlendControl);
			_hit = new HitBox(0, 15, size, size, null, 0f);
			Add(_hit);
			if (World.ClientFeatures.TooltipsEnabled)
			{
				_hit.SetTooltip(item);
			}
			_hit.MouseUp += delegate(object? sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtonType.Left)
				{
					GameActions.GrabItem(item, (ushort)amount.Value);
				}
			};
			base.Width = alphaBlendControl.Width;
			base.Height = alphaBlendControl.Height + 15;
			base.WantUpdateSize = false;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			base.Draw(batcher, x, y);
			Item item = World.Items.Get(base.LocalSerial);
			Vector3 hueVector;
			if (item != null)
			{
				Rectangle bounds;
				Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(item.DisplayedGraphic, out bounds);
				Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(item.DisplayedGraphic);
				hueVector = ShaderHueTranslator.GetHueVector(item.Hue, item.ItemData.IsPartialHue, 1f);
				Point point = new Point(_hit.Width, _hit.Height);
				Point point2 = default(Point);
				if (realArtBounds.Width < _hit.Width)
				{
					point.X = realArtBounds.Width;
					point2.X = (_hit.Width >> 1) - (point.X >> 1);
				}
				if (realArtBounds.Height < _hit.Height)
				{
					point.Y = realArtBounds.Height;
					point2.Y = (_hit.Height >> 1) - (point.Y >> 1);
				}
				batcher.Draw(staticTexture, new Rectangle(x + point2.X, y + point2.Y + _hit.Y, point.X, point.Y), new Rectangle(bounds.X + realArtBounds.X, bounds.Y + realArtBounds.Y, realArtBounds.Width, realArtBounds.Height), hueVector);
			}
			hueVector = ShaderHueTranslator.GetHueVector(0);
			batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y + 15, base.Width, base.Height - 15, hueVector);
			if (_hit.MouseIsOver)
			{
				hueVector.Z = 0.7f;
				batcher.Draw(SolidColorTextureCache.GetTexture(Color.Yellow), new Rectangle(x + 1, y + 15, base.Width - 1, base.Height - 15), hueVector);
				hueVector.Z = 1f;
			}
			return true;
		}
	}

	private const int MAX_WIDTH = 300;

	private const int MAX_HEIGHT = 420;

	private static int _lastX = ((ProfileManager.CurrentProfile.GridLootType == 2) ? 200 : 100);

	private static int _lastY = 100;

	private readonly AlphaBlendControl _background;

	private readonly NiceButton _buttonPrev;

	private readonly NiceButton _buttonNext;

	private readonly NiceButton _setlootbag;

	private readonly Item _corpse;

	private int _currentPage = 1;

	private readonly Label _currentPageLabel;

	private readonly Label _corpseNameLabel;

	private readonly bool _hideIfEmpty;

	private int _pagesCount;

	public GridLootGump(uint local)
		: base(local, 0u)
	{
		_corpse = World.Items.Get(local);
		if (_corpse == null)
		{
			Dispose();
			return;
		}
		if (World.Player.ManualOpenedCorpses.Contains(base.LocalSerial))
		{
			World.Player.ManualOpenedCorpses.Remove(base.LocalSerial);
		}
		else if (World.Player.AutoOpenedCorpses.Contains(base.LocalSerial) && ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.SkipEmptyCorpse)
		{
			base.IsVisible = false;
			_hideIfEmpty = true;
		}
		base.X = _lastX;
		base.Y = _lastY;
		CanMove = true;
		AcceptMouseInput = true;
		base.WantUpdateSize = true;
		base.CanCloseWithRightClick = true;
		_background = new AlphaBlendControl();
		Add(_background);
		base.Width = _background.Width;
		base.Height = _background.Height;
		_setlootbag = new NiceButton(3, base.Height - 23, 100, 20, ButtonAction.Activate, ResGumps.SetLootBag)
		{
			ButtonParameter = 2,
			IsSelectable = false
		};
		Add(_setlootbag);
		_buttonPrev = new NiceButton(base.Width - 80, base.Height - 20, 40, 20, ButtonAction.Activate, ResGumps.Prev)
		{
			ButtonParameter = 0,
			IsSelectable = false
		};
		_buttonNext = new NiceButton(base.Width - 40, base.Height - 20, 40, 20, ButtonAction.Activate, ResGumps.Next)
		{
			ButtonParameter = 1,
			IsSelectable = false
		};
		_buttonNext.IsVisible = (_buttonPrev.IsVisible = false);
		Add(_buttonPrev);
		Add(_buttonNext);
		Label label = new Label("1", isunicode: true, 999, 0, byte.MaxValue, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label.X = base.Width / 2 - 5;
		label.Y = base.Height - 20;
		Label c = label;
		_currentPageLabel = label;
		Add(c);
		Label label2 = new Label(GetCorpseName(), isunicode: true, 1153, 300, byte.MaxValue, FontStyle.None, TEXT_ALIGN_TYPE.TS_CENTER);
		label2.Width = 300;
		label2.X = 0;
		label2.Y = 0;
		c = label2;
		_corpseNameLabel = label2;
		Add(c);
	}

	public override void OnButtonClick(int buttonID)
	{
		switch (buttonID)
		{
		case 0:
			_currentPage--;
			if (_currentPage <= 1)
			{
				_currentPage = 1;
				_buttonPrev.IsVisible = false;
			}
			_buttonNext.IsVisible = true;
			ChangePage(_currentPage);
			_currentPageLabel.Text = base.ActivePage.ToString();
			_currentPageLabel.X = base.Width / 2 - _currentPageLabel.Width / 2;
			break;
		case 1:
			_currentPage++;
			if (_currentPage >= _pagesCount)
			{
				_currentPage = _pagesCount;
				_buttonNext.IsVisible = false;
			}
			_buttonPrev.IsVisible = true;
			ChangePage(_currentPage);
			_currentPageLabel.Text = base.ActivePage.ToString();
			_currentPageLabel.X = base.Width / 2 - _currentPageLabel.Width / 2;
			break;
		case 2:
			GameActions.Print(ResGumps.TargetContainerToGrabItemsInto, 946, MessageType.Regular, 3);
			TargetManager.SetTargeting(CursorTarget.SetGrabBag, 0u, TargetType.Neutral);
			break;
		default:
			base.OnButtonClick(buttonID);
			break;
		}
	}

	protected override void UpdateContents()
	{
		int num = 20;
		int num2 = 20;
		foreach (GridLootItem item2 in base.Children.OfType<GridLootItem>())
		{
			item2.Dispose();
		}
		int num3 = 0;
		_pagesCount = 1;
		_background.Width = num;
		_background.Height = num2;
		int num4 = 1;
		int num5 = 0;
		for (LinkedObject linkedObject = _corpse.Items; linkedObject != null; linkedObject = linkedObject.Next)
		{
			Item item = (Item)linkedObject;
			if (item.IsLootable)
			{
				GridLootItem gridLootItem = new GridLootItem(item, 50);
				if (num >= 280)
				{
					num = 20;
					num4++;
					num2 += gridLootItem.Height + 20;
					if (num2 >= 360)
					{
						_pagesCount++;
						num2 = 20;
					}
				}
				gridLootItem.X = num;
				gridLootItem.Y = num2 + 20;
				Add(gridLootItem, _pagesCount);
				num += gridLootItem.Width + 20;
				num5++;
				num3++;
			}
		}
		_background.Width = 70 * num5 + 20;
		_background.Height = 60 + 70 * num4 + 40;
		if (_background.Height >= 380)
		{
			_background.Height = 420;
		}
		_background.Width = 300;
		if (base.ActivePage <= 1)
		{
			base.ActivePage = 1;
			_buttonNext.IsVisible = _pagesCount > 1;
			_buttonPrev.IsVisible = false;
		}
		else if (base.ActivePage >= _pagesCount)
		{
			base.ActivePage = _pagesCount;
			_buttonNext.IsVisible = false;
			_buttonPrev.IsVisible = _pagesCount > 1;
		}
		else if (base.ActivePage > 1 && base.ActivePage < _pagesCount)
		{
			_buttonNext.IsVisible = true;
			_buttonPrev.IsVisible = true;
		}
		if (num3 == 0)
		{
			GameActions.Print(ResGumps.CorpseIsEmpty, 946, MessageType.Regular, 3);
			Dispose();
		}
		else if (_hideIfEmpty && !base.IsVisible)
		{
			base.IsVisible = true;
		}
	}

	public override void Dispose()
	{
		if (_corpse != null && _corpse == SelectedObject.CorpseObject)
		{
			SelectedObject.CorpseObject = null;
		}
		_lastX = base.X;
		_lastY = base.Y;
		base.Dispose();
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		if (!base.IsVisible || base.IsDisposed)
		{
			return false;
		}
		base.Draw(batcher, x, y);
		Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
		batcher.DrawRectangle(SolidColorTextureCache.GetTexture(Color.Gray), x, y, base.Width, base.Height, hueVector);
		return true;
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (_corpse == null || _corpse.IsDestroyed || (_corpse.OnGround && _corpse.Distance > 3))
		{
			Dispose();
			return;
		}
		base.Update(totalTime, frameTime);
		if (!base.IsDisposed)
		{
			if (_background.Width < 100)
			{
				_background.Width = 100;
			}
			if (_background.Height < 120)
			{
				_background.Height = 120;
			}
			base.Width = _background.Width;
			base.Height = _background.Height;
			_buttonPrev.X = base.Width - 80;
			_buttonPrev.Y = base.Height - 23;
			_buttonNext.X = base.Width - 40;
			_buttonNext.Y = base.Height - 20;
			_setlootbag.X = 3;
			_setlootbag.Y = base.Height - 23;
			_currentPageLabel.X = base.Width / 2 - 5;
			_currentPageLabel.Y = base.Height - 20;
			_corpseNameLabel.Text = GetCorpseName();
			base.WantUpdateSize = true;
			if (_corpse != null && !_corpse.IsDestroyed && UIManager.MouseOverControl != null && (UIManager.MouseOverControl == this || UIManager.MouseOverControl.RootParent == this))
			{
				SelectedObject.Object = _corpse;
				SelectedObject.LastObject = _corpse;
				SelectedObject.CorpseObject = _corpse;
			}
		}
	}

	protected override void OnMouseExit(int x, int y)
	{
		if (_corpse != null && !_corpse.IsDestroyed)
		{
			SelectedObject.CorpseObject = null;
		}
	}

	private string GetCorpseName()
	{
		string name = _corpse.Name;
		if (name == null || name.Length <= 0)
		{
			return "a corpse";
		}
		return _corpse.Name;
	}
}
