using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps;

internal class ShopGump : Gump
{
	private enum ButtonScroll
	{
		None = -1,
		LeftScrollUp,
		LeftScrollDown,
		RightScrollUp,
		RightScrollDown
	}

	private enum Buttons
	{
		Accept,
		Clear
	}

	private class ShopItem : Control
	{
		private readonly Label _amountLabel;

		private readonly Label _name;

		internal string ShopItemName => _name.Text;

		public int Amount
		{
			get
			{
				return int.Parse(_amountLabel.Text);
			}
			set
			{
				_amountLabel.Text = value.ToString();
			}
		}

		public bool IsSelected
		{
			set
			{
				foreach (Label item in base.Children.OfType<Label>())
				{
					item.Hue = (ushort)(value ? 33u : 537u);
				}
			}
		}

		public uint Price { get; }

		public ushort Hue { get; }

		public ushort Graphic { get; }

		public string Name { get; }

		public bool NameFromCliloc { get; set; }

		public ShopItem(uint serial, ushort graphic, ushort hue, int count, uint price, string name)
		{
			base.LocalSerial = serial;
			Graphic = graphic;
			Hue = hue;
			Price = price;
			Name = name;
			ResizePicLine resizePicLine = new ResizePicLine(57);
			resizePicLine.X = 10;
			resizePicLine.Width = 190;
			ResizePicLine resizePicLine2 = resizePicLine;
			Add(resizePicLine2);
			int num = 15;
			string arg = StringHelper.CapitalizeAllWords(Name);
			if (SerialHelper.IsValid(serial))
			{
				string text = string.Format(ResGumps.Item0Price1, arg, Price);
				Label label = new Label(text, isunicode: true, 537, 110, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, ishtml: true);
				label.X = 55;
				label.Y = num;
				Label c = label;
				_name = label;
				Add(c);
				int num2 = Math.Max(_name.Height, 35) + 10;
				if (SerialHelper.IsItem(serial))
				{
					num2 = Math.Max(TileDataLoader.Instance.StaticData[graphic].Height, num2);
				}
				Label label2 = new Label(count.ToString(), isunicode: true, 537, 35, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_RIGHT);
				label2.X = 168;
				label2.Y = num + (num2 >> 2);
				c = label2;
				_amountLabel = label2;
				Add(c);
				base.Width = 220;
				base.Height = Math.Max(50, num2) + resizePicLine2.Height;
				base.WantUpdateSize = false;
				if (World.ClientFeatures.TooltipsEnabled)
				{
					SetTooltip(base.LocalSerial);
				}
				Amount = count;
			}
		}

		private static byte GetAnimGroup(ushort graphic)
		{
			return AnimationsLoader.Instance.GetGroupIndex(graphic) switch
			{
				ANIMATION_GROUPS.AG_LOW => 2, 
				ANIMATION_GROUPS.AG_HIGHT => 1, 
				ANIMATION_GROUPS.AG_PEOPLE => 4, 
				_ => 0, 
			};
		}

		private static AnimationDirection GetMobileAnimationDirection(ushort graphic, ref ushort hue, byte dirIndex)
		{
			if (graphic >= 4096)
			{
				return null;
			}
			byte animGroup = GetAnimGroup(graphic);
			AnimationDirection animDir = AnimationsLoader.Instance.DataIndex[graphic].Groups[animGroup].Direction[dirIndex];
			for (int i = 0; i < 2; i++)
			{
				if (animDir.FrameCount != 0)
				{
					break;
				}
				AnimationsLoader.Instance.LoadAnimationFrames(graphic, animGroup, dirIndex, ref animDir);
			}
			return animDir;
		}

		public void SetName(string s, bool new_name)
		{
			_name.Text = (new_name ? $"{s}: {Price}" : string.Format(ResGumps.Item0Price1, s, Price));
			base.WantUpdateSize = false;
		}

		protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
		{
			return true;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			if (SerialHelper.IsMobile(base.LocalSerial))
			{
				ushort hue = Hue;
				AnimationDirection mobileAnimationDirection = GetMobileAnimationDirection(Graphic, ref hue, 1);
				if (mobileAnimationDirection != null && mobileAnimationDirection.SpriteInfos != null && mobileAnimationDirection.FrameCount != 0)
				{
					Vector3 hueVector = ShaderHueTranslator.GetHueVector(hue, TileDataLoader.Instance.StaticData[Graphic].IsPartialHue, 1f);
					batcher.Draw(mobileAnimationDirection.SpriteInfos[0].Texture, new Rectangle(x - 3, y + 5 + 15, Math.Min(mobileAnimationDirection.SpriteInfos[0].UV.Width, 45), Math.Min(mobileAnimationDirection.SpriteInfos[0].UV.Height, 45)), mobileAnimationDirection.SpriteInfos[0].UV, hueVector);
				}
			}
			else if (SerialHelper.IsItem(base.LocalSerial))
			{
				Rectangle bounds;
				Texture2D staticTexture = ArtLoader.Instance.GetStaticTexture(Graphic, out bounds);
				Vector3 hueVector = ShaderHueTranslator.GetHueVector(Hue, TileDataLoader.Instance.StaticData[Graphic].IsPartialHue, 1f);
				Rectangle realArtBounds = ArtLoader.Instance.GetRealArtBounds(Graphic);
				Point point = new Point(50, base.Height);
				Point point2 = default(Point);
				if (realArtBounds.Width < 50)
				{
					point.X = realArtBounds.Width;
					point2.X = 25 - (point.X >> 1);
				}
				if (realArtBounds.Height < base.Height)
				{
					point.Y = realArtBounds.Height;
					point2.Y = (base.Height >> 1) - (point.Y >> 1);
				}
				batcher.Draw(staticTexture, new Rectangle(x + point2.X - 5, y + point2.Y + 10, point.X, point.Y), new Rectangle(bounds.X + realArtBounds.X, bounds.Y + realArtBounds.Y, realArtBounds.Width, realArtBounds.Height), hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private class TransactionItem : Control
	{
		private readonly Label _amountLabel;

		public ushort Graphic { get; }

		public ushort Hue { get; }

		public ushort Price { get; }

		public int Amount
		{
			get
			{
				return int.Parse(_amountLabel.Text);
			}
			set
			{
				_amountLabel.Text = value.ToString();
			}
		}

		public event EventHandler OnIncreaseButtomClicked;

		public event EventHandler OnDecreaseButtomClicked;

		public TransactionItem(uint serial, ushort graphic, ushort hue, int amount, ushort price, string realname)
		{
			TransactionItem transactionItem = this;
			base.LocalSerial = serial;
			Graphic = graphic;
			Hue = hue;
			Price = price;
			Label label = new Label(realname, isunicode: true, 543, 140, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_LEFT, ishtml: true);
			label.X = 50;
			label.Y = 0;
			Label label2 = label;
			Add(label);
			Label label3 = new Label(amount.ToString(), isunicode: true, 543, 35, 1, FontStyle.None, TEXT_ALIGN_TYPE.TS_RIGHT);
			label3.X = 10;
			label3.Y = 0;
			Label c = label3;
			_amountLabel = label3;
			Add(c);
			Button button = new Button(0, 55, 55, 0, "", 0);
			button.X = 190;
			button.Y = 5;
			button.ButtonAction = ButtonAction.Activate;
			button.ContainsByBounds = true;
			Button button2 = button;
			Add(button);
			int status = 0;
			float t0 = Time.Ticks;
			bool pressedAdd = false;
			button2.MouseOver += delegate
			{
				if (status == 2 && pressedAdd && (float)Time.Ticks > t0)
				{
					t0 = Time.Ticks + (45 - Control._StepChanger);
					transactionItem.OnButtonClick(0);
					Control._StepsDone++;
					if (Control._StepChanger < 45 && Control._StepsDone % 3 == 0)
					{
						Control._StepChanger += 2;
					}
				}
			};
			button2.MouseDown += delegate(object? sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtonType.Left)
				{
					pressedAdd = true;
					Control._StepChanger = 0;
					status = 2;
					t0 = Time.Ticks + 500;
				}
			};
			button2.MouseUp += delegate
			{
				pressedAdd = false;
				status = 0;
				Control._StepsDone = (Control._StepChanger = 1);
			};
			Button button3 = new Button(1, 56, 56, 0, "", 0);
			button3.X = 210;
			button3.Y = 5;
			button3.ButtonAction = ButtonAction.Activate;
			button3.ContainsByBounds = true;
			Button button4 = button3;
			Add(button3);
			bool pressedRemove = false;
			button4.MouseOver += delegate
			{
				if (status == 2 && pressedRemove && (float)Time.Ticks > t0)
				{
					t0 = Time.Ticks + (45 - Control._StepChanger);
					transactionItem.OnButtonClick(1);
					Control._StepsDone++;
					if (Control._StepChanger < 45 && Control._StepsDone % 3 == 0)
					{
						Control._StepChanger += 2;
					}
				}
			};
			button4.MouseDown += delegate(object? sender, MouseEventArgs e)
			{
				if (e.Button == MouseButtonType.Left)
				{
					pressedRemove = true;
					Control._StepChanger = 0;
					status = 2;
					t0 = Time.Ticks + 500;
				}
			};
			button4.MouseUp += delegate
			{
				pressedRemove = false;
				status = 0;
				Control._StepsDone = (Control._StepChanger = 1);
			};
			base.Width = 245;
			base.Height = label2.Height;
			base.WantUpdateSize = false;
			Amount = amount;
		}

		public override void OnButtonClick(int buttonID)
		{
			switch (buttonID)
			{
			case 0:
				this.OnIncreaseButtomClicked?.Invoke(this, EventArgs.Empty);
				break;
			case 1:
				this.OnDecreaseButtomClicked?.Invoke(this, EventArgs.Empty);
				break;
			}
		}
	}

	private class ResizePicLine : Control
	{
		private readonly ushort _graphic;

		public ResizePicLine(ushort graphic)
		{
			_graphic = graphic;
			CanMove = true;
			base.CanCloseWithRightClick = true;
			GumpsLoader.Instance.GetGumpTexture(_graphic, out var bounds);
			GumpsLoader.Instance.GetGumpTexture((ushort)(_graphic + 1), out var bounds2);
			GumpsLoader.Instance.GetGumpTexture((ushort)(_graphic + 2), out var bounds3);
			base.Height = Math.Max(bounds.Height, Math.Max(bounds2.Height, bounds3.Height));
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(_graphic, out bounds);
			Rectangle bounds2;
			Texture2D gumpTexture2 = GumpsLoader.Instance.GetGumpTexture((ushort)(_graphic + 1), out bounds2);
			Rectangle bounds3;
			Texture2D gumpTexture3 = GumpsLoader.Instance.GetGumpTexture((ushort)(_graphic + 2), out bounds3);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0, partial: false, base.Alpha, gump: true);
			int width = base.Width - bounds.Width - bounds3.Width;
			batcher.Draw(gumpTexture, new Vector2(x, y), bounds, hueVector);
			batcher.DrawTiled(gumpTexture2, new Rectangle(x + bounds.Width, y, width, bounds2.Height), bounds2, hueVector);
			batcher.Draw(gumpTexture3, new Vector2(x + base.Width - bounds3.Width, y), bounds3, hueVector);
			return base.Draw(batcher, x, y);
		}
	}

	private class GumpPicTexture : Control
	{
		private readonly bool _tiled;

		private readonly ushort _graphic;

		private readonly Rectangle _rect;

		public GumpPicTexture(ushort graphic, int x, int y, Rectangle bounds, bool tiled)
		{
			CanMove = true;
			AcceptMouseInput = true;
			_graphic = graphic;
			_rect = bounds;
			base.X = x;
			base.Y = y;
			base.Width = bounds.Width;
			base.Height = bounds.Height;
			base.WantUpdateSize = false;
			_tiled = tiled;
		}

		public override bool Draw(UltimaBatcher2D batcher, int x, int y)
		{
			Rectangle bounds;
			Texture2D gumpTexture = GumpsLoader.Instance.GetGumpTexture(_graphic, out bounds);
			Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);
			if (_tiled)
			{
				batcher.DrawTiled(gumpTexture, new Rectangle(x, y, base.Width, base.Height), new Rectangle(bounds.X + _rect.X, bounds.Y + _rect.Y, _rect.Width, _rect.Height), hueVector);
			}
			else
			{
				batcher.Draw(gumpTexture, new Rectangle(x, y, base.Width, base.Height), new Rectangle(bounds.X + _rect.X, bounds.Y + _rect.Y, _rect.Width, _rect.Height), hueVector);
			}
			return base.Draw(batcher, x, y);
		}
	}

	private const int SCROLL_DELAY = 60;

	private uint _lastMouseEventTime = Time.Ticks;

	private ButtonScroll _buttonScroll = ButtonScroll.None;

	private readonly Dictionary<uint, ShopItem> _shopItems;

	private readonly ScrollArea _shopScrollArea;

	private readonly ScrollArea _transactionScrollArea;

	private readonly Label _totalLabel;

	private readonly Label _playerGoldLabel;

	private readonly DataBox _transactionDataBox;

	private readonly Dictionary<uint, TransactionItem> _transactionItems;

	private bool _updateTotal;

	public bool IsBuyGump { get; }

	public ShopGump(uint serial, bool isBuyGump, int x, int y)
		: base(serial, 0u)
	{
		int vendorGumpHeight = ProfileManager.CurrentProfile.VendorGumpHeight;
		base.X = x;
		base.Y = y;
		AcceptMouseInput = false;
		AcceptKeyboardInput = true;
		CanMove = true;
		base.CanCloseWithRightClick = true;
		IsBuyGump = isBuyGump;
		_transactionItems = new Dictionary<uint, TransactionItem>();
		_shopItems = new Dictionary<uint, ShopItem>();
		_updateTotal = false;
		base.WantUpdateSize = true;
		ushort num = (ushort)(isBuyGump ? 2160 : 2162);
		ushort num2 = (ushort)(isBuyGump ? 2161 : 2163);
		GumpsLoader.Instance.GetGumpTexture(num, out var bounds);
		GumpsLoader.Instance.GetGumpTexture(num2, out var bounds2);
		Rectangle bounds3 = new Rectangle(0, 0, bounds.Width, 64);
		GumpPicTexture gumpPicTexture = new GumpPicTexture(num, 0, 0, bounds3, tiled: false);
		Add(gumpPicTexture);
		bounds3.Y += 64;
		bounds3.Height = bounds.Height - 180;
		GumpPicTexture leftmiddle = new GumpPicTexture(num, 0, 64, bounds3, tiled: true);
		int num3 = vendorGumpHeight - leftmiddle.Height;
		leftmiddle.Height = vendorGumpHeight;
		Add(leftmiddle);
		bounds3.Y += bounds3.Height;
		bounds3.Height = 116;
		GumpPicTexture leftBottom = new GumpPicTexture(num, 0, leftmiddle.Y + leftmiddle.Height, bounds3, tiled: false);
		Add(leftBottom);
		int x2 = bounds.Width - 32;
		int num4 = bounds.Height / 2 - 32;
		bounds3 = new Rectangle(0, 0, bounds2.Width, 64);
		GumpPicTexture gumpPicTexture2 = new GumpPicTexture(num2, x2, num4, bounds3, tiled: false);
		Add(gumpPicTexture2);
		bounds3.Y += 64;
		bounds3.Height = bounds2.Height - 157;
		GumpPicTexture rightMiddle = new GumpPicTexture(num2, x2, num4 + 64, bounds3, tiled: true);
		rightMiddle.Height += num3;
		Add(rightMiddle);
		bounds3.Y += bounds3.Height;
		bounds3.Height = 93;
		GumpPicTexture rightBottom = new GumpPicTexture(num2, x2, rightMiddle.Y + rightMiddle.Height, bounds3, tiled: false);
		Add(rightBottom);
		_shopScrollArea = new ScrollArea(32, leftmiddle.Y, bounds.Width - 64 + 5, leftmiddle.Height + 50, normalScrollbar: false, leftmiddle.Height);
		Add(_shopScrollArea);
		_transactionScrollArea = new ScrollArea(16 + gumpPicTexture2.X, 64 + gumpPicTexture2.Y, bounds2.Width - 64 + 16 + 5, rightMiddle.Height, normalScrollbar: false);
		Add(_transactionScrollArea);
		_transactionDataBox = new DataBox(0, 0, 1, 1);
		_transactionDataBox.WantUpdateSize = true;
		_transactionScrollArea.Add(_transactionDataBox);
		Label label = new Label("0", isunicode: true, 902, 0, 1);
		label.X = 32 + gumpPicTexture2.X + 32 + 4;
		label.Y = rightBottom.Y + rightBottom.Height - 96 + 15;
		_totalLabel = label;
		Add(_totalLabel);
		if (isBuyGump)
		{
			Label label2 = new Label(World.Player.Gold.ToString(), isunicode: true, 902, 0, 1);
			label2.X = _totalLabel.X + 120;
			label2.Y = _totalLabel.Y;
			_playerGoldLabel = label2;
			Add(_playerGoldLabel);
		}
		else
		{
			_totalLabel.X = gumpPicTexture2.X + gumpPicTexture2.Width - 96;
		}
		Button obj = new Button(2, 2094, 2095, 0, "", 0)
		{
			ButtonAction = ButtonAction.Activate
		};
		obj.X = bounds.Width / 2 - 10;
		obj.Y = leftBottom.Y + leftBottom.Height - 5;
		Button expander = obj;
		Add(expander);
		HitBox accept = new HitBox(32 + gumpPicTexture2.X, rightBottom.Y + rightBottom.Height - 50, 34, 30, "Accept", 0f);
		HitBox clear = new HitBox(accept.X + 175, accept.Y, 20, 20, "Clear", 0f);
		accept.MouseUp += delegate
		{
			OnButtonClick(0);
		};
		clear.MouseUp += delegate
		{
			OnButtonClick(1);
		};
		Add(accept);
		Add(clear);
		HitBox hitBox = new HitBox(gumpPicTexture.X + gumpPicTexture.Width - 50, gumpPicTexture.Y + gumpPicTexture.Height - 18, 18, 16, "Scroll Up", 0f);
		HitBox leftDown = new HitBox(hitBox.X, leftBottom.Y, 18, 16, "Scroll Down", 0f);
		HitBox hitBox2 = new HitBox(gumpPicTexture2.X + gumpPicTexture2.Width - 50, gumpPicTexture2.Y + gumpPicTexture2.Height - 18, 18, 16, "Scroll Up", 0f);
		HitBox rightDown = new HitBox(hitBox2.X, rightBottom.Y, 18, 16, "Scroll Down", 0f);
		hitBox.MouseUp += ButtonMouseUp;
		leftDown.MouseUp += ButtonMouseUp;
		hitBox2.MouseUp += ButtonMouseUp;
		rightDown.MouseUp += ButtonMouseUp;
		hitBox.MouseDown += delegate
		{
			_buttonScroll = ButtonScroll.LeftScrollUp;
		};
		leftDown.MouseDown += delegate
		{
			_buttonScroll = ButtonScroll.LeftScrollDown;
		};
		hitBox2.MouseDown += delegate
		{
			_buttonScroll = ButtonScroll.RightScrollUp;
		};
		rightDown.MouseDown += delegate
		{
			_buttonScroll = ButtonScroll.RightScrollDown;
		};
		Add(hitBox);
		Add(leftDown);
		Add(hitBox2);
		Add(rightDown);
		bool is_pressing = false;
		int initial_height = 0;
		int initialHeightRight = 0;
		int minHeight = bounds.Height - 180;
		int minHeightRight = bounds2.Height - 157;
		expander.MouseDown += delegate
		{
			is_pressing = true;
			initial_height = leftmiddle.Height;
			initialHeightRight = rightMiddle.Height;
		};
		expander.MouseUp += delegate
		{
			is_pressing = false;
		};
		expander.MouseOver += delegate
		{
			int y2 = Mouse.LDragOffset.Y;
			if (is_pressing && y2 != 0)
			{
				leftmiddle.Height = initial_height + y2;
				if (leftmiddle.Height < minHeight)
				{
					leftmiddle.Height = minHeight;
				}
				else if (leftmiddle.Height > 640)
				{
					leftmiddle.Height = 640;
				}
				rightMiddle.Height = initialHeightRight + y2;
				if (rightMiddle.Height < minHeightRight)
				{
					rightMiddle.Height = minHeightRight;
				}
				else if (rightMiddle.Height > 576)
				{
					rightMiddle.Height = 576;
				}
				ProfileManager.CurrentProfile.VendorGumpHeight = leftmiddle.Height;
				leftBottom.Y = leftmiddle.Y + leftmiddle.Height;
				expander.Y = leftBottom.Y + leftBottom.Height - 5;
				rightBottom.Y = rightMiddle.Y + rightMiddle.Height;
				_shopScrollArea.Height = leftmiddle.Height + 50;
				_shopScrollArea.ScrollMaxHeight = leftmiddle.Height;
				_transactionDataBox.Height = (_transactionScrollArea.Height = rightMiddle.Height);
				_totalLabel.Y = rightBottom.Y + rightBottom.Height - 96 + 15;
				accept.Y = (clear.Y = rightBottom.Y + rightBottom.Height - 50);
				leftDown.Y = leftBottom.Y;
				rightDown.Y = rightBottom.Y;
				if (_playerGoldLabel != null)
				{
					_playerGoldLabel.Y = _totalLabel.Y;
				}
				_transactionDataBox.ReArrangeChildren();
				base.WantUpdateSize = true;
			}
		};
	}

	private void ButtonMouseUp(object sender, MouseEventArgs e)
	{
		_buttonScroll = ButtonScroll.None;
	}

	public void AddItem(uint serial, ushort graphic, ushort hue, ushort amount, uint price, string name, bool fromcliloc)
	{
		int num = _shopScrollArea.Children.Count - 1;
		int num2 = ((num > 0) ? _shopScrollArea.Children[num].Bounds.Bottom : 0);
		ShopItem shopItem = new ShopItem(serial, graphic, hue, amount, price, name);
		shopItem.X = 5;
		shopItem.Y = num2 + 2;
		shopItem.NameFromCliloc = fromcliloc;
		ShopItem shopItem2 = shopItem;
		_shopScrollArea.Add(shopItem2);
		shopItem2.MouseUp += ShopItem_MouseClick;
		shopItem2.MouseDoubleClick += ShopItem_MouseDoubleClick;
		_shopItems.Add(serial, shopItem2);
	}

	public void SetNameTo(Item item, string name)
	{
		if (!string.IsNullOrEmpty(name) && _shopItems.TryGetValue(item, out var value))
		{
			value.SetName(name, new_name: false);
		}
	}

	public override void Update(double totalTime, double frameTime)
	{
		if (!World.InGame || base.IsDisposed)
		{
			return;
		}
		if (_shopItems.Count == 0)
		{
			Dispose();
		}
		if (_buttonScroll != ButtonScroll.None)
		{
			ProcessListScroll();
		}
		if (_updateTotal)
		{
			int num = 0;
			foreach (TransactionItem value in _transactionItems.Values)
			{
				num += value.Amount * value.Price;
			}
			_totalLabel.Text = num.ToString();
			_updateTotal = false;
		}
		if (_playerGoldLabel != null)
		{
			_playerGoldLabel.Text = World.Player.Gold.ToString();
		}
		base.Update(totalTime, frameTime);
	}

	public override bool Draw(UltimaBatcher2D batcher, int x, int y)
	{
		return base.Draw(batcher, x, y);
	}

	private void ProcessListScroll()
	{
		if (Time.Ticks - _lastMouseEventTime >= 60)
		{
			switch (_buttonScroll)
			{
			case ButtonScroll.LeftScrollUp:
				_shopScrollArea.Scroll(isup: true);
				break;
			case ButtonScroll.LeftScrollDown:
				_shopScrollArea.Scroll(isup: false);
				break;
			case ButtonScroll.RightScrollUp:
				_transactionScrollArea.Scroll(isup: true);
				break;
			case ButtonScroll.RightScrollDown:
				_transactionScrollArea.Scroll(isup: false);
				break;
			}
			_lastMouseEventTime = Time.Ticks;
		}
	}

	private void ShopItem_MouseDoubleClick(object sender, MouseDoubleClickEventArgs e)
	{
		ShopItem shopItem = (ShopItem)sender;
		if (shopItem.Amount > 0)
		{
			int num = ((!Keyboard.Shift) ? 1 : shopItem.Amount);
			if (_transactionItems.TryGetValue(shopItem.LocalSerial, out var value))
			{
				value.Amount += num;
			}
			else
			{
				value = new TransactionItem(shopItem.LocalSerial, shopItem.Graphic, shopItem.Hue, num, (ushort)shopItem.Price, shopItem.ShopItemName);
				value.OnIncreaseButtomClicked += TransactionItem_OnIncreaseButtomClicked;
				value.OnDecreaseButtomClicked += TransactionItem_OnDecreaseButtomClicked;
				_transactionDataBox.Add(value);
				_transactionItems.Add(shopItem.LocalSerial, value);
				_transactionDataBox.WantUpdateSize = true;
				_transactionDataBox.ReArrangeChildren();
			}
			shopItem.Amount -= num;
			_updateTotal = true;
		}
	}

	private void TransactionItem_OnDecreaseButtomClicked(object sender, EventArgs e)
	{
		TransactionItem transactionItem = (TransactionItem)sender;
		int num = ((!Keyboard.Shift) ? 1 : transactionItem.Amount);
		if (transactionItem.Amount > 0)
		{
			_shopItems[transactionItem.LocalSerial].Amount += num;
			transactionItem.Amount -= num;
		}
		if (transactionItem.Amount <= 0)
		{
			RemoveTransactionItem(transactionItem);
		}
		_updateTotal = true;
	}

	private void RemoveTransactionItem(TransactionItem transactionItem)
	{
		_shopItems[transactionItem.LocalSerial].Amount += transactionItem.Amount;
		transactionItem.OnIncreaseButtomClicked -= TransactionItem_OnIncreaseButtomClicked;
		transactionItem.OnDecreaseButtomClicked -= TransactionItem_OnDecreaseButtomClicked;
		_transactionItems.Remove(transactionItem.LocalSerial);
		transactionItem.Dispose();
		_transactionDataBox.WantUpdateSize = true;
		_transactionDataBox.ReArrangeChildren();
		_updateTotal = true;
	}

	private void TransactionItem_OnIncreaseButtomClicked(object sender, EventArgs e)
	{
		TransactionItem transactionItem = (TransactionItem)sender;
		if (_shopItems[transactionItem.LocalSerial].Amount > 0)
		{
			_shopItems[transactionItem.LocalSerial].Amount--;
			transactionItem.Amount++;
		}
		_updateTotal = true;
	}

	private void ShopItem_MouseClick(object sender, MouseEventArgs e)
	{
		foreach (ShopItem item in _shopScrollArea.Children.SelectMany((Control o) => o.Children).OfType<ShopItem>())
		{
			item.IsSelected = item == sender;
		}
	}

	public override void OnButtonClick(int buttonID)
	{
		switch ((Buttons)buttonID)
		{
		case Buttons.Accept:
		{
			Tuple<uint, ushort>[] items = _transactionItems.Select((KeyValuePair<uint, TransactionItem> t) => new Tuple<uint, ushort>(t.Key, (ushort)t.Value.Amount)).ToArray();
			if (IsBuyGump)
			{
				NetClient.Socket.Send_BuyRequest(base.LocalSerial, items);
			}
			else
			{
				NetClient.Socket.Send_SellRequest(base.LocalSerial, items);
			}
			Dispose();
			break;
		}
		case Buttons.Clear:
		{
			foreach (TransactionItem item in _transactionItems.Values.ToList())
			{
				RemoveTransactionItem(item);
			}
			break;
		}
		}
	}
}
